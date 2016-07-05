<%@ Page DisplayTitle="Dashboard" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StageBitz.UserWeb.Default" %>

<%@ Register TagPrefix="uc" TagName="CompanyList" Src="~/Controls/Company/CompanyList.ascx" %>
<%@ Register TagPrefix="uc" TagName="ProjectList" Src="~/Controls/Project/ProjectList.ascx" %>
<%@ Register TagPrefix="uc" TagName="ScheduleList" Src="~/Controls/Project/ScheduleList.ascx" %>
<%@ Register TagPrefix="uc" TagName="WelcomeMessage" Src="~/Controls/Common/WelcomeMessage.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" runat="server">
    <div id="divSuccess" runat="server" visible="false" class="message success" style="margin-bottom: 5px;">
    </div>
    <div id="divError" runat="server" visible="false" class="message error" style="margin-bottom: 5px;">
    </div>
    <asp:UpdatePanel runat="server" ID="upnlScheduleList" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:ScheduleList ID="scheduleList" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <uc:CompanyList ID="ucCompanyList" runat="server" />
    <uc:ProjectList ID="myProjectList" DisplayMode="UserDashboard" runat="server" />
    <uc:WelcomeMessage ID="welcomeMessage" runat="server" />
</asp:Content>
