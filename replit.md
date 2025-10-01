# Animal Collector / Critter Wrangler

## Overview

Animal Collector (also branded as "Critter Wrangler") is a mobile-first web application built with Blazor WebAssembly and ASP.NET Core. The application is specifically designed for mobile and tablet devices, with desktop users seeing a friendly QR code-based redirect message. The system follows a three-tier architecture with separate Client, Server, and Shared projects.

The application is a collection-based game focused on discovering and collecting "critters" (animals) through NFC scanning. Each 3D printed animal figure contains an NFC tag with a unique token that links to https://www.nfcritters.com/{token}, allowing users to discover and collect animals by scanning physical objects.

## User Preferences

Preferred communication style: Simple, everyday language.

## Recent Changes (October 1, 2025)

### Achievement Celebration System (Latest)
- **Modal Celebration Popups**: Child-friendly achievement unlocks with animations and confetti effects
- **Six Achievement Types**: First Discovery (1 collected), Collector (5), Hunter (25), Legendary Hunter (1 legendary), Cryptozoologist (all legendary), Explorer (100% completion)
- **Stats API Endpoint**: GET /api/animals/stats returns user progress (collectedCount, legendaryCount, totalLegendaryCount, completionRate)
- **Smart Detection**: Compares pre/post collection stats to identify newly unlocked achievements
- **CSS Animations**: Bounce, fade, scale, and sparkle effects for exciting celebrations
- **Case-Insensitive Rarity**: Fixed legendary achievement triggers with normalized rarity comparisons
- **Non-Intrusive Flow**: Users click "Continue Adventure!" to proceed after celebration
- **Architect-Approved**: Production-ready with recommendations for future enhancements (write-time normalization, automated regression tests)

### Nickname Privacy System
- **Privacy Protection**: Users now have unique nicknames for public display (3-20 chars, alphanumeric/spaces/underscores/hyphens)
- **Email Privacy**: All public-facing displays (Profile header, leaderboards, achievements) show nicknames instead of email addresses
- **Nickname Validation**: Comprehensive validation with profanity filtering, whitespace normalization, and case-insensitive uniqueness enforcement
- **Race Condition Handling**: DbUpdateException catching prevents duplicate nicknames under concurrent registration
- **UI Updates**: All auth forms (Scan, Profile, Collection, AnimalView) include nickname input field
- **Database**: Added nullable nickname column with unique constraint; existing users default to "Explorer" in leaderboard
- **Architect-Approved**: Production-ready implementation with recommendations for future enhancements (DB functional indexes, enhanced profanity detection)

### Child Safety Features
- **Email-Only Usernames**: Registration now requires valid email addresses as usernames
- **Profanity Filtering**: ContentFilter service blocks inappropriate words in usernames and nicknames
- All auth forms updated to use email input type with "Email Address" placeholder
- Validation messages tailored for children ("Please use a valid email address")

### Leaderboard System
- Added "Top Explorers" leaderboard on Profile page
- Shows top 5 users by collection count with rank medals (🥇🥈🥉)
- Current user's entry highlighted with purple border
- New API endpoint: GET /api/animals/leaderboard

### Token-Based Animal Discovery System
- Added unique token system for NFC-enabled animal discovery
- Each animal now has a unique token (e.g., LN001, EL002, PG003)
- Database schema updated with token column and unique index
- New API endpoint: GET /api/animals/token/{token} to fetch animals by token
- Created dedicated animal view page at /animal/{token} route
- Scan page now navigates to animal view page instead of showing modal popup
- Full authentication integration on animal view page (sign in to collect)

### Mobile & Desktop UX
- Desktop users see friendly message with QR code linking to nfcritters.com
- Tablets (< 1024px width) can access the full application
- Only desktops (≥ 1024px width) are blocked with redirect message

## System Architecture

### Frontend Architecture (Blazor WebAssembly)

**Problem Addressed**: Need for a responsive, mobile-first web application with rich client-side interactivity.

**Solution**: Blazor WebAssembly (NET 8.0) running entirely in the browser via WebAssembly.

**Key Design Decisions**:
- **Mobile-First Design**: The application explicitly targets mobile and tablet devices, showing a QR code redirect for desktop users
- **Component-Based UI**: Uses Razor components with scoped CSS for modular, maintainable UI code
- **Static Web Assets**: Employs ASP.NET Core's static web assets system for efficient resource delivery
- **Comic Sans Typography**: Custom font stack using 'Comic Neue' and fallback comic-style fonts, suggesting a playful, casual user experience

**Pros**:
- No server round-trips for UI interactions
- Offline-capable once loaded
- Cross-platform mobile support
- Rich client-side interactivity

**Cons**:
- Larger initial download size
- WebAssembly browser compatibility requirements

### Backend Architecture (ASP.NET Core)

**Problem Addressed**: Need for API services, static file hosting, and server-side logic.

**Solution**: ASP.NET Core 8.0 web server with API endpoints and static file serving.

**Key Design Decisions**:
- **Unified Hosting**: Server project hosts both the API and the compiled Blazor WebAssembly client
- **RESTful API Design**: Configured with Swagger/OpenAPI for API documentation and testing
- **Health Checks**: Integrated health check endpoints for monitoring (specifically PostgreSQL health checks)
- **Development-Optimized**: Configured with launchSettings for easy local development with Swagger UI

**Pros**:
- Single deployment artifact
- Simplified hosting model
- Built-in API documentation
- Easy local development

**Cons**:
- Server resources needed for both static files and API

