## Overview

The Digicert CertCentral AnyCA REST plugin extends the capabilities of Digicert's CertCentral product to Keyfactor Command via the Keyfactor AnyCA Gateway REST. The plugin represents a fully featured AnyCA REST Plugin with the following capabilies:
* SSL Certificate Synchronization
* SSL Certificate Enrollment
* SSL Certificate Revocation


## Requirements

An API Key within your Digicert account that has the necessary permissions to enroll, approve, and revoke certificates.

## Gateway Registration

In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you identify your Root and/or Subordinate CA in your Digicert account, make sure to download and import the certificate chain into the Command Server certificate store

## Certificate Template Creation Step

Note for SMIME product types (Secure Email types): The template configuration fields provided for those are not required to be filled out in the gateway config. Many of those values would change on a per-enrollment basis. The way to handle that is to create Enrollment fields in Command with the same name (for example: CommonNameIndicator) and then any values populated in those fields will override any static values provided in the configuration.

