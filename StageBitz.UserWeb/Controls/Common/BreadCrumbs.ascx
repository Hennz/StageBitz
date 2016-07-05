<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BreadCrumbs.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Common.BreadCrumbs" %>
<asp:UpdatePanel runat="server" ID="upnlBreadCrumbs" UpdateMode="Conditional">
    <ContentTemplate>
        <ol>
            <asp:Repeater ID="repeater" OnItemDataBound="repeater_ItemDataBound" runat="server">
                <ItemTemplate>
                    <li>
                        <asp:HyperLink ID="hypLink" runat="server"></asp:HyperLink>
                        <asp:Literal ID="ltrlTitle" runat="server"></asp:Literal>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ol>
    </ContentTemplate>
</asp:UpdatePanel>
