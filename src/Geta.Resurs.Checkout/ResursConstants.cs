namespace Geta.Resurs.Checkout
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
        public const string AmountForNewCard = "AmountForNewCard";
        public const string MinLimit = "MinLimit";
        public const string MaxLimit = "MaxLimit";
        public const string CallBackUrl = "CallBackUrl";
        public const string ResursVatPercent = "ResursVatPercent";
        public const string LineItemExClass = "LineItemEx";
        public const string InvoiceDeliveryType = "InvoiceDeliveryType";

    }
    public static class ResursPaymentMethodType
    {
        public const string CARD = "CARD";
        public const string NEWCARD = "NEWCARD";
        public const string PARTPAYMENT = "PARTPAYMENT";
        public const string INVOICE = "INVOICE";
    }

    public static class UnitMeasureType
    {
        public const string ST = "st";
    }
}
