### 2.0.0  
* Initial Public Release  

### 2.0.1  
* Add configuration fields to support sync filtering  
* Bug fixes around SAN processing  

### 2.1.0  
* Add support for enrolling for client certs  
* Option to filter sync by division ID  
* Option to provide division ID for enrollment  
* Add support for secure_email_* SMIME product types  

### 2.1.1  
* Add configuration flag to support adding client auth EKU to ssl cert requests  
	* NOTE: This is a temporary feature which is planned for loss of support by Digicert in May 2026   
* For smime certs, use profile type defined on the product as the default if not supplied, rather than just defaulting to 'strict'  
* Hotfix for data type conversion  

### 2.1.2  
* Hotfix for incremental sync to default to a 6 day window if no previous incremental sync has run  
* Workaround for DigiCert API issue where retrieving the PEM data of multiple certificates in the same order can occasionally return duplicate data rather than the correct cert  
* Remove caching of product ID lookups from DigiCert account  

### 2.2.0  
* Add support for duplicating certs

### 2.2.1  
* Properly mark 'needs_approval' status as Pending rather than Failed  

### 2.3.0
* Add configuration flag to support adding KDC/SmartCardLogon EKU to ssl cert requests  
