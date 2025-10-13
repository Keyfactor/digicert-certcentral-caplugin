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