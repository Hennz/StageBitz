<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectItemTypes.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Project.ProjectItemTypes" %>
    <div id="divItemTypePanel" runat="server" >
    Displaying: &nbsp;
<sb:DropDownListOPTGroup Width="150" ID="ddItemTypes" AutoPostBack="true" OnSelectedIndexChanged="ddItemTypes_OnSelectedIndexChanged" runat="server">
</sb:DropDownListOPTGroup>
</div>
