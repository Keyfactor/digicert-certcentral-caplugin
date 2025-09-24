<h1 align="center" style="border-bottom: none">
    DigiCert CertCentral   Gateway AnyCA Gateway REST Plugin
</h1>

<p align="center">
  <!-- Badges -->
<img src="https://img.shields.io/badge/integration_status-production-3D1973?style=flat-square" alt="Integration Status: production" />
<a href="https://github.com/Keyfactor/digicert-certcentral-caplugin/releases"><img src="https://img.shields.io/github/v/release/Keyfactor/digicert-certcentral-caplugin?style=flat-square" alt="Release" /></a>
<img src="https://img.shields.io/github/issues/Keyfactor/digicert-certcentral-caplugin?style=flat-square" alt="Issues" />
<img src="https://img.shields.io/github/downloads/Keyfactor/digicert-certcentral-caplugin/total?style=flat-square&label=downloads&color=28B905" alt="GitHub Downloads (all assets, all releases)" />
</p>

<p align="center">
  <!-- TOC -->
  <a href="#support">
    <b>Support</b>
  </a> 
  ·
  <a href="#requirements">
    <b>Requirements</b>
  </a>
  ·
  <a href="#installation">
    <b>Installation</b>
  </a>
  ·
  <a href="#license">
    <b>License</b>
  </a>
  ·
  <a href="https://github.com/orgs/Keyfactor/repositories?q=anycagateway">
    <b>Related Integrations</b>
  </a>
</p>


The Digicert CertCentral AnyCA REST plugin extends the capabilities of Digicert's CertCentral product to Keyfactor Command via the Keyfactor AnyCA Gateway REST. The plugin represents a fully featured AnyCA REST Plugin with the following capabilies:
* SSL Certificate Synchronization
* SSL Certificate Enrollment
* SSL Certificate Revocation

## Compatibility

The DigiCert CertCentral   Gateway AnyCA Gateway REST plugin is compatible with the Keyfactor AnyCA Gateway REST 24.2.0 and later.

## Support
The DigiCert CertCentral   Gateway AnyCA Gateway REST plugin is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket with your Keyfactor representative. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com. 

> To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

## Requirements

An API Key within your Digicert account that has the necessary permissions to enroll, approve, and revoke certificates.

## Installation

1. Install the AnyCA Gateway REST per the [official Keyfactor documentation](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/InstallIntroduction.htm).

