namespace HelseID.Clients.HIDEnabler.Models
{
    public class Settings
    {
        public string Authority { get; set; }

        public string DcrApi { get; set; }
        public string Thumbprint { get; set; } 
        public string ClientId { get; set; } 
        public string GrantType { get; internal set; }
        public string RedirectUri { get; internal set; }
        public string LogoutUri { get; internal set; }
        public string Scopes { get; internal set; }
    }
}
