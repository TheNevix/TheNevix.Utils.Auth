namespace TheNevix.Utils.Auth.Configuration
{
    public class AuthMethodConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public AuthMethod Type { get; set; }
        public string? HeaderName { get; set; }
    }

    public enum AuthMethod
    {
        Jwt,
        ApiKey
    }

}