2. On the server hosting the AnyCA Gateway REST, download and unzip the latest [DigiCert CertCentral   Gateway AnyCA Gateway REST plugin](https://github.com/Keyfactor/digicert-certcentral-caplugin/releases/latest) from GitHub.

3. Copy the unzipped directory (usually called `net6.0` or `net8.0`) to the Extensions directory:


    ```shell
    Depending on your AnyCA Gateway REST version, copy the unzipped directory to one of the following locations:
    Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net6.0\Extensions
    Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net8.0\Extensions
    ```

    > The directory containing the DigiCert CertCentral   Gateway AnyCA Gateway REST plugin DLLs (`net6.0` or `net8.0`) can be named anything, as long as it is unique within the `Extensions` directory.

4. Restart the AnyCA Gateway REST service.

5. Navigate to the AnyCA Gateway REST portal and verify that the Gateway recognizes the DigiCert CertCentral   Gateway plugin by hovering over the ⓘ symbol to the right of the Gateway on the top left of the portal.

## Configuration

1. Follow the [official AnyCA Gateway REST documentation](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/AddCA-Gateway.htm) to define a new Certificate Authority, and use the notes below to configure the **Gateway Registration** and **CA Connection** tabs:

    * **Gateway Registration**

        In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you identify your Root and/or Subordinate CA in your Digicert account, make sure to download and import the certificate chain into the Command Server certificate store

    * **CA Connection**

        Populate using the configuration fields collected in the [requirements](#requirements) section.

        * **APIKey** - API Key for connecting to DigiCert 
        * **DivisionId** - Division ID to use for retrieving product details (only if account is configured with per-divison product settings) 
        * **Region** - The geographic region that your DigiCert CertCentral account is in. Valid options are US and EU. 
        * **RevokeCertificateOnly** - Default DigiCert behavior on revocation requests is to revoke the entire order. If this value is changed to 'true', revocation requests will instead just revoke the individual certificate. 
        * **SyncCAFilter** - If you list one or more CA IDs here (comma-separated), the sync process will only sync records from those CAs. If you want to sync all CA IDs, leave this field empty. 
        * **SyncDivisionFilter** - If you list one or more Divison IDs (also known as Container IDs) here (comma-separated), the sync process will filter records to only return orders from those divisions. If you want to sync all divisions, leave this field empty. Note that this has no relationship to the value of the DivisionId config field. 
        * **FilterExpiredOrders** - If set to 'true', syncing will apply a filter to not return orders that are expired for longer than specified in SyncExpirationDays. 
        * **SyncExpirationDays** - If FilterExpiredOrders is set to true, this setting determines how many days in the past to still return expired orders. For example, a value of 30 means the sync will return any certs that expired within the past 30 days. A value of 0 means the sync will not return any certs that expired before the current day. This value is ignored if FilterExpiredOrders is false. 
        * **Enabled** - Flag to Enable or Disable gateway functionality. Disabling is primarily used to allow creation of the CA prior to configuration information being available. 

2. Note for SMIME product types (Secure Email types): The template configuration fields provided for those are not required to be filled out in the gateway config. Many of those values would change on a per-enrollment basis. The way to handle that is to create Enrollment fields in Command with the same name (for example: CommonNameIndicator) and then any values populated in those fields will override any static values provided in the configuration.

3. Follow the [official Keyfactor documentation](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/AddCA-Keyfactor.htm) to add each defined Certificate Authority to Keyfactor Command and import the newly defined Certificate Templates.

4. In Keyfactor Command (v12.3+), for each imported Certificate Template, follow the [official documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Configuring%20Template%20Options.htm) to define enrollment fields for each of the following parameters:

    * **LifetimeDays** - OPTIONAL: The number of days of validity to use when requesting certs. If not provided, default is 365. 
    * **CACertId** - OPTIONAL: ID of issuing CA to use by DigiCert. If not provided, the default for your account will be used. 
    * **Organization-Name** - OPTIONAL: For requests that will not have a subject (such as ACME) you can use this field to provide the organization name. Value supplied here will override any CSR values, so do not include this field if you want the organization from the CSR to be used. 
    * **RenewalWindowDays** - OPTIONAL: The number of days from certificate expiration that the gateway should do a renewal rather than a reissue. If not provided, default is 90. 
    * **CertType** - OPTIONAL: The type of cert to enroll for. Valid values are 'ssl' and 'client'. The value provided here must be consistant with the ProductID. If not provided, default is 'ssl'. Ignored for secure_email_* product types. 
    * **EnrollDivisionId** - OPTIONAL: The division (container) ID to use for enrollments against this template. 
    * **CommonNameIndicator** - Required for secure_email_sponsor and secure_email_organization products, ignored otherwise. Defines the source of the common name. Valid values are: email_address, given_name_surname, pseudonym, organization_name 
    * **ProfileType** - Optional for secure_email_* types, ignored otherwise. Valid values are: strict, multipurpose. Default value is strict. 
    * **FirstName** - Required for secure_email_* types if CommonNameIndicator is given_name_surname, ignored otherwise. 
    * **LastName** - Required for secure_email_* types if CommonNameIndicator is given_name_surname, ignored otherwise. 
    * **Pseudonym** - Required for secure_email_* types if CommonNameIndicator is pseudonym, ignored otherwise. 
    * **UsageDesignation** - Required for secure_email_* types, ignored otherwise. The primary usage of the certificate. Valid values are: signing, key_management, dual_use 



## License

Apache License 2.0, see [LICENSE](LICENSE).

## Related Integrations

See all [Keyfactor Any CA Gateways (REST)](https://github.com/orgs/Keyfactor/repositories?q=anycagateway).