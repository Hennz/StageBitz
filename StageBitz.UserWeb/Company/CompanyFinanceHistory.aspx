<%@ Page DisplayTitle="Transaction History" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true" CodeBehind="CompanyFinanceHistory.aspx.cs" Inherits="StageBitz.UserWeb.Company.CompanyFinanceHistory" %>

<%@ Register Src="~/Controls/Finance/TransactionSearch.ascx" TagName="TransactionSearch" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Company/CompanyWarningDisplay.ascx" TagPrefix="sb" TagName="CompanyWarningDisplay" %>
<%@ Register Src="~/Controls/Project/CreateNewProjectLink.ascx" TagPrefix="sb" TagName="CreateNewProjectLink" %>
<%@ Register Src="~/Controls/Company/PlanMonitor.ascx" TagName="PlanMonitor"
    TagPrefix="uc" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    |<a id="lnkCompanyBilling" runat="server">Company Billing</a>
    <span id="spanCreateNewProject" runat="server">|
        <sb:CreateNewProjectLink runat="server" ID="lnkCreateNewProject" LinkText="Create New Project" />
    </span>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageTitleRight" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageContent" runat="server">
    <sb:CompanyWarningDisplay runat="server" ID="sbCompanyWarningDisplay" />
    <div style="width: 370px; float: right;">
        <uc:PlanMonitor ID="planMonitor" runat="server" />
    </div>
    <div style="clear:both;"></div>
    <div style="margin-top: 10px;"></div>
    <sb:TransactionSearch ID="transactionSearch" runat="server" />
    <div class="left" style="font-size: 11px;">* All prices are shown in Australian Dollars.</div>
</asp:Content>
