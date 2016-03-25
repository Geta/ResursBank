using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geta.Resurs.Checkout.AfterShopFlowService;
using Geta.Resurs.Checkout.Model;
using Geta.Resurs.Checkout.SimplifiedShopFlowService;
using address = Geta.Resurs.Checkout.SimplifiedShopFlowService.address;
using countryCode = Geta.Resurs.Checkout.SimplifiedShopFlowService.countryCode;
using customerType = Geta.Resurs.Checkout.SimplifiedShopFlowService.customerType;
using invoiceDeliveryTypeEnum = Geta.Resurs.Checkout.AfterShopFlowService.invoiceDeliveryTypeEnum;
using paymentMethodType = Geta.Resurs.Checkout.AfterShopFlowService.paymentMethodType;
using paymentSpec = Geta.Resurs.Checkout.SimplifiedShopFlowService.paymentSpec;
using specLine = Geta.Resurs.Checkout.SimplifiedShopFlowService.specLine;

namespace Geta.Resurs.Checkout
{
    public class ResursBankServiceClient : IResursBankServiceClient
    {
        private SimplifiedShopFlowWebServiceClient _shopServiceClient;
        private AfterShopFlowWebServiceClient _afterShopServiceClient;


        public ResursBankServiceClient(ResursCredential credential)
        {
            _shopServiceClient = new SimplifiedShopFlowWebServiceClient();
            _afterShopServiceClient = new AfterShopFlowWebServiceClient();
            if (credential != null)
            {
                // TODO: Chage to get value directly when code complete
                var appSettings = ConfigurationManager.AppSettings;
                _shopServiceClient.ClientCredentials.UserName.UserName = credential.UserName ?? appSettings["username"] ?? "Not Found";
                _shopServiceClient.ClientCredentials.UserName.Password = credential.Password ?? appSettings["password"] ?? "Not Found";

                _afterShopServiceClient.ClientCredentials.UserName.UserName = credential.UserName ?? appSettings["username"] ?? "Not Found";
                _afterShopServiceClient.ClientCredentials.UserName.Password = credential.Password ?? appSettings["password"] ?? "Not Found";
            }
        }

        public List<PaymentMethodResponse> GetPaymentMethods(string lang, string custType, decimal amount)
        {
            if (_shopServiceClient == null || _shopServiceClient.ClientCredentials == null)
                return null;

            var paymentMethodList = new List<PaymentMethodResponse>();
            language langEnum = (language)System.Enum.Parse(typeof(language), lang);
            customerType customerTypeEnum = (customerType)System.Enum.Parse(typeof(customerType), custType);

            var paymentMethods = _shopServiceClient.getPaymentMethods(langEnum, customerTypeEnum, amount);
            _shopServiceClient.Close();
            foreach (var paymentMethod in paymentMethods)
            {
                var paymentMethodResponse = new PaymentMethodResponse(paymentMethod.id, paymentMethod.description, paymentMethod.minLimit, paymentMethod.maxLimit, paymentMethod.specificType);
                paymentMethodList.Add(paymentMethodResponse);
            }
            return paymentMethodList;

        }

