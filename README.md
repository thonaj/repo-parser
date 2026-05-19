# Repo Parser — Smart Repository Docu-Bot v2

A full-stack tool that automatically ingests a local code repository, maps its architecture, tracks documentation drift, and flags outdated comments when code changes.

## Architecture

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  File Watcher    │────▶│  AST Parser       │────▶│  Drift Detector  │
│  (.NET Worker)   │     │  (Roslyn)         │     │  (C# Engine)     │
└─────────────────┘     └──────────────────┘     └────────┬────────┘
                                                          │
                                                          ▼
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Vue.js Dashboard│◀────│  SignalR Hub      │◀────│  SQLite + Vectors│
│  (vis-network)   │     │  (Real-time)      │     │  (EF Core)       │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

## Tech Stack

- **Backend**: C# .NET 8 (Clean Architecture)
- **Frontend**: Vue.js 3 + Vite
- **Database**: SQLite with vector embeddings (EF Core)
- **AST Parsing**: Roslyn (C#) + tree-sitter (multi-language)
- **Real-time**: SignalR
- **Testing**: NUnit + Moq

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker (optional, for containerized deployment)

### Setup
```powershell
# Clone and build
git clone https://github.com/thonaj/repo-parser.git

cd repo-parser

# Build backend
dotnet build

# Install frontend dependencies
cd frontend
npm install

# Run
cd ..
dotnet run --project src/RepoParser.Api
```

## Project Structure

```
src/
├── RepoParser.Api/              # ASP.NET Core Web API + SignalR
├── RepoParser.Core/             # Domain models & interfaces
├── RepoParser.Infrastructure/   # Data access, parsers, services
└── RepoParser.Worker/           # Background file watcher
frontend/                        # Vue.js 3 dashboard
tests/                           # NUnit unit tests
```

## License

MIT
