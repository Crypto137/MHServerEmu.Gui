# MHServerEmu.Gui

Graphical user interface implementation for [MHServerEmu](https://github.com/Crypto137/MHServerEmu), powered by the Avalonia framework.

<p align="center">
<img alt="MHServerEmu.Gui.Avalonia Screenshot" src="https://raw.githubusercontent.com/Crypto137/MHServerEmu.gui/master/assets/screenshot.png"/>
</p>

## Features

- Launching MHServerEmu and tracking its status.

- Command input with presets for important commands.

- Server configuration with support for overriding `Config.ini` without making changes to it.

- AOT publishing compatibility for fast startup.

- Works natively on both Windows and Linux with touch screen support.

- Designed with Steam Deck and other handheld PCs in mind, but can also be used on desktops.

## Installation

1. Download the latest release [here](https://github.com/Crypto137/MHServerEmu/releases) and extract it.

2. Create a new subdirectory named `MHServerEmu` in the GUI directory and copy or move a build of MHServerEmu to it.

3. Run `MHServerEmu.Gui.Avalonia.exe` (Windows) or `MHServerEmu.Gui.Avalonia` (Linux).

## Building

MHServerEmu.Gui is made with AOT publishing in mind. Here are the recommended commands for building it:

- Windows: `dotnet publish -c Release -r win-x64 -p:PublishAot=true`.

- Linux: `dotnet publish -c Release -r linux-x64 -p:PublishAot=true`.

## Remarks

- MHServerEmu.Gui does not contain a reverse proxy. You need to do **one** of the following in order to connect to the server:
  
  - [Use an external reverse proxy.](https://github.com/Crypto137/MHServerEmu/blob/master/docs/Setup/ManualSetup.md) 
  
  - [Patch the client executable to enable authentication over port 8080.](https://crypto137.github.io/mh-exe-patcher/)
    
    - This option disables encryption of account credentials and should be used only for offline play.

- When running the server on Linux without a reverse proxy, the `Address` setting of the `WebFrontend` section needs to be set to `127.0.0.1` instead of `localhost` for it to be accessible to clients running under Wine/Proton on the same machine.

- When running the server on Linux, we recommend to use [self-contained nightly builds of MHServerEmu](https://nightly.link/Crypto137/MHServerEmu/workflows/nightly-release-linux-x64-self-contained/master?preview).