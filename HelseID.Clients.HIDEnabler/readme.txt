HIDEnabler (HelseID Enabler)
----------------------------

HIDEnabler is meant for creating HelseID configurations for EHRs with support for Kjernejournal.

When it is run it creates the following artifacts:
	- A file with configuration meant to be used by the EHR when authentication with HelseID
	- A corresponding configuration for the EHR is created in HelseID
	- The configuration is secured with a RSA keypair (public/private) where the private key is stored in secured local storage.





