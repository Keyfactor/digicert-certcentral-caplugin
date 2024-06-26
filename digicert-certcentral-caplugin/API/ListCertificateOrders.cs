﻿using Keyfactor.Extensions.CAPlugin.DigiCert.Models;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	public class ListCertificateOrdersRequest : CertCentralBaseRequest
	{
		public ListCertificateOrdersRequest(bool ignoreExpired = false)
		{
			this.Resource = "services/v2/order/certificate";
			this.Method = "GET";
			this.limit = 1000;
			this.offset = 0;
			this.ignoreExpired = ignoreExpired;
		}

		[JsonProperty("limit")]
		public int limit { get; set; }

		[JsonProperty("offset")]
		public int offset { get; set; }

		public bool ignoreExpired { get; set; }
		public int expiredWindow { get; set; } = 0;

		public new string BuildParameters()
		{
			StringBuilder sbParamters = new StringBuilder();

			sbParamters.Append("limit=").Append(this.limit.ToString());
			sbParamters.Append("&offset=").Append(HttpUtility.UrlEncode(this.offset.ToString()));

			if (ignoreExpired)
			{
				DateTime cutoffDate = DateTime.Today.AddDays(-1 - expiredWindow);
				sbParamters.Append("&filters[valid_till]=>").Append(cutoffDate.ToString("yyyy-MM-dd"));
			}

			return sbParamters.ToString();
		}
	}

	public class CertificateSummary
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("common_name")]
		public string common_name { get; set; }

		[JsonProperty("dns_names")]
		public List<string> dns_names { get; set; }

		[JsonProperty("signature_hash")]
		public string signature_hash { get; set; }
	}

	public class Order
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("certificate")]
		public CertificateSummary certificate { get; set; }

		//pending, rejected, processing, issued, revoked, canceled, needs_csr, and needs_approval
		[JsonProperty("status")]
		public string status { get; set; }

		[JsonProperty("date_created")]
		public string date_created { get; set; }

		//[JsonProperty("organization")]
		//public IdInformation organization { get; set; }

		[JsonProperty("validity_years")]
		public int validity_years { get; set; }

		[JsonProperty("container")]
		public IdInformation container { get; set; }

		[JsonProperty("product")]
		public Product product { get; set; }

		[JsonProperty("price")]
		public decimal price { get; set; }

		[JsonProperty("has_duplicates")]
		public bool has_duplicates { get; set; }

		[JsonProperty("is_renewed")]
		public bool is_renewed { get; set; }
	}

	public class ListCertificateOrdersResponse : CertCentralBaseResponse
	{
		public ListCertificateOrdersResponse()
		{
			this.orders = new List<Order>();
			page = new PageInfo();
		}

		[JsonProperty("orders")]
		public List<Order> orders { get; set; }

		[JsonProperty("page")]
		public PageInfo page { get; set; }
	}

	public class PageInfo
	{
		[JsonProperty("total")]
		public int total { get; set; }

		[JsonProperty("page")]
		public int page { get; set; }

		[JsonProperty("offset")]
		public int offset { get; set; }
	}
}
