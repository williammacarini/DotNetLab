# DotNetLab Guide

DotNetLab is a modular sandbox for .NET experimentation. This guide explains how to navigate, run, and extend the project.

## Project Structure

- `src/DotNetLab.Core`: Shared abstractions and infrastructure.
- `src/DotNetLab.Labs`: The home for all experiments (e.g., AI, Event Sourcing, WASM).
- `src/DotNetLab.Cli`: The command-line tool to interact with the labs.
- `plugins/`: Directory for dynamically loaded extensions.
- `labs/`: JSON configurations for specific lab instances.

## How to Run

### 1. Build the Solution
Ensure everything is compiled before running:
```bash
dotnet build
```

### 2. Run the CLI
The CLI is the main entry point for managing labs.
```bash
dotnet run --project src/DotNetLab.Cli
```

### 3. Running a Specific Lab
(Note: Implementation of the 'run' command is in progress)
Planned usage:
```bash
dotnet run --project src/DotNetLab.Cli -- lab run Lab001
```

## Creating a New Lab

1.  Create a new folder under `src/DotNetLab.Labs/LabXXX-Name`.
2.  Implement your logic using the abstractions provided in `DotNetLab.Core`.
3.  Add a configuration file in `labs/labXXX.config.json`.
4.  (Optional) Add a test project in `tests/DotNetLab.Labs.Tests`.

## Documentation
- Refer to `docs/architecture-decision-records/` for design decisions.
- Detailed lab-specific guides can be found in `docs/lab-guides/`.
