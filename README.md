# URL Shortener in C#

This repository contains a C# implementation of a URL shortener class with dependency-injected storage and unit tests.

---

## Overview

The `UrlShortener` class provides functionality to:

- Shorten a given long URL into a unique short URL.
- Retrieve the original long URL from a previously generated short URL.
- Validate inputs and handle errors appropriately.
- Store URL mappings using a database interface (`IUrlMapDb`), which is injected via dependency injection.

An in-memory implementation of `IUrlMapDb` is included for demonstration and testing purposes.

---

## Context

This project was created as a **coding exercise submitted for a job application**. The exercise focuses on:

- Implementing a small, testable C# class library.
- Using dependency injection.
- Writing unit tests (preferably following Test-Driven Development principles).

---

## Project Structure

The solution contains a class library project (`UrlShortener`) in `src/` and an xUnit test project (`UrlShortener.Tests`) in `tests/`. 
The test project references the class library.

---

## Running the Tests

1. Open the solution in Visual Studio or VS Code.
2. Build the solution.
3. Run the unit tests via Test Explorer or the command line:

```bash
dotnet test
```

All tests should pass and cover:
- Shortening URLs
- Retrieving long URLs from short URLs
- Input validation
- Collision handling (if implemented)

--- 

## Notes

- The project uses a simple in-memory storage (`InMemoryUrlMapDb`) to keep the implementation self-contained.
- For production, a real persistent storage could be used.
- Input validation includes checking for null, empty, or invalid URLs.
- Optional: The short URL generation ensures uniqueness and can be extended to handle collisions more robustly.

