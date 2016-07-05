<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TransactionSearch.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Finance.TransactionSearch" %>

<script type="text/javascript">

    var isDateRangeValid = true;

    function onDateSelected(sender, eventArgs) {

        var dtpkFrom = $find("<%= dtpkFrom.ClientID %>");
        var dtpkTo = $find("<%= dtpkTo.ClientID %>");

        var fromDate = dtpkFrom.get_selectedDate();
        var toDate = dtpkTo.get_selectedDate();

        var divMsg = $("#<%= divCriteriaValidation.ClientID %>");
        divMsg.html("");

        if (toDate != null && fromDate != null && toDate < fromDate) {
            isDateRangeValid = false;
            divMsg.show();
            divMsg.html("Invalid date range.");
            IntializeErrorMessages();
        }
        else {
            isDateRangeValid = true;
            divMsg.hide();
        }
    }

    function ClearProjectCheckboxes() {
        $("#divProjectCheckboxes input:checkbox").removeAttr('checked');
    }

    function SelectAllProjectCheckboxes() {
        $("#divProjectCheckboxes input:checkbox").attr('checked', true);
    }
</script>

<div id="divSearchCriteria" runat="server">
    <asp:UpdatePanel ID="upnlSearchCriteria" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <asp:Panel ID="Panel1" DefaultButton="btnSearch"
                runat="server">
                <asp:Panel ID="pnlCommonFilters" CssClass="left sideErrorContainer" runat="server" Width="90%">
                    <div class="left">
                        <div class="left" style="margin: 0px 0px 5px 0px; text-align: right;">
                            <span style="position:relative; top:4px;">Transaction Date From:</span>
                            <telerik:RadDatePicker ID="dtpkFrom" runat="server" ClientEvents-OnDateSelected="onDateSelected">
                            </telerik:RadDatePicker>
                        </div>

                        <div class="left" style="margin: 0px 0px 0px 15px; text-align: right;">
                            <span style="position:relative; top:4px;">To:</span>
                                <telerik:RadDatePicker ID="dtpkTo" runat="server" ClientEvents-OnDateSelected="onDateSelected">
                                </telerik:RadDatePicker>
                        </div>
                    </div>
                    <div id="divCriteriaValidation" runat="server" style="display: none; width: 110px; top:4px !important; position:relative;"
                        class="inputError left">
                    </div>
                    <span style="clear:both"></span>
                    <div class="right" style="position:relative; top:3px;">
                        <asp:CheckBox ID="chkShowUnpaid" Text="Show unpaid invoice transactions only" runat="server" />
                    </div>
                </asp:Panel>

                <asp:Button ID="btnSearch" runat="server" Style="float: right;" CssClass="buttonStyle"
                    OnClientClick="return isDateRangeValid;" OnClick="btnSearch_Click" Text="Search" />

                <div style="clear: both;">
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>

<asp:UpdatePanel ID="upnlSearchResults" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <sb:SBRadGrid ID="gvTransactions" runat="server" OnSortCommand="gvTransactions_SortCommand"
            OnItemDataBound="gvTransactions_ItemDataBound" AllowSorting="true" AllowPaging="true" Style="margin-top: 10px;"
            OnNeedDataSource="gvTransactions_NeedDataSource" AutoGenerateColumns="False">
            <MasterTableView PageSize="50" AllowNaturalSort="false">
                <NoRecordsTemplate>
                    <div class="noData">
                        No records to display.
                    </div>
                </NoRecordsTemplate>
                <PagerStyle AlwaysVisible="true" />
                <Columns>
                    <telerik:GridTemplateColumn HeaderText="Amount" DataType="System.Decimal" HeaderStyle-Width="105"
                        HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" UniqueName="Amount"
                        SortExpression="Amount">
                        <HeaderStyle CssClass="GridColoumnHeaderRight" />
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="From Date" DataType="System.DateTime" HeaderStyle-Width="105" HeaderStyle-HorizontalAlign="Center"
                        ItemStyle-HorizontalAlign="Center" UniqueName="FromDate" SortExpression="FromDate">
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="To Date" DataType="System.DateTime" HeaderStyle-Width="105" HeaderStyle-HorizontalAlign="Center"
                        ItemStyle-HorizontalAlign="Center" UniqueName="ToDate" SortExpression="ToDate">
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Invoice Date" DataType="System.DateTime" HeaderStyle-Width="105" HeaderStyle-HorizontalAlign="Center"
                        ItemStyle-HorizontalAlign="Center" UniqueName="InvoiceDate" SortExpression="InvoiceDate">
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Receipt Date" DataType="System.DateTime" HeaderStyle-Width="105" HeaderStyle-HorizontalAlign="Center"
                        ItemStyle-HorizontalAlign="Center" UniqueName="ReceiptDate" SortExpression="ReceiptDate">
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText=" " HeaderStyle-Width="20" HeaderStyle-HorizontalAlign="Center"
                        ItemStyle-HorizontalAlign="Center" UniqueName="tooltipInvoiceandReceipt">
                        <ItemTemplate>
                            <asp:Image ID="imgInfo" ImageUrl="~/Common/Images/msginfo.png" runat="server" />
                            <asp:Image ID="imgError" Width="16" Height="16" ImageUrl="~/Common/Images/error.png" runat="server" />
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                </Columns>
            </MasterTableView>
        </sb:SBRadGrid>
    </ContentTemplate>
</asp:UpdatePanel>

