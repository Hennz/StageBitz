<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PlanMonitor.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.PlanMonitor" %>
<sb:GroupBox ID="GroupBox1" Height="100" runat="server">
    <TitleLeftContent>
        <div class="left">
            Plan Monitor
        </div>
        <div class="left"  style="margin-left:120px; top:5px; margin-top:-5px; ">
            <asp:Button ID="btnViewPricingPlan" runat="server" CssClass="buttonStyle"  Text="View Pricing Plans" OnClick="btnViewPricingPlan_Click" />
        </div>
        <div style="clear:both;"></div>
    </TitleLeftContent>
    <BodyContent>
        <table id="tblPlanMonitor" style="width: 350px; height: 100px;" runat="server">
            <tr>
                <td>
                    <b>Your Plan Includes up to</b>
                </td>
                <td>
                    <b>You are currently using</b>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Literal ID="litActiveProjects" runat="server"></asp:Literal>
                </td>
                <td style="text-align: center;">
                    <asp:Literal ID="litCurrentProjectCount" runat="server"></asp:Literal>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Literal ID="litActiveUsers" runat="server"></asp:Literal>
                </td>
                <td style="text-align: center;">
                    <asp:Literal ID="litCurrentUserCount" runat="server"></asp:Literal>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Literal ID="litInventoryItems" runat="server"></asp:Literal>
                </td>
                <td style="text-align: center;">
                    <asp:Literal ID="litInvCurrentCount" runat="server"></asp:Literal>
                </td>
            </tr>
        </table>
        <div id="divDefaultText" style="text-align: center; width: 350px;" runat="server">
            <p runat="server" id="paraFreeTrailIndication" visible="false">You are still on your Free Trial.</p>
            <p>
                If you're ready to step it up, you can choose the Project & Inventory level that suits you.
            </p>
        </div>
    </BodyContent>
</sb:GroupBox>
