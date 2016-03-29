using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geta.Resurs.Checkout.Model;

namespace Geta.Resurs.Checkout
{
    public class ResursBankServiceSettingFactory: IResursBankServiceSettingFactory
    {
        public ResursBankServiceClient Init(ResursCredential credential)
        {
            return new ResursBankServiceClient(credential);
        }
    }
}
