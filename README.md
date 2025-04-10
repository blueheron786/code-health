# Code Health

Analyze your local code-base for code health. Fix things. Go faster. Code better.

- Variety of code-quality and code-health metrics
- Runs entirely locally, no need for network
- Lightweight and fast to run
- No expensive fees

Made in C# with Blazor.

# Supported Languages

- C# (WIP)

# Design and Architecture

- UI: a Blazor desktop app that consumes analysis data and presents it in a visually interesting way
- Analyzers: single-responsibility classes that analyze code for one thing, and spit out data for the UI

Note that there's no database involved; all report data lives on disk.