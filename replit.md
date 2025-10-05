# Animal Collector / Critter Wrangler

## Overview

Animal Collector (also branded as "Critter Wrangler") is a mobile-first web application built with Blazor WebAssembly and ASP.NET Core, designed for discovering and collecting digital "critters" (animals) via NFC scanning of physical 3D printed figures. Each figure contains an NFC tag linking to a unique URL, allowing users to collect animals. The application supports a collection-based game experience, offering features like achievement tracking, leaderboards, and a token economy. Its primary business vision is to create an engaging, interactive collecting game with a focus on mobile accessibility and child safety.

## User Preferences

Preferred communication style: Simple, everyday language.

## System Architecture

### Frontend Architecture

The application uses Blazor WebAssembly (NET 8.0) for a responsive, component-based, mobile-first UI. It leverages Razor components with scoped CSS and 'Comic Neue' typography for a playful user experience. Desktop users are redirected with a QR code to the mobile site.

### Backend Architecture

The backend is an ASP.NET Core 8.0 web server that hosts both the API and the compiled Blazor WebAssembly client. It provides RESTful API endpoints, configured with Swagger/OpenAPI for documentation, and integrates health checks for monitoring.

### Data Storage Architecture

PostgreSQL is used as the relational database, managed with Entity Framework Core 9.0 using a code-first approach with migrations. Health monitoring for the database is integrated via AspNetCore.HealthChecks.NpgSql.

### Shared Code Architecture

A separate `AnimalCollector.Shared` class library project targets .NET 8.0 to share models, DTOs, and common logic between the client and server, ensuring type safety and reducing code duplication.

### Authentication and Authorization

The system implements ASP.NET Core Authorization, integrated across both client and server for secure access control. It supports external authentication (Google, Apple) alongside email/password, with features like account linking and nickname auto-generation. Session-based authentication is used with a 30-day cookie expiration.

### Child Safety Architecture

Child safety features include email-only registration with valid email address requirements and a `ContentFilter` service for profanity filtering in usernames and nicknames. Public-facing displays use unique nicknames instead of email addresses.

### Feature Specifications

*   **NFC-Based Discovery**: Animals are discovered by scanning NFC tags with unique tokens, leading to a dedicated animal view page.
*   **Collection & Achievements**: Users collect animals, track progress, and unlock achievements with celebratory modal popups. Achievement types include First Discovery, Collector, Hunter, Legendary Hunter, Cryptozoologist, and Explorer (100% completion).
*   **Leaderboard**: A "Top Explorers" leaderboard displays the top 5 users by collection count with rank medals.
*   **Token Economy**: A token tracking system is integrated into user accounts, with a `WheelSpin` component for potential future reward mechanics.
*   **Nickname Privacy**: Users have unique nicknames (3-20 chars, alphanumeric/spaces/underscores/hyphens) for public display, with validation and profanity filtering.
*   **Mobile-First UX**: Designed primarily for mobile and tablet devices; desktops see a QR code redirect.

## External Dependencies

### Third-Party Services

*   **QR Code Generation**: `qrserver.com` API
*   **Google Fonts**: `Comic Neue` font family

### Database

*   **PostgreSQL**: Primary relational database, compatible with Npgsql 8.0.3 and EF Core 9.0.4. Health checks are configured via AspNetCore.HealthChecks.NpgSql.

### NuGet Packages

*   **Server**:
    *   AspNetCore.HealthChecks.NpgSql 8.0.2
    *   Microsoft.AspNetCore.Components.WebAssembly.Server 9.0.9
    *   Microsoft.AspNetCore.OpenApi 8.0.5
    *   Microsoft.EntityFrameworkCore.Design 9.0.9
    *   Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
    *   Swashbuckle.AspNetCore 6.4.0
    *   Humanizer.Core 2.14.1
    *   Microsoft.AspNetCore.Authentication.Google
    *   AspNet.Security.OAuth.Apple
*   **Client**:
    *   Microsoft.AspNetCore.Components.WebAssembly 8.0.5
    *   Microsoft.AspNetCore.Components.Web 8.0.5
    *   Microsoft.AspNetCore.Components.Forms 8.0.5
    *   Microsoft.JSInterop.WebAssembly

### Runtime Dependencies

*   .NET 8.0 Runtime
*   ASP.NET Core 8.0 Runtime
*   WebAssembly runtime (dotnet.native.wasm, version 8.0.5)