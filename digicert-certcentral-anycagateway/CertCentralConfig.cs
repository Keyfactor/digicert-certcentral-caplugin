using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert
{
	public class CertCentralConfig
	{

		public CertCentralConfig()
		{
			SyncCAFilter = new List<string>();
		}
		public string APIKey { get; set; }
		public string Region { get; set; } = "US";
		public int? DivisionId { get; set; }
		public bool? RevokeCertificateOnly { get; set; }
		public bool Enabled { get; set; } = true;
		public List<string> SyncCAFilter { get; set; }
		public bool? FilterExpiredOrders { get; set; }
		public int? SyncExpirationDays { get; set; }
	}
}
