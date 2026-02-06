# Supabase Angular Setup

## ✅ What's Installed

1. **@supabase/supabase-js** - Supabase client library
2. **Services:**
   - `SupabaseService` - Initializes Supabase client
   - `AuthService` - Handles authentication (sign in/up/out)
   - `ApiService` - Updated to auto-include JWT tokens
3. **Guards:**
   - `authGuard` - Protects routes requiring authentication
4. **Components:**
   - `LoginComponent` - Example login/signup form

## 🔑 Get Your Anon Key

**You need to update the environment files with your actual Supabase anon key:**

1. Go to [Supabase Dashboard](https://supabase.com/dashboard)
2. Select your project: `umwycxfebintkenehqlj`
3. Go to **Settings** → **API**
4. Copy the **anon/public** key (NOT the service_role key!)
5. Replace `'YourAnonKeyHere'` in both environment files

**Files to update:**
- `src/environments/environment.ts` (line 5)
- `src/environments/environment.prod.ts` (line 5)

## 🚀 How to Use

### 1. Protect Routes with Auth Guard

```typescript
// app.routes.ts
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { 
    path: 'recipes', 
    component: RecipesComponent,
    canActivate: [authGuard]  // Protected route
  },
  // ...
];
```

### 2. Sign In

```typescript
// In your component
constructor(private auth: AuthService) {}

async login() {
  try {
    await this.auth.signIn('user@example.com', 'password');
    // User is now signed in, token is stored
  } catch (error) {
    console.error('Login failed:', error);
  }
}
```

### 3. Sign Up

```typescript
async signup() {
  try {
    await this.auth.signUp('user@example.com', 'password');
    // Check email for confirmation link
  } catch (error) {
    console.error('Signup failed:', error);
  }
}
```

### 4. Sign Out

```typescript
async logout() {
  await this.auth.signOut(); // Redirects to /login
}
```

### 5. Get Current User

```typescript
// Subscribe to user changes
this.auth.currentUser$.subscribe(user => {
  if (user) {
    console.log('Logged in as:', user.email);
  }
});

// Or get current value
const user = this.auth.currentUser;
```

### 6. Make Authenticated API Calls

```typescript
// The ApiService automatically includes the JWT token!
constructor(private api: ApiService) {}

getRecipes() {
  this.api.get<Recipe[]>('recipes').subscribe(recipes => {
    // Token is automatically sent in Authorization header
  });
}
```

## 🔐 How Authentication Works

1. User signs in via `AuthService.signIn()`
2. Supabase returns a JWT access token
3. `AuthService` stores the session and user in observables
4. `ApiService` automatically includes `Authorization: Bearer <token>` header
5. Your .NET API validates the token using the JWKS endpoint
6. Protected routes use `authGuard` to check authentication

## 📝 Example Flow

```typescript
// 1. User signs in
await this.auth.signIn('user@example.com', 'password');

// 2. Make API call (token automatically included)
this.api.get<Recipe[]>('recipes').subscribe(recipes => {
  console.log('Fetched recipes:', recipes);
});

// 3. Access user info
this.auth.currentUser$.subscribe(user => {
  if (user) {
    console.log('User ID:', user.id);
    console.log('Email:', user.email);
  }
});

// 4. Sign out when done
await this.auth.signOut();
```

## 🔧 Environment Configuration

### Development (`environment.ts`)
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  supabase: {
    url: 'https://umwycxfebintkenehqlj.supabase.co',
    anonKey: '<YOUR_ANON_KEY_HERE>'  // ← UPDATE THIS!
  }
};
```

### Production (`environment.prod.ts`)
```typescript
export const environment = {
  production: true,
  apiUrl: '/api/v1',  // Relative path for production
  supabase: {
    url: 'https://umwycxfebintkenehqlj.supabase.co',
    anonKey: '<YOUR_ANON_KEY_HERE>'  // ← UPDATE THIS!
  }
};
```

## 🎯 Next Steps

1. **Get your anon key** from Supabase dashboard and update environment files
2. **Add login route** to `app.routes.ts`:
   ```typescript
   { path: 'login', component: LoginComponent }
   ```
3. **Protect your routes** with `authGuard`
4. **Test authentication:**
   - Create a user in Supabase dashboard or via signup
   - Sign in with the Angular app
   - Make API calls - they'll include the JWT automatically

## 🐛 Troubleshooting

**"Failed to fetch"** - Check that `apiUrl` in environment matches your running API

**"Unauthorized (401)"** - JWT token might be expired or invalid. Sign out and sign in again.

**"Invalid login credentials"** - User doesn't exist or password is wrong. Try signing up first.

**Token not sent** - Make sure you're using `ApiService` methods, not raw HttpClient

## 📚 More Info

- [Supabase Auth Docs](https://supabase.com/docs/guides/auth)
- [Supabase JS Client](https://supabase.com/docs/reference/javascript/introduction)
