# MiniRedis 🧠⚡

**MiniRedis** is a C# in-memory Redis-like server that supports basic operations on `Strings` and `Sorted Sets (ZSets)`. It includes key expiration, thread-safe operations, robust validation, and integration tests, offering a clean API for exploration.

---

## 🚀 Key Features

- 🔹 Redis-style operations: `SET`, `GET`, `DEL`, `INCR`, `DBSIZE`, `ZADD`, `ZCARD`, `ZRANK`, `ZRANGE`
- 🧵 Thread-safe with `ConcurrentDictionary` and fine-grained locking
- ⏱ TTL support via background `Task.Delay`
- 🧪 Full integration tests with `XUnit`
- 🛡 Friendly JSON error handling with input validation
- ⚙ RESTful API, Postman/curl ready
- 🔑Keys and values must match: `[a-zA-Z0-9-_]`

---

## 📦 Setup & Run

1. **Clone the repo**:
   ```bash
   git clone https://github.com/your-user/MiniRedis.git
   cd MiniRedis
   ```

2. **Run the server**:
   ```bash
   dotnet run --project MiniRedis
   ```

3. API will be available at `https://localhost:8080`.

---

## 🧪 Testing

To run all integration tests:

```bash
dotnet test
```

Covers:
- ZSet concurrent addition
- Invalid key validation
- TTL expiration
- ZSet rank/range/count accuracy
- Edge cases like negative indices

---

## 📘 API Overview

### Strings
| Method | Endpoint                  | Description                   |
|--------|---------------------------|-------------------------------|
| POST   | `/strings/{key}`          | Set a value with optional TTL |
| GET    | `/strings/{key}`          | Get a value                   |
| DELETE | `/strings/{key}`          | Delete key                    |
| POST   | `/strings/{key}/increment`| Increment numeric value       |
| GET    | `/dbsize`                 | Get total valid keys          |

### ZSets
| Method | Endpoint                           | Description                |
|--------|------------------------------------|----------------------------|
| POST   | `/zsets/{key}`                     | Add members with scores    |
| GET    | `/zsets/{key}/count`               | Count elements in ZSet     |
| GET    | `/zsets/{key}/rank/{member}`       | Get rank of a member       |
| GET    | `/zsets/{key}/range?start=x&stop=y`| Get members by score range |

---

## ⚙ Tech Stack

- [.NET 8](https://dotnet.microsoft.com/)
- `ASP.NET Core`, `XUnit`, `TestServer`
- `ConcurrentDictionary`, `SortedSet`, LINQ
- Middleware-based JSON error handling

---

## 📂 Project Structure

```
MiniRedis/
├── MiniRedis/
   ├── Controllers/
   │   ├── StringsController.cs
   │   ├── BaseController.cs
   │   ├── SystemController.cs
   │   └── ZSetsController.cs
   ├── Models/
   │   ├── ValueWithExpiry.cs
   │   ├── SortedSetEntry.cs
   │   ├── ErrorResponse.cs
   │   └── Requests/SetRequest.cs
   ├── Stores/
   │   └── InMemoryDataStore.cs
   ├── Services/
   │   ├── IRedisService.cs
   │   └── RedisService.cs
   ├── Utils/
   │   ├── AppStatus.cs
   │   ├── ValidationHelper.cs
   │   └── MetricsTracker.cs
   ├── Tests/
   │   └── ZSetsControllerTests.cs
   ├── Program.cs
   └── README.md
├── MiniRedis.Tests/
   ├── Integration/
   │   ├── StringsControllerTests.cs
   │   └── ZSetsControllerTests.cs
   ├── InMemoryDataStoreTests.cs
```
---
## 📊 Metrics & Health

### 🔍 Health Check Endpoint

MiniRedis includes a lightweight health check endpoint

GET /health

#### ✅ Response Example:
```json
{
    "status": "healthy",
    "uptime": "0d 0h 0m 16s",
    "startedAtUtc": "2025-07-29T08:45:21.7333920Z",
    "version": "1.0.0"
}

- status: Always "ok" if the application is running.
- uptime: Duration since first checked health endpoint.
- startedAt: UTC timestamp of application start time.
- version: the product version
The uptime is calculated using a static AppStatus.StartTime field initialized at health first run. This endpoint is fast, lightweight, and suitable for readiness and liveness probes.

### 📈 Metrics Tracking

MiniRedis tracks usage metrics for Redis-like commands to provide visibility into system behavior and load.

Each key corresponds to a Redis command and shows the number of times that command has been successfully executed since the application started.

🔒 Metrics Exclusions:
The /metrics and /health endpoints do not increment any command counters.

Invalid requests (e.g. malformed JSON, invalid keys) are counted under the errors key.

GET /metrics

#### ✅ Response Example:
```json
{
    "totalCommands": 7,
    "commands": {
        "SET": 1,
        "INCR": 1,
        "ZADD": 1,
        "ZCARD": 2,
        "ZRANGE": 1,
        "ZRANK": 1
    },
    "errors": 2,
    "keysInStore": "3"
}
---

## 👨‍💻 Author

Crafted with care by Victor Del Razo
Inspired by Redis, built to understand and explore.
Thanks to Scopely for the challenge.

---

## 📖 License

MIT License — free to use, modify and distribute.