﻿using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	public class ListOrganizationsRequest : CertCentralBaseRequest
	{
		public ListOrganizationsRequest()
		{
			Resource = "services/v2/organization";
			Method = "GET";
			ActiveOnly = true;
		}

		[JsonProperty("container_id")]
		public int ContainerId { get; set; }

		[JsonProperty("include_validation")]
		public bool IncludeValidation { get; set; }

		public bool ActiveOnly { get; set; }

		public new string BuildParameters()
		{
			StringBuilder sbParameters = new StringBuilder();

			sbParameters.Append("include_validation=").Append(HttpUtility.UrlEncode(IncludeValidation.ToString()));
			if (ContainerId > 0)
			{
				sbParameters.Append("&container_id=").Append(HttpUtility.UrlEncode(ContainerId.ToString()));
			}
			if (ActiveOnly)
			{
				sbParameters.Append("&filters[status]=active");
			}
			return sbParameters.ToString();
		}
	}

	public class OrgContainer
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("parent_id")]
		public int ParentId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("is_active")]
		public bool IsActive { get; set; }
	}

	public class Validation
	{
		[JsonProperty("type")]
		public int Type { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("date_created")]
		public string DateCreated { get; set; }

		[JsonProperty("validated_until")]
		public string ValidatedUntil { get; set; }

		[JsonProperty("status")]
		public bool Status { get; set; }
	}

	public class EVApprover
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("first_name")]
		public string FirstName { get; set; }

		[JsonProperty("last_name")]
		public string LastName { get; set; }
	}

	public class Organization
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("display_name")]
		public string DisplayName { get; set; }

		[JsonProperty("is_active")]
		public string IsActive { get; set; }

		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("address2")]
		public string Address2 { get; set; }

		[JsonProperty("zip")]
		public string Zip { get; set; }

		[JsonProperty("city")]
		public string City { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("country")]
		public string Country { get; set; }

		[JsonProperty("telephone")]
		public string Telephone { get; set; }

		[JsonProperty("container")]
		public OrgContainer Container { get; set; }

		[JsonProperty("ev_approvers")]
		public List<EVApprover> EvApprovers { get; set; }
	}

	public class ListOrganizationsResponse : CertCentralBaseResponse
	{
		public ListOrganizationsResponse()
		{
			Organizations = new List<Organization>();
		}

		[JsonProperty("organizations")]
		public List<Organization> Organizations { get; set; }
	}
}
