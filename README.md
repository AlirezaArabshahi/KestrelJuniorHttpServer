# Kestrel Jr.

> What *really* happens when a web request hits your server? Let's find out.

### What is this?

**Kestrel Jr.** is a minimalist, educational HTTP/1.1 web server built from the ground up in modern C# (.NET). Instead
of using high-level abstractions, it's built directly on `System.Net.Sockets.TcpListener` to demystify the magic behind
production web servers like the real Kestrel.

This project is **not** a production-ready server. It's a "glass box" designed for learning, allowing you to see and
understand every step of handling an HTTP request: from the raw TCP connection to parsing byte-by-byte and sending a
response back.

### Project Scope: The Server Layer Only

This project is intentionally focused *only* on the **server layer**. Its job is to handle TCP connections, parse HTTP,
and manage the low-level request/response lifecycle. All framework-level features (like routing, middleware pipelines,
authentication, etc.) are explicitly out of scope. They belong in a separate, higher-level project that will *use*
Kestrel Jr. as its engine.

### Learning Roadmap & Branches

This repository is structured as an educational resource. You can explore the project's evolution step-by-step by
checking out the branch corresponding to each development phase. The `main` branch always contains the latest, most
complete version of the project.

- **Phase 1 Branch:** `phase/1-core-server`
- **Phase 2 Branch:** `phase/2-response-generation`
- **Phase 3 Branch:** `phase/3-stability`
- **Phase 4 Branch:** `phase/4-advanced-features`

---

## Development Roadmap

This roadmap tracks the evolution of Kestrel Jr. from a simple socket listener to a more compliant and robust HTTP
server.

### ✅ Phase 1: Core Server & Request Parsing

*Goal: Establish a basic server that can accept connections, understand raw HTTP requests, and pass them to a consumer
queue.*

-   [x] **TCP Listener Setup:** Listen on a configured IP and port.
-   [x] **Asynchronous Connection Loop:** Create a non-blocking loop to accept multiple clients.
-   [x] **Request Line Parsing:** Parse the first line into Method, Path, and HTTP Version.
-   [x] **Header Parsing:** Read all headers into a key-value dictionary.
-   [x] **Request Body Reading:** Correctly read the body based on `Content-Length`.
-   [x] **Request Queue:** Implement a `Channel<T>` to decouple I/O from processing.

### 🟡 Phase 2: Response Generation & Basic Compliance

*Goal: Build the ability to send back valid HTTP responses and adhere to basic protocol rules.*

-   [ ] **Basic Response Class:** Create classes to represent and build an HTTP response.
-   [ ] **Buffer-Based Body:** Use a `MemoryStream` to build the response body.
-   [ ] **JSON & Text Helpers:** Add methods to easily serialize objects or write strings to the response.
-   [ ] **Correct Status & Reason Phrases:** Implement a mechanism to map status codes (e.g., 404) to their official
    reason phrases (e.g., "Not Found").
-   [ ] **Automatic `Content-Type` Header:** Automatically set the `Content-Type` header based on the response method
    used (`text/plain`, `application/json`).

### ⏳ Phase 3: Stability & Performance

*Goal: Make the server more robust and implement key HTTP/1.1 performance optimizations.*

-   [ ] **Robust Error Handling:** Wrap request processing to catch errors and return a `500 Internal Server Error`
    response instead of crashing.
-   [ ] **Proper Resource Management:** Implement `IDisposable` on connection-related classes to guarantee sockets and
    streams are always closed, preventing resource leaks.
-   [ ] **Persistent Connections (HTTP Keep-Alive):** A critical feature. Keep the connection open after a response to
    handle more requests on the same socket, avoiding TCP handshake overhead.

### 🚀 Phase 4: Advanced Server Features

*Goal: Add more advanced, real-world server features that are still within the scope of a server.*

-   [ ] **Query String Parsing:** Parse the URL's query string (e.g., `/users?id=123`) into an accessible dictionary.
-   [ ] **(Ambitious) Basic HTTPS/TLS Support:** Explore using `SslStream` to handle encrypted `https://` connections.
-   [ ] **(Ambitious) Chunked Transfer Encoding:** Support reading/sending data in chunks when the total content length
    is unknown.
