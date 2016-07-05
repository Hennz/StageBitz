<%@ Page Title="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true" CodeBehind="ExportFiles.aspx.cs" Inherits="StageBitz.UserWeb.Company.ExportFiles" %>
<%@ Register TagPrefix="uc" TagName="ExportFilesList" Src="~/Controls/Company/ExportFiles.ascx" %>
<%@ Register Src="~/Controls/Project/CreateNewProjectLink.ascx" TagPrefix="sb" TagName="CreateNewProjectLink" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    <span id="spnNewCompanyNavigation" runat="server">
        | <sb:CreateNewProjectLink runat="server" ID="lnkCreateNewProject" LinkText="Create New Project" />
    </span>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageTitleRight" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageContent" runat="server">
    <uc:ExportFilesList ID="exportFilesList" runat="server" />
</asp:Content>
