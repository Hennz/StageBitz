<%@ Page DisplayTitle="Create New Company" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="CompanyDetails.aspx.cs" Inherits="StageBitz.UserWeb.Company.CompanyDetails" %>

<%@ Register Src="~/Controls/Common/FileUpload.ascx" TagName="FileUpload" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Company/CompanyDetails.ascx" TagName="CompanyDetails" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Company/CompanyHeaderDetails.ascx" TagName="CompanyHeaderDetails" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Company/CompanyWarningDisplay.ascx" TagPrefix="sb" TagName="CompanyWarningDisplay" %>
<%@ Register Src="~/Controls/Project/CreateNewProjectLink.ascx" TagPrefix="sb" TagName="CreateNewProjectLink" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    <span id="spnNewCompanyNavigation" runat="server">
        | <sb:CreateNewProjectLink runat="server" ID="lnkCreateNewProject" LinkText="Create New Project" />
    </span>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <sb:CompanyWarningDisplay runat="server" ID="sbCompanyWarningDisplay" />
    <uc:CompanyHeaderDetails runat="server" ID="CompanyHeaderDetails" />
    <uc:CompanyDetails runat="server" ID="ucCompanyDetails" />
    <asp:Button ID="btnSubmit" runat="server" Text="Done" CssClass="buttonStyle" ValidationGroup="FieldsValidation"
        OnClick="btnSubmit_Click" />
    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="buttonStyle" Visible="false"
        OnClick="btnCancel_Click" />
</asp:Content>
