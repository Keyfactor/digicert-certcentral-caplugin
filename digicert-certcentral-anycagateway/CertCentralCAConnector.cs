﻿using Keyfactor.AnyGateway.Extensions;
using Keyfactor.Common;
using Keyfactor.Extensions.CAGateway.DigiCert.API;
using Keyfactor.Extensions.CAGateway.DigiCert.Client;
using Keyfactor.Extensions.CAGateway.DigiCert.Models;
using Keyfactor.Logging;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic;

using Newtonsoft.Json;

using Org.BouncyCastle.Asn1.X509;

using System.Collections.Concurrent;
using System.Runtime.InteropServices;

using static Keyfactor.PKI.PKIConstants.Microsoft;

using CertCentralConstants = Keyfactor.Extensions.CAGateway.DigiCert.Constants;

namespace Keyfactor.Extensions.CAGateway.DigiCert
{
	public class CertCentralCAConnector : ICAConnector
	{
		private CertCentralConfig _config;
		private readonly ILogger _logger;
		private ICertificateDataReader _certificateDataReader;

		private Dictionary<int, string> DCVTokens { get; } = new Dictionary<int, string>();

		public CertCentralCAConnector()
		{
			_logger = LogHandler.GetClassLogger<CertCentralCAConnector>();
		}
		public void Initialize(ICAConnectorConfigProvider configProvider, ICertificateDataReader certificateDataReader)
		{
			_certificateDataReader = certificateDataReader;
			string rawConfig = JsonConvert.SerializeObject(configProvider.CAConnectionData);
			_config = JsonConvert.DeserializeObject<CertCentralConfig>(rawConfig);
		}

