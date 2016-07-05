<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryLocationRoles.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Inventory.InventoryLocationRoles" %>
<script type="text/javascript">
    function <%= this.ClientID%>_OnSelectLocationRoleHeader(cssClass) {
        $('input:enabled.' + cssClass, "#<%= this.ClientID%> .LocationRoles").prop('checked', true);
        // inform page about radio button select
        $(document).trigger('<%= this.ClientID%>_onLocationRoleSelect');
    }

    function <%= this.ClientID%>_OnRoleClick(cssClass, isInitialize) {
        var selectedCount = $('input:checked.' + cssClass, "#<%= this.ClientID%> .LocationRoles").length;
        var allCount = $('input.' + cssClass, "#<%= this.ClientID%> .LocationRoles").length;

        if (allCount == selectedCount) {
            $('input.' + cssClass, "#<%= this.ClientID%> .AllLocations").prop('checked', true);
        }
        else {
            if (isInitialize) {
                $('input.' + cssClass, "#<%= this.ClientID%> .AllLocations").prop('checked', false);
            }
            else {
                $('input:checked', "#<%= this.ClientID%> .AllLocations").prop('checked', false
                    );
            }
        }

        if (!isInitialize) {
            // inform page about radio button select
            $(document).trigger('<%= this.ClientID%>_onLocationRoleSelect');
        }
    }

    function <%= this.ClientID%>_InitializeUI() {
        <%= this.ClientID%>_OnRoleClick('LM', true);
        <%= this.ClientID%>_OnRoleClick('IS', true);
        <%= this.ClientID%>_OnRoleClick('IO', true);
        <%= this.ClientID%>_OnRoleClick('NoAccess', true);
        <%= this.ClientID%>_DisabledLocationRows();
    }

    function <%= this.ClientID%>_UncheckAll() {
        $('input:checked', "#<%= this.ClientID%>").prop('checked', false);
    }

    function <%= this.ClientID%>_ValidateUISelection() {
        var isValid = true;
        $("#<%= this.ClientID%> tr.LocationRoles:not(.Disabled)").each(function () {
            var locationRoleRow = $(this);
            isValid = isValid && $('input:checked', locationRoleRow).length > 0;
        });

        return isValid;
    }

    function <%= this.ClientID%>_DisabledLocationRows() {
        $("tr.LocationRoles.Disabled input", "#<%= this.ClientID%>").prop('disabled', true);
    }
</script>
<style type="text/css">
    div.helpTipCentered .helpTipIcon {
        margin-left: 40%;
    }
</style>

