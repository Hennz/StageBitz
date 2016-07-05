<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CountryList.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Common.CountryList" %>
<asp:UpdatePanel ID="up" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div style="float: left;margin-bottom:2px; margin-right: 5px; height:22px;">
            <asp:DropDownList ID="ddCountry" onchange="return DisablePostBackForLit(this.id);" OnSelectedIndexChanged="ShowAllCountries" DataTextField="CountryName" DataValueField="CountryID"
                runat="server">
            </asp:DropDownList>
        </div>
        <div id="divRight" runat="server" style="padding-top: 4px;float: left;">
            <asp:RequiredFieldValidator ID="rqdRight" ValidationGroup="FieldsValidation" InitialValue="-1"
                ErrorMessage="Please select a Country." ControlToValidate="ddCountry" runat="server"></asp:RequiredFieldValidator>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
