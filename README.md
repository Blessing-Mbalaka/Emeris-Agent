# Emeris Academic Success Agent

ASP.NET Core MVC student-support agent for the Emeris Pretoria Campus qualifier. The app is structured with clean architecture layers and includes:

- MVC frontend styled with Tailwind CSS
- Modular tool-based agent orchestration
- Gemini integration for planning, response synthesis, and embeddings
- PDF upload for timetable, assessment schedule, and study guides
- Heuristic table extraction for Word-to-PDF timetable and schedule layouts
- RAG search across uploaded study materials
- Reminder creation and current-date tooling

## Projects

- `EmerisAcademicSuccess.Web`: MVC UI, controllers, session Gemini key handling
- `EmerisAcademicSuccess.Application`: contracts and application models
- `EmerisAcademicSuccess.Domain`: entities and enums
- `EmerisAcademicSuccess.Infrastructure`: EF Core, Gemini client, PDF parsing, RAG, tools
- `EmerisAcademicSuccess.Tests`: test project for focused logic

## Run locally

1. Build Tailwind CSS:

```powershell
cd .\EmerisAcademicSuccess.Web
npm install
npm run build:css
```

2. Start the MVC app:

```powershell
cd ..
dotnet run --project .\EmerisAcademicSuccess.Web
```

3. Open the home page, add your Gemini API key in the Settings panel, then upload PDFs.

## Configuration

- `Persistence:Provider`
  - `InMemory`: zero-setup demo mode
  - `Postgres`: persistent mode using `ConnectionStrings:Postgres`
- `Gemini:ApiKey`
  - keep empty to use the session form in the UI
  - or set it in configuration for a fixed environment setup

## Notes

- The current PDF extraction expects PDF uploads.
- Table extraction is heuristic-based, which is practical for converted Word tables but not guaranteed for every layout.
- When no Gemini key is available, the app falls back to heuristic planning and keyword search.