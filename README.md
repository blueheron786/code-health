# Code Health

[![.NET](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml/badge.svg)](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml)

Analyze your local code-base for code health. Fix things. Go faster. Code better.

- Variety of code-quality and code-health metrics
- Runs entirely locally, no need for network
- Lightweight and fast to run
- No expensive fees

Made in C# with Blazor.

# Why Does This Exist?

Honestly? Because SonarQube is really heavy-weight.

- It requires docker, a DB, etc. to install (not to mention admin permissions)
- It takes ages to scan even small code-bases
- When things don't work (e.g. code coverage analysis), it's virtually impossible to figure out why
- It's expensive (and therefore often not approved in corporate settings)

I just need something lightweight and fast that can look at a local repo and say "yeah ok fix *these* things." That's what Code Health is.

# Supported Languages

**Note:** While basic static code analysis runs in .NET 8, runtime analysis (build warnings, unit test coverage, etc.) require you to have a working dev environment for whichever project you're analyzing.

Everything is WIP since this project is relatively new and under heavy development. Planned languages include:

- C# (requires .NET 8)
- Java
- Javascript

# Design and Architecture

- UI: a Blazor desktop app that consumes analysis data and presents it in a visually interesting way
- Analyzers: single-responsibility classes that analyze, build, or test code for one thing, and spit out data for the UI

Note that not every language will support every analyzer.

Since you need to be able to build your code for runtime analysis, we went the simpler route of using language-specific scanners:

- C# scanners are built in Roslyn
- Java scanners are built in JavaParser


