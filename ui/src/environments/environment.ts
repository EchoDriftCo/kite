export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  supabase: {
    url: 'https://umwycxfebintkenehqlj.supabase.co',
    anonKey: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InVtd3ljeGZlYmludGtlbmVocWxqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Njk5MTg0MTMsImV4cCI6MjA4NTQ5NDQxM30.oCxq_-TA_MfB094rHVQYXs3VeEy4jYlZ5mJaiVrXues'
  },
  sentry: {
    dsn: '', // Leave empty for local dev, or add your Sentry DSN
    enabled: false
  }
};
