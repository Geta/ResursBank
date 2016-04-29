INTEGRATE TO QUICKSILVER
=============
This documentation shows you how to integrate ResursBank payment checkout plugin to Ecommerce Quicksilver sample site by step-by-step.

I hope it would be usefull to you.

###1. Install nuget package
Start by installing NuGet packages for EPiServer.Reference.Commerce.Site: 

    Install-Package Geta.EPi.Commerce.Payments.Resurs.Checkout
    Install-Package Sample_Integrate_ResurCheckOut_To_QuickSilver

For the EPiServer.Reference.Commerce.Manager site run the following package:

    Install-Package Geta.EPi.Payments.Resurs.CommerceManager

    Install-Package Geta.EPi.Commerce.Payments.Resurs.Checkout


###2. Configure Commerce Manager

Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

- **Name**
- **System Keyword** - ResursBankCheckout
- **Language**
- **Class Name** - choose **Geta.EPi.Commerce.Payments.Resurs.Checkout.ResursCheckoutPaymentGateway**
- **Payment Class** - choose **Geta.Epi.Commerce.Payments.Resurs.Checkout.Bussiness.ResursBankPayment**
- **IsActive** - **Yes**
- select shipping methods available for this payment
- navigate to parameters tab and fill inn settings (see screenshot below)


![Payment method settings](screenshots/ResursSettings.png?raw=true "Payment method settings")

![Payment method settings](screenshots/ResursParameter.png?raw=true "Payment method parameters")

**Note: If the parameters tab is empty (or gateway class is missing), make sure you have installed the commerce manager nuget (see above)**

In **Markets** tab select market for which this payment will be available.


###3. Changes in Quicksilver solution

- Include file "EPiServer.Reference.Commerce.Site/sampleCodes/ResursBankCheckoutViewModel.cs" to  project

- Changes in  "EPiServer.Reference.Commerce.Site/Styles/style.less"

```CSS
// add this code to end of file
@import url("form/form");
@import url("Pages/common");
@import url("Pages/checkout");
@import url("Pages/payment");
@import url("Pages/confirm");
@import url("Pages/digital_signing");
```

- Changes in "EPiServer.Reference.Commerce.Site\Features\Payment\Models\PaymentMethodViewModelResolver.cs"

```C#
//add this code for Resurs payment gateway
using Geta.Epi.Commerce.Payments.Resurs.Checkout.Bussiness;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class PaymentMethodViewModelResolver
    {
        public static IPaymentMethodViewModel<IPaymentOption> Resolve(string paymentMethodName)
        {
            switch (paymentMethodName)
            {
                case "CashOnDelivery":
                    return new CashOnDeliveryViewModel() { PaymentMethod = new CashOnDeliveryPaymentMethod() };

                case "GenericCreditCard":
                    return new GenericCreditCardViewModel() { PaymentMethod = new GenericCreditCardPaymentMethod() };
                //add this code for Resurs payment gateway
                case "ResursBankCheckout":
                    return new ResursBankCheckoutViewModel() { PaymentMethod = new ResursCheckoutPaymentGateway() };
            }

            throw new ArgumentException("No view model has been implemented for the method " + paymentMethodName, "paymentMethodName");
        }
    }
}
```

-  Changes in "EPiServer.Reference.Commerce.Site\Features\Checkout\Controllers\CheckoutController.cs"
    + Add BookSignedPayment Method

```C#
   [HttpGet]
        public ActionResult BookSignedpayment()
        {

            ResursBankServiceClient service = new ResursBankServiceClient(null);
            //bookPaymentResult result = service.BookSignedPayment(paymentId);
            _cartService.RunWorkflow(OrderGroupWorkflowManager.CartCheckOutWorkflowName);
            PurchaseOrder purchaseOrder = _checkoutService.SaveCartAsPurchaseOrder();
            _checkoutService.DeleteCart();

            var startpage = _contentRepository.Get<StartPage>(ContentReference.StartPage);
            var confirmationPage = _contentRepository.GetFirstChild<OrderConfirmationPage>(startpage.CheckoutPage);
            var queryCollection = new NameValueCollection
            {
                {"contactId", _customerContext.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderGroupId.ToString(CultureInfo.InvariantCulture)}
            };

            return Redirect(new UrlBuilder(confirmationPage.LinkURL) { QueryCollection = queryCollection }.ToString());
        }
```

   + CreateCheckoutViewModel method

