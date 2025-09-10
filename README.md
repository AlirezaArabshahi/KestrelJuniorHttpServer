# LiteWebFramework
> What *really* happens when a web request hits your server? Let's build a full web stack from scratch and find out.

### What is this?
**LiteWebFramework** is a minimal, educational web ecosystem built from the ground up in modern C# (.NET). It is designed to demystify the magic behind production web stacks like ASP.NET Core by building both a web server and an application framework from scratch.

This repository contains two primary projects:
*   **`LiteWeb.Server`**: A low-level HTTP/1.1 web server responsible for handling raw TCP connections and parsing the HTTP protocol.
*   **`LiteWeb.Framework`**: A high-level application framework that runs on top of `LiteWeb.Server`, providing features like routing and a simplified application model.

### Key Features
*   **Educational Focus:** The entire stack is designed for learning. Every step of its operation is explained through detailed comments and a structured roadmap. For example:
    ```
    // At this point, MemoryStream contains the raw bytes read from the network stream so far.
    // For example, if the request was "GET / HTTP/1.1\r\nHost: localhost:8080\r\n\r\n",
    // MemoryStream would internally hold the byte sequence corresponding to this string.
    // In hexadecimal representation, these bytes would look like:
    // 47 45 54 20 2F 20 48 54 54 50 2F 31 2E ...
    ```
*   **Step-by-Step Development:** The code is developed incrementally. Each Git tag represents a new, completed feature, allowing you to follow the evolution of the project.
*   **Layered Architecture:** The project is explicitly structured into a server layer and a framework layer, demonstrating a clean separation of concerns, just like real-world web stacks.

### Found this useful? Give it a star ⭐️
If you've found this project helpful for learning about web servers and frameworks, please consider giving it a star on GitHub! It helps others discover the repository and motivates further development.

**Note:** This project is **not** a production-ready stack. It is built for learning, allowing you to see and understand the behind-the-scenes of how a web server and framework work together.

### Project Structure: Understanding the Monorepo
This repository is a **Monorepo**, meaning it contains multiple related projects in one place. This structure simplifies development and makes it easy to work on the server and framework simultaneously.
*   `src/LiteWeb.Server/`: The web server class library.
*   `src/LiteWeb.Framework/`: The application framework class library.
*   `samples/WebApp/`: A sample console application that uses both the server and framework to run a simple website.

To see the code at the end of each completed step, simply check out its corresponding Git tag. For example:
    ```
    git checkout basic-listener
    ```
    The `main` branch always contains the latest, most complete version of the project.

---
## Development Roadmap

### ✅ Phase 1: Core Server & Request Parsing
*Goal: Establish a basic server that can accept connections, understand raw HTTP requests, and pass them to a consumer queue.*

-   [x] **TCP Listener Setup**
    *   **Task:** Listen on a configured IP and port.
    *   **Approach (The "Why"):** Uses .NET's standard `TcpListener` class for its simplicity and reliability in binding to a network endpoint.
    *   **Limitation:** The configuration (IP/Port) is currently hardcoded, making it inflexible for different environments and requiring a recompile to change.
    *   **Future Improvement:** In **Phase 5 (Tag: `multi-address`)**, we will externalize this into a configuration file. This allows changing addresses without recompiling and is the foundation for supporting multiple listeners. 
    *   **Tag:** `listener-setup`

-   [x] **Asynchronous Connection Loop**
    *   **Task:** Create a non-blocking loop to accept multiple clients, utilizing `CancellationToken` for graceful shutdown.
    *   **Approach (The "Why"):** An `async while` loop with `AcceptTcpClientAsync` prevents the main thread from blocking while waiting for connections.
    *   **Limitation:** Without robust error handling for the acceptance logic itself, an exception here could terminate the server's ability to accept new clients.
    *   **Future Improvement:** In **Phase 4 (Tag: `error-handling`)**, we will add try-catch blocks to handle exceptions gracefully.
    *   **Tag:** `async-loop`

