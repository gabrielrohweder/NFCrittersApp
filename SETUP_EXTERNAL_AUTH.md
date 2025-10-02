# External Authentication Setup Guide

This guide explains how to configure Google and Apple sign-in for Critter Wrangler.

## Overview

The application supports three authentication methods:
1. **Email/Password** - Built-in local authentication (always available)
2. **Google Sign-In** - OAuth authentication via Google
3. **Apple Sign In** - OAuth authentication via Apple

External authentication providers (Google and Apple) are **optional** and only activate when credentials are configured.

## Architecture

### Database Schema
The `users` table has been extended to support external authentication:
- `auth_provider` (varchar, default: 'local') - Identifies the auth method: 'local', 'Google', or 'Apple'
- `external_id` (varchar, nullable) - Unique identifier from the external provider
- `password` (varchar, nullable) - Only required for local auth, null for external auth

### Authentication Flow
1. User clicks Google/Apple button on any auth modal
2. Server redirects to provider's OAuth consent page
3. Provider redirects back to `/api/auth/external-login-callback`
4. Server creates or links user account based on email
5. Session is created and user is redirected to the original page

### Account Linking
If a user signs in with Google/Apple using an email that already exists:
- The existing account is automatically linked to the external provider
- User can sign in with either method in the future

## Google Authentication Setup

### 1. Create Google OAuth Application

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Navigate to **APIs & Services > Credentials**
4. Click **Create Credentials > OAuth 2.0 Client ID**
5. Configure the OAuth consent screen:
   - User Type: External
   - App name: Critter Wrangler
   - User support email: Your email
   - Authorized domains: Add your domain (e.g., `nfcritters.com`)
   - Scopes: email, profile
6. Create OAuth 2.0 Client ID:
   - Application type: Web application
   - Name: Critter Wrangler Web
   - Authorized redirect URIs:
     - `http://localhost:5000/api/auth/signin-google` (development)
     - `https://yourdomain.com/api/auth/signin-google` (production)

### 2. Configure Google Credentials

Add to `Server/appsettings.json`:
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

For production, use environment variables or secrets management:
```bash
export Authentication__Google__ClientId="your-client-id"
export Authentication__Google__ClientSecret="your-client-secret"
```

## Apple Authentication Setup

### 1. Create Apple Service ID

