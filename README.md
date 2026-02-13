# MCC Multibox Commander (GUI Fork)

<div align="center">

<!-- You can add a screenshot here if you want -->
<!-- <img src="screenshots/preview.png" alt="GUI Preview" width="800"/> -->

**A modern, dark-themed GUI for [Minecraft Console Client](https://github.com/MCCTeam/Minecraft-Console-Client) focused on multiboxing and macro management.**

[Features](#features-✨) | [Installation](#installation-📦) | [Usage](#usage-📚) | [License](#license-⚖️)

</div>

## About ℹ️

This project is a modified fork of **Minecraft Console Client (MCC)**. While the core logic relies on the robust MCCTeam implementation, the Graphical User Interface (GUI) has been completely rewritten to support **managing multiple bots simultaneously**.

This tool is perfect for server administration, AFK farming, or chat management across multiple accounts using a convenient, dark-themed dashboard.

## Features ✨

### 🚀 New GUI Features (Added in this Fork)
-   **Multiboxing Support**: Run multiple Minecraft accounts in separate tabs within a single window.
-   **Dark Mode UI**: A completely redesigned, eye-friendly dark interface replacing the old Windows Forms look.
-   **Global Control**: Send chat messages or commands to **all connected bots** simultaneously with the "Send to all" checkbox.
-   **Macro System**: 
    -   Side panel with quick-action buttons.
    -   Load custom commands from `macros.txt`.
    -   Edit and reload macros on the fly.
-   **Bilingual Support**: Switch between **English** and **Polish** (PL/EN) instantly.
-   **Session History**: Remembers your last used usernames, IPs, and passwords (stored locally in `settings_v3.txt`).
-   **Active Counter**: Displays the number of currently connected accounts.

### 🛠 Core MCC Features (Inherited)
-   Connect to any Minecraft Java server (Offline/Online mode support depends on core MCC configuration).
-   Lightweight and fast.
-   Full chat and command support with color parsing.
-   Inventory handling, auto-response, and other core MCC features run in the background.

## Installation 📦

You can install the application using the installer or run it directly from the folder.

### Option 1: Installer (Recommended)
1.  Download the latest **publish.zip** from the [Releases section](../../releases).
2.  Extract the ZIP file.
3.  Run **`setup.exe`**.
4.  Follow the prompts to install the application on your system.

### Option 2: Run from Folder / Portable
1.  Clone this repository or download the source code.
2.  Navigate to the `MinecraftClientGUI` folder.
3.  Run **`MinecraftClientGUI.exe`**.

*Note: The first time you run the application, it will automatically generate necessary configuration files like `macros.txt` and `settings_v3.txt`.*

## Usage 📚

### Connecting
1.  Enter your **Username/Email**.
2.  Enter **Password** (leave blank for offline servers if supported).
3.  Enter **Server IP**.
4.  Click **Add Account (+)**.
5.  A new tab will open, and the bot will attempt to connect.

### Using Macros
1.  Click the **Edit** button in the "Quick Actions" panel.
2.  Add lines in the format: `Label|/command|Color`.
    *   *Example:* `Survival|/gamemode survival|Green`
3.  Save the text file and click **Reload**.
4.  Clicking a macro button sends the command to the active tab (or all tabs if "Send to all" is checked).

### Global Chat
1.  Check the **Send to all** box at the bottom.
2.  Type a message or command in the bottom input box.
3.  Press Enter or click **Send**. The message will be executed by every connected bot.

## Building from source 🏗️

This project requires **Visual Studio** (2019 or newer recommended) with **.NET Framework** support.
1.  Open the solution file.
2.  Ensure `MinecraftClient` project is set as the startup project.
3.  Build and Run.

## Credits & License ⚖️

**This project is a fork of [Minecraft Console Client](https://github.com/MCCTeam/Minecraft-Console-Client).**

-   **GUI & Multibox modifications:** Created by [AnonBOTpl](https://github.com/AnonBOTpl).
-   **Core Logic & Original Project:** Copyright (c) 2012-2024 MCC Team & Contributors.

Licensed under **CDDL-1.0**.
You may use, modify, and distribute this software under the terms of the CDDL-1.0 license. Source code modifications must be made available if distributed.

Full license text available at: http://opensource.org/licenses/CDDL-1.0
