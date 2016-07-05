<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListViewDisplaySettings.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Common.ListViewDisplaySettings" %>

<asp:UpdatePanel ID="upnlDisplaySettings" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <asp:ImageButton ID="imgbtnThumbView" ToolTip="Thumbnail view" OnClick="imgbtnViewMode_Click" ImageUrl="~/Common/Images/viewtype_thumbs.png" runat="server" />
        <asp:ImageButton ID="imgbtnListView" ToolTip="List view" OnClick="imgbtnViewMode_Click" ImageUrl="~/Common/Images/viewtype_list.png" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>