<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Geta.EPi.Payments.Klarna.CommerceManager.Apps.Order.Payments.Plugins.KlarnaCheckout.ConfigurePayment" %>

<h2>Merchant settings</h2>

<table class="DataForm">
    <tbody>
        <tr>
            <td class="FormLabelCell">Is production:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" id="cbIsProduction" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Merchant ID (EID):</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtMerchantId" />
                <asp:RequiredFieldValidator ID="val1" runat="server" ControlToValidate="txtMerchantId" ErrorMessage="The merchant ID is required." />
                <asp:CompareValidator ID="val2" runat="server" ErrorMessage="The merchant ID must be a positive integer."
                    ControlToValidate="txtMerchantId" Type="Integer" Operator="GreaterThan" ValueToCompare="0" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Shared secret:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtSecret" TextMode="Password" />
                <asp:RequiredFieldValidator ID="val3" runat="server" ControlToValidate="txtSecret" ErrorMessage="The shared secret is required." />
            </td>
        </tr>
    </tbody>
        <tr>
            <td class="FormLabelCell">Locale:</td>
            <td class="FormFieldCell">
                <asp:DropDownList runat="server" ID="ddlLocale" DataTextField="Text" DataValueField="Value" />
            </td>
        </tr>
    </tbody>
</table>