# LiteWebFramework
> An educational project to build a mini ASP.NET Core and its web server (Kestrel) from scratch in modern C#.

### What is this?
**LiteWebFramework** is a hands-on, educational ecosystem designed to demystify the magic behind modern web stacks. Have you ever wondered how **ASP.NET Core** and its high-performance web server, **Kestrel**, work under the hood? This project answers that question by building simplified versions of both, from the ground up.

This repository is built as a "duo" of projects, mimicking the real-world separation of concerns:

*   **`LiteWeb.Server` (Our "mini-Kestrel")**: A low-level web server built from scratch. It handles raw TCP/IP connections, parses the HTTP/1.1 protocol, and manages the request lifecycle. This is where we get our hands dirty with sockets, streams, and byte manipulation.

*   **`LiteWeb.Framework` (Our "mini-ASP.NET Core")**: A high-level application framework that runs on `LiteWeb.Server`. It provides the elegant abstractions developers love, such as the `WebApp.CreateBuilder()` pattern, routing (`MapGet`), and a clean application model.

---

### Project Structure
This repository is a **Monorepo**, containing multiple related projects. This structure simplifies development and makes it easy to work on the server and framework simultaneously.

```
├── LiteWeb.sln
├── src/
│   ├── LiteWeb.Server/      # The core web server (Class Library)
│   └── LiteWeb.Framework/   # The application framework (Class Library)
└── samples/
    └── WebApp/              # Example app using the framework (Console App)
```

### Project Goals
This project is built for one primary reason: **learning**.

1.  **Demystify the Server (Kestrel):** We'll dive deep into how a web server handles network traffic, manages threads, parses raw HTTP requests, and deals with resource management, all without relying on existing web server libraries.

2.  **Demystify the Framework (ASP.NET Core):** We'll explore how a modern framework provides clean, high-level APIs on top of a low-level server. We'll implement the famous Builder pattern, routing engine, and other familiar concepts.

3.  **Learn by Doing:** The code is heavily commented, and tags (e.g., `git checkout listener-setup`) used to see different stages of development and see how the project evolves from a simple socket listener to a more capable framework.


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

-   [x] **Request Body Reading**
    *   **Approach (The "Why"):** Learn how to read the request payload based on the `Content-Length` header.
    *   **Limitation:** Reading the entire body into memory is highly inefficient for large file uploads and consumes significant RAM.
    *   **Future Improvement:** Explore streaming reads and `Chunked Transfer Encoding` in Phase 5 (Tag: `chunked-encoding`)
    *   **Tag:** `body-reading`

-   [x] **Request Queue (`Channel<T>`)**
    *   **Approach (The "Why"):** Decouple the I/O layer from the processing layer to prevent blocking and improve concurrency. Without a queue, each new connection must wait for the previous one to be fully processed, severely limiting concurrency. `Channel<T>` provides a high-performance, thread-safe queue to solve this.
    *   **Tag:** `request-queue`

### 🟡 Phase 2: Robust Parsing & Abstraction Layer
*Goal: Replace the brittle manual parsing with a robust solution (`StreamReader`) and then build a clean abstraction layer (`HttpContext`) on top of it.*

-   [x] **Restructure Project into Modular Architecture**
    *   **Task:** Split the project into multiple projects, each with a clear responsibility (e.g., `LiteWeb.Server`, `LiteWeb.Framework`).
    *   **Approach (The "Why"):** A modular architecture makes the codebase more maintainable, testable, and scalable. It allows us to focus on one aspect of the project at a time.
    *   **Tag:** `modular-architecture`

-   [x] **Introduce `StreamReader` for Request Parsing**
    *   **Task:** Refactor the manual, byte-level parsing from Phase 1 to use `StreamReader` to read the request line, headers and body.
    *   **Approach (The "Why"):** The manual parsing was educational but fragile. Building our abstractions on top of it would force us to rewrite code later. By refactoring to `StreamReader` *first*, we build the abstraction layer on a solid, reliable foundation, avoiding rework.
    *   **Limitation:** This implementation simplifies request body handling by treating it as a string. This is sufficient for text-based content like JSON or form data but does not support binary data such as file uploads. This limitation will be addressed in a future refactoring task.
    *   **Tag:** `streamreader-refactor`

-   [x] **HTTP Abstractions**
    *   **Task:** Create `HttpRequest`, `HttpResponse`, and `HttpContext` classes.
    *   **Approach (The "Why"):** Now that we have robust parsing, we can channel that data into clean, strongly-typed objects. This is the foundation for building any framework logic.
    *   **Tag:** `http-abstractions`

-   [ ] Refactor Request Body to Support Binary Data
    *   **Task:** Modify the HttpRequest class to store the request body as ReadOnlyMemory<byte> instead of string.
    *   **Approach (The "Why"):** Storing the body as raw bytes makes the server more versatile. It enables handling of non-textual data like file uploads and ensures correct processing of different character encodings, resolving the limitation of the initial StreamReader implementation.
    *   **Tag:** refactor-binary-request


-   [ ] Refactor Response Handling with a Buffered Stream
    *   **Task:** Enhance the HttpResponse class to use an internal MemoryStream as a buffer for the response body.
    *   **Approach (The "Why"):** Buffering the response allows developers to build the response body incrementally using multiple Write calls. This provides greater flexibility than a single-string body and makes it easier to implement helper methods, such as WriteJson, which can directly serialize objects to the buffer.
    *   **Tag:** refactor-buffered-response

