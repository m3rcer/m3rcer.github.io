---
title: OpenVAS Session class
permalink: /permalinks/OpenVas/OpenVASSession
categories: greyhatcch7
---

## To automate sending commands and receiving responses from OpenVAS, we’ll create a session with the OpenVASSession class and execute API commands.

- We setup a TCP stream to send and recieve commands. 

- We implement the IDisposable interface when the currently instantiated class in the using statement is disposed during garbage collection.

### Code:

```csharp
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;

namespace Automating_Openvas
{
    public class OpenVASSession : IDisposable
    {
        private SslStream _stream = null;

        // Connect to OpenVAS server
        public OpenVASSession(string user, string pass, string host, int port = 9390)
        {
            this.ServerIPAdress = IPAddress.Parse(host);
            this.ServerPort = port;
            this.Authenticate(user, pass);
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public IPAddress ServerIPAdress { get; set; }
        public int ServerPort { get; set; }

        public SslStream Stream
        {
            get
            {
                if ( _stream == null)
                    GetStream();

                return _stream;
            }

            set { _stream = value; }
        }

        // Authentication using and XML req
        public XDocument Authenticate(string username, string password)
        {
            XDocument authXML = new XDocument(
                new XElement("authenticate",
                    new XElement("credentials",
                        new XElement("username", username),
                        new XElement("password", password))));

            XDocument response = this.ExecuteCommand(authXML);

            if (response.Root.Attribute("status").Value != "200")
                throw new Exception("[!] Authentication failed!");

            this.Username = username;
            this.Password = password;

            return response;
        }

        // Method to execute cmds
        public XDocument ExecuteCommand(XDocument doc)
        {
            ASCIIEncoding enc = new ASCIIEncoding();

            string xml = doc.ToString();
            this.Stream.Write(enc.GetBytes(xml), 0, xml.Length);

            return ReadMessage(this.Stream);
        }

        // Read Server Message
        private XDocument ReadMessage(SslStream sslStream)
        {
            // Dynamically store data from server
            using (var stream = new MemoryStream())
            {
                int bytesRead = 0;

                do
                {
                    byte[] buffer = new byte[2048];
                    bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                    stream.Write(buffer, 0, bytesRead);
                    // Check for valid XML doc response
                    if (bytesRead < buffer.Length)
                    {
                        try
                        {
                            string xml = System.Text.Encoding.ASCII.GetString(stream.ToArray());
                            return XDocument.Parse(xml);
                        }
                        // If haven't finished reading the stream
                        catch
                        {
                            continue;
                        }
                    }
                }
                while (bytesRead > 0);
            }
            return null;
        }

        // Setup TCP stream to send and recieve cmds
        private void GetStream()
        {
            if (_stream == null || !_stream.CanRead)
            {
                TcpClient client = new TcpClient(this.ServerIPAdress.ToString(), this.ServerPort);

                _stream = new SslStream(client.GetStream(), false,
                    new RemoteCertificateValidationCallback (ValidateServerCertificate),
                    (sender, targethost, LocalCertificates, remoteCertificate, acceptableIssuers) => null);

                _stream.AuthenticateAsClient("OpenVAS", null, SslProtocols.Tls, false);
            }
        }

        // Certificate validation
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void Dispose()
        {
            if (_stream != null)
                _stream.Dispose();
        }
    }
}
```