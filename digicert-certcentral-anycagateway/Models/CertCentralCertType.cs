using Keyfactor.Extensions.CAGateway.DigiCert.API;
using Keyfactor.Extensions.CAGateway.DigiCert.Client;
using Keyfactor.Logging;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAGateway.DigiCert.Models
{
    public class CertCentralCertType
    {
        #region Private Fields

        private static readonly ILogger Logger = LogHandler.GetClassLogger<CertCentralCertType>();
        private static List<CertCentralCertType> _allTypes;

        #endregion Private Fields

        #region Properties

        /// <summary>
        /// All of the product types for which we do not support enrollment.
        /// </summary>
        public static List<string> UnsupportedProductTypes => new List<string>
        {
            "Document Signing - Organization (2000)",
            "Document Signing - Organization (5000)",
            "Code Signing",
            "EV Code Signing",
            "Premium SHA256",
            "Premium",
            "Email Security Plus",
            "Email Security Plus SHA256",
            "Digital Signature Plus",
            "Digital Signature Plus SHA256",
            "Grid Premium",
            "Grid Robot FQDN",
            "Grid Robot Name",
            "Grid Robot Email"
        };

        public string signatureAlgorithm { get; set; }
        public bool multidomain { get; set; }

        public string ProductType { get; set; }
		public string ShortName { get; set; }
		public string DisplayName { get; set; }
		public string ProductCode { get; set; }

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets all of the product types for DigiCert.
		/// </summary>
		/// <param name="proxyConfig"></param>
		/// <returns></returns>
		public static List<CertCentralCertType> GetAllTypes(CertCentralConfig config)
        {
            if (_allTypes == null || !_allTypes.Any())
            {
                _allTypes = RetrieveCertCentralCertTypes(config);
            }

            return _allTypes;
        }

        /// <summary>
        /// Uses the <see cref="DigiCertCAConfig"/> to build a client and retrieve the product types for the given account.
        /// </summary>
        /// <param name="proxyConfig"></param>
        /// <returns></returns>
        private static List<CertCentralCertType> RetrieveCertCentralCertTypes(CertCentralConfig config)
        {
            Logger.MethodEntry(LogLevel.Trace);
            CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(config);

            // Get all of the cert types.
            CertificateTypesResponse certTypes = client.GetAllCertificateTypes();
            if (certTypes.Status == API.CertCentralBaseResponse.StatusType.ERROR)
            {
                throw new Exception(string.Join("\n", certTypes.Errors?.Select(x => x.message)));
            }
            Logger.LogDebug($"RetrieveCertCentralTypes: Found {certTypes.Products.Count} product types");
            // Get all the information we need.
            List<CertCentralCertType> types = new List<CertCentralCertType>();
            foreach (var type in certTypes.Products)
            {
                try
                {
                    Logger.LogTrace($"RetrieveCertCentralType: Retrieving details for product type: {type.NameId}");
                    CertificateTypeDetailsRequest detailsRequest = new CertificateTypeDetailsRequest(type.NameId, config.DivisionId);
                    CertificateTypeDetailsResponse details = client.GetCertificateTypeDetails(detailsRequest);
                    if (details.Status == API.CertCentralBaseResponse.StatusType.ERROR)
                    {
                        throw new Exception(string.Join("\n", certTypes.Errors?.Select(x => x.message)));
                    }

                    types.Add(new CertCentralCertType
                    {
                        DisplayName = $"{details.Name} {(UnsupportedProductTypes.Contains(details.Name) ? "(Enrollment Unavailable)" : string.Empty)}",
                        multidomain = details.AdditionalDNSNamesAllowed,
                        ProductCode = details.NameId,
                        ShortName = details.Name,
                        ProductType = details.Type,
                        signatureAlgorithm = details.SignatureHashType.DefaultHashTypeId
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError($"RetrieveCertCentralType: Unable to retrieve details for product type: {type.NameId}. Skipping...");
                    Logger.LogTrace($"RetrieveCertCentralType: Type retrieval error details: {ex.Message}");
                }
            }
            return types;
        }

        #endregion Methods
    }
}
