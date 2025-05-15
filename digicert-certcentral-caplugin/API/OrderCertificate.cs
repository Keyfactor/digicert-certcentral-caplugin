using Keyfactor.Extensions.CAPlugin.DigiCert.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	public class OrderRequest : CertCentralBaseRequest
	{
		public OrderRequest(CertCentralCertType certType)
		{
			Resource = "services/v2/order/certificate/" + certType.ProductCode;
			Method = "POST";
			CertType = certType;
			Certificate = new CertificateRequest();
			CustomExpirationDate = null;
		}

		[JsonIgnore]
		public CertCentralCertType CertType { get; set; }

		[JsonProperty("certificate")]
		public CertificateRequest Certificate { get; set; }

		[JsonProperty("organization")]
		private IdInformation Organization { get; set; } // Set via SetOrganization method

		[JsonProperty("validity_years")]
		public int ValidityYears { get; set; }

		[JsonProperty("custom_expiration_date")] //YYYY-MM-DD
		public DateTime? CustomExpirationDate { get; set; }

		[JsonProperty("comments")]
		public string Comments { get; set; }

		[JsonProperty("disable_renewal_notifications")]
		public bool DisableRenewalNotifications { get; set; }

		[DefaultValue(0)]
		[JsonProperty("renewal_of_order_id")]
		public int RenewalOfOrderId { get; set; }

		[JsonProperty("dcv_method")]
		public string DCVMethod { get; set; }

		[JsonProperty("container")]
		public CertificateOrderContainer Container { get; set; }

		[JsonProperty("custom_fields")]
		public List<MetadataField> CustomFields { get; set; }

		[JsonProperty("skip_approval")]
		public bool SkipApproval {  get; set; }

		public void SetOrganization(int? organizationId)
		{
			if (organizationId.HasValue)
			{
				Organization = new IdInformation()
				{
					Id = organizationId.Value.ToString()
				};
			}
			else
			{
				Organization = null;
			}
		}
	}

	public class CertificateRequest
	{
		[JsonProperty("common_name")]
		public string CommonName { get; set; }

		[JsonProperty("dns_names")]
		public List<string> DNSNames { get; set; }

		[JsonProperty("emails")]
		public List<String> Emails { get; set; }

		[JsonProperty("csr")]
		public string CSR { get; set; }

		[JsonProperty("organization_units")]
		public List<string> OrganizationUnits { get; set; }

		[JsonProperty("server_platform")]
		public IdInformation ServerPlatform { get; set; }

		[JsonProperty("signature_hash")]
		public string SignatureHash { get; set; }

		[JsonProperty("ca_cert_id")]
		public string CACertID { get; set; }
	}

	public class CertificateOrderContainer
	{
		[JsonProperty("id")]
		public int Id { get; set; }
	}

	public class MetadataField
	{
		[JsonProperty("metadata_id")]
		public int MetadataId { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }
	}

	public class OrderResponse : CertCentralBaseResponse
	{
		public OrderResponse()
		{
			this.Requests = new List<Requests>();
			CertificateChain = null;
		}

		[JsonProperty("id")]
		public int OrderId { get; set; }

		[JsonProperty("requests")]
		public List<Requests> Requests { get; set; }

		[JsonProperty("certificate_id")]
		public int? CertificateId { get; set; }

		[JsonProperty("certificate_chain")]
		public List<CertificateChainElement> CertificateChain { get; set; }

		[JsonProperty("dcv_random_value")]
		public string DCVRandomValue { get; set; }
	}

	public class Requests
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }
	}

	public class CertificateChainElement
	{
		[JsonProperty("subject_common_name")]
		public string SubjectCommonName { get; set; }

		[JsonProperty("pem")]
		public string PEM { get; set; }
	}
}
