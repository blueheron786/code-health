# Code Health

[![.NET](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml/badge.svg)](https://github.com/blueheron786/code-health/actions/workflows/dotnet.yml)

**Code Health** is a fast, local-first code analysis tool that helps you understand and improve your codebase without installing giant IDE plugins, setting up servers, or wiring up a SaaS.

🚀 **Scan your code. Catch issues. Stay fast.**

---

### 💡 Why Code Health?

Most code quality tools are either:
- Heavyweight (hello SonarQube),
- Language-specific (like Detekt or ESLint),
- Or locked behind subscriptions and vendor accounts.

**Code Health** gives you:
- ✅ Lightweight, fast CLI scanning
- ✅ Consistent results across languages
- ✅ A visual, interactive UI (built in Blazor)
- ✅ Zero external dependencies at runtime (optional tools downloaded only when needed)

> Analyze what matters. Skip the noise.

---

### 🧠 What It Checks

- Cyclomatic complexity
- TODOs / tech debt comments
- Style violations
- Build warnings
- Unit test coverage
- More to come...

---

### ⚙️ Supported Languages

Currently in active development. Support varies per analysis type.

| Language    | Static Analysis | Runtime (Build/Tests) |
|-------------|-----------------|------------------------|
| C#          | ✅ (Roslyn)     | ✅ Requires .NET SDK   |
| Java        | ✅ (JavaParser) | ✅ Requires JDK 17     |
| JavaScript  | ⚠️ Planned      | ⚠️ Planned             |
| Kotlin, etc.| 🚧 Planned      | 🚧 Planned             |
---

### 🛠️ Getting Started

Make sure you have:
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Java 17 JDK](https://adoptium.net/) on your `PATH` (if analyzing Java projects)

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

Prebuilt Java scanner binaries live in `CodeHealth.Scanners.Java/binaries` (for now).

If you change the Java code:
```bash
cd CodeHealth.Scanners.Java
gradle clean build
```

---

### 🚀 Roadmap Highlights

- Plugin system to download tools like Detekt on-demand
- Visual dashboards of complexity and hotspots

---

### ❤️ Contributing

PRs, feedback, and issue reports are welcome. This project is early and evolving fast — feel free to jump in and help shape it.

---