		public async Task<EnrollmentResult> Enroll(string csr, string subject, Dictionary<string, string[]> san, EnrollmentProductInfo productInfo, RequestFormat requestFormat, EnrollmentType enrollmentType)
		{
			_logger.MethodEntry(LogLevel.Trace);
			OrderResponse orderResponse = new OrderResponse();
			CertCentralCertType certType = CertCentralCertType.GetAllTypes(_config).FirstOrDefault(x => x.ProductCode.Equals(productInfo.ProductID));
			OrderRequest orderRequest = new OrderRequest(certType);

			var days = (productInfo.ProductParameters.ContainsKey("LifetimeDays")) ? int.Parse(productInfo.ProductParameters["LifetimeDays"]) : 365;
			int validityYears = 0;
			DateTime? customExpirationDate = null;
			switch (days)
			{
				case 365:
				case 730:
				case 1095:
					validityYears = days / 365;
					break;
				default:
					customExpirationDate = DateTime.Now.AddDays(days);
					break;
			}

			List<string> dnsNames = new List<string>();
			if (san.ContainsKey("Dns"))
			{
				dnsNames = new List<string>(san["Dns"]);
			}

			X509Name subjectParsed = null;
			string commonName = null, organization = null, orgUnit = null;
			try
			{
				subjectParsed = new X509Name(subject);
				commonName = subjectParsed.GetValueList(X509Name.CN).Cast<string>().LastOrDefault();
				organization = subjectParsed.GetValueList(X509Name.O).Cast<string>().LastOrDefault();
				orgUnit = subjectParsed.GetValueList(X509Name.OU).Cast<string>().LastOrDefault();
			}
			catch (Exception) { }

			if (commonName == null)
			{
				if (dnsNames.Count > 0)
				{
					commonName = dnsNames[0];
				}
				else
				{
					throw new Exception("No Common Name or DNS SAN provided, unable to enroll");
				}
			}

			if (productInfo.ProductParameters.TryGetValue(CertCentralConstants.RequestAttributes.ORGANIZATION_NAME, out string orgName))
			{
				// If org name is provided as a parameter, it overrides whatever is in the CSR
				organization = orgName;
			}

			string signatureHash = certType.signatureAlgorithm;

			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(_config);
			int? organizationId = null;
			// DV certs have no organization, so only do the org check if its a non-DV cert
			if (!string.Equals(productInfo.ProductID, CertCentralConstants.ProductTypes.DV_SSL_CERT, StringComparison.OrdinalIgnoreCase))
			{
				if (organization == null)
				{
					throw new Exception("No organization provided in either subject or attributes, unable to enroll");
				}

				ListOrganizationsResponse organizations = client.ListOrganizations(new ListOrganizationsRequest());
				if (organizations.Status == CertCentralBaseResponse.StatusType.ERROR)
				{
					_logger.LogError($"Error from CertCentral client: {organizations.Errors.First().message}");
				}

				Organization org = organizations.Organizations.FirstOrDefault(x => x.Name.Equals(organization, StringComparison.OrdinalIgnoreCase));
				if (org != null)
				{
					organizationId = org.Id;
				}
				else
				{
					throw new Exception($"Organization '{organization}' is invalid for this account, please check name");
				}
			}

			// Process metadata fields
			orderRequest.CustomFields = new List<MetadataField>();
			var metadataResponse = client.ListMetadata(new ListMetadataRequest());
			if (metadataResponse.MetadataFields != null && metadataResponse.MetadataFields.Count > 0)
			{
				var metadata = metadataResponse.MetadataFields.Where(m => m.Active).ToList();
				_logger.LogTrace($"Found {metadata.Count()} active metadata fields in the account");
				foreach (var field in metadata)
				{
					// See if the field has been provided in the request
					if (productInfo.ProductParameters.TryGetValue(field.Label, out string fieldValue))
					{
						_logger.LogTrace($"Found {field.Label} in the request, adding...");
						orderRequest.CustomFields.Add(new MetadataField() { MetadataId = field.Id, Value = fieldValue });
					}
				}
			}

			// Get CA Cert ID (if present)
			string caCertId = (productInfo.ProductParameters.ContainsKey("CACertId")) ? (string)productInfo.ProductParameters["CACertId"] : null;

			// Set up request
			orderRequest.Certificate.CommonName = commonName;
			orderRequest.Certificate.CSR = csr;
			orderRequest.Certificate.SignatureHash = signatureHash;
			orderRequest.Certificate.DNSNames = dnsNames;
			orderRequest.Certificate.CACertID = caCertId;
			orderRequest.SetOrganization(organizationId);
			if (!string.IsNullOrEmpty(orgUnit))
			{
				List<string> ous = new List<string>
				{
					orgUnit
				};
				orderRequest.Certificate.OrganizationUnits = ous;
			}

			string dcvMethod = "email";

			// AnyGateway Core does not currently support retreiving DCV tokens, the following code block can be uncommented once support is added.

			//if (productInfo.ProductParameters.TryGetValue(DigiCertConstants.RequestAttributes.DCV_METHOD, out string rawDCV))
			//{
			//	Logger.Trace($"Parsing DCV method: {rawDCV}");
			//	if (rawDCV.IndexOf("mail", StringComparison.OrdinalIgnoreCase) >= 0)
			//	{
			//		Logger.Trace("Selecting DCV method 'email'");
			//		dcvMethod = "email";
			//	}
			//	else if (rawDCV.IndexOf("dns", StringComparison.OrdinalIgnoreCase) >= 0)
			//	{
			//		Logger.Trace("Selecting DCV method 'dns-txt-token'");
			//		dcvMethod = "dns-txt-token";
			//	}
			//	else if (rawDCV.IndexOf("http", StringComparison.OrdinalIgnoreCase) >= 0)
			//	{
			//		Logger.Trace("Selecting DCV method 'http-token'");
			//		dcvMethod = "http-token";
			//	}
			//	else
			//	{
			//		Logger.Warn($"Unexpected DCV method '{rawDCV}'. Falling back to default of 'email'");
			//	}
			//}

			orderRequest.DCVMethod = dcvMethod;

			if (customExpirationDate != null)
			{
				orderRequest.CustomExpirationDate = customExpirationDate;
			}
			else
			{
				orderRequest.ValidityYears = validityYears;
			}

			var renewWindow = (productInfo.ProductParameters.ContainsKey(CertCentralConstants.Config.RENEWAL_WINDOW)) ? int.Parse(productInfo.ProductParameters[CertCentralConstants.Config.RENEWAL_WINDOW]) : 90;
			string priorCertSnString = null;
			string priorCertReqID = null;
			if (enrollmentType == EnrollmentType.RenewOrReissue)
			{
				//// Determine if we're going to do a renew or a reissue.
				//string priorCertSnString = productInfo.ProductParameters["PriorCertSN"];
				//_logger.LogTrace($"Attempting to retrieve the certificate with serial number {priorCertSnString}.");
				//byte[] priorCertSn = DataConversion.HexToBytes(priorCertSnString);
				//CAConnectorCertificate anyGatewayCertificate = _certificateDataReader.(priorCertSn);
				//if (anyGatewayCertificate == null)
				//{
				//	throw new Exception($"No certificate with serial number '{priorCertSnString}' could be found.");
				//}
				enrollmentType = EnrollmentType.Renew;

			}

			// Check if the order has more validity in it (multi-year cert). If so, do a reissue instead of a renew
			if (enrollmentType == EnrollmentType.Renew)
			{
				// Get the old cert so we can properly construct the request.
				priorCertSnString = productInfo.ProductParameters["PriorCertSN"];
				_logger.LogTrace($"Attempting to retrieve the certificate with serial number {priorCertSnString}.");
				priorCertReqID = await _certificateDataReader.GetRequestIDBySerialNumber(priorCertSnString);
				if (string.IsNullOrEmpty(priorCertReqID))
				{
					throw new Exception($"No certificate with serial number '{priorCertSnString}' could be found.");
				}

				// Get order ID
				_logger.LogTrace("Attempting to parse the order ID from the AnyGateway certificate.");
				uint orderId = 0;
				try
				{
					orderId = uint.Parse(priorCertReqID.Split('-').First());
				}
				catch (Exception e)
				{
					throw new Exception($"There was an error parsing the order ID from the certificate: {e.Message}", e);
				}

				ViewCertificateOrderResponse certOrder = client.ViewCertificateOrder(new ViewCertificateOrderRequest(orderId));

				if (certOrder.order_valid_till.HasValue && certOrder.order_valid_till.Value.AddDays(renewWindow * -1) > DateTime.UtcNow)
				{
					enrollmentType = EnrollmentType.Reissue;
				}
			}


			_logger.LogTrace("Making request to Enroll");

			switch (enrollmentType)
			{
				case EnrollmentType.New:
					return await NewCertificate(client, orderRequest, commonName);

				case EnrollmentType.Reissue:
					return await Reissue(client, productInfo, priorCertReqID, commonName, csr, dnsNames, signatureHash, caCertId);

				case EnrollmentType.Renew:
					return await Renew(client, orderRequest, productInfo, priorCertReqID, commonName);

				default:
					throw new Exception($"The enrollment type '{enrollmentType}' is invalid for the DigiCert gateway.");
			}
		}

