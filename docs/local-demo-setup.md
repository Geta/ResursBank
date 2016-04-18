# Local demo setup

1. Clone the develop branch to C:\Projects\ResursBank
2. In IIS add resursBank.localtest.me -> C:\Projects\ResursBank\demo\Quicksilver\Sources\EPiServer.Reference.Commerce.Site
   and manager.ResursBank.localtest.me -> C:\Projects\ResursBank\demo\Quicksilver\Sources\EPiServer.Reference.Commerce.Manager
3. Open up C:\Projects\ResursBank\demo\Quicksilver\Quicksilver.sln in Visual Studio 2015. Build (restore packages).
4. Restore databases (C:\Projects\ResursBank\setup\demo) using default credentials and name from C:\Projects\ResursBank\demo\Quicksilver\Sources\EPiServer.Reference.Commerce.Site\connectionStrings.config).
5. Login with resursBank.localtest.me/episerver to login with admin/store   