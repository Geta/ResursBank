# RELEASE NOTES 1.0.0.7

Release Date: 2016-06-20

## What is Geta.EPi.Commerce.Payments.Resurs.Checkout?

Geta.EPi.Commerce.Payments.Resurs.Checkout is library which helps to integrate Resurs Bank Checkout as one of the payment options in your EPiServer Commerce site.

There are three NuGet packages: 
* Geta.Resurs.Checkout is wrapper for Resurs Checkout API and simplifies API usage 

* Geta.EPi.Commerce.Payments.Resurs.Checkout contains extensions and helpers for easier EPiServer Commerce and Klarna Checkout integration 

* Geta.EPi.Payments.Resurs.CommerceManager contains .ascx for payment method settings for Commerce Manager

## Dependencies

- Episerver CMS 9.0 or higher.
- Episerver Commerce 9.0 or higher.

## Features

1. Implements the Simplified Basic Shop Flow of Resurs Bank (https://test.resurs.com/docs/display/ecom/Step+By+Step%3A+Simplified+Basic+Shop+Flow) 

    * Get Payment Methods.
    * Book Payment.
    * Book Signed Payment(if Signing required).

2.  Supports 4 payment methods:
    * Invoice.
    * Customer card.
    * Customer new card
    * Down payment invoice

3. Implements ResursBank Checkout payment gateway provider for Episerver Commerce.

4. Implements ResursBank gateway configuration for Commerce Manager.

