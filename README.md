# Tavus.io Minimal API Prototype

This repo contains a .NET 8 minimal API that reproduces the Tavus quick start flow: it creates an interviewer persona using the full pipeline, then immediately launches a conversation and returns the `conversation_url` so you can join.

## Getting started

1. **Install the .NET 8 SDK** (already included in this workspace, download from <https://dotnet.microsoft.com/> if needed).
2. **Configure your Tavus API key** using either option:
   * Export an environment variable before running the app:
     ```bash
     export Tavus__ApiKey="YOUR_API_KEY"
     ```
   * Or edit `TavusPrototype/appsettings.json` and replace the `SET-IN-SECRETS` placeholder.

> Tip: every other setting (base URL, persona defaults, replica ID) can also be overridden through the `Tavus:*` configuration keys.

## Run the API

```bash
cd TavusPrototype
dotnet run
```

Once the app is running you can browse to `https://localhost:7086/swagger` (or the HTTP port shown in the console) to try the endpoint interactively.

## Start a conversation via cURL

```bash
curl --request POST \
  --url https://localhost:7086/tavus/conversations \
  --header 'Content-Type: application/json' \
  --data '{
    "conversationName": "Interview User"
  }'
```

The response contains the `conversationUrl` along with the IDs returned by Tavus so you can join immediately.
