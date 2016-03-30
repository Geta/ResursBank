using Geta.Epi.Commerce.Payments.Resurs.Checkout;
using Geta.EPi.Commerce.Payments.ResursBank.Checkout.Extensions;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;
//using Geta.Epi.Commerce.Payments.Resurs.Checkout.Extensions;

namespace Geta.EPi.Payments.Resurs.CommerceManager.Apps.Order.Payments.Plugins.ResursBankCheckout
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        public string ValidationGroup { get; set; }

        public void LoadObject(object dto)
        {
            var paymentMethod = dto as PaymentMethodDto;
            if (paymentMethod == null)
                return;
            txtUserName.Text = paymentMethod.GetParameter(ResursConstants.UserName, "");
            txtPassword.Text = paymentMethod.GetParameter(ResursConstants.Password, "");
        }

        public void SaveChanges(object dto)
        {
            if (!Visible)
                return;

            var paymentMethod = dto as PaymentMethodDto;
            if (paymentMethod == null)
                return;

            paymentMethod.SetParameter(ResursConstants.UserName, txtUserName.Text);
            paymentMethod.SetParameter(ResursConstants.Password, txtPassword.Text);
        }
    }
}