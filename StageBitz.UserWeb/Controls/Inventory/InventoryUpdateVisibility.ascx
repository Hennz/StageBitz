<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryUpdateVisibility.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Inventory.InventoryUpdateVisibility" %>

<script type="text/javascript">
    function <%=this.ClientID%>_OnClickAnyOption(element) {
        var $element = $(element);
        var sortOrder = $element.attr('data-sortorder');
        var allCheckBoxes = $("input[data-sortorder]", "#<%=divOptions.ClientID%>");

        if ($element.is(':checked')) {
            // Select all check boxes with lesser sort order.
            allCheckBoxes.filter(function () {
                return $(this).attr('data-sortorder') < sortOrder;
            }).prop('checked', true);
        }
        else {
            // Deselect all check boxes with higher sort order.
            allCheckBoxes.filter(function () {
                return $(this).attr('data-sortorder') > sortOrder;
            }).prop('checked', false);
        }

        var isChecked = false;
        allCheckBoxes.each(function () {
            if ($(this).is(':checked') && ($(this).attr('data-sortorder') == 1 || $(this).attr('data-sortorder') == 2)) {
                isChecked = true;
                return false;
            }
        });

        if (isChecked) {
            $("#<%= chkVisibilityInventoryTeam.ClientID%>").prop('checked', true);
        }
        else {
            $("#<%= chkVisibilityInventoryTeam.ClientID%>").prop('checked', false);
        }
    }

    function <%=this.ClientID%>_OnClickCompanyTeam() {
        var $chkVisibilityInventoryObservers = $("#<%= chkVisibilityInventoryObservers.ClientID%>");
        var $chkVisibilityInventoryStaff = $("#<%= chkVisibilityInventoryStaff.ClientID%>");
        var $chkVisibilityInventoryTeam = $("#<%= chkVisibilityInventoryTeam.ClientID%>");
        var $chkVisibilitySharedInventory = $("#<%= chkVisibilitySharedInventory.ClientID%>");

        if ($chkVisibilityInventoryTeam.is(':checked')) {
            $chkVisibilityInventoryObservers.prop('checked', true);
            $chkVisibilityInventoryStaff.prop('checked', true);
        }
        else {
            $chkVisibilityInventoryObservers.prop('checked', false);
            $chkVisibilityInventoryStaff.prop('checked', false);
            $chkVisibilitySharedInventory.prop('checked', false);
        }
    }
</script>

<div>
    <div>
        <div class="left">Choose Visibility:&nbsp;</div>
        <div class="left">
            <sb:HelpTip ID="helptipVisibility" runat="server">
                Your selected Items will only show in search results for the user types you tick; they'll be hidden for everyone else.
            </sb:HelpTip>
        </div>
    </div>
    <br style="clear: both;" />
    <div style="padding-top: 5px;" id="divOptions" runat="server" class="font11">
        <div class="TableWrap">
            <label>
                <input type="checkbox" runat="server" id="chkVisibilityInventoryTeam" />
                <asp:Literal runat="server" ID="ltrlChkVisibilityInventoryTeam"></asp:Literal>
            </label>
            <div style="padding: 2px 20px;">
                <label>
                    <input type="checkbox" runat="server" id="chkVisibilityInventoryStaff" data-sortorder="1" />
                    Inventory Staff
                </label>
                <label>
                    <input type="checkbox" runat="server" id="chkVisibilityInventoryObservers" data-sortorder="2" />
                    Inventory Observers
                </label>
            </div>
        </div>
        <div class="TableWrap" style="padding-top: 5px;">
            <label>
                <input type="checkbox" runat="server" id="chkVisibilitySharedInventory" data-sortorder="3" />
                Visitors from Shared Inventories
            </label>
        </div>
    </div>
    <sb:PopupBox runat="server" ID="popupConcurrencyVisibilityBulkEdit" ShowCornerCloseButton="true" Title="Your update failed.">
        <BodyContent>
            <div style="width:500px;">
                <p style="text-align: center;"><b>We couldn't update all of the Items you selected.</b></p>
                <br />
                <p>
                    At least one of the Items you selected has one or more existing bookings that would be affected by 
                changing their visibility (Items must be visible to a person for that person to book them). You'll see these Item(s) listed.
                </p>
                <br />
                <p>Please change the affected booking or wait for it to be completed, then try updating these Items again.</p>
            </div>
        </BodyContent>
        <BottomStripeContent>
            <input type="button" class="popupBoxCloser buttonStyle" value="Ok" />
        </BottomStripeContent>
    </sb:PopupBox>
    <sb:PopupBox runat="server" ID="popupConcurrencyVisibilityItemDetails" ShowCornerCloseButton="true" Title="Your update failed.">
        <BodyContent>
            <div style="width:500px;">
                <p style="text-align: center;"><b>We couldn't update all of the Items you selected.</b></p>
                <br />
                <p>
                    This Item has one or more existing bookings that would be affected by 
                changing the visibility of this Item(Item must be visible to a person for that person to book them). You'll see these bookings on the Bookings tab.
                </p>
                <br />
                <p>Please change the affected booking or wait for it to be completed, then try updating this Item again.</p>
            </div>
        </BodyContent>
        <BottomStripeContent>
            <input type="button" class="buttonStyle" value="Ok" onclick="location.reload();" />
        </BottomStripeContent>
    </sb:PopupBox>
</div>
