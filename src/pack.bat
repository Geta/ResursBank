@echo off
set outputDir=%~dp0

if not "%1"=="" (
  set outputDir=%1
)


nuget pack Geta.Epi.Commerce.Payments.Resurs.Checkout\Geta.Epi.Commerce.Payments.Resurs.Checkout.csproj -IncludeReferencedProjects
nuget pack Geta.EPi.Payments.Resurs.CommerceManager\Geta.EPi.Payments.Resurs.CommerceManager.csproj -IncludeReferencedProjects

@echo on