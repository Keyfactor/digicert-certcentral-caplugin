using Keyfactor.Extensions.CAPlugin.DigiCert.Models;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	public class ListDuplicatesRequest : CertCentralBaseRequest
	{
		public ListDuplicatesRequest(int orderId)
		{
			this.Resource = $"services/v2/order/certificate/{orderId}/duplicate";
			this.Method = "GET";
		}
	}

	public class ListDuplicatesResponse : CertCentralBaseResponse
	{
		[JsonProperty("certificates")]
		public List<CertificateOrder> certificates { get; set; }
	}
}
