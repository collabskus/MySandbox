# MySandbox

[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Build Status](https://github.com/collabskus/MySandbox/actions/workflows/build.yml/badge.svg)](https://github.com/collabskus/MySandbox/actions/workflows/build.yml)
[![License](https://img.shields.io/github/license/collabskus/MySandbox)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)](#supported-platforms)

A comprehensive .NET 10.0 sandbox project demonstrating clean architecture, dependency injection, Entity Framework Core, and automated multi-platform builds with GitHub Actions.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Running the Application](#running-the-application)
  - [Running Tests](#running-tests)
- [Configuration](#configuration)
- [CI/CD Pipeline](#cicd-pipeline)
  - [Automated Builds](#automated-builds)
  - [Supported Platforms](#supported-platforms)
- [Development](#development)
  - [Adding New Features](#adding-new-features)
  - [Testing Guidelines](#testing-guidelines)
  - [Code Quality](#code-quality)
- [Export Script](#export-script)
- [Technologies Used](#technologies-used)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

MySandbox is a learning and demonstration project showcasing modern .NET development practices. It implements a simple but complete application that manages "stuff items" with full CRUD operations, business logic, and comprehensive testing.

The project serves as a template for building enterprise-grade .NET applications with:
- Clean separation of concerns
- Dependency injection throughout
- Entity Framework Core with SQLite
- Comprehensive unit and integration testing
- Multi-platform CI/CD with GitHub Actions

## ✨ Features

### Core Functionality
- **CRUD Operations**: Full create, read, update, delete operations for stuff items
- **Business Rules Engine**: Configurable business logic with options pattern
- **Data Retention**: Automated cleanup of old items based on retention policies
- **Batch Operations**: Import sample data with configurable prefixes
- **Daily Reporting**: Generate reports on item creation activity

### Development Features
- **Clean Architecture**: Separation of concerns with class library, console app, and tests
- **Dependency Injection**: Full DI container setup with Microsoft.Extensions.DependencyInjection
- **Entity Framework Core**: Database-first approach with SQLite
- **Configuration Management**: JSON-based configuration with strong typing
- **Comprehensive Logging**: Structured logging with Microsoft.Extensions.Logging
- **Input Validation**: Security-focused validation preventing common vulnerabilities
- **OWASP Best Practices**: SQL injection prevention, input sanitization, and DoS protection

### Testing
- **Unit Tests**: Isolated tests for individual components
- **Integration Tests**: End-to-end workflow testing
- **In-Memory Database**: Fast, isolated test database using SQLite in-memory mode
- **Test Fixtures**: Reusable test infrastructure with DatabaseTestBase
- **Fluent Assertions**: AwesomeAssertions library for readable test assertions

### CI/CD
- **Automated Builds**: GitHub Actions workflow for every push to master
- **Multi-Platform Publishing**: Self-contained binaries for 10 platforms
- **Automated Releases**: Tagged releases with all platform binaries
- **Test Automation**: Automated test execution and result reporting

## 🏗️ Architecture

The project follows a layered architecture pattern:

```
MySandbox
├── MySandbox.ClassLibrary (Core Business Logic Layer)
│   ├── Entities (StuffItem)
│   ├── Data Access (StuffDbContext, StuffRepository)
│   ├── Business Logic (StuffDoer)
│   ├── Interfaces (ICanDoStuff, IStuffRepository)
│   └── Configuration (BusinessRulesOptions)
├── MySandbox.Console (Presentation Layer)
│   └── Dependency Injection Setup & Execution
└── MySandbox.Tests (Test Layer)
    ├── Unit Tests
    ├── Integration Tests
    └── Test Infrastructure
```

### Design Patterns

- **Repository Pattern**: Data access abstraction through IStuffRepository
- **Dependency Injection**: Constructor injection throughout the application
- **Options Pattern**: Type-safe configuration with IOptions<T>
- **Interface Segregation**: Separate interfaces for different capabilities (ICanDoStuff, ICanDoStuffA, ICanDoStuffB)
- **Factory Pattern**: Test helpers for creating configured objects

## 📁 Project Structure

```
MySandbox/
├── .github/
│   └── workflows/
│       └── build.yml                    # GitHub Actions CI/CD pipeline
├── docs/
│   └── llm/
│       └── dump.txt                     # Project export for LLM analysis
├── MySandbox.ClassLibrary/              # Core business logic library
│   ├── BusinessRulesOptions.cs          # Configuration model
│   ├── ICanDoStuff.cs                   # Service interface
│   ├── ICanDoStuffA.cs                  # Service interface A
│   ├── ICanDoStuffB.cs                  # Service interface B
│   ├── IStuffRepository.cs              # Repository interface
│   ├── StuffDbContext.cs                # Entity Framework DbContext
│   ├── StuffDoer.cs                     # Business logic implementation
│   ├── StuffItem.cs                     # Entity model
│   └── StuffRepository.cs               # Repository implementation
├── MySandbox.Console/                   # Console application
│   ├── appsettings.json                 # Application configuration
│   └── Program.cs                       # Entry point & DI setup
├── MySandbox.Tests/                     # Test project
│   ├── DatabaseTestBase.cs              # Test base class
│   ├── IntegrationTests.cs              # Integration test suite
│   ├── LogEntry.cs                      # Test logging model
│   ├── StuffDbContextTests.cs           # DbContext tests
│   ├── StuffDoerTests.cs                # Business logic tests
│   ├── StuffItemTests.cs                # Entity tests
│   ├── StuffRepositoryTests.cs          # Repository tests
│   ├── TestBusinessRulesOptions.cs      # Test configuration helper
│   └── TestLogger.cs                    # Test logging implementation
├── .gitignore                           # Git ignore rules
├── Export.ps1                           # PowerShell export script
└── MySandbox.slnx                       # Solution file
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A code editor (Visual Studio 2025, Visual Studio Code, or JetBrains Rider)
- Git (for cloning the repository)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/collabskus/MySandbox.git
   cd MySandbox
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

### Running the Application

**Console Application:**
```bash
cd MySandbox.Console
dotnet run
```

The application will:
1. Create a SQLite database (`stuff.db`) in the console project directory
2. Execute cleanup operations (remove items older than 30 days)
3. Import sample batch data (Batch-Alpha, Batch-Beta, Batch-Gamma)
4. Generate a daily report showing today's items and total count
5. Display all items in the database

### Running Tests

**Run all tests:**
```bash
dotnet test
```

**Run tests with detailed output:**
```bash
dotnet test --verbosity normal
```

**Run tests with code coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Run specific test class:**
```bash
dotnet test --filter "FullyQualifiedName~StuffRepositoryTests"
```

## ⚙️ Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "BusinessRules": {
    "DataRetentionDays": 30,
    "BatchOperationPrefix": "Batch-"
  },
  "SeedData": {
    "Products": [
      "Product A",
      "Product B",
      "Product C"
    ]
  }
}
```

### Configuration Options

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| BusinessRules | DataRetentionDays | 30 | Number of days to retain items before cleanup |
| BusinessRules | BatchOperationPrefix | "Batch-" | Prefix for batch-imported items |
| Logging | LogLevel.Default | "Information" | Minimum log level to display |

## 🔄 CI/CD Pipeline

### Automated Builds

The project uses GitHub Actions to automatically:
1. Build the solution on every push to master
2. Run all tests and collect results
3. Publish self-contained binaries for 10 platforms
4. Create GitHub releases with all binaries

### Workflow Jobs

**Build and Test:**
- Checkout code
- Setup .NET 10.0
- Restore dependencies
- Build solution
- Run tests with result collection
- Upload test results as artifacts

**Publish Binaries (per platform):**
- Checkout code
- Setup .NET 10.0
- Publish self-contained single-file executable
- Create platform-specific archive (ZIP for Windows, TAR.GZ for Unix)
- Upload as artifact

**Create Release:**
- Download all platform artifacts
- Generate timestamped release tag
- Create GitHub release with all binaries
- Include commit information and platform documentation

### Supported Platforms

The CI/CD pipeline produces binaries for the following platforms:

#### Windows
- **win-x64**: Windows 64-bit (Intel/AMD)
- **win-x86**: Windows 32-bit (Intel/AMD)
- **win-arm64**: Windows ARM 64-bit (Surface Pro X, etc.)

#### Linux
- **linux-x64**: Linux 64-bit (Intel/AMD)
- **linux-arm**: Linux ARM 32-bit (Raspberry Pi, etc.)
- **linux-arm64**: Linux ARM 64-bit (Raspberry Pi 4, AWS Graviton)
- **linux-musl-x64**: Alpine Linux 64-bit
- **linux-musl-arm64**: Alpine Linux ARM 64-bit

#### macOS
- **osx-x64**: macOS Intel 64-bit
- **osx-arm64**: macOS Apple Silicon (M1, M2, M3)

All binaries are:
- Self-contained (no .NET runtime installation required)
- Single-file executables
- Optimized and ready to run

### Download Latest Release

Visit the [Releases](https://github.com/collabskus/MySandbox/releases) page to download the latest build for your platform.

## 💻 Development

### Adding New Features

1. **Add business logic** to `MySandbox.ClassLibrary`
2. **Add database entities** to `StuffDbContext`
3. **Update repository** interface and implementation
4. **Wire up DI** in `Program.cs`
5. **Add tests** to `MySandbox.Tests`

### Testing Guidelines

- All public methods should have unit tests
- Integration tests should verify complete workflows
- Use `DatabaseTestBase` for tests requiring database access
- Use `TestLogger<T>` to verify logging behavior
- Use `TestBusinessRulesOptions.Create()` for configuration in tests
- Follow AAA pattern (Arrange, Act, Assert)
- Use AwesomeAssertions for fluent, readable assertions

### Code Quality

The project follows these best practices:

**Security:**
- Input validation on all user-provided data
- Maximum length constraints to prevent DoS attacks
- EF Core parameterized queries prevent SQL injection
- Null reference checks throughout

**Performance:**
- Database indexes on frequently queried columns
- Async/await for all I/O operations
- Efficient LINQ queries with proper filtering

**Maintainability:**
- Clear separation of concerns
- Comprehensive logging
- Strong typing with nullable reference types
- XML documentation on public APIs
- Consistent naming conventions

## 📤 Export Script

The project includes a PowerShell script (`Export.ps1`) that exports the entire codebase to a single text file for LLM analysis or documentation purposes.

**Usage:**
```powershell
.\Export.ps1
```

**Features:**
- Exports all source code files to `docs/llm/dump.txt`
- Includes directory structure using tree command
- Filters out build artifacts, binaries, and node_modules
- Preserves file metadata (size, modification date)
- Includes all C#, JSON, YAML, and configuration files

**Output includes:**
- Complete directory tree
- All source files with headers
- File sizes and timestamps
- Project metadata

The export is useful for:
- Feeding entire codebases to LLMs for analysis
- Creating documentation snapshots
- Code review preparation
- Architecture analysis

## 🛠️ Technologies Used

### Frameworks & Libraries
- **.NET 10.0**: Latest .NET platform
- **Entity Framework Core 10.0.2**: ORM for database access
- **Microsoft.EntityFrameworkCore.Sqlite**: SQLite database provider
- **Microsoft.Extensions.DependencyInjection**: Built-in DI container
- **Microsoft.Extensions.Logging**: Logging infrastructure
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Options**: Options pattern implementation

### Testing
- **xUnit v3.2.2**: Modern testing framework
- **AwesomeAssertions 9.3.0**: Fluent assertion library
- **coverlet.collector**: Code coverage collection
- **Microsoft.NET.Test.Sdk 18.0.1**: Test platform

### Development Tools
- **PowerShell 5+**: Export script
- **GitHub Actions**: CI/CD automation
- **Git**: Version control

### Database
- **SQLite**: Lightweight, embedded database
- **In-Memory SQLite**: For fast, isolated testing

## 🤝 Contributing

Contributions are welcome! This is a sandbox project for learning and experimentation.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Contribution Guidelines

- Follow existing code style and patterns
- Add tests for new functionality
- Update documentation as needed
- Ensure all tests pass before submitting PR
- Keep commits atomic and well-described

## 📄 License

This project is open source and available under the AGPL license.

## 👤 Author

**Kushal**
- GitHub: [@collabskus](https://github.com/collabskus)
- Repository: [MySandbox](https://github.com/collabskus/MySandbox)

## 🙏 Acknowledgments

- Microsoft for the excellent .NET platform and documentation
- The open-source community for amazing libraries and tools
- Entity Framework team for making data access a joy
- xUnit team for the best testing framework

---

**Built with .NET 10.0** • **Powered by GitHub Actions** • **Made with ❤️ for learning**

## 🤖 AI-Assisted Development

*Notice: This project contains code generated by Large Language Models such as Claude and Gemini. All code is experimental whether explicitly stated or not.*
