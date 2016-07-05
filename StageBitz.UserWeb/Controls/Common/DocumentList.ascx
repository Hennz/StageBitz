<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocumentList.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Common.DocumentList" %>

<asp:ListView ID="lvDocuments" OnItemDataBound="lvDocuments_ItemDataBound" OnItemCommand="lvDocuments_ItemCommand" runat="server">
    <LayoutTemplate>
        <table class="imageList">
            <tr>
                <td ID="itemPlaceholder" runat="server">
                </td>
            </tr>
        </table>
    </LayoutTemplate>
    <EmptyDataTemplate>
        <div class="grayText" style="padding:5px;">You don't have any uploaded files.</div>
    </EmptyDataTemplate>
    <ItemTemplate>
        <td>
            <asp:LinkButton ID="lnkbtnDocument" ClientIDMode="AutoID" Enabled="false" CommandName="Pick" runat="server">
            
                <sb:ImageDisplay ID="thumbImage" ShowImagePreview="true" runat="server" />
            
            </asp:LinkButton>
        </td>
    </ItemTemplate>
</asp:ListView>

