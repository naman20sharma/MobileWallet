# MobileWallet

## Overview
MobileWallet is a cross-platform self-sovereign identity (SSI) wallet built with Xamarin.Forms and the Hyperledger Aries .NET SDK. It provisions an edge agent, connects to a mediator, and exchanges credentials and proof requests through ACA-Py compatible agents. The shared code targets .NET Standard and is paired with platform projects for Android and iOS.

## Key Features
- **Aries edge agent and mediator support** using `Hyperledger.Aries`, `Hyperledger.Aries.Routing`, and `Hyperledger.Aries.Routing.Edge`, including delayed provisioning so wallets can be created on-device and later connected to a pool.
- **Wallet provisioning flow** that creates a local wallet, secures credentials, and persists provisioning flags before navigating to the main experience.
- **Push and local notifications** for DIDComm traffic with device token registration and message polling through the Aries routing client.
- **QR code scanning** for connection invitations and credential offers via ZXing.
- **Modular navigation and MVVM** with Autofac dependency injection, hosted services, and view model-to-page bindings for the major features (connections, credentials, proofs, onboarding, and settings).

## Project Structure
- `MyWallet/` – Shared Xamarin.Forms project that contains views, view models, services, and platform-agnostic configuration.
- `MyWallet.Android/` – Android head project (and equivalent iOS project if added) that references the shared code.
- `Configuration/config.json` – Default environment and agent configuration consumed at startup.

## Prerequisites
- Visual Studio 2019 or later with Xamarin workload, or Visual Studio for Mac with Xamarin.
- .NET SDK capable of building netstandard2.0 libraries.
- Android SDK/NDK for mobile builds. iOS builds require Xcode and a macOS host.
- Access to an ACA-Py mediator/agent endpoint reachable from the device or emulator.

## Configuration
Update `MyWallet/Configuration/config.json` with the mediator endpoint and Indy pool name you want to target. The Aries host bootstrap uses this file to set the edge agent endpoint, wallet storage location, and pool configuration. Defaults include:

```json
{
  "Environment": "Debug",
  "AgentFrameworkEndpoint": "<ngrok-or-agent-endpoint>",
  "PoolConfigurationName": "sovrin-staging"
}
```

## Running the App
1. Restore NuGet packages (`dotnet restore` or let Visual Studio restore on build).
2. Open `MyWallet.sln` in Visual Studio.
3. Set `MyWallet.Android` (or the iOS project) as the startup project and choose a device/emulator.
4. Build and deploy. On first launch, follow the onboarding flow to provision the local wallet and register with the mediator.
5. Use the scan feature to accept connection invitations or credential offers. Incoming notifications will register the device token and trigger message fetches from the mediator.

## Development Notes
- Dependency injection is configured in `App.xaml.cs` via Autofac and `XamarinHost`, wiring up Aries services, hosted pool configuration, and navigation bindings.
- Cloud message retrieval is handled by `CloudWalletService`, which serializes inbox fetches and logs results.
- The registration view model provisions the edge agent, secures wallet keys, and persists provisioning status before navigating to the main shell.

## License
This project follows the licensing terms defined by the repository owners. If none are provided, treat it as internal until a license is added.
