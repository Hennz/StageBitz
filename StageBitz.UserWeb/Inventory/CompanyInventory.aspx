<%@ Page DisplayTitle="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="CompanyInventory.aspx.cs" Inherits="StageBitz.UserWeb.Inventory.CompanyInventory" %>

<%@ Register Src="~/Controls/Common/ListViewDisplaySettings.ascx" TagName="ListViewDisplaySettings"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Inventory/InventoryBookingPanel.ascx" TagName="InventoryBookingPanel"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/ExportData.ascx" TagName="ExportData" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Inventory/ContactInventoryManager.ascx" TagName="ContactInventoryManager"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Item/ImportBulkItems.ascx" TagName="ImportbulkItems"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>

<%@ Register Src="~/Controls/Item/ItemDeletedWarning.ascx" TagName="ItemDeletedWarning" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Inventory/InventorySharingRemovedWarning.ascx" TagName="InventorySharingRemovedWarning" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Company/CompanyWarningDisplay.ascx" TagPrefix="sb" TagName="CompanyWarningDisplay" %>
<%@ Register Src="~/Controls/Company/PackageLimitsValidation.ascx" TagPrefix="sb" TagName="PackageLimitsValidation" %>
<%@ Register Src="~/Controls/Inventory/InventoryLocations.ascx" TagPrefix="sb" TagName="InventoryLocations" %>
<%@ Register Src="~/Controls/Inventory/InventoryBulkUpdatePanel.ascx" TagPrefix="sb" TagName="InventoryBulkUpdatePanel" %>
<%--Head Content--%>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script src='<%# ResolveUrl("../Common/Scripts/ComboSearchEvents.js?v="+ this.ApplicationVersionString) %>'></script>
    <script src='<%# ResolveUrl("../Common/Scripts/Inventory.js?v="+ this.ApplicationVersionString) %>'></script>
    <script type="text/javascript">
        var _gaq = _gaq || [];
        _gaq.push(['_setCustomVar', 1, 'Category', 'Inventory', 2]);
        _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyId %>', 2]);
        _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
    </script>
    <script type="text/javascript">

        var urlDelimiter = '<%= StageBitz.Common.Constants.GlobalConstants.parameterDelimiter %>';

        function OnClientResponseError(sender, args) {
            args.set_cancelErrorAlert(true);
        }

        function gvItemBulkEdit_OnRowSelected(sender, args) {
            var itemId = args.get_item().getDataKeyValue('Item.ItemId');
            if (<%=inventoryBulkUpdatePanel.ClientID%>_OnRowSelected) {
                <%=inventoryBulkUpdatePanel.ClientID%>_OnRowSelected(itemId);
            }
        }

        function gvItemBulkEdit_OnRowDeselected(sender, args) {
            var itemId = args.get_item().getDataKeyValue('Item.ItemId');
            if (<%=inventoryBulkUpdatePanel.ClientID%>_OnRowDeselected) {
                <%=inventoryBulkUpdatePanel.ClientID%>_OnRowDeselected(itemId);
            }
        }

        function gvItemBulkEdit_OnRowCreated(sender, args) {
            var itemId = args.get_item().getDataKeyValue('Item.ItemId');
            if (<%=inventoryBulkUpdatePanel.ClientID%>_OnRowCreated) {
                <%=inventoryBulkUpdatePanel.ClientID%>_OnRowCreated(itemId, args.get_gridDataItem());
            }
        }

        // Make checkboxes function like radio buttons
        function InitializeCheckBoxes() {
            var global = this;
            var contentArea = $("div[id$='divItemList'], div[id$='gvItemList'], div[id$='gvWatchList']");
            $("input[type='radio']", contentArea).attr("name", "inventoryItems");
            $("input[type='radio']:disabled", contentArea).css("opacity", "0.5");

            $("input[type='radio']", contentArea).click(function () {
                var self = $(this);

                contentArea.find("span[id*='txtQtyBooked']").each(function () {
                    $(this).hide();
                });

                //Display the spinner
                var txtQty = self.nextAll("span[id*='txtQtyBooked']");
                txtQty.show();

                var projectBookingPrefix = "<%= StageBitz.Logic.Business.Inventory.InventoryBL.ProjectBookingPrefix%>";
                var nonProjectBookingPrefix = "<%= StageBitz.Logic.Business.Inventory.InventoryBL.NonProjectBookingPrefix%>";

                var itemId = self.next('input[id$="hdnItemId"]').val();
                var selectedBookingCode = <%= inventoryProjectPanel.ClientID%>GetSelectedBookingCode();
                var selectedBookingId = InventoryBookingPanel_GetBookingId(selectedBookingCode);
                var bookingTypePrefix = InventoryBookingPanel_GetBookingTypePrefix(selectedBookingCode);

                var selectedItemBriefTypeId = <%= inventoryProjectPanel.ClientID%>GetSelecteItemBriefTypeId();
                var fromDate = <%=inventoryProjectPanel.ClientID%>GetSelecteFromDate();
                var toDate = <%=inventoryProjectPanel.ClientID%>GetSelecteToDate();

                var qtyBooked = self.closest('.selectItemClass').find("input[id$='txtQtyBooked']").val();
                var param = null;
                var url = null;
                //call the WebMethod
                if (bookingTypePrefix == projectBookingPrefix) {
                    param = '{"itemId" : ' + itemId + ',"selectedProjectId" : ' + selectedBookingId +
                        ',"selectedItemBriefTypeId" : ' + selectedItemBriefTypeId + ',"fromDate" : "' + fromDate +
                        '","toDate" : "' + toDate + '","bookedQty" : ' + qtyBooked + '}';
                    url = '<%=ResolveUrl("~/Inventory/CompanyInventory.aspx/GetToolTipToDisplayInProjectPanel") %>';
                }
                else if (bookingTypePrefix == nonProjectBookingPrefix) {
                    param = '{"itemId" : ' + itemId + ', "bookingId" : ' + selectedBookingId + ', "fromDate" : "' + fromDate +
                        '","toDate" : "' + toDate + '","bookedQty" : ' + qtyBooked + '}';
                    url = '<%=ResolveUrl("~/Inventory/CompanyInventory.aspx/GetToolTipToDisplayInMyBookingPanel") %>';
                }

                if (param && url) {
                    $.ajax({
                        type: "POST",
                        url: url,
                        data: param,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (msg) {
                            $(document).trigger('onItemClicked', [itemId, msg.d.CanPin, 1, msg.d.ToolTip, msg.d.AvailableQuantity]);

                            //Set the Min/Max value for the Quantity Booked
                            global.SetBookedQuantityMaxValue(contentArea, itemId, msg.d.AvailableQuantity);

                        }
                    });
                }


                //Reset the value to 1 (Because It may have changed from earlier round)
                var txtQtyInput = txtQty.children("input[id$='txtQtyBooked']");
                var hdnQuantity = self.nextAll("input[id$='hdnQuantity']");
                txtQtyInput.val(hdnQuantity.val() == 0 ? "0" : "1");
            });
    };

    var cboSearchEvents = new StageBitz.UserWeb.Common.Scripts.ComboSearchEvents();
    cboSearchEvents.FindButton = "#<%= btnFind.ClientID %>";

    function cboSearch_onItemsRequested(sender, eventArgs) {
        cboSearchEvents.OnItemsRequested(sender, eventArgs);
    };

    function cboSearch_onKeyPressing(sender, eventArgs) {
        cboSearchEvents.OnKeyPressing(sender, eventArgs);
    };

    function cboSearch_OnClientFocus(sender, eventArgs) {
        cboSearchEvents.OnClientFocus(sender, eventArgs);
    };

    function cboSearch_onSelectedIndexChanged(sender, eventArgs) {
        cboSearchEvents.OnSelectedIndexChanged(sender, eventArgs);
    };

    var _inventoryBookingCode;
    var _inventoryBookingItemTypeId;
    var _isGridView = null;

    function InitializeListViewUrls() {
        _isGridView = false;
        var thums = $("div.thumbListItem", "div[id$='divItemList']");
        InitializeUrls(thums);
    };

    function InitializeGridViewUrls() {
        _isGridView = true;
        var gridRows = $("tr.rgRow, tr.rgAltRow", "div[id$='gvItemList']");
        InitializeUrls(gridRows);
    };

    function InitializeUrls(elements) {
        $.each(elements, function () {
            var itemId = $("input[id$='hdnItemId']", $(this)).val();
            var link = $("a.lnkItemDetails", $(this));
            var bookingQty = $(this).find("input[id$='txtQtyBooked']").val();

            if (link) {
                var href = link.attr("href");
                var newHref = GetItemUrl(href, itemId, bookingQty);
                link.attr("href", newHref);
            }
        });
    };

    function GetItemUrl(url, itemId, bookedQty) {
        var itemId = encodeURIComponent(getParameterByName(url, "ItemId"));
        var sorting = encodeURIComponent(getParameterByName(url, "Sort"));
        var companyId = encodeURIComponent(getParameterByName(url, "CompanyId"));
        var booking = encodeURIComponent(GetBookingParam(bookedQty));
        var inventory = encodeURIComponent(GetInventoryParam());
        return url.split("?")[0] + "?ItemId=" + itemId + "&CompanyId=" + companyId + "&Sort=" + sorting + "&Booking=" + booking + "&Inventory=" + inventory;
    };

    function getParameterByName(url, name) {
        var match = RegExp('[?&]' + name + '=([^&]*)')
                .exec(url);

        return match ? decodeURIComponent(match[1].replace(/\+/g, ' ')) : null;
    };

    function GetBookingParam(bookedQty) {
        return _inventoryBookingCode + urlDelimiter + _inventoryBookingItemTypeId + urlDelimiter + <%= inventoryProjectPanel.ClientID%>GetSelecteFromDate() + urlDelimiter
            + <%= inventoryProjectPanel.ClientID%>GetSelecteToDate() + urlDelimiter + bookedQty;
    };

    function GetInventoryParam() {
        var addItemTypeId = $("select[id$='ddlAddItemTypes']").val();
        var searchItemTypeId = $("select[id$='ddlSearchItemTypes']").val();
        var sppCompanyId = $("select[id$='ddlSPPCompanies']").val();
        var SearchLocationId = InventoryLocations_Jquery_GetValue($("*[id$='searchInvLocation']"));
        var AddLocationId = InventoryLocations_Jquery_GetValue($("*[id$='sbInventoryLocations']"));
        if (!sppCompanyId) {
            sppCompanyId = '';
        }

        return addItemTypeId + urlDelimiter + searchItemTypeId + urlDelimiter + sppCompanyId + urlDelimiter + SearchLocationId + urlDelimiter + AddLocationId;
    };

    function BindEvents(startBookingCode, startItemTypeId, startFromDate, startToDate) {

        var self = this;
        _inventoryBookingCode = startBookingCode;
        _inventoryBookingItemTypeId = startItemTypeId;
        _fromDate = startFromDate;
        _toDate = startToDate;

        $(document).on("onProjectPanelFilterationsChange", function (event, bookingCode, itemTypeId, fromDate, toDate) {
            _inventoryBookingCode = bookingCode;
            _inventoryBookingItemTypeId = itemTypeId;
            _fromDate = fromDate;
            _toDate = toDate;
            if (_isGridView != null) {
                if (_isGridView) {
                    InitializeGridViewUrls();
                }
                else {
                    InitializeListViewUrls();
                }
            }
        });


        $(document).on("onAvailableQtyChanged", function (event, itemId, availableQty) {
            var contentArea = $("div[id$='divItemList'], div[id$='gvItemList'], div[id$='gvWatchList']");
            SetBookedQuantityMaxValue(contentArea, itemId, availableQty);
        });



        $("select[id$='ddlAddItemTypes']").change(function () {
            if (_isGridView) {
                InitializeGridViewUrls();
            }
            else {
                InitializeListViewUrls();
            }
        });

        $("#listViewPagerCount").parent("div").css("float", "right");
        $("span.rdpPagerLabel").parent("div").css("margin-left", "20px");
    };


    function SetBookedQuantityMaxValue(contentArea, itemId, availableQty) {
        var numerickInput = contentArea.find('input[value=' + itemId + ']').nextAll().find("input[id$='txtQtyBooked']").get(0).id;
        var currentQtyBooked = $find(numerickInput).get_value();
        var qtyToSet = availableQty == 0 ? availableQty : availableQty < currentQtyBooked ? availableQty : currentQtyBooked != '' ? currentQtyBooked : 1;
        $find(numerickInput).set_value(qtyToSet);
        $find(numerickInput).set_textBoxValue(qtyToSet);

        $find(numerickInput).set_maxValue(availableQty);
        $find(numerickInput).set_minValue(availableQty == 0 ? availableQty : 1);
    }

    function BookedQtyChanged(sender, eventArgs) {
        // trigger an event to check from Project panel
        var bookedQty = eventArgs.get_newValue();
        $(document).trigger('onQuantityChanged', bookedQty);
        var txtQty = sender.get_element();
        var td = $(txtQty).closest('.selectItemClass');
        var itemId = td.closest('tr').find('input[id$="hdnItemId"]').val();
        var link;
        if (_isGridView) {
            link = td.closest('tr').find("a.lnkItemDetails");
        }
        else {
            link = td.closest('tr').siblings('tr.trThumbView').eq(0).find('td.tdThumbView').find("a.lnkItemDetails")
        }

        if (link.length >= 1) {
            var href = link.attr("href");
            var newHref = GetItemUrl(href, itemId, bookedQty);
            link.attr("href", newHref);
        }
    }

    function pageLoad() {
        globalDocumentReady();
        IntializeErrorMessages();
        if (_isGridView) {
            InitializeGridViewUrls();
        }
        else {
            InitializeListViewUrls();
        }

    }
    </script>
