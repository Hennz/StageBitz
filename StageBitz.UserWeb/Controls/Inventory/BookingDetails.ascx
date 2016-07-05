<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookingDetails.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Inventory.BookingDetails" %>
<%@ Register Src="~/Controls/Common/ExportData.ascx" TagName="ExportData" TagPrefix="sb" %>

<script type="text/javascript">
    var selectedItemBookingId;
    var selectedImg;
    var selectedReleaseLink;
    var userId;
    var parentRow;
    var isNonProjectBooking;
    function InitializeSettings() {

        //var hdnItemBookingId = self.next('input[id$="hdnItemBookingId"]').val();

        //alert($(this).parents('td').find('input[id$="hdnItemBookingId"]').val());
        userId = $('#<%= hdnUserId.ClientID %>').val();
        $("input[id$='chkIsReturned']").click(function () {
            var self = $(this);
            parentRow = self.closest('tr');
            var itemBookingId = parentRow.find('input[id$="hdnItemBookingId"]').val();
            var isActive = parentRow.find('input[id$="hdnIsActive"]').val();


            ReturnItem(itemBookingId, self.is(':checked'), userId, function (result) {
                if (result.Status == "OK") {

                    if (self.is(':checked')) {
                        if (isNonProjectBooking == true) {
                            parentRow.find('input[id$="chkIsReturned"]').attr("disabled", true);
                            parentRow.find('input[id$="chkIsPickedUp"]').attr("disabled", true);
                            UpdateStatus('<%= returnedStatus %>');
                        }
                        else {
                            //Need to decide whether to display the Link button or the Image
                            if (isActive == 'True') {
                                parentRow.find('a[id$="lnkRelease"]').show();

                            }
                            else
                                parentRow.find('img[id$="imgReleased"]').show();
                        }

                    }
                    else {
                        parentRow.find('a[id$="lnkRelease"]').hide();
                    }
                }
                else {
                    if (result.ErrorCode) {
                        showErrorPopup(result.ErrorCode);
                    }
                }
            });
        });

        $("input[id$='chkIsPickedUp']").click(function () {
            var self = $(this);
            parentRow = self.closest("tr");
            var itemBookingId = parentRow.find('input[id$="hdnItemBookingId"]').val();

            if (self.is(':checked')) {
                var chkBox = parentRow.find('input[id$="chkIsReturned"]');
                chkBox.parent('span').show();
                chkBox.show();
                chkBox.attr("checked", false);
            }
            else {
                parentRow.find('input[id$="chkIsReturned"]').hide();
                parentRow.find('a[id$="lnkRelease"]').hide();
            }

            PickUpItem(itemBookingId, self.is(':checked'), userId, function (result) {
                if (result.Status == "NOTOK") {
                    if (result.ErrorCode && showErrorPopup(result.ErrorCode))
                    {
                        return;
                    }

                    if (isNonProjectBooking != true) {
                        if (self.is(':checked')) {
                            parentRow.find('input[id$="chkIsReturned"]').hide();
                            parentRow.find('a[id$="lnkRelease"]').hide();
                        }
                        else {
                            var chkBox = parentRow.find('input[id$="chkIsReturned"]');
                            chkBox.parent('span').show();
                            chkBox.show();
                            chkBox.attr("checked", false);
                        }
                    }
                    else {
                        $('#divApproveBookingError').text(result.Message);
                        showPopup('popUpError');
                    }
                }
            });
        });

        $("a[id$='lnkRelease']").click(function () {
            var self = $(this);
            parentRow = self.closest('tr');
            selectedItemBookingId = self.parents('tr').find('input[id$="hdnItemBookingId"]').val();
            selectedImg = self.parents('tr').find('img[id$="imgReleased"]');
            selectedReleaseLink = self.parents('tr').find('a[id$="lnkRelease"]');
            showPopup('<%=popupConfirmRelease.ClientID%>');
            return false;
        });



        $("a[id$='lnkApprove']").click(function () {
            //check if not "Delayed". If so show a message
            var self = $(this);
            parentRow = self.closest('tr');
            selectedItemBookingId = self.parents('tr').find('input[id$="hdnItemBookingId"]').val();

            ApproveSelectedBooking();
            return false;
        });

        function ApproveSelectedBooking() {

            ApproveBooking(selectedItemBookingId, userId, function (result) {
                if (result.Status == "OK") {
                    //Disable the Action links
                    self.UpdateStatus('<%= approvedStatus %>');
                    self.DisableApprovalActions(true);
                    //Show PickUp checkBox
                    var chkBox = parentRow.find('input[id$="chkIsPickedUp"]');
                    chkBox.parent('span').show();
                    chkBox.show();
                }
                else {
                    if (result.ErrorCode) {
                        if (!showErrorPopup(result.ErrorCode)) {
                            $('#divApproveBookingError').text(result.Message);
                            showPopup('popUpError');
                        }
                    }
                }

                hidePopup('<%=popupConfirmRelease.ClientID%>');
            });

            return false;
        }


        $("a[id$='lnkDeny']").click(function () {
            var self = $(this);
            parentRow = self.closest('tr');
            selectedItemBookingId = self.parents('tr').find('input[id$="hdnItemBookingId"]').val();
            //Call the 
            RejectSelectedBooking();

            var lnkDeny = self.parents('tr').find('a[id$="lnkDeny"]');
            return false;
        });

    };


    function ReleaseSelecedItem() {

        ReleaseItem(selectedItemBookingId, userId, function (result) {
            if (result.Status == "OK") {
                selectedImg.show();
                selectedReleaseLink.hide();
                parentRow.find('input[id$="hdnIsActive"]').val("False");

                var tdStatusArea = parentRow.find('td.StatusArea');

                if (tdStatusArea.length == 1) {
                    var rejectedStatusCode = '<%= rejectedStatus %>';
                    tdStatusArea.val(rejectedStatusCode);
                    tdStatusArea.html(rejectedStatusCode);
                }
            }

            hidePopup('<%=popupConfirmRelease.ClientID%>');
        });

        return false;
    }

    function DisableApprovalActions(isApproval) {
        var selectedApproveLink = parentRow.find('a[id$="lnkApprove"]');
        if (selectedApproveLink.length > 0)
            selectedApproveLink.hide();

        var selectedRejectedLink = parentRow.find('a[id$="lnkDeny"]');
        if (selectedRejectedLink.length > 0)
            selectedRejectedLink.hide();

        var lvlApprovedDisabled = parentRow.find('span[id$="lblApprovedDisabled"]');
        var lblDeny = parentRow.find('span[id$="lblDeny"]');

        lvlApprovedDisabled.show();
        lblDeny.show();

        if (isApproval) {
            lvlApprovedDisabled.attr("title", "The Booking has already been approved");
            lblDeny.attr("title", "The Booking has already been approved");
        }
        else {
            lvlApprovedDisabled.attr("title", "The Booking has already been denied");
            lblDeny.attr("title", "The Booking has already been denied");
        }
    }

    function UpdateStatus(statusCode) {
        var tdStatusArea = parentRow.find('td.StatusArea');
        if (tdStatusArea.length == 1) {
            tdStatusArea.text(statusCode);
        }
    }

    function RejectSelectedBooking() {
        var self = this;
        RejectBooking(selectedItemBookingId, userId, function (result) {
            if (result.Status == "OK") {
                parentRow.find('input[id$="hdnIsActive"]').val("False");
                self.UpdateStatus('<%= notApprovedStatus %>');
                self.DisableApprovalActions(false);
            }
            else {
                $('#divApproveBookingError').text(result.Message);
                showPopup('popUpError');
            }
        });
        return false;
    }

    function OnClientResponseError(sender, args) {
        args.set_cancelErrorAlert(true);
    }

