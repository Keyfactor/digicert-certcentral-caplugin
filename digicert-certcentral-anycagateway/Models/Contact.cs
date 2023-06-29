using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAGateway.DigiCert.Models
{
	public class User : Contact
	{
		[JsonProperty("id")]
		public int id { get; set; }
	}

	public class Contact
	{
		[JsonProperty("first_name")]
		public string first_name { get; set; }

		[JsonProperty("last_name")]
		public string last_name { get; set; }

		[JsonProperty("email")]
		public string email { get; set; }

		[DefaultValue(null)]
		[JsonProperty("telephone")]
		public string telephone { get; set; }
	}
}
