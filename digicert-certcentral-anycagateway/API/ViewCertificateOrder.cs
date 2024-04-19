using Keyfactor.Extensions.CAPlugin.DigiCert.Models;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	public class ViewCertificateOrderRequest : CertCentralBaseRequest
	{
		public ViewCertificateOrderRequest(uint OrderId)
		{
			this.order_id = OrderId;
			this.Resource = "services/v2/order/certificate/" + this.order_id.ToString();
			this.Method = "GET";
		}

		public uint order_id { get; set; }
	}

	public class CertificateOrganization
	{
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("display_name")]
		public string display_name { get; set; }

		[JsonProperty("is_active")]
		public bool is_active { get; set; }

		[JsonProperty("city")]
		public string city { get; set; }

		[JsonProperty("state")]
		public string state { get; set; }

		[JsonProperty("country")]
		public string country { get; set; }
	}

	public class OrderNote
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("text")]
		public string text { get; set; }
	}

	public class ViewCertificateOrderResponse : CertCentralBaseResponse
	{
		public ViewCertificateOrderResponse()
		{
			this.ContentType = ContentTypes.TEXT;
			this.requests = new List<RequestSummary>();
			this.notes = new List<OrderNote>();
		}

		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("certificate")]
		public CertificateOrder certificate { get; set; }

		[JsonProperty("status")]
		public string status { get; set; }

		[JsonProperty("date_created")]
		public DateTime? date_created { get; set; }

		[JsonProperty("order_valid_till")]
		public DateTime? order_valid_till { get; set; }

		[JsonProperty("product")]
		public Product product { get; set; }

		[JsonProperty("organization_contact")]
		public Contact organization_contact { get; set; }

		[JsonProperty("requests")]
		public List<RequestSummary> requests { get; set; }

		[JsonProperty("dcv_method")]
		public string dcv_method { get; set; }

		[JsonProperty("notes")]
		public List<OrderNote> notes { get; set; }

		[JsonIgnore]
		public string Certificate { get; set; }

		[JsonIgnore]
		public string CertificateTemplate { get; set; }

		[JsonIgnore]
		public string RawData { get; set; }
	}
}