-   [x] **Request Line Parsing**
    *   **Approach (The "Why"):** To understand the protocol deeply, we start with manual, byte-level parsing to see how raw HTTP works.
    *   **Limitation:** This low-level approach is brittle and can fail in real-world network conditions due to **data fragmentation**.
    *   **Future Improvement:** In **Phase 2 (Tag: `streamreader-refactor`)**, we will refactor this to use `StreamReader` for a more robust solution.
    *   **Tag:** `request-line-parsing`

-   [x] **Header Parsing**
    *   **Approach (The "Why"):** Accumulate raw bytes into a `MemoryStream`, then convert the entire block to a string for parsing.
    *   **Limitation:** This is a performance anti-pattern. Calling `ms.ToArray()` creates a **full copy** of the buffer, leading to unnecessary memory allocations.
    *   **Future Improvement:** We will refactor to use `StreamReader` in **Phase 2 (Tag: `streamreader-refactor`)** and ultimately `System.IO.Pipelines` in **Phase 5 (Tag: `pipelines-integration`)** for high-performance parsing.
    *   **Tag:** `header-parsing`

-   [x] **Request Body Reading**
    *   **Approach (The "Why"):** Learn how to read the request payload based on the `Content-Length` header.
    *   **Limitation:** Reading the entire body into memory is highly inefficient for large file uploads and consumes significant RAM.
    *   **Future Improvement:** Explore streaming reads and `Chunked Transfer Encoding` in Phase 5 (Tag: `chunked-encoding`)
    *   **Tag:** `body-reading`

-   [x] **Request Queue (`Channel<T>`)**
    *   **Approach (The "Why"):** Decouple the I/O layer from the processing layer to prevent blocking and improve concurrency.
    *   **Limitation (The Problem it Solves):** Without a queue, each new connection must wait for the previous one to be fully processed, severely limiting concurrency. `Channel<T>` provides a high-performance, thread-safe queue to solve this.
    *   **Tag:** `request-queue`

### 🟡 Phase 2: Robust Parsing & Abstraction Layer
*Goal: Replace the brittle manual parsing with a robust solution (`StreamReader`) and then build a clean abstraction layer (`HttpContext`) on top of it.*

-   [ ] **Introduce `StreamReader` for Request Parsing**
    *   **Task:** Refactor the manual, byte-level parsing from Phase 1 to use `StreamReader` to read the request line and headers.
    *   **Approach (The "Why"):** The manual parsing was educational but fragile. Building our abstractions on top of it would force us to rewrite code later. By refactoring to `StreamReader` *first*, we build the abstraction layer on a solid, reliable foundation, avoiding rework.
    *   **Tag:** `streamreader-refactor`

-   [ ] **Create Core HTTP Abstractions**
    *   **Task:** Create `HttpRequest`, `HttpResponse`, and `HttpContext` classes.
    *   **Approach (The "Why"):** Now that we have robust parsing, we can channel that data into clean, strongly-typed objects. This is the foundation for building any framework logic.
    *   **Tag:** `http-abstractions`

-   [ ] **Implement a `RequestHandler` Delegate Hook**
    *   **Task:** Add a public `Action<HttpContext>` delegate to the `HttpListener` class.
    *   **Approach (The "Why"):** This is the critical "hook" that decouples the server from the framework. The server's only job is to create the `HttpContext` and pass it to this handler for processing.
    *   **Tag:** `request-handler-delegate`

-   [ ] **Implement Response Sending**
    *   **Task:** Create logic within the `HttpResponse` class to serialize its state (status code, headers, body) into a valid HTTP response and write it back to the client's stream.
    *   **Tag:** `response-sending`

### ⏳ Phase 3: Building the `LiteWeb.Framework`
*Goal: Create a new, separate framework project that uses `LiteWeb.Server` to handle web requests and provides high-level features like routing.*

-   [ ] **Create the Framework Project**
    *   **Task:** Add a new `LiteWeb.Framework` class library project to the solution. This project will reference `LiteWeb.Server`.
    *   **Tag:** `framework-project-setup`

-   [ ] **Implement a Simple Application Host & Builder**
    *   **Task:** Create a simple builder API (e.g., `WebApp.CreateBuilder()`) that configures and starts the server, connecting the framework's logic to the `RequestHandler` delegate.
    *   **Approach (The "Why"):** To provide a clean, user-friendly API for developers to run their web application using your full stack.
    *   **Tag:** `webapp-builder`

