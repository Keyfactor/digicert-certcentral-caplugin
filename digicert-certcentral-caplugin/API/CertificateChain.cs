using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	public class CertificateChainRequest : CertCentralBaseRequest
	{
		public CertificateChainRequest(string certificate_id)
		{
			this.Resource = $"services/v2/certificate/{certificate_id}/chain";
			this.Method = "GET";
		}
	}

	public class CertificateChainResponse : CertCentralBaseResponse
	{
		[JsonProperty("Intermediates")]
		public List<CertificateChainElement> Intermediates { get; set; }
	}
}
