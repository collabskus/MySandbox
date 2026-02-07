# MySandbox

[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Build Status](https://github.com/collabskus/MySandbox/actions/workflows/build.yml/badge.svg)](https://github.com/collabskus/MySandbox/actions/workflows/build.yml)
[![License](https://img.shields.io/github/license/collabskus/MySandbox)](LICENSE)

**⚠️ AI CONTENT WARNING**: This project contains code generated and iterated on by LLMs (Claude, Gemini). It demonstrates both the potential and pitfalls of AI-assisted development.

## 📋 Table of Contents

- [What This Actually Is](#what-this-actually-is)
- [What Changed (The Fixes)](#what-changed-the-fixes)
- [Pedagogical Value](#pedagogical-value)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Load Testing](#-load-testing)
- [CI/CD Pipeline](#-cicd-pipeline)
- [What's Still Wrong](#whats-still-wrong)

## What This Actually Is

A .NET 10.0 console application that demonstrates:
- ✅ Dependency Injection syntax
- ✅ Entity Framework Core basics
- ✅ GitHub Actions multi-platform builds
- ❌ ~~Good application architecture~~ (it's getting better, but still has issues)

This is a **learning sandbox**, not production code. Use it to learn .NET syntax and tooling. Do not use it as an architectural reference without understanding the remaining problems.

## What Changed (The Fixes)

### Critical Fixes Applied

#### 1. **The Memory Bomb** → FIXED ✅
**Before:**
```csharp
var existingItems = await _repository.GetAllAsync(); // Loads ENTIRE table into memory
var exists = existingItems.Any(x => x.Name == name);
```

**After:**
```csharp
var exists = await _dbContext.StuffItems.AnyAsync(x => x.Name == name); // Database-side check
```

**Impact:** Application can now handle databases with millions of records without crashing.

#### 2. **N+1 Delete Problem** → FIXED ✅
**Before:**
```csharp
foreach (var item in oldItems) { 
    await _repository.RemoveAsync(item.Id); // 50 items = 100 database roundtrips
}
```

**After:**
```csharp
var deletedCount = await _dbContext.StuffItems
    .Where(x => x.CreatedAt < cutoffDate)
    .ExecuteDeleteAsync(); // Single database operation
```

**Impact:** Cleanup of 1000 old records went from ~2000 queries to 1 query.

#### 3. **Configuration Abuse** → FIXED ✅
**Before:**
```json
"Products": ["Product 1", "Product 2", ... "Product 1000"] // 1000 entries in JSON
```

**After:**
```json
"Products": ["Alpha", "Beta", "Gamma", "Delta", "Epsilon"] // 5 sample entries
```

**Impact:** Configuration file is now readable and maintainable. Use `--load-test` for bulk data.

#### 4. **Architecture Changes**
- `StuffDoer` now injects `StuffDbContext` directly instead of relying on repository abstraction for everything
- Repository pattern kept for backward compatibility, but direct `DbContext` usage is now demonstrated as the preferred approach
- Added `ImportLargeDatasetAsync()` method with batching for performance testing

## Pedagogical Value

### ✅ What This Teaches Well

1. **Dependency Injection Syntax**
   - Service registration in `Program.cs`
   - Constructor injection
   - Interface-based design

2. **Entity Framework Core Basics**
   - `DbContext` configuration
   - Model mapping with `OnModelCreating`
   - Database-side queries vs in-memory operations

3. **The Cost of Abstraction**
   - Shows when repository pattern helps (testability, isolation)
   - Shows when it hurts (performance, unnecessary indirection)
   - Demonstrates direct `DbContext` usage as a valid alternative

4. **GitHub Actions**
   - Multi-platform builds (Windows, Linux, macOS)
   - Multi-architecture support (x64, ARM64)
   - Automated releases

5. **Common Performance Pitfalls**
   - Before/after examples of database performance anti-patterns
   - Real-world fixes with measurable impact

### ⚠️ What This Still Doesn't Teach

1. **Meaningful Domain Modeling**
   - Names like "StuffDoer" teach nothing about domain-driven design
   - "DoStuff" methods hide business intent

2. **Real Testing Strategy**
   - Integration tests exist but are minimal
   - No unit tests of business logic (because there's no complex business logic)

3. **Production Patterns**
   - No error handling strategy
   - No retry logic
   - No distributed tracing or observability

## 📁 Project Structure

```
MySandbox/
├── .github/workflows/build.yml   # ✅ Strong: Multi-platform CI/CD
├── MySandbox.ClassLibrary/       # ⚠️ Mixed: Logic & Data Access
│   ├── StuffDbContext.cs         # ✅ EF Core basics done right
│   ├── StuffDoer.cs              # ✅ Fixed: Now uses database-side queries
│   └── StuffRepository.cs        # ⚠️ Kept for compatibility, but less necessary
├── MySandbox.Console/            # ✅ Entry Point with --load-test flag
│   ├── appsettings.json          # ✅ Fixed: Minimal sample data
│   └── Program.cs                # ✅ DI composition & CLI args
└── MySandbox.Tests/              # ✅ Integration tests with in-memory SQLite
```

## ✨ Functionality

1. **Database Initialization**: Creates SQLite database on startup
2. **Cleanup (DoStuffA)**: Deletes items older than configured days using **bulk delete**
3. **Import (DoStuffB)**: Imports sample data with **database-side duplicate checking**
4. **Reporting (DoStuff)**: Counts items using **database aggregation**
5. **Load Testing**: `--load-test` flag for bulk data import with batching

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK

### Running the App
```bash
cd MySandbox.Console
dotnet run
```

### Running Tests
```bash
dotnet test
```

## 🔥 Load Testing

The application now includes a `--load-test` flag for performance testing:

```bash
# Import 1 million items (default)
dotnet run --load-test

# Import custom amount
dotnet run --load-test 5000000

# Short form
dotnet run -l 100000
```

**What this demonstrates:**
- Batched inserts (10,000 items per transaction)
- Progress logging every 50,000 items
- How SQLite handles large datasets
- Scalability of the fixed query patterns

**Performance on typical hardware:**
- 100,000 items: ~5-10 seconds
- 1,000,000 items: ~50-90 seconds
- 5,000,000 items: ~4-7 minutes

## 📦 CI/CD Pipeline

The **most production-ready** part of this repository.

Demonstrates:
- Multi-platform builds (Windows x64/x86/ARM64, Linux x64/ARM/ARM64/musl, macOS x64/ARM64)
- Self-contained single-file executables
- Automated GitHub Releases
- Artifact uploading

See `.github/workflows/build.yml` for the complete workflow.

## What's Still Wrong

### 1. **Generic Naming** (Unfixable Without Domain Change)
- `StuffDoer`, `DoStuff`, `StuffItem` convey zero business meaning
- **Why not fixed:** This is a sandbox. Renaming to domain-specific names would require inventing a fake domain
- **Lesson:** In real projects, fight for meaningful names from day one

### 2. **Repository Pattern Debate** (Partially Addressed)
- Repository abstraction adds little value here
- **What changed:** Direct `DbContext` usage now demonstrated in `StuffDoer`
- **What stayed:** Repository kept for backward compatibility and testing examples
- **Lesson:** Don't add abstraction layers "just because." Use them when they solve actual problems (like isolating EF from business logic in complex domains)

### 3. **Missing Production Patterns**
- No retry logic for transient failures
- No circuit breakers
- No health checks
- **Why not fixed:** Out of scope for a syntax demo
- **Lesson:** These matter in production. Don't skip them.

### 4. **SQLite Limitations**
- Not suitable for multi-user scenarios
- Limited concurrency
- **Why not fixed:** Perfect for a local demo/sandbox
- **Lesson:** Use PostgreSQL/SQL Server/MySQL for real applications

## 🎓 How to Use This Project

**DO:**
- Reference syntax for DI, EF Core, GitHub Actions
- Study the before/after performance fixes
- Use as a starting point for experimentation
- Copy the CI/CD workflow

**DON'T:**
- Copy the architecture to production
- Use SQLite for multi-user applications
- Keep generic names like "StuffDoer"
- Skip error handling like this code does

## 📝 Final Thoughts

This project is what happens when you ask an LLM to "make a .NET sandbox" and then iteratively fix the problems. It's pedagogically valuable **because** it made mistakes and shows the fixes.

The CI/CD pipeline is genuinely good. The syntax demonstrations are accurate. The architecture is "good enough" for a learning tool but would fail code review in a professional setting.

If that's what you need, you're in the right place. If you need production-ready patterns, look elsewhere (or use this as a "what to improve" checklist).

---

*This project demonstrates both the power and limitations of LLM-generated code. All mistakes were made with the best of intentions by Claude and Gemini. All fixes were made with human guidance and validation.*
