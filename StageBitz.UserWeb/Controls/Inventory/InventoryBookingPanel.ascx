<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryBookingPanel.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Inventory.InventoryBookingPanel" %>

<%@ Register Src="~/Controls/Item/ItemDeletedWarning.ascx" TagName="ItemDeletedWarning" TagPrefix="sb" %>
<style type="text/css">
    .highLightDay {
        background-color: orange;
    }
</style>

<script type="text/javascript">
    var fromDate;
    var toDate;
    var msgItemNotAvailable = 'The Item does not have sufficient units available for this booking period.';
    var msgNoDateFilteration = 'Please choose a booking period';
    var msgSelectItem = 'Select an Item to be able to add it to the Project';
    var hasFilterationSelected = false;
    var availableQty = 0;

    var userId = '<%= UserID %>';

    function ConfigureUI() {
        var self = this;
        hasFilterationSelected = HasFilterationSelected();
        var divMsg = $("#<%= divCriteriaValidation.ClientID %>");
        divMsg.html("");
        divMsg.hide();

        $(document).trigger('onProjectPanelFilterationsChange', [$('#<%= ddBookings.ClientID %>').val(), $('#<%= ddItemTypes.ClientID %>').val(), fromDate, toDate]);

        if (hasFilterationSelected) {
            if (Date.parse(toDate) < Date.parse(fromDate)) {

                divMsg.show();
                if (toDate < fromDate)
                    divMsg.html("Invalid date range.");

                IntializeErrorMessages();
                return;
            }
            else {

                divMsg.hide();
            }
            //doPostback
            var btnDoInventirySearch = document.getElementById("<%= btnDoInventirySearch.ClientID %>");
            btnDoInventirySearch.click();
        }
        else {
            ConfigureProjectPanel(false, msgNoDateFilteration);
        }
    };

    function InitializeDatePickersForProjectPanel() {

        $(function () {
            $("#<%= dtpkPPFrom.ClientID%>").datepicker({
                changeMonth: true,
                changeYear: true,
                dateFormat: 'dd M yy',
                showOn: "button",
                buttonImage: '<%=Page.ResolveUrl("~/Common/Styles/Purple/Calendar/Cal.gif") %>',
                buttonImageOnly: true,
                defaultDate: new Date('<%= StageBitz.Common.Utils.FormatDate(Today) %>'),
                minDate: new Date('<%= StageBitz.Common.Utils.FormatDate(Today) %>'),
                onSelect: function (dateText, inst) {
                    var fromDate = new Date(dateText);
                    var toDate = new Date(fromDate);
                    toDate.setDate(fromDate.getDate());
                    $("#<%= dtpkPPTo.ClientID%>").datepicker("option", "defaultDate", toDate);
                    ConfigureUI();
                    $(document).trigger('onProjectPanelDateFilterationsChange');
                }
            }).keydown(false).change(function () {
                ConfigureUI();
                $(document).trigger('onProjectPanelDateFilterationsChange');
            });

            $("#<%= dtpkPPTo.ClientID%>").datepicker({
                changeMonth: true,
                changeYear: true,
                dateFormat: 'dd M yy',
                showOn: "button",
                buttonImage: '<%=Page.ResolveUrl("~/Common/Styles/Purple/Calendar/Cal.gif") %>',
                buttonImageOnly: true,
                defaultDate: new Date('<%= StageBitz.Common.Utils.FormatDate(Today.AddDays(1)) %>'),
                minDate: new Date('<%= StageBitz.Common.Utils.FormatDate(Today) %>')
            }).keydown(false).change(function () {
                ConfigureUI();
                $(document).trigger('onProjectPanelDateFilterationsChange');
            });
        });
    };

    function PerformValidationForDateFilteration() {
        if (!HasFilterationSelected()) {
            divMsg.show();
            divMsg.html(msgNoDateFilteration);
        }
        else {
            divMsg.hide();
        }
    };

    function HasFilterationSelected() {

        fromDate = $("#<%= dtpkPPFrom.ClientID%>").val();
        toDate = $("#<%= dtpkPPTo.ClientID%>").val()

        if (toDate != '' && fromDate != '') {
            return true;
        }
        else
            return false;

    };

    $(document).ready(function () {
        $(document).trigger('onProjectPanelFilterationsChange', [$('#<%= ddBookings.ClientID %>').val(), $('#<%= ddItemTypes.ClientID %>').val(), fromDate, toDate]);
    });

        function InitializeFilterations() {
            InitializeDatePickersForProjectPanel();
            $('#<%= ddBookings.ClientID %>').change(function () {
                $(document).trigger('onProjectPanelFilterationsChange', [$('#<%= ddBookings.ClientID %>').val(), $('#<%= ddItemTypes.ClientID %>').val(), fromDate, toDate]);
            })

            $('#<%= ddItemTypes.ClientID %>').change(function () {
                $(document).trigger('onProjectPanelFilterationsChange', [$('#<%= ddBookings.ClientID %>').val(), $('#<%= ddItemTypes.ClientID %>').val(), fromDate, toDate]);
            })
        };

        function OnClientResponseError(sender, args) {
            args.set_cancelErrorAlert(true);
        };

        $(document).on("onItemClicked", function (event, itemId, canPin, qtyBooked, toolTip, qtyAvailable) {

            document.getElementById('<%=hdnItemId.ClientID%>').value = itemId;
            document.getElementById('<%=hdnQuantityBooked.ClientID%>').value = qtyBooked;
            document.getElementById('<%=hdnCanPinFromInventory.ClientID%>').value = canPin;
            availableQty = qtyAvailable;

            ConfigureProjectPanel(canPin, toolTip);

        });

        $(document).on("onQuantityChanged", function (event, qtyBooked) {

            document.getElementById('<%=hdnQuantityBooked.ClientID%>').value = qtyBooked;

            //If the status from Inventory is success and if the quantity is not sufficient, show the reason or success message.
            var statusFromInventory = document.getElementById('<%=hdnCanPinFromInventory.ClientID%>').value;
            if ((statusFromInventory == '' || statusFromInventory == 'true') && hasFilterationSelected == true) {
                if (availableQty == 0 || availableQty < qtyBooked)
                    ConfigureProjectPanel(false, msgItemNotAvailable);
                else
                    ConfigureProjectPanel(true, document.getElementById('<%=hdnSuccessAddMessage.ClientID%>').value);
            }

        });

        function ConfigureProjectPanel(canPin, toolTip) {
            $('.headerRow').each(function () {
                if (canPin && HasFilterationSelected()) {
                    $(".buttonDisabled").hide();
                    $('.buttonEnabled').show();
                    if (toolTip) {
                        $(".buttonEnabled").attr('title', toolTip);
                    }
                }
                else {
                    $(".buttonDisabled").show();
                    $(".buttonDisabled").attr('disabled', 'disabled');
                    $(".buttonEnabled").hide();

                    if (toolTip) {
                        $(".buttonDisabled").attr('title', toolTip);
                    }

                }
            });

            $('#<%= btnMakeBooking.ClientID%>').prop('disabled', !canPin);
            if (toolTip) {
                $('#<%= btnMakeBooking.ClientID%>').attr('title', toolTip);
            }
        };

        function <%= this.ClientID %>GetSelectedBookingCode() {
        var bookingId = $("#<%= ddBookings.ClientID %>").val();
            return bookingId ? bookingId : 0;
        };

        function <%= this.ClientID %>GetSelecteItemBriefTypeId() {
        var itemBriefId = $("#<%= ddItemTypes.ClientID %>").val();
            return itemBriefId ? itemBriefId : 0;
        };

        function <%= this.ClientID %>GetSelecteFromDate() {
        return $('#<%= dtpkPPFrom.ClientID %>').val();
        };

        function <%= this.ClientID %>GetSelecteToDate() {
        return $('#<%= dtpkPPTo.ClientID %>').val();
        };

        function client_btnConfirmSave_Click() {
            if (Page_IsValid) {
                $(document).trigger('onSaveItemDetails', [function () {
                    $('#<%= btnConfirmSave.ClientID%>').click();
                }]);
            }
        };

        function ReloadItemDetails() {
            $(document).trigger('onLoadItemDetailsAfterPin');
        };

        function ReloadItemDetailsForDateFilteration(fromDate, toDate, hasDatefilteration, availableQty) {
            $(document).trigger('onLoadItemDetailsForDateFilteration', [fromDate, toDate, hasDatefilteration, availableQty]);
        };

        // NOTE : Any changes to this method should refelct same method in server side.
        function InventoryBookingPanel_GetBookingId(bookingCode) {
            var nonProjectBookingPrefix = "<%= NonProjectBookingPrefix%>";
            var projectBookingPrefix = "<%= ProjectBookingPrefix%>";

            if (bookingCode.indexOf(nonProjectBookingPrefix) > -1) {
                return InventoryBookingPanel_GetBookingIdByPrefix(bookingCode, nonProjectBookingPrefix);
            }
            else if (bookingCode.indexOf(projectBookingPrefix) > -1) {
                return InventoryBookingPanel_GetBookingIdByPrefix(bookingCode, projectBookingPrefix);
            }
            else {
                return 0;
            }
        };

        // NOTE : Any changes to this method should refelct same method in server side.
        function InventoryBookingPanel_GetBookingIdByPrefix(bookingCode, prefix) {
            var codeParts = bookingCode.split(prefix);
            if (codeParts.length > 1) {
                if (!isNaN(codeParts[1])) {
                    return codeParts[1];
                }
            }

            return 0;
        };

        function InventoryBookingPanel_GetBookingTypePrefix(bookingCode) {
            var nonProjectBookingPrefix = "<%= NonProjectBookingPrefix%>";
            var projectBookingPrefix = "<%= ProjectBookingPrefix%>";

            if (bookingCode.indexOf(nonProjectBookingPrefix) > -1) {
                return nonProjectBookingPrefix;
            }
            else if (bookingCode.indexOf(projectBookingPrefix) > -1) {
                return projectBookingPrefix;
            }
            else {
                return '';
            }
        }

        function CreateNewBooking() {
            $("#<%= txtNewBookingName.ClientID%>").val('');
            showPopup('popupCreateNewBooking');
        }
