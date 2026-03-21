export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  supabase: {
    url: 'http://localhost:54321',
    anonKey: 'sb_publishable_ACJWlzQHlZjBrEguHvfOxg_3BJgxAaH'
  },
  sentry: {
    dsn: '', // Leave empty for local dev, or add your Sentry DSN
    enabled: false
  }
};
