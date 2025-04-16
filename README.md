# Code Health

[![.NET](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml/badge.svg)](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml)

![Project dashboard screenshot](https://i.imgur.com/DMpqwV0.png)

**Code Health** is a fast, local-first code analysis tool that helps you understand and improve your codebase without installing giant IDE plugins, setting up servers, or wiring up a SaaS.

🚀 **Scan your code. Catch issues. Stay fast.**

---

### 💡 Why Code Health?

Most code quality tools are either:
- Heavyweight and slow (hello SonarQube), or
- Language-specific (like Detekt or ESLint), or
- Locked behind subscriptions accounts

**Code Health** gives you:
- ✅ Lightweight, fast CLI scanning
- ✅ Consistent results across languages
- ✅ A visual, interactive UI (built in Blazor)
- ✅ Zero external dependencies at runtime

> Analyze what matters. Skip the noise.

### 🧱 Why Not Use Detekt (and Friends)?
Simplicity and scan speed are the core of Code Health.

- Tools like Detekt, PMD, or SpotBugs are powerful—but come with baggage
- They require platform-specific binaries, slow startup times, and their output isn’t made for custom pipelines
- We need something fast, portable, and minimal: no build steps, no IDE integration, just scan and go

This means we avoid runtime scanning (build warnings, unit test coverage, etc.) but we think the trade-off is well worthi t.

---

### 🧠 What It Checks

- Cyclomatic complexity
- TODOs / tech debt comments
- Long methods
- Large classes
- Empty try/catch blocks
- And more ...

---

### ⚙️ Supported Languages

Currently in active development. Support varies per analysis type.

| Language    | Static Analysis |
|-------------|-----------------|
| C#          | ✅ Working      |
| Java        | ✅ Working      |
| Kotlin      | ✅ Working      |
| JavaScript  | ✅ Working      |

---

### 🛠️ Getting Started

Make sure you have .NET 8 installed. 

Run the app:
```bash
dotnet run --project CodeHealth.App
```

---

### 🔧 Developer Notes

The architecture is intentionally modular:

- **Scanners** do the hard work — language-specific, single-responsibility analyzers
- **UI** renders clean summaries grouped by file, language, and issue type
- **Data** is stored locally in JSON files so you can inspect, diff, or reprocess them

---

### 🚀 Roadmap Highlights

- Visual dashboards of complexity and hotspots
- Language usage graphs
- Language-agnostic analysis (e.g. duplication)

---

### ❤️ Contributing

PRs, feedback, and issue reports are welcome. This project is early and evolving fast — feel free to jump in and help shape it.

---