		public Dictionary<string, PropertyConfigInfo> GetCAConnectorAnnotations()
		{
			return new Dictionary<string, PropertyConfigInfo>()
			{
				[CertCentralConstants.Config.APIKEY] = new PropertyConfigInfo()
				{
					Comments = "API Key for connecting to DigiCert",
					Hidden = true,
					DefaultValue = ""
				},
				[CertCentralConstants.Config.DIVISION_ID] = new PropertyConfigInfo()
				{
					Comments = "Division ID to use for retrieving product details (only if account is configured with per-divison product settings)",
					Hidden = false,
					DefaultValue = ""
				},
				[CertCentralConstants.Config.REGION] = new PropertyConfigInfo()
				{
					Comments = "The geographic region that your DigiCert CertCentral account is in. Valid options are US and EU.",
					Hidden = false,
					DefaultValue = "US"
				},
				[CertCentralConstants.Config.REVOKE_CERT] = new PropertyConfigInfo()
				{
					Comments = "Default DigiCert behavior on revocation requests is to revoke the entire order. If this value is changed to 'true', revocation requests will instead just revoke the individual certificate.",
					Hidden = false,
					DefaultValue = "false"
				}
			};
		}

		public List<string> GetProductIds()
		{
			try
			{
				string authAPIKey = _config.APIKey;
				string region = _config.Region;
				CertCentralClient client = new CertCentralClient(authAPIKey, region);

				// Get product types.
				CertificateTypesResponse productTypesResponse = client.GetAllCertificateTypes();

				// If we couldn't get the types, return an empty comment.
				if (productTypesResponse.Status != CertCentralBaseResponse.StatusType.SUCCESS)
				{
					throw new Exception("Unable to retrieve product list");
				}

				return productTypesResponse.Products.Select(x => x.NameId).ToList();			}
			catch (Exception ex)
			{
				// Swallow exceptions and return an empty string.
				_logger.LogError($"Unable to retrieve product list: {ex.Message}");
				throw;
			}
		}

		public async Task<CAConnectorCertificate> GetSingleRecord(string caRequestID)
		{
			_logger.MethodEntry(LogLevel.Trace);
			// Split ca request id into order and cert id
			string[] idParts = caRequestID.Split('-');
			int orderId = Int32.Parse(idParts.First());
			string certId = idParts.Last();
			int certIdInt = Int32.Parse(certId);

			// Get status of cert and the cert itself from Digicert
			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(_config);
			ViewCertificateOrderResponse orderResponse = client.ViewCertificateOrder(new ViewCertificateOrderRequest((uint)orderId));

			var orderCerts = GetAllCertsForOrder(orderId);

			StatusOrder certToCheck = orderCerts.Where(c => c.certificate_id == certIdInt).First();

			string certificate = null;
			int status = GetCertificateStatusFromCA(certToCheck.status, orderId);
			if (status == (int)RequestDisposition.ISSUED || status == (int)RequestDisposition.REVOKED || status == (int)RequestDisposition.UNKNOWN)
			{
				// We have a status where there may be a cert to download, try to download it
				CertificateChainResponse certificateChainResponse = client.GetCertificateChain(new CertificateChainRequest(certId));
				if (certificateChainResponse.Status == CertCentralBaseResponse.StatusType.SUCCESS)
				{
					certificate = certificateChainResponse.Intermediates[0].PEM;
				}
				else
				{
					_logger.LogWarning($"Unexpected error downloading certificate {certId} for order {orderId}: {certificateChainResponse.Errors.FirstOrDefault()?.message}");
				}
			}
			_logger.MethodExit(LogLevel.Trace);
			return new CAConnectorCertificate
			{
				CARequestID = caRequestID,
				Certificate = certificate,
				Status = status,
				ProductID = orderResponse.product.name_id,
				RevocationDate = GetRevocationDate(orderResponse)
			};
		}

		public Dictionary<string, PropertyConfigInfo> GetTemplateParameterAnnotations()
		{
			return new Dictionary<string, PropertyConfigInfo>()
			{
				[CertCentralConstants.Config.LIFETIME] = new PropertyConfigInfo()
				{
					Comments = "OPTIONAL: The number of days of validity to use when requesting certs. If not provided, default is 365.",
					Hidden = false,
					DefaultValue = "365"
				},
				[CertCentralConstants.Config.CA_CERT_ID] = new PropertyConfigInfo()
				{
					Comments = "OPTIONAL: ID of issuing CA to use by DigiCert. If not provided, the default for your account will be used.",
					Hidden = false,
					DefaultValue = ""
				},
				[CertCentralConstants.RequestAttributes.ORGANIZATION_NAME] = new PropertyConfigInfo()
				{
					Comments = "OPTIONAL: For requests that will not have a subject (such as ACME) you can use this field to provide the organization name. Value supplied here will override any CSR values, so do not include this field if you want the organization from the CSR to be used.",
					Hidden = false,
					DefaultValue = ""
				},
				[CertCentralConstants.Config.RENEWAL_WINDOW] = new PropertyConfigInfo()
				{
					Comments = "OPTIONAL: The number of days from certificate expiration that the gateway should do a renewal rather than a reissue. If not provided, default is 90.",
					Hidden = false,
					DefaultValue = "90"
				}
			};
		}

		public async Task Ping()
		{
			_logger.MethodEntry(LogLevel.Trace);
			try
			{
				CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(_config);

				_logger.LogDebug("Attempting to ping DigiCert API.");
				ListDomainsResponse response = client.ListDomains(new ListDomainsRequest());

				if (response.Errors.Count > 0)
				{
					throw new Exception($"Error attempting to ping DigiCert: {string.Join("\n", response.Errors)}");
				}

				_logger.LogDebug("Successfully pinged DigiCert API.");
			}
			catch (Exception e)
			{
				_logger.LogError($"There was an error contacting DigiCert: {e.Message}.");
				throw new Exception($"Error attempting to ping DigiCert: {e.Message}.", e);
			}
			_logger.MethodExit(LogLevel.Trace);
		}

