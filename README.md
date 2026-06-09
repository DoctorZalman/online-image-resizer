# Online Image Resizer

A full-stack web application for resizing JPEG/PNG images in the browser.
Upload multiple files, set a scale percentage, and download the resized results.

---

## What it does

- Upload up to 10 JPEG/PNG images at once via drag & drop or file picker
- Set scale percentage (1–100%) and resize all uploaded files
- Real-time progress updates via SignalR
- Download individual files or all at once
- Light/dark theme with localStorage persistence
- Anonymous session tracking — no login required

---

## Tech Stack

### Frontend
- React 18 + TypeScript + Vite
- Tailwind CSS 3 (dark mode)
- Zustand — client state
- @microsoft/signalr — real-time progress
- Framer Motion — animations
- Sonner — toast notifications
- react-dropzone — file upload UI
- Vitest + Testing Library — unit tests

### Backend
- ASP.NET Core (.NET 10) Web API
- SixLabors.ImageSharp — image processing
- SignalR — real-time hub
- Per-user Channel<T> queue — sequential processing per user
- xUnit + Moq — unit tests

---

## Project Structure

```
image-resizer/
├── client/                        # React frontend
│   ├── src/
│   │   ├── api/                   # REST API functions
│   │   ├── components/            # UI components
│   │   │   ├── DropZone/
│   │   │   ├── FileCard/
│   │   │   ├── FileList/
│   │   │   ├── ResizeControls/
│   │   │   ├── DownloadList/
│   │   │   └── ThemeToggle/
│   │   ├── hooks/                 # SignalR hook
│   │   ├── store/                 # Zustand store
│   │   └── test/                  # Test setup
│   ├── Dockerfile
│   └── nginx.conf
├── server/                        # ASP.NET Core backend
│   ├── Controllers/               # HTTP endpoints
│   ├── Hubs/                      # SignalR hub
│   ├── Middleware/                # Session middleware
│   ├── Models/                    # ResizeJob, JobStatus
│   ├── Services/                  # Business logic
│   └── server.Tests/              # xUnit tests
├── docker-compose.yml
└── README.md
```

---

## Getting Started (Development)

### Prerequisites
- Node.js 20+ and Yarn
- .NET 10 SDK
- CSharpier (`dotnet tool install -g csharpier`)

### Run locally

**Terminal 1 — Backend:**
```bash
cd server
dotnet run
# runs on http://localhost:5000
```

**Terminal 2 — Frontend:**
```bash
cd client
yarn install
yarn dev
# runs on http://localhost:5173
```

Open `http://localhost:5173` in your browser.

---

## Live Demo

- **Frontend:** https://online-image-resizer.up.railway.app
- **Backend:** https://online-image-resizer-backend.up.railway.app

Deployed on [Railway](https://railway.app) using separate services for frontend (nginx) and backend (ASP.NET Core).

---

## Docker

```bash
docker compose up --build
```

Open `http://localhost:3000` in your browser.

To stop:
```bash
docker compose down
```

---

## Running Tests

**Backend (xUnit):**
```bash
cd server
dotnet test server.Tests/server.Tests.csproj
```

**Frontend (Vitest):**
```bash
cd client
yarn test
```

---

## Key Architecture Decisions

1. **Per-user Channel\<T\>** — sequential processing per user without blocking others
2. **202 Accepted + SignalR** — avoids HTTP timeout for large images
3. **Temp file cleanup** — `CleanupService` removes files older than 10 minutes
4. **Anonymous sessions** — cookie GUID avoids auth complexity while enabling per-user queuing
5. **UI blocking** — `isProcessing` flag prevents new uploads during active processing
