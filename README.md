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


TODO Overview is a required section

## Compatibility

The DigiCert CertCentral   Gateway AnyCA Gateway REST plugin is compatible with the Keyfactor AnyCA Gateway REST 24.2.0 and later.

## Support
The DigiCert CertCentral   Gateway AnyCA Gateway REST plugin is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket with your Keyfactor representative. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com. 

> To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

## Requirements

TODO Requirements is a required section

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

        TODO Gateway Registration is a required section

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

2. TODO Certificate Template Creation Step is a required section

3. Follow the [official Keyfactor documentation](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/AddCA-Keyfactor.htm) to add each defined Certificate Authority to Keyfactor Command and import the newly defined Certificate Templates.

4. TODO Custom Enrollment Parameter Creation Step is an optional section. If this section doesn't seem necessary on initial glance, please delete it. Refer to the docs on [Confluence](https://keyfactor.atlassian.net/wiki/x/SAAyHg) for more info



## License

Apache License 2.0, see [LICENSE](LICENSE).

## Related Integrations

See all [Keyfactor Any CA Gateways (REST)](https://github.com/orgs/Keyfactor/repositories?q=anycagateway).