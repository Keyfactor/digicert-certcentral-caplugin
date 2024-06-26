﻿using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.API
{
	public class DVCheckDCVRequest : CertCentralBaseRequest
	{
		public DVCheckDCVRequest(int orderID)
		{
			this.OrderID = orderID;
			this.Resource = "services/v2/order/certificate/" + orderID.ToString() + "/check-dcv";
			this.Method = "PUT";
		}

		[JsonIgnore]
		public int OrderID { get; set; }
	}

	public class DVCheckDCVResponse : CertCentralBaseResponse
	{
		[JsonProperty("dcv_status")]
		public string dcv_status { get; set; }

		[JsonProperty("order_status")]
		public string order_status { get; set; }
	}
}
