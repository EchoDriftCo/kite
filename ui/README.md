# RecipeVault - Frontend (Angular 17)

This is the frontend application for RecipeVault, a recipe management app with AI image parsing capabilities.

## Prerequisites

- **Node.js**: v18+ (currently using v22.17.1)
- **npm**: v8+
- **Angular CLI**: v17 (installed globally)

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure API Endpoint

The application is configured to connect to the backend API at `http://localhost:5000/api` by default.

If your API runs on a different port, update the `apiUrl` in:
- `src/environments/environment.ts` (development)
- `src/environments/environment.prod.ts` (production)

### 3. Start the Backend API

Before running the frontend, ensure the .NET backend API is running:

```bash
# From the root of the kite project
cd src/RecipeVault.WebApi
dotnet run
```

The API should be accessible at `http://localhost:5000` (or check the console output for the actual port).

### 4. Run the Development Server

```bash
npm start
# or
ng serve
```

Navigate to `http://localhost:4200/` in your browser. The application will automatically reload if you change any of the source files.

## Project Structure

```
ui/
├── src/
│   ├── app/
│   │   ├── components/
│   │   │   └── recipes/
│   │   │       ├── recipe-list/       # Recipe listing page
│   │   │       ├── recipe-detail/     # Recipe detail view
│   │   │       └── recipe-form/       # Create/Edit recipe form
│   │   ├── models/
│   │   │   └── recipe.model.ts        # TypeScript interfaces for Recipe
│   │   ├── services/
│   │   │   ├── api.service.ts         # Base HTTP service
│   │   │   └── recipe.service.ts      # Recipe-specific API calls
│   │   ├── app.component.*            # Root component with navigation
│   │   ├── app.config.ts              # App configuration (providers)
│   │   └── app.routes.ts              # Route definitions
│   ├── environments/
│   │   ├── environment.ts             # Development environment config
│   │   └── environment.prod.ts        # Production environment config
│   └── index.html
├── angular.json                       # Angular CLI configuration
├── package.json                       # npm dependencies
└── tsconfig.json                      # TypeScript configuration
```

## Features Implemented

### ✅ Components
- **Recipe List**: Browse all recipes with search/filter capabilities
- **Recipe Detail**: View full recipe details including ingredients and instructions
- **Recipe Form**: Create new recipes or edit existing ones

### ✅ Services
- **ApiService**: Generic HTTP service for API communication
- **RecipeService**: Recipe-specific operations (CRUD + image upload)

### ✅ Routing
- `/` - Redirects to recipes list
- `/recipes` - Recipe list view
- `/recipes/new` - Create new recipe
- `/recipes/:id` - View recipe details
- `/recipes/:id/edit` - Edit recipe

### ✅ UI/UX
- Angular Material components for consistent design
- Responsive grid layout
- Loading states and error handling
- Image upload support (placeholder for AI parsing feature)

## Available Scripts

### Development
```bash
npm start          # Start dev server at http://localhost:4200
ng serve --open    # Start dev server and open browser
```

### Build
```bash
npm run build      # Build for production
ng build --configuration production
```

Output will be in the `dist/` directory.

### Testing
```bash
npm test           # Run unit tests with Karma
ng test
```

### Code Quality
```bash
ng lint            # Run ESLint (if configured)
```

## API Integration

The frontend connects to the following API endpoints (all relative to `http://localhost:5000/api`):

### Recipes
- `GET /recipes` - Get all recipes
- `GET /recipes/:id` - Get recipe by ID
- `POST /recipes` - Create new recipe
- `PUT /recipes/:id` - Update recipe
- `DELETE /recipes/:id` - Delete recipe
- `POST /recipes/:id/image` - Upload recipe image
- `POST /recipes/parse-image` - Parse recipe from image using AI

### Request/Response Examples

**GET /recipes**
```json
[
  {
    "id": "uuid",
    "name": "Chocolate Chip Cookies",
    "description": "Classic homemade cookies",
    "ingredients": ["2 cups flour", "1 cup sugar", ...],
    "instructions": "Preheat oven to 350°F...",
    "prepTime": 15,
    "cookTime": 12,
    "servings": 24,
    "category": "Dessert",
    "thumbnailUrl": "https://...",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

## Environment Configuration

### Development (environment.ts)
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

### Production (environment.prod.ts)
```typescript
export const environment = {
  production: true,
  apiUrl: '/api'  // Uses relative path in production
};
```

## CORS Configuration

The backend API is already configured to allow requests from `http://localhost:4200`. If you change the frontend port, update the CORS settings in:

`src/RecipeVault.WebApi/appsettings.json`:
```json
{
  "Cors": {
    "Origins": ["http://localhost:4200", "https://localhost:4200"]
  }
}
```

## Next Steps

### Immediate Tasks
1. ✅ Basic project scaffolding - DONE
2. ✅ Component structure - DONE
3. ✅ Routing setup - DONE
4. ✅ API service configuration - DONE

### Upcoming Features
1. **Image Upload & AI Parsing**
   - Implement drag-and-drop image upload
   - Integrate with Gemini AI image parsing endpoint
   - Preview parsed recipe data before saving

2. **Search & Filtering**
   - Add search bar to recipe list
   - Filter by category, prep time, etc.
   - Sort options (name, date, rating)

3. **User Authentication** (if required)
   - Login/Register components
   - JWT token management
   - Protected routes

4. **Enhanced UI**
   - Recipe rating system
   - Tags/labels
   - Recipe collections/favorites
   - Print view for recipes

5. **Advanced Features**
   - Meal planning
   - Shopping list generation
   - Recipe sharing
   - Nutrition information

## Troubleshooting

### API Connection Issues
**Error:** "Failed to load recipes. Make sure the API is running."

**Solution:**
1. Verify the backend is running: `cd ../src/RecipeVault.WebApi && dotnet run`
2. Check the API URL in `src/environments/environment.ts`
3. Verify CORS is configured correctly in backend `appsettings.json`
4. Check browser console for specific error messages

### Port Already in Use
**Error:** Port 4200 is already in use

**Solution:**
```bash
ng serve --port 4201  # Use a different port
```

### Build Errors
If you encounter build errors after pulling changes:
```bash
rm -rf node_modules package-lock.json
npm install
```

## Technologies Used

- **Angular 17**: Modern web framework with standalone components
- **Angular Material**: UI component library
- **RxJS**: Reactive programming for async operations
- **TypeScript**: Type-safe JavaScript
- **SCSS**: CSS preprocessor for styling

## Learn More

- [Angular Documentation](https://angular.io/docs)
- [Angular Material Components](https://material.angular.io/components)
- [RxJS Documentation](https://rxjs.dev/)

## Contributing

1. Create a feature branch: `git checkout -b feature/my-feature`
2. Make your changes
3. Test thoroughly
4. Commit: `git commit -m "Add my feature"`
5. Push: `git push origin feature/my-feature`
6. Create a Pull Request

---

**Note:** This is the initial scaffold of the application. Many features are placeholder implementations and will need to be fully fleshed out as the backend API evolves.
