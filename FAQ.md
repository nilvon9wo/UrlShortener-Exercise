# UrlShortener FAQ

This FAQ provides answers to common questions about the design, implementation, and testing of the `UrlShortener` solution.

---

### **1. Why SHA256 for generating short URLs?**
Deterministic hashing ensures the same long URL always produces the same short URL. SHA256 has extremely low collision probability, and mapping to Base62 creates compact, URL-safe codes. Salt counters handle rare collisions.

---

### **2. How are collisions handled?**
If a generated short URL already maps to a different long URL, a salt counter is appended (`#1`, `#2`, …) and a new short URL is generated. This continues until a unique URL is found or the maximum attempt limit is reached.

---

### **3. How is idempotency ensured?**
The deterministic hashing ensures that the same long URL always generates the same short URL, avoiding duplicate entries in the database.

---

### **4. Why preserve the URL scheme (HTTP/HTTPS)?**
Preserving the original scheme ensures correct routing and fidelity. Users can safely retrieve URLs with the correct protocol.

---

### **5. How is this solution testable?**
The `UrlShortener` uses dependency injection for `IUrlMapDb`, allowing mocking in unit tests. TDD covers edge cases including null inputs, collisions, case sensitivity, relative URLs, and unusual schemes.

---

### **6. Why use culture-invariant string comparison (`Ordinal`)?**
URLs are case-sensitive and may contain percent-encoded characters. Ordinal comparison ensures byte-for-byte correctness and avoids locale-specific issues (e.g., Turkish `i` problem).

---

### **7. How could the solution be expanded for real-world business use?**
The core `UrlShortener` class focuses on deterministic, idempotent mapping, collision handling, and URL validation. Additional business-driven functionality should be implemented outside this class, in a **wrapper/decorator (decorator pattern)** or a **consumer service** that exposes the class via an API or serverless endpoint.  

For example, a business could implement:  
- **Hosting / exposure:** Azure App Service, Azure Function, AWS Lambda, or any other service to make the shortener accessible to users.  
- **Analytics / logging:** Track each call to `GetLongUrl` for usage metrics.  
- **URL expiration:** A scheduled cleanup service can remove expired URLs from the database instead of checking expiration on every call. This keeps `UrlShortener` simple. If strict enforcement is required, `GetLongUrl` could optionally check timestamps, but this adds complexity and extra database calls. Business requirements should guide the approach.  
- **Click statistics:** Track usage for reporting or business insights.  

> **Note:** Human-friendly aliases or custom short codes should not be handled by `UrlShortener` itself. If required, these would typically live in a separate alias service, potentially alongside the consumer service that wraps `UrlShortener`. They require different method signatures (e.g., providing a desired alias) and do not share `ShortenUrl`, though retrieval logic could overlap.

This approach keeps the core `UrlShortener` class simple, testable, and maintainable, while enabling business-driven extensions and real-world consumption in a controlled, flexible way.

---
