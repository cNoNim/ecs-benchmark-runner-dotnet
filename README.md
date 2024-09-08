# Entity-Component-System Benchmarks for .Net

[![License](https://img.shields.io/github/license/cNoNim/ecs-benchmark-runner-dotnet)](https://github.com/cNoNim/ecs-benchmark-runner-dotnet?tab=MIT-1-ov-file#readme)
[![Stars](https://img.shields.io/github/stars/cNoNim/ecs-benchmark-runner-dotnet?color=brightgreen)](https://github.com/cNoNim/ecs-benchmark-runner-dotnet/stargazers)

This repository contains a collection of benchmarks for .Net Entity-Component-System (ECS) frameworks. 
Benchmarks perform a complex performance comparison of ECS frameworks on a near-real-world scenario.

[**Unity Version**](https://github.com/cNoNim/ecs-benchmark-runner-unity)

### Frameworks:
|                                                        ECS | Version                                                                                                                                                                                                                                                                                                                                            | Implemented |
|-----------------------------------------------------------:|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:-----------:|
|                    [Arch](https://github.com/genaray/Arch) | [1.2.8.2-alpha](https://www.nuget.org/packages/Arch/1.2.8.2-alpha)<br/>[Arch.System](https://github.com/genaray/Arch.Extended) - [1.0.5](https://www.nuget.org/packages/Arch.System/1.0.5)<br/>[Arch.System.SourceGenerator](https://github.com/genaray/Arch.Extended) - [1.2.1](https://www.nuget.org/packages/Arch.System.SourceGenerator/1.2.1) |      ✅     |
|       [DragonECS](https://github.com/DCFApixels/DragonECS) | [0.8.42](https://github.com/DCFApixels/DragonECS/commit/d159bbff4ba661b9c6a8b1a054c729c2c58cbaf8)                                                                                                                                                                                                                                                  |      ✅     |
|      [FriFlo](https://github.com/friflo/Friflo.Engine.ECS) | [3.0.0-preview.13](https://www.nuget.org/packages/Friflo.Engine.ECS/3.0.0-preview.13)                                                                                                                                                                                                                                                              |      ✅     |
|          [LeoECSLite](https://github.com/Leopotam/ecslite) | [2024.5.22](https://github.com/Leopotam/ecslite/releases/tag/2024.5.22)                                                                                                                                                                                                                                                                            |      ✅     |
|               [Morpeh](https://github.com/scellecs/morpeh) | [2024.1.0-rc49](https://github.com/scellecs/morpeh/releases/tag/2024.1.0-rc49)                                                                                                                                                                                                                                                                     |      ✅     |

## Running

0. Install [NPM](https://nodejs.org/en/download/)
1. Clone repository
   ```sh
   git clone https://github.com/cNoNim/ecs-benchmark-runner-dotnet.git
   ```
2. Run

   Windows
   ```cmd
   cd .\Benchmark.Runner
   dotnet run -c Release --filter *
   ```
   Mac/Linux
   ```sh
   cd ./Benchmark.Runner
   dotnet run -c Release --filter \*
   ```

## Structure

The benchmark is divided into repositories. 
Each repository is an NPM package.
Current repository integrate packages into runner project for .Net.

### [Benchmark.Core Package](https://github.com/cNoNim/ecs-benchmark-core)

A separate repository contains common assemblies that are used by benchmarks or by infrastructure.
Integration is done in the [Benchmark.Core](Benchmark.Core) project

#### [Benchmark.Generator](https://github.com/cNoNim/ecs-benchmark-core/tree/main/Runtime/Benchmark.Core/SourceGenerators/Sources~/Benchmark.Generator)

Part of **Benchmark.Core** package.
**Benchmark.Generator** is a source generator that checks referenced assemblies and looks for implementations of `Benchmark.Core.IContext` and generates `Benchmark.Contexts.Factories` that are used to get all the contexts involved in the benchmark.
Integration is done in the [Benchmark.Generator](Benchmark.Generator) project

### Benchmark Projects

Each benchmark is a separate repository, integration is done through separate projects in the [Public](Public) folder.

|                                                      Repository | Project                                           |
|----------------------------------------------------------------:|:--------------------------------------------------|
| [Arch](https://github.com/cNoNim/ecs-benchmark-archecs)         | [Benchmark.ArchEcs](Public/Benchmark.ArchEcs)     |
| [Dragon ECS](https://github.com/cNoNim/ecs-benchmark-dragonecs) | [Benchmark.DragonEcs](Public/Benchmark.DragonEcs) |
| [FriFlo ECS](https://github.com/cNoNim/ecs-benchmark-frifloecs) | [Benchmark.FrifloEcs](Public/Benchmark.FrifloEcs) |
| [LeoEcsLite](https://github.com/cNoNim/ecs-benchmark-ecslite)   | [Benchmark.EcsLite](Public/Benchmark.EcsLite)     |
| [Morpeh](https://github.com/cNoNim/ecs-benchmark-morpeh)        | [Benchmark.Morpeh](Public/Benchmark.Morpeh)       |

#### Dependencies

[Dependencies](Dependencies) folder contains integration projects for frameworks.

Frameworks can be referenced:
* as Nuget packages if the option is provided;
* as NPM git packages, if framework is an NPM package;
* otherwise as a git submodule in the [Submodules](Submodules) folder.

#### [Benchmark.Template](Benchmark.Template)

Contains template project for benchmark integration.

### [Benchmark.Runner](Benchmark.Runner)

Runner application runs benchmarks using [BenchmarkDotNet](https://benchmarkdotnet.org).
Runner include all benchmarks from **Public** folder.

### [Benchmark.Tests](Benchmark.Tests)

Contains unit test project that validates that each benchmark produces the same state.
