<div align="center">

<img src="wwwroot/images/brand/ironcore-logo.svg" alt="IRONCORE" width="280">

# IRONCORE

**Gym & Fitness Centre Management System**

*Train harder. Track smarter.*

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-Code--First-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap%205-7952B3?style=flat-square&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)

<img src="https://img.shields.io/badge/Status-Completed-2ECC71?style=flat-square" alt="status">


</div>

---

## About

IronCore is a full-stack gym management platform built for **members, trainers, and administrators**. Members subscribe to a plan, book classes, follow a workout and diet plan, and track their progress over time. Trainers manage their classes and attendance. Administrators run the business end-to-end — plans, subscriptions, payments, and reporting.

Delivered as a complete ASP.NET Core MVC application with a Code-First database, role-based access, and an AI assistant built into the member experience.

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Data Model](#data-model)
- [Business Rules](#business-rules)
- [AI Assistant](#ai-assistant)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Team](#team)

## Features

| Area | Highlights |
|---|---|
| **Public** | Home, Membership Plans, Class Schedule, Trainer Directory & Profiles, Login/Sign-up |
| **Member** | Dashboard, subscription status, class booking & cancellation, workout & diet plans, progress tracking with charts, gym check-in, profile |
| **Trainer** | Dashboard, assigned classes, rosters, attendance management |
| **Admin** | Full CRUD on plans, classes, trainers, members, subscriptions & payments; search, filter, sort, pagination; reports |

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core MVC (C#) |
| ORM | Entity Framework Core — Code-First |
| Database | Microsoft SQL Server |
| Auth | ASP.NET Core Identity, role-based authorization |
| Frontend | Razor Views, Bootstrap 5, custom CSS |
| Charts | Chart.js |
| Validation | Data Annotations + `ModelState` + client-side validation |
| AI | HTTP-based model integration (chat over app data) |

## Architecture

```
IronCore
├── Controllers/      Account, Admin, Classes, Home, Member, Plans, Trainer(s)
├── Models/            Domain & view models
├── Views/             One per controller area, plus Shared/_Layout
├── wwwroot/           css, js, images, lib
├── Program.cs
├── GymMvc.csproj
└── appsettings.json
```

Single shared layout, partial views and tag helpers throughout — no Razor Pages, no separate front-end API.

## Data Model

```
ApplicationUser
 ├── Member
 │    ├── Subscription ── MembershipPlan
 │    │        └── Payment
 │    ├── ClassBooking ── FitnessClass ── Trainer
 │    ├── Attendance
 │    ├── CheckIn
 │    ├── ProgressLog
 │    ├── WorkoutPlan
 │    └── DietPlan
 └── Trainer
```

**Key relationships**
- Trainer → FitnessClass: one-to-many
- Member → Subscription, CheckIn, ProgressLog, WorkoutPlan, DietPlan: one-to-many
- MembershipPlan → Subscription: one-to-many
- Subscription → Payment: one-to-many
- Member ↔ FitnessClass: many-to-many via `ClassBooking`

Six-plus related tables, seeded with realistic data so the app looks alive from the first run.

## Business Rules

**Class capacity**
```
Available Spots = Maximum Capacity − Current Bookings
```
A class can't be booked once it hits capacity; cancelling a booking frees the spot immediately.

**Check-in eligibility**
```
Check-In Request → Active Subscription? → Yes: Check in | No: Reject
```

## AI Assistant

A chat assistant is built into the Member Portal. It reads relevant data from the database — plans, schedule, member goals — and answers using that context, giving members real, grounded guidance instead of generic responses.

- Coach chat for training and nutrition questions, with a visible "not medical advice" notice
- Class recommendations based on the member's goal and available days
- AI-assisted weekly workout plans for trainers to review and assign
- Progress summaries generated from weight logs and attendance history

## Getting Started

**Requirements:** .NET SDK · SQL Server · SSMS · Visual Studio or VS Code · Git

```bash
git clone https://github.com/mohamedxk9tb/IronCore-Gym-Management-System.git
cd IronCore-Gym-Management-System
dotnet restore
dotnet run
```

## Project Structure

```
GymMvc/
├── Controllers/
├── Models/
├── Views/
│   ├── Account/  Admin/  Home/  Member/  Trainer/  Trainers/  Shared/
├── wwwroot/
│   ├── css/  js/  images/  lib/
├── Program.cs
├── GymMvc.csproj
└── appsettings.json
```

## Team

| Member | Responsibility |
|---|---|
| [Mohamed Kotb](https://github.com/mohamedxk9tb) | Team Lead — core system, shared UI, database integration, Identity & member features |
| [Marwan Abulazm](https://github.com/MarwanAbulazm) | Membership, plans, subscriptions & payments |
| [Ziad Salah](https://github.com/z0800salah-stack) | Trainers, classes, booking, attendance & AI |

---

<div align="center">

**IronCore** · Train harder. Track smarter.

Licensed for academic and educational use — see [LICENSE](LICENSE).


</div>
