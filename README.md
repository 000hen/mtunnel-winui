# MTunnel

A Windows App SDK (WinUI 3) application for managing simple client/server tunneling with a clean UI. Targeting .NET 8.

## Features
- Client and server pages to configure tunnels
- Port and host input components
- Copy-to-clipboard helpers
- Centralized tunnel log view
- Custom controls for options and dialogs

## Requirements
- Windows 10/11
- Visual Studio 2022 (v17.8+) with .NET 8 SDK
- Windows App SDK 1.8

## Getting Started
1. Clone the repository:
   ```
   git clone https://github.com/000hen/mtunnel-winui
   ```
2. Open the solution folder `/path/to/project` in Visual Studio.
3. Restore packages and build:
   - Visual Studio will restore NuGet packages automatically.
   - Build the `MTunnel` project.
4. Run the app:
   - Set `MTunnel` as startup project.
   - Start debugging (F5).

## Project Structure
- `MTunnel/` WinUI 3 app project
  - `Pages/` app pages like `ClientPage.xaml`, `ServerPage.xaml`, `DefaultPage.xaml`, `TunnelLog.xaml`
  - `Components/` reusable controls (e.g., `PortTextBox`, `CopyButton`, `HostDialogControl`, `ControlOptionsControl`, `ButtonIconText`)
  - `MainWindow.xaml` main window
  - `App.xaml` application entry and resources
  - `ProcessHandler.cs` process/tunnel handling logic

## Configuration
- `Package.appxmanifest` and `Package.StoreAssociation.xml` store packaging metadata
- `app.manifest` application manifest

## Usage
- Use `Server` page to start a tunnel server with desired port.
- Use `Client` page to connect to a server and forward ports.
- Check `Tunnel Log` for operational messages.

## Contributing
- Issues and PRs are welcome.
- Keep code style consistent with `.editorconfig`.

## License
- See `LICENSE` (if present) or repository terms.

## Acknowledgments
- Built with Windows App SDK (WinUI 3).
