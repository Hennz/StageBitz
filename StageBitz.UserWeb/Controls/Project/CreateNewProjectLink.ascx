<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CreateNewProjectLink.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Project.CreateNewProjectLink" %>
<%@ Register Src="~/Controls/Company/PackageLimitsValidation.ascx" TagPrefix="uc" TagName="PackageLimitsValidation" %>

<asp:UpdatePanel ID="upnlCreateNewProject" UpdateMode="Always" runat="server" RenderMode="Inline">
    <ContentTemplate>
        <asp:LinkButton ID="lbtnCreateNewProject" runat="server" Text="Create New Project" OnClick="lbtnCreateNewProject_Click"></asp:LinkButton>
        <uc:PackageLimitsValidation runat="server" ID="ucPackageLimitsValidation" />
    </ContentTemplate>
</asp:UpdatePanel>

