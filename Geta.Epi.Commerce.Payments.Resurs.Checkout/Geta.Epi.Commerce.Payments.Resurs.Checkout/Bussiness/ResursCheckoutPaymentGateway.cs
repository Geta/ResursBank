using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Model;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Plugins.Payment;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Bussiness
{
    public class ResursCheckoutPaymentGateway: AbstractPaymentGateway
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ResursCheckoutPaymentGateway));

        private ResursCredential _resursCredential;
        internal ResursCredential ResursCredential
        {
            get
            {
                if (_resursCredential == null)
                {
                    _resursCredential = new ResursCredential(Settings[ResursConstants.UserName], Settings[ResursConstants.Password]);
                }
                Logger.Debug(string.Format("Active Klarna merchant id is {0}", ResursConstants.UserName));
                return _resursCredential;
            }
        }

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            try
            { 
                Logger.Debug("Resurs checkout gateway. Processing Payment ....");
                if (VerifyConfiguration())
                {
                    var factory = ServiceLocator.Current.GetInstance<IResursBankServiceSettingFactory>();
                    var resursBankServices = factory.Init(ResursCredential);
                }
                
            }
            catch (Exception exception)
            {
                Logger.Error("Process payment failed with error: " + exception.Message, exception);
                throw;
            }
            return true;
        }

        private bool VerifyConfiguration()
        {
            if (string.IsNullOrEmpty(Settings[ResursConstants.UserName]))
            {
                throw new PaymentException(PaymentException.ErrorType.ConfigurationError, "",
                                           "Payment configuration is not valid. Missing payment provider username.");
            }

            if (string.IsNullOrEmpty(Settings[ResursConstants.Password]))
            {
                throw new PaymentException(PaymentException.ErrorType.ConfigurationError, "",
                                           "Payment configuration is not valid. Missing payment provider password.");
            }
            Logger.Debug("Payment method configuuration verified.");
            return true;
        }
    }
}
