<%@ Page DisplayTitle="Itemised Purchase Report" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="ItemisedPurchaseReport.aspx.cs" Inherits="StageBitz.UserWeb.ItemBrief.ItemisedPurchaseReport" %>

<%@ Register Src="~/Controls/Common/ExportData.ascx" TagName="ExportData" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectItemTypes.ascx" TagName="ProjectItemTypes" TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        if ('<%= this.CompanyId %>' != '0' && '<%= this.ProjectId %>' != '0') {
            var _gaq = _gaq || [];
            _gaq.push(['_setCustomVar', 1, 'Category', 'Project', 2]);
            _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyId %>', 2]);
            _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
            _gaq.push(['_setCustomVar', 4, 'ProjectId', '<%= this.ProjectId %>', 2]);
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
 | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
| <a id="lnkBookings" runat="server">Bookings</a>
|<asp:HyperLink ID="hyperLinkTaskManager" runat="server">Task Manager</asp:HyperLink>
|<sb:ProjectUpdatesLink ID="projectUpdatesLink" runat="server" />
<sb:ReportList ID="reportList" runat="server"/>
</asp:Content>
<asp:Content ID="ContentNavigation" runat="server" ContentPlaceHolderID="PageTitleRight">
    <asp:PlaceHolder ID="plcItemTypeDD" runat="server">
      <sb:ProjectItemTypes ID="projectItemTypes" runat="server" />
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">

    <sb:ProjectWarningDisplay ID="warningDisplay" runat="server" />    
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>
    <sb:ExportData ID="ExportData1" runat="server" />

    <div style="overflow-x: hidden;">
        <telerik:RadGrid ID="rgvItemisedPurchase" Width="99.8%" runat="server" OnSortCommand="rgvItemisedPurchase_SortCommand"
            OnItemDataBound="rgvItemisedPurchase_ItemDataBound" AllowSorting="true" SortToolTip="Click to sort"
            SortedBackColor="Transparent" OnNeedDataSource="rgvItemisedPurchase_NeedDataSource"
            AllowAutomaticUpdates="True" AutoGenerateColumns="False">
            <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
            <MasterTableView Width="100%" AllowNaturalSort="false">
                <NoRecordsTemplate>
                    <div class="noData">
                        You don't have completed tasks.
                    </div>
                </NoRecordsTemplate>
                <Columns>
                    <telerik:GridTemplateColumn SortExpression="ItemBriefTask.CompletedDate" UniqueName="CompletedDate"
                        HeaderText="Completed Date">
                        <ItemStyle Width="50" />
                        <ItemTemplate>
                            <%# StageBitz.UserWeb.Common.Helpers.Support.FormatDate(Eval("ItemBriefTask.CompletedDate"))%>
                        </ItemTemplate>
                        <HeaderStyle Width="50" />
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Item Name" SortExpression="ItemBriefName"
                        HeaderStyle-HorizontalAlign="Left" UniqueName="ItemName">
                        <HeaderStyle Width="58" />
                        <ItemTemplate>
                            <asp:HyperLink ID="hyperLinkItem" Text="Item" Target="_blank" runat="server"></asp:HyperLink>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Task Description" UniqueName="Description">
                        <ItemStyle Width="80" />
                        <ItemTemplate>
                        </ItemTemplate>
                        <HeaderStyle Width="80" />
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Vendor" SortExpression="ItemBriefTask.Vendor"
                        UniqueName="Vendor" EditFormColumnIndex="1">
                        <ItemStyle Width="50" />
                        <HeaderStyle Width="50" />
                        <ItemTemplate>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Net Cost" SortExpression="ItemBriefTask.NetCost" DataType="System.Decimal"
                        HeaderStyle-HorizontalAlign="Right" UniqueName="NetCost">
                        <ItemStyle HorizontalAlign="Right" Width="37" />
                        <HeaderStyle Width="37" />
                        <ItemTemplate>
                            <%# StageBitz.UserWeb.Common.Helpers.Support.FormatCurrency(Eval("ItemBriefTask.NetCost"),CultureName)%>                           
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Tax" SortExpression="ItemBriefTask.Tax" HeaderStyle-HorizontalAlign="Right" DataType="System.Decimal"
                        UniqueName="Tax" EditFormColumnIndex="1">
                        <ItemStyle HorizontalAlign="Right" Width="30" />
                        <HeaderStyle Width="30" />
                        <ItemTemplate>
                            <%# StageBitz.UserWeb.Common.Helpers.Support.FormatCurrency(Eval("ItemBriefTask.Tax"),CultureName)%>                           
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Total Cost" SortExpression="Total" HeaderStyle-HorizontalAlign="Right" DataType="System.Decimal"
                        UniqueName="Total" EditFormColumnIndex="1">
                        <ItemStyle HorizontalAlign="Right" Width="35" />
                        <HeaderStyle Width="35" />
                        <ItemTemplate> 
                              <%# StageBitz.UserWeb.Common.Helpers.Support.FormatCurrency(Eval("Total"),CultureName)%>                           
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                </Columns>
            </MasterTableView>
            <ClientSettings>
                <Scrolling AllowScroll="True" ScrollHeight="470px" UseStaticHeaders="True" SaveScrollPosition="True">
                </Scrolling>
            </ClientSettings>
        </telerik:RadGrid>
    </div>
    <div style="float: right;">
        <asp:Button ID="btnDone" CssClass="buttonStyle" runat="server" Text="Done" OnClick="btnDone_Click" />
    </div>
</asp:Content>
