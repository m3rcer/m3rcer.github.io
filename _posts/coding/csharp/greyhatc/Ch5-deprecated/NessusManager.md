## We use the NessusManager Class to wrap common API calls and functionality for Nessus in easy-to-use methods we can call later.

* The NessusManager class implements `IDisposable `so that we can use NessusSession to interact with the Nessus API and log out automatically if necessary.
* Nessus is preconfigured with a few different scan policies. We’ll sort through these policies using `GetScanPolicies()`.
* We create a scan by using the returned policy ID in the `CreatScan()` method. 
* Once we’ve created the scan with `CreateScan()`, we’ll pass its ID to the `StartScan()` method, which will create a POST request and return the JSON response telling us whether the scan was launched.


### Code: 

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

namespace Automating_Nessus
{
    public class NessusManager : IDisposable
    {
        NessusSession _session;
        public NessusManager(NessusSession session)
        {
            _session = session;
        }

        // Sort through various policies and return their ID's
        public JObject GetScanPolicies()
        {
            return _session.MakeRequest("GET", "/editor/scan/templates", null, _session.Token);
        }

        // Succesfully creating the scan
        public JObject CreateScan(string policyID, string cidr, string name, string description)
        {
            JObject data = new JObject();
            data["uuid"] = policyID;
            data["settings"] = new JObject();
            data["settings"]["name"] = name;
            data["settings"]["text_targets"] = cidr;
            data["settings"]["description"] = description;
            data["settings"]["enabled"] = true;


            JObject resp = _session.MakeRequest("POST", "/scans", data, _session.Token);
            return resp;
        }
        // Starting the scan using scanID returned from CreateScan
        public JObject StartScan(int scanID)
        {
            return _session.MakeRequest("GET", "/scans/" + scanID + "/launch", null, _session.Token);
        }
        // Monitor Scan
        public JObject GetScan(int scanID)
        {
            return _session.MakeRequest("GET", "/scans/" + scanID, null, _session.Token);
        }

        public void Dispose()
        {
            if (_session.Authenticated)
                _session.LogOut();
            _session = null;
        }

    }
}
```