		public async Task<int> Revoke(string caRequestID, string hexSerialNumber, uint revocationReason)
		{
			_logger.MethodEntry(LogLevel.Trace);
			int orderId = Int32.Parse(caRequestID.Substring(0, caRequestID.IndexOf('-')));
			int certId = Int32.Parse(caRequestID.Substring(caRequestID.IndexOf('-') + 1));
			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(_config);
			ViewCertificateOrderResponse orderResponse = client.ViewCertificateOrder(new ViewCertificateOrderRequest((uint)orderId));
			if (orderResponse.Status == CertCentralBaseResponse.StatusType.ERROR || orderResponse.status.ToLower() != "issued")
			{
				string errorMessage = String.Format("Request {0} was not found in CertCentral database or is not valid", orderId);
				_logger.LogInformation(errorMessage);
				throw new COMException(errorMessage, HRESULTs.PROP_NOT_FOUND);
			}
			string req = "";
			RequestSummary request_temp = orderResponse.requests.FirstOrDefault(x => x.status == "approved");
			if (request_temp != null && !String.IsNullOrEmpty(request_temp.comments) && request_temp.comments.Contains("CERTIFICATE_REQUESTOR:"))
			{
				req = request_temp.comments.Replace("CERTIFICATE_REQUESTOR:", "").Trim();
			}
			_logger.LogTrace("Making request to Revoke");
			RevokeCertificateResponse revokeResponse;
			if (_config.RevokeCertificateOnly.HasValue && _config.RevokeCertificateOnly.Value)
			{
				revokeResponse = client.RevokeCertificate(new RevokeCertificateRequest(certId) { comments = Conversions.RevokeReasonToString(revocationReason) });
			}
			else
			{
				revokeResponse = client.RevokeCertificate(new RevokeCertificateByOrderRequest(orderResponse.id) { comments = Conversions.RevokeReasonToString(revocationReason) });
			}

			if (revokeResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				string errMsg = $"Unable to revoke certificate {caRequestID}. Error(s): {string.Join(";", revokeResponse.Errors.Select(e => e.message))}";
				_logger.LogError(errMsg);
				throw new Exception(errMsg);
			}

			var updateRequest = client.UpdateRequestStatus(new UpdateRequestStatusRequest(revokeResponse.request_id) { Status = "approved" });

			_logger.MethodExit(LogLevel.Trace);
			if (updateRequest.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				string errMsg = $"Unable to approve revocation request. Manual approval through the DigiCert portal required. Verify that the gateway API key has administrator rights for future revocations.";
				_logger.LogError(errMsg);
				throw new Exception(errMsg);
			}
			return (int)RequestDisposition.REVOKED;
		}

