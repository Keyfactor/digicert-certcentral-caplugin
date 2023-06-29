# <Gateway Name Here>

<Description Here>

#### Integration status: Prototype - Demonstration quality. Not for use in customer environments.


## About the Keyfactor AnyGateway CA Connector

This repository contains an AnyGateway CA Connector, which is a plugin to the Keyfactor AnyGateway. AnyGateway CA Connectors allow Keyfactor Command to be used for inventory, issuance, and revocation of certificates from a third-party certificate authority.




## Support for <Gateway Name Here>

<Gateway Name Here> is open source and community supported, meaning that there is **no SLA** applicable for these tools.

###### To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.



---






## Keyfactor AnyGateway Framework Supported

This gateway was compiled against version 10.x.x of the AnyGateway Framework.  You will need at least this version of the AnyGateway Framework Installed.  If you have a later AnyGateway Framework Installed you will probably need to add binding redirects in the CAProxyServer.exe.config file to make things work properly.



---


# Introduction
This AnyGateway plug-in enables issuance, revocation, and synchronization of certificates from <Company> <Product> offering.  
# Prerequisites

## Certificate Chain

In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you create your Root and/or Subordinate CA, make sure to import the certificate chain into the AnyGateway and Command Server certificate store


# Install
* Download latest successful build from [GitHub Releases](../../releases/latest)

* Copy <GatewayDLL>.dll to the Program Files\Keyfactor\Keyfactor AnyGateway directory

* Update the CAProxyServer.config file
  * Update the CAConnection section to point at the DigiCertCAProxy class
  ```xml
  <alias alias="CAConnector" type="Keyfactor.Extensions.AnyGateway.Company.Product.GatewayNameCAConnector, DLLName"/>
  ```

# Configuration
The following sections will breakdown the required configurations for the AnyGatewayConfig.json file that will be imported to configure the AnyGateway.

## Templates
The Template section will map the CA's products to an AD template.
* ```ProductID```
This is the ID of the <Product> product to map to the specified template.

 ```json
  "Templates": {
	"WebServer": {
      "ProductID": "<productID>",
      "Parameters": {
      }
   }
}
 ```
 
## Security
The security section does not change specifically for the <Product> CA Gateway.  Refer to the AnyGateway Documentation for more detail.
```json
  /*Grant permissions on the CA to users or groups in the local domain.
	READ: Enumerate and read contents of certificates.
	ENROLL: Request certificates from the CA.
	OFFICER: Perform certificate functions such as issuance and revocation. This is equivalent to "Issue and Manage" permission on the Microsoft CA.
	ADMINISTRATOR: Configure/reconfigure the gateway.
	Valid permission settings are "Allow", "None", and "Deny".*/
    "Security": {
        "Keyfactor\\Administrator": {
            "READ": "Allow",
            "ENROLL": "Allow",
            "OFFICER": "Allow",
            "ADMINISTRATOR": "Allow"
        },
        "Keyfactor\\gateway_test": {
            "READ": "Allow",
            "ENROLL": "Allow",
            "OFFICER": "Allow",
            "ADMINISTRATOR": "Allow"
        },		
        "Keyfactor\\SVC_TimerService": {
            "READ": "Allow",
            "ENROLL": "Allow",
            "OFFICER": "Allow",
            "ADMINISTRATOR": "None"
        },
        "Keyfactor\\SVC_AppPool": {
            "READ": "Allow",
            "ENROLL": "Allow",
            "OFFICER": "Allow",
            "ADMINISTRATOR": "Allow"
        }
    }
```
## CerificateManagers
The Certificate Managers section is optional.
	If configured, all users or groups granted OFFICER permissions under the Security section
	must be configured for at least one Template and one Requester. 
	Uses "<All>" to specify all templates. Uses "Everyone" to specify all requesters.
	Valid permission values are "Allow" and "Deny".
```json
  "CertificateManagers":{
		"DOMAIN\\Username":{
			"Templates":{
				"MyTemplateShortName":{
					"Requesters":{
						"Everyone":"Allow",
						"DOMAIN\\Groupname":"Deny"
					}
				},
				"<All>":{
					"Requesters":{
						"Everyone":"Allow"
					}
				}
			}
		}
	}
```
## CAConnection
The CA Connection section will determine the API endpoint and configuration data used to connect to the <Product> API. 


```json
  "CAConnection": {

  },
```
## GatewayRegistration
There are no specific Changes for the GatewayRegistration section. Refer to the AnyGateway Documentation for more detail.
```json
  "GatewayRegistration": {
    "LogicalName": "CASandbox",
    "GatewayCertificate": {
      "StoreName": "CA",
      "StoreLocation": "LocalMachine",
      "Thumbprint": "0123456789abcdef"
    }
  }
```

## ServiceSettings
There are no specific Changes for the ServiceSettings section. Refer to the AnyGateway Documentation for more detail.
```json
  "ServiceSettings": {
    "ViewIdleMinutes": 8,
    "FullScanPeriodHours": 24,
	"PartialScanPeriodMinutes": 240 
  }
```

