# Kestrel Junior Http Server
> What *really* happens when a web request hits your server? Let's find out.

### What is this?
**Kestrel Junior** is a simple, educational HTTP/1.1 web server built from the ground up in modern C# (.NET). Instead of using high-level abstractions, its core philosophy is to start with the `System.Net.Sockets.TcpListener` to demystify the magic behind production web servers like the real AspNetCore Kestrel. 

**Key Features:**
*   **Educational Focus:** The project is designed for learning purposes. Every step of the server's operation is explained in detail through comments, making it an ideal resource for understanding how web servers work.
*   **Step-by-Step Development:** The code is incrementally developed, with each tag representing a new feature or improvement. This allows you to follow the evolution of the server and see how it grows over time.
*   **Modular Design:** The project is structured into distinct phases, each with its own set of features and improvements.

### Found this useful? Give it a star ⭐️
If you've found this project helpful for learning about web servers, please consider giving it a star on GitHub! It helps others discover the repository and motivates further development.

**Note:** This project is **not** a production-ready server. It's for learning, allowing you to see and understand the behind-the-scenes of how a web server works.

### Project Structure: Understanding the Codebase, Branches & Tags
*   The source code is filled with comments explaining the "why" behind the implementation. For example:
    ```
    // At this point, MemoryStream contains the raw bytes read from the network stream so far.
    // For example, if the request was "GET / HTTP/1.1\r\nHost: localhost:8080\r\n\r\n",
    // MemoryStream would internally hold the byte sequence corresponding to this string.
    // In hexadecimal representation, these bytes would look like:
    // 47 45 54 20 2F 20 48 54 54 50 2F 31 2E ...
    ```
*   To see the code at the end of each completed step, simply check out its corresponding Git tag. For example:
    ```
    git checkout basic-listener
    ```
The `main` branch always contains the latest, most complete version of the project.

**Branches:**
-   `main` 
-   `phase/1-core-server`
-   `phase/2-response-generation` 
-   `phase/3-stability-security`   
-   `phase/4-advanced-features` 

**Tags:**
-   `basic-listener`
-   `async-loop`
-   `request-line-parsing`
-   `header-parsing`
-   `body-reading`
-   `request-queue`
-   `response-class`
-   `response-buffering`
-   `status-codes`
-   `error-handling`
-   `resource-management`
-   `keep-alive`
-   `streamreader-refactor`
-   `request-size-limits`
-   `request-timeouts`
-   `query-string-parsing`
-   `cookie-parsing`
-   `chunked-encoding`
-   `https-tls`
-   `multi-address`
-   `pipelines-integration`
---
## Development Roadmap
This roadmap tracks the evolution of the Http Server from a simple socket listener to a more compliant and robust HTTP server.
### ✅ Phase 1: Core Server & Request Parsing
*Goal: Establish a basic server that can accept connections, understand raw HTTP requests, and pass them to a consumer queue.*


-   [x] **TCP Listener Setup**
    *   **Task:** Listen on a configured IP and port.
    *   **Approach (The "Why"):** Uses .NET's standard `TcpListener` class for its simplicity and reliability in binding to a network endpoint.
    *   **Limitation:** The configuration (IP/Port) is currently hardcoded, making it inflexible for different environments and requiring a recompile to change.
    *   **Future Improvement:** In **Phase 4 (Tag: `multi-address`)**, we will externalize this into a configuration file. This allows changing addresses without recompiling and is the foundation for supporting multiple listeners. 
    *   **Tag:** `listener-setup`


-   [x] **Asynchronous Connection Loop**
    *   **Task:** Create a non-blocking loop to accept multiple clients, utilizing `CancellationToken` for graceful shutdown.
    *   **Approach (The "Why"):** An `async while` loop with `AcceptTcpClientAsync` prevents the main thread from blocking while waiting for connections.
    *   **Limitation:** Without robust error handling for the acceptance logic itself, an exception here could terminate the server's ability to accept new clients.
    *   **Future Improvement:** In **Phase 3 (Tag: `error-handling`)**, we will add try-catch blocks to handle exceptions gracefully.
    *   **Tag:** `async-loop`


-   [x] **Request Line Parsing**
    *   **Approach (The "Why"):** To understand the protocol deeply, we start with manual, byte-level parsing to see how raw HTTP works.
    *   **Limitation:** This low-level approach is brittle and can fail in real-world network conditions due to **data fragmentation**.
    *   **Future Improvement:** In **Phase 3 (Tag: `streamreader-refactor`)**, we will refactor this to use `StreamReader` for a more robust solution.
    *   **Tag:** `request-line-parsing`


