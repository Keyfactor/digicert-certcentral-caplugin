# digicert-certcentral-anycagateway

DigiCert CertCentral plugin for the AnyCA Gateway framework

#### Integration status: Prototype - Demonstration quality. Not for use in customer environments.


## About the Keyfactor AnyGateway CA Connector

This repository contains an AnyGateway CA Connector, which is a plugin to the Keyfactor AnyGateway. AnyGateway CA Connectors allow Keyfactor Command to be used for inventory, issuance, and revocation of certificates from a third-party certificate authority.




## Support for digicert-certcentral-anycagateway

digicert-certcentral-anycagateway is open source and community supported, meaning that there is **no SLA** applicable for these tools.

###### To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.



---






## Keyfactor AnyGateway Framework Supported

This gateway was compiled against version 1.0.0 of the AnyGateway Framework.  You will need at least this version of the AnyGateway Framework Installed.  If you have a later AnyGateway Framework Installed you will probably need to add binding redirects in the CAProxyServer.exe.config file to make things work properly.



---


# Introduction
This AnyGateway plug-in enables issuance, revocation, and synchronization of certificates from DigiCert's CertCentral offering.  
# Prerequisites

## Certificate Chain

In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you create your Root and/or Subordinate CA, make sure to import the certificate chain into the AnyGateway and Command Server certificate store


# Install
* Download latest successful build from [GitHub Releases](../../releases/latest)

* Copy DigiCertCAGateway.dll and DigiCertCAGateway.deps.json to the Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net6.0\Extensions directory

* Update the manifest.json file located in Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net6.0\Extensions\Connectors
  * If the manifest.json file or the Connectors folder do not exist, create them.
  ```json
{  
	"extensions": {  
		"Keyfactor.AnyGateway.Extensions.ICAConnector": {  
			"DigiCertCAConnector": {  
				"assemblypath": "../DigiCertCAGateway.dll",  
				"TypeFullName": "Keyfactor.Extensions.CAGateway.DigiCert.CertCentralCAConnector"  
			}  
		}  
	}  
}
  ```

