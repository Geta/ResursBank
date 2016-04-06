using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout
{
    public static class ResursConstants
    {
        public static readonly string UserName = "UserName";
        public static readonly string Password = "Password";

        public const string OrderNamespace = "Mediachase.Commerce.Orders";
        public const string ResursBankPaymentMethod = "ResursBankPaymentMethod";
        public const string GovernmentId = "GovernmentId";
        public const string OtherPaymentClass = "OtherPayment";
        public const string ResursBankPayment = "ResursBankPayment";
        public const string VatPercent = "VatPercent";
        public const string OrderId = "OrderId";
        public const string ResursBankPaymentType = "ResursBankPaymentType";

        public const string PaymentResultCookieName = "ResursPaymentBookResult";
        public const string CardNumber = "CardNumber";
        public const string CustomerIpAddress = "CustomerIpAddress";
        public const string SuccessfullUrl = "SuccessfullUrl";
        public const string FailBackUrl = "FailBackUrl";
    }
}
