using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MinecraftClientGUI
{
    public partial class Form1 : Form
    {
        // --- ZMIENNE GŁÓWNE ---
        private TabControl mainTabs;
        private const string SettingsFile = "settings_v3.txt";
        private const string MacrosFile = "macros.txt";
        private string currentLang = "en"; // Domyślny język

        // UI - Góra
        private Label lblLogin, lblPass, lblIP, lblActive;
        private ComboBox cmbLogin;
        private TextBox txtPassword;
        private ComboBox cmbIP;
        private Button btnAddBot;

        // UI - Dół
        private TextBox boxGlobalInput;
        private Button btnGlobalSend;
        private CheckBox chkSendToAll;

        // UI - Prawa (Makra)
        private Label lblMacrosTitle;
        private FlowLayoutPanel macroPanel;
        private Button btnEditMacros;
        private Button btnRefreshMacros;
        private Button btnLangSwitch; // Przycisk zmiany języka

        // Historia
        private List<string> historyLogins = new List<string>();
        private List<string> historyIPs = new List<string>();

        public Form1(string[] args)
        {
            InitializeComponent();
            InitializeCustomUI();
            LoadSettings();
            LoadMacros();
            UpdateLanguage(); // Ustawienie tekstów na start

            if (args.Length > 0)
            {
                AddNewTab("Auto-Bot", args);
            }

            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mainTabs != null)
            {
                for (int i = mainTabs.TabPages.Count - 1; i >= 0; i--)
                {
                    if (mainTabs.TabPages[i] is ConsoleTab tab) tab.CloseTab();
                }
            }
        }

        private void InitializeCustomUI()
        {
            // --- 1. GŁÓWNE OKNO ---
            this.Text = "MCC Multibox Commander";
            this.Size = new Size(1150, 750);
            this.BackColor = Color.FromArgb(28, 28, 28);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);

            // --- 5. TAB CONTROL (CENTRUM) ---
            mainTabs = new TabControl();
            mainTabs.Dock = DockStyle.Fill;
            mainTabs.BackColor = Color.FromArgb(30, 30, 30);
            mainTabs.ForeColor = Color.White;
            // Poprawka rysowania kart
            mainTabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            mainTabs.Padding = new Point(20, 5);
            mainTabs.ItemSize = new Size(120, 30);
            mainTabs.DrawItem += MainTabs_DrawItem;
            this.Controls.Add(mainTabs);

            // --- 2. PANEL GÓRNY ---
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.FromArgb(45, 45, 48), Padding = new Padding(10) };
            this.Controls.Add(topPanel);

            lblLogin = CreateLabel("Username / Profile:", 10, 8);
            topPanel.Controls.Add(lblLogin);
            cmbLogin = CreateComboBox(10, 28, 200);
            topPanel.Controls.Add(cmbLogin);

            lblPass = CreateLabel("Password:", 220, 8);
            topPanel.Controls.Add(lblPass);
            txtPassword = new TextBox { Location = new Point(220, 28), Size = new Size(150, 25), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
            topPanel.Controls.Add(txtPassword);

            lblIP = CreateLabel("Server IP:", 380, 8);
            topPanel.Controls.Add(lblIP);
            cmbIP = CreateComboBox(380, 28, 200);
            topPanel.Controls.Add(cmbIP);

            btnAddBot = new Button { Text = "Add Account (+)", Location = new Point(600, 26), Size = new Size(140, 29), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, Cursor = Cursors.Hand };
            btnAddBot.FlatAppearance.BorderSize = 0;
            btnAddBot.Click += BtnAddBot_Click;
            topPanel.Controls.Add(btnAddBot);

            // Licznik
            System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            lblActive = new Label { Location = new Point(760, 30), AutoSize = true, ForeColor = Color.LightGreen, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            updateTimer.Tick += (s, e) => {
                string txt = currentLang == "en" ? "Active accounts: " : "Aktywne konta: ";
                lblActive.Text = $"{txt}{mainTabs.TabPages.Count}";
            };
            updateTimer.Start();
            topPanel.Controls.Add(lblActive);

            // --- 3. PANEL PRAWY (MAKRA) ---
            Panel rightContainer = new Panel { Dock = DockStyle.Right, Width = 180, BackColor = Color.FromArgb(35, 35, 38) };
            this.Controls.Add(rightContainer);

            // Nagłówek Makr
            Panel macroHeader = new Panel { Dock = DockStyle.Top, Height = 75, BackColor = Color.FromArgb(35, 35, 38) };
            rightContainer.Controls.Add(macroHeader);

            lblMacrosTitle = new Label { Text = "Quick Actions", Location = new Point(0, 10), Width = 180, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.LightGray };
            macroHeader.Controls.Add(lblMacrosTitle);

            btnEditMacros = new Button { Text = "Edit", Location = new Point(10, 40), Size = new Size(75, 25), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            btnEditMacros.FlatAppearance.BorderSize = 0;
            btnEditMacros.Click += BtnEditMacros_Click;
            macroHeader.Controls.Add(btnEditMacros);

            btnRefreshMacros = new Button { Text = "Reload", Location = new Point(95, 40), Size = new Size(75, 25), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            btnRefreshMacros.FlatAppearance.BorderSize = 0;
            btnRefreshMacros.Click += (s, e) => LoadMacros();
            macroHeader.Controls.Add(btnRefreshMacros);

            // Przycisk Języka (Mały, na samej górze po prawej)
            btnLangSwitch = new Button { Text = "PL", Location = new Point(145, 0), Size = new Size(35, 20), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, Font = new Font("Segoe UI", 7f) };
            btnLangSwitch.FlatAppearance.BorderSize = 0;
            btnLangSwitch.Click += (s, e) => {
                currentLang = (currentLang == "en") ? "pl" : "en";
                UpdateLanguage();
            };
            macroHeader.Controls.Add(btnLangSwitch);


            // Panel z przyciskami (Flow)
            macroPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 5, 0, 0),
                AutoScroll = true,
                BackColor = Color.FromArgb(35, 35, 38)
            };
            rightContainer.Controls.Add(macroPanel);

            // Naprawa Layoutu: Header na górę, Panel wypełnia resztę
            macroHeader.SendToBack();
            macroPanel.BringToFront();

            // --- 4. PANEL DOLNY ---
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 45, BackColor = Color.FromArgb(45, 45, 48), Padding = new Padding(5) };
            this.Controls.Add(bottomPanel);

            btnGlobalSend = new Button { Text = "Send", Dock = DockStyle.Right, Width = 80, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60) };
            btnGlobalSend.FlatAppearance.BorderSize = 0;
            btnGlobalSend.Click += BtnGlobalSend_Click;
            bottomPanel.Controls.Add(btnGlobalSend);

            chkSendToAll = new CheckBox { Text = "Send to all", Dock = DockStyle.Right, Width = 140, ForeColor = Color.LightSalmon, Padding = new Padding(5, 0, 0, 0) };
            bottomPanel.Controls.Add(chkSendToAll);

            boxGlobalInput = new TextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 11f) };
            boxGlobalInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { BtnGlobalSend_Click(s, e); e.SuppressKeyPress = true; } };
            bottomPanel.Controls.Add(boxGlobalInput);
        }

        // --- TŁUMACZENIA ---
        private void UpdateLanguage()
        {
            bool isEn = currentLang == "en";
            btnLangSwitch.Text = isEn ? "PL" : "EN"; // Pokazujemy na jaki język zmienimy

            // Górny Panel
            lblLogin.Text = isEn ? "Username / Email:" : "Login / Email:";
            lblPass.Text = isEn ? "Password:" : "Hasło:";
            lblIP.Text = isEn ? "Server IP:" : "IP Serwera:";
            btnAddBot.Text = isEn ? "Add Account (+)" : "Dodaj Konto (+)";

            // Makra
            lblMacrosTitle.Text = isEn ? "Quick Actions" : "Szybkie Akcje";
            btnEditMacros.Text = isEn ? "Edit" : "Edytuj";
            btnRefreshMacros.Text = isEn ? "Reload" : "Odśwież";

            // Dół
            btnGlobalSend.Text = isEn ? "Send" : "Wyślij";
            chkSendToAll.Text = isEn ? "Send to all" : "Wyślij do wszystkich";

            // Aktualizacja przycisków na kartach
            foreach (TabPage page in mainTabs.TabPages)
            {
                if (page is ConsoleTab tab) tab.UpdateLang(currentLang);
            }
        }

        // --- MAKRA ---
        private void BtnEditMacros_Click(object sender, EventArgs e)
        {
            if (!File.Exists(MacrosFile))
            {
                // Domyślnie angielskie makra
                File.WriteAllLines(MacrosFile, new string[] {
                    "Creative|/gamemode creative|Gold",
                    "Survival|/gamemode survival|Gray",
                    "Hello|Hello everyone!|Green",
                    "Login|/login password123|Purple",
                    "Spawn|/spawn|Blue"
                });
            }
            Process.Start("notepad.exe", MacrosFile);
        }

        private void LoadMacros()
        {
            macroPanel.Controls.Clear();
            if (!File.Exists(MacrosFile)) return;
            try
            {
                var lines = File.ReadAllLines(MacrosFile);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('|');
                    if (parts.Length >= 2)
                    {
                        Color c = parts.Length > 2 ? Color.FromName(parts[2]) : Color.White;
                        AddMacroBtn(parts[1], parts[0], c);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading macros: " + ex.Message); }
        }

        private void AddMacroBtn(string cmd, string label, Color c)
        {
            // Dynamiczna szerokość (Szerokość Panelu - Scrollbar ~25px)
            int btnWidth = macroPanel.Width - 25;

            Button btn = new Button
            {
                Text = label,
                Width = btnWidth, // Fix ucinania
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = c,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(3, 3, 3, 3),
                TextAlign = ContentAlignment.MiddleLeft
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                if (chkSendToAll.Checked)
                {
                    foreach (TabPage page in mainTabs.TabPages) if (page is ConsoleTab tab) tab.Send(cmd);
                }
                else
                {
                    if (mainTabs.SelectedTab is ConsoleTab activeTab) activeTab.Send(cmd);
                }
            };
            macroPanel.Controls.Add(btn);
        }

        // --- LOGIKA BOTÓW ---
        private void BtnAddBot_Click(object sender, EventArgs e)
        {
            string user = cmbLogin.Text.Trim();
            string pass = txtPassword.Text.Trim();
            string ip = cmbIP.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(ip))
            {
                string msg = currentLang == "en" ? "Please enter username and IP!" : "Podaj login i IP serwera!";
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveSettings(user, ip);
            AddNewTab(user, new string[] { user, pass, ip });
        }

        private void AddNewTab(string title, string[] args)
        {
            ConsoleTab newTab = new ConsoleTab(title, args, currentLang);
            mainTabs.TabPages.Add(newTab);
            mainTabs.SelectedTab = newTab;
        }

        private void BtnGlobalSend_Click(object sender, EventArgs e)
        {
            string cmd = boxGlobalInput.Text.Trim();
            if (string.IsNullOrEmpty(cmd)) return;

            if (chkSendToAll.Checked)
            {
                foreach (TabPage page in mainTabs.TabPages) if (page is ConsoleTab tab) tab.Send(cmd);
            }
            else
            {
                if (mainTabs.SelectedTab is ConsoleTab activeTab) activeTab.Send(cmd);
            }
            boxGlobalInput.Clear();
        }

        // --- RYSOWANIE ZAKŁADEK (DARK) ---
        private void MainTabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= mainTabs.TabPages.Count) return;
            TabPage page = mainTabs.TabPages[e.Index];
            Rectangle rect = e.Bounds;
            bool isSelected = (e.State == DrawItemState.Selected);

            // Tło
            Brush backBrush = isSelected ? new SolidBrush(Color.FromArgb(60, 60, 60)) : new SolidBrush(Color.FromArgb(35, 35, 35));
            e.Graphics.FillRectangle(backBrush, rect);

            // Tekst
            Color textColor = isSelected ? Color.White : Color.Gray;
            TextRenderer.DrawText(e.Graphics, page.Text, new Font("Segoe UI", 9f, isSelected ? FontStyle.Bold : FontStyle.Regular),
                rect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Akcent
            if (isSelected) e.Graphics.FillRectangle(Brushes.DodgerBlue, rect.Left, rect.Bottom - 3, rect.Width, 3);
        }

        // --- HELPERS ---
        private Label CreateLabel(string text, int x, int y) => new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = Color.DarkGray, Font = new Font("Segoe UI", 8f) };
        private ComboBox CreateComboBox(int x, int y, int w) => new ComboBox { Location = new Point(x, y), Size = new Size(w, 25), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var lines = File.ReadAllLines(SettingsFile).ToList();
                    if (lines.Count > 0) txtPassword.Text = lines[0];
                    if (lines.Count > 1) { historyLogins = lines[1].Split('|').ToList(); cmbLogin.Items.AddRange(historyLogins.ToArray()); if (cmbLogin.Items.Count > 0) cmbLogin.SelectedIndex = 0; }
                    if (lines.Count > 2) { historyIPs = lines[2].Split('|').ToList(); cmbIP.Items.AddRange(historyIPs.ToArray()); if (cmbIP.Items.Count > 0) cmbIP.SelectedIndex = 0; }
                }
            }
            catch { }
        }

        private void SaveSettings(string user, string ip)
        {
            historyLogins.Remove(user); historyLogins.Insert(0, user); if (historyLogins.Count > 10) historyLogins.RemoveAt(10);
            historyIPs.Remove(ip); historyIPs.Insert(0, ip); if (historyIPs.Count > 10) historyIPs.RemoveAt(10);
            try { File.WriteAllLines(SettingsFile, new string[] { txtPassword.Text, string.Join("|", historyLogins), string.Join("|", historyIPs) }); } catch { }
        }
    }

    // ==========================================================
    // KLASA: CONSOLE TAB
    // ==========================================================
    public class ConsoleTab : TabPage
    {
        private MinecraftClient Client;
        private Thread t_read;
        private RichTextBox boxOutput;
        private Button btnDisconnect;
        private Label lblStatus;

        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)] private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);
        private const int WM_VSCROLL = 0x115; private const int SB_BOTTOM = 7;

        public ConsoleTab(string title, string[] args, string lang)
        {
            this.Text = title;
            this.BackColor = Color.FromArgb(20, 20, 20);

            // Panel karty
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 35, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(5) };

            btnDisconnect = new Button
            {
                Text = lang == "en" ? "Disconnect" : "Rozłącz",
                Dock = DockStyle.Right,
                Width = 100,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnDisconnect.FlatAppearance.BorderSize = 0;
            btnDisconnect.Click += (s, e) => CloseTab();
            topPanel.Controls.Add(btnDisconnect);

            lblStatus = new Label { Text = $"Account: {title}", Dock = DockStyle.Fill, ForeColor = Color.LightGray, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            topPanel.Controls.Add(lblStatus);
            this.Controls.Add(topPanel);

            // Konsola
            boxOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 10f),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Disconnect / Close", null, (s, e) => CloseTab());
            menu.Items.Add("Clear Console", null, (s, e) => boxOutput.Clear());
            boxOutput.ContextMenuStrip = menu;

            this.Controls.Add(boxOutput);
            try { SetWindowTheme(boxOutput.Handle, "DarkMode_Explorer", null); } catch { }

            PrintSystem("Initializing...");

            if (args.Length == 3) new Thread(() => InitClient(new MinecraftClient(args[0], args[1], args[2]))).Start();
            else new Thread(() => InitClient(new MinecraftClient(args))).Start();
        }

        public void UpdateLang(string lang)
        {
            if (btnDisconnect != null) btnDisconnect.Text = lang == "en" ? "Disconnect" : "Rozłącz";
        }

        private void InitClient(MinecraftClient client)
        {
            Client = client;
            t_read = new Thread(ReadLoop);
            t_read.IsBackground = true;
            t_read.Start();
            InvokeUI(() => PrintSystem("Connected."));
        }

        private void ReadLoop()
        {
            try
            {
                while (Client != null && !Client.Disconnected)
                {
                    string line = Client.ReadLine();
                    if (!string.IsNullOrEmpty(line)) PrintLine(line);
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex) { InvokeUI(() => PrintSystem($"Error: {ex.Message}", Color.Red)); }
            finally { InvokeUI(() => PrintSystem("Disconnected.", Color.Red)); }
        }

        public void Send(string text)
        {
            if (Client != null && !Client.Disconnected) { Client.SendText(text); InvokeUI(() => PrintSystem($"> {text}", Color.Cyan)); }
        }

        public void CloseTab()
        {
            try
            {
                if (Client != null) { Client.Close(); Client = null; }
                if (t_read != null && t_read.IsAlive) { t_read.Abort(); t_read = null; }
                if (this.Parent is TabControl tc) tc.Invoke(new Action(() => { tc.TabPages.Remove(this); this.Dispose(); }));
            }
            catch { }
        }

        private void PrintSystem(string text, Color? c = null)
        {
            InvokeUI(() => {
                boxOutput.SelectionColor = c ?? Color.Gray;
                boxOutput.AppendText($"[SYSTEM] {text}\n");
                SendMessage(boxOutput.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
            });
        }

        private void PrintLine(string raw)
        {
            InvokeUI(() => {
                boxOutput.SuspendLayout();
                string[] subs = raw.Split('§');
                boxOutput.SelectionColor = Color.LightGray;
                if (subs.Length > 0) boxOutput.AppendText(subs[0]);
                for (int i = 1; i < subs.Length; i++)
                {
                    if (subs[i].Length > 1)
                    {
                        char code = subs[i][0];
                        boxOutput.SelectionColor = GetColor(code);
                        boxOutput.SelectionFont = GetFont(code, boxOutput.Font);
                        boxOutput.AppendText(subs[i].Substring(1));
                    }
                }
                boxOutput.AppendText("\n");
                SendMessage(boxOutput.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
                boxOutput.ResumeLayout();
            });
        }

        private void InvokeUI(Action action) { if (!boxOutput.IsDisposed) { if (boxOutput.InvokeRequired) try { boxOutput.Invoke(action); } catch { } else action(); } }
        private Font GetFont(char c, Font f) { return (c == 'l') ? new Font(f, FontStyle.Bold) : f; }
        private Color GetColor(char code)
        {
            switch (code)
            {
                case '0': return Color.Black;
                case '1': return Color.FromArgb(80, 80, 255);
                case '2': return Color.FromArgb(80, 255, 80);
                case '3': return Color.FromArgb(80, 255, 255);
                case '4': return Color.FromArgb(255, 80, 80);
                case '5': return Color.FromArgb(255, 80, 255);
                case '6': return Color.FromArgb(255, 170, 0);
                case '7': return Color.Silver;
                case '8': return Color.Gray;
                case '9': return Color.FromArgb(100, 100, 255);
                case 'a': return Color.FromArgb(85, 255, 85);
                case 'b': return Color.FromArgb(85, 255, 255);
                case 'c': return Color.FromArgb(255, 85, 85);
                case 'd': return Color.FromArgb(255, 85, 255);
                case 'e': return Color.FromArgb(255, 255, 85);
                case 'f': return Color.White;
                default: return Color.LightGray;
            }
        }
    }
}