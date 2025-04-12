# Code Health

[![.NET](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml/badge.svg)](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml)

Analyze your local code-base for code health, without heavy installations or slow runtimes. Fix things. Go faster. Code better.

- Variety of code-quality and code-health metrics
- Runs entirely locally, no need for network
- Lightweight and fast to run
- No expensive fees

Made in C# with Blazor.

# Supported Languages

**Note:** While basic static code analysis runs in .NET 8, runtime analysis (build warnings, unit test coverage, etc.) require you to have a working dev environment for whichever project you're analyzing.

Everything is WIP since this project is relatively new and under heavy development. Planned languages include:

- C#
- Java
- Javascript

# Developer Environment Setup

Make sure you have:

- C# (.NET 8 SDK)
- JDK 17 and java.exe on the `PATH`

## Building Java Scanners

The repository for Code Health ships with a pre-built version of the code in `CodeHealth.Scanners.Java`, so you can get started quickly. If you make any changes to the Java code and need to rebuild, run `mvn clean build` from the `CodeHealth.Scanners.Java` directory.

# Design and Architecture

- UI: a Blazor desktop app that consumes analysis data and presents it in a visually interesting way
- Scanners: single-responsibility classes that that check one thing, and spit out data for the UI. These subdivide into **static code** scanners (analyze code on disk) and **dynamic** scanners (that do things like run the build/tests and collect output).

Note that not every language will support every type of analysis.

Since you need to be able to build your code for runtime analysis, we went the simpler route of using language-specific scanners:

- C# scanners are built in Roslyn
- Java scanners are built in JavaParser

The implication is that `CodeHealth.Scanners.Java` has a `binaries` directory with the Java scanners