### Data Storage Architecture

**Problem Addressed**: Need for persistent, relational data storage with modern ORM tooling.

**Solution**: PostgreSQL database with Entity Framework Core 9.0.

**Key Design Decisions**:
- **PostgreSQL**: Chosen as the relational database (via Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4)
- **EF Core**: Code-first approach with migrations for schema management
- **Health Monitoring**: AspNetCore.HealthChecks.NpgSql integration for database availability monitoring
- **Design Tools**: Microsoft.EntityFrameworkCore.Design for migration tooling

**Alternatives Considered**: The architecture supports potential future migration to other providers supported by Entity Framework Core.

**Pros**:
- Robust, ACID-compliant data storage
- Rich querying capabilities via LINQ
- Strong typing and compile-time checking
- Cross-platform compatibility

**Cons**:
- Requires PostgreSQL infrastructure
- Additional complexity compared to NoSQL alternatives

### Shared Code Architecture

**Problem Addressed**: Need to share models, DTOs, and common logic between client and server.

**Solution**: Separate Shared class library project (AnimalCollector.Shared).

**Key Design Decisions**:
- **Shared Models**: Common data structures accessible to both client and server
- **NET Standard Targeting**: Uses .NET 8.0 for maximum compatibility
- **Zero Dependencies**: Pure .NET implementation with no external packages

**Pros**:
- Eliminates code duplication
- Type safety across client-server boundary
- Single source of truth for data contracts

**Cons**:
- Client bundle includes all shared code (even server-only models if not careful)

### Authentication and Authorization

**Problem Addressed**: Need for secure access control.

**Solution**: ASP.NET Core Authorization framework integrated into both client and server.

**Key Design Decisions**:
- **Built-in Authorization**: Uses Microsoft.AspNetCore.Authorization (8.0.5)
- **WebAssembly Integration**: Client-side authorization components for UI-level security
- **Stateless Design**: Likely token-based given the WebAssembly architecture

**Note**: Specific authentication mechanism (JWT, OAuth, etc.) not evident from provided files.

### Child Safety Architecture

**Problem Addressed**: Need to ensure appropriate usernames and prevent inappropriate language in a children's app.

**Solution**: ContentFilter service with email validation and profanity filtering.

**Key Design Decisions**:
- **Email-Only Registration**: Only valid email addresses accepted as usernames
  - Provides accountability and reduces anonymous inappropriate usernames
  - Uses regex validation for email format
- **Profanity Filter**: Maintains a list of inappropriate words
  - Case-insensitive matching
  - Checks entire username string for bad word patterns
  - Returns friendly error messages appropriate for children
- **Validation Order**: Email format → Profanity check → Duplicate check
  - Ensures all safety checks happen before database operations

**Implementation**:
- `Server/Services/ContentFilter.cs`: Static class with `IsValidEmail()` and `ContainsBadWords()` methods
- `AuthController.cs`: Validates on registration (login doesn't need revalidation)
- All auth forms use `type="email"` inputs for browser-level validation too

**Pros**:
- Simple, maintainable solution
- No external API dependencies
- Fast server-side validation
- Browser-level email validation as first line of defense

**Cons**:
- Static word list requires manual updates
- Simple pattern matching (could be more sophisticated)

**Future Enhancements**:
- Could integrate external content moderation API for more robust filtering
- Could add username change audit logging

### Build and Deployment Strategy

**Problem Addressed**: Need for optimized production builds and efficient asset delivery.

**Solution**: Multi-configuration build system with compression and optimization.

**Key Design Decisions**:
- **Compressed Assets**: Gzip compression for WebAssembly files and resources
- **Static Web Assets Pipeline**: Automatic bundling and optimization
- **Scoped CSS**: Component-scoped stylesheets compiled into bundles
- **Release Optimization**: Separate Debug/Release configurations with different optimization levels

**Pros**:
- Reduced bandwidth usage
- Faster load times
- Cache-friendly asset management

## External Dependencies

### Third-Party Services
- **QR Code Generation**: qrserver.com API for generating QR codes (used in mobile redirect UI)
- **Google Fonts**: Comic Neue font family hosted via Google Fonts CDN

### Database
- **PostgreSQL**: Primary relational database
  - Version: Compatible with Npgsql 8.0.3 and EF Core 9.0.4
  - Connection: Health checks configured via AspNetCore.HealthChecks.NpgSql 8.0.2

### NuGet Packages

**Server**:
- AspNetCore.HealthChecks.NpgSql 8.0.2
- Microsoft.AspNetCore.Components.WebAssembly.Server 9.0.9
- Microsoft.AspNetCore.OpenApi 8.0.5
- Microsoft.EntityFrameworkCore.Design 9.0.9
- Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
- Swashbuckle.AspNetCore 6.4.0
- Humanizer.Core 2.14.1

**Client**:
- Microsoft.AspNetCore.Components.WebAssembly 8.0.5
- Microsoft.AspNetCore.Components.Web 8.0.5
- Microsoft.AspNetCore.Components.Forms 8.0.5
- Microsoft.JSInterop.WebAssembly

### Development Tools
- Swagger UI (via Swashbuckle.AspNetCore)
- Blazor WebAssembly Debugging Proxy
- Microsoft.Build.Locator (for EF Core design-time tools)

### Runtime Dependencies
- .NET 8.0 Runtime
- ASP.NET Core 8.0 Runtime
- WebAssembly runtime (dotnet.native.wasm, version 8.0.5)