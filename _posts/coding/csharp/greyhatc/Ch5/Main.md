## The Main Method implements the NessusSession and NessusManager classes to run a scan and print the results.

* We begin our automation by first disabling SSL certificate verification. Once we have an authenticated session and a manager, we can begin interacting with the Nessus server.

* As we iterate over the scan policies, we check whether the name of the current scan policy
equals the string basic and then  call CreateScan() and we store its response in a JObject. We then pull the scan ID out of the JObjectso that we can pass the scan ID to StartScan() to start the scan.
We use GetScan() to monitor the scan by passing it the scan ID, storing the result in a JObject.

_Note: A null object is returned when we create the scan as discussed,  automation for scans was removed versions 7.0 prior._

## Code:

```csharp
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
using System.Threading;

/*The REST API to automate scans was removed from Nesssus 7.0 and above. If you need to launch scans in an automated way, you would have to upgrade to Tenable.io or Tenable.sc which have full API integrations*/


namespace Automating_Nessus
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            // Returning true allows any ssl certificate to be accepted by disabling ssl verification
            ServicePointManager.ServerCertificateValidationCallback =
                (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) => true;

            using (NessusSession session = new NessusSession("192.168.0.113", "username", "password"))
            {
                using (NessusManager manager = new NessusManager(session))
                {
                    JObject policies = manager.GetScanPolicies();
                    string discoveryPolicyID = string.Empty;
                    foreach (JObject template in policies["templates"])
                    {
                        // Finding basic scan policy ID
                        if (template["name"].Value<string>() == "discovery")
                            discoveryPolicyID = template["uuid"].Value<string>();
                    }
                    // Create and start scan policy with basic scan policy ID
                    JObject scan = manager.CreateScan(discoveryPolicyID, "192.168.0.0/24", "A Basic Network Scan", "A full sysytem scan suitable for any host.");
                    int scanID = scan["scan"]["id"].Value<int>();
                    manager.StartScan(scanID);
                    JObject scanStatus = manager.GetScan(scanID);

                    while (scanStatus["info"]["status"].Value<string>() != "completed")
                    {
                        Console.WriteLine("[+] Scan Status: " + scanStatus["info"]["status"].Value<string>());
                        // Printing scan status every 5 seconds
                        Thread.Sleep(5000);
                        scanStatus = manager.GetScan(scanID);
                    }

                    foreach (JObject vuln in scanStatus["vulnerabilities"])
                        Console.WriteLine(vuln.ToString());
                }
            }
        }
    }
}
```
