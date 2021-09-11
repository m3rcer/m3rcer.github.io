---
title: Nexpose Session class
permalink: /permalinks/Nexpose/NessusSession
categories: greyhatcch6
---

## To automate sending commands and receiving responses from Nexpose, we’ll create a session with the NexposeSession class and execute API commands.

* We implement the IDisposable interface when the currently instantiated class in the using statement is disposed during garbage collection.


* Nexpose "multipart/mixed" responses always use the string "--AxB9s13299asdjvbA" to seperate HTTP params.

* The LogOut() method tests whether we’re authenticated with the Nexpose server.

* The NexposeAPIVersion() method finds the API version. (version 1.1 by default, current version 3.0)

### Code:

```csharp
using System;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Automating_Nexpose
{
    public class NexposeSession : IDisposable
    {
        public NexposeSession(string username, string password, string host, int port = 3780, NexposeAPIVersion version = NexposeAPIVersion.v11)
        {
            this.Host = host;
            this.Port = port;
            this.APIVersion = version;

            // Disable ssl verification
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

            this.Authenticate(username, password);
        }

        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsAuthenticated { get; set; }
        public string SessionID { get; set; }
        public NexposeAPIVersion APIVersion { get; set; }


        public XDocument Authenticate(string username, string password)
        {
            XDocument cmd = new XDocument(
                new XElement("LoginRequest",
                    new XAttribute("user-id", username),
                    new XAttribute("password", password)));

            XDocument doc = (XDocument)this.ExecuteCommand(cmd);

            if (doc.Root.Attribute("success").Value == "1")
            {
                this.SessionID = doc.Root.Attribute("session-id").Value;
                this.IsAuthenticated = true;
            }
            else
                throw new Exception("[!] Authentication Failed.");

            return doc;
        }

        public object ExecuteCommand(XDocument commandXml)
        {
            string uri = string.Empty;
            switch (this.APIVersion)
            {
                case NexposeAPIVersion.v11:
                    uri = "/api/1.1/xml";
                    break;
                // Changed to current version
                case NexposeAPIVersion.v3:
                    uri = "/api/1.2/xml";
                    break;
                default:
                    throw new Exception("[!] Unknown API version.");
            }

            // Making Request
            byte[] byteArray = Encoding.ASCII.GetBytes(commandXml.ToString());
            HttpWebRequest request = WebRequest.Create("https://" + this.Host + ":" + this.Port.ToString() + uri) as HttpWebRequest;
            request.Method = "POST";
            //request.Proxy = new WebProxy("127.0.0.1:8080");
            request.ContentType = "text/xml";
            request.ContentLength = byteArray.Length;

            using (Stream dataStream = request.GetRequestStream())
                dataStream.Write(byteArray, 0, byteArray.Length);

            // Recieving response
            string response = string.Empty;
            using (HttpWebResponse r = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(r.GetResponseStream()))
                    response = reader.ReadToEnd();

                // If response if "multipart/mixed" we break it into an array of strs
                if (r.ContentType.Contains("multipart/mixed"))
                {
                    // Nexpose "multipart/mixed" responses always use the string "--AxB9s13299asdjvbA" to seperate HTTP params 
                    string[] splitResponse = response
                        .Split(new string[] { "--AxB9s13299asdjvbA" }, StringSplitOptions.None);

                    // Seperate out base64 report data
                    splitResponse = splitResponse[2]
                        .Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);

                    // Remove some invalid data at end of base64 report data
                    string base64Data = splitResponse[1];

                    // Converting Base64
                    return Convert.FromBase64String(base64Data);
                }
            }
            return XDocument.Parse(response);
        }

        public XDocument Logout()
        {
            XDocument cmd = new XDocument(
                new XElement("LogoutRequest",
                new XAttribute("session-id", this.SessionID)));

            XDocument doc = (XDocument)this.ExecuteCommand(cmd);
            this.IsAuthenticated = false;
            this.SessionID = string.Empty;

            return doc;
        }
        // Cleanup Session
        public void Dispose()
        {
            if (this.IsAuthenticated)
                this.Logout();
        }

        // Find API version
        public enum NexposeAPIVersion
        {
            v11,
            v12
        }
    }
}

```
