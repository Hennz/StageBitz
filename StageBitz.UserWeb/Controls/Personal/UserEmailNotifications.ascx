<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserEmailNotifications.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Personal.UserEmailNotifications" %>

<asp:UpdatePanel ID="upnlEmailNotifications" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <p>
            StageBitz can send you email updates that show you the progress of each Project. 
            You can also check the latest status of any Project by going to the Project Dashboard and selecting the Updates Report from the Report panel.
        </p>
        <br />
        <p>
            How often would you like email updates for Projects?
            <div style="padding: 5px 15px">
                <asp:RadioButtonList runat="server" ID="rbtnListUpdatesForProjects" OnSelectedIndexChanged="rbtnListUpdatesForProjects_SelectedIndexChanged" AutoPostBack="true">
                </asp:RadioButtonList>
            </div>
        </p>
        <br />
        <p>
            You are a Company Administrator. You can choose to receive updates for any active Projects owned by a Company you are an Administrator for, 
            even if you are not directly working on these Projects. How often would you like to receive these updates?
            <div style="padding: 5px 15px">
                <asp:RadioButtonList runat="server" ID="rbtnListUpdatesForCompanies" OnSelectedIndexChanged="rbtnListUpdatesForCompanies_SelectedIndexChanged" AutoPostBack="true">
                </asp:RadioButtonList>
            </div>
        </p>
        <div id="emailNotifications" class="right inlineNotification">
            Changes saved.
        </div>
        <br style="clear:both;"/>
    </ContentTemplate>
</asp:UpdatePanel>
