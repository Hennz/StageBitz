<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ItemBriefTypeSummary.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Project.ItemBriefTypeSummary" %>
<asp:Repeater OnItemDataBound="rptItemBriefTypes_ItemDataBound" runat="server" ID="rptItemBriefTypes">
    <HeaderTemplate>
        <table>
    </HeaderTemplate>
    <ItemTemplate>
        <tr>
            <td>
                <b>
                    <asp:Label runat="server" ID="lblItemTypeName"></asp:Label></b>
            </td>
            <td>&nbsp;:&nbsp;
                    <asp:Label runat="server" ID="lblItemTypeSummary"></asp:Label>
            </td>
        </tr>
    </ItemTemplate>
    <FooterTemplate></table></FooterTemplate>
</asp:Repeater>
