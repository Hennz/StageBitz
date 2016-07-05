<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditBookingDetails.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Inventory.EditBookingDetails" %>

<script type="text/javascript">
    var globalData;
    var bookingDHeaderDetailsObj;
    function pageLoad() {
        var tableView = $find("<%= gvBookingDetails.ClientID %>").get_masterTableView();//telerik Grid Object
        bookingDHeaderDetailsObj.GridView = tableView;
        bookingDHeaderDetailsObj.DatePicker = $find("<%= dtPeriod.ClientID %>");
        bookingDHeaderDetailsObj.GridTable = $("table[id*='gvBookingDetails']");
        bookingDHeaderDetailsObj.HeaderUIElement = $("#tblHeaderInfo");
        bookingDHeaderDetailsObj.UIElement = $("#divgvBookingDetails");

        bookingDHeaderDetailsObj.popupConflictErrorID = $('#<%= popupConflictError.ClientID %>');
        bookingDHeaderDetailsObj.ButtonConfirm = $('#<%= btnConfirm.ClientID %>');
        bookingDHeaderDetailsObj.LoadData();

        $("#divPreviousNav").click(function () {
            bookingDHeaderDetailsObj.NavigatePrevious();
        });
        $("#divNextNav").click(function () {
            bookingDHeaderDetailsObj.NavigateNext();
        });

        $('#<%=btnConfirm.ClientID%>').click(function (evt) {
            evt.preventDefault(); //prevent postback
            bookingDHeaderDetailsObj.SaveChanges();
        });

    }

    function LoadBookingDetails_<%= this.ClientID %>(Data) {
        bookingDHeaderDetailsObj = new StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader();
        bookingDHeaderDetailsObj.BookingId = Data.BookingId;
        bookingDHeaderDetailsObj.IsInventoryManager = Data.IsInventoryManager;
        bookingDHeaderDetailsObj.CompanyId = Data.CompanyId;
        bookingDHeaderDetailsObj.UserId = Data.UserId;
        bookingDHeaderDetailsObj.ItemTypeId = Data.ItemTypeId;
        bookingDHeaderDetailsObj.IsToDateEdit = Data.IsToDateEdit;
        bookingDHeaderDetailsObj.ToDay = Data.ToDay;
    }

    function HeaderCheckClicked(cell) {
        bookingDHeaderDetailsObj.HeaderDateClicked(cell.value, $(cell).is(':checked'));
    }

    function OnDateSelected(sender, e) {
        if (e.get_newDate() != null) {
            bookingDHeaderDetailsObj.SetNewDate();
        }
    }

    function OnColumnCreated(sender, args) {
        var column = args.get_column();
        switch (column.get_uniqueName()) {
            case "Date":
                column.get_element().innerHTML = bookingDHeaderDetailsObj.IsToDateEdit ? "Booked To" : "Booked From"; //setting new HeaderText
                break;
        }
    }

</script>

<table id="tblHeaderInfo">
    <tr>
        <td>Booking Name:
            <asp:Label ID="lblBookingName" runat="server"></asp:Label></td>
    </tr>
    <tr>
        <td>Booking ID:
            <asp:Label ID="lblBookingNumber" runat="server"></asp:Label>
        </td>
    </tr>
