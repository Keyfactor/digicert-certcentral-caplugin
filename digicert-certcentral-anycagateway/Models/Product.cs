using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAGateway.DigiCert.Models
{
	public class Product
	{
		[JsonProperty("name_id")]
		public string name_id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("validation_type")]
		public string validation_type { get; set; }

		[JsonProperty("validation_name")]
		public string validation_name { get; set; }

		[JsonProperty("validation_description")]
		public string validation_description { get; set; }
	}
}
