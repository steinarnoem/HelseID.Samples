using IdentityModel.OidcClient;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HelseID.Test.WPF
{
    internal class RequestHandler: IDisposable
    {
        public const string DefaultUri = "http://127.0.0.1:7890/";

        private HttpListener Listener { get; set; }

        public RequestHandler(string uri = DefaultUri)
        {
            Listener = new HttpListener();
            try
            {
                Listener.Prefixes.Add(uri);               
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
        }

        public bool IsListening
        {
            get
            {
                if (Listener == null)
                    return false;

                return Listener.IsListening;
            }
        }

        internal async Task Start(OidcClient client, AuthorizeState state, Action<string, OidcClient, AuthorizeState> loginSuccess, string uri = DefaultUri)
        {
            if (Listener.IsListening)
                Listener.Stop();

            Listener.Start();

            await Listener.GetContextAsync().ContinueWith(async (ctx) =>
            {                    
                var formData = HandleRequest(ctx.Result);
                loginSuccess(formData, client, state);
                await OutputResponse(ctx.Result.Response);
            });
            
            Listener.Stop();
        }

        private static string HandleRequest(HttpListenerContext context)
        {
            var postData = GetRequestPostData(context.Request);
            return postData;
        }

        public static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            using (var body = request.InputStream)
            {
                using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static async Task OutputResponse(HttpListenerResponse response)
        {
            const string responseString = "<html><head><meta charset=\"UTF-8\"></head><body><h1>Innlogging vellykket :)</h1><h2>Lukk nettleseren for å logge ut.</h2></body></html>";

            var buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

            response.OutputStream.Close();
        }

        public void AddUri(string uri)
        {            
            Listener.Prefixes.Add(uri);
        }

        public void Dispose()
        {
            Listener.Stop();
            Listener.Close();
        }
    }
}
