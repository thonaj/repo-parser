# Project Vision: Repo Parser

## Why This Project Exists

Modern software development relies heavily on AI coding agents — tools like Cline, GitHub Copilot, and others that can read, write, and refactor code autonomously. These agents are powerful, but they share a fundamental limitation: **they lack persistent understanding of a codebase's architecture and documentation state.**

A human developer who has worked on a project for months knows which comments are trustworthy, which documentation is stale, and how files depend on each other. An AI agent, by contrast, starts fresh each session — it reads files, infers context, and makes changes without awareness of documentation drift.

**Repo Parser solves this.** It is a tool designed to give AI coding agents (and the humans directing them) a persistent, queryable understanding of a codebase's structure and documentation health.

## The Core Problem

Consider this scenario:

> A developer instructs an AI agent: *"Write an AST parser that identifies all public methods in this directory, matches them to our database embeddings, and alerts us via a Vue component if a method's implementation contradicts its inline documentation."*

This is a complex, multi-step instruction that requires:
1. Parsing source code into an abstract syntax tree
2. Extracting method signatures, parameters, return types, and doc comments
3. Generating vector embeddings of both code and documentation
4. Computing similarity between code and its documentation
5. Surfacing mismatches through a real-time UI

Without a tool like Repo Parser, the AI agent would need to build this infrastructure from scratch each time, or the developer would need to manually maintain documentation and cross-reference it against code changes.

## What Repo Parser Provides

Repo Parser is a **persistent infrastructure layer** that:

1. **Watches your repository** for file changes in real time
2. **Parses source code** into structured method definitions using AST analysis (Roslyn for C#, tree-sitter for other languages)
3. **Generates vector embeddings** of both code implementations and their documentation
4. **Detects documentation drift** — when code changes but comments don't, or when comments contradict the implementation
5. **Exposes everything via API** — a RESTful interface with real-time SignalR push, consumable by both human-facing dashboards and AI agents

## How AI Agents Benefit

When Repo Parser is running alongside a development session, an AI agent can:

- **Query the dependency graph** to understand how files relate before making changes
- **Check for documentation drift** before trusting inline comments
- **Get structured method data** (signatures, parameters, return types) without re-parsing files
- **Receive real-time alerts** when changes introduce documentation inconsistencies

This transforms the AI agent from a stateless code generator into a **context-aware development partner** that understands the project's documentation health.

## The Technical Approach

- **Backend**: C# .NET 8 with Clean Architecture — chosen for performance, type safety, and excellent tooling for AST parsing via Roslyn
- **Frontend**: Vue.js 3 with a visual dependency graph — provides an intuitive interface for exploring codebase structure
- **Storage**: SQLite with vector embeddings — lightweight, zero-configuration, suitable for single-repository scale
- **Real-time**: SignalR — enables live updates as files change
- **Embeddings**: Local ONNX models — no external API dependencies, fully private

## Status

This project is under active development. It is being built as a demonstration of directing AI coding agents through complex, multi-step software engineering tasks — from architecture design through implementation, testing, and deployment.

---

*"The best documentation is the code itself — but only when the code and its documentation agree."*
