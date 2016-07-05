<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryLocations.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Inventory.InventoryLocations" %>
<script type="text/javascript">
    $(document).bind('click', function (e) {
        var $clicked = $(e.target);
        if ($clicked.closest("#<%= this.ClientID%>").length == 0) {
            <%= this.ClientID%>_HideDropDown();
        }
    });

    $(document).ready(function () {
        <%= this.ClientID%>_InitializeLocationDropdown();
    });

    var prm = Sys.WebForms.PageRequestManager.getInstance();  //To load js functions after the update panel update

    prm.add_endRequest(function () {
        <%= this.ClientID%>_InitializeLocationDropdown();
    });

    function <%= this.ClientID%>_InitializeLocationDropdown() {
        <%= this.ClientID%>_HideDropDown();

        $("#<%= divLocationTree.ClientID%>").keydown(function (event) {
            event.preventDefault();
            event.stopPropagation();

            if (event.keyCode != 9) {
                if (event.keyCode == 27) { // Esc key
                    <%= this.ClientID%>_HideDropDown();
                }
            }
            else {
                $("#<%= divLocationTree.ClientID%>").slideUp(0, function () {
                    <%= this.ClientID%>_FocusNextInput();
                });
            }
        });

        $("#<%= txtLocation.ClientID%>").keydown(function (e) {
            if (e.keyCode == 27) { // Esc key
                <%= this.ClientID%>_HideDropDown();
            }
            else {
                $("#<%= rtvLocations.ClientID%>").focus();
            }
            return false;
        });

        $("#<%= txtLocation.ClientID%>:enabled").focus(function () {
            $("#<%= divLocationTree.ClientID%>").slideDown("slow", function () {
                $("#<%= rtvLocations.ClientID%>").focus();
            });
        });
    }

    function <%= this.ClientID%>_OnClientNodeClicking(sender, args) {
        var txtLocation = $("#<%= txtLocation.ClientID %>");
        var node = args.get_node();
        if (node.get_isEnabled()) {
            var locationBreadcrumbArray = BuildLocationBreadCrumb(node, 25);
            if (locationBreadcrumbArray.length > 1) {
                txtLocation.val(locationBreadcrumbArray[0]).change();
                txtLocation.prop('title', locationBreadcrumbArray[1]);
            }

            $("#<%= divLocationTree.ClientID%>").slideUp(0);
            <%= this.ClientID%>_FocusNextInput();
        }
    }


    function <%= this.ClientID%>_HideDropDown() {
        $("#<%= divLocationTree.ClientID%>").slideUp(0, function () {
            $("#<%= this.ClientID%>").focus();
        });
    }

    function <%= this.ClientID%>_FocusNextInput() {
        var nextInput = "<%= this.ibtnClearSearch.ClientID%>";
        if (nextInput) {
            $("*[id$='" + nextInput + "']").focus();
        }
    }

    function <%=ibtnClearSearch.ClientID%>_ClientClick() {
        var element = $("#<%= this.ClientID %>");
        InventoryLocations_Jquery_SetValue(element, null);
        return false;
    }

    function <%=ibtnClearSearch.ClientID%>_ClientKeyPress(event) {
        var keyPressed = event.keyCode || event.which;
        if (keyPressed == 13) {
            var element = $("#<%= this.ClientID %>");
            InventoryLocations_Jquery_SetValue(element, null);
        }

        return false;
    }

</script>
<div class="left">
    <div class="clearBox" id="<%= this.ClientID %>">
        <asp:TextBox runat="server" ID="txtLocation" Height="15" placeholder="Select a location" Width="140"></asp:TextBox>
        <asp:ImageButton ID="ibtnClearSearch" CssClass="searchButton" runat="server" CausesValidation="false" Text=""
            ImageUrl="~/Common/Images/button_cancel.png" Width="16" Height="16"
            OnClick="ibtnClearSearch_Click" />

        <div id="divLocationTree" style="z-index: 101; display:none;" runat="server" class="locationTree">
            <telerik:RadTreeView runat="server" ID="rtvLocations" AccessKey="T" Width="100%" Height="140px" DataFieldID="LocationId"
                DataFieldParentID="ParentLocationId" DataTextField="LocationName" DataValueField="LocationId" TabIndex="1000">
            </telerik:RadTreeView>
        </div>
    </div>
</div>
<div class="left" style="padding-top: 5px;">
    <asp:RequiredFieldValidator runat="server" ID="rfvLocation" ControlToValidate="txtLocation" ErrorMessage="*" ToolTip="Please select a location."></asp:RequiredFieldValidator>
</div>
<div style="clear: both;"></div>

