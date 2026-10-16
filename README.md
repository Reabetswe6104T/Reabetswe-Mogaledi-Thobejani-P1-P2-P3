# RaceDay Event Management System

## About This Project

This is a full-stack event management system called RaceDay, built for the South African road running, walking, and cycling community. The project was completed across two parts for PROG6212.

Part 1 covered system planning, database design, and SQL scripting.

Part 2 covers the RESTful API built with ASP.NET Core.

## Roles

**Participant** - Can browse events, register for events, view their entry history, and track their performance results.

**Organiser** - Can create and manage events, view participant entries, and submit race results.

## Database Design

The database has 6 entities:

| Entity | Purpose |
|--------|---------|
| User | Authentication and login details |
| Participant | Personal details for race participants |
| Organiser | Event organisers and their organisations |
| Event | Race and event details |
| Entry | Participant registrations for events |
| Result | Race results for participants |

## What Was Built in Part 2

A complete RESTful API using ASP.NET Core, Entity Framework Core, and SQL Server.

### API Endpoints

| Resource | Count | What It Does |
|----------|-------|--------------|
| Auth | 2 | Register and Login with JWT |
| Participants | 4 | Profile management |
| Organisers | 3 | Profile and event listings |
| Events | 6 | CRUD, filter, upcoming |
| Categories | 5 | Event categories |
| Entries | 6 | Enrolments, cancellations, payments |
| Results | 6 | Submit results, leaderboards |

Total: approximately 32 endpoints.

### Authentication

Users register and log in with email and password. Passwords are hashed using BCrypt before storage. On login, the API returns a JWT token containing the user's ID and role.

Role-based access is enforced so that Organisers and Participants can only access their own functionality.

### Unit Testing

8 unit tests were written using xUnit, Moq, and FluentAssertions. All 8 tests pass.

### Technologies Used

- ASP.NET Core
- Entity Framework Core (Code-First)
- SQL Server (LocalDB)
- JWT for authentication
- BCrypt.Net for password hashing
- Swagger for API documentation
- xUnit, Moq, and FluentAssertions for testing
- GitHub Actions for CI/CD

## Project Structure

Part2/
└── RaceDayAPI/
    ├── Controllers/
    ├── Data/
    ├── DTOs/
    ├── Migrations/
    ├── Models/
    ├── Services/
    ├── Program.cs
    ├── appsettings.json
    └── RaceDayAPI.csproj

## Video Walkthrough

Watch the walkthrough video here:

[Part 2 Walkthrough Video](https://youtu.be/-gFsK_VTYMM)

## Author

Reabetswe Mogaledi Thobejani
ST10486001
PROG6212
October 2026

---

Copyright The Independent Institute of Education (Pty) Ltd 2026