</script>



<sb:PopupBox Title="Are you sure?" ID="popupConfirmRelease" runat="server">
    <BodyContent>
        This will force the release of the Item from the Project it is booked to. This action cannot be undone.
    </BodyContent>
    <BottomStripeContent>
        <asp:Button ID="btnRelease" runat="server" OnClientClick="return ReleaseSelecedItem();" CssClass="popupBoxCloser buttonStyle" Text="Confirm" />
        <asp:Button ID="Button1" runat="server" CssClass="popupBoxCloser buttonStyle" Text="Cancel" />
    </BottomStripeContent>
</sb:PopupBox>

<sb:PopupBox ID="popUpError" runat="server" Title="Error" ShowCornerCloseButton="false">
    <BodyContent>
        <div style="width: 350px;">
            <div id="divApproveBookingError"></div>
        </div>
    </BodyContent>
    <BottomStripeContent>
        <asp:Button ID="btnReloadBookingDetails" CssClass="buttonStyle" runat="server" Text="Ok" OnClick="btnReloadBookingDetails_Click" />
    </BottomStripeContent>
</sb:PopupBox>

<sb:ErrorPopupBox ID="popupNoEditPermission" Title="Change to your Permissions" runat="server" ShowCornerCloseButton="false" ErrorCode="NoEditPermissionForInventory">
    <BodyContent>
        <div>
            <div style="width: 300px;">
                <p>
                    Your permission level in the Inventory has just been changed.  An email has been sent to you with the details. 
                </p>
            </div>
        </div>
    </BodyContent>
    <BottomStripeContent>
        <asp:HyperLink CssClass="buttonStyle" NavigateUrl="~/Default.aspx" runat="server" Text="Ok" ID="lnkOk"></asp:HyperLink>
    </BottomStripeContent>
