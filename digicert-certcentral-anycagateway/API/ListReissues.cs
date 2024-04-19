using Keyfactor.Extensions.CAPlugin.DigiCert.Models;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	public class ListReissueRequest : CertCentralBaseRequest
	{
		public ListReissueRequest(int orderId)
		{
			this.Resource = $"services/v2/order/certificate/{orderId}/reissue";
			this.Method = "GET";
		}
	}

	public class ListReissueResponse : CertCentralBaseResponse
	{
		[JsonProperty("certificates")]
		public List<CertificateOrder> certificates { get; set; }
	}
}
