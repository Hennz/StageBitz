<%@ Page DisplayTitle="Admin Dashboard" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="StageBitz.AdminWeb._Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageNavigationLinks" runat="server">
| <a id="A1" href="~/System/SystemSettings.aspx" runat="server">System Settings</a>
|<a id="lnkManageDiscountCodes" href="~/Company/ManageDiscountCodes.aspx" runat="server">Manage Discount Codes</a>
|<a id="lnkItemTypeFieldPreferences" href="~/ItemTypes/ItemTypeFieldPreferences.aspx" runat="server">Field Preferences</a>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" runat="server">

<div class="boxedItemDisplay" style="margin:0px auto; width:600px;">
    
    <a href="~/User/Users.aspx" runat="server">
        <div style="text-align:center; float:left;">
            <div class="dashboardBox">
                <asp:Literal ID="ltrlUserStats" runat="server"></asp:Literal>
            </div>
            Users
        </div>
    </a>

    <a href="~/Company/Companies.aspx" runat="server">
        <div style="text-align:center; float:left;">
            <div class="dashboardBox">
                <asp:Literal ID="ltrlCompanyStats" runat="server"></asp:Literal>
            </div>
            Companies
        </div>
    </a>

    <a href="~/Finance/TransactionSearch.aspx" style="display:block; margin-top:15px;" runat="server">
        <div style="text-align:center; float:left;">
            <div class="dashboardBox">
                <div style="margin-top:35px;">Transaction Search</div>
            </div>
            Finance
        </div>
    </a>

    <div style="clear:both;"></div>
</div>

</asp:Content>