</sb:ErrorPopupBox>

<asp:HiddenField ID="hdnUserId" runat="server" />
<asp:UpdatePanel ID="upnelCompanies" runat="server">
    <ContentTemplate>

        <div style="float: right;">
            <asp:CheckBox ID="chkMyBookingsOnly" AutoPostBack="true" OnCheckedChanged="chkMyBookingsOnly_CheckedChanged" runat="server" Text="Show only my bookings" />
        </div>
        <div style="clear: both;"></div>
        <div style="width: 430px; padding-top: 5px;" class="left">
            <b>Booking Name:
                    
                      <asp:Label ID="lblBookingName" runat="server"></asp:Label></b>
        </div>
        <div id="divCompanyInventory" runat="server">
            <div style="padding-top: 5px;" class="left">
                <b>Company Inventory: </b>&nbsp;
            </div>
            <div class="left">

                <asp:DropDownList ID="ddCompanyInventory" Width="150" AutoPostBack="true" OnSelectedIndexChanged="ddCompanyInventory_SelectedIndexChanged" AppendDataBoundItems="true" runat="server">
                </asp:DropDownList>

                <div style="margin-top: 5px;">
                    <asp:Label ID="lblCompany" runat="server"></asp:Label>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<div style="margin-right: 2px;">
    <sb:ExportData ID="exportData" OnExcelExportClick="exportData_ExcelExportClick" OnPDFExportClick="exportData_PDFExportClick" runat="server" />
