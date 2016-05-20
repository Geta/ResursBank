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
        private ResursCredential NorwayCredentials
        {
            get
            {
                var appSettings = ConfigurationManager.AppSettings;

                return new ResursCredential(appSettings["ResursBank:Norway:UserName"], appSettings["ResursBank:Norway:Password"]);
            }
        }

        private ResursCredential SwedenCredentials
        {
            get
            {
                var appSettings = ConfigurationManager.AppSettings;

                return new ResursCredential(appSettings["ResursBank:Sweden:UserName"], appSettings["ResursBank:Sweden:Password"]);
            }
        }

        private ResursCredential FinlandCredentials
        {
            get
            {
                var appSettings = ConfigurationManager.AppSettings;

                return new ResursCredential(appSettings["ResursBank:Finland:UserName"], appSettings["ResursBank:Finland:Password"]);
            }
        }

        [Fact]
        public void GetPaymentMethodsNorway()
        {
            var resursBankServices = new ResursBankServiceClient(NorwayCredentials);
            List<PaymentMethodResponse> list = resursBankServices.GetPaymentMethods("no", "NATURAL", 1000);
            Assert.Equal(4,list.Count);
        }

        [Fact]
        public void GetPaymentMethodsSweden()
        {
            var resursBankServices = new ResursBankServiceClient(SwedenCredentials);
            List<PaymentMethodResponse> list = resursBankServices.GetPaymentMethods("sv", "NATURAL", 1000);
            Assert.Equal(4, list.Count);
        }

        

        [Fact]
        public void GetPaymentMethodsFinland()
        {
            var resursBankServices = new ResursBankServiceClient(FinlandCredentials);
            List<PaymentMethodResponse> list = resursBankServices.GetPaymentMethods("fi", "NATURAL", 1000);
            Assert.Equal(4, list.Count);
        }

        [Fact]
        public void BookPaymentNorway()
        {
            var resursBankServiceClient = new ResursBankServiceClient(NorwayCredentials);

            var bookPaymentObject = CreateBookPaymentObject("16066405994", PaymentSpecNorway);
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            Assert.NotEqual(bookPaymentStatus.DENIED, result.bookPaymentStatus);
        }

        [Fact]
        public void BookPaymentSweden()
        {
            var resursBankServiceClient = new ResursBankServiceClient(SwedenCredentials);

            var bookPaymentObject = CreateBookPaymentObject("194608282333", PaymentSpecSweden);
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            Assert.NotEqual(bookPaymentStatus.DENIED, result.bookPaymentStatus);
        }

        [Fact]
        public void BookPaymentFinland()
        {
            var resursBankServiceClient = new ResursBankServiceClient(FinlandCredentials);
            
            var bookPaymentObject = CreateBookPaymentObject("100370-897V", PaymentSpecFinland);
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            Assert.NotEqual(bookPaymentStatus.DENIED, result.bookPaymentStatus);
        }

        [Fact]
        public void BookSignedPaymentNorway()
        {
            var resursBankServiceClient = new ResursBankServiceClient(NorwayCredentials);

            var bookPaymentObject = CreateBookPaymentSignedObject(PaymentSpecNorway, CustomerNorway);
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            var resultSigning = resursBankServiceClient.BookSignedPayment(result.paymentId);
            Assert.NotEqual(bookPaymentStatus.DENIED, resultSigning.bookPaymentStatus);
        }

        [Fact]
        public void BookSignedPaymentSweden()
        {
            var resursBankServiceClient = new ResursBankServiceClient(SwedenCredentials);

            var bookPaymentObject = CreateBookPaymentSignedObject(PaymentSpecSweden, CustomerSweden);
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            var resultSigning = resursBankServiceClient.BookSignedPayment(result.paymentId);
            Assert.NotEqual(bookPaymentStatus.DENIED, resultSigning.bookPaymentStatus);
        }

        [Fact]
        public void BookSignedPaymentFinland()
        {
            var resursBankServiceClient = new ResursBankServiceClient(FinlandCredentials);

            var bookPaymentObject = CreateBookPaymentSignedObject(PaymentSpecFinland, CustomerFinland);
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            var resultSigning = resursBankServiceClient.BookSignedPayment(result.paymentId);
            Assert.NotEqual(bookPaymentStatus.DENIED, resultSigning.bookPaymentStatus);
        }

        private BookPaymentObject CreateBookPaymentSignedObject(paymentSpec paymentSpecification, extendedCustomer customer)
        {
            var bookPaymentObject = new BookPaymentObject
            {
                PaymentData = new paymentData
                {
                    paymentMethodId = "NEWCARD",
                    customerIpAddress = "127.0.0.1"
                }
            };

            //create paymentSpecification;
            bookPaymentObject.PaymentSpec = paymentSpecification;
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

            bookPaymentObject.ExtendedCustomer = customer;
            return bookPaymentObject;
        }

        private BookPaymentObject CreateBookPaymentObject(string governmentId, paymentSpec paymentSpecification)
        {
            var bookPaymentObject = new BookPaymentObject
            {
                PaymentData = new paymentData
                {
                    paymentMethodId = "CARD",
                    customerIpAddress = "127.0.0.1"
                }
            };
            
            //create paymentSpecification;
            bookPaymentObject.PaymentSpec = paymentSpecification;
            bookPaymentObject.MapEntry = null;

            var card = new cardData {cardNumber = "9000 0000 0010 0000"};
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
            var extendedCustomer = new extendedCustomer
            {
                governmentId = governmentId,
                address = new address
                {
                    fullName = "David Smeichel",
                    firstName = "David",
                    lastName = "Smeichel",
                    addressRow1 = "1st Infinite loop",
                    addressRow2 = "2nd Infinite loop",
                    postalArea = "norway",
                    postalCode = "no",
                    country = countryCode.NO
                },
                type = customerType.NATURAL
            };

            bookPaymentObject.ExtendedCustomer = extendedCustomer;
            return bookPaymentObject;
        }

        private paymentSpec PaymentSpecNorway
        {
            get
            {
                var spLine = new specLine
                {
                    id = "product01",
                    artNo = "sku-001",
                    description = "denim trunk",
                    quantity = 1,
                    unitMeasure = "st",
                    unitAmountWithoutVat = 1000,
                    vatPct = 25,
                    totalVatAmount = 250,
                    totalAmount = 1250
                };

                var paymentSpec = new paymentSpec();

                specLine[] spLines = new specLine[1];

                spLines[0] = spLine;
                paymentSpec.specLines = spLines;
                paymentSpec.totalAmount = 1250;
                paymentSpec.totalVatAmount = 250;
                paymentSpec.totalVatAmountSpecified = true;

                return paymentSpec;
            }
        }

        private paymentSpec PaymentSpecSweden
        {
            get
            {
                var spLine = new specLine
                {
                    id = "product01",
                    artNo = "sku-001",
                    description = "denim trunk",
                    quantity = 1,
                    unitMeasure = "st",
                    unitAmountWithoutVat = 1000,
                    vatPct = 25,
                    totalVatAmount = 250,
                    totalAmount = 1250
                };

                var paymentSpec = new paymentSpec();

                specLine[] spLines = new specLine[1];

                spLines[0] = spLine;
                paymentSpec.specLines = spLines;
                paymentSpec.totalAmount = 1250;
                paymentSpec.totalVatAmount = 250;
                paymentSpec.totalVatAmountSpecified = true;

                return paymentSpec;
            }
        }

        private paymentSpec PaymentSpecFinland
        {
            get
            {
                var spLine = new specLine
                {
                    id = "product01",
                    artNo = "sku-001",
                    description = "denim trunk",
                    quantity = 1,
                    unitMeasure = "st",
                    unitAmountWithoutVat = 1000,
                    vatPct = 24,
                    totalVatAmount = 240,
                    totalAmount = 1240
                };

                var paymentSpec = new paymentSpec();

                specLine[] spLines = new specLine[1];

                spLines[0] = spLine;
                paymentSpec.specLines = spLines;
                paymentSpec.totalAmount = 1240;
                paymentSpec.totalVatAmount = 240;
                paymentSpec.totalVatAmountSpecified = true;

                return paymentSpec;
            }
        }

        private extendedCustomer CustomerNorway
        {
            get
            {
                return new extendedCustomer
                {
                    governmentId = "010986-14741",
                    address = new address
                    {
                        fullName = "David Smeichel",
                        firstName = "David",
                        lastName = "Smeichel",
                        addressRow1 = "1st Infinite loop",
                        addressRow2 = "2nd Infinite loop",
                        postalArea = "norway",
                        postalCode = "no",
                        country = countryCode.NO
                    },
                    phone = "+4797674852",
                    email = "post@geta.no",
                    type = customerType.NATURAL
                };
            }
        }

        private extendedCustomer CustomerSweden
        {
            get
            {
                return new extendedCustomer
                {
                    governmentId = "194608282333",
                    address = new address
                    {
                        fullName = "David Smeichel",
                        firstName = "David",
                        lastName = "Smeichel",
                        addressRow1 = "1st Infinite loop",
                        addressRow2 = "2nd Infinite loop",
                        postalArea = "Sweden",
                        postalCode = "se",
                        country = countryCode.SE
                    },
                    phone = "+46 111111111",
                    email = "post@geta.no",
                    type = customerType.NATURAL
                };
            }
        }

        private extendedCustomer CustomerFinland
        {
            get
            {
                return new extendedCustomer
                {
                    governmentId = "261171-203H",
                    address = new address
                    {
                        fullName = "David Smeichel",
                        firstName = "David",
                        lastName = "Smeichel",
                        addressRow1 = "1st Infinite loop",
                        addressRow2 = "2nd Infinite loop",
                        postalArea = "Finland",
                        postalCode = "fi",
                        country = countryCode.FI
                    },
                    phone = "+3589111111",
                    email = "post@geta.no",
                    type = customerType.NATURAL
                };
            }
        }

        private ResursCredential DenmarkCredentials
        {
            get
            {
                var appSettings = ConfigurationManager.AppSettings;

                return new ResursCredential(appSettings["ResursBank:Denmark:UserName"], appSettings["ResursBank:Denmark:Password"]);
            }
        }

        [Fact]
        public void GetPaymentMethodsDenmark()
        {
            var resursBankServices = new ResursBankServiceClient(DenmarkCredentials);
            List<PaymentMethodResponse> list = resursBankServices.GetPaymentMethods("da", "NATURAL", 1000);
            Assert.Equal(4, list.Count);
        }

        /// <summary>
        /// Denmark is not ready yet
        /// </summary>
        //[Fact]
        public void BookPaymentDenmark()
        {
            var resursBankServiceClient = new ResursBankServiceClient(DenmarkCredentials);

            var bookPaymentObject = CreateBookPaymentObject("1502640867", PaymentSpecDenmark);
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            Assert.NotEqual(bookPaymentStatus.DENIED, result.bookPaymentStatus);
        }

        /// <summary>
        /// Denmark is not ready yet
        /// </summary>
        //[Fact]
        public void BookSignedPaymentDenmark()
        {
            var resursBankServiceClient = new ResursBankServiceClient(DenmarkCredentials);

            var bookPaymentObject = CreateBookPaymentSignedObject(PaymentSpecDenmark, CustomerDenmark);
            var result = resursBankServiceClient.BookPayment(bookPaymentObject);
            var resultSigning = resursBankServiceClient.BookSignedPayment(result.paymentId);
            Assert.NotEqual(bookPaymentStatus.DENIED, resultSigning.bookPaymentStatus);
        }

        private paymentSpec PaymentSpecDenmark
        {
            get
            {
                var spLine = new specLine
                {
                    id = "product01",
                    artNo = "sku-001",
                    description = "denim trunk",
                    quantity = 1,
                    unitMeasure = "st",
                    unitAmountWithoutVat = 1000,
                    vatPct = 25,
                    totalVatAmount = 250,
                    totalAmount = 1250
                };

                var paymentSpec = new paymentSpec();

                specLine[] spLines = new specLine[1];

                spLines[0] = spLine;
                paymentSpec.specLines = spLines;
                paymentSpec.totalAmount = 1250;
                paymentSpec.totalVatAmount = 250;
                paymentSpec.totalVatAmountSpecified = true;

                return paymentSpec;
            }
        }

        private extendedCustomer CustomerDenmark
        {
            get
            {
                return new extendedCustomer
                {
                    governmentId = "1502640867",
                    address = new address
                    {
                        fullName = "David Smeichel",
                        firstName = "David",
                        lastName = "Smeichel",
                        addressRow1 = "1st Infinite loop",
                        addressRow2 = "2nd Infinite loop",
                        postalArea = "Denmark",
                        postalCode = "dk",
                        country = countryCode.DK
                    },
                    phone = "+45 91-11-11-11",
                    email = "post@geta.no",
                    type = customerType.NATURAL
                };
            }
        }
    }
}
