
# Introduction

This AnyCA REST Gateway plug-in enables issuance, revocation, and synchronization of certificates from DigiCert's CertCentral offering.  
# Prerequisites

## Certificate Chain

In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you create your Root and/or Subordinate CA, make sure to import the certificate chain into the AnyGateway and Command Server certificate store

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

5. Navigate to the AnyCA Gateway REST portal and verify that the Gateway recognizes the DigiCert plugin by hovering over the ⓘ symbol to the right of the Gateway on the top left of the portal. CAPlugin Type should now be listed as CertCentralCA.


## Configuration

1. Follow the [official Keyfactor AnyCA Gateway REST documentation](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/AddCA-Gateway.htm#Add_or_Edit_a_Certificate_Authority) to define a new Certificate Authority, using the following information to configure the CA Connection section:

SETTING | REQUIRED? | DESCRIPTION
--|--|--
Enabled | Yes | Enables the DigiCert gateway functionality. Should almost always be set to 'true'
APIKey | Yes | The API key the Gateway should use to communicate with the DigiCert API. Can be generated from the DigiCert portal.
Region | No | The geographic region associated with your DigiCert account. Valid values are US and EU. Default if not provided is US.
DivisionId | No | If your CertCentral account has multiple divisions AND uses any custom per-division product settings, provide a division ID for the gateway to use for product type lookups.
RevokeCertificateOnly | No | If set to 'true', revoke operations will only revoke the individual certificate in question rather than the entire DigiCert order. Default if not provided is 'false'.
SyncCAFilter | No | If you list one or more DigiCert issuing CA IDs here (comma-separated if more than one), the sync process will only return certs issued by one of those CAs. Leave this option empty to sync all certs from all CAs.
FilterExpiredOrders | No | If set to 'true', syncing will not return certs that are expired more than a specified number of days. The number of days is specified by the SyncExpirationDays config option. Default value is 'false'.
SyncExpirationDays | No | Only used if FilterExpiredOrders is 'true', otherwise ignored. Sets the number of days a cert has to be expired for the sync process to no longer sync it. For example, a value of 30 means sync will continue to return certs that have expired within the past 30 days, but not ones older than that. Default value is 0, meaning sync would not return any certs expired before the current day.

2. After saving the CA configuration, Follow the [official AnyCA Gateway REST documentation](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/AddCP-Gateway.htm#Certificate_Profile) to define one or more Certificate Profiles.
3. Edit your newly configured CA, and you should now be able to modify the Templates tab. You need at least one template for each product type you wish to be able to enroll for. It is recommended to include the product type in the template name to make them easier to identify. Use the following information to configure the parameters for each template:

SETTING | REQUIRED? | DESCRIPTION
--|--|--
LifetimeDays | No | The number of days of validity to use when requesting certs. Default if not provided is 365. NOTE FOR RENEWALS: If the value of LifetimeDays is evenly divisible by 365, the expiration day and month of the new cert will be set to the same values as the old cert if possible, to avoid renewal date drift.
CACertId | No | The ID of the issuing CA to be used by DigiCert. If not specified, the default for your account will be used.
Organization-Name | No | If specified, this value will override any organization name provided in the subject of the cert request on enrollment. Useful for requests (such as ACME) that contain no subject.
RenewalWindowDays | No | The number of days from expiration that the gateway should do a reissue rather than a renewal. Default if not provided is 90, meaning any renewal request for certs that expire in more than 90 days will be treated as a reissue request.
