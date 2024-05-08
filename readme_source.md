# Introduction
This AnyCA Gateway plug-in enables issuance, revocation, and synchronization of certificates from DigiCert's CertCentral offering.  
# Prerequisites

## Certificate Chain

In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you create your Root and/or Subordinate CA, make sure to import the certificate chain into the AnyGateway and Command Server certificate store


# Install
* Download latest successful build from [GitHub Releases](../../releases/latest)

* Copy DigicertCAPlugin.dll and DigicertCAPlugin.deps.json to the Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net6.0\Extensions directory

* Update the manifest.json file located in Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net6.0\Extensions\Connectors
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
