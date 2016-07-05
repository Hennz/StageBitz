<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ItemVisibilityToolTip.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Inventory.ItemVisibilityToolTip" %>
<i class="fa fa-eye" runat="server" id="iEyeIcon" style="cursor:pointer;"></i>
<telerik:RadToolTip runat="server" ID="itemVisibilityTooltip" TargetControlID="iEyeIcon">
    <div>
        <asp:UpdatePanel runat="server" ID="upnlVisibility" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Literal runat="server" ID="ltrlVisibility"></asp:Literal>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>    
</telerik:RadToolTip>