<div id='<%= this.ClientID %>' class="RadGrid RadGrid_Default">
    <table class="rgMasterTable rgClipCells" style="width: 100%">
        <tr>
            <th class="rgHeader" style="width: 40%"></th>
            <th class="rgHeader" style="width: 15%; text-align: center;" runat="server" id="thAllLocationManager">
                <b style="display: block;">Location</b>
                <b>Manager</b>
                <div class="helpTipCentered">
                    <sb:HelpTip runat="server" ID="helptipLM">
                        A Location Manager:
                        <ul>
                            <li>Receives any correspondence regarding a booking.</li>
                            <li>Can approve/deny bookings.</li>
                            <li>Has full administration access to their Inventory Location, including inviting team members.</li>
                        </ul>
                    </sb:HelpTip>
                </div>
            </th>
            <th class="rgHeader" style="width: 15%; text-align: center;">
                <b style="display: block;">Inventory</b>
                <b>Staff</b>
                <div class="helpTipCentered">
                    <sb:HelpTip runat="server" ID="helptip1">
                        Inventory Staff can:
                        <ul style="margin: 0px;">
                            <li>Add, edit and delete Items.</li>
                            <li>View bookings and check Items in and out.</li>
                            <li>Make their own bookings within the Inventory.</li>
                        </ul>
                        They cannot approve/deny bookings
                    </sb:HelpTip>
                </div>
            </th>
            <th class="rgHeader" style="width: 15%; text-align: center;">
                <b style="display: block;">Inventory</b>
                <b>Observer</b>
                <div class="helpTipCentered">
                    <sb:HelpTip runat="server" ID="helptip2">
                        Inventory Observers can:
                        <ul style="margin: 0px;">
                            <li>Browse the Inventory Items.</li>
                            <li>Create booking requests.</li>
                        </ul>
                        They cannot view Bookings apart from their own.
                    </sb:HelpTip>
                </div>
            </th>
            <th class="rgHeader" style="width: 15%; text-align: center;">
                <b style="display: block;">No</b>
                <b>Access</b>
                <div class="helpTipCentered">
                    <sb:HelpTip runat="server" ID="helptip3">
                        'No Access' means Items in the selected locations will not be visible to this team member at all.
                    </sb:HelpTip>
                </div>
            </th>
        </tr>
        <tr class="rgAltRow AllLocations">
            <td style="border-bottom-width: 2px !important"><b>All Managed Locations</b></td>
            <td style="border-bottom-width: 2px !important; text-align: center;" runat="server" id="tdAllLocationManager">
                <input type="radio" runat="server" id="rbtnLocationManagerAll" name='<%# string.Concat("All_", this.RadioButtonGroupName) %>' class="LM" /></td>
            <td style="border-bottom-width: 2px !important; text-align: center;">
                <input type="radio" runat="server" id="rbtnInventoryStaffAll" name='<%# string.Concat("All_", this.RadioButtonGroupName) %>' class="IS" /></td>
            <td style="border-bottom-width: 2px !important; text-align: center;">
                <input type="radio" runat="server" id="rbtnInventoryObserverAll" name='<%# string.Concat("All_", this.RadioButtonGroupName) %>' class="IO" /></td>
            <td style="border-bottom-width: 2px !important; text-align: center;">
                <input type="radio" runat="server" id="rbtnNoAccessAll" name='<%# string.Concat("All_", this.RadioButtonGroupName) %>' class="NoAccess" /></td>
        </tr>
        <asp:Repeater runat="server" ID="rptrLocations">
            <ItemTemplate>
                <tr class="<%# (Container.ItemIndex % 2 == 0 ? "LocationRoles rgRow" : "LocationRoles rgAltRow") +  ((bool)Eval("CanEdit") ? string.Empty : " Disabled") %>">
                    <td>
                        <asp:Label runat="server" ID="lblLocation"
                            Text='<%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("Location.LocationName"), 25) %>'
                            ToolTip='<%# ((string)Eval("Location.LocationName")).Length > 25 ? Eval("Location.LocationName") : string.Empty %>'></asp:Label>
                        <asp:HiddenField runat="server" ID="hdnLocationId" Value='<%# (int)Eval("Location.LocationId")%>' />
                        <asp:HiddenField runat="server" ID="hdnCanEdit" Value='<%# Eval("CanEdit")%>' />
                    </td>
                    <td style="text-align: center;" runat="server" visible='<%# this.InventoryRolesDisplayMode == DisplayMode.EditMode %>'>
                        <input type="radio" runat="server" id="rbtnLocationManager"
                            name='<%# string.Concat(((int)Eval("Location.LocationId")).ToString(), "_", this.RadioButtonGroupName) %>' class="LM"
                            onclick='<%# string.Concat(this.ClientID, "_OnRoleClick(\"LM\", false)")%>' /></td>
                    <td style="text-align: center;">
                        <input type="radio" runat="server" id="rbtnInventoryStaff"
                            name='<%# string.Concat(((int)Eval("Location.LocationId")).ToString(), "_", this.RadioButtonGroupName) %>' class="IS"
                            onclick='<%# string.Concat(this.ClientID, "_OnRoleClick(\"IS\", false)")%>' /></td>
                    <td style="text-align: center;">
                        <input type="radio" runat="server" id="rbtnInventoryObserver"
                            name='<%# string.Concat(((int)Eval("Location.LocationId")).ToString(), "_", this.RadioButtonGroupName) %>' class="IO"
                            onclick='<%# string.Concat(this.ClientID, "_OnRoleClick(\"IO\", false)")%>' /></td>
                    <td style="text-align: center;">
                        <input type="radio" runat="server" id="rbtnNoAccess"
                            name='<%# string.Concat(((int)Eval("Location.LocationId")).ToString(), "_", this.RadioButtonGroupName) %>' class="NoAccess"
                            onclick='<%# string.Concat(this.ClientID, "_OnRoleClick(\"NoAccess\", false)")%>' /></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                <tr>
                    <td colspan="5">
                        <div class="noData">
                            No Managed Locations found
                        </div>
                    </td>
                </tr>
            </FooterTemplate>
        </asp:Repeater>
    </table>
</div>
