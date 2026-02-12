using Keyfactor.Extensions.CAPlugin.DigiCert.Models;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	[Serializable]
	public class DuplicateRequest : CertCentralBaseRequest
	{
		public DuplicateRequest(uint orderId)
		{
			Method = "POST";
			OrderId = orderId;
			Resource = $"services/v2/order/certificate/{OrderId}/duplicate";
			Certificate = new CertificateDuplicateRequest();
		}

		[JsonProperty("certificate")]
		public CertificateDuplicateRequest Certificate { get; set; }

		[JsonProperty("order_id")]
		public uint OrderId { get; set; }

		[JsonProperty("skip_approval")]
		public bool SkipApproval { get; set; }
	}

	public class CertificateDuplicateRequest
	{
		[JsonProperty("common_name")]
		public string CommonName { get; set; }

		[JsonProperty("dns_names")]
		public List<string> DnsNames { get; set; }

		[JsonProperty("csr")]
		public string CSR { get; set; }

		[JsonProperty("server_platform")]
		public Server_platform ServerPlatform { get; set; }

		[JsonProperty("signature_hash")]
		public string SignatureHash { get; set; }

		[JsonProperty("ca_cert_id")]
		public string CACertID { get; set; }
	}

	public class DuplicateResponse : CertCentralBaseResponse
	{
		public DuplicateResponse()
		{
			Requests = new List<Requests>();
		}

		[JsonProperty("id")]
		public int OrderId { get; set; }

		[JsonProperty("requests")]
		public List<Requests> Requests { get; set; }

		[JsonProperty("certificate_chain")]
		public List<CertificateChainElement> CertificateChain { get; set; }
	}
}
