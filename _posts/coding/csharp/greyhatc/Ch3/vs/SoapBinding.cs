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
	public class SoapBinding
	{
		public SoapBinding(XmlNode node)
		{
			this.Name = node.Attributes["name"].Value;
			this.Type = node.Attributes["type"].Value;
			this.IsHTTP = false;
			this.Operations = new List<SoapBindingOperation>();
			foreach (XmlNode op in node.ChildNodes)
			{
				if (op.Name.EndsWith("operation"))
				{
					this.Operations.Add(new SoapBindingOperation(op));
				}
				else if (op.Name == "http:binding")
				{
					this.Verb = op.Attributes["verb"].Value;
					this.IsHTTP = true;
				}
			}
		}

		public string Name { get; set; }
		public List<SoapBindingOperation> Operations { get; set; }
		public bool IsHTTP { get; set; }
		public string Verb { get; set; }
		public string Type { get; set; }
	}

	public class SoapBindingOperation
	{
		public SoapBindingOperation(XmlNode op)
		{
			this.Name = op.Attributes["name"].Value;

			foreach (XmlNode node in op.ChildNodes)
			{
				if (node.Name == "http:operation")
					this.Location = node.Attributes["location"].Value;
				if (node.Name == "soap:operation" || node.Name == "soap12:operation")
					this.SoapAction = node.Attributes["soapAction"].Value;
			}
		}

		public string Name { get; set; }
		public string Location { get; set; }
		public string SoapAction { get; set; }
	}
}
