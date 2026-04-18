# AffiliateBotSentinel – Copilot Instructions

## Project Overview

AffiliateBotSentinel is a .NET 10 ASP.NET Core Web API that detects and tracks affiliate bots. It is designed to integrate with a WordPress site (see `src/wordpress/`, currently a placeholder).

## Build & Run

```bash
# From the solution root
dotnet build AffiliateBotSentinel.sln

# Run the API (http://localhost:5290)
cd src/backend/ApiBotSentinel
dotnet run --launch-profile http
```

The API exposes OpenAPI docs at `/openapi/v1.json` in Development mode.

## Architecture

```
src/
  backend/
    ApiBotSentinel/      # ASP.NET Core Web API (.NET 10)
      Controllers/       # API endpoints
      Dts/               # Data transfer objects (DTQ suffix)
      Services/          # Business logic services
  wordpress/             # WordPress integration (planned)
```

The single current endpoint is `POST /api/trackbot`, which accepts bot tracking data (IP + User-Agent), validates the caller via a custom API key header, and writes the event to Azure Table Storage.

## Configuration

`AzureTableStorage:ConnectionString` must be set in `appsettings.Development.json` (or environment/secrets). Use `UseDevelopmentStorage=true` for local development with [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite).

```json
{
  "AzureTableStorage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  }
}
```

The `BotTracks` table is created automatically on startup via `CreateIfNotExistsAsync`.

## Key Conventions

### No `var`
Never use `var`. All variable declarations must use explicit types.

### DTQ Naming
Request/query data transfer objects use the suffix **`Dtq`** (not `Dto`), stored in the `Dts/` folder. Example: `TrackBotPostDtq`.

### Azure Table Entities
Table entities live in `Dts/` and implement `ITableEntity`. `PartitionKey` is the UTC date (`yyyy-MM-dd`); `RowKey` is a new `Guid` per event.

### Service Registration
Services are registered as **singletons** in `Program.cs`. `TableClient` (from `Azure.Data.Tables`) is injected directly as a singleton — it is thread-safe and designed for reuse.

### API Key Authentication
Incoming requests are authenticated by checking the `trackbot-api-key` request header inside a `CheckHeaderApiKey()` private method on the controller. This pattern is used instead of middleware or a filter.

### Controller Structure
Controllers live in `Controllers/`, inherit `ControllerBase`, and use attribute routing (`[Route("api/[controller]")]`). Action methods that call services are `async Task<IActionResult>`.

### Nullable & Implicit Usings
Both `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>` are active. All new code must be null-safe.
