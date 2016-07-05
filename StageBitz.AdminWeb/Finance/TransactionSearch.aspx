<%@ Page DisplayTitle="Transaction Search" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="TransactionSearch.aspx.cs" Inherits="StageBitz.AdminWeb.Finance.TransactionSearch" %>

<%@ Register Src="~/Controls/Common/TransactionSearch.ascx" TagName="TransactionSearch"
    TagPrefix="uc" %>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <uc:TransactionSearch ID="transactionSearch" runat="server" />
</asp:Content>