        public bookPaymentResult BookPayment(string paymentMethodId, string customerIpAddress, List<SpecLine> specLines, Customer customer, string successUrl, string failUrl, bool forceSigning, string callBackUrl)
        {
            var paymentData = new paymentData();
            paymentData.paymentMethodId = paymentMethodId;
            paymentData.customerIpAddress = customerIpAddress;

            //paymentspec
            var paymentSpec = new paymentSpec();

            specLine[] spLines = new specLine[specLines.Count];
            var i = 0;
            decimal totalAmount = 0;
            decimal totalVatAmount = 0;
            foreach (var specLine in specLines)
            {
                var spLine = new specLine();
                spLine.id = specLine.Id;
                spLine.artNo = specLine.ArtNo;
                spLine.description = specLine.Description;
                spLine.quantity = specLine.Quantity;
                spLine.unitMeasure = specLine.UnitMeasure;
                spLine.unitAmountWithoutVat = specLine.UnitAmountWithoutVat;
                spLine.vatPct = specLine.VatPct;
                spLine.totalVatAmount = specLine.TotalVatAmount;
                spLine.totalAmount = specLine.TotalAmount;
                totalAmount += specLine.TotalAmount;
                totalVatAmount += specLine.TotalVatAmount;
                spLines[i] = spLine;
                i++;
            }
            paymentSpec.specLines = spLines;
            paymentSpec.totalAmount = totalAmount;
            paymentSpec.totalVatAmount = totalVatAmount;
            paymentSpec.totalVatAmountSpecified = true;
            //extendedCustomer
            var extendedCustomer = new extendedCustomer();
            extendedCustomer.governmentId = customer.GovernmentId;
            extendedCustomer.address = new address();
            extendedCustomer.address.fullName = customer.Address.FullName;
            extendedCustomer.address.firstName = customer.Address.FirstName;
            extendedCustomer.address.lastName = customer.Address.LastName;
            extendedCustomer.address.addressRow1 = customer.Address.AddressRow1;
            extendedCustomer.address.addressRow2 = customer.Address.AddressRow2;
            extendedCustomer.address.postalArea = customer.Address.PostalArea;
            extendedCustomer.address.postalCode = customer.Address.PostalCode;
            countryCode cCode = (countryCode)System.Enum.Parse(typeof(countryCode), customer.Address.Country);
            extendedCustomer.address.country = cCode;

            extendedCustomer.phone = customer.Phone;
            extendedCustomer.email = customer.Email;
            customerType cType = (customerType)System.Enum.Parse(typeof(customerType), customer.Type);
            extendedCustomer.type = cType;
            //signinng
            var signing = new signing();
            signing.successUrl = successUrl;
            signing.failUrl = failUrl;
            signing.forceSigning = forceSigning;


            return _shopServiceClient.bookPayment(paymentData, paymentSpec, null, extendedCustomer, null, signing, null, callBackUrl);
        }

        public bookPaymentResult BookSignedPayment(string paymentId)
        {
            return _shopServiceClient.bookSignedPayment(paymentId);
        }

        public payment GetPayment(string paymentId)
        {
            return _afterShopServiceClient.getPayment(paymentId);
        }

        public void FinalizePayment(string paymentId, string preferredTransactionId, List<SpecLine> specLines, string createdBy,
            string orderId, DateTime orderDate, string invoiceId, DateTime invoiceDate, invoiceDeliveryTypeEnum invoiceDeliveryType)
        {
            //paymentspec
            var paymentInfor = GetPayment(paymentId);
            if (paymentId != null && !string.IsNullOrEmpty(paymentInfor.id))
            {
                var paymentSpec = new Geta.Resurs.Checkout.AfterShopFlowService.paymentSpec();
                Geta.Resurs.Checkout.AfterShopFlowService.specLine[] spLines = new Geta.Resurs.Checkout.AfterShopFlowService.specLine[specLines.Count];
                var i = 0;
                decimal totalAmount = 0;
                decimal totalVatAmount = 0;
                foreach (var specLine in specLines)
                {
                    var spLine = new Geta.Resurs.Checkout.AfterShopFlowService.specLine();
                    spLine.id = specLine.Id;
                    spLine.artNo = specLine.ArtNo;
                    spLine.description = specLine.Description;
                    spLine.quantity = specLine.Quantity;
                    spLine.unitMeasure = specLine.UnitMeasure;
                    spLine.unitAmountWithoutVat = specLine.UnitAmountWithoutVat;
                    spLine.vatPct = specLine.VatPct;
                    spLine.totalVatAmount = specLine.TotalVatAmount;
                    spLine.totalAmount = specLine.TotalAmount;
                    totalAmount += specLine.TotalAmount;
                    totalVatAmount += specLine.TotalVatAmount;
                    spLines[i] = spLine;
                    i++;
                }
                paymentSpec.specLines = spLines;
                paymentSpec.totalAmount = totalAmount;
                paymentSpec.totalVatAmount = totalVatAmount;
                paymentSpec.totalVatAmountSpecified = true;

                if (paymentInfor.paymentMethodType != paymentMethodType.INVOICE)
                {
                    _afterShopServiceClient.finalizePayment(paymentId, preferredTransactionId, paymentSpec, createdBy,
                        orderId,
                        null, null, null, invoiceDeliveryType);
                }
                else
                {
                    _afterShopServiceClient.finalizePayment(paymentId, preferredTransactionId, paymentSpec, createdBy, orderId,
                        orderDate, invoiceId, invoiceDate, invoiceDeliveryType);
                }
                
                
                    
            }
            
        }

        public address GetAddress(string governmentId, string customerType, string customerIpAddress)
        {
            customerType cType = (customerType)System.Enum.Parse(typeof(customerType), customerType);
            return _shopServiceClient.getAddress(governmentId, cType, customerIpAddress);
        }
    }
}