</div>
<div style="clear: both; margin-bottom: 5px;">
</div>
<asp:UpdatePanel ID="upnel" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <sb:GroupBox ID="grpBookings" runat="server">
            <TitleLeftContent>
                <div style="float: left;">
                    <span class="boldText">
                        <asp:Literal ID="ltrlBookingCount" runat="server"></asp:Literal></span>
                </div>
                <div style="float: left; margin-left: 4px;">
                    <sb:HelpTip ID="helpTipBooking" Visible="true" runat="server" Width="470">
                        <b>All the Items in a Booking are grouped by Item Type.</b>
                        <ul>
                            <li>You can see if an Item has been confirmed or is still pending.
                            </li>
                            <li>If an Item has been returned and not released from the Project you can release it by clicking the link.
                            </li>
                        </ul>
                    </sb:HelpTip>
                </div>

            </TitleLeftContent>
            <TitleRightContent>
                <div style="padding-top: 3px;" class="left">
                    <span class="boldText">Show Item Type: &nbsp;</span>
                </div>
                <div class="left">
                    <asp:DropDownList ID="ddItemTypes" Width="100" AutoPostBack="true" OnSelectedIndexChanged="ddItemTypes_SelectedIndexChanged" AppendDataBoundItems="true" runat="server">
                        <asp:ListItem Text="Show All" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </TitleRightContent>
            <BodyContent>

                <telerik:RadToolTipManager ID="tooltipManager" Width="400px" Height="120px" ShowDelay="2000" OnClientResponseError="OnClientResponseError"
                    Position="MiddleRight" OnAjaxUpdate="tooltipManager_AjaxUpdate" runat="server">
                </telerik:RadToolTipManager>
                <table>
                    <%--   <tr>
                        <td style="width: 120px;">Company Inventory </td>
                        <td>:
                        <asp:Label ID="lblCompany" runat="server"></asp:Label>
                        </td>
                    </tr>--%>
                    <tr id="trContactedPerson" runat="server">
                        <td>
                            <asp:Literal ID="litContactedPersonType" runat="server"></asp:Literal>
                        </td>
                        <td>:
                          <asp:HyperLink runat="server" ID="lnkContactPersonEmail"></asp:HyperLink>
                        </td>
                    </tr>
                    <tr>
                        <td>Booking ID</td>
                        <td>:
                        <asp:Literal ID="litBookingNumber" runat="server"></asp:Literal>
                        </td>
                    </tr>
                    <tr id="trLastUpdated" runat="server">
                        <td>Last Updated </td>
                        <td>:
                        <asp:Literal ID="litLastUpdatedDate" runat="server"></asp:Literal>
                        </td>
                    </tr>
                </table>
                <telerik:RadGrid ID="gvBookingDetails" runat="server" AutoGenerateColumns="false" EnableLinqExpressions="false" Width="100%" AllowAutomaticInserts="true" OnNeedDataSource="gvBookingDetails_NeedDataSource"
                    AllowSorting="true" AllowAutomaticUpdates="True" OnSortCommand="gvBookingDetails_SortCommand" AllowPaging="True" OnPreRender="gvBookingDetails_PreRender"
                    OnItemDataBound="gvBookingDetails_ItemDataBound" AllowAutomaticDeletes="true" PageSize="20" PagerStyle-AlwaysVisible="true" CssClass="font11">
                    <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                    <MasterTableView EditMode="InPlace" AllowMultiColumnSorting="true" AllowNaturalSort="false" Font-Size="11">
                        <NoRecordsTemplate>
                            <div class="noData">
                                This Item Type does not have any Bookings.
                            </div>
                        </NoRecordsTemplate>
                        <GroupHeaderTemplate>
                            <%--               <td colspan="2">
                                <asp:Literal ID="litItemTypeName" runat="server" Text='<%# Eval("ItemTypeName") %>'></asp:Literal>
                            </td>
                            <td style='<%# "padding-left:"+ ((DisplayMode == ViewMode.Admin) ? "200px;": "300px;") + "display:" + (CanEditBookingDates ? "": "none") %>' colspan='<%# ((DisplayMode == ViewMode.Admin) ? "5" : "6") %>'>Edit Booking dates:
                            <a id="lnkFrom" href='<%# (DisplayMode == ViewMode.Admin) ? ResolveUrl(string.Format("~/Inventory/BookingDetailsEdit.aspx?CompanyId={0}&BookingId={1}&ItemTypeId={2}&IsToDateChange={3}", CompanyId,BookingId,(int)Eval("ItemTypeId"),false)) : ResolveUrl(string.Format("~/Project/ProjectEditBookingDetails.aspx?ProjectId={0}&ItemTypeId={1}&IsToDateChange={2}",RelatedId,(int)Eval("ItemTypeId"),false)) %>' target="_blank" class="lnkItemDetails" runat="server">From | </a>
                                <a id="lnkToDate" href='<%# (DisplayMode == ViewMode.Admin) ? ResolveUrl(string.Format("~/Inventory/BookingDetailsEdit.aspx?CompanyId={0}&BookingId={1}&ItemTypeId={2}&IsToDateChange={3}", CompanyId,BookingId,(int)Eval("ItemTypeId"),true)) : ResolveUrl(string.Format("~/Project/ProjectEditBookingDetails.aspx?ProjectId={0}&ItemTypeId={1}&IsToDateChange={2}",RelatedId,(int)Eval("ItemTypeId"),true)) %>' target="_blank" class="lnkItemDetails" runat="server">To </a>
                            </td>--%>
                            <asp:Label ID="litItemTypeName" Style="margin-right: 100px;" Width="200" runat="server" Text='<%# Eval("ItemTypeName") %>'></asp:Label>
                            <span style='<%# "padding-left:"+ ((DisplayMode == ViewMode.Admin) ? "200px;": "300px;") + "display:" + (CanEditBookingDates ? "": "none") %>'>Edit Booking dates:
                                <a id="lnkFrom" href='<%# (DisplayMode == ViewMode.Admin) ? ResolveUrl(string.Format("~/Inventory/BookingDetailsEdit.aspx?CompanyId={0}&BookingId={1}&ItemTypeId={2}&IsToDateChange={3}", CompanyId,BookingId,(int)Eval("ItemTypeId"),false)) : (DisplayMode == ViewMode.NonProject)  ? ResolveUrl(string.Format("~/Inventory/BookingDetailsEditNonProject.aspx?CompanyId={0}&BookingId={1}&ItemTypeId={2}&IsToDateChange={3}&ViewCId={4}", CompanyId,BookingId,(int)Eval("ItemTypeId"),false,ViewingCompanyId)) : ResolveUrl(string.Format("~/Project/ProjectEditBookingDetails.aspx?ProjectId={0}&ItemTypeId={1}&IsToDateChange={2}",RelatedId,(int)Eval("ItemTypeId"),false)) %>' target="_blank" class="lnkItemDetails" runat="server">From | </a>
                                <a id="lnkToDate" href='<%# (DisplayMode == ViewMode.Admin) ? ResolveUrl(string.Format("~/Inventory/BookingDetailsEdit.aspx?CompanyId={0}&BookingId={1}&ItemTypeId={2}&IsToDateChange={3}", CompanyId,BookingId,(int)Eval("ItemTypeId"),true)) : (DisplayMode == ViewMode.NonProject) ? ResolveUrl(string.Format("~/Inventory/BookingDetailsEditNonProject.aspx?CompanyId={0}&BookingId={1}&ItemTypeId={2}&IsToDateChange={3}&ViewCId={4}", CompanyId,BookingId,(int)Eval("ItemTypeId"),true,ViewingCompanyId)) : ResolveUrl(string.Format("~/Project/ProjectEditBookingDetails.aspx?ProjectId={0}&ItemTypeId={1}&IsToDateChange={2}",RelatedId,(int)Eval("ItemTypeId"),true)) %>' target="_blank" class="lnkItemDetails" runat="server">To </a>
                            </span>
                        </GroupHeaderTemplate>
                        <GroupByExpressions>
                            <telerik:GridGroupByExpression>
                                <SelectFields>
                                    <telerik:GridGroupByField HeaderText=" " FormatString="{0}"
                                        HeaderValueSeparator=" " FieldName="ItemTypeName"></telerik:GridGroupByField>
                                </SelectFields>
                                <GroupByFields>
                                    <telerik:GridGroupByField FieldName="ItemTypeId" HeaderText="ItemTypeName" SortOrder="Ascending"></telerik:GridGroupByField>
                                </GroupByFields>
                            </telerik:GridGroupByExpression>
                        </GroupByExpressions>
                        <SortExpressions>
                            <telerik:GridSortExpression FieldName="StatusSortOrder" SortOrder="Ascending" />
                        </SortExpressions>
                        <Columns>

                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Item Brief"
                                HeaderStyle-Width="100px" SortExpression="ItemBriefName" UniqueName="ItemBriefName">
                                <ItemTemplate>
                                    <a id="lnkItemBriefDetails" runat="server" title='<%# Eval("ItemBriefName").ToString().Length > 12 ? Eval("ItemBriefName") : string.Empty %>' href='<%# ResolveUrl(string.Format("~/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}", (int)Eval("ItemBriefId"))) %>' target="_blank" class="lnkItemDetails"><%#StageBitz.UserWeb.Common.Helpers.Support.TruncateString((string)Eval("ItemBriefName"),12) %></a>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Item Name"
                                HeaderStyle-Width="100px" SortExpression="ItemName" UniqueName="ItemName">
                                <ItemTemplate>
                                    <asp:HiddenField runat="server" ID="hdnItemBookingId" Value='<%# Bind("ItemBookingId") %>' />
                                    <asp:HiddenField runat="server" ID="hdnIsActive" Value='<%# Bind("IsActive") %>' />
                                    <a id="lnkItemDetails" runat="server" 
                                        href='<%# ResolveUrl(string.Format("~/Inventory/ItemDetails.aspx?ItemId={0}&CompanyId={1}", (int)Eval("ItemId"), (int)Eval("CompanyId"))) %>' 
                                        target="_blank" class="lnkItemDetails" visible='<%# (bool)Eval("CanAccessItemDetails") %>'>
                                        <%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString((string)Eval("ItemName"),12) %></a>
                                    <asp:Label ID="lblItemDetails" runat="server" Visible='<%# !(bool)Eval("CanAccessItemDetails") %>'
                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString((string)Eval("ItemName"),12) %>'
                                        ToolTip='<%# ((string)Eval("ItemName")).Length > 12 ? Eval("ItemName") : string.Empty %>'></asp:Label>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn UniqueName="Location" HeaderText="Location" HeaderStyle-Width="80">
                                <ItemTemplate>
                                    <asp:Label ID="lblLocation"
                                        Text='<%# StageBitz.Common.Utils.ReverseEllipsize((string)Eval("Location"), 10) %>'
                                        ToolTip='<%# ((string)Eval("Location")).Length > 10 ? Eval("Location") : string.Empty %>' runat="server"></asp:Label>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn UniqueName="Quantity" DataType="System.Int32" SortExpression="Quantity" ItemStyle-HorizontalAlign="Center"
                                HeaderText="Qty" HeaderStyle-Width="30">
                                <ItemTemplate>
                                    <asp:Literal ID="litQuantity" Text='<%# Eval("Quantity") %>' runat="server"></asp:Literal>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Booked By"
                                HeaderStyle-Width="100px" SortExpression="BookedBy" UniqueName="BookedBy">
                                <ItemTemplate>
                                    <asp:HyperLink runat="server" ID="lnkBookedBy"
                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("BookedBy"), 12) %>'
                                        ToolTip='<%# ((string)Eval("BookedBy")).Length > 12 ? Eval("BookedBy") : string.Empty %>'
                                        NavigateUrl='<%# "mailto:" + Eval("BookedByEmail") %>'></asp:HyperLink>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="From"
                                HeaderStyle-Width="80px" SortExpression="FromDate" UniqueName="FromDate">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblFrom"
                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.FormatDate(Eval("FromDate")) %>'></asp:Label>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="To"
                                HeaderStyle-Width="80px" SortExpression="ToDate" UniqueName="ToDate">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblTo"
                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.FormatDate(Eval("ToDate")) %>'></asp:Label>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Status" ItemStyle-CssClass="StatusArea"
                                SortExpression="StatusSortOrder" UniqueName="CurrentStatus" HeaderStyle-Width="40px">
                                <ItemTemplate>
                                    <asp:Literal Text='<%# Eval("CurrentStatus") %>' runat="server" ID="litStatus"></asp:Literal>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" HeaderStyle-Width="140" HeaderText="Approval" ItemStyle-HorizontalAlign="Center"
                                UniqueName="Approval">
                                <ItemTemplate>
                                    <asp:LinkButton runat="server" Visible='<%# (bool)Eval("CanApprove") && StageBitz.Common.Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, (int?)Eval("LocationId")) %>' Text="Approve" ID="lnkApprove" />

                                    <asp:Label ID="lblApprovedDisabled" CssClass="ActionLink" Style='<%# "display:" + (((bool)Eval("CanApprove") && StageBitz.Common.Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, (int?)Eval("LocationId"))) ? "none": "") %>' ToolTip='<%# (string)Eval("ApprovalFailureReason") %>' runat="server"><u>Approve</u></asp:Label>
                                    &nbsp;&nbsp;
                                        <asp:LinkButton runat="server" Visible='<%# (bool)Eval("CanReject") && StageBitz.Common.Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, (int?)Eval("LocationId")) %>' Text="Deny" ID="lnkDeny" />

                                    <asp:Label ID="lblDeny" CssClass="ActionLink" Style='<%# "display:" + (((bool)Eval("CanReject")  && StageBitz.Common.Utils.HasLocationManagerPermission(this.CompanyId, this.UserID, (int?)Eval("LocationId"))) ? "none": "") %>' runat="server" ToolTip='<%# (string)Eval("RejectionFailureReason") %>'> <u>Deny</u></asp:Label>

                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Picked up" ItemStyle-HorizontalAlign="Center"
                                HeaderStyle-Width="80" SortExpression="IsPickedUpOrder" UniqueName="IsPickedUp">
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkIsPickedUp" 
                                        Enabled='<%# !(RelatedTableName == "NonProject" && (int)Eval("InventoryStatusCodeId") == StageBitz.Common.Utils.GetCodeIdByCodeValue("InventoryStatusCode","RETURNED")) %>' 
                                        Visible='<%# DisplayMode == ViewMode.Admin && StageBitz.Common.Utils.HasCompanyInventoryStaffMemberPermissions(this.CompanyId, this.UserID, (int?)Eval("LocationId"))  %>' 
                                        Style='<%# "display:" + ((RelatedTableName == "NonProject" && (int)Eval("ItemBookingStatusCodeId") == StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode","INUSE")) ||  (RelatedTableName == "Project" && (int)Eval("ItemBookingStatusCodeId") != StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode","PINNED") || (int)Eval("InventoryStatusCodeId") == StageBitz.Common.Utils.GetCodeIdByCodeValue("InventoryStatusCode","PICKEDUP") || (int)Eval("InventoryStatusCodeId") == StageBitz.Common.Utils.GetCodeIdByCodeValue("InventoryStatusCode","RETURNED")) ? "": "none") %>' 
                                        runat="server" Checked='<%# Eval("IsPickedUp") %>' />
                                    <asp:Image runat="server" Visible='<%# DisplayMode == ViewMode.Project || DisplayMode == ViewMode.NonProject %>' Style='<%# "display:" + ((bool)Eval("IsPickedUp") ? "": "none") %>' 
                                        ID="imgIsPickedUp" ImageUrl="~/Common/Images/tick.gif" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Returned" ItemStyle-HorizontalAlign="Center"
                                HeaderStyle-Width="60" SortExpression="IsReturnedOrder" UniqueName="IsReturned">
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkIsReturned" 
                                        Visible='<%# DisplayMode == ViewMode.Admin && StageBitz.Common.Utils.HasCompanyInventoryStaffMemberPermissions(this.CompanyId, this.UserID, (int?)Eval("LocationId")) %>' 
                                        Enabled='<%# !(RelatedTableName == "NonProject" && (int)Eval("InventoryStatusCodeId") == StageBitz.Common.Utils.GetCodeIdByCodeValue("InventoryStatusCode","RETURNED")) %>' 
                                        Style='<%# "display:" + ((int)Eval("InventoryStatusCodeId") == StageBitz.Common.Utils.GetCodeIdByCodeValue("InventoryStatusCode","PICKEDUP") || (bool)Eval("IsReturned") ? "": "none") %>' 
                                        runat="server" 
                                        Checked='<%# Eval("IsReturned") %>' />
                                    <asp:Image runat="server" Visible='<%# DisplayMode == ViewMode.Project || DisplayMode == ViewMode.NonProject %>' Style='<%# "display:" + ((bool)Eval("IsReturned") ? "": "none") %>' ID="imgIsReturned" ImageUrl="~/Common/Images/tick.gif" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Released" ItemStyle-HorizontalAlign="Center"
                                SortExpression="IsActive" UniqueName="IsActive">
                                <ItemTemplate>
                                    <asp:Image runat="server" Style='<%# "display:" + ((bool)Eval("IsActive") ? "none": "") %>' ID="imgReleased" ImageUrl="~/Common/Images/tick.gif" />
                                    <asp:LinkButton runat="server" 
                                        Visible='<%# (DisplayMode == ViewMode.Admin || DisplayMode == ViewMode.NonProject) && StageBitz.Common.Utils.HasCompanyInventoryStaffMemberPermissions(this.CompanyId, this.UserID, (int?)Eval("LocationId")) %>' 
                                        Text="Release" Style='<%# "display:" + ((int)Eval("InventoryStatusCodeId") == StageBitz.Common.Utils.GetCodeIdByCodeValue("InventoryStatusCode","RETURNED") && (bool)Eval("IsActive") && (int)Eval("ItemBookingStatusCodeId") != StageBitz.Common.Utils.GetCodeIdByCodeValue("ItemBookingStatusCode","PINNED") ? "": "none") %>' ID="lnkRelease" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>

                        </Columns>
                    </MasterTableView>
                    <ClientSettings>
                        <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="465" SaveScrollPosition="True"></Scrolling>
                    </ClientSettings>

                </telerik:RadGrid>
                <telerik:RadDataPager ID="pagerBooking" runat="server" PagedControlID="gvBookingDetails"
                    PageSize="5" Width="100%" OnPreRender="pagerBooking_PreRender">
                    <Fields>
                        <telerik:RadDataPagerButtonField FieldType="FirstPrev" />
                        <telerik:RadDataPagerButtonField FieldType="Numeric" PageButtonCount="5" />
                        <telerik:RadDataPagerButtonField FieldType="NextLast" />
                        <telerik:RadDataPagerPageSizeField PageSizeComboWidth="60" PageSizeText="Page size: "
                            PageSizes="20,50,100" />
                        <telerik:RadDataPagerTemplatePageField>
                            <PagerTemplate>
                                <div style="text-align: right; color: #5a6779; padding-right: 10px;" id="listViewPagerCount">
                                    <asp:Label runat="server" ID="CurrentPageLabel" Text="<%# BookingCount %>" />
                                    <asp:Label runat="server" ID="lblItemText" />
                                    <asp:Label runat="server" ID="TotalItemsLabel" Text="<%# Container.Owner.PageCount %>" />
                                    <asp:Label runat="server" ID="lblPagesText" />
                                </div>
                            </PagerTemplate>
                        </telerik:RadDataPagerTemplatePageField>
                    </Fields>
                </telerik:RadDataPager>
            </BodyContent>
        </sb:GroupBox>
    </ContentTemplate>
</asp:UpdatePanel>

