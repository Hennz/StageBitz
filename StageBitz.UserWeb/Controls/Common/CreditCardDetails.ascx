<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CreditCardDetails.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Common.CreditCardDetails" %>

<div id="divCCDetailsEditableLabels" runat="server" class="left" style="line-height: 30px; margin-right: 10px; margin-left: 110px; margin-top: 10px; width: 160px; white-space: nowrap;">
    Cardholder Name:<br />
    Card Number:<br />
    Expiry Date (Month / Year):<br />
    Card Verification Code:<br />
</div>
<div id="divCCDetailsEditable" runat="server" class="left sideErrorContainer" style="line-height: 30px; width: 310px; margin-top: 9px;">
    <asp:TextBox ID="txtCardHolderName" MaxLength="255" Width="200" runat="server"></asp:TextBox>
    <asp:RequiredFieldValidator ID="reqCardHolderName" runat="server" ControlToValidate="txtCardHolderName"
        ErrorMessage="* Required"></asp:RequiredFieldValidator>
    <br />
    <asp:TextBox ID="txtCardNumber" ToolTip="Enter your credit card number" Width="200"
        runat="server"></asp:TextBox>
    <asp:CustomValidator ID="ccValidator" runat="server" ControlToValidate="txtCardNumber"
        ValidateEmptyText="true" ClientValidationFunction="ValidateCreditCardDetails"
        ErrorMessage="* Invalid"></asp:CustomValidator>
    <asp:RequiredFieldValidator ID="reqCardNumber" runat="server" ControlToValidate="txtCardNumber"
        ErrorMessage="* Required"></asp:RequiredFieldValidator><br />
    <asp:DropDownList ID="ddMonth" Width="80" runat="server">
    </asp:DropDownList>
    /
                <asp:DropDownList ID="ddYear" Width="70" runat="server">
                </asp:DropDownList>
    <div id="divNotificationDates" runat="server" class="inputError" style="display: inline-block; display: none; text-align: center;">
    </div>
    <br />
    <asp:TextBox ID="txtCVV" Width="100" MaxLength="3" runat="server"></asp:TextBox>
    &nbsp;
                <div style="display: inline-block;">
                    <sb:HelpTip ID="HelpTipCVV" runat="server">
                        <div style="text-align: left; width: 180px; line-height: 18px;">
                            Your card verification code is the last 3 digit number located on the back of your
                            card on or above your signature line.
                            <br />
                            <img id="Imgtooltip" style="margin-left: 5px;" runat="server" src="~/Common/Images/cvvtooltip.png"
                                alt="cvvtooltip" />
                        </div>
                    </sb:HelpTip>
                </div>
    <asp:RegularExpressionValidator ID="regexCVV" runat="server" ControlToValidate="txtCVV"
        ErrorMessage="* Invalid" ToolTip="Invalid" ValidationExpression="^[0-9]{3}$"></asp:RegularExpressionValidator>
    <asp:RequiredFieldValidator ID="reqCVV" runat="server" ControlToValidate="txtCVV"
        ErrorMessage="* Required"></asp:RequiredFieldValidator>
    <br />
</div>
<div style="clear: both;">
</div>
<div id="divNotification" runat="server" visible="true" class="inputError" style="text-align: center; margin-top: 5px; height: 10px; width: 450px;">
    &nbsp;
</div>
