# Kite (RecipeVault) - Quick Start Guide

## Running the Full Stack Application

This guide will help you get both the .NET backend and Angular frontend running together.

### Prerequisites
- ✅ .NET 8 SDK installed
- ✅ Node.js v18+ installed
- ✅ Angular CLI v17 installed (`npm install -g @angular/cli@17`)
- ✅ Database configured (check backend .env file)

### Step 1: Start the Backend API

Open a terminal and run:

```bash
cd C:\Projects\kite\src\RecipeVault.WebApi
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

> **Note:** The actual port may vary. Check the console output for the exact URL.
> If the API runs on a different port, update `src/environments/environment.ts` in the UI project.

### Step 2: Start the Frontend

Open a **new terminal** and run:

```bash
cd C:\Projects\kite\ui
npm start
```

**Expected output:**
```
✔ Browser application bundle generation complete.
Initial Chunk Files               | Names         |  Raw Size
main.js                           | main          |   1.41 MB |
...

** Angular Live Development Server is listening on localhost:4200, open your browser on http://localhost:4200/ **
```

### Step 3: Access the Application

Open your browser and navigate to:
```
http://localhost:4200
```

You should see the RecipeVault application with:
- A navigation toolbar
- "Recipes" and "New Recipe" buttons
- The recipe list (empty if no recipes exist)

## Project Structure

```
C:\Projects\kite\
├── src/                          # Backend (.NET 8)
│   └── RecipeVault.WebApi/       # API project
│       ├── Program.cs
│       ├── Startup.cs
│       └── appsettings.json      # API configuration (CORS, DB, etc.)
│
└── ui/                           # Frontend (Angular 17)
    ├── src/
    │   ├── app/
    │   │   ├── components/       # Recipe components
    │   │   ├── services/         # API services
    │   │   └── models/           # TypeScript models
    │   └── environments/         # Environment configs
    ├── package.json
    └── README.md                 # Detailed UI documentation
```

## API Endpoints

The frontend is configured to connect to these backend endpoints:

- `GET /api/recipes` - List all recipes
- `GET /api/recipes/:id` - Get recipe by ID
- `POST /api/recipes` - Create recipe
- `PUT /api/recipes/:id` - Update recipe
- `DELETE /api/recipes/:id` - Delete recipe
- `POST /api/recipes/:id/image` - Upload recipe image
- `POST /api/recipes/parse-image` - Parse recipe from image (AI)

## CORS Configuration

The backend is already configured to accept requests from the Angular dev server:

**File:** `src/RecipeVault.WebApi/appsettings.json`
```json
{
  "Cors": {
    "Origins": ["http://localhost:4200", "https://localhost:4200"]
  }
}
```

If you change the frontend port, update this configuration.

## Environment Variables

The backend uses environment variables from a `.env` file in the root:

**File:** `C:\Projects\kite\.env`
```env
Database__ConnectionString=...
Supabase__Url=...
Supabase__ServiceKey=...
Gemini__ApiKey=...
```

Make sure this file is configured before starting the API.

## Troubleshooting

### Backend won't start
**Problem:** Database connection error  
**Solution:** Check `.env` file has correct `Database__ConnectionString`

**Problem:** Port 5000 already in use  
**Solution:** 
```bash
dotnet run --urls "http://localhost:5005;https://localhost:5006"
```
Then update `ui/src/environments/environment.ts` to use port 5005.

### Frontend won't connect to API
**Problem:** "Failed to load recipes. Make sure the API is running."  
**Solution:**
1. Verify backend is running (check terminal)
2. Check browser console for CORS errors
3. Verify `environment.ts` has correct API URL
4. Check backend `appsettings.json` CORS configuration

**Problem:** Port 4200 already in use  
**Solution:**
```bash
ng serve --port 4201
```
Then update backend CORS config to include `http://localhost:4201`.

### Build Errors
**Problem:** Angular build fails  
**Solution:**
```bash
cd ui
rm -rf node_modules package-lock.json
npm install
```

## Features Available

### ✅ Currently Scaffolded
- Recipe list view with grid layout
- Recipe detail view
- Create/Edit recipe form
- Image upload field (backend integration pending)
- Responsive Material Design UI
- Client-side routing

### 🚧 Pending Backend Integration
- Actual API calls (once endpoints are ready)
- Image upload to Supabase storage
- AI recipe parsing with Gemini
- Recipe categories and tags
- Search and filtering

## Next Steps

1. **Test API Integration**
   - Verify all CRUD operations work through the UI
   - Test error handling
   - Check response formats match TypeScript models

2. **Implement Image Features**
   - Test image upload flow
   - Integrate AI parsing
   - Add image preview

3. **Enhance UI/UX**
   - Add loading indicators
   - Improve error messages
   - Add confirmation dialogs
   - Toast notifications

## Documentation

- **Frontend README:** `ui/README.md` - Detailed Angular documentation
- **Scaffold Summary:** `ui/SCAFFOLD_SUMMARY.md` - What was built
- **Backend API:** Check `RecipeVault-API.postman_collection.json` for API specs

## Development Workflow

1. **Backend Changes:**
   - Edit C# code
   - Hot reload should work automatically
   - Or restart: `Ctrl+C` then `dotnet run`

2. **Frontend Changes:**
   - Edit TypeScript/HTML/SCSS
   - Browser auto-refreshes (Angular dev server)
   - No restart needed

3. **Database Changes:**
   - Create migration: `.\add-migration.ps1 "MigrationName"`
   - Apply: `dotnet ef database update`

---

**Status:** ✅ Frontend scaffolding complete and ready for backend integration!

For questions or issues, check the detailed documentation in `ui/README.md` or the scaffold summary in `ui/SCAFFOLD_SUMMARY.md`.
