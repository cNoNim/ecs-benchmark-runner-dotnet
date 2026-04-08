# Entity-Component-System Benchmarks for .NET

[![License](https://img.shields.io/github/license/cNoNim/ecs-benchmark-runner-dotnet)](https://github.com/cNoNim/ecs-benchmark-runner-dotnet?tab=MIT-1-ov-file#readme)
[![Stars](https://img.shields.io/github/stars/cNoNim/ecs-benchmark-runner-dotnet?color=brightgreen)](https://github.com/cNoNim/ecs-benchmark-runner-dotnet/stargazers)

This repository contains a collection of benchmarks for .NET Entity-Component-System (ECS) frameworks.
Benchmarks perform a complex performance comparison of ECS frameworks on a near-real-world scenario.

[**Unity Version**](https://github.com/cNoNim/ecs-benchmark-runner-unity)

### Frameworks:

|                                                           ECS | Version                                                                 | Implemented |
|--------------------------------------------------------------:|:------------------------------------------------------------------------|:-----------:|
|          [DragonECS](https://github.com/DCFApixels/DragonECS) | [0.9.21](https://github.com/DCFApixels/DragonECS/releases/tag/0.9.21)   |      ✅      |
|         [FriFlo](https://github.com/friflo/Friflo.Engine.ECS) | [3.6.0](https://www.nuget.org/packages/Friflo.Engine.ECS/3.6.0)         |      ✅      |
|             [LeoECSLite](https://github.com/Leopotam/ecslite) | [2025.4.22](https://github.com/Leopotam/ecslite/releases/tag/2025.4.22) |      ✅      |
|        [Massive ECS](https://github.com/nilpunch/massive-ecs) | [v20.1.1](https://github.com/nilpunch/massive-ecs/tree/v20.1.1)         |      ✅      |
|                  [Morpeh](https://github.com/scellecs/morpeh) | [2024.1.1](https://github.com/scellecs/morpeh/releases/tag/2024.1.1)    |      ✅      |
| [StaticEcs](https://github.com/Felid-Force-Studios/StaticEcs) | [2.0.3](https://www.nuget.org/packages/FFS.StaticEcs/2.0.3)             |      ✅      |

## Scenario

All benchmark implementations run the same deterministic simulation.

At startup, the benchmark creates `N` units. During spawn, each unit receives a type (`NPC`, `Hero`, or `Monster`), combat stats, position, and movement state. Each tick then runs the same gameplay pipeline: spawning and respawning, target selection, attack creation, damage application, movement, velocity updates, and rendering into a framebuffer.

This makes the benchmark closer to a small RTS-like combat loop than to a synthetic ECS microbenchmark.

## Tick Pipeline

Each benchmark context executes the same logical steps:

1. Spawn / Respawn / Kill
2. Render current state into framebuffer
3. Update unit sprite state
4. Apply damage from pending attacks
5. Create new attacks
6. Move units
7. Update velocity
8. Advance simulation data

## Validation

Benchmark implementations are validated by shared tests.
For the same entity count and tick count, all contexts must produce the same framebuffer hash. This ensures the comparison is based on equivalent simulation results, not on different behavior.

## Running

0. Install [.NET 10 SDK](https://dotnet.microsoft.com/download) and [NPM](https://nodejs.org/en/download/)
1. Clone repository
   ```sh
   git clone https://github.com/cNoNim/ecs-benchmark-runner-dotnet.git
   ```
2. Run tests
   ```sh
   dotnet test Ecs.Benchmark.Dotnet.slnx
   ```
3. Run benchmarks

   ```sh
   dotnet run --project Benchmark.Runner -c Release --filter \*
   ```

## Structure

The benchmark is divided into repositories.
Current repository integrates benchmark packages into runner and test projects for .NET.

### [Benchmark.Core Package](https://github.com/cNoNim/ecs-benchmark-core)

A separate repository contains common assemblies that are used by benchmarks or by infrastructure.
Integration is done in the [Benchmark.Core](Benchmark.Core) project.

#### [Benchmark.Generator](https://github.com/cNoNim/ecs-benchmark-core/tree/main/Runtime/Benchmark.Core/SourceGenerators/Sources~/Benchmark.Generator)

Part of **Benchmark.Core** package.
**Benchmark.Generator** is a source generator that checks referenced assemblies and looks for implementations of `Benchmark.Core.IContext` and generates `Benchmark.Contexts.Factories` that are used to get all the contexts involved in the benchmark.
Integration is done in the [Benchmark.Generator](Benchmark.Generator) project.

### Benchmark Projects

Each benchmark is a separate repository, integration is done through separate projects in the [Benchmarks](Benchmarks) folder.

|                                                      Repository | Project                                           |
|----------------------------------------------------------------:|:--------------------------------------------------|
| [Dragon ECS](https://github.com/cNoNim/ecs-benchmark-dragonecs) | [Benchmark.DragonEcs](Benchmarks/Benchmark.DragonEcs) |
| [FriFlo ECS](https://github.com/cNoNim/ecs-benchmark-frifloecs) | [Benchmark.FrifloEcs](Benchmarks/Benchmark.FrifloEcs) |
| [LeoEcsLite](https://github.com/cNoNim/ecs-benchmark-ecslite)   | [Benchmark.EcsLite](Benchmarks/Benchmark.EcsLite)     |
| [Massive ECS](https://github.com/cNoNim/ecs-benchmark-massiveecs) | [Benchmark.MassiveEcs](Benchmarks/Benchmark.MassiveEcs) |
| [Morpeh](https://github.com/cNoNim/ecs-benchmark-morpeh)        | [Benchmark.Morpeh](Benchmarks/Benchmark.Morpeh)       |
| [StaticEcs](https://github.com/cNoNim/ecs-benchmark-staticecs)  | [Benchmark.StaticEcs](Benchmarks/Benchmark.StaticEcs) |

#### Dependencies

[Dependencies](Dependencies) folder contains integration projects for frameworks.

Frameworks can be referenced:
* as NuGet packages if the option is provided;
* as NPM git packages, if the benchmark integration is an NPM package;
* otherwise as a git submodule in the [Submodules](Submodules) folder.

#### [Benchmark.Template](Benchmark.Template)

Contains template project for benchmark integration.

### [Benchmark.Runner](Benchmark.Runner)

Runner application runs benchmarks using [BenchmarkDotNet](https://benchmarkdotnet.org).
Runner includes all benchmarks from the **Benchmarks** folder.
Default configuration runs both `.NET 10` and `NativeAOT 10.0` jobs.

### [Benchmark.Tests](Benchmark.Tests)

Contains unit test project that validates that each benchmark produces the same state.
