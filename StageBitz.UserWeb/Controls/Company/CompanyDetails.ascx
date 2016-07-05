<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompanyDetails.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.CompanyDetails" %>

<%@ Register Src="~/Controls/Common/CountryList.ascx" TagName="CountryList" TagPrefix="uc1" %>

<div runat="server" id="divLeftSection">
    <table>
        <tr>
            <td class="mandatory" style="vertical-align: top; padding-top: 5px;">*
            </td>
            <td style="width: 150px; vertical-align: top; padding-top: 5px;">Company Name:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtCompanyName" MaxLength="50" Width="250"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvCompanyName" runat="server" ControlToValidate="txtCompanyName"
                    ErrorMessage="Company Name is required." ValidationGroup="FieldsValidation"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td class="mandatory" style="vertical-align: top; padding-top: 5px;">*
            </td>
            <td style="width: 150px; vertical-align: top; padding-top: 5px;">Address Line 1:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtAddressLine1" MaxLength="100" Width="250"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvAddress1" runat="server" ControlToValidate="txtAddressLine1"
                    ErrorMessage="Address Line1 is required." ValidationGroup="FieldsValidation"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td></td>
            <td style="width: 150px">Address Line 2:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtAddressLine2" MaxLength="100" Width="250"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="mandatory" style="vertical-align: top; padding-top: 5px;">*
            </td>
            <td style="width: 150px; vertical-align: top; padding-top: 5px;">City:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtCity" MaxLength="50" Width="250"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvCity" runat="server" ControlToValidate="txtCity"
                    ErrorMessage="City is required." ValidationGroup="FieldsValidation"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td class="mandatory" style="vertical-align: top; padding-top: 5px;">*
            </td>
            <td style="width: 150px; vertical-align: top; padding-top: 5px;">State:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtState" MaxLength="50" Width="250"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvState" runat="server" ControlToValidate="txtState"
                    ErrorMessage="State is required." ValidationGroup="FieldsValidation"></asp:RequiredFieldValidator>
            </td>
        </tr>
    </table>
</div>
<div runat="server" id="divSeperator" style="float: left; width: 25px; height: 150px;">
</div>
<div runat="server" id="divRightSection">
    <table>
        <tr>
            <td class="mandatory" style="vertical-align: top; padding-top: 5px;">*
            </td>
            <td style="width: 150px; vertical-align: top; padding-top: 5px;">Postal/Zip Code:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtPostalCode" MaxLength="20" Width="250"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvPostalCode" runat="server" ControlToValidate="txtPostalCode"
                    ErrorMessage="Postal Code is required." ValidationGroup="FieldsValidation"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td class="mandatory" style="vertical-align: top; padding-top: 5px;">*
            </td>
            <td style="width: 150px; vertical-align: top; padding-top: 5px;">Country:
            </td>
            <td colspan="2">
                <uc1:CountryList ID="ucCountryList" runat="server" />
            </td>
        </tr>
        <tr>
            <td class="mandatory" style="vertical-align: top; padding-top: 5px;">*
            </td>
            <td style="width: 150px; vertical-align: top; padding-top: 5px;">Company Phone:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtCompanyPhone" MaxLength="20" Width="250"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvCompanyPhone" runat="server" ControlToValidate="txtCompanyPhone"
                    ErrorMessage="Company Phone is required." ValidationGroup="FieldsValidation"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="revCompanyPhone" runat="server" ValidationGroup="FieldsValidation"
                    ErrorMessage="Please enter valid Phone Number." ValidationExpression="^(\+)?((\()?[0-9](\))?(\s)?)*$"
                    ControlToValidate="txtCompanyPhone"></asp:RegularExpressionValidator>
            </td>
        </tr>
        <tr runat="server" id="trPersonalPhone">
            <td class="mandatory" style="vertical-align: top; padding-top: 5px;">*
            </td>
            <td style="width: 150px; vertical-align: top; padding-top: 5px;">Personal Phone:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtPersonalPhone" MaxLength="20" Width="230"></asp:TextBox>
                <div style="display: inline-block; position:relative; top:2px;">
                    <sb:HelpTip runat="server" ID="helpTipPersonalPhone" Width="300">
                        We need these details so we can contact you if we have any questions regarding the invoice.
                    </sb:HelpTip>
                </div>
                <asp:RequiredFieldValidator ID="rfvPersonalPhone" runat="server" ControlToValidate="txtPersonalPhone"
                    ErrorMessage="Personal Phone is required." ValidationGroup="FieldsValidation"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="revPersonalPhone" runat="server" ValidationGroup="FieldsValidation"
                    ErrorMessage="Please enter valid Phone Number." ValidationExpression="^(\+)?((\()?[0-9](\))?(\s)?)*$"
                    ControlToValidate="txtPersonalPhone"></asp:RegularExpressionValidator>
            </td>
        </tr>
        <tr>
            <td></td>
            <td style="width: 150px; vertical-align: top; padding-top: 5px;">Website:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtWebsite" MaxLength="100" Width="230"></asp:TextBox>
                <div style="display: inline-block; position:relative; top:2px;">
                    <sb:HelpTip runat="server" ID="helpTipWebSite"  Width="300">
                        We need these details so we can contact you if we have any questions regarding the invoice.
                    </sb:HelpTip>
                </div>
                <asp:RegularExpressionValidator ID="revWebSite" runat="server" ValidationGroup="FieldsValidation"
                    ErrorMessage="Please enter valid Url." ValidationExpression="^[-a-zA-Z0-9@:%_\+.~#?&//=]{2,256}\.[a-z]{2,4}\b((\/)?[-a-zA-Z0-9@:%_\+.~#?&//=]*)?"
                    ControlToValidate="txtWebsite"></asp:RegularExpressionValidator>
            </td>
        </tr>
    </table>
</div>
<div style="clear: both;"></div>

