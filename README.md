# MobileWallet

## Overview
MobileWallet is a cross-platform self-sovereign identity (SSI) wallet built with Xamarin.Forms and the Hyperledger Aries .NET SDK. It provisions an edge agent, connects to a mediator, and exchanges credentials and proof requests through Aries-compatible agents. The shared code targets .NET Standard and is paired with platform head projects (Android is included; an iOS head can be added).

> **Note:** This wallet uses the Hyperledger Aries .NET SDK (an SDK I have contributed to as part of the Hyperledger Aries community). For background on the Aries protocols and ecosystem, see https://github.com/hyperledger-aries/aries.

## Key Features
- **Aries edge agent and mediator support** using `Hyperledger.Aries`, `Hyperledger.Aries.Routing`, and `Hyperledger.Aries.Routing.Edge`, including delayed provisioning so wallets can be created on-device and later connected to a mediator.
- **Wallet provisioning flow** that creates a local wallet, secures credentials, and persists provisioning flags before navigating to the main experience.
- **Push and local notifications** for DIDComm traffic with device token registration and message polling through the Aries routing client.
- **QR code scanning** for connection invitations and credential offers via ZXing.
- **Modular navigation and MVVM** with Autofac dependency injection, hosted services, and view model-to-page bindings for connections, credentials, proofs, onboarding, and settings.

## Technology Stack
- **Framework:** Xamarin.Forms targeting Android head project (iOS head can be added)
- **Identity:** Hyperledger Aries .NET SDK (`Hyperledger.Aries`, `Hyperledger.Aries.Routing`, `Hyperledger.Aries.Routing.Edge`)
- **Messaging:** DIDComm via an Aries mediator/edge routing (e.g. an ACA-Py-based mediator, or a custom mediator/agent including ASP.NET implementations)
- **Storage:** Indy wallet backing store configured per environment
- **UX:** ZXing for QR scanning, push/local notifications for credential and proof traffic

## Project Structure
- `MyWallet/` – Shared Xamarin.Forms project that contains views, view models, services, and platform-agnostic configuration.
- `MyWallet.Android/` – Android head project; iOS head project not included in this repository.
- `MyWallet/Configuration/config.json` – Default environment and agent configuration consumed at startup.

## Prerequisites
- Visual Studio 2019 or later with Xamarin workload, or Visual Studio for Mac with Xamarin.
- .NET SDK capable of building netstandard2.0 libraries.
- Android SDK/NDK for mobile builds. iOS builds require Xcode and a macOS host.
- Access to an Aries mediator/agent endpoint reachable from the device or emulator (e.g. ACA-Py, or a custom mediator/agent).

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
3. Set `MyWallet.Android` as the startup project and choose a device/emulator.
4. Build and deploy. On first launch, follow the onboarding flow to provision the local wallet and register with the mediator.
5. Use the scan feature to accept connection invitations or credential offers. Incoming notifications will register the device token and trigger message fetches from the mediator.

## Mediator and Edge Agent Flow
- The **registration view model** provisions the agent, generating a secure wallet key and persisting provisioning flags.
- The **mediator client** registers the device token and polls on an interval so DIDComm messages are fetched even when push delivery is unavailable.
- The **edge routing service** handles inbox fetches and dispatches notifications into the app so credential offers and proofs are surfaced quickly.

## Development Notes
- Dependency injection is configured in `App.xaml.cs` via Autofac and `XamarinHost`, wiring up Aries services, hosted pool configuration, and navigation bindings.
- Cloud message retrieval is handled by `CloudWalletService`, which serializes inbox fetches and logs results.
- The registration view model provisions the edge agent, secures wallet keys, and persists provisioning status before navigating to the main shell.

## License
Licensed under the MIT License. See `LICENSE`.
