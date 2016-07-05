<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryBulkUpdatePanel.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Inventory.InventoryBulkUpdatePanel" %>
<%@ Register Src="~/Controls/Inventory/InventoryLocations.ascx" TagPrefix="sb" TagName="InventoryLocations" %>
<%@ Register Src="~/Controls/Inventory/InventoryUpdateVisibility.ascx" TagPrefix="sb" TagName="InventoryUpdateVisibility" %>

<script type="text/javascript">
    var prm = Sys.WebForms.PageRequestManager.getInstance();  //To load js functions after the update panel update
    prm.add_endRequest(function () {
        <%=this.ClientID%>_InitializeControl();
    });

    $(function () {
        <%=this.ClientID%>_InitializeControl();
    });

    function <%=this.ClientID%>_OnRowSelected(itemId) {
        var selectedItemArray = <%=this.ClientID%>_GetItemArray();
        if (selectedItemArray.indexOf(itemId) < 0) {
            selectedItemArray.push(itemId);

            var btnBulkUpdate = $("#<%= btnBulkUpdate.ClientID%>");
            var btnUpdateVisibility = $("#<%= btnUpdateVisibility.ClientID%>");
            btnBulkUpdate.removeAttr('disabled').removeAttr('title');
            btnUpdateVisibility.removeAttr('disabled').removeAttr('title');
        }

        <%=this.ClientID%>_SaveItemArray(selectedItemArray);
    }

    function <%=this.ClientID%>_OnRowDeselected(itemId) {
        var selectedItemArray = <%=this.ClientID%>_GetItemArray();
        selectedItemArray.pop(itemId);
        if (selectedItemArray.length == 0) {
            var btnBulkUpdate = $("#<%= btnBulkUpdate.ClientID%>");
            var btnUpdateVisibility = $("#<%= btnUpdateVisibility.ClientID%>");

            btnBulkUpdate.attr('disabled', 'disabled').attr('title', 'Select Items to be able to bulk update.');
            btnUpdateVisibility.attr('disabled', 'disabled').attr('title', 'Select Items to be able to bulk update.');
        }

        <%=this.ClientID%>_SaveItemArray(selectedItemArray);
    }

    function <%=this.ClientID%>_OnRowCreated(itemId, gridDataItem) {
        var selectedItemArray =  <%=this.ClientID%>_GetItemArray();
        if (selectedItemArray.indexOf(itemId) >= 0) {
            gridDataItem.set_selected(true);
        }
    }

    function <%=this.ClientID%>_GetItemArray() {
        var delimiter = "<%= Delimiter%>";
        var hiddenElement = $("#<%=hdnBulkUpdateSelectedItems.ClientID%>");
        var selectedItemArray = [];
        var hiddenVal = hiddenElement.val();
        if (hiddenVal) {
            selectedItemArray = hiddenVal.split(delimiter);
        }

        return selectedItemArray;
    }

    function <%=this.ClientID%>_SaveItemArray(array) {
        var delimiter = "<%= Delimiter%>";
        var hiddenElement = $("#<%=hdnBulkUpdateSelectedItems.ClientID%>");
        hiddenElement.val(array.join(delimiter));
    }

    function <%=this.ClientID%>_InitializeControl() {
        var selectedItemArray = <%=this.ClientID%>_GetItemArray();
        var btnBulkUpdate = $("#<%= btnBulkUpdate.ClientID%>");
        var btnUpdateVisibility = $("#<%= btnUpdateVisibility.ClientID%>");
        if (selectedItemArray.length == 0) {
            btnBulkUpdate.attr('disabled', 'disabled').attr('title', 'Select Items to be able to bulk update.');
            btnUpdateVisibility.attr('disabled', 'disabled').attr('title', 'Select Items to be able to bulk update.');
        }
        else {
            btnBulkUpdate.removeAttr('disabled').removeAttr('title');
            btnUpdateVisibility.removeAttr('disabled').removeAttr('title');
        }
    }
</script>

<sb:GroupBox ID="grbProjectPanel" runat="server" CssClass="thin">
    <TitleLeftContent>
        <div style="padding: 10px;">
            Bulk Update
        </div>
    </TitleLeftContent>
    <TitleRightContent>
    </TitleRightContent>
    <BodyContent>
        <asp:UpdatePanel ID="upnlBulkUpdate" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <div style="padding: 5px" class="InventoryBookingPanelBoxBorder">
                    <div>
                        Move to Location:&nbsp;
                    </div>
                    <div>
                        <sb:InventoryLocations runat="server" ID="sbInventoryLocations" AccessKey="S" ValidationGroup="BulkUpdate" InventoryLocationDisplayMode="Generic" Width="155" DisableViewOnlyLocations="true" />
                    </div>
                    <div style="text-align: center;">
                        <asp:Button ID="btnBulkUpdate" runat="server" CssClass="buttonStyle center" Text="Update" OnClick="btnBulkUpdate_Click" ValidationGroup="BulkUpdate" />
                        <asp:HiddenField runat="server" ID="hdnBulkUpdateSelectedItems" />
                    </div>
                    <div id="bulkUpdateSavedNotice" class="inlineNotification" runat="server" style="text-align: center;">
                        All Items were successfully moved to
                        <asp:Label runat="server" ID="lblToLocation"></asp:Label>.
                    </div>

                    <sb:PopupBox ID="popupInventoryLocationDeleted" runat="server" Title="Error" ShowCornerCloseButton="false">
                        <BodyContent>
                            <div style="width: 300px;">
                                Selected Inventory Location has already been deleted.
                                <br />
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <input type="button" value="Ok" class="buttonStyle popupBoxCloser" />
                        </BottomStripeContent>
                    </sb:PopupBox>

                    <div runat="server" id="divUpdateVisibility">
                        <hr />

                        <div>
                            <sb:InventoryUpdateVisibility runat="server" ID="sbInventoryUpdateVisibility" />
                        </div>
                        <div style="text-align: center;">
                            <asp:Button ID="btnUpdateVisibility" runat="server" CssClass="buttonStyle center" Text="Update" OnClick="btnUpdateVisibility_Click" />
                        </div>
                        <div id="visibilitySavedNotice" class="inlineNotification" runat="server" style="text-align: center;">
                            Changes Saved.
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </BodyContent>
</sb:GroupBox>

