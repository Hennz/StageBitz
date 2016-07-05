<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TransactionSearch.ascx.cs"
    Inherits="StageBitz.AdminWeb.Controls.Common.TransactionSearch" %>
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
            divMsg.html("Invalid date range.");
        }
        else {
            isDateRangeValid = true;
            divMsg.html("&nbsp;");
        }
    }
</script>
<sb:GroupBox ID="gbSearchResults" runat="server">
    <TitleLeftContent>
        Transactions
    </TitleLeftContent>
    <BodyContent>
        <div style="margin: 5px 0px 5px 0px;">
            <asp:UpdatePanel ID="upnlDiscountCode" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div style="float: left; margin-right: 5px;">
                        Discount Code:
                    </div>
                    <div style="float: left;">
                        <div style="float: left;" id="divDiscountSet" runat="server" visible="false">
                            <div style="float: left; font-weight: bold;">
                                <asp:Literal ID="litDiscountApplied" runat="server"></asp:Literal>
                            </div>
                            <img src="~/Common/Images/msginfo.png" alt="InformationIcon" runat="server" id="imgDiscountCode"
                                style="display: block; float: left; margin-left: 3px;" />
                            &nbsp;|&nbsp;
                        </div>
                        <asp:LinkButton ID="lnkSetUpDiscountCode" OnClick="lnkSetUpDiscountCode_Click" runat="server" Text="Apply Discount Code"></asp:LinkButton>
                        <asp:LinkButton ID="lnkRemoveDiscountCode" OnClick="lnkRemoveDiscountCode_Click" runat="server" Text="Remove Discount Code"></asp:LinkButton>
                    </div>
                    <div id="divSuccessDiscount" style="margin-left: 5px; display: none; color: Gray; float: left;">
                        Changes saved.
                    </div>

                    <div style="clear: both;">
                    </div>
                    <sb:PopupBox ID="popupManageDiscount" Title="Apply Discount Code" runat="server">
                        <BodyContent>
                            <div style="width: 350px;">
                                Discount Code:
                                <asp:TextBox ID="txtDiscountCode" ValidationGroup="discountCreateValGroup" Width="100"
                                    MaxLength="20" runat="server"></asp:TextBox>
                                <span id="spanErrorMsg" visible="true" style="margin-top: 15px; min-height: 20px;"
                                    class="message error" runat="server" />
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnOk" CssClass="buttonStyle" runat="server" Text="OK" OnClick="btnOk_Click" />
                            <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                        </BottomStripeContent>
                    </sb:PopupBox>

                    <div style="clear: both;">
                    </div>
                    <sb:PopupBox ID="popupRemoveDiscountCode" Title="Remove Discount Code" runat="server">
                        <BodyContent>
                            <div style="width: 350px;">
                                <p>
                                    Are you sure you want to remove this discount code? 
                                </p>
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnConfirmDeleteDiscount" CssClass="buttonStyle" runat="server" Text="OK" OnClick="btnConfirmDeleteDiscount_Click" />
                            <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                    <div style="clear: both;">
                    </div>

                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <asp:UpdatePanel ID="upnlSearchCriteria" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <asp:Panel ID="Panel1" Style="line-height: 30px; min-height: 60px;" DefaultButton="btnSearch"
                    runat="server">
                    <div class="left">
                        Transaction Date From:
                        <telerik:RadDatePicker ID="dtpkFrom" runat="server" Style="margin-top: -4px;" ClientEvents-OnDateSelected="onDateSelected">
                        </telerik:RadDatePicker>
                        &nbsp;To:
                        <telerik:RadDatePicker ID="dtpkTo" runat="server" Style="margin-top: -4px;" ClientEvents-OnDateSelected="onDateSelected">
                        </telerik:RadDatePicker>
                    </div>
                    <div class="left" style="margin-left: 20px;">
                        <asp:Literal runat="server" ID="litCompany" Text="Company: "></asp:Literal>
                        <asp:TextBox ID="txtCompany" MaxLength="100" Width="160" runat="server"></asp:TextBox>
                    </div>
                    <%--<br style="clear: both;" />--%>
                    <div class="left" style="margin-left: 20px;">
                        <asp:CheckBox ID="chkShowUnpaid" Text="Show unpaid invoice transactions only" runat="server" />
                    </div>
                    <asp:Button ID="btnSearch" runat="server" Style="float: right;" CssClass="buttonStyle"
                        OnClientClick="return isDateRangeValid;" OnClick="btnSearch_Click" Text="Search" />
                    <%--<br style="clear: both;" />--%>
                    <div id="divCriteriaValidation" runat="server" style="margin-left: 360px;" class="inputError left">
                        &nbsp;
                    </div>
                    <div style="clear: both;">
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="upnlSearchResults" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <div id="divNoSearchCriteria" style="text-align: center; padding: 5px;" runat="server">
                    Enter a search criteria to locate transactions.
                </div>
                <sb:SBRadGrid ID="gvTransactions" Width="100%" runat="server" OnSortCommand="gvTransactions_SortCommand"
                    OnItemDataBound="gvTransactions_ItemDataBound" AllowSorting="true" AllowPaging="true"
                    OnNeedDataSource="gvTransactions_NeedDataSource" AutoGenerateColumns="False">
                    <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                    <MasterTableView PageSize="50" AllowNaturalSort="false">
                        <NoRecordsTemplate>
                            <div class="noData">
                                No records to display.
                            </div>
                        </NoRecordsTemplate>
                        <PagerStyle AlwaysVisible="true" />
                        <SortExpressions>
                            <telerik:GridSortExpression FieldName="CompanyName" SortOrder="Ascending" />
                        </SortExpressions>
                        <Columns>
                            <telerik:GridTemplateColumn HeaderText="Company" UniqueName="CompanyName" HeaderStyle-Width="145"
                                SortExpression="CompanyName">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkCompanyName" runat="server"></asp:HyperLink>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Country" HeaderStyle-Width="100" UniqueName="Country"
                                SortExpression="CountryName">
                            </telerik:GridTemplateColumn>                            
                            <telerik:GridTemplateColumn HeaderText="Amount" DataType="System.Decimal" HeaderStyle-Width="75"
                                HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" UniqueName="Amount"
                                SortExpression="Invoice.Amount">
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="From Date" DataType="System.DateTime" HeaderStyle-Width="105"
                                HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" UniqueName="FromDate"
                                SortExpression="Invoice.FromDate">
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="To Date" DataType="System.DateTime" HeaderStyle-Width="105"
                                HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" UniqueName="ToDate"
                                SortExpression="Invoice.ToDate">
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Inv. Date" DataType="System.DateTime" HeaderStyle-Width="105"
                                HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" UniqueName="InvoiceDate"
                                SortExpression="Invoice.InvoiceDate">
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Rcpt. Date" DataType="System.DateTime" HeaderStyle-Width="105"
                                HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" UniqueName="ReceiptDate"
                                SortExpression="ReceiptDate">
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText=" " HeaderStyle-Width="20" HeaderStyle-HorizontalAlign="Center"
                                ItemStyle-HorizontalAlign="Center" UniqueName="tooltipInvoiceandReceipt">
                                <ItemTemplate>
                                    <asp:Image ImageUrl="~/Common/Images/msginfo.png" alt="InformationIcon" runat="server" ID="imgInfo" />
                                    <asp:Image ID="imgError" Width="16" Height="16" ImageUrl="~/Common/Images/error.png"
                                        runat="server" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                        </Columns>
                    </MasterTableView>
                </sb:SBRadGrid>
            </ContentTemplate>
        </asp:UpdatePanel>
    </BodyContent>
</sb:GroupBox>
