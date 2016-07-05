<%@ Page DisplayTitle="Budget Summary Report" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="BudgetSummaryReport.aspx.cs" Inherits="StageBitz.UserWeb.ItemBrief.BudgetSummaryReport" %>

<%@ Register TagPrefix="uc" TagName="BudgetList" Src="~/Controls/ItemBrief/BudgetList.ascx" %>
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
    <style>
        .BalanceColoumnHeader
        {
            padding-right: 20px !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    | <a id="lnkBookings" runat="server">Bookings</a>
    |<asp:HyperLink ID="hyperLinkTaskManager" runat="server">Task Manager</asp:HyperLink>
    |<sb:projectupdateslink id="projectUpdatesLink" runat="server" />
    <sb:reportlist id="reportList" runat="server" />
</asp:Content>
<asp:Content ID="ContentNavigation" runat="server" ContentPlaceHolderID="PageTitleRight">
    <asp:PlaceHolder ID="plcItemTypeDD" runat="server">
        <sb:projectitemtypes id="projectItemTypes" runat="server" />
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">

    <sb:projectwarningdisplay id="warningDisplay" runat="server" />
    <sb:ProjectWarningPopup id="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>

    <div style="float: left; padding-top: 22px; line-height: 25px; width: 645px;">
        <div>
            <h4>
                <asp:Label ID="lblProjectName" runat="server"></asp:Label>
            </h4>
        </div>
        <div style="width: 100%;">
            <h4>
                <asp:Label ID="lblCompanyName" runat="server"></asp:Label></h4>
        </div>
    </div>
    <div style="float: left;">
        <sb:exportdata id="ExportData1" runat="server" />
        <div style="margin-bottom: 20px;">
            <uc:BudgetList ID="budgetList" runat="server" />
        </div>
    </div>
    <div style="clear: both;">
    </div>
    <asp:UpdatePanel runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div>
                <telerik:RadGrid ID="gvItems" runat="server" OnSortCommand="gvItems_SortCommand" Width="930"
                    OnNeedDataSource="gvItems_NeedDataSource" OnItemDataBound="gvItems_ItemDataBound"
                    AllowSorting="true" SortToolTip="Click to sort" SortedBackColor="Transparent"
                    AllowAutomaticInserts="false" AutoGenerateColumns="False">
                    <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                    <MasterTableView DataKeyNames="ItemBriefID" Width="930" AllowNaturalSort="false">
                        <NoRecordsTemplate>
                            <div class="noData">
                                No data
                            </div>
                        </NoRecordsTemplate>
                        <SortExpressions>
                            <telerik:GridSortExpression FieldName="ItemType" SortOrder="Ascending" />
                        </SortExpressions>
                        <Columns>
                            <telerik:GridBoundColumn SortExpression="ItemType" DataField="ItemType" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left"
                                HeaderStyle-Width="150" HeaderText="Item Type" UniqueName="ItemType">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn SortExpression="ItemName" HeaderText="Item Name" HeaderStyle-HorizontalAlign="Left"
                                DataField="ItemName" UniqueName="ItemName">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkItemBriefDetails" Text="Item" Target="_blank" runat="server"></asp:HyperLink>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn DataField="Budget" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right"
                                SortExpression="Budget" HeaderStyle-Width="150" HeaderText="Budget" UniqueName="Budget">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Expended" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right"
                                SortExpression="Expended" HeaderStyle-Width="150" HeaderText="Expended" UniqueName="Expended">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="Remaining" SortExpression="Remaining"
                                DataType="System.Decimal" HeaderStyle-HorizontalAlign="Right" UniqueName="Remaining">
                                <ItemStyle HorizontalAlign="Right" Width="150" />
                                <HeaderStyle Width="150" />
                                <ItemTemplate>
                                    <img runat="server" class="WarningIconForFinance" id="imgNoEstimatedCost" title="Please check you've entered an estimated cost for each task." src="~/Common/Images/NoExpendedCostWarning.png" />
                                    <%#  StageBitz.UserWeb.Common.Helpers.Support.FormatCurrency(Eval("Remaining"),CultureName)%>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>

                            <telerik:GridBoundColumn DataField="Balance" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right"
                                SortExpression="Balance" HeaderStyle-Width="150" HeaderText="Balance" UniqueName="Balance">
                                <HeaderStyle CssClass="BalanceColoumnHeader" />
                                <ItemStyle CssClass="BalanceColoumnHeader" />

                            </telerik:GridBoundColumn>
                        </Columns>
                    </MasterTableView>
                    <ClientSettings>
                        <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="300" SaveScrollPosition="True"></Scrolling>
                    </ClientSettings>
                </telerik:RadGrid>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div style="float: right;">
        <asp:Button ID="btnDone" CssClass="buttonStyle" runat="server" Text="Done" OnClick="btnDone_Click" />
    </div>
</asp:Content>
