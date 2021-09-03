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
    // Defines set of data that server expects/responds to using "parts"
    public class SoapMessage
    {
        public SoapMessage(XmlNode node)
        {
            this.Name = node.Attributes["name"].Value;
            this.Parts = new List<SoapMessagePart>();

            if (node.HasChildNodes)
            {
                foreach (XmlNode part in node.ChildNodes)
                    this.Parts.Add(new SoapMessagePart(part));
            }
        }

        public string Name { get; set; }

        public List<SoapMessagePart> Parts { get; set; }
    }




    public class SoapMessagePart
    {
        public SoapMessagePart(XmlNode part)
        {
            this.Name = part.Attributes["name"].Value;

            if (part.Attributes["element"] != null)
                this.Element = part.Attributes["element"].Value;
            else if (part.Attributes["type"] != null)
                this.Type = part.Attributes["type"].Value;
            else
                throw new ArgumentException("Neither element nor type attribute exist", nameof(part));
        }

        public string Name { get; set; }
        public string Element { get; set; }
        public string Type { get; set; }
    }
}
