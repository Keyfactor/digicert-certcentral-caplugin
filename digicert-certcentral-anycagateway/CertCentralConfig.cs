using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAGateway.DigiCert
{
	public class CertCentralConfig
	{
		public string APIKey { get; set; }
		public string Region { get; set; } = "US";
		public int? DivisionId { get; set; }
		public bool? RevokeCertificateOnly { get; set; }
		public bool Enabled { get; set; } = true;
	}
}
