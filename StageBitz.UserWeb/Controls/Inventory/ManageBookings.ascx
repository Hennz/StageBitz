<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManageBookings.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Inventory.ManageBookings" %>
<%@ Register Src="~/Controls/Common/ExportData.ascx" TagName="ExportData" TagPrefix="sb" %>
<script type="text/javascript">
    // Search in booking tab
    var cboSearchEvents = new StageBitz.UserWeb.Common.Scripts.ComboSearchEvents();
    cboSearchEvents.FindButton = "#<%= btnFindBooking.ClientID %>";

    function cboSearch_OnItemsRequested(sender, eventArgs) {
        cboSearchEvents.OnItemsRequested(sender, eventArgs);
    };

    function cboSearch_OnKeyPressing(sender, eventArgs) {
        cboSearchEvents.OnKeyPressing(sender, eventArgs);
    };

    function cboSearch_OnClientFocus(sender, eventArgs) {
        cboSearchEvents.OnClientFocus(sender, eventArgs);
    };

    function cboSearch_OnSelectedIndexChanged(sender, eventArgs) {
        cboSearchEvents.OnSelectedIndexChanged(sender, eventArgs);
    };
</script>
<sb:GroupBox ID="grpBookings" runat="server">
    <TitleLeftContent>
        <asp:UpdatePanel runat="server" ID="upnlBookingsHeader" UpdateMode="Conditional">
            <ContentTemplate>
                <div style="float: left;">
                    <span class="boldText">
                        <asp:Literal ID="ltrlBookingCount" runat="server"></asp:Literal></span>
                </div>
                <div style="float: left; margin-left: 4px;">
                    <sb:HelpTip ID="helpTipInventory" Visible="true" runat="server">
                        <div style="width: 300px;">
                            <b>This is where you can manage Bookings.</b>
                            <ul>
                                <li>Click on the Booking name to view the full list of Items for that Booking.</li>
                                <li>If the '!' icon appears in the From and To columns it means there is an Item with a different booking period within the Booking.</li>
                                <li>Once a Booking is closed you can archive it to hide it from the main list.</li>
                            </ul>
                        </div>
                    </sb:HelpTip>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </TitleLeftContent>
    <TitleRightContent>
    </TitleRightContent>
    <BodyContent>
        <asp:Panel ID="pnlBookings" DefaultButton="btnFindBooking" runat="server">
            <div style="float: left; width: 70%;">
                <asp:UpdatePanel ID="upnlBookingSearch" runat="server">
                    <ContentTemplate>
                        <table>
                            <tr>
                                <td>
                                    <div class="searchbox rounded">
                                        <span style="position: relative; top: 2px;">
                                            <telerik:RadComboBox runat="server" ID="cboSearch" AutoPostBack="false" OnItemsRequested="cboSearch_ItemsRequested"
                                                OnClientItemsRequested="cboSearch_OnItemsRequested" OnClientKeyPressing="cboSearch_OnKeyPressing"
                                                EnableLoadOnDemand="true" EmptyMessage="Search for Booking..." ChangeTextOnKeyBoardNavigation="true"
                                                ShowWhileLoading="false" ShowToggleImage="false" MaxLength="100" Width="182"
                                                OnClientFocus="cboSearch_OnClientFocus" OnClientSelectedIndexChanged="cboSearch_OnSelectedIndexChanged">
                                                <ExpandAnimation Type="None" />
                                                <CollapseAnimation Type="None" />
                                            </telerik:RadComboBox>
                                            <span style="position: relative; bottom: 2px;">
                                                <asp:ImageButton ID="ibtnClearSearch" runat="server" CausesValidation="false" Text=""
                                                    ImageUrl="~/Common/Images/button_cancel.png" Width="16" Height="16" CssClass="searchButton"
                                                    OnClick="ibtnClearSearch_Click" /></span>
                                        </span>
                                    </div>
                                </td>
                                <td>
                                    <asp:Button ID="btnFindBooking" runat="server" CssClass="buttonStyle" OnClick="btnFindBooking_Click"
                                        CausesValidation="false" Text="Find" />
                                </td>
                                <td>Display Status:&nbsp;</td>
                                <td>
                                    <sb:DropDownListOPTGroup ID="ddlBookingStatus" runat="server" AppendDataBoundItems="true"
                                        AutoPostBack="true" OnSelectedIndexChanged="ddlSPPCompanies_OnSelectedIndexChanged" Width="150">
                                        <asp:ListItem Text="Show All" Value="" Selected="True"></asp:ListItem>
                                    </sb:DropDownListOPTGroup>
                                </td>
                            </tr>
                        </table>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div style="float: left; width: 29%;">
                <sb:ExportData ID="exportData" OnExcelExportClick="exportData_ExcelExportClick" OnPDFExportClick="exportData_PDFExportClick" runat="server" />
            </div>
        </asp:Panel>
        <asp:UpdatePanel ID="upnlBookingGrid" runat="server">
            <ContentTemplate>
                <telerik:RadGrid ID="gvBookings" EnableLinqExpressions="False" AutoGenerateColumns="false"
                    AllowSorting="true" AllowAutomaticUpdates="True" OnNeedDataSource="gvBookings_NeedDataSource" runat="server"
                    AllowPaging="True" PageSize="20" PagerStyle-AlwaysVisible="true" ViewStateMode="Disabled">
                    <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                    <MasterTableView AllowNaturalSort="false" TableLayout="Fixed" ViewStateMode="Disabled">
                        <NoRecordsTemplate>
                            <div class="noData">
                                <b>No Data.</b>
                            </div>
                        </NoRecordsTemplate>
                        <SortExpressions>
                            <telerik:GridSortExpression FieldName="StatusSortOrder" SortOrder="Ascending" />
                        </SortExpressions>
                        <Columns>
                            <telerik:GridTemplateColumn HeaderText="Booking ID" DataType="System.Int32" SortExpression="BookingNumber"
                                ItemStyle-Width="7%" HeaderStyle-Width="7%" ItemStyle-HorizontalAlign="Right" UniqueName="BookingNumber">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblBookingNumber"
                                        Text='<%# (int)Eval("BookingNumber") > 0 ? Eval("BookingNumber") : string.Empty %>'></asp:Label>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridDateTimeColumn ReadOnly="true" HeaderText="Last Updated" DataField="LastUpdatedDate" DataFormatString="{0:dd MMM yyyy}"
                                SortExpression="LastUpdatedDate" ItemStyle-Width="12%" HeaderStyle-Width="12%">
                            </telerik:GridDateTimeColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Booking Name" SortExpression="BookingName" UniqueName="BookingName"
                                ItemStyle-Width="15.5%" HeaderStyle-Width="15.5%">
                                <ItemTemplate>
                                    <asp:HyperLink runat="server" ID="lnkBookingName"
                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("BookingName"), 15) %>' Target="_blank"
                                        ToolTip='<%# ((string)Eval("BookingName")).Length > 15 ? Eval("BookingName") : string.Empty %>'
                                        NavigateUrl='<%# GetBookingUrl((int)Eval("BookingId") , CompanyId.Value, (string)Eval("RelatedTable")) %>'></asp:HyperLink>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" DataField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName"
                                ItemStyle-Width="15.5%" HeaderStyle-Width="15.5%">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblCompanyName"
                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("CompanyName"), 15) %>'
                                        ToolTip='<%# ((string)Eval("CompanyName")).Length > 15 ? Eval("CompanyName") : string.Empty %>'></asp:Label>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn DataField="BookingCount" HeaderText="Count" SortExpression="BookingCount" DataType="System.Int32"
                                ItemStyle-Width="7%" HeaderStyle-Width="7%" ItemStyle-HorizontalAlign="Right">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="From Date" SortExpression="FromDate" DataType="System.DateTime"
                                ItemStyle-Width="13%" HeaderStyle-Width="13%">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblFromDate"
                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.FormatDate(Eval("FromDate")) %>'></asp:Label>
                                    <img runat="server" id="imgWarningFromDate" src="~/Common/Images/exclamation.png"
                                        visible='<%# ((bool)Eval("IsDifferentFromDate")) %>' title="Some Items within this booking have different booking dates." />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="To Date" SortExpression="ToDate" DataType="System.DateTime"
                                ItemStyle-Width="13%" HeaderStyle-Width="13%">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblToDate"
                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.FormatDate(Eval("ToDate")) %>'></asp:Label>
                                    <img runat="server" id="imgWarningToDate" src="~/Common/Images/exclamation.png"
                                        visible='<%# ((bool)Eval("IsDifferentToDate")) %>' title="Some Items within this booking have different booking dates." />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn DataField="Status" HeaderText="Status" SortExpression="StatusSortOrder"
                                ItemStyle-Width="10%" HeaderStyle-Width="10%">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="Archive" ItemStyle-Width="7%" HeaderStyle-Width="7%">
                                <ItemTemplate>
                                    <asp:CheckBox runat="server" ID="chkArchived" AutoPostBack="true" OnCheckedChanged="chkArchived_CheckedChanged"
                                        Enabled='<%# CanArchiveBooking() %>' Checked='<%#(bool)Eval("IsArchived")%>' />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                        </Columns>
                        <NoRecordsTemplate>
                            <div style="text-align: center;" class="noData">
                                <asp:Label runat="server" ID="lblNoData"></asp:Label>
                            </div>
                        </NoRecordsTemplate>
                    </MasterTableView>
                </telerik:RadGrid>
            </ContentTemplate>
        </asp:UpdatePanel>
    </BodyContent>
</sb:GroupBox>