-  [ ] **Server-Framework Communication Model: Approach 1 (The "Pull" Model - Rejected)**
    *   **Task:** Establish a clean boundary for how the `LiteWeb.Server` passes a ready-to-process `HttpContext` to the `LiteWeb.Framework`.
    *   ***Approach (The "Why"):** The server could expose a method like `server.GetNextContextAsync()`. The framework would then need to implement a `while` loop to continuously "pull" contexts from the server. This model was rejected as it creates tighter coupling and forces the framework to manage the processing loop.
    *   **Limitation:** This model forces the framework (and by extension, the end-user) to manage the complexity of the request processing loop. It tightly couples the framework to the server's internal queueing mechanism and is generally less intuitive for building applications. This approach was observed in earlier reference code.
    *   **Tag:** `server-framework-pull-model`

-  [ ] **Server-Framework Communication Model: Approach 2 (The "Push" Model - Chosen)**
    *   **Task:** Establish a clean boundary for how the `LiteWeb.Server` passes a ready-to-process `HttpContext` to the `LiteWeb.Framework`.
    *   ***Approach (The "Why"):** The server retains full ownership of its processing loop. When a context is ready, it actively "pushes" it to the framework by invoking a pre-registered handler. This creates a cleaner separation of concerns and a simpler API, aligning with modern frameworks like ASP.NET Core.
    *   **Tag:** `server-framework-push-model`

-   [ ] **Implement a `RequestHandler` Delegate Hook**
    *   **Task:** Implement the chosen "Push Model" by adding a public `Action<HttpContext>` delegate to the `LiteWebServer` class.
    *   **Approach (The "Why"):** This delegate is the critical "hook" that physically implements the Push Model. It allows the server to pass control to the framework without having any knowledge of the framework's internal logic, thus achieving true decoupling.
    *   **Tag:** `request-handler-delegate`

-   [ ] **Implement Response Sending**
    *   **Task:** Create logic within the `HttpResponse` class to serialize its state (status code, headers, body) into a valid HTTP response and write it back to the client's stream.
    *   **Approach (The "Why"):** Without the ability to send a response, the `RequestHandler` hook is incomplete. This task completes the request-response lifecycle, making the server functional.
    *   **Tag:** `response-sending`


### ⏳ Phase 3: Building the `LiteWeb.Framework`
*Goal: Create a new, separate framework project that uses `LiteWeb.Server` to provide high-level application features.*

-   [ ] **Create the Framework Project**
    *   **Task:** Add a new `LiteWeb.Framework` class library project to the solution. This project will reference `LiteWeb.Server`.
    *   **Tag:** `framework-project-setup`

-   [ ] **Implement a Simple Application Host & Builder**
    *   **Task:** Create a simple builder API (e.g., `WebApp.CreateBuilder()`) that configures and starts the server, connecting the framework's logic to the `RequestHandler` delegate.
    *   **Approach (The "Why"):** To provide a clean, user-friendly API for developers to run their web application using your full stack.
    *   **Tag:** `webapp-builder`

-   [ ] **Implement Simple (Manual) Routing**
    *   **Task:** Create a basic router that allows the developer to manually map specific paths to handler functions.
    *   **Approach (The "Why"):** This is the most fundamental form of routing. It provides an explicit, easy-to-understand way to handle requests before introducing more complex, "magic" features. Example usage: `app.MapGet("/hello", context => ...);`
    *   **Tag:** `simple-routing`

-   [ ] **(Advanced) Implement Convention-Based Routing (Controller-Action Style)**
    *   **Task:** Build a more advanced router that uses **Reflection** to automatically discover and map routes based on controller and action method names (e.g., `/Home/Index` maps to `HomeController.Index()`).
    *   **Approach (The "Why"):** This "convention-over-configuration" approach is a core feature of powerful frameworks like **ASP.NET Core**. It dramatically speeds up development by reducing boilerplate code.
    *   **Tag:** `convention-routing`

-   [ ] **(Advanced) Create a `BaseController` Class**
    *   **Task:** Create a base class for controllers to inherit from, providing helper methods.
    *   **Approach (The "Why"):** This class will provide common properties (`HttpContext`) and helper methods (`Ok()`, `NotFound()`, `Json()`) to its children, promoting code reuse and making controller logic cleaner. This is tightly coupled with the controller-based routing task.
    *   **Tag:** `base-controller`

-   [ ] **(New) Evolve to Attribute-Based Routing**
    *   **Task:** Enhance the router to support custom `[Route]` attributes on action methods. The developer will now explicitly register controllers, and the router will use Reflection to find and map these attributed methods.
    *   **Approach (The "Why"):** This is the most powerful and flexible routing strategy, used heavily in modern ASP.NET Core for building APIs. It gives developers ultimate control over their URL structure and demonstrates the evolution from implicit "convention" to explicit "configuration". This exactly matches the latest code from the instructor.
    *   **Tag:** `attribute-routing`


### 🚀 Phase 4: Stability, Security & Performance
*Goal: Make the entire stack (server and framework) more robust, secure, and performant.*

-   [ ] **Architectural Refactoring: Implement a Layered Architecture**
    *   **Task:** Restructure the entire project, to achieve a clear separation of concerns.
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


---

### Getting Started

#### Prerequisites
- .NET SDK (Version 8.0 or newer recommended)

#### How to Run
1.  Clone this repository.
2.  Open a terminal in the root directory.
3.  Run the sample application:
    ```
    dotnet run --project samples/WebApp/WebApp.csproj
    ```
4.  Open your browser and navigate to `http://localhost:11231`. You should see the welcome page from the sample app!

---

### Give it a star ⭐️
If this project helps you understand the internals of .NET web stacks, please consider giving it a star! It helps other developers discover the repository.

### Disclaimer ‼️

This is an educational project and is **not** suitable for production use. It intentionally omits many security features, performance optimizations, and edge-case handling found in real-world software for the sake of clarity and simplicity.