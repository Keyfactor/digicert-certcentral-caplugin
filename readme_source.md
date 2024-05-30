# Introduction
This AnyCA REST Gateway plug-in enables issuance, revocation, and synchronization of certificates from DigiCert's CertCentral offering.  
# Prerequisites

## Certificate Chain

In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you create your Root and/or Subordinate CA, make sure to import the certificate chain into the AnyGateway and Command Server certificate store

## Compatibility
The DigiCert AnyCA plugin is compatible with the Keyfactor AnyCA Gateway REST 24.2 and later


## Installation
1. Download latest successful build from [GitHub Releases](../../releases/latest)

2. Copy DigicertCAPlugin.dll and DigicertCAPlugin.deps.json to the Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net6.0\Extensions directory

3. Update the manifest.json file located in Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net6.0\Extensions\Connectors
  * If the manifest.json file or the Connectors folder do not exist, create them.
```json
{  
	"extensions": {  
		"Keyfactor.AnyGateway.Extensions.IAnyCAPlugin": {  
			"CertCentralCAPlugin": {  
				"assemblypath": "../DigicertCAPlugin.dll",  
				"TypeFullName": "Keyfactor.Extensions.CAPlugin.DigiCert.CertCentralCAPlugin"  
			}  
		}  
	}  
}
```

4. Restart the AnyCA Gateway service

5. Navigate to the AnyCA Gateway REST portal and verify that the Gateway recognizes the GoDaddy plugin by hovering over the ⓘ symbol to the right of the Gateway on the top left of the portal.


## Configuration

1. Follow the official AnyCA Gateway REST documentation to define a new Certificate Authority, using the following information to configure the CA Connection section:

	* Enabled - whether the DigiCert gateway should be enabled or not. Should almost always be set to 'true'
	* APIKey - the API key the Gateway should use to communicate with the DigiCert API. Can be generated from the DigiCert portal.
	* Region - (Optional) The geographic region associated with your DigiCert account. Valid values are US and EU. If not provided, default of US is used.
	* DivisionId - (Optional) If your CertCentral account has multiple divisions AND uses any custom per-division product settings, provide a division ID for the gateway to use for enrollment. Otherwise, omit this setting. NOTE: Division ID is currently only use for product type lookups, it will not affect any other gateway functionality
	* RevokeCertificateOnly - (Optional) By default, when revoking a certificate through DigiCert, the entire order gets revoked. Set this value to 'true' if you want to only revoke individual certificates instead.
	* SyncCAFilter - (Optional) If you list one or more issuing CA IDs here from DigiCert, the sync process will only return certs issued by one of those CAs. Leave this option out to sync all certs from all CAs.
	* FilterExpiredOrders - (Optional) If set to 'true', syncing will apply a filter to NOT return certs that are not expired, or only recently expired. See the next configuration value to set that window. Setting this to 'false' will return all certs regardless of expiration.
	* SyncExpirationDays - (Optional) Only used if FilterExpiredOrders is set to 'true'. Specifies the number of days in the past to sync expired certs. For example, a value of 30 means sync will continue to return certs that have expired within the past 30 days. The default value if not specified is 0, meaning sync would not return any certs expired before the current day.


2. Follow the official AnyCA Gateway REST documentation to define one or more Certificate Profiles. These are what will show up as Templates in Keyfactor Command. You need at least one profile for each product type you wish to be able to enroll for. It is recommended to include the product type in the profile name to make them easier to identify. Use the following information to configure each profile:

	* LifetimeDays - (Optional) The number of days of validity to use when requesting certs. If not specified, the default of 365 will be used. NOTE FOR RENEWALS: If the LifetimeDays value is evenly divisible by 365, when a certificate is renewed, the lifetime will be treated as years instead of days, so the new certificate's expiration will be the same month and day as the original certificate (assuming you are renewing close enough to expiration that the new expiration date fits within the maximum validity)
	* CACertId - (Optional) ID of issuing CA to be used by DigiCert. If not specified, the default for your account will be used.
	* Organization-Name - (Optional) If specified, will override any organzation name provided in the subject of the cert request on enrollment. Useful for requests (such as ACME) that contain no subject.
	* RenewalWindowDays - (Optional) The number of days from expiration that the gateway should do a reissue rather than a renewal. Default if not provided is 90, meaning any renewal request for certs that expired in more than 90 days will be treated as a reissue.