1. Go to [Apple Developer Portal](https://developer.apple.com/)
2. Navigate to **Certificates, Identifiers & Profiles**
3. Create an **App ID**:
   - Description: Critter Wrangler
   - Bundle ID: `com.nfcritters.app`
   - Enable "Sign in with Apple"
4. Create a **Service ID**:
   - Description: Critter Wrangler Web
   - Identifier: `com.nfcritters.service`
   - Enable "Sign in with Apple"
   - Configure:
     - Primary App ID: Select your App ID
     - Domains: `nfcritters.com`
     - Return URLs:
       - `http://localhost:5000/api/auth/signin-apple` (development)
       - `https://nfcritters.com/api/auth/signin-apple` (production)

### 2. Create Private Key

1. In Apple Developer Portal, go to **Keys**
2. Click **+** to create a new key
3. Name: Critter Wrangler Auth Key
4. Enable "Sign in with Apple"
5. Configure: Select your primary App ID
6. Download the `.p8` private key file (keep it secure!)
7. Note the **Key ID** (10 characters, e.g., `ABC123DEFG`)

### 3. Get Team ID

1. In Apple Developer Portal, go to **Membership**
2. Note your **Team ID** (10 characters)

### 4. Configure Apple Credentials

The private key content must be formatted as a single line string without headers.

Original `.p8` file format:
```
-----BEGIN PRIVATE KEY-----
MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQg...
...additional lines...
-----END PRIVATE KEY-----
```

Remove headers and join lines:
```
MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQg...
```

Add to `Server/appsettings.json`:
```json
{
  "Authentication": {
    "Apple": {
      "ClientId": "com.nfcritters.service",
      "TeamId": "ABC123DEFG",
      "KeyId": "XYZ789PQRS",
      "PrivateKey": "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQg..."
    }
  }
}
```

For production, use environment variables:
```bash
export Authentication__Apple__ClientId="com.nfcritters.service"
export Authentication__Apple__TeamId="ABC123DEFG"
export Authentication__Apple__KeyId="XYZ789PQRS"
export Authentication__Apple__PrivateKey="MIGTAgEA..."
```

## Testing External Authentication

### Development Testing
1. Start the application: `dotnet run` (in Server directory)
2. Open any page requiring authentication
3. Click "Login" to open auth modal
4. You should see:
   - Email/Password fields (always available)
   - "or continue with" divider
   - Google and Apple buttons (only if configured)

### Verify Configuration
Check server startup logs for:
```
Google authentication configured
Apple authentication configured
```

If credentials are missing, providers won't register (no errors, just unavailable).

### Test Account Creation
1. Click Google/Apple button
2. Complete provider authentication
3. Check database: `SELECT * FROM users WHERE auth_provider = 'Google';`
4. Verify user was created with:
   - `auth_provider`: 'Google' or 'Apple'
   - `external_id`: Provider's user ID
   - `password`: NULL
   - `nickname`: Auto-generated from name or email

### Test Account Linking
1. Create account with email/password: `test@example.com`
2. Sign out
3. Click Google/Apple button and authenticate with same email
4. Check database: User should now have:
   - `auth_provider`: 'Google' or 'Apple' (updated)
   - `external_id`: Provider's user ID (added)
   - `password`: Original hashed password (preserved)

## Production Deployment

### Environment Variables
Use secrets management for production credentials:

**Google Cloud Run / Azure / AWS:**
```bash
Authentication__Google__ClientId=your-client-id
Authentication__Google__ClientSecret=your-client-secret
Authentication__Apple__ClientId=com.nfcritters.service
Authentication__Apple__TeamId=ABC123DEFG
Authentication__Apple__KeyId=XYZ789PQRS
Authentication__Apple__PrivateKey=MIGTAgEA...
```

### Redirect URLs
Update OAuth redirect URLs to production domain:
- Google: `https://nfcritters.com/api/auth/signin-google`
- Apple: `https://nfcritters.com/api/auth/signin-apple`

### Security Considerations
1. **Never commit secrets** to version control
2. **Use HTTPS** in production (required by providers)
3. **Rotate keys** periodically (especially Apple private key)
4. **Monitor failed auth attempts** in application logs
5. **Verify provider email** before creating accounts

## Troubleshooting

### Google Issues
- **Error: redirect_uri_mismatch**
  - Verify authorized redirect URIs match exactly
  - Include protocol (http/https) and port in development
  
- **Error: invalid_client**
  - Check ClientId and ClientSecret are correct
  - Ensure OAuth consent screen is configured

### Apple Issues
- **Error: invalid_client**
  - Verify Service ID matches ClientId
  - Check Team ID and Key ID are correct
  
- **Error: invalid_grant**
  - Private key format might be incorrect
  - Ensure key is active in Apple Developer Portal
  - Check that Service ID is configured with correct App ID

### General Issues
- **Buttons not showing**
  - Check server logs for configuration messages
  - Verify credentials are in appsettings.json or environment variables
  
- **Redirect loop**
  - Check session configuration
  - Clear browser cookies and try again
  
- **Database errors**
  - Ensure `auth_provider`, `external_id` columns exist
  - Run database migrations if needed

## API Endpoints

### External Login
**GET** `/api/auth/external-login?provider={Google|Apple}&returnUrl={url}`
- Initiates OAuth flow with specified provider
- Redirects to provider's consent page
- Returns to `returnUrl` after successful authentication

### Callback
**GET** `/api/auth/external-login-callback?returnUrl={url}`
- Handles OAuth callback from provider
- Creates or links user account
- Establishes session
- Redirects to `returnUrl`

### Available Providers
**GET** `/api/auth/providers`
- Returns list of configured authentication providers
- Response: `[{ "name": "Google", "displayName": "Google" }]`

## UI Integration

External auth buttons are automatically displayed in auth modals on all pages:
- Scan page (`/`)
- Animal view page (`/animal/{token}`)
- Collection page (`/collection`)
- Profile page (`/profile`)

Buttons only appear when providers are configured (graceful degradation).

## Future Enhancements

Potential improvements for external authentication:
1. Support additional providers (Facebook, GitHub, Microsoft)
2. Allow users to link multiple providers to one account
3. Add provider management page (view/unlink providers)
4. Implement refresh token handling for long-lived sessions
5. Add custom scopes for provider-specific features
