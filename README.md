---
page_type: sample
languages:
- csharp
- html
products:
- azure
- azure-active-directory
- azure-storage
description: "This is an example web app that will use the users' credentials to read all images from a configured directory in a configured Storage account, and serve also serve them up as AAD-authenticated image URL's."
urlFragment: SecureImageBrowser
---

# App sign-in and viewing images via the Azure Storage API and Microsoft identity platform

This is an example web app based upon the code sample at https://docs.microsoft.com/azure/storage/common/storage-auth-aad-app. It will use the users' credentials to read all images from a configured directory in a configured Storage account, and serve also serve them up as AAD-authenticated image URL's. This is a more secure alternative to viewing images in a users' web browser than using SAS keys.


