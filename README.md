# ?? LiveQuiz - Real-Time Interactive Quiz Platform

A **world-class**, feature-rich live quiz platform built with ASP.NET Core 10 Razor Pages. Host engaging quiz games similar to Kahoot with real-time participant tracking, beautiful UI, and advanced analytics.

![.NET Version](https://img.shields.io/badge/.NET-10-purple)
![License](https://img.shields.io/badge/license-MIT-blue)
![Build](https://img.shields.io/badge/build-passing-brightgreen)

---

## ? Features Overview

### ?? **For Players**
- ? **No Account Required** - Just enter a join code and nickname
- ?? **Keyboard Shortcuts** - Press 1-4 for lightning-fast answers
- ?? **Points-Based Scoring** - 1000 base points + up to 500 speed bonus
- ?? **Performance Stats** - Detailed breakdown with achievement badges
- ?? **Real-Time Countdown** - Visual timer with pulsing animations
- ?? **Live Leaderboards** - See rankings update in real-time

### ????? **For Hosts**
- ?? **Quiz Management** - Create, edit, delete, and clone quizzes
- ?? **Categories & Tagging** - Organize quizzes by subject
- ?? **Shuffle Options** - Randomize questions and answers
- ?? **Public/Private Quizzes** - Share with community or keep private
- ?? **CSV Export** - Download leaderboards for record-keeping
- ?? **Advanced Analytics** - Chart.js visualizations with difficulty analysis

---

## ?? Elite Features (Version 2.0)

### ?? **RESTful API**
- `GET /api/quizzes/public` - Browse with pagination
- `GET /api/quizzes/{id}` - Quiz details
- `GET /api/games/{id}/leaderboard` - Live leaderboard
- `POST /api/games/join` - Programmatic join
- Full API docs at `/ApiDocs`

### ?? **Real-Time Updates (SignalR)**
- Live participant join notifications
- Instant response counts
- WebSocket-based (no polling!)
- Audio notifications

### ? **Performance**
- In-memory caching
- Smart cache invalidation
- Optimized queries

### ? **WCAG AAA Accessibility**
- High contrast mode
- Reduced motion support
- Keyboard navigation
- Screen reader friendly

### ?? **PWA Support**
- Install as app
- Offline-ready
- App shortcuts

---

## ??? Technology Stack

- **Framework**: ASP.NET Core 10 Razor Pages
- **Language**: C# 14.0
- **Database**: SQL Server LocalDB + EF Core 10
- **Real-Time**: SignalR
- **API**: ASP.NET Core Controllers
- **Frontend**: Bootstrap 5 + Custom CSS
- **Charts**: Chart.js

---

## ?? Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server LocalDB

### Installation

```bash
git clone https://github.com/yourusername/ITEC275LiveQuiz.git
cd ITEC275LiveQuiz
dotnet restore
dotnet run --project ITEC275LiveQuiz
```

Access at `https://localhost:7XXX`
- Demo account: `demo` / `Password123!`
- 15-question demo quiz included

---

## ?? Quick Usage

### Players
1. Click "Join Game"
2. Enter 6-character code
3. Choose nickname
4. Answer fast for more points!

### Hosts
1. Register/Login
2. Create quiz with questions
3. Click "Start Game"
4. Share join code
5. Control flow & view analytics

---

## ?? Scoring System

```
Points = 1000 (if correct) + SpeedBonus (up to 500)
SpeedBonus = 500 × (1 - timeUsed/timeLimit)
```

Example: Correct answer in 6/30 seconds = 1000 + 400 = **1400 points**

---

## ?? Project Structure

```
ITEC275LiveQuiz/
??? Controllers/      # API endpoints
??? Hubs/            # SignalR real-time
??? Models/          # Entities (User, Quiz, Game, etc.)
??? Pages/           # Razor Pages
?   ??? Account/     # Auth
?   ??? Admin/       # Stats
?   ??? Host/        # Game hosting
?   ??? Play/        # Player experience
?   ??? Quizzes/     # Quiz CRUD
??? Services/        # Business logic
??? Seed/            # Demo data
??? wwwroot/         # Static files + PWA manifest
```

---

## ?? Roadmap

- [ ] Image support in questions
- [ ] Team-based mode
- [ ] Global leaderboards
- [ ] OAuth login
- [ ] Quiz templates
- [ ] Redis caching

---

## ?? Educational Highlights

Demonstrates:
- Full-stack ASP.NET Core development
- EF Core & database design
- SignalR real-time communication
- RESTful API design
- Modern UI/UX
- Accessibility (WCAG AAA)
- PWA concepts
- Security best practices

---

## ?? License

MIT License - See [LICENSE](LICENSE)

---

## ????? Credits

**ITEC 275 Final Project** | v2.0 Elite Edition
- Inspired by Kahoot!, Quizizz, Mentimeter
- Built with ?? using ASP.NET Core 10

---

**Built with ?? for education**
