using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	public class StatusChangesRequest : CertCentralBaseRequest
	{
		public StatusChangesRequest(string lastSync, string todayUTC)
		{
			this.Resource = $"services/v2/order/certificate/status-changes?filters[status_last_updated]={lastSync}...{todayUTC}";
			this.Method = "GET";
		}
	}

	public class StatusOrder
	{
		[JsonProperty("order_id")]
		public int order_id { get; set; }

		[JsonProperty("certificate_id")]
		public int certificate_id { get; set; }

		[JsonProperty("status")]
		public string status { get; set; }

		[JsonIgnore]
		public string serialNum { get; set; }
	}

	public class StatusChangesResponse : CertCentralBaseResponse
	{
		[JsonProperty("orders")]
		public List<StatusOrder> orders { get; set; }
	}
}