From
```C#
((PaymentMethodBase)viewModel.Payment.PaymentMethod).PaymentMethodId = selectedPaymentMethod.Id;
```
To
```C#
if (viewModel.Payment.PaymentMethod is ResursCheckoutPaymentGateway)
            {
                ((ResursCheckoutPaymentGateway)viewModel.Payment.PaymentMethod).PaymentMethodId = selectedPaymentMethod.Id;
                ((ResursCheckoutPaymentGateway)viewModel.Payment.PaymentMethod).CallBackUrlWhenFail = Url.Action("BookSignedpayment");

            }
            else
                ((PaymentMethodBase)viewModel.Payment.PaymentMethod).PaymentMethodId = selectedPaymentMethod.Id;
```
   + Purchase method

From

```C#
            try
            {
                _paymentService.ProcessPayment(checkoutViewModel.Payment.PaymentMethod);
            }
            catch (PreProcessException)
            {
                ModelState.AddModelError("PaymentMethod", _localizationService.GetString("/Checkout/Payment/Errors/PreProcessingFailure"));
            }
```

To 

```C#
            try
            {
                var resursBank = checkoutViewModel.Payment as ResursBankCheckoutViewModel;
                if (resursBank != null)
                {
                    var resursBankPaymentMethod =
                        checkoutViewModel.Payment.PaymentMethod as ResursCheckoutPaymentGateway;
                    if (resursBankPaymentMethod != null)
                    {
                        resursBankPaymentMethod.CardNumber = resursBank.CardNumber;
                        resursBankPaymentMethod.ResursPaymentMethod = resursBank.ResursPaymentMethod;
                        //resursBankPaymentMethod.CallBackUrlWhenFail = Url.Action("BookSignedpayment", "Checkout", null, this.Request.Url.Scheme);
                        resursBankPaymentMethod.SuccessUrl = "http://" + this.Request.Url.DnsSafeHost + Url.Action("BookSignedpayment");
                        resursBankPaymentMethod.CallBackUrlWhenFail = "http://" + this.Request.Url.DnsSafeHost + _contentRepository.GetFirstChild<OrderConfirmationPage>(currentPage.ContentLink).LinkURL;
                        resursBankPaymentMethod.GovernmentId = resursBank.GovernmentId;
                        resursBankPaymentMethod.AmountForNewCard = resursBank.AmountForNewCard;
                        resursBankPaymentMethod.MinLimit = resursBank.MinLimit;
                        resursBankPaymentMethod.MaxLimit = resursBank.MaxLimit;
                        resursBankPaymentMethod.InvoiceDeliveryType = resursBank.DeliveryType;
                    }
                }

                _paymentService.ProcessPayment(checkoutViewModel.Payment.PaymentMethod);
            }
            catch (PreProcessException)
            {
                ModelState.AddModelError("PaymentMethod", _localizationService.GetString("/Checkout/Payment/Errors/PreProcessingFailure"));
            }
```

   + Finish Method

    Comment out
```C#
            //if (totalProcessedAmount != orderForm.Total)
            //{
            //    throw new InvalidOperationException("Wrong amount");
            //}
```
and
```C#
            //try
            //{
            //    _mailService.Send(startpage.OrderConfirmationMail, queryCollection, emailAddress, currentPage.Language.Name);
            //}
            //catch (Exception)
            //{
            //    // The purchase has been processed and the payment was successfully settled, but for some reason the e-mail
            //    // receipt could not be sent to the customer. Rollback is not possible so simple make sure to inform the
            //    // customer to print the confirmation page instead.
            //    queryCollection.Add("notificationMessage", string.Format(_localizationService.GetString("/OrderConfirmationMail/ErrorMessages/SmtpFailure"), emailAddress));

            //    // Todo: Log the error and raise an alert so that an administrator can look in to it.
            //}
```

-  Changes in "EPiServer.Reference.Commerce.Site\Features\Checkout\Controllers\CheckoutController.cs"
    + Change ProcessPayment Method
```C#
        public void ProcessPayment(IPaymentOption method)
        {
            var cart = _cartHelper(Mediachase.Commerce.Orders.Cart.DefaultName).Cart;

            if (!cart.OrderForms.Any())
            {
                cart.OrderForms.AddNew();
            }
            
            var payment = method.PreProcess(cart.OrderForms[0]);

            if (payment == null)
            {
                throw new PreProcessException();
            }

            if (cart.OrderForms[0].Payments.Count > 0)
            {
                cart.OrderForms[0].Payments.Clear();
                cart.AcceptChanges();
            }

            cart.OrderForms[0].Payments.Add(payment);
            cart.AcceptChanges();

            foreach (var lineItem in cart.OrderForms[0].LineItems)
            {
                var metaFieldItem = lineItem as MetaStorageBase;
                //assume that vat is 0
                metaFieldItem.SetMetaField(ResursConstants.ResursVatPercent, 0, false);
                metaFieldItem.AcceptChanges();
            }

            method.PostProcess(cart.OrderForms[0]);
        }
```

###4. The End
- Let build the solution and test it.
