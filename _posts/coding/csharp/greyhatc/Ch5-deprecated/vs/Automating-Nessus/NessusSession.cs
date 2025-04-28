using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;
using System.IO;


namespace Automating_Nessus
{
    // Automatically clean session by calling Dispose() method to implement in an using statement here
    public class NessusSession: IDisposable
    {
        public NessusSession(string host, string username, string password)
        {
            ServicePointManager.ServerCertificateValidationCallback =
                (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) => true;

            this.Host = host;

            if (!Authenticate(username, password))
                throw new Exception("[!] Authentication Failed");
        }

        public bool Authenticate(string username, string password)
        {
            JObject obj = new JObject();
            obj["username"] = username;
            obj["password"] = password;

            JObject ret = MakeRequest(WebRequestMethods.Http.Post, "/session", obj);

            if (ret["token"] == null)
                return false;

            this.Token = ret["token"].Value<string>();
            this.Authenticated = true;

            return true;
        }

        public JObject MakeRequest(string method, string uri, JObject data = null, string token = null)
        {
            string url = "https://" + this.Host + ":8834" + uri;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;

            if (!string.IsNullOrEmpty(token))
                request.Headers["X-Cookie"] = "token=" + token;

            request.ContentType = "application/json";
            request.Accept = "application/json";

            // Write() method can only write bytes hence converting 
            if (data != null)
            {
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data.ToString());
                request.ContentLength = bytes.Length;
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(bytes, 0, bytes.Length);
            }
            else
                request.ContentLength = 0;

            string response = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                    response = reader.ReadToEnd();
            }
            catch
            {
                return new JObject();
            }

            if (string.IsNullOrEmpty(response))
                return new JObject();

            return JObject.Parse(response);
        }


        public void LogOut()
        {
            if (this.Authenticated)
            {
                MakeRequest("DELETE", "/session", null, this.Token);
                this.Authenticated = false;
            }
        }


        public void Dispose()
        {
            if (this.Authenticated)
                this.LogOut();
        }

        public string Host { get; set; }
        public bool Authenticated { get; set; }
        public string Token { get; private set; }


        //public static void Main(string[] args)
        //{
            // The NessusSession class is created in the context of the using block, which will implement the IDispose() method at the expiration of the using blocks scope.
        //    using (NessusSession session = new NessusSession("127.0.0.1", "username", "password"))
        //    {
        //        Console.WriteLine("[+] Your authentication token is: " + session.Token);
        //    }
        //}
    }
}
