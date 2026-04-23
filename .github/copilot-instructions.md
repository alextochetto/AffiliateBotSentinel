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

The single current endpoint is `POST /api/TrackBot/Track`, which accepts bot tracking data, validates the caller via a custom API key header, and writes the event to Azure Table Storage.

**Request body (`TrackBotPostDtq`):**
| Field | Type | Description |
|---|---|---|
| `Ip` | `string` | Client IP address |
| `UserAgent` | `string` | HTTP User-Agent string |
| `IsBot` | `bool` | Whether the caller was identified as a bot |
| `Path` | `string` | Requested URL path |
| `Timestamp` | `DateTime` | Event timestamp provided by the caller |
| `Gclid` | `string?` | Google Click ID from the `gclid` query string parameter (omitted when absent) |

**Stored entity (`BotTrackEntity`)** maps all of the above, plus `PartitionKey` (UTC date `yyyy-MM-dd`) and `RowKey` (new `Guid`).

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

## Deploying to Azure App Service (Free Tier)

The workflow `.github/workflows/deploy.yml` builds and deploys the API to Azure App Service on every push to `main`. It can also be triggered manually from GitHub Actions.

### One-time Azure setup

**1. Create an Azure App Service**
1. Go to [portal.azure.com](https://portal.azure.com) → **Create a resource → Web App**
2. Choose these settings:
   - **Publish:** Code
   - **Runtime stack:** .NET 10 (LTS)
   - **Operating System:** Linux
   - **Pricing plan:** Free F1

**2. Set the connection string**

In the App Service → **Settings → Environment variables → App settings**, add:

| Name | Value |
|---|---|
| `AzureTableStorage__ConnectionString` | your Azure Storage connection string |

> Azure App Service maps `__` (double underscore) to `:` in .NET configuration, so `AzureTableStorage__ConnectionString` is read as `AzureTableStorage:ConnectionString`.

**3. Get the publish profile**
1. In the App Service → **Overview**, click **Download publish profile**
2. Open the downloaded file and copy its entire contents

**4. Add GitHub Secrets**

In your GitHub repository → **Settings → Secrets and variables → Actions**, add:

| Secret name | Value |
|---|---|
| `AZURE_WEBAPP_NAME` | your App Service name (e.g. `affiliatebotsentinel`) |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | the full contents of the publish profile file |

**5. Push to `main`**

The workflow triggers automatically. Monitor progress in the **Actions** tab on GitHub.

### Free tier limitations (F1)

- **No "Always On"** — the app sleeps after ~20 min of inactivity; first request after sleep will be slow
- **60 CPU minutes/day** — sufficient for a low-traffic bot tracker
- **No custom domain SSL** — the app runs on `https://<app-name>.azurewebsites.net`

---

## WordPress Plugin Installation

Plugin location in this repo: `src/wordpress/plugins/csharp-bot-track-plugin/bot-track-plugin.php`

### Prerequisites
- WordPress site with admin access
- `AffiliateBotSentinel` API deployed and publicly accessible
- `AzureTableStorage:ConnectionString` configured on the API host

### Steps

**1. Set the correct API URL**

In `bot-track-plugin.php`, replace the URL if your API is not deployed at `https://botsentinel.azurewebsites.net`:
```php
wp_remote_post('https://<your-actual-domain>/api/TrackBot/Track', [
```

**2. Upload the plugin to WordPress**

Copy the entire folder `csharp-bot-track-plugin/` (containing `bot-track-plugin.php`) into your WordPress installation:
```
wp-content/plugins/csharp-bot-track-plugin/
```
You can do this via FTP, SFTP, or your hosting file manager.

**3. Activate the plugin**

1. Log in to **WordPress Admin**
2. Go to **Plugins → Installed Plugins**
3. Find **C# Tracker** and click **Activate**

**4. Verify it is working**

1. Visit any page on your WordPress site while logged out, **appending `?gclid=test123` to the URL**
2. Open [Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer/) (or the Azure Portal → Storage Account → Tables)
3. Open the `BotTracks` table — a new row should appear with your IP, User-Agent, path, `IsBot` flag, and the `Gclid` value

### How the plugin works

- **Only fires when a `gclid` query string parameter is present** — requests without it are silently ignored
- Captures the `gclid` value (Google Click ID) and sends it in the request body
- Non-blocking (10s timeout) — no impact on page speed
- Detects bot hints from the User-Agent string (`bot`, `crawl`, `spider`, `slurp`)
- Reads the real client IP from `HTTP_CF_CONNECTING_IP` first (Cloudflare), falling back to `REMOTE_ADDR`

---

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
