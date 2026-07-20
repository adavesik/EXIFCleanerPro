# EXIFCleaner Pro

EXIFCleaner Pro is a local-first Windows desktop application for inspecting and removing privacy-sensitive photo metadata. It explains metadata in plain language, assigns an explainable privacy score, and verifies cleaned outputs by reading them again.

## Highlights

- Inspect JPEG and PNG metadata with human-friendly descriptions.
- Detect location, identity, device, timeline, editing-history, and embedded-text risks.
- Show an explainable privacy score and category findings.
- Open embedded GPS coordinates in OpenStreetMap only when requested.
- Create cleaned copies, write to a selected folder, or replace originals with backups.
- Compare metadata before and after cleaning and flag any sensitive findings that remain.
- Export a self-contained local HTML privacy report.
- Queue individual images or folders, including optional recursive discovery.

All inspection, cleaning, scoring, verification, and report generation runs locally. The map action is the only feature that opens an external website, and it does so only after an explicit click.

## Requirements

- Windows 10 version 1809 or later
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) for development

## Build and test

```powershell
dotnet restore --locked-mode
dotnet build EXIFCleanerPro.sln --no-restore
dotnet test EXIFCleanerPro.sln --no-build
```

Run the application during development with:

```powershell
dotnet run --project EXIFCleanerPro/EXIFCleanerPro.csproj
```

## Privacy score

The score is a deterministic explanation of the metadata rules matched by an image; it is not a guarantee of anonymity. Visual image content can still reveal people, locations, documents, or other identifying information even after metadata is removed.

The current implementation plan and future slices are documented in [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md).

## License

Licensed under the [Apache License 2.0](LICENSE.txt).