-   [x] **Header Parsing**
    *   **Approach (The "Why"):** Accumulate raw bytes into a `MemoryStream`, then convert the entire block to a string for parsing.
    *   **Limitation:** This is a performance anti-pattern. Calling `ms.ToArray()` creates a **full copy** of the buffer, leading to unnecessary memory allocations.
    *   **Future Improvement:** We will refactor to use `StreamReader` in **Phase 3 (Tag: `streamreader-refactor`)** and ultimately `System.IO.Pipelines` in **Phase 4 (Tag: `pipelines-integration`)** for high-performance parsing.
    *   **Tag:** `header-parsing`


-   [ ] **Request Body Reading**
    *   **Approach (The "Why"):** Learn how to read the request payload based on the `Content-Length` header.
    *   **Limitation:** Reading the entire body into memory is highly inefficient for large file uploads and consumes significant RAM.
    *   **Future Improvement:** Explore streaming reads and `Chunked Transfer Encoding` in Phase 4 (Tag: `chunked-encoding`)
    *   **Tag:** `body-reading`


-   [ ] **Request Queue (`Channel<T>`)**
    *   **Approach (The "Why"):** Decouple the I/O layer from the processing layer to prevent blocking and improve concurrency.
    *   **Limitation (The Problem it Solves):** Without a queue, each new connection must wait for the previous one to be fully processed, severely limiting concurrency. `Channel<T>` provides a high-performance, thread-safe queue to solve this.
    *   **Tag:** `request-queue`


### 🟡 Phase 2: Response Generation & Basic Compliance
*Goal: Build the ability to send back valid HTTP responses and adhere to basic protocol rules.*


-   [ ] **Basic Response Class**
    *   **Task:** Create classes to represent and build an HTTP response.
    *   **Tag:** `response-class`


-   [ ] **Buffer-Based Response Body**
    *   **Approach (The "Why"):** Construct the response payload in a `MemoryStream` before sending.
    *   **Limitation:** Inefficient for large responses (like files) and can delay the Time to First Byte (TTFB).
    *   **Future Improvement:** Move towards streaming responses.
    *   **Tag:** `response-buffering`


-   [ ] **Correct Status & Reason Phrases**
    *   **Task:** Implement a mechanism to map status codes (e.g., 404) to their official reason phrases (e.g., "Not Found").
    *   **Tag:** `status-codes`

### ⏳ Phase 3: Stability, Security & Performance
*Goal: Refactor the project's structure into a clean, layered architecture, then implement key features to make the server more stable, secure, and performant.*

-   [ ] **Architectural Refactoring: Implement a Layered Architecture**
    *   **Task:** Restructure the entire project, drawing inspiration from Kestrel's architecture, to achieve a clear separation of concerns.
    *   **Approach (The "Why"):** The current design, which places most logic in one or two large classes, was suitable for Phase 1 but is too brittle for future development. By refactoring into distinct layers, we improve testability, extensibility, and maintainability.
    *   **Layers:**
        - Core Layer (Core/): The public entry point for the server (KestrelJuniorServer, KestrelJuniorServerOptions)
        - Transport Layer (Transport/): Responsible for low-level network management (SocketTransport)
        - HTTP Layer (Http/): Responsible for parsing and processing the HTTP protocol (HttpParser, HttpContext)
        - Logging Layer (Logging/): Provides structured logging capabilities across all layers
    *   **Advantage:** This refactoring is a fundamental prerequisite for correctly implementing all other tasks in Phase 3 and 4 (such as Keep-Alive and HTTPS).
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


-   [ ] **Introduce `StreamReader` for Request Parsing**
    *   **Task:** Refactor the manual parsing from Phase 1 to use `StreamReader`.
    *   **Approach (The "Why"):** `StreamReader` provides a robust abstraction that handles buffering and data fragmentation internally.
    *   **Tag:** `streamreader-refactor`


-   [ ] **Request Size Limits**
    *   **Task:** Enforce limits on header and body size to prevent memory-exhaustion Denial-of-Service attacks. This is a core server protection mechanism.
    *   **Tag:** `request-size-limits`


-   [ ] **Request Timeouts**
    *   **Task:** Close connections that are idle or sending data too slowly to prevent resource exhaustion from attacks like Slowloris.
    *   **Tag:** `request-timeouts`


### 🚀 Phase 4: Advanced Server Features & Performance Deep Dive
*Goal: Add more advanced, real-world server features and dive deeper into performance optimizations.*


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
