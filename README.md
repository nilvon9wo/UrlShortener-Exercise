# URL Shortener in C#

A C# implementation of a URL shortener with dependency injection, comprehensive validation, and collision-resistant hash-based short URL generation.

---

## Context

This project was created as a **coding exercise for a job application**. The exercise focuses on:
- Implementing a testable C# class library
- Using dependency injection
- Following Test-Driven Development principles

---

## Quick Start

## Running the Tests

```bash
dotnet test
```

All tests pass, covering URL shortening/retrieval, input validation, collision handling, and configuration options.


## Key Design Decisions

### Type-Safe Public API

The `UrlShortener` public API uses `System.Uri` instead of strings for type safety and built-in validation, while the required `IUrlMapDb` interface uses strings internally (as specified in requirements). This separation provides:
- Type safety at the API boundary
- Simple string-based storage
- Clear architectural boundaries

### Hash-Based Short URLs

- **Deterministic** - Same long URL always produces the same short URL
- **Collision-resistant** - SHA256 with 8-byte codes (~218 trillion combinations)
- **Automatic retry** - Salt counter handles rare collisions

### Configurable

Optional `UrlShortenerSettings` allows customization of domain, code length, retry attempts, and supported schemes while maintaining sensible defaults.

---

## Implementation Highlights

- ✅ Dependency injection via constructor
- ✅ Comprehensive input validation with custom exceptions
- ✅ Scheme preservation (HTTP stays HTTP, HTTPS stays HTTPS)
- ✅ LINQ-based functional approach
- ✅ Clean separation of concerns (public API vs internal implementation)
- ✅ 100% test coverage of public API