</table>
<div style="height: 5px;"></div>
<sb:GroupBox ID="grpBookings" runat="server">
    <TitleLeftContent>
        <div style="float: left;">
            <span class="boldText">
                <asp:Literal ID="litTitle" runat="server"></asp:Literal></span>
        </div>
        <div style="float: left; margin-left: 4px;">
            <sb:HelpTip ID="helpTipBooking" Visible="true" runat="server" Width="470">
                <b>This page allows you to change the
                    <asp:Literal ID="litDateTypeHeader" runat="server"></asp:Literal>
                    dates for a Booking.</b>
                <ul>
                    <li>Select the new
                        <asp:Literal ID="litDateTypeBody" runat="server"></asp:Literal>
                        date for any Items you would like to change and click confirm.
                    </li>
                    <li>If an Item has another Booking you will not be able to select it for that date.
                    </li>
                </ul>
            </sb:HelpTip>
        </div>
    </TitleLeftContent>
    <BodyContent>
        
        <telerik:RadFormDecorator ID="QsfFromDecorator" runat="server" DecoratedControls="All" EnableRoundedCorners="false" />

        <div id="divgvBookingDetails">
            <div style="display: none;" id="itemtypeOverlay">
                <div class="updateProgressOverlay">
                </div>
                <div class="updateProgressIcon">
                </div>
            </div>
            <sb:PopupBox Title="Hold on... there's a conflict with your dates" ID="popupConflictError" ShowCornerCloseButton="false" runat="server">
                <BodyContent>
                    <p>
                        Someone else has already booked at least one of these Items for the new dates you're trying to book.
                    </p>
                    <p>
                        When you close this pop-up, the availability of each Item will be refreshed.
                    </p>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnOKpopupConflictError" runat="server" OnClientClick="return false;" CssClass="popupBoxCloser buttonStyle" Text="OK" />
                </BottomStripeContent>
            </sb:PopupBox>

            <div id="DataArea">
                <div style="height: 40px;">
                    <div style="margin-left: 300px; margin-top: 1px;" class="left">
                        <div style="padding-top: 5px;" class="left">
                            Choose Month:&nbsp;
                        </div>
                        <div class="left">
                            <telerik:RadMonthYearPicker ID="dtPeriod" ToolTip="Select a Period." DatePopupButton-ToolTip="Select a Period." runat="server">
                                <ClientEvents OnDateSelected="OnDateSelected"></ClientEvents>
                            </telerik:RadMonthYearPicker>
                        </div>
                    </div>
                    <div style="width: 150px; margin-left: 60px;" id="divPreviousNav" class="DivNavigation">
                        <div class="left">
                            <asp:Image runat="server" ID="imgPrevious" ImageUrl="~/Common/Images/previous.png" />
                        </div>
                        <div class="left" style="padding-top: 5px;">
                            Previous Month    
                        </div>
                    </div>
                    <div style="width: 140px" id="divNextNav" class="DivNavigation">
                        <div class="left" style="padding-top: 5px;">
                            Next Month    
                        </div>
                        <div class="left">
                            <asp:Image runat="server" ID="imgNext" ImageUrl="~/Common/Images/next.png" />
                        </div>
                    </div>
                    <div style="clear: both;"></div>
                </div>

                <div style="overflow-x:auto;width:920px;" class="dirtyValidationArea">
                    <telerik:RadGrid ID="gvBookingDetails" runat="server" EnableViewState="false" Width="1600" OnPreRender="gvBookingDetails_PreRender" AutoGenerateColumns="false">
                        <ItemStyle Wrap="false"></ItemStyle>
                        <MasterTableView TableLayout="Auto">
                            <Columns>
                                <telerik:GridBoundColumn DataField="ItemBookingId" HeaderStyle-Width="0" HeaderText="">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="ItemBriefName" UniqueName="ItemBriefName" HeaderStyle-Width="100" HeaderText="Item Brief">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="ItemName" HeaderStyle-Width="100" HeaderText="Item Name">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="Date" DataType="System.DateTime" HeaderStyle-CssClass="DateClass" UniqueName="Date" HeaderStyle-Width="105">
                                </telerik:GridBoundColumn>
                            </Columns>
                        </MasterTableView>
                        <ClientSettings>
                            <Scrolling UseStaticHeaders="true" AllowScroll="True" ></Scrolling>
                            <ClientEvents OnColumnCreated="OnColumnCreated" />
                        </ClientSettings>
                    </telerik:RadGrid>
                </div>
            </div>

            <div style="display: none; text-align: center;" id="divNoData">
                All Items booked for this Item Type have already been released to the Inventory.
            </div>
        </div>

    </BodyContent>
</sb:GroupBox>
<asp:Button ID="btnConfirm" CssClass="buttonStyle" runat="server" Text="Confirm" />
<asp:Button ID="btnCancel" CausesValidation="false" runat="server" Text="Cancel" CssClass="ignoreDirtyFlag buttonStyle" />
<div id="BookingDetailsSavedNotice" class="inlineNotification right">
    Changes saved.
</div>

