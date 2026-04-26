# OrbitPM - Project Matching System

A secure, role-based platform for managing academic project proposals and supervisor matching.

## 🚀 Features

- **Triple-Role Dashboard**: Tailored experiences for Students, Supervisors, and Module Leaders.
- **Critical Anonymity Layer**: Student identities are decoupled from project proposals to ensure unbiased matching by supervisors.
- **Smart Matching**: Automated and manual matching of project proposals based on research areas.
- **Research Categorization**: Centralized management of research areas and technical stacks.
- **Automated Workflows**: Real-time status tracking from "Pending" to "Matched".

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Auth**: Cookie-based Authentication
- **Frontend**: Razor Views, Vanilla CSS, Bootstrap

## 📥 Quick Start

1. **Prerequisites**: Install [.NET SDK 8.0+](https://dotnet.microsoft.com/download) and SQL Server.
2. **Clone & Configure**:
   ```bash
   git clone https://github.com/5h3ld0rr/OrbitPM.git
   cd OrbitPM
   ```
3. **Setup Connection String**: Update `appsettings.json` (or create `appsettings.local.json`) with your `DefaultConnection`.
4. **Run Application**:
   ```bash
   dotnet restore
   dotnet run --project OrbitPM
   ```
   *Note: Database migrations run automatically on startup.*

## 🏗️ Architecture

- `OrbitPM/Models`: Domain entities including `MatchRecord`, `ProjectProposal`, and `ResearchArea`.
- `OrbitPM/Controllers`: Logic for Student, Supervisor, and Module Leader dashboards.
- `OrbitPM/Data`: Entity Framework `ApplicationDbContext` and migrations.

## ⚖️ License

MIT
