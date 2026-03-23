# ADR 001: Modular Lab-Based Architecture

## Status
Proposed

## Context
The goal of DotNetLab is to provide a sandbox environment for experimenting with modern .NET features, AI integrations (Semantic Kernel, GraphRAG), and advanced architectural patterns (Event Sourcing, Plugin Systems). A monolithic project structure would become cluttered and difficult to maintain as more experiments are added.

## Decision
We will adopt a modular, multi-project architecture separated into three main layers:
1.  **Core (`DotNetLab.Core`)**: Contains shared abstractions, plugin loading logic, and workflow orchestration.
2.  **Labs (`DotNetLab.Labs`)**: Each experiment resides in its own namespace/directory. This allows for isolated dependency management and clear separation of concepts.
3.  **CLI (`DotNetLab.Cli`)**: A unified entry point to discover, configure, and execute specific labs.

## Consequences
- **Pros**:
    - High isolation between experiments.
    - Clear path for adding new "Labs" without affecting existing ones.
    - Centralized CLI simplifies the developer experience.
- **Cons**:
    - Slightly higher initial setup overhead (solution/project management).
    - Requires a robust plugin/reflection mechanism to run labs dynamically from the CLI.
