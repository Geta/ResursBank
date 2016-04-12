RELEASE NOTES 1.0.0.0
=============

Release Date: 2016-04-15

## What is Geta.EPi.Commerce.Payments.Resurs.Checkout?

Geta.EPi.Commerce.Resurs.Klarna.Checkout is library which helps to integrate Resurs Checkout as one of the payment options in your EPiServer Commerce sites.
This library consists of three assemblies: 
* Geta.Resurs.Checkout is wrapper for Resurs Checkout API and simplifies API usage 

* Geta.EPi.Commerce.Payments.Resurs.Checkout contains extensions and helpers for easier EPiServer Commerce and Klarna Checkout integration 

* Geta.EPi.Payments.Resurs.CommerceManager contains .ascx for payment method settings for Commerce Manager

## Dependences

- Episerver CMS 9.0 and higher.
- Episerver Ecommerce 9.0 and higher.

## Features

1. Implement the Simplified Basic Shop Flow of Resurs Bank (https://test.resurs.com/docs/display/ecom/Step+By+Step%3A+Simplified+Basic+Shop+Flow) 

    * Get Payment Methods.
    * Book Payment.
    * Book Signed Payment(if Signing required).

2.  Supporting 4 payment methods:
    * Invoice.
    * Customer card.
    * Customer new card
    * Down payment invoice

3. Implement ResursBank Checkout payment gateway provider for Episerver Ecommerce.

4. Implement ResursBank gateway configuration for Commerce Manager.

