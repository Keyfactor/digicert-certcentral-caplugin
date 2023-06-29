using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAGateway.DigiCert.Models
{
	public class CertificateOrder
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("thumbprint")]
		public string thumbprint { get; set; }

		[JsonProperty("status")]
		public string status { get; set; }

		[JsonProperty("serial_number")]
		public string serial_number { get; set; }

		[JsonProperty("common_name")]
		public string common_name { get; set; }

		[JsonProperty("dns_names")]
		public List<string> dns_names { get; set; }

		[JsonProperty("date_created")]
		public DateTime? date_created { get; set; }

		//YYYY-MM-DD
		[JsonProperty("valid_from")]
		public DateTime? valid_from { get; set; }

		//YYYY-MM-DD
		[JsonProperty("valid_till")]
		public DateTime? valid_till { get; set; }

		[JsonProperty("csr")]
		public string csr { get; set; }

		[JsonProperty("organization")]
		public IdInformation organization { get; set; }

		[JsonProperty("organization_units")]
		public List<string> organization_units { get; set; }

		[JsonProperty("server_platform")]
		public Server_platform server_platform { get; set; }

		[JsonProperty("signature_hash")]
		public string signature_hash { get; set; }

		[JsonProperty("key_size")]
		public int key_size { get; set; }

		[JsonProperty("ca_cert")]
		public IdInformation ca_cert { get; set; }
	}

	public class Server_platform
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("install_url")]
		public string install_url { get; set; }

		[JsonProperty("csr_url")]
		public string csr_url { get; set; }
	}
}
