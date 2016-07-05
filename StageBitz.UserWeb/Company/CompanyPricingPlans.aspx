<%@ Page Title="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true" CodeBehind="CompanyPricingPlans.aspx.cs" Inherits="StageBitz.UserWeb.Company.CompanyPricingPlans" %>

<%@ Register Src="~/Controls/Company/PaymentPackageSelector.ascx" TagName="PaymentPackageSelector"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Company/PaymentPackageSummary.ascx" TagName="PaymentPackageSummary"
    TagPrefix="uc" %>
<%--<%@ Register Src="~/Controls/Company/PaymentDetails.ascx" TagPrefix="uc" TagName="PaymentDetails" %>--%>
<%@ Register Src="~/Controls/Company/PaymentDetailsValidation.ascx" TagName="PaymentValidation"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Project/CreateNewProjectLink.ascx" TagPrefix="sb" TagName="CreateNewProjectLink" %>
<%@ Register Src="~/Controls/Company/CompanyWarningDisplay.ascx" TagPrefix="sb" TagName="CompanyWarningDisplay" %>
<%@ Register Src="~/Controls/Common/FutureRequestNotificationMessage.ascx" TagPrefix="sb" TagName="FutureRequestNotificationMessage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    <span runat="server" id="spnCreateNewProject">|<sb:CreateNewProjectLink runat="server" ID="lnkCreateNewProject" LinkText="Create New Project" />
    </span>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageTitleRight" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageContent" runat="server">
    <sb:CompanyWarningDisplay runat="server" ID="sbCompanyWarningDisplay" />
      <sb:FutureRequestNotificationMessage runat="server" ID="sbFutureRequestNotificationMessage" />
    <div style="width: 520px;" class="left">
        <asp:Label ID="lblMsg" runat="server"></asp:Label>

    </div>
    <div class="left">
        <uc:PaymentPackageSummary ID="paymentPackageSummary" runat="server" />
    </div>
    <div style="clear: both"></div>
    <asp:UpdatePanel runat="server" ID="upnlPaymentPackage" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:PaymentPackageSelector ID="paymentPackageSelector" runat="server" />
            <asp:Button ID="btnNext" OnClick="btnNext_Click" CssClass="ignoreDirtyFlag buttonStyle"
                ValidationGroup="ItemBriefFields" runat="server" Text="Next" />
            <asp:Button ID="btnCancel" CssClass="buttonStyle" runat="server" Text="Cancel" OnClick="btnCancel_Click" />
            <uc:PaymentValidation runat="server" ID="paymentValidation" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <div class="left" style="font-size: 11px;">* All prices are shown in Australian Dollars.</div>
</asp:Content>
