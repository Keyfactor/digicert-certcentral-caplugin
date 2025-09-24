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
	public class OrderSmimeRequest : CertCentralBaseRequest
	{
		public OrderSmimeRequest(CertCentralCertType certType)
		{
			Resource = "services/v2/order/certificate/" + certType.ProductCode;
			Method = "POST";
			CertType = certType;
			Certificate = new SmimeCertificateRequest();
			Certificate.Individual = new SmimeIndividual();
			Certificate.UsageDesignation = new SmimeUsage();
			Subject = new SmimeSubject();
			CustomExpirationDate = null;
		}

		[JsonIgnore]
		public CertCentralCertType CertType { get; set; }

		[JsonProperty("certificate")]
		public SmimeCertificateRequest Certificate { get; set; }

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

		[JsonProperty("subject")]
		public SmimeSubject Subject { get; set; }

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

	public class SmimeSubject
	{
		[JsonProperty("include_pseudonym")]
		public bool IncludePseudonym { get; set; }

		[JsonProperty("include_email")]
		public bool IncludeEmail { get; set; }

		[JsonProperty("include_given_name_surname")]
		public bool IncludeGivenName { get; set; }
		
	}

	public class SmimeCertificateRequest
	{
		[JsonProperty("emails")]
		public List<String> Emails { get; set; }

		[JsonProperty("csr")]
		public string CSR { get; set; }

		[JsonProperty("signature_hash")]
		public string SignatureHash { get; set; }

		[JsonProperty("ca_cert_id")]
		public string CACertID { get; set; }

		[JsonProperty("common_name_indicator")]
		public string CommonNameIndicator { get; set; }

		[JsonProperty("individual")]
		public SmimeIndividual Individual { get; set; }

		[JsonProperty("usage_designation")]
		public SmimeUsage UsageDesignation { get; set; }

		[JsonProperty("profile_type")]
		public string ProfileType { get; set; }
	}

	public class SmimeIndividual
	{
		[JsonProperty("first_name")]
		public string FirstName { get; set; }

		[JsonProperty("last_name")]
		public string LastName { get; set; }

		[JsonProperty("pseudonym")]
		public string Pseudonym { get; set; }
	}

    public class SmimeUsage
    {
		[JsonProperty("primary_usage")]
		public string PrimaryUsage { get; set; }
    }
}
