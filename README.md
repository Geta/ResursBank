Library for Resurs Checkout integration with EPiServer Commerce
=============

## What is Geta.EPi.Commerce.Payments.Resurs.Checkout?

Geta.EPi.Commerce.Resurs.Klarna.Checkout is library which helps to integrate Resurs Checkout as one of the payment options in your EPiServer Commerce sites.
This library consists of three assemblies: 
* Geta.Resurs.Checkout is wrapper for Resurs Checkout API and simplifies API usage 
* Geta.EPi.Commerce.Payments.Resurs.Checkout contains extensions and helpers for easier EPiServer Commerce and Klarna Checkout integration 
* Geta.EPi.Payments.Resurs.CommerceManager contains .ascx for payment method settings for Commerce Manager

## How to get started?

Start by installing NuGet packages (use [NuGet](http://nuget.episerver.com/)):

    Install-Package Geta.EPi.Commerce.Payments.Resurs.Checkout

For the Commerce Manager site run the following package:

    Install-Package Geta.EPi.Payments.Resurs.CommerceManager
	Install-Package Geta.EPi.Commerce.Payments.Resurs.Checkout

## Setup

#### Endpoints

Resurs Checkout requires to add Resurs WCF service reference to your Website:
- Get testing services from : https://test.resurs.com/docs/display/ecom/Test+URLs
- Get go-live services from : https://test.resurs.com/docs/display/ecom/Live+URLs+and+go-live+checklist

#### Web.config / app.config
Resurs Checkout requires to 2 app-setting key for authenticate service:
This credential was supplied from Resurs Bank. Please contact to them to get detail.
```XML
  <appSettings>
    <add key="ResursBank:UserName" value="service_username"/>
    <add key="ResursBank:Password" value="service_password"/>
  </appSettings>
```

#### Configure Commerce Manager

Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

- **Name**
- **System Keyword** - ResursBankCheckout
- **Language**
- **Class Name** - choose **Geta.EPi.Commerce.Payments.Resurs.Checkout.ResursCheckoutPaymentGateway**
- **Payment Class** - choose **Geta.Epi.Commerce.Payments.Resurs.Checkout.Bussiness.ResursBankPayment**
- **IsActive** - **Yes**
- select shipping methods available for this payment
- navigate to parameters tab and fill inn settings (see screenshot below)


![Payment method settings](docs/screenshots/ResursSettings.png?raw=true "Payment method settings")

![Payment method settings](docs/screenshots/ResursParameter.png?raw=true "Payment method parameters")

**Note: If the parameters tab is empty (or gateway class is missing), make sure you have installed the commerce manager nuget (see above)**

In **Markets** tab select market for which this payment will be available.

## Creating NuGet package

Project contains _pack.bat_ file for easier packaging. It calls _nuget.exe_ (assuming it's in your environment PATH) with _-IncludeReferencedProjects_ to include referenced Geta.Klarna.Checkout assembly. You also can provide output directory as a parameter for _pack.bat_.

## Troubleshooting tips

## More info

### Related blog posts

### Resurs Checkout Simplified Basic Shop Flow

https://test.resurs.com/docs/display/ecom/Step+By+Step%3A+Simplified+Basic+Shop+Flow

### Resurs Checkout documentation

https://test.resurs.com/docs/display/ecom/Full+manual

## Demo

To setup your own demo environment (local or otherwise): [local demo setup](docs/local-demo-setup.md).

### Quicksilver

Here's a step by step guide on how to add it to a new Quicksilver site: [Integrate to Quicksilver](docs/Integrate-to-quicksilver.md)

## Release notes

* [Changes in version 1.0.0.0](docs/release-notes-1.md)
