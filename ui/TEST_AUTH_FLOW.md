# Testing the Auth Flow

## ✅ What's Running

- **Backend API**: http://localhost:5000
  - Swagger: http://localhost:5000/swagger
  - Health: http://localhost:5000/api/health
  
- **Frontend Angular**: http://localhost:4200
  - Login: http://localhost:4200/login

## 🔒 Routes Configuration

All recipe routes are now **protected** with `authGuard`:
- `/recipes` → redirects to `/login` if not authenticated
- `/recipes/new` → redirects to `/login` if not authenticated  
- `/recipes/:id` → redirects to `/login` if not authenticated
- `/recipes/:id/edit` → redirects to `/login` if not authenticated

## 🧪 Test the Full Flow

### 1. Create a Test User in Supabase

1. Go to [Supabase Dashboard](https://supabase.com/dashboard)
2. Select your project
3. Go to **Authentication** → **Users**
4. Click **Add User** → **Create new user**
5. Enter:
   - **Email**: test@example.com
   - **Password**: Test123456!
6. Click **Create user**

### 2. Test Authentication

1. **Navigate to root**: http://localhost:4200/
   - Should redirect to `/login` (because `/recipes` requires auth)

2. **Go directly to login**: http://localhost:4200/login
   - You should see the login form

3. **Sign in** with:
   - Email: test@example.com
   - Password: Test123456!

4. **After successful sign in**:
   - You'll be redirected to `/recipes`
   - The JWT token is stored in session
   - All API calls will include `Authorization: Bearer <token>`

### 3. Test Protected Routes

Once signed in:
- Navigate around: `/recipes`, `/recipes/new`, etc.
- All should work without redirecting

**Sign out** (you'll need to add a logout button):
```typescript
// In any component
constructor(private auth: AuthService) {}

logout() {
  this.auth.signOut(); // Redirects to /login
}
```

### 4. Test API Calls

Open browser DevTools → Network tab:
1. Make any API call via the app
2. Check the request headers
3. You should see: `Authorization: Bearer eyJhbGc...`

### 5. Test Unauthenticated Access

1. **Open incognito/private window**
2. Go to http://localhost:4200/recipes
3. Should immediately redirect to `/login`

## 🐛 Troubleshooting

**Redirects to /recipes instead of showing login:**
- Clear browser storage (Application → Local Storage → Clear)
- Old session might be cached

**"Invalid login credentials":**
- Make sure the user exists in Supabase
- Check password is correct
- Try creating a new user

**API returns 401 Unauthorized:**
- Check that JWT token is being sent (Network tab → Headers)
- Token might be expired (sign out and sign in again)
- Verify API is using correct Supabase JWKS endpoint

**CORS errors:**
- Make sure API `.env` has `CORS_ORIGINS=http://localhost:4200,https://localhost:4200`
- Restart the API after changing CORS

## 📝 What Happens Behind the Scenes

1. User enters credentials → `AuthService.signIn()`
2. Supabase validates credentials → returns JWT access token
3. `AuthService` stores session and emits user observable
4. `AuthGuard` checks if user is authenticated
5. If authenticated → allow access to route
6. If not → redirect to `/login` with return URL
7. `ApiService` automatically includes token in all requests
8. Backend validates token via Supabase JWKS endpoint
9. If valid → allows access to protected endpoints

## 🎯 Next Steps

1. Add a **logout button** to the app
2. Add a **user profile** display (show current user email)
3. Add **signup flow** (or use the existing signup button on login page)
4. Add **password reset** functionality
5. Handle **token refresh** (Supabase handles this automatically via session)
6. Add **loading states** during auth operations