</asp:Content>
<%--Navigation Links--%>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    |<asp:HyperLink ID="hyperLinkCompanyInventory" runat="server">Company Inventory</asp:HyperLink>
    |
    <asp:HyperLink ID="hyperLinkMyBooking" runat="server">My Bookings</asp:HyperLink>
    <span runat="server" id="spnInventorySharing">|<asp:HyperLink ID="hyperLinkInventorySharing" runat="server">Manage Inventory</asp:HyperLink></span>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageTitleRight" runat="server">

    <table>
        <tr>
            <td style="width: 200px; text-align: right;">Display: </td>
            <td>
                <asp:RadioButtonList ID="groupWatchList" Width="110" runat="server" AutoPostBack="True" OnSelectedIndexChanged="rbtnWatchList_OnSelectedIndexChanged">
                    <asp:ListItem runat="server" id="litSearchResult" Text="Search Results" Selected="True"></asp:ListItem>
                    <asp:ListItem runat="server" id="litWatchList" Text="Watch List"></asp:ListItem>
                </asp:RadioButtonList>

            </td>
        </tr>
    </table>
    <br />
    <asp:UpdatePanel runat="server" ID="upnlManagerMode">
        <ContentTemplate>

            <div id="divManagerMode" runat="server" class="right">
                <table>
                    <tr>
                        <td style="width: 50%; text-align: right;" nowrap>Manager Mode:&nbsp;</td>
                        <td style="width: 50%; text-align: right; padding-bottom: 6px;">
                            <telerik:RadButton ID="tbtnManagerMode" runat="server" ToggleType="CheckBox" ButtonType="ToggleButton"
                                ValidationGroup="ManagerMode" AutoPostBack="true" Width="40" ToolTip="Off" OnCheckedChanged="tbtnManagerMode_CheckedChanged">
                                <ToggleStates>
                                    <telerik:RadButtonToggleState Text="" PrimaryIconCssClass="switchOn" HoveredCssClass="hover" />
                                    <telerik:RadButtonToggleState Text="" PrimaryIconCssClass="switchOff" HoveredCssClass="hover" />
                                </ToggleStates>
                            </telerik:RadButton>
                            <span style="position: relative; top: 4px;" class="right">
                                <sb:HelpTip ID="helptipInventoryMode" runat="server" Width="470">
                                    Edit Items in bulk without having to change each one individually. Please note Locations Managers may only edit items from their own Location(s).
                                </sb:HelpTip>
                            </span>
                        </td>
                    </tr>
                </table>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
