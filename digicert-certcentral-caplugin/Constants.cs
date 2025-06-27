using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert
{
	public class Constants
	{
		public class Status
		{
			public const string ISSUED = "issued";
			public const string PENDING = "pending";
			public const string APPROVED = "approved";
			public const string REJECTED = "rejected";
			public const string NEEDS_APPROVAL = "needs_approval";
		}

		public class Config
		{
			public const string APIKEY = "APIKey";
			public const string REGION = "Region";
			public const string DIVISION_ID = "DivisionId";
			public const string LIFETIME = "LifetimeDays";
			public const string CA_CERT_ID = "CACertId";
			public const string RENEWAL_WINDOW = "RenewalWindowDays";
			public const string REVOKE_CERT = "RevokeCertificateOnly";
			public const string ENABLED = "Enabled";
			public const string SYNC_CA_FILTER = "SyncCAFilter";
			public const string SYNC_DIV_FILTER = "SyncDivisionFilter";
			public const string FILTER_EXPIRED = "FilterExpiredOrders";
			public const string SYNC_EXPIRATION_DAYS = "SyncExpirationDays";
			public const string CERT_TYPE = "CertType";
		}

		public class RequestAttributes
		{
			public const string ORGANIZATION_NAME = "Organization-Name";
			public const string DCV_METHOD = "DCV-Method";
		}

		public class ProductTypes
		{
			public const string DV_SSL_CERT = "dv_ssl_certificate";
		}
	}
}
