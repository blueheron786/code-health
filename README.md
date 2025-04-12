# Code Health

[![.NET](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml/badge.svg)](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml)

**Code Health** is a fast, local-first code analysis tool that helps you understand and improve your codebase without installing giant IDE plugins, setting up servers, or wiring up a SaaS.

🚀 **Scan your code. Catch issues. Stay fast.**

---

### 💡 Why Code Health?

Most code quality tools are either:
- Heavyweight (hello SonarQube), or
- Language-specific (like Detekt or ESLint), or
- Locked behind subscriptions accounts

**Code Health** gives you:
- ✅ Lightweight, fast CLI scanning
- ✅ Consistent results across languages
- ✅ A visual, interactive UI (built in Blazor)
- ✅ Zero external dependencies at runtime

> Analyze what matters. Skip the noise.

---

### 🧠 What It Checks

- Cyclomatic complexity
- TODOs / tech debt comments
- Style violations
- Build warnings (if enabled)
- Unit test coverage (if enabled)
- More to come...

---

### ⚙️ Supported Languages

Currently in active development. Support varies per analysis type.

| Language    | Static Analysis |
|-------------|-----------------|
| C#          | ✅ Working      |
| Java        | ✅ In Progress  |
| JavaScript  | ⚠️ Planned      |
| Kotlin, etc.| 🚧 Planned      |

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