<%--Main Content--%>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <sb:ContactInventoryManager ID="contactInventoryManager" runat="server" />
    <sb:ProjectWarningPopup ID="projectWarningPopupInventory" runat="server"></sb:ProjectWarningPopup>
    <sb:CompanyWarningDisplay runat="server" ID="sbCompanyWarningDisplay" />

    <telerik:RadWindowManager ID="RadWindowManager1" runat="server">
    </telerik:RadWindowManager>
    <div id="divSearchItems" runat="server">

        <asp:Panel ID="pnlFindItems" DefaultButton="btnFind" runat="server">
            <div style="width: 100%;" class="left">
                <div class="right">
                    <sb:ExportData ID="exportData" OnExcelExportClick="exportData_ExcelExportClick" OnPDFExportClick="exportData_PDFExportClick" runat="server" />
                </div>
                <div class="right">
                    <asp:UpdatePanel ID="upnlImportBulkItems" UpdateMode="Conditional" runat="server">
                        <ContentTemplate>
                            <div id="divImportBulkItems" runat="server">
                                <sb:ImportbulkItems ID="ImportbulkItemsControl" runat="server" />
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
            <div class="left">
                <asp:UpdatePanel ID="upnlFindItems" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <div class="left" style="width: 33%; padding-left:5px;">
                            <b>Search criteria:</b>
                        </div>
                        <div class="left" style="width: 45.25%;">
                            <b>Filter search results by:</b>
                        </div>
                        <div class="left" style="width: 20%;">
                            <b><asp:Label runat="server" ID="lblVisibilityText"></asp:Label></b>
                        </div>
                        <div style="margin-right: 2px;" class="left searchbox rounded">
                            <span style="position: relative; top: 2px;">

                                <telerik:RadComboBox runat="server" ID="cboSearch" AutoPostBack="false" OnItemsRequested="cboSearch_ItemsRequested"
                                    OnClientItemsRequested="cboSearch_onItemsRequested" OnClientKeyPressing="cboSearch_onKeyPressing"
                                    EnableLoadOnDemand="true" EmptyMessage="Search for item..." ChangeTextOnKeyBoardNavigation="true"
                                    ShowWhileLoading="false" ShowToggleImage="false" MaxLength="100" Width="190"
                                    OnClientFocus="cboSearch_OnClientFocus" OnClientSelectedIndexChanged="cboSearch_onSelectedIndexChanged">
                                    <ExpandAnimation Type="None" />
                                    <CollapseAnimation Type="None" />
                                </telerik:RadComboBox>
                                <span style="position: relative; bottom: 2px;">
                                    <asp:ImageButton ID="ibtnClearSearch" runat="server" CausesValidation="false" Text=""
                                        ImageUrl="~/Common/Images/button_cancel.png" Width="16" Height="16" CssClass="searchButton"
                                        OnClick="ibtnClearSearch_Click" /></span>

                            </span>

                        </div>
                        <div style="margin-right: 10px;" class="left">
                            <asp:Button ID="btnFind" runat="server" CssClass="buttonStyle"
                                OnClick="btnFind_Click" Text="Find" ValidationGroup="InventorySearch" />
                        </div>
                        <div id="divCompany" runat="server" style="width: 150px; margin-right: 25px;" class="left">
                            <asp:DropDownList Height="25" runat="server" ID="ddlSPPCompanies" AppendDataBoundItems="true"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlSPPCompanies_OnSelectedIndexChanged" Width="170">
                            </asp:DropDownList>
                        </div>
                        <div id="divItemType" runat="server" style="vertical-align: bottom; margin-right: 5px;" class="left">
                            <asp:DropDownList Height="25" Width="180" runat="server" ID="ddlSearchItemTypes" AppendDataBoundItems="true"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlSearchItemTypes_OnSelectedIndexChanged">
                                <asp:ListItem Text="All Items" Value=""></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div style="margin-top: 5px;" class="left">
                            <sb:InventoryLocations runat="server" Width="200" InventoryLocationDisplayMode="SearchInventory" ID="searchInvLocation" AccessKey="S" />
                        </div>
                        <div style="vertical-align: bottom; margin-left: 5px;" class="left">
                            <sb:DropDownListOPTGroup runat="server" ID="ddlVisibilityFilter" Height="25" Width="195"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlVisibilityFilter_SelectedIndexChanged">
                            </sb:DropDownListOPTGroup>
                        </div>
                        <div style="clear: both;"></div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div style="clear: both;"></div>
        </asp:Panel>

    </div>

    <sb:GroupBox ID="grpItems" runat="server">
        <TitleLeftContent>
            <asp:UpdatePanel runat="server" ID="upnlDisplaySettings" UpdateMode="Conditional">
                <ContentTemplate>
                    <div style="float: left;">
                        <asp:Label CssClass="boldText" ID="ltrlItemListTitle" runat="server"></asp:Label>
                    </div>
                    <div style="float: left;">
                        <sb:HelpTip ID="helpTipInventory" Visible="true" runat="server" Width="470">
                            Search for Items, then pin them to Item Briefs in the Project Panel or add them to your Watch List.
                        </sb:HelpTip>
                    </div>
                    <br />
                    <div style="font-size: smaller;">
                        <asp:Literal ID="ltrlItemListTitleForDateFilteration" runat="server"></asp:Literal>
                    </div>
                    <div style="clear: both;"></div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </TitleLeftContent>
        <TitleRightContent>
            <asp:UpdatePanel runat="server" ID="upnlDisplayMode">
                <ContentTemplate>
                    <sb:ListViewDisplaySettings ID="displaySettings" OnDisplayModeChanged="displaySettings_DisplayModeChanged"
                        runat="server" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </TitleRightContent>
        <BodyContent>
            <div>
                <div id="divAddInventoryItem" runat="server" style="background: #F2F0F4">
                    <asp:UpdatePanel runat="server" ID="upnlAdd" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Panel ID="pnlAddItem" DefaultButton="btnAdd" runat="server">
                                <table style="width: 100%;">
                                    <tr>
                                        <td></td>
                                        <td>Name</td>
                                        <td>Description</td>
                                        <td>Qty</td>
                                        <td>Inventory Location</td>
                                        <td>Item Type</td>
                                        <td></td>
                                    </tr>
                                    <tr>
                                        <td>Add New Item:</td>
                                        <td>
                                            <asp:TextBox ID="txtName" Width="120" MaxLength="100" Height="20" runat="server"></asp:TextBox>
                                            <asp:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender2" TargetControlID="txtName"
                                                WatermarkText="New Item Name..." runat="server">
                                            </asp:TextBoxWatermarkExtender>
                                            <asp:RequiredFieldValidator ID="reqName" runat="server" ControlToValidate="txtName"
                                                SkinID="Hidden" ValidationGroup="ItemFields" ErrorMessage="Name is required."></asp:RequiredFieldValidator>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtDescription" Height="20" Width="120" runat="server"></asp:TextBox>
                                        </td>

                                        <td>
                                            <telerik:RadNumericTextBox ID="txtQuantity" Width="40" Height="24" runat="server">
                                            </telerik:RadNumericTextBox>
                                            <asp:RangeValidator ID="rngItemQuantity" runat="server" ControlToValidate="txtQuantity"
                                                SkinID="Hidden" Style="top: 0px;" MinimumValue="0" MaximumValue="99999999999"
                                                ValidationGroup="ItemFields" ErrorMessage="Quantity cannot be zero."></asp:RangeValidator>
                                            <asp:RequiredFieldValidator ID="reqQuantity" runat="server" ControlToValidate="txtQuantity"
                                                SkinID="Hidden" ValidationGroup="ItemFields" ErrorMessage="Quantity is required."></asp:RequiredFieldValidator>
                                        </td>
                                        <td>
                                            <sb:InventoryLocations runat="server" Width="200" InventoryLocationDisplayMode="Generic" ID="sbInventoryLocations" AccessKey="S" DisableViewOnlyLocations="true" />
                                        </td>
                                        <td>
                                            <asp:DropDownList runat="server" ID="ddlAddItemTypes" Height="25" Width="170" AppendDataBoundItems="true">
                                                <asp:ListItem Text="Select Item Type..." Value=""></asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:RequiredFieldValidator ID="reqItemType" runat="server" ControlToValidate="ddlAddItemTypes"
                                                SkinID="Hidden" ValidationGroup="ItemFields" ErrorMessage="Item Type is required."></asp:RequiredFieldValidator>
                                        </td>
                                        <td>
                                            <asp:Button ID="btnAdd" runat="server" ValidationGroup="ItemFields" OnClick="btnAdd_Click"
                                                Text="Add to Inventory" CssClass="buttonStyle" />
                                            <sb:PackageLimitsValidation runat="server" ID="sbPackageLimitsValidation" />
                                        </td>
                                    </tr>
                                </table>
                                <asp:ValidationSummary ID="validationSummaryItemAdd" ValidationGroup="ItemFields"
                                    DisplayMode="List" CssClass="message error" runat="server" />

                                <sb:PopupBox ID="popupSaveGenericError" runat="server" Title="Can not create Item" Height="100">
                                    <BodyContent>
                                        <asp:Label ID="lblError" runat="server"></asp:Label>
                                    </BodyContent>
                                    <BottomStripeContent>
                                        <asp:Button ID="Button2" CssClass="buttonStyle" runat="server"
                                            Text="OK" OnClientClick="hidePopup('popupSaveGenericError');" />
                                    </BottomStripeContent>
                                </sb:PopupBox>

                            </asp:Panel>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div style="width: 698px;" class="left boxBorderDark">
                    <asp:UpdatePanel ID="upnlItemList" UpdateMode="Conditional" runat="server">
                        <ContentTemplate>
                            <sb:ItemDeletedWarning runat="server" ID="popupItemDeletedWarning" />
                            <sb:InventorySharingRemovedWarning runat="server" ID="popupInventorySharingRemovedWarning" />
                            <telerik:RadToolTipManager ID="tooltipManager" Width="400px" Height="120px" ShowDelay="2000" OnClientResponseError="OnClientResponseError"
                                Position="MiddleRight" OnAjaxUpdate="tooltipManager_AjaxUpdate" runat="server">
                            </telerik:RadToolTipManager>
                            <div>
                                <div runat="server" id="divItemGrid" style="overflow-y: auto; height: 518px;">
                                    <telerik:RadGrid ID="gvItemList" EnableLinqExpressions="False" AutoGenerateColumns="false"
                                        AllowSorting="true" OnItemDataBound="gvItemList_ItemDataBound" OnSortCommand="gvItemList_SortCommand"
                                        AllowAutomaticUpdates="True" OnNeedDataSource="gvItemList_NeedDataSource" runat="server"
                                        AllowPaging="True" PageSize="20" Height="516" PagerStyle-AlwaysVisible="true" AllowCustomPaging="true"
                                        Width="675" OnItemCommand="gvItemList_ItemCommand">
                                        <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                                        <MasterTableView EditMode="InPlace" DataKeyNames="Item.ItemId,Item.LastUpdatedDate"
                                            AllowNaturalSort="false" AllowMultiColumnSorting="true" Width="680">
                                            <AlternatingItemStyle Height="32" />
                                            <ItemStyle Height="32" />
                                            <NoRecordsTemplate>
                                                <div class="noData">
                                                    No results found
                                                </div>
                                            </NoRecordsTemplate>
                                            <Columns>
                                                <telerik:GridTemplateColumn UniqueName="Name" HeaderText="Name" SortExpression="Item.Name"
                                                    HeaderStyle-Width="105">
                                                    <ItemTemplate>
                                                        <a id="lnkItemDetails" runat="server" href="#" class="lnkItemDetails"></a>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn UniqueName="Description" HeaderText="Description"
                                                    HeaderStyle-Width="120">
                                                    <ItemTemplate>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn HeaderText="Item Type" SortExpression="ItemTypeName"
                                                    UniqueName="ItemTypeName" HeaderStyle-Width="70px">
                                                    <ItemTemplate>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn UniqueName="Quantity" SortExpression="Item.Quantity" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Center"
                                                    HeaderText="Qty" HeaderStyle-Width="40" DataType="System.Int32">
                                                    <ItemTemplate>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn UniqueName="AvailableQty" HeaderStyle-HorizontalAlign="Center" SortExpression="AvailableQty" ItemStyle-HorizontalAlign="Right" HeaderText="Available" HeaderStyle-Width="50">
                                                    <ItemTemplate>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn UniqueName="Select" ItemStyle-CssClass="selectItemClass" HeaderText="Select" HeaderStyle-Width="75">
                                                    <ItemStyle VerticalAlign="Middle" />
                                                    <ItemTemplate>
                                                        <asp:RadioButton runat="server" ID="rbtnItem" />
                                                        <asp:HiddenField runat="server" ID="hdnItemId" Value='<%# Bind("Item.ItemId") %>' />
                                                        <asp:HiddenField runat="server" ID="hdnQuantity" Value='<%# Bind("Item.Quantity") %>' />
                                                        <telerik:RadNumericTextBox ID="txtQtyBooked" Display="false" DisplayText="1" Width="50" ShowSpinButtons="true" runat="server">
                                                            <ClientEvents OnValueChanged="BookedQtyChanged" />
                                                        </telerik:RadNumericTextBox>

                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Center" HeaderText="Watch List"
                                                    HeaderStyle-Width="72" UniqueName="WatchList">
                                                    <ItemTemplate>
                                                        <span class="left smallText grayText">
                                                            <asp:LinkButton ID="lnkAddToWatchList" CssClass="smallText"
                                                                runat="server">Add</asp:LinkButton>
                                                            <asp:Literal ID="ltrlAddedToWatchList" runat="server">Added</asp:Literal>
                                                        </span>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                            </Columns>
                                        </MasterTableView>
                                        <ClientSettings>
                                            <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="465" SaveScrollPosition="True"></Scrolling>
                                        </ClientSettings>
                                    </telerik:RadGrid>
                                </div>
                                <div runat="server" id="divBulkUpdate">
                                    <telerik:RadGrid ID="gvItemBulkEdit" EnableLinqExpressions="False" AutoGenerateColumns="false"
                                        AllowSorting="true" OnItemDataBound="gvItemBulkEdit_ItemDataBound" OnSortCommand="gvItemBulkEdit_SortCommand"
                                        AllowAutomaticUpdates="True" OnNeedDataSource="gvItemBulkEdit_NeedDataSource" runat="server"
                                        AllowPaging="True" AllowCustomPaging="true" PageSize="20" Height="516" PagerStyle-AlwaysVisible="true"
                                        Width="675" AllowMultiRowSelection="true">
                                        <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                                        <ClientSettings>
                                            <Selecting AllowRowSelect="true" />
                                        </ClientSettings>
                                        <MasterTableView EditMode="InPlace" DataKeyNames="Item.ItemId,Item.LastUpdatedDate" ClientDataKeyNames="Item.ItemId"
                                            AllowNaturalSort="false" AllowMultiColumnSorting="true" Width="680">
                                            <AlternatingItemStyle Height="32" />
                                            <ItemStyle Height="32" />
                                            <NoRecordsTemplate>
                                                <div class="noData">
                                                    No results found
                                                </div>
                                            </NoRecordsTemplate>
                                            <Columns>
                                                <telerik:GridClientSelectColumn UniqueName="ClientSelectColumn" HeaderStyle-Width="25">
                                                </telerik:GridClientSelectColumn>
                                                <telerik:GridTemplateColumn UniqueName="Name" HeaderText="Name" SortExpression="Item.Name"
                                                    HeaderStyle-Width="100">
                                                    <ItemTemplate>
                                                        <a id="lnkItemDetails" runat="server" href="#" class="lnkItemDetails"></a>
                                                        <asp:Image runat="server" ID="imgError" ImageUrl="../Common/Images/error.png" Visible='<%#(bool)Eval("HasError") %>' CssClass="right"
                                                            ToolTip='<%#(string)Eval("ErrorMessage") %>'/>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn UniqueName="Description" HeaderText="Description"
                                                    HeaderStyle-Width="100">
                                                    <ItemTemplate>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn HeaderText="Item Type" SortExpression="ItemTypeName"
                                                    UniqueName="ItemTypeName" HeaderStyle-Width="70px">
                                                    <ItemTemplate>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn UniqueName="Quantity" SortExpression="Item.Quantity" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Center"
                                                    HeaderText="Qty" HeaderStyle-Width="40" DataType="System.Int32">
                                                    <ItemTemplate>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn HeaderText="Location" HeaderStyle-Width="100" UniqueName="Location">
                                                    <ItemTemplate>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                            </Columns>
                                        </MasterTableView>
                                        <ClientSettings>
                                            <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="465" SaveScrollPosition="True"></Scrolling>
                                            <ClientEvents OnRowSelected="gvItemBulkEdit_OnRowSelected" OnRowDeselected="gvItemBulkEdit_OnRowDeselected" OnRowCreated="gvItemBulkEdit_OnRowCreated" />
                                        </ClientSettings>
                                    </telerik:RadGrid>
                                </div>
                                <div id="divWatchList" runat="server">
                                    <div id="divEmptyWatchList" class="WatchListEmptyDataBox" runat="server">
                                        <div style="text-align: center; width: 600px; height: 400px; border: 1px solid #e4e1e8; margin-top: 50px; margin-left: 40px; font-weight: bold; font-size: large;">
                                            <div style="margin-top: 80px;"></div>
                                            This is your watch list.
                                            <br />
                                            <br />
                                            You can add Items you find in your searches to this list and
                                            <br />
                                            compare them later.<br />
                                            <br />
                                            <div id="divemptyNormalWatchList" runat="server">
                                                You will then be able to add them to your Projects. 
                                            </div>
                                            <div id="divemptySPPWatchList" runat="server">
                                                If you are working on a Project for the company that owns the
                                                <br />
                                                Items you will be able to add them to your Project.<br />
                                                <br />
                                                If not you will be able to contact the Booking Manager who
                                                <br />
                                                looks after them to ask a question and arrange a booking.
                                            </div>
                                        </div>
                                    </div>
                                    <div runat="server" id="divWatchListItems" style="overflow-y: auto; height: 550px;">
                                        <telerik:RadGrid ID="gvWatchList" EnableLinqExpressions="false" AutoGenerateColumns="false" OnItemCommand="gvWatchList_OnItemCommand"
                                            AllowSorting="true" OnItemDataBound="gvWatchList_ItemDataBound" OnSortCommand="gvWatchList_SortCommand"
                                            AllowAutomaticUpdates="True" OnNeedDataSource="gvWatchList_NeedDataSource" runat="server"
                                            Height="516" PagerStyle-AlwaysVisible="true" OnDeleteCommand="gvWatchList_DeleteCommand"
                                            Width="680" OnPreRender="gvWatchList_PreRender">
                                            <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                                            <MasterTableView EditMode="InPlace" DataKeyNames="ItemId,WatchListHeaderId,CompanyId" AllowNaturalSort="false" AllowMultiColumnSorting="true" Width="680">
                                                <NoRecordsTemplate>
                                                    <div class="noData">
                                                        No results found
                                                    </div>
                                                </NoRecordsTemplate>
                                                <GroupByExpressions>
                                                    <telerik:GridGroupByExpression>
                                                        <SelectFields>
                                                            <telerik:GridGroupByField HeaderText="Items from " FieldName="CompanyName" FormatString="{0:D}"></telerik:GridGroupByField>
                                                        </SelectFields>
                                                        <GroupByFields>
                                                            <telerik:GridGroupByField FieldName="CompanyId" SortOrder="Descending"></telerik:GridGroupByField>
                                                        </GroupByFields>
                                                    </telerik:GridGroupByExpression>
                                                </GroupByExpressions>
                                                <SortExpressions>
                                                    <telerik:GridSortExpression FieldName="CompanyId" SortOrder="Ascending" />
                                                    <telerik:GridSortExpression FieldName="Name" SortOrder="Ascending" />
                                                </SortExpressions>
                                                <Columns>
                                                    <telerik:GridTemplateColumn UniqueName="CompanyName" ItemStyle-HorizontalAlign="Center"
                                                        HeaderStyle-Width="20" ItemStyle-Width="30">
                                                        <ItemTemplate>
                                                            <asp:ImageButton runat="server" ID="imgbtnSendEmail" CommandName="SendEmail" ToolTip="Send an email to the Booking Manager" Visible="false"
                                                                ImageUrl="~/Common/Images/email.png" />
                                                            <asp:ImageButton runat="server" ID="imgbtnEmailSent" CommandName="SendEmail" ToolTip="Email has already been sent to the Booking Manager" Visible="false"
                                                                ImageUrl="~/Common/Images/email_sent.png" />
                                                            <asp:Label ID="lblCompanyName" runat="server"></asp:Label>
                                                            <asp:HiddenField runat="server" ID="hdnCompanyId" />
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn UniqueName="Image" HeaderText="Image"
                                                        HeaderStyle-Width="60">
                                                        <ItemTemplate>
                                                            <sb:ImageDisplay ID="itemThumbDisplay" ShowImagePreview="false" runat="server" />
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn UniqueName="Name" HeaderText="Name" SortExpression="Name"
                                                        HeaderStyle-Width="50">
                                                        <ItemTemplate>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn UniqueName="Description" HeaderText="Description"
                                                        HeaderStyle-Width="50">
                                                        <ItemTemplate>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn UniqueName="Quantity" DataType="System.Int32" SortExpression="Quantity" ItemStyle-HorizontalAlign="Right"
                                                        HeaderText="Qty" HeaderStyle-Width="30">
                                                        <ItemTemplate>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn UniqueName="AvailableQty" SortExpression="AvailableQuantity" HeaderText="Available" ItemStyle-HorizontalAlign="Right" HeaderStyle-Width="45">
                                                        <ItemTemplate>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn UniqueName="Select" ItemStyle-CssClass="selectItemClass" HeaderText="Select"
                                                        HeaderStyle-Width="50">
                                                        <ItemTemplate>
                                                            <asp:RadioButton runat="server" ID="rbtnItem" />
                                                            <asp:HiddenField runat="server" ID="hdnItemId" Value='<%# Bind("ItemId") %>' />
                                                            <telerik:RadNumericTextBox ID="txtQtyBooked" Display="false" DisplayText="1" Width="50" ShowSpinButtons="true" runat="server">
                                                                <ClientEvents OnValueChanged="BookedQtyChanged" />
                                                            </telerik:RadNumericTextBox>
                                                            <asp:HiddenField runat="server" ID="hdnQuantity" Value='<%# Bind("Quantity") %>' />
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridButtonColumn UniqueName="DeleteColumn" HeaderStyle-Width="20" ConfirmText="Are you sure you want to remove this Item from the Watch List?" ConfirmDialogType="RadWindow"
                                                        ConfirmTitle="Remove" ButtonType="ImageButton" ConfirmDialogHeight="140" CommandName="Delete"
                                                        Text="Delete">
                                                    </telerik:GridButtonColumn>

                                                </Columns>
                                            </MasterTableView>
                                            <ClientSettings>
                                                <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="465" SaveScrollPosition="True"></Scrolling>
                                            </ClientSettings>
                                        </telerik:RadGrid>
                                        <asp:Button ID="btnClearList" CssClass="buttonStyle" runat="server" Text="Clear List"
                                            Style="margin-right: 20px;" OnClientClick="showPopup('popupClearWatchListConfirmation');" />
                                        <sb:PopupBox ID="popupClearWatchListConfirmation" runat="server" Title="Clear Watch List" Height="100">
                                            <BodyContent>
                                                Are you sure you want to clear all items from this list?
                                            </BodyContent>
                                            <BottomStripeContent>
                                                <asp:Button ID="btnClearListConfirm" CssClass="buttonStyle"
                                                    runat="server" Text="Yes" OnClick="btnClearListConfirm_Click" />
                                                <asp:Button ID="btnCancelConfirmCreatePDF" CssClass="buttonStyle" runat="server"
                                                    Text="No" OnClientClick="hidePopup('popupClearWatchListConfirmation');" />
                                            </BottomStripeContent>
                                        </sb:PopupBox>

                                    </div>
                                    <sb:PopupBox ID="popupRemovedWatchListItemsNotification" runat="server" Title="Notification" ShowCornerCloseButton="false">
                                        <BodyContent>
                                            <div style="width: 400px;">
                                                <asp:Literal ID="ltrWatchListItemsRemovedCompany" runat="server" />
                                                no longer sharing their inventory. 
                                                Their Items have been removed from your Watch list.
                                            </div>

                                        </BodyContent>
                                        <BottomStripeContent>
                                            <asp:Button ID="btnRespondRemovedWatchListItemNotification" CssClass="buttonStyle"
                                                runat="server" Text="OK" OnClick="btnRespondRemovedWatchListItemNotification_Click" />
                                        </BottomStripeContent>
                                    </sb:PopupBox>
                                </div>
                                <div id="divItemList" runat="server">
                                    <table>
                                        <tr>
                                            <td>
                                                <div style="overflow-y: auto; height: 480px;">
                                                    <telerik:RadListView ID="lvItemList" runat="server" OnNeedDataSource="lvItemList_NeedDataSource" AllowPaging="true" AllowCustomPaging="true"
                                                        ItemPlaceholderID="ProductsHolder" OnItemDataBound="lvItemList_ItemDataBound" OnItemCommand="lvItemList_ItemCommand">
                                                        <EmptyDataTemplate>
                                                            <div class="noData">
                                                                No results found
                                                            </div>
                                                        </EmptyDataTemplate>
                                                        <LayoutTemplate>
                                                            <asp:Panel ID="ProductsHolder" runat="server" />
                                                        </LayoutTemplate>
                                                        <ItemTemplate>
                                                            <div class="thumbListItem" style="height: 200px; width: 120px;">
                                                                <table style="display: block;">
                                                                    <tr class="trThumbView">
                                                                        <td class="tdThumbView" style="height: 127px;">
                                                                            <asp:HyperLink ID="lnkItem" runat="server" CssClass="lnkItemDetails">
                                                                                <sb:ImageDisplay ID="itemThumbDisplay" ShowImagePreview="false" runat="server" />
                                                                            </asp:HyperLink>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            <span class="smallText grayText">
                                                                                <asp:LinkButton ID="lnkAddToWatchList" CommandName="Add to WatchList" CssClass="smallText"
                                                                                    ToolTip="Add this Item to WatchList" runat="server">Add to Watch List</asp:LinkButton>
                                                                                <asp:Literal ID="ltrlAddedToWatchList" runat="server">Added to Watch List</asp:Literal>
                                                                            </span>
                                                                            <br />
                                                                            <div class="selectItemClass" style="display: inline;">
                                                                                <asp:RadioButton runat="server" ID="rbtnItem" />
                                                                                <asp:HiddenField runat="server" ID="hdnItemId" Value='<%# Bind("Item.ItemId") %>' />
                                                                                <asp:Literal runat="server" ID="ltrItemName"></asp:Literal>
                                                                                <br />
                                                                                <asp:Literal ID="litAvailableQty" runat="server"></asp:Literal>
                                                                                <br />
                                                                                &nbsp;&nbsp;&nbsp;&nbsp;<telerik:RadNumericTextBox ID="txtQtyBooked" Display="false" DisplayText="1" ShowSpinButtons="true" Width="50" runat="server">
                                                                                    <ClientEvents OnValueChanged="BookedQtyChanged" />
                                                                                </telerik:RadNumericTextBox>
                                                                                <asp:HiddenField runat="server" ID="hdnQuantity" Value='<%# Bind("Item.Quantity") %>' />
                                                                            </div>
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </div>
                                                        </ItemTemplate>
                                                    </telerik:RadListView>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <telerik:RadDataPager ID="pagerInventory" runat="server" PagedControlID="lvItemList"
                                                    PageSize="20" Width="694" OnPreRender="pagerInventory_PreRender">
                                                    <Fields>
                                                        <telerik:RadDataPagerButtonField FieldType="FirstPrev" />
                                                        <telerik:RadDataPagerButtonField FieldType="Numeric" PageButtonCount="5" />
                                                        <telerik:RadDataPagerButtonField FieldType="NextLast" />
                                                        <telerik:RadDataPagerPageSizeField PageSizeComboWidth="60" PageSizeText="Page size: "
                                                            PageSizes="20,50,100" />
                                                        <telerik:RadDataPagerTemplatePageField>
                                                            <PagerTemplate>
                                                                <div style="text-align: right; color: #5a6779; padding-right: 10px;" id="listViewPagerCount">
                                                                    <asp:Label runat="server" ID="CurrentPageLabel" Text="<%# Container.Owner.TotalRowCount%>" />
                                                                    <asp:Label runat="server" ID="lblItemText" />
                                                                    <asp:Label runat="server" ID="TotalItemsLabel" Text="<%# Container.Owner.PageCount %>" />
                                                                    <asp:Label runat="server" ID="lblPagesText" />
                                                                </div>
                                                            </PagerTemplate>
                                                        </telerik:RadDataPagerTemplatePageField>
                                                    </Fields>
                                                </telerik:RadDataPager>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="left" style="height: 500px; width: 222px;">
                    <asp:UpdatePanel runat="server" ID="upnlRightPanel">
                        <ContentTemplate>
                            <div id="divInventoryBookingPanel" runat="server">
                                <sb:InventoryBookingPanel ID="inventoryProjectPanel" runat="server" />
                            </div>
                            <div id="divInventoryBulkUpdatePanel" runat="server" style="display: none;">
                                <sb:InventoryBulkUpdatePanel runat="server" ID="inventoryBulkUpdatePanel" />
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div style="clear: both">
                </div>
            </div>
        </BodyContent>
    </sb:GroupBox>
</asp:Content>
