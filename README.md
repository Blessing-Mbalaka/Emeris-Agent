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
  - `SqlServer`: persistent SQL Server mode using `ConnectionStrings:SqlServer`
  - `Postgres`: persistent mode using `ConnectionStrings:Postgres`
- `Gemini:ApiKey`
  - keep empty to use the session form in the UI
  - or set it in configuration for a fixed environment setup

  ## SQL Server setup

  Set `Persistence:Provider` to `SqlServer` in `EmerisAcademicSuccess.Web/appsettings.Development.json`, then update `ConnectionStrings:SqlServer` for your SQL Server instance. The development default targets LocalDB.

  Create the database and tables with the following script in SQL Server Management Studio or Azure Data Studio:

  ```sql
  CREATE DATABASE EmerisAcademicSuccess;
  GO

  USE EmerisAcademicSuccess;
  GO

  CREATE TABLE Documents (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    FileName NVARCHAR(260) NOT NULL,
    StoredFileName NVARCHAR(260) NOT NULL,
    ContentType NVARCHAR(128) NOT NULL,
    Category INT NOT NULL,
    UploadedAtUtc DATETIME2 NOT NULL,
    ExtractedText NVARCHAR(MAX) NOT NULL,
    Summary NVARCHAR(4000) NOT NULL
  );

  CREATE TABLE DocumentChunks (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    DocumentAssetId UNIQUEIDENTIFIER NOT NULL,
    Sequence INT NOT NULL,
    Content NVARCHAR(4000) NOT NULL,
    EmbeddingJson NVARCHAR(MAX) NULL,
    CONSTRAINT FK_DocumentChunks_Documents FOREIGN KEY (DocumentAssetId)
      REFERENCES Documents(Id) ON DELETE CASCADE
  );
  CREATE INDEX IX_DocumentChunks_DocumentAssetId_Sequence ON DocumentChunks (DocumentAssetId, Sequence);

  CREATE TABLE ScheduleEntries (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    DocumentAssetId UNIQUEIDENTIFIER NOT NULL,
    EntryType INT NOT NULL,
    Title NVARCHAR(256) NOT NULL,
    ModuleCode NVARCHAR(64) NULL,
    DayOfWeek INT NULL,
    OccursOn DATE NULL,
    StartTime TIME NULL,
    EndTime TIME NULL,
    Venue NVARCHAR(256) NULL,
    Notes NVARCHAR(1000) NULL,
    CONSTRAINT FK_ScheduleEntries_Documents FOREIGN KEY (DocumentAssetId)
      REFERENCES Documents(Id) ON DELETE CASCADE
  );

  CREATE TABLE Reminders (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Notes NVARCHAR(1000) NULL,
    DueAtLocal DATETIME2 NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    IsCompleted BIT NOT NULL
  );

  CREATE TABLE Conversations (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    UpdatedAtUtc DATETIME2 NOT NULL
  );

  CREATE TABLE ConversationMessages (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ConversationId UNIQUEIDENTIFIER NOT NULL,
    Role NVARCHAR(32) NOT NULL,
    Content NVARCHAR(8000) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_ConversationMessages_Conversations FOREIGN KEY (ConversationId)
      REFERENCES Conversations(Id) ON DELETE CASCADE
  );
  CREATE INDEX IX_ConversationMessages_ConversationId_CreatedAtUtc
    ON ConversationMessages (ConversationId, CreatedAtUtc);
  ```

  Chunks, their embedding JSON, document metadata, and each browser-session conversation are persisted in these tables. SQL Server schema creation is intentionally manual so the app has no production schema-creation permissions.

## Notes

- The current PDF extraction expects PDF uploads.
- Table extraction is heuristic-based, which is practical for converted Word tables but not guaranteed for every layout.
- When no Gemini key is available, the app falls back to heuristic planning and keyword search.