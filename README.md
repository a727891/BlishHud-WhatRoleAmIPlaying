# What Am I Playing - Development Server

A simple Node.js development server to serve the roles.json configuration file locally for testing the BlishHUD module.

## Setup

1. Install Node.js dependencies:
   ```bash
   npm install
   ```

2. Start the development server:
   ```bash
   npm start
   ```

   Or for development with auto-restart:
   ```bash
   npm run dev
   ```

## Usage

The server will start on `http://localhost:3000` and serve:

- **`/roles.json`** - The main roles configuration file
- **`/health`** - Health check endpoint
- **`/`** - Server info and available endpoints

## Generating roles from Snow Crows URLs

To regenerate `roles.json` from a list of Snow Crows raid build URLs (one URL per line):

```bash
node generate-roles-from-urls.js path/to/snowcrows_raid_build_urls.txt
```

Optional: `--output roles.json` (default is `./roles.json`). The script preserves existing `professions` and `elite_specs` in `roles.json` and replaces only the `roles` array. Each role includes a `beginner` flag (`true` when the build URL slug starts with `beginner-`).

## Configuration

To use this with your BlishHUD module during development, update the `STATIC_HOST_URL` in your module to point to:

```csharp
public static string STATIC_HOST_URL = "http://localhost:3000";
```

## Endpoints

- `GET /roles.json` - Returns the roles configuration
- `GET /health` - Health check endpoint
- `GET /` - Server information

## Stopping the Server

Press `Ctrl+C` in the terminal to stop the development server. 