		public async Task Synchronize(BlockingCollection<CAConnectorCertificate> blockingBuffer, DateTime? lastSync, bool fullSync, CancellationToken cancelToken)
		{
			_logger.MethodEntry(LogLevel.Trace);

			lastSync = lastSync.HasValue ? lastSync.Value.AddHours(-7) : DateTime.MinValue; // DigiCert issue with treating the timezone as mountain time. -7 to accomodate DST
			DateTime? utcDate = DateTime.UtcNow.AddDays(1);
			string lastSyncFormat = FormatSyncDate(lastSync);
			string todaySyncFormat = FormatSyncDate(utcDate);

			List<CAConnectorCertificate> certs = new List<CAConnectorCertificate>();
			List<StatusOrder> certsToSync = new List<StatusOrder>();

			_logger.LogDebug("Attempting to create a CertCentral client");
			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(_config);

			List<string> skippedOrders = new List<string>();
			int certCount = 0;

			if (fullSync)
			{
				ListCertificateOrdersResponse ordersResponse = client.ListAllCertificateOrders();
				if (ordersResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
				{
					Error error = ordersResponse.Errors[0];
					_logger.LogError("Error in listing all certificate orders");
					throw new Exception($"DigiCert CertCentral web service returned {error.code} - {error.message} when retrieving all rows");
				}
				else
				{
					foreach (var orderDetails in ordersResponse.orders)
					{
						List<CAConnectorCertificate> orderCerts = new List<CAConnectorCertificate>();
						try
						{
							cancelToken.ThrowIfCancellationRequested();
							string caReqId = orderDetails.id + "-" + orderDetails.certificate.id;
							orderCerts = GetAllConnectorCertsForOrder(caReqId);
						}
						catch
						{
							skippedOrders.Add(orderDetails.id.ToString());
							_logger.LogWarning($"An error occurred attempting to sync order '{orderDetails.id}'. This order will be skipped.");
							continue;
						}

						foreach (var cert in orderCerts)
						{
							certCount++;
							blockingBuffer.Add(cert);
						}

					}
				}
			}
			else
			{
				StatusChangesResponse statusChangesResponse = client.StatusChanges(new StatusChangesRequest(lastSyncFormat, todaySyncFormat));
				if (statusChangesResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
				{
					Error error = statusChangesResponse.Errors[0];
					_logger.LogError("Error in retrieving list of status changes for partial sync");
					throw new Exception($"DigiCert CertCentral web service returned {error.code} - {error.message} when retrieving list of status changes");
				}
				if (statusChangesResponse.orders?.Count > 0)
				{
					int orderCount = statusChangesResponse.orders.Count;
					foreach (var order in statusChangesResponse.orders)
					{
						List<CAConnectorCertificate> orderCerts = new List<CAConnectorCertificate>();
						try
						{
							cancelToken.ThrowIfCancellationRequested();
							string caReqId = order.order_id + "-" + order.certificate_id;
							orderCerts = GetAllConnectorCertsForOrder(caReqId);
						}
						catch
						{
							skippedOrders.Add(order.order_id.ToString());
							_logger.LogWarning($"An error occurred attempting to sync order '{order.order_id}'. This order will be skipped.");
							continue;
						}
						foreach (var cert in orderCerts)
						{
							certCount++;
							blockingBuffer.Add(cert);
						}
					}
				}
			}

			if (cancelToken.IsCancellationRequested)
			{
				_logger.LogInformation("DigiCert CertCentral sync cancelled.");

				cancelToken.ThrowIfCancellationRequested();
			}

			if (skippedOrders?.Count > 0)
			{
				_logger.LogInformation($"Sync skipped the following orders: {string.Join(",", skippedOrders.ToArray())}");
			}
			_logger.LogInformation($"Sync complete with {certCount} certificates");
			_logger.MethodExit(LogLevel.Trace);
		}

		public async Task ValidateCAConnectionInfo(Dictionary<string, object> connectionInfo)
		{
			_logger.MethodEntry(LogLevel.Trace);
			List<string> errors = new List<string>();

			_logger.LogTrace("Checking the API Key.");
			string apiKey = connectionInfo.ContainsKey(CertCentralConstants.Config.APIKEY) ? (string)connectionInfo[CertCentralConstants.Config.APIKEY] : string.Empty;
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				errors.Add("The API Key is required.");
			}

			_logger.LogTrace("Checking the region.");
			string region = "US";
			if (connectionInfo.ContainsKey(CertCentralConstants.Config.REGION))
			{
				region = (string)connectionInfo[CertCentralConstants.Config.REGION];
				List<string> validRegions = new List<string> { "US", "EU" };
				if (string.IsNullOrWhiteSpace(region) || !validRegions.Contains(region.ToUpper()))
				{
					errors.Add($"Region must be one of the following values if provided: {string.Join(",", validRegions)}");
				}
			}
			else
			{
				_logger.LogTrace("Region not specified, using US default");
			}

			CertCentralClient digiClient = new CertCentralClient(apiKey, region);
			ListDomainsResponse domains = digiClient.ListDomains(new ListDomainsRequest());
			_logger.LogDebug("Domain Status: " + domains.Status);
			if (domains.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				_logger.LogError($"Error from CertCentral client: {domains.Errors[0].message}");
				errors.Add("Error grabbing DigiCert domains");
			}
			_logger.MethodExit(LogLevel.Trace);
			// We cannot proceed if there are any errors.
			if (errors.Any())
			{
				ThrowValidationException(errors);
			}
		}

		private void ThrowValidationException(List<string> errors)
		{
			throw new ArgumentException(string.Join("\n", errors));
		}

		public async Task ValidateProductInfo(EnrollmentProductInfo productInfo, Dictionary<string, object> connectionInfo)
		{
			_logger.MethodEntry(LogLevel.Trace);
			// Set up.
			string productId = productInfo.ProductID;
			string apiKey = (string)connectionInfo[CertCentralConstants.Config.APIKEY];
			string region = "US";
			if (connectionInfo.ContainsKey(CertCentralConstants.Config.REGION))
			{
				var r = (string)connectionInfo[CertCentralConstants.Config.REGION];
				if (!string.IsNullOrWhiteSpace(r))
				{
					region = r;
				}
			}
			CertCentralClient client = new CertCentralClient(apiKey, region);

			// Get the available types and check that it's one of them.
			// We're doing this because to get the list of valid product IDs in a comment, the user must have at least one correct product/template mapping.
			// We therefore need to have some way of telling them what the valid product IDs are to begin with.
			CertificateTypesResponse productIdResponse = client.GetAllCertificateTypes();
			if (productIdResponse.Status != CertCentralBaseResponse.StatusType.SUCCESS)
			{
				throw new Exception($"The product types could not be retrieved from the server. The following errors occurred: {string.Join(" ", productIdResponse.Errors.Select(x => x.message))}");
			}

			// Get product and check if it exists.
			var product = productIdResponse.Products.FirstOrDefault(x => x.NameId.Equals(productId, StringComparison.InvariantCultureIgnoreCase));
			if (product == null)
			{
				throw new Exception($"The product ID '{productId}' does not exist. The following product IDs are valid: {string.Join(", ", productIdResponse.Products.Select(x => x.NameId))}");
			}

			// Get product ID details.
			CertificateTypeDetailsRequest detailsRequest = new CertificateTypeDetailsRequest(product.NameId);

			detailsRequest.ContainerId = null;
			if (connectionInfo.ContainsKey(CertCentralConstants.Config.DIVISION_ID))
			{
				int.TryParse((string)connectionInfo[CertCentralConstants.Config.DIVISION_ID], out int divId);
				detailsRequest.ContainerId = divId;
			}

			CertificateTypeDetailsResponse details = client.GetCertificateTypeDetails(detailsRequest);
			if (details.Errors.Any())
			{
				throw new Exception($"Validation of '{productId}' failed for the following reasons: {string.Join(" ", details.Errors.Select(x => x.message))}.");
			}
			_logger.MethodExit(LogLevel.Trace);
		}


		/// <summary>
		/// Enrolls for a new certificate.
		/// </summary>
		/// <param name="client">The client that makes requests to DigiCert.</param>
		/// <param name="request">The request to order a certificate.</param>
		/// <param name="commonName">The common name.</param>
		/// <returns>The <see cref="EnrollmentResult"/> containing the result of the enrollment request</returns>
		private async Task<EnrollmentResult> NewCertificate(CertCentralClient client, OrderRequest request, string commonName)
		{
			_logger.LogTrace("Attempting to enroll for a certificate.");
			return await ExtractEnrollmentResult(client, client.OrderCertificate(request), commonName);
		}


		/// <summary>
		/// Gets the enrollment result from an <see cref="OrderResponse"/> object.
		/// </summary>
		private async Task<EnrollmentResult> ExtractEnrollmentResult(CertCentralClient client, OrderResponse orderResponse, string commonName)
		{
			int status = (int)RequestDisposition.UNKNOWN;
			string statusMessage = null;
			string certificate = null;
			string caRequestID = null;

			if (orderResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				_logger.LogError($"Error from CertCentral client: {orderResponse.Errors.First().message}");

				status = (int)RequestDisposition.FAILED;
				statusMessage = orderResponse.Errors[0].message;
			}
			else if (orderResponse.Status == CertCentralBaseResponse.StatusType.SUCCESS)
			{
				uint orderID = (uint)orderResponse.OrderId;
				ViewCertificateOrderResponse certificateOrderResponse = client.ViewCertificateOrder(new ViewCertificateOrderRequest(orderID));
				if (certificateOrderResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
				{
					string errorMessage = $"Order {orderID} was not found for rejection in CertCentral database";
					_logger.LogInformation(errorMessage);
					throw new Exception(errorMessage);
				}

				status = GetCertificateStatusFromCA(certificateOrderResponse.status, (int)orderID);

				// Get cert from response
				if (orderResponse.CertificateChain != null)
				{
					_logger.LogTrace($"Certificate for order {orderResponse.OrderId} was immediately issued");
					string certPem = orderResponse.CertificateChain.SingleOrDefault(c => c.SubjectCommonName.Equals(commonName, StringComparison.OrdinalIgnoreCase))?.PEM;
					if (string.IsNullOrEmpty(certPem))
					{
						_logger.LogWarning($"Order {orderResponse.OrderId} was for Common Name '{commonName}', but no certificate with that Common Name was returned");
					}

					certificate = certPem;
					caRequestID = orderResponse.OrderId.ToString() + "-" + orderResponse.CertificateId;
				}
				else if (orderResponse.CertificateId.HasValue)
				{
					_logger.LogTrace($"Certificate for order {orderResponse.OrderId} is being processed by DigiCert. Most likely a domain/organization requires further validation");
					if (!string.IsNullOrEmpty(orderResponse.DCVRandomValue))
					{
						_logger.LogDebug($"Saving DCV token for order {orderResponse.OrderId}");
						DCVTokens[orderResponse.OrderId] = orderResponse.DCVRandomValue;
					}

					caRequestID = orderResponse.OrderId.ToString() + "-" + orderResponse.CertificateId;
				}
				else // We should really only get here if there is a misconfiguration (e.g. set up for approval in DigiCert)
				{
					_logger.LogWarning($"Order {orderResponse.OrderId} did not return a CertificateId. Manual intervention may be required");
					if (orderResponse.Requests.Any(x => x.Status == CertCentralConstants.Status.PENDING))
					{
						_logger.LogTrace($"Attempting to approve order '{orderResponse.OrderId}'.");

						// Attempt to update the request status.
						int requestId = int.Parse(orderResponse.Requests.FirstOrDefault(x => x.Status == CertCentralConstants.Status.PENDING).Id);
						UpdateRequestStatusRequest updateStatusRequest = new UpdateRequestStatusRequest(requestId, CertCentralConstants.Status.APPROVED);
						UpdateRequestStatusResponse updateStatusResponse = client.UpdateRequestStatus(updateStatusRequest);

						if (updateStatusResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
						{
							string errors = string.Join(" ", updateStatusResponse.Errors.Select(x => x.message));
							_logger.LogError($"The order '{orderResponse.OrderId}' could not be approved: '{errors}");

							caRequestID = orderResponse.OrderId.ToString();
							if (updateStatusResponse.Errors.Any(x => x.code == "access_denied|invalid_approver"))
							{
								status = (int)RequestDisposition.EXTERNAL_VALIDATION;
								statusMessage = errors;
							}
							else
							{
								status = (int)RequestDisposition.FAILED;
								statusMessage = $"Approval of order '{orderResponse.OrderId}' failed. Check the gateway logs for more details.";
							}
						}
						else // If the request was successful, we attempt to retrieve the certificate.
						{
							ViewCertificateOrderResponse order = client.ViewCertificateOrder(new ViewCertificateOrderRequest((uint)orderResponse.OrderId));

							// We don't worry about failures here, since the sync will update the cert if we can't get it right now for some reason.
							if (order.Status != CertCentralBaseResponse.StatusType.ERROR)
							{
								caRequestID = $"{order.id}-{order.certificate.id}";
								try
								{
									CAConnectorCertificate connCert = await GetSingleRecord($"{order.id}-{order.certificate.id}");
									certificate = connCert.Certificate;
									status = connCert.Status;
									statusMessage = $"Post-submission approval of order {order.id} returned success";
								}
								catch (Exception getRecordEx)
								{
									_logger.LogWarning($"Unable to retrieve certificate {order.certificate.id} for order {order.id}: {getRecordEx.Message}");
									status = (int)RequestDisposition.UNKNOWN;
									statusMessage = $"Post-submission approval of order {order.id} was successful, but pickup failed";
								}
							}
						}
					}
					else
					{
						_logger.LogWarning("The request disposition is for this enrollment could not be determined.");
						status = (int)RequestDisposition.UNKNOWN;
						statusMessage = "The request disposition could not be determined.";
					}
				}
			}
			return new EnrollmentResult
			{
				CARequestID = caRequestID,
				Certificate = certificate,
				Status = status,
				StatusMessage = statusMessage
			};
		}

		private int GetCertificateStatusFromCA(string status, int orderId)
		{
			switch (status)
			{
				case "issued":
				case "approved":
				case "expired":
					return (int)RequestDisposition.ISSUED;

				case "processing":
				case "reissue_pending":
				case "pending": // Pending from DigiCert means it will be issued after validation
					return (int)RequestDisposition.EXTERNAL_VALIDATION;

				case "denied":
					return (int)RequestDisposition.DENIED;

				case "revoked":
					return (int)RequestDisposition.REVOKED;

				case "needs_approval": // This indicates that the request has to be approved through DigiCert, which is a misconfiguration
					_logger.LogWarning($"Order {orderId} needs to be approved in the DigiCert portal prior to issuance");
					return (int)RequestDisposition.EXTERNAL_VALIDATION;

				default:
					_logger.LogWarning($"Order {orderId} has unexpected status {status}");
					return (int)RequestDisposition.UNKNOWN;
			}
		}

		private List<StatusOrder> GetReissues(CertCentralClient digiClient, int orderId)
		{
			_logger.LogTrace($"Getting Reissues for order {orderId}");
			List<StatusOrder> reissueCerts = new List<StatusOrder>();
			ListReissueResponse reissueResponse = digiClient.ListReissues(new ListReissueRequest(orderId));
			if (reissueResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				Error error = reissueResponse.Errors[0];
				_logger.LogError($"Error in retrieving reissues for order {orderId}");
				throw new Exception($"DigiCert CertCentral Web Service returned {error.code} - {error.message} to retrieve all rows");
			}
			if (reissueResponse.certificates?.Count > 0)
			{
				foreach (CertificateOrder reissueCert in reissueResponse.certificates)
				{
					StatusOrder reissueStatusOrder = new StatusOrder
					{
						order_id = orderId,
						certificate_id = reissueCert.id,
						status = reissueCert.status
					};
					reissueCerts.Add(reissueStatusOrder);
				}
			}

			return reissueCerts;
		}

		private List<StatusOrder> GetDuplicates(CertCentralClient digiClient, int orderId)
		{
			_logger.LogTrace($"Getting Duplicates for order {orderId}");
			List<StatusOrder> dupeCerts = new List<StatusOrder>();
			ListDuplicatesResponse duplicateResponse = digiClient.ListDuplicates(new ListDuplicatesRequest(orderId));
			if (duplicateResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				Error error = duplicateResponse.Errors[0];
				_logger.LogError($"Error in retrieving duplicates for order {orderId}");
				throw new Exception($"DigiCert CertCentral Web Service returned {error.code} - {error.message} to retreive all rows");
			}
			if (duplicateResponse.certificates?.Count > 0)
			{
				foreach (CertificateOrder dupeCert in duplicateResponse.certificates)
				{
					StatusOrder dupeStatusOrder = new StatusOrder
					{
						order_id = orderId,
						certificate_id = dupeCert.id,
						status = dupeCert.status
					};
					dupeCerts.Add(dupeStatusOrder);
				}
			}

			return dupeCerts;
		}

		/// <summary>
		/// Get the revocation date tied to a certificate order.
		/// </summary>
		/// <param name="order">The order we seek the revocation date for.</param>
		/// <returns></returns>
		private DateTime? GetRevocationDate(ViewCertificateOrderResponse order)
		{
			if (order.Status != CertCentralBaseResponse.StatusType.SUCCESS)
			{
				_logger.LogWarning($"Could not retrieve the revocation date for order '{order.id}'. This may cause problems syncing with Command.");
				return null;
			}

			RequestSummary revokeRequest = order.requests.FirstOrDefault(x => x.type.Equals("revoke", StringComparison.OrdinalIgnoreCase) &&
				"approved".Equals(x.status, StringComparison.OrdinalIgnoreCase));
			if (revokeRequest == null)
			{
				if ("revoked".Equals(order.status, StringComparison.OrdinalIgnoreCase))
				{
					_logger.LogWarning($"Order '{order.id}' is revoked, but lacks a revoke request and revocation date. This may cause problems syncing with Command.");
				}

				return null;
			}

			return revokeRequest.date;
		}

		/// <summary>
		/// Renews a certificate.
		/// </summary>
		/// <param name="client">The client used to contact DigiCert.</param>
		/// <param name="request">The <see cref="OrderRequest"/>.</param>
		/// <param name="enrollmentProductInfo">Information about the DigiCert product this certificate uses.</param>
		/// <returns></returns>
		private async Task<EnrollmentResult> Reissue(CertCentralClient client, EnrollmentProductInfo enrollmentProductInfo, string caRequestId, string commonName, string csr, List<string> dnsNames, string signatureHash, string caCertId)
		{
			CheckProductExistence(enrollmentProductInfo.ProductID);

			// Get order ID
			_logger.LogTrace("Attempting to parse the order ID from the AnyGateway certificate.");
			uint orderId = 0;
			try
			{
				orderId = uint.Parse(caRequestId.Split('-').First());
			}
			catch (Exception e)
			{
				throw new Exception($"There was an error parsing the order ID from the certificate: {e.Message}", e);
			}

			// Reissue certificate.
			ReissueRequest reissueRequest = new ReissueRequest(orderId)
			{
				Certificate = new CertificateReissueRequest
				{
					CommonName = commonName,
					CSR = csr,
					DnsNames = dnsNames,
					SignatureHash = signatureHash,
					CACertID = caCertId
				},
				// Setting SkipApproval to true to allow certificate id to return a value. See DigiCert documentation on Reissue API call for more info.
				SkipApproval = true
			};

			_logger.LogTrace("Attempting to reissue certificate.");
			return await ExtractEnrollmentResult(client, client.ReissueCertificate(reissueRequest), commonName);
		}

		private void CheckProductExistence(string productId)
		{
			// Check that the product type is still valid.
			_logger.LogTrace($"Checking that the product '{productId}' exists.");
			CertCentralCertType productType = CertCentralCertType.GetAllTypes(_config).FirstOrDefault(x => x.ProductCode.Equals(productId, StringComparison.InvariantCultureIgnoreCase));
			if (productType == null)
			{
				throw new Exception($"The product type '{productId}' does not exist.");
			}
		}

		/// <summary>
		/// Renews a certificate.
		/// </summary>
		/// <param name="client">The client used to contact DigiCert.</param>
		/// <param name="request">The <see cref="OrderRequest"/>.</param>
		/// <param name="enrollmentProductInfo">Information about the DigiCert product this certificate uses.</param>
		/// <returns></returns>
		private async Task<EnrollmentResult> Renew(CertCentralClient client, OrderRequest request, EnrollmentProductInfo enrollmentProductInfo, string caRequestId, string commonName)
		{
			CheckProductExistence(enrollmentProductInfo.ProductID);

			int orderId = 0;
			_logger.LogTrace("Parsing the order ID from the database certificate.");
			try
			{
				orderId = int.Parse(caRequestId.Split('-').First());
			}
			catch (Exception e)
			{
				throw new Exception($"There was an error parsing the order ID from the certificate: {e.Message}", e);
			}

			request.RenewalOfOrderId = orderId;

			_logger.LogTrace($"Attempting to renew certificate with order id {orderId}.");
			return await ExtractEnrollmentResult(client, client.OrderCertificate(request), commonName);
		}

		string FormatSyncDate(DateTime? syncTime)
		{
			string date = syncTime.Value.Year + "-" + syncTime.Value.Month + "-" + syncTime.Value.Day;
			string time = syncTime.Value.TimeOfDay.Hours + ":" + syncTime.Value.TimeOfDay.Minutes + ":" + syncTime.Value.TimeOfDay.Seconds;
			return date + "+" + time;
		}


		private List<CAConnectorCertificate> GetAllConnectorCertsForOrder(string caRequestID)
		{
			_logger.MethodEntry(LogLevel.Trace);
			// Split ca request id into order and cert id
			string[] idParts = caRequestID.Split('-');
			int orderId = Int32.Parse(idParts.First());
			string certId = idParts.Last();
			int certIdInt = Int32.Parse(certId);

			// Get status of cert and the cert itself from Digicert
			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(_config);
			ViewCertificateOrderResponse orderResponse = client.ViewCertificateOrder(new ViewCertificateOrderRequest((uint)orderId));

			var orderCerts = GetAllCertsForOrder(orderId);

			List<CAConnectorCertificate> certList = new List<CAConnectorCertificate>();

			foreach (var cert in orderCerts)
			{
				string certificate = null;
				string caReqId = cert.order_id + "-" + cert.certificate_id;
				int status = GetCertificateStatusFromCA(cert.status, orderId);
				if (status == (int)RequestDisposition.ISSUED || status == (int)RequestDisposition.REVOKED || status == (int)RequestDisposition.UNKNOWN)
				{
					// We have a status where there may be a cert to download, try to download it
					CertificateChainResponse certificateChainResponse = client.GetCertificateChain(new CertificateChainRequest(certId));
					if (certificateChainResponse.Status == CertCentralBaseResponse.StatusType.SUCCESS)
					{
						certificate = certificateChainResponse.Intermediates[0].PEM;
					}
					else
					{
						_logger.LogWarning($"Unexpected error downloading certificate {certId} for order {orderId}: {certificateChainResponse.Errors.FirstOrDefault()?.message}");
					}
				}
				var connCert = new CAConnectorCertificate
				{
					CARequestID = caReqId,
					Certificate = certificate,
					Status = status,
					ProductID = orderResponse.product.name_id,
					RevocationDate = GetRevocationDate(orderResponse)
				};
				certList.Add(connCert);
			}
			return certList;
		}
		private List<StatusOrder> GetAllCertsForOrder(int orderId)
		{
			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(_config);
			ViewCertificateOrderResponse orderResponse = client.ViewCertificateOrder(new ViewCertificateOrderRequest((uint)orderId));
			if (orderResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				string errorMessage = String.Format("Request {0} was not found in CertCentral database or is not valid", orderId);
				_logger.LogInformation(errorMessage);
				throw new COMException(errorMessage, HRESULTs.PROP_NOT_FOUND);
			}
			List<StatusOrder> reissueCerts = new List<StatusOrder>(), dupeCerts = new List<StatusOrder>();
			try
			{
				reissueCerts = GetReissues(client, orderId);
			}
			catch { }
			try
			{
				dupeCerts = GetDuplicates(client, orderId);
			}
			catch { }

			var orderStatusString = (string.IsNullOrEmpty(orderResponse.certificate.status)) ? orderResponse.status : orderResponse.certificate.status;
			StatusOrder primary = new StatusOrder
			{
				order_id = orderId,
				certificate_id = orderResponse.certificate.id,
				status = orderStatusString
			};
			List<StatusOrder> orderCerts = new List<StatusOrder>();
			orderCerts.Add(primary);
			if (reissueCerts?.Count > 0)
			{
				orderCerts.AddRange(reissueCerts);
			}
			if (dupeCerts?.Count > 0)
			{
				orderCerts.AddRange(dupeCerts);
			}
			return orderCerts;
		}
	}
}