using IdentityModel.OidcClient;

namespace HelseID.Test.WPF.WebBrowser.EventArgs
{
    public class LoginEventArgs: System.EventArgs
    {
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public bool IsError { get; set; }
        public string Error { get; set; }
        public LoginResult Response { get; set; }
    }
}
