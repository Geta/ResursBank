using System.Collections.Generic;
using System.Configuration;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;
using Xunit;

namespace Test.Integration
{
    public class ResursBankServiceClientTest
    {
        [Fact]
        public void GetPaymentMethods()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var testCredential = new ResursCredential(appSettings["ResursBankUserName"], appSettings["ResursBankUserNamePassword"]);
            var resursBankServices = new ResursBankServiceClient(testCredential);
            List<PaymentMethodResponse> list = resursBankServices.GetPaymentMethods("sv", "NATURAL", 1000);
            Assert.Equal(4,list.Count);
        }

        [Fact]
        public void BookPayment()
        {
            var credential = new ResursCredential();
            var appSettings = ConfigurationManager.AppSettings;
            credential.UserName = appSettings["ResursBankUserName"] ?? "Not Found";
            credential.Password = appSettings["ResursBankUserNamePassword"] ?? "Not Found";
            var resursBankServiceClient = new ResursBankServiceClient(credential);
            var bookPaymentObject = CreateBookPaymentObject();
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            Assert.NotEqual(bookPaymentStatus.DENIED, result.bookPaymentStatus);
        }

        [Fact]
        public void BookSignedPayment()
        {
            var credential = new ResursCredential();
            var appSettings = ConfigurationManager.AppSettings;
            credential.UserName = appSettings["ResursBankUserName"] ?? "Not Found";
            credential.Password = appSettings["ResursBankUserNamePassword"] ?? "Not Found";
            var resursBankServiceClient = new ResursBankServiceClient(credential);

            
            var bookPaymentObject = CreateBookPaymentSignedObject();
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            var resultSigning = resursBankServiceClient.BookSignedPayment(result.paymentId);
            Assert.NotEqual(bookPaymentStatus.DENIED, resultSigning.bookPaymentStatus);
        }

        private BookPaymentObject CreateBookPaymentSignedObject()
        {
            var bookPaymentObject = new BookPaymentObject();
            bookPaymentObject.PaymentData = new paymentData();
            bookPaymentObject.PaymentData.paymentMethodId = "NEWCARD";
            bookPaymentObject.PaymentData.customerIpAddress = "127.0.0.1";

            specLine[] spLines = new specLine[1];
            var paymentSpec = new paymentSpec();
            var spLine = new specLine();
            spLine.id = "product01";
            spLine.artNo = "sku-001";
            spLine.description = "denim trunk";
            spLine.quantity = 1;
            spLine.unitMeasure = "st";
            spLine.unitAmountWithoutVat = 1000;
            spLine.vatPct = 25;
            spLine.totalVatAmount = 250;
            spLine.totalAmount = 1250;
            spLines[0] = spLine;
            paymentSpec.specLines = spLines;
            paymentSpec.totalAmount = 1250;
            paymentSpec.totalVatAmount = 250;
            paymentSpec.totalVatAmountSpecified = true;
            //create paymentSpecification;
            bookPaymentObject.PaymentSpec = paymentSpec;
            bookPaymentObject.MapEntry = null;

            var card = new cardData();
            card.cardNumber = "0000 0000 0000 0000";
            card.amount = 10000;
            card.amountSpecified = true;
            var signing = new signing()
            {
                failUrl = "http://google.com",
                forceSigning = true,
                forceSigningSpecified = false,
                successUrl = "http://google.com"
            };

            bookPaymentObject.Card = card;
            bookPaymentObject.Signing = signing;

            //extendedCustomer
            var extendedCustomer = new extendedCustomer();
            extendedCustomer.governmentId = "010986-14741";
            extendedCustomer.address = new address();
            extendedCustomer.address.fullName = "David Smeichel";
            extendedCustomer.address.firstName = "David";
            extendedCustomer.address.lastName = "Smeichel";
            extendedCustomer.address.addressRow1 = "1st Infinite loop";
            extendedCustomer.address.addressRow2 = "2nd Infinite loop";
            extendedCustomer.address.postalArea = "norway";
            extendedCustomer.address.postalCode = "no";
            extendedCustomer.address.country = countryCode.NO;
            extendedCustomer.phone = "+4797674852";
            extendedCustomer.email = "thien.trinh@niteco.se";
            extendedCustomer.type = customerType.NATURAL;

            bookPaymentObject.ExtendedCustomer = extendedCustomer;
            return bookPaymentObject;
        }

        private BookPaymentObject CreateBookPaymentObject()
        {
            var bookPaymentObject = new BookPaymentObject();
            bookPaymentObject.PaymentData = new paymentData();
            bookPaymentObject.PaymentData.paymentMethodId = "CARD";
            bookPaymentObject.PaymentData.customerIpAddress = "127.0.0.1";

            specLine[] spLines = new specLine[1];
            var paymentSpec = new paymentSpec();
            var spLine = new specLine();
            spLine.id = "product01";
            spLine.artNo = "sku-001";
            spLine.description = "denim trunk";
            spLine.quantity = 1;
            spLine.unitMeasure = "st";
            spLine.unitAmountWithoutVat = 1000;
            spLine.vatPct = 25;
            spLine.totalVatAmount = 250;
            spLine.totalAmount = 1250;
            spLines[0] = spLine;
            paymentSpec.specLines = spLines;
            paymentSpec.totalAmount = 1250;
            paymentSpec.totalVatAmount = 250;
            paymentSpec.totalVatAmountSpecified = true;
            //create paymentSpecification;
            bookPaymentObject.PaymentSpec = paymentSpec;
            bookPaymentObject.MapEntry = null;

            var card = new cardData();
            card.cardNumber = "9000 0000 0010 0000";
            var signing = new signing()
            {
                failUrl = "http://google.com",
                forceSigning = false,
                forceSigningSpecified = false,
                successUrl = "http://google.com"
            };

            bookPaymentObject.Card = card;
            bookPaymentObject.Signing = signing;

            //extendedCustomer
            var extendedCustomer = new extendedCustomer();
            extendedCustomer.governmentId = "16066405994";
            extendedCustomer.address = new address();
            extendedCustomer.address.fullName = "David Smeichel";
            extendedCustomer.address.firstName = "David";
            extendedCustomer.address.lastName = "Smeichel";
            extendedCustomer.address.addressRow1 = "1st Infinite loop";
            extendedCustomer.address.addressRow2 = "2nd Infinite loop";
            extendedCustomer.address.postalArea = "norway";
            extendedCustomer.address.postalCode = "no";
            extendedCustomer.address.country = countryCode.NO;
            extendedCustomer.type = customerType.NATURAL;

            bookPaymentObject.ExtendedCustomer = extendedCustomer;
            return bookPaymentObject;
        }
    }
}