</script>
<sb:GroupBox ID="grbProjectPanel" runat="server" CssClass="thin">
    <TitleLeftContent>
        <div style="position: relative; top: 6px;">
            <div style="float: left;">
                Bookings
            </div>
            <div style="float: left; margin-left: 4px; margin-top: -3px;">
                <sb:HelpTip ID="helpTipProjectPanel" Visible="true" runat="server" Width="470">
                    <b>Bookings</b>
                    <ol>
                        <li>Select a Project or Booking from the dropdown.</li>
                        <li>Enter booking period dates to see the availability for that period in the search results.</li>
                        <li>If you select a Project you will be able to pin Items as suggestions directly to the Item Briefs in that Project.</li>
                        <li>If you select a Booking you will be able to add Items to that booking as a simple list.</li>
                    </ol>

                </sb:HelpTip>
            </div>
        </div>
    </TitleLeftContent>
    <TitleRightContent>
        <div>
            <input type="button" class="buttonStyle" value="New Booking" onclick="CreateNewBooking();" />
        </div>
    </TitleRightContent>
    <BodyContent>

        <asp:UpdatePanel ID="upnl" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <asp:Button ID="btnDoInventirySearch" runat="server" Style="display: none;" OnClick="btnDoInventirySearch_Click" />
                <asp:HiddenField ID="hdnItemId" runat="server" Value="" />
                <asp:HiddenField ID="hdnQuantityBooked" runat="server" Value="" />
                <asp:HiddenField ID="hdnCanPinFromInventory" runat="server" Value="" />
                <asp:HiddenField ID="hdnSuccessAddMessage" runat="server" Value="" />
                <div class="InventoryBookingPanelBoxBorder" id="divInventoryPanel" runat="server">
                    <div style="padding-left: 5px;">
                        <asp:DropDownList Width="200" ID="ddBookings" AutoPostBack="true" OnSelectedIndexChanged="ddBookings_OnSelectedIndexChanged"
                            runat="server" AppendDataBoundItems="true">
                        </asp:DropDownList>
                        <table style="padding-left: 5px;" runat="server" id="tblBookingInputs">
                            <tr runat="server" id="trItemType">
                                <td style="width: 70px;">Item Type:
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddItemTypes" Width="125" AutoPostBack="true" OnSelectedIndexChanged="ddItemTypes_OnSelectedIndexChanged"
                                        runat="server">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">Booking period:
                                <asp:LinkButton ID="lnkClearbtn" Font-Size="Smaller" OnClick="lnkClearbtn_Click" Text="(Clear Dates)" runat="server"></asp:LinkButton>
                                </td>
                            </tr>

                            <tr>
                                <td style="width: 70px;">From
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="dtpkPPFrom" CssClass="datePicker CancelClearInIE" Width="70%"></asp:TextBox>
                                </td>


                            </tr>
                            <tr>
                                <td>To
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="dtpkPPTo" CssClass="datePicker CancelClearInIE" Width="70%"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <div id="divCriteriaValidation" runat="server" style="display: none; width: 150px; top: 4px !important; position: relative;"
                                        class="inputError left">
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="divProjectBookings" runat="server">
                        <div style="padding-left: 5px;">
                            <div style="line-height: 20px; margin-top: 5px; overflow-y: auto;">
                                <div id="divItemBriefs" runat="server" style="overflow-y: auto; overflow-x: hidden; height: 300px; margin-right: 5px;">
                                    <telerik:RadToolTipManager ShowEvent="OnMouseOver" Position="MiddleRight" ID="tooltipManager" OnClientResponseError="OnClientResponseError"
                                        Width="400px" Height="120px" ShowDelay="2000" OnAjaxUpdate="tooltipManager_AjaxUpdate"
                                        runat="server">
                                    </telerik:RadToolTipManager>

                                    <asp:ListView ID="lvItemBriefGroup" runat="server">
                                        <LayoutTemplate>
                                            <table>
                                                <tr id="itemPlaceHolder" runat="server"></tr>
                                            </table>
                                        </LayoutTemplate>
                                        <ItemTemplate>
                                            <tr>
                                                <th>
                                                    <div style="float: left;"><b><%# Eval("GroupName") %> (<%# Eval("GroupCount") %>)</b></div>
                                                </th>
                                            </tr>
                                            <asp:ListView runat="server" OnItemDataBound="lvItemBriefs_OnItemDataBound" DataKeyNames="ItemBriefId" DataSource='<%# Eval("itemBriefList") %>'
                                                OnItemCommand="lvItemBriefs_OnItemCommand" ID="lvItemBriefs">

                                                <LayoutTemplate>
                                                    <tr id="itemPlaceholder" runat="server"></tr>
                                                </LayoutTemplate>
                                                <ItemTemplate>
                                                    <tr id="row" runat="server" class="items">
                                                        <td>
                                                            <div style="width: 100%;" class="left headerRow">
                                                                <div style="padding-top: 1px; width: 18px;" class="left">
                                                                    <asp:ImageButton ID="btnAdd" CommandName="PinToItemBrief" CssClass="buttonEnabled" OnClientClick="PerformValidationForDateFilteration"
                                                                        runat="server" ImageUrl="~/Common/Images/addIcon.png" />
                                                                    <asp:ImageButton ID="btnAddDisabled" CssClass="buttonDisabled"
                                                                        runat="server" ImageUrl="~/Common/Images/addIconDisabled.png" />
                                                                    <asp:ImageButton ID="btnReleasedToInventoryIB" Enabled="false" Visible="false"
                                                                        runat="server" ImageUrl="~/Common/Images/addIconDisabled.png" />
                                                                </div>
                                                                <div class="left">
                                                                    <strong>
                                                                        <a id="lnkItemBriefName" runat="server" href="#" target="_blank"></a>
                                                                    </strong>
                                                                </div>
                                                            </div>
                                                            <div style="padding-left: 15px;" class="left">
                                                                <asp:GridView ID="gvItems" OnRowDataBound="gvItems_OnRowDataBound" CellPadding="0"
                                                                    AutoGenerateColumns="false" CellSpacing="0" runat="server" DataFieldID="ItemID"
                                                                    ShowHeader="false" DataTextField="Name">
                                                                    <Columns>
                                                                        <asp:TemplateField HeaderText="Name">
                                                                            <ItemTemplate>
                                                                                <div style="padding-top: 1px;" class="left">
                                                                                    <asp:Image ImageUrl="~/Common/Images/pin_small.png" ID="imgPin" runat="server" />
                                                                                </div>
                                                                                <div class="left">
                                                                                    <asp:Label ID="lblName" runat="server"></asp:Label>
                                                                                </div>
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </div>
                                                            <div style="clear: both;">
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                            </asp:ListView>

                                        </ItemTemplate>
                                    </asp:ListView>
                                    <br />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div id="divNonProjectBookings" runat="server">
                        <div style="padding-left: 5px;">
                            <div style="line-height: 20px; margin-top: 5px; overflow-y: auto;">
                                <div style="text-align: center;">
                                    <asp:Button ID="btnMakeBooking" runat="server" CssClass="buttonStyle center" Text="Add Item to Booking" OnClick="btnMakeBooking_Click" Enabled="false" />
                                </div>
                                <div style="clear: both; overflow-y: auto; overflow-x: hidden; height: 300px; margin-right: 5px;" id="divNonProjectBookingGrid" runat="server">
                                    <asp:ListView ID="lvNonProjectBookings" runat="server">
                                        <LayoutTemplate>
                                            <table>
                                                <tr id="itemPlaceHolder" runat="server"></tr>
                                            </table>
                                        </LayoutTemplate>
                                        <ItemTemplate>
                                            <tr>
                                                <th>
                                                    <div style="float: left;">
                                                        <b title='<%# ((string)Eval("CompanyName")).Length > 20 ? Eval("CompanyName") : string.Empty %>'>
                                                            <%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("CompanyName"), 20)%> (<%# Eval("CompanyBookingCount") %>)</b>
                                                    </div>
                                                </th>
                                            </tr>
                                            <asp:ListView runat="server" DataSource='<%# Eval("StatusGroups") %>' ID="lvStatusGroups">
                                                <LayoutTemplate>
                                                    <tr id="itemPlaceholder" runat="server"></tr>
                                                </LayoutTemplate>
                                                <ItemTemplate>
                                                    <tr>
                                                        <th>
                                                            <div style="padding-left: 10px;" class="left"><i class="fa fa-circle" style="font-size: 10px;"></i>&nbsp;<%# Eval("Status") %> (<%# Eval("ItemCount") %>)</div>
                                                        </th>
                                                    </tr>
                                                    <tr id="row" runat="server" class="items">
                                                        <td>
                                                            <div style="padding-left: 15px; width: 85%;" class="left">
                                                                <asp:ListView runat="server" DataSource='<%# Eval("Items") %>' ID="lvMyBookingItems" OnItemCommand="lvMyBookingItems_ItemCommand">
                                                                    <LayoutTemplate>
                                                                        <table style="width: 100%;">
                                                                            <tr id="itemPlaceholder" runat="server"></tr>
                                                                        </table>
                                                                    </LayoutTemplate>
                                                                    <ItemTemplate>
                                                                        <tr>
                                                                            <td>
                                                                                <div style="padding-top: 1px;" class="left">
                                                                                    <img src='<%#ResolveUrl("~/Common/Images/pin_small.png")%>' title="" />
                                                                                </div>
                                                                                <div class="left" style="font-size: 12px;">
                                                                                    <a id="lnkItemDetails" runat="server" target="_blank"
                                                                                        href='<%# ResolveUrl(string.Format("~/Inventory/ItemDetails.aspx?ItemId={0}&CompanyId={1}", (int)Eval("ItemId"), (int)Eval("CompanyId"))) %>'
                                                                                        title='<%# ((string)Eval("ItemName")).Length > 12 ? Eval("ItemName") : string.Empty %>'>
                                                                                        <%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString((string)Eval("ItemName"), 12) %>
                                                                                    </a>
                                                                                </div>
                                                                                <div class="right">
                                                                                    <asp:LinkButton runat="server" ID="lbtnRemove" Font-Underline="false" ToolTip="Remove"
                                                                                        CommandName="DeleteItemBooking" Visible='<%#(bool)Eval("CanDelete") %>' CommandArgument='<%#Eval("ItemBookingId").ToString()%>' ClientIDMode="AutoID">
                                                                                                    <i class="fa fa-times"></i>
                                                                                    </asp:LinkButton>
                                                                                </div>
                                                                            </td>
                                                                        </tr>
                                                                    </ItemTemplate>
                                                                </asp:ListView>
                                                            </div>
                                                            <div style="clear: both;">
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                            </asp:ListView>

                                        </ItemTemplate>
                                    </asp:ListView>
                                </div>
                                <div id="divNoDataNonProjectBookings" style="text-align: center;" class="noData" runat="server">
                                    Enter a booking period, select an Item and add it to the list. It's that easy.
                                </div>
                            </div>
                        </div>
                    </div>
                    <div id="divEmptyBookings" runat="server" style="padding-left: 10px; padding-top: 20px;">
                        <p>
                            <b>Select an existing Project or Booking from the dropdown.</b>
                        </p>
                    </div>
                </div>
                <div>
                    <sb:PopupBox ID="popupConfirmationForAlreadyCompleted" Title="Pin Item" runat="server">
                        <BodyContent>
                            <div style="text-align: left; width: 450px;">
                                <p>
                                    The Item you are adding this to has information in the Complete Item tab.
                                            <br />
                                    If you add this Inventory Item to the Pinboard tab...
                                            <br />
                                    1. The Complete Item tab will be cleared of information and attachments.
                                            <br />
                                    2. Both Items will appear on the Pinboard ready for approval.
                                            <br />
                                    Do you wish to continue?
                                </p>
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnConfirm" runat="server" OnClick="btnConfirm_Click" CssClass="buttonStyle"
                                Text="Confirm" />
                            <asp:Button ID="btnCancel" runat="server" CssClass="popupBoxCloser buttonStyle" Text="Cancel" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                    <sb:PopupBox ID="popupConfirmItemDetailSave" runat="server" Title="Save unsaved changes" ShowCornerCloseButton="false">
                        <BodyContent>
                            <div style="width: 300px;">
                                You have unsaved changes in this page. Do you want to save them and proceed?<br />
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <input type="button" value="Yes" onclick="client_btnConfirmSave_Click(); return false;" class="buttonStyle" />
                            <asp:Button ID="btnCancelSave" CssClass="buttonStyle" runat="server"
                                Text="No" OnClick="btnCancelSave_Click" />
                            <asp:Button ID="btnConfirmSave" runat="server" Text="" OnClick="btnConfirmSave_Click" Style="display: none;" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                    <sb:PopupBox ID="popUpAlreadyPinned" runat="server" Title="This Item cannot be booked again">
                        <BodyContent>
                            <asp:Literal ID="litAlreadyPinned" runat="server"></asp:Literal>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnReloadData" runat="server" OnClick="btnReloadData_Click" CssClass="buttonStyle" Text="OK" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                    <sb:PopupBox ID="popupBookingCannotDeleted" runat="server" Title="Error">
                        <BodyContent>
                            <div style="width: 350px;">
                                <asp:Label Text="This Item cannot be removed." runat="server" ID="lblBookingCannotDeleted"></asp:Label>
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnReload1" runat="server" OnClick="btnReload_Click" CssClass="buttonStyle" Text="OK" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                    <sb:PopupBox ID="popupBookingArchived" runat="server" Title="This Booking has been archived.">
                        <BodyContent>
                            <div style="width: 350px;">
                                This Item cannot be booked to the selected booking.
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnReload2" runat="server" OnClick="btnReload_Click" CssClass="buttonStyle" Text="OK" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                    <sb:PopupBox ID="popupDeleteBooking" runat="server" Title="Remove Item">
                        <BodyContent>
                            <div style="width: 300px;">
                                <asp:HiddenField runat="server" ID="hdnDeleteItemBooking" />
                                Are you sure you want to remove this Item?
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnConfirmRemoveBooking" runat="server" OnClick="btnConfirmRemoveBooking_Click" CssClass="buttonStyle" Text="Yes" />
                            <input type="button" class="popupBoxCloser buttonStyle" value="No" />
                        </BottomStripeContent>
                    </sb:PopupBox>

                    <sb:PopupBox ID="popupPinnedStatus" Title="Error" runat="server">
                        <BodyContent>
                            <div style="text-align: left; width: 300px;">
                                <p>
                                    <asp:Literal ID="litpopupPinnedStatus" runat="server"></asp:Literal>
                                </p>
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="Button1" runat="server" CssClass="popupBoxCloser buttonStyle" Text="Ok" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                    <sb:PopupBox ID="popupPinClosedProjectsItemBriefs" Title="This Project is finished and has been closed" runat="server">
                        <BodyContent>
                            <div style="text-align: left; width: 450px; padding-top: 20px; padding-left: 30px; padding-right: 30px;">
                                <asp:Literal ID="litPersonClosedProject" runat="server"></asp:Literal>
                                has just closed this Project.
                                        <br />
                                <br />
                                This means you will not be able to add or edit any information to this 
                                            Project and it will no longer appear on your Personal Dashboard.
                                        <br />
                                <br />
                                <asp:Literal ID="litClosedProjectCompanyName" runat="server"></asp:Literal>
                                Administrators are able to access a read only version of the Project 
                                        from the Company Dashboard. Please contact your Primary Company Administrator,
                                        <asp:HyperLink ID="hyperLinkClosedProjectCompanyAdming" runat="server"></asp:HyperLink>
                                if you have any questions. 
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnDone" runat="server" CssClass="buttonStyle" OnClick="btnDone_Click" Text="Done" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                    <sb:ItemDeletedWarning runat="server" ID="popupItemDeletedWarning" />
                    <sb:PopupBox runat="server" ID="popupCreateNewBooking" Title="Create a new booking">
                        <BodyContent>
                            <div style="width: 400px; text-align: center;">
                                Booking name:&nbsp;<asp:TextBox runat="server" ID="txtNewBookingName" Width="200" MaxLength="100" ValidationGroup="CreateNewBooking"></asp:TextBox>
                                <span style="position: relative; top: 2px;">
                                    <asp:RequiredFieldValidator runat="server" ID="rfvNewBookingName" ControlToValidate="txtNewBookingName" ErrorMessage="*"
                                        CssClass="inputError" ValidationGroup="CreateNewBooking"
                                        ToolTip="Booking Name is required."></asp:RequiredFieldValidator>
                                </span>
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnCreateNewBooking" runat="server" CssClass="buttonStyle" Text="Confirm" ValidationGroup="CreateNewBooking" OnClick="btnCreateNewBooking_Click" />
                            <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </BodyContent>
</sb:GroupBox>

