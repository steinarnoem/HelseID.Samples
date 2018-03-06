namespace HelseID.Models
{
    public class Settings
    {
        public string Authority { get; set; }
        public string DcrApi { get; set; }
        public string Thumbprint { get; set; } 
        public string ClientId { get; set; } 
        public string GrantType { get; set; }
        public string RedirectUri { get; set; }
        public string LogoutUri { get; set; }
        public string Scopes { get; set; }
    }
}
