# MiniRedis

&#x20; &#x20;

A minimal Redis-like in-memory datastore implemented in C#, supporting basic string operations and sorted sets (ZSets), with expiration support and a clean RESTful API design.

---

## 🚀 Features

### ✅ Strings

- `SET` with optional TTL
- `GET` with TTL validation
- `DEL` key
- `INCR` auto-initializing integer increment

### ✅ Sorted Sets (ZSet)

- `ZADD` with score
- `ZCARD` (count members)
- `ZRANK` (rank of a member)
- `ZRANGE` (members in a score range, inclusive)

### ✅ Expiration

- Optional expiration (`ttlSeconds`) handled asynchronously

### ✅ RESTful API Design

- POST/GET/DELETE endpoints with input validation
- Clean JSON responses

### ✅ Validation Rules

- Keys and values must match: `[a-zA-Z0-9-_]`
- Enforced via `ValidationHelper`

### ✅ Tests

- ✅ Unit tests for internal store logic (in-memory)
- ✅ Integration tests for API (via TestServer)
- ✅ Concurrency test for multiple `INCR` calls

---

## 🧪 Running the Tests

```bash
dotnet test
```

Tests are located in:

- `MiniRedis.Tests`: Unit and integration coverage
- Integration tests follow REST patterns and verify response consistency

---

## 📦 Project Structure

```
MiniRedis/
├── Controllers/
│   ├── StringsController.cs
│   └── ZSetsController.cs
├── Models/
│   ├── Requests/
│   │   ├── SetRequest.cs
│   │   └── ZAddRequest.cs
│   └── ValueWithExpiry.cs
│   └── SortedSetEntry.cs
├── Services/
│   ├── IRedisService.cs
│   └── RedisService.cs
├── Stores/
│   └── InMemoryDataStore.cs
├── Utils/
│   └── ValidationHelper.cs
├── Program.cs
└── MiniRedis.csproj
```

Tests live in `MiniRedis.Tests/` with folders for:

- `Integration/`: RESTful API test coverage
- `InMemoryDataStoreTests.cs`: low-level store tests

---

## 🛠️ API Usage (via Postman or cURL)

### ✅ Strings

```bash
# Set key with value
dcurl -X POST http://localhost:8080/strings \
     -H "Content-Type: application/json" \
     -d '{"key": "foo", "value": "bar"}'

# Get key
curl http://localhost:8080/strings/foo

# Increment key
curl -X POST http://localhost:8080/strings/foo/increment

# Delete key
curl -X DELETE http://localhost:8080/strings/foo
```

### ✅ ZSets

```bash
# Add to sorted set
curl -X POST http://localhost:8080/zsets/myzset \
     -H "Content-Type: application/json" \
     -d '{"score": 1, "member": "a"}'

# Get count
curl http://localhost:8080/zsets/myzset/count

# Get rank of member
curl http://localhost:8080/zsets/myzset/rank/a

# Get range
curl http://localhost:8080/zsets/myzset/range?start=0&stop=1
```

---

## 📋 Validation Rules

All keys and values must match this pattern:

```regex
^[a-zA-Z0-9-_]+$
```

Invalid input returns:

```json
{"error": "Invalid key or member. Only [a-zA-Z0-9-_] allowed."}
```

---

## 🧠 Concurrency

The in-memory store is thread-safe via:

- `ConcurrentDictionary` for key access
- `lock` statements for sorted set list operations

A specific integration test ensures multiple clients calling `INCR` still get consistent results.

---

## 📖 License

MIT License — free to use, modify and distribute.

---

## 👨‍💻 Author

Victor Del Razo + ChatGPT (OpenAI)

