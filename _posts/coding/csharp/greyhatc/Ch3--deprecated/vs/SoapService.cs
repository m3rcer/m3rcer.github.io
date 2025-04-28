using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace WSDL_Fuzzer
{
    public class SoapService
    {
        public SoapService(XmlNode node)
        {
            this.Name = node.Attributes["name"].Value;
            this.Ports = new List<SoapPort>();
            foreach (XmlNode port in node.ChildNodes)
                this.Ports.Add(new SoapPort(port));
        }
        public string Name { get; set; }
        public List<SoapPort> Ports { get; set; }
    }

    public class SoapPort
    {
        public SoapPort(XmlNode port)
        {
            this.Name = port.Attributes["name"].Value;
            this.Binding = port.Attributes["binding"].Value;
            this.ElementType = port.FirstChild.Name;
            this.Location = port.FirstChild.Attributes["location"].Value;
        }
        public string Name { get; set; }
        public string Binding { get; set; }
        public string ElementType { get; set; }
        public string Location { get; set; }

    }
}
