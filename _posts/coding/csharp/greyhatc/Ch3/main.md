## The MAIN class uses all the parsed data from the WSDL class to interact and fuzz the data in the WSDL using sub-methods.

- We use the Query (LINQ) Single() method to select a single object that corresponds to the definied definition.

- Use the HTTPWebRequest and StreamReader methods as defined in previous programs.

- Create WSDL definitions using XNameSpace, XElement for fuzzing SOAP endpoints.

### CODE:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.Reflection;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace WSDL_Fuzzer
{
    public class MainClass
    {
        //Static variables
        private static WSDL _wsdl = null;
        private static string _endpoint = null;
        public static void Main(string[] args)
        {
            _endpoint = args[0];
            Console.WriteLine("[+] Fetching the WSDL for service: " + _endpoint);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_endpoint + "?WSDL");
            XmlDocument wsdlDoc = new XmlDocument();
            using (WebResponse resp = req.GetResponse())
            using (Stream respStream = resp.GetResponseStream())
                wsdlDoc.Load(respStream);

            _wsdl = new WSDL(wsdlDoc);
            Console.WriteLine("[+] Fetched and loaded the web service description.");

            foreach (SoapService service in _wsdl.Services)
                FuzzService(service);
        }

        // Checking wether service type is HTTP/Soap and passing execution to respective methods
        static void FuzzService(SoapService service)
        {
            Console.WriteLine("[+] Fuzzing service: " + service.Name);

            foreach (SoapPort port in service.Ports)
            {
                Console.WriteLine("[+] Fuzzing " + port.ElementType.Split(':')[0] + " port: " + port.Name);
                SoapBinding binding = _wsdl.Bindings.Single(b => b.Name == port.Binding.Split(':')[1]);

                if (binding.IsHTTP)
                    FuzzHttpPort(binding);
                else
                    FuzzSoapPort(binding);
            }
        }

        /* HTTP Methods:*/
        // Determine if GET/POST type:
        static void FuzzHttpPort(SoapBinding binding)
        {
            if (binding.Verb == "GET")
                FuzzHttpGetPort(binding);
            else if (binding.Verb == "POST")
                FuzzHttpPostPort(binding);
            else
                throw new Exception("[!] Dont know the verb: " + binding.Verb);
        }

        // Fuzzing Http GET requests:
        static void FuzzHttpGetPort(SoapBinding binding)
        {
            SoapPortType portType = _wsdl.PortTypes.Single(pt => pt.Name == binding.Type.Split(':')[1]);
            foreach (SoapBindingOperation op in binding.Operations)
            {
                Console.WriteLine("[+] Fuzzing Operation: " + op.Name);
                string url = _endpoint + op.Location;
                SoapOperation po = portType.Operations.Single(p => p.Name == op.Name);
                SoapMessage input = _wsdl.Messages.Single(m => m.Name == po.Input.Split(':')[1]);
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                foreach (SoapMessagePart part in input.Parts)
                    parameters.Add(part.Name, part.Type);

                bool first = true;
                List<Guid> guidList = new List<Guid>();
                foreach (var param in parameters)
                {
                    if (param.Value.EndsWith("string"))
                    {
                        Guid guid = Guid.NewGuid();
                        guidList.Add(guid);
                        // Ternary operation to decide if first to prepend with "?" else with an "&"
                        url += (first ? "?" : "&") + param.Key + "=" + guid.ToString();
                    }
                    first = false;
                    }

                // This part adds tainted values to complete fuzzing
                Console.WriteLine("[+] Fuzzing full url: " + url);
                int k = 0;
                foreach (Guid guid in guidList)
                {
                    string testUrl = url.Replace(guid.ToString(), "se'xy");
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(testUrl);
                    string resp = string.Empty;
                    try
                    {
                        using (StreamReader rdr = new StreamReader(req.GetResponse().GetResponseStream()))
                            resp = rdr.ReadToEnd();
                    }
                    catch (WebException ex)
                    {
                        using (StreamReader rdr = new StreamReader(ex.Response.GetResponseStream()))
                            resp = rdr.ReadToEnd();

                        if (resp.Contains("syntax error"))
                            Console.WriteLine("[+++] Possible SQL injection vector found in parameter: " + input.Parts[k].Name);
                    }
                    k++;
                }                
            }
        }

        // Fuzzing Http POST requests:
        static void FuzzHttpPostPort(SoapBinding binding)
        {
            SoapPortType portType = _wsdl.PortTypes.Single(pt => pt.Name == binding.Type.Split(':')[1]);
            foreach (SoapBindingOperation op in binding.Operations)
            {
                Console.WriteLine("[+] Fuzzing operation: " + op.Name);
                string url = _endpoint + op.Location;
                SoapOperation po = portType.Operations.Single(p => p.Name == op.Name);
                SoapMessage input = _wsdl.Messages.Single(m => m.Name == po.Input.Split(':')[1]);
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                foreach (SoapMessagePart part in input.Parts)
                    parameters.Add(part.Name, part.Type);

                // Building HTTP parameters
                string postParams = string.Empty;
                bool first = true;
                List<Guid> guids = new List<Guid>();
                foreach (var param in parameters)
                {
                    if (param.Value.EndsWith("string"))
                    {
                        Guid guid = Guid.NewGuid();
                        postParams += (first ? "" : "&") + param.Key + "=" + guid.ToString();
                        guids.Add(guid);
                     }
                    if (first)
                        first = false;

                    //Sending, recieving request and fuzzing for SQL errors
                    int k = 0;
                    foreach (Guid guid in guids)
                    {
                        string testParams = postParams.Replace(guid.ToString(), "ha'cker");
                        byte[] data = System.Text.Encoding.ASCII.GetBytes(testParams);

                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                        req.Method = "POST";
                        req.ContentType = "application/x-www-form-urlencoded";
                        req.ContentLength = data.Length;
                        req.GetRequestStream().Write(data, 0, data.Length);

                        string resp = string.Empty;
                        try
                        {
                            using (StreamReader rdr = new StreamReader(req.GetResponse().GetResponseStream()))
                                resp = rdr.ReadToEnd();
                        }
                        catch (WebException ex)
                        {
                            using (StreamReader rdr = new StreamReader(ex.Response.GetResponseStream()))
                                resp = rdr.ReadToEnd();

                            if (resp.Contains("syntax error"))
                                Console.WriteLine("[+++] Possible Sql Injection vector found in parameter: " + input.Parts[k].Name);
                        }
                        k++;
                    }
                }
            }
        }

        /* SOAP Method:*/
        static void FuzzSoapPort(SoapBinding binding)
        {
            SoapPortType portType = _wsdl.PortTypes.Single(pt => pt.Name == binding.Type.Split(':')[1]);

            foreach (SoapBindingOperation op in binding.Operations)
            {
                Console.WriteLine("[+] Fuzzing operation: " + op.Name);
                SoapOperation po = portType.Operations.Single(p => p.Name == op.Name);
                SoapMessage input = _wsdl.Messages.Single(m => m.Name == po.Input.Split(':')[1]);

                //Dynamically building XML using System.Xml.Linq
                XNamespace soapNS = "http://schemas.xmlsoap.org/soap/envelope";
                XNamespace xmlNS = op.SoapAction.Replace(op.Name, string.Empty);
                XElement soapBody = new XElement(soapNS + "Body");
                XElement soapOperation = new XElement(xmlNS + op.Name);

                //Add() method combining SOAP body and operation element
                soapBody.Add(soapOperation);

                List<Guid> paramList = new List<Guid>();
                SoapType type = _wsdl.Types.Single(t => t.Name == input.Parts[0].Element.Split(':')[1]);
                foreach (SoapTypeParameter param in type.Parameters)
                {
                    XElement soapParam = new XElement(xmlNS + param.Name);
                    if (param.Type.EndsWith("string"))
                    {
                        Guid guid = Guid.NewGuid();
                        paramList.Add(guid);
                        soapParam.SetValue(guid.ToString());
                    }
                    soapOperation.Add(soapParam);
                }

                //Putting the whole XML DOC together
                XDocument soapDoc = new XDocument(new XDeclaration("1.0", "ascii", "true"),
                    new XElement(soapNS + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soap", soapNS),
                    new XAttribute("xmlns", xmlNS),
                    soapBody));

                //Sending request and fuzzing
                int k = 0;
                foreach (Guid parm in paramList)
                {
                    string testSoap = soapDoc.ToString().Replace(parm.ToString(), "Se'xy");
                    byte[] data = System.Text.Encoding.ASCII.GetBytes(testSoap);
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_endpoint);
                    req.Headers["SOAPAction"] = op.SoapAction;
                    req.Method = "POST";
                    req.ContentType = "text/xml";
                    req.ContentLength = data.Length;
                    using (Stream stream = req.GetRequestStream())
                        stream.Write(data, 0, data.Length);

                    //Reading and determining response
                    string resp = string.Empty;
                    try
                    {
                        using (StreamReader rdr = new StreamReader(req.GetResponse().GetResponseStream()))
                            resp = rdr.ReadToEnd();
                    }
                    catch (WebException ex)
                    {
                        using (StreamReader rdr = new StreamReader(ex.Response.GetResponseStream()))
                            resp = rdr.ReadToEnd();

                        if (resp.Contains("syntax error"))
                            Console.WriteLine("[+++] Possible SQL injection vector found in parameter: " + type.Parameters[k].Name);
                    }
                    k++;
                }
            }
        }


    }
}
```