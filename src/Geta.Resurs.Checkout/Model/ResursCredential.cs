namespace Geta.Resurs.Checkout.Model
{
    public class ResursCredential
    {
        public ResursCredential()
        {
            
        }
        public ResursCredential(string username, string password)
        {
            UserName = username;
            Password = password;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
