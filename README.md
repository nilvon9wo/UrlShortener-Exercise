# URL Shortener in C#

A C# implementation of a URL shortener with dependency injection, comprehensive validation, and collision-resistant hash-based short URL generation.

---

## Context

This project was created as a **coding exercise for a job application**.  
It demonstrates:

- A clean and testable C# class library
- Dependency injection design
- Use of TDD to guide implementation
- Sensible architectural decisions extending the minimal brief

---

## Requirements Summary

The assignment required:

- A `UrlShortener` class that:
  - Generates a short URL from a long URL
  - Retrieves the long URL from a short URL
  - Validates inputs
  - Uses a database implementing `IUrlMapDb`
- Dependency injection via constructor
- Unit tests for the class (preferably TDD-driven)

---

## Quick Start

```bash
dotnet test
```

All tests pass, covering URL shortening/retrieval, input validation, collision handling, and configuration options.


## Key Design Decisions

### Type-Safe Public API

The public API uses `System.Uri` instead of raw strings to ensure:

- Built-in validation  
- Safer invocation and clearer intent  
- A clean boundary between typed API usage and the string-based `IUrlMapDb` interface (as required)

### Deterministic Hash-Based Short URLs

Short URL generation is:

- **Deterministic** – the same long URL always produces the same short code  
- **Collision-resistant** – SHA-256 truncated to 8 bytes (~218 trillion combinations)  
- **Self-healing** – automatic salted retries handle extremely rare collisions  

### Configurable Behavior

`UrlShortenerSettings` supports customizing:

- Base domain  
- Code length  
- Retry attempts  
- Supported URI schemes  

Defaults follow common conventions while remaining flexible.

---

## Implementation Highlights

- ✅ Constructor-based dependency injection  
- ✅ Detailed input validation with custom exception types  
- ✅ Scheme preservation (HTTPS stays HTTPS)  
- ✅ LINQ-based functional transformations  
- ✅ Clear separation of concerns (public API vs storage layer)  
- ✅ 100% test coverage of the public API