-   [ ] **Implement Basic Routing**
    *   **Task:** Build a simple router that inspects `HttpContext.Request.Path` and executes a user-defined function for that path.
    *   **Tag:** `basic-routing`

### 🚀 Phase 4: Stability, Security & Performance
*Goal: Make the entire stack (server and framework) more robust, secure, and performant.*

-   [ ] **Architectural Refactoring: Implement a Layered Architecture**
    *   **Task:** Restructure the entire project, drawing inspiration from Kestrel's architecture, to achieve a clear separation of concerns.
    *   **Approach (The "Why"):** The current design, which places most logic in one or two large classes, was suitable for Phase 1 but is too brittle for future development. By refactoring into distinct layers, we improve testability, extensibility, and maintainability.
    *   **Layers:**
        - Core Layer (Core/): The public entry point for the server (LiteWebServer, LiteWebServerOptions)
        - Transport Layer (Transport/): Responsible for low-level network management (SocketTransport)
        - HTTP Layer (Http/): Responsible for parsing and processing the HTTP protocol (HttpParser, HttpContext)
        - Logging Layer (Logging/): Provides structured logging capabilities across all layers
    *   **Advantage:** This refactoring is a fundamental prerequisite for correctly implementing all other tasks in Phase 4 and 5 (such as Keep-Alive and HTTPS).
    *   **Tag:** `architectural-refactor`

-   [ ] **Robust Error Handling**
    *   **Task:** Wrap request processing and the main listener loop to catch errors, log them, and prevent the server from crashing.
    *   **Tag:** `error-handling`

-   [ ] **Proper Resource Management (`IDisposable`)**
    *   **Task:** Use `IDisposable` to guarantee sockets and streams are always closed, preventing resource leaks.
    *   **Tag:** `resource-management`

-   [ ] **Persistent Connections (HTTP Keep-Alive)**
    *   **Approach (The "Why"):** Implement this critical HTTP/1.1 optimization to avoid the expensive TCP handshake for subsequent requests from the same client.
    *   **Limitation (Without It):** Closing the connection after each response severely degrades performance.
    *   **Tag:** `keep-alive`

-   [ ] **Request Size Limits**
    *   **Task:** Enforce limits on header and body size to prevent memory-exhaustion Denial-of-Service attacks. This is a core server protection mechanism.
    *   **Tag:** `request-size-limits`

-   [ ] **Request Timeouts**
    *   **Task:** Close connections that are idle or sending data too slowly to prevent resource exhaustion from attacks like Slowloris.
    *   **Tag:** `request-timeouts`

### 🌌 Phase 5: Advanced Features & Deep Dive
*Goal: Add more advanced, real-world server and framework features, and explore deeper performance optimizations.*

-   [ ] **Query String Parsing**
    *   **Task:** Parse the URL's query string (e.g., `/users?id=123`) into a dictionary.
    *   **Tag:** `query-string-parsing`

-   [ ] **Cookie Parsing**
    *   **Task:** Parse the `Cookie` request header into a structured collection. The server is responsible for parsing protocol elements; the framework is responsible for *using* them.
    *   **Tag:** `cookie-parsing`

-   [ ] **(Ambitious) Chunked Transfer Encoding**
    *   **Task:** Support reading/sending data in chunks when the content length is unknown.
    *   **Tag:** `chunked-encoding`

-   [ ] **(Ambitious) Basic HTTPS/TLS Support**
    *   **Task:** Explore using `SslStream` to handle encrypted `https://` connections.
    *   **Tag:** `https-tls`

-   [ ] **(Ambitious) Multiple Listening Addresses**
    *   **Task:** Support listening on multiple IP addresses and ports simultaneously, similar to production web servers.
    *   **Tag:** `multi-address`

-   [ ] **(Ambitious) Full `System.IO.Pipelines` Integration**
    *   **Approach (The "Why"):** To achieve peak I/O performance. `Pipelines` is a sophisticated API that minimizes data copies ("zero-copy" parsing) and reduces memory allocations.
    *   **Limitation (of Previous Methods):** `StreamReader` and manual `byte[]` are not ideal for max performance due to buffer copying and GC pressure.
    *   **Tag:** `pipelines-integration`
