# eopgp-hub-manager — Quick Usage Guide

WARN: This readme is yet incomplete as more implementation is pushed, the main functionality is missing and will be updated in the coming days! 

A lightweight .NET host that wires your **Application** logic to **Kafka** consumers/producers and background workers. 

---

## Features
- Clean composition: Host (HubManager) + DI, Application, Infrastructure, BackgroundServices
- Kafka-ready producers/consumers (configurable topics & consumer groups)
- Structured logging via **Serilog**
- Config-first (`config/appsettings*.json`) with environment overrides

---

## Prerequisites
- **.NET 8 SDK**
- (Optional) **Kafka** broker reachable from your machine
- Config files in `src/HubManager/config/`

---

## Quick Start

```bash
# Restore & build
dotnet restore
dotnet build

# Run (adjust path if needed)
DOTNET_ENVIRONMENT=Development dotnet run --project src/HubManager
```

---

## Configuration

Create/update `src/HubManager/config/appsettings.json`.  
Put machine-local secrets in `appsettings.Development.local.json` (git-ignored).

```json
{
  "Serilog": { "MinimumLevel": "Information", "WriteTo": [{ "Name": "Console" }] },
  "Kafka": {
    "Brokers": [ "localhost:9092" ],
    "ConsumerGroup": "hub-manager",
    "Topics": { "Input": [ "ingest.s1" ], "Output": [ "processed.events" ] }
  },
  "Database": { "Provider": "Sqlite", "ConnectionString": "Data Source=hub.db" }
}
```

### Environment overrides

Use **double underscores** for nested keys:

```bash
# examples
Kafka__Brokers__0=kafka-1:39092
Kafka__ConsumerGroup=hub-manager-dev
Database__Provider=Postgres
Database__ConnectionString="Host=db;Port=5432;Database=hub;Username=app;Password=secret"
```

---

## Typical Workflow

1. Configure Kafka brokers, consumer group, and topics.
2. Start **Hub-Manager**; background services subscribe to input topics.
3. Application services process messages.
4. Results are persisted and/or published to output topics.

---

## Project Structure (high level)

```
src/
  Application/        # Interfaces, DTOs, services, CQRS/handlers
  Infrastructure/     # DB, Kafka clients, external adapters, configs
  BackgroundServices/ # Hosted services / orchestrators
  Core/               # Domain entities & enums
  HubManager/         # Composition root (Program.cs / DI / logging)
```

---

## Useful Commands

```bash
# Run with Production env
DOTNET_ENVIRONMENT=Production dotnet run --project src/HubManager

# (If EF is used) add & apply migrations from Infrastructure
cd src/Infrastructure
dotnet ef migrations add Init --startup-project ../HubManager
dotnet ef database update --startup-project ../HubManager
```

---

## Troubleshooting

- **No messages flowing**: verify `Kafka.Brokers` and topic names; ensure broker reachable.
- **Offset/consumption issues**: check `Kafka.ConsumerGroup`; confirm commits occur after processing.
- **Low visibility in logs**: set `Serilog.MinimumLevel` to `Debug` or `Verbose`.
- **Config not applied**: confirm `DOTNET_ENVIRONMENT` and file under `src/HubManager/config/`.

---

## License

AGPL-3.0 (see `LICENSE`).

```
# Save this file as README.md at the repository root when you fork this project
```
