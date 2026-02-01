namespace RecipeVault.Configuration {
    public class SupabaseConfiguration {
        public string Url { get; set; }
        public string ServiceKey { get; set; }
        public string StorageBucket { get; set; }
        public SupabaseAuthConfiguration Auth { get; set; }
    }

    public class SupabaseAuthConfiguration {
        public string JwtSecret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
