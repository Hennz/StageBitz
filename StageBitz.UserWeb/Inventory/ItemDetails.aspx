<%@ Page DisplayTitle="Item Details" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="ItemDetails.aspx.cs" Inherits="StageBitz.UserWeb.Inventory.ItemDetails" %>

<%@ Register Src="~/Controls/Common/ExportData.ascx" TagName="ExportData" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Item/CompleteItem.ascx" TagName="CompleteItem" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Item/CompleteItemHeader.ascx" TagName="CompleteItemHeader"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/FileUpload.ascx" TagName="FileUpload" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Inventory/InventoryBookingPanel.ascx" TagName="InventoryBookingPanel"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>

<%@ Register Src="~/Controls/Item/ItemDeletedWarning.ascx" TagName="ItemDeletedWarning" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Inventory/InventorySharingRemovedWarning.ascx" TagName="InventorySharingRemovedWarning" TagPrefix="sb" %>

<%@ Register Src="~/Controls/Company/CompanyWarningDisplay.ascx" TagPrefix="sb" TagName="CompanyWarningDisplay" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <asp:PlaceHolder runat="server" ID="plhScripts">
        <script src="<%# ResolveUrl("../Common/Scripts/ComboSearchEvents.js?v="+ this.ApplicationVersionString) %>"></script>
        <link href="<%# ResolveUrl("../Common/Styles/ItemTypes.css?v="+ this.ApplicationVersionString) %>" rel="stylesheet" />
        <script src="<%# ResolveUrl("../Common/Scripts/ItemTypes/Base.js?v="+ this.ApplicationVersionString) %>"></script>
        <script src="<%# ResolveUrl("../Common/Scripts/ItemTypes/ItemBrief.js?v="+ this.ApplicationVersionString) %>"></script>
        <script src="<%# ResolveUrl("../Common/Scripts/ItemTypes/Item.js?v="+ this.ApplicationVersionString) %>"></script>
        <script src="<%# ResolveUrl("../Common/Scripts/Inventory.js?v="+ this.ApplicationVersionString) %>"></script>
        <script type="text/javascript">
            var _gaq = _gaq || [];
            _gaq.push(['_setCustomVar', 1, 'Category', 'Inventory', 2]);
            _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyId %>', 2]);
            _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
        </script>
        <script type="text/javascript">
            var itemObj;
            var bookedQty = 1;
            var itemValidationGroup = '<%=btnDone.ValidationGroup%>';
            var userId = '<%= this.UserID %>';
            var itemId = '<%= this.ItemId %>';
            var isReadOnly = '<%=this.IsReadOnly.ToString().ToLower()%>';
            var navigateUrl = null;

            $(document).ready(function () {
                var fromDate = '<%# InventoryParamFromDate.HasValue ? StageBitz.Common.Utils.FormatDatetime(InventoryParamFromDate.Value,false) : string.Empty%>';
                var toDate = '<%# InventoryParamToDate.HasValue ? StageBitz.Common.Utils.FormatDatetime(InventoryParamToDate.Value,false) : string.Empty %>';
                var hasDatefilteration = '<%# InventoryParamFromDate.HasValue && InventoryParamToDate.HasValue %>';
                DisplayHeaderText(hasDatefilteration, fromDate, toDate);
                itemObj = new StageBitz.UserWeb.Common.Scripts.ItemTypes.Item();
                itemObj.UserId = userId;
                itemObj.ItemId = itemId;
                itemObj.FieldsElement = $(".DynamicFields");
                itemObj.ItemHeaderElement = $("table[id$='itemBriefDetailsTable']").parent().parent();
                itemObj.ItemSpecElement = $('#completeItemSpecArea');
                itemObj.DisplayMode = StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.DisplayMode.ItemDetails;
                itemObj.StopProcessingBtn = $("#<%=btnCheckStopProcessing.ClientID %>");
                itemObj.RelatedTable = "Company";
                itemObj.HideOverlay = function () { HideOverlay(); }
                itemObj.ShowOverlay = function () { ShowOverlay(); }
                itemObj.BookQty = bookedQty;
                ShowOverlay();

                LoadBookingInfo(fromDate, toDate, hasDatefilteration);

                itemObj.LoadData(function () {
                    itemCompleteLoadCallBack(itemObj);
                });
            });

            function LoadBookingInfo(fromDate, toDate, hasDatefilteration) {
                itemObj.FromDate = fromDate ? fromDate : null;
                itemObj.ToDate = toDate ? toDate : null;
                itemObj.HasDatefilteration = hasDatefilteration;
            }

            $(document).on("onProjectPanelDateFilterationsChange", function () {
                UpdateNavigateURL();
            });

            $(document).on("onQuantityChanged", function (event, qtyBooked) {
                bookedQty = qtyBooked;
                UpdateNavigateURL();
            });

            function DisplayHeaderText(hasDatefilteration, fromDate, toDate) {
                $("#divHeaderLeft").html("Item Details: ".concat(hasDatefilteration == 'True' ? String.format("Showing availability from {0} to {1}", fromDate, toDate) : "Select a booking period to show availability"));
            }

            function UpdateNavigateURL() {
                navigateUrl = updateQueryStringParameter(navigateUrl, 'Booking', encodeURIComponent(GetBookingParam()));
                $('a.InventoryLink', 'div.breadCrumbs div[id$="upnlBreadCrumbs"]').attr('href', navigateUrl);
            }

            function GetBookingParam() {
                return <%= inventoryProjectPanel.ClientID%>GetSelectedBookingCode() + "|" + <%= inventoryProjectPanel.ClientID%>GetSelecteItemBriefTypeId() + "|" + <%= inventoryProjectPanel.ClientID%>GetSelecteFromDate() + "|" + <%= inventoryProjectPanel.ClientID%>GetSelecteToDate() + "|" + bookedQty;
            };

            var itemCompleteLoadCallBack = function (obj) {
                obj.PopulateProperties();
                obj.InitializeUI(obj.FieldsHtml, obj.FieldsElement, obj.ItemSpecElement, itemObj.ItemHeaderElement);
                SetDirtyValidationsForDynamicFields(obj.ItemSpecElement);
                HideOverlay();
            }


            function client_btnDone_Click() {
                if (Page_ClientValidate(itemValidationGroup)) {
                    if ((isReadOnly == "false" || itemObj.CanSaveQuantity()) && getGlobalDirty()) {
                        ShowOverlay();
                        itemObj.PopulateSaveData();
                        itemObj.CallSaveDataService(function () {
                            itemObj.LoadData(function () {
                                setGlobalDirty(false);
                                itemCompleteLoadCallBack(itemObj);
                                showNotification('itemDetailSavedNotice');
                            });
                        });
                    }
                }
            }

            function client_ItemDetails_btnConfirmSave_Click() {
                if (Page_IsValid) {
                    $(document).trigger('onSaveItemDetails', [function () {
                        $('#<%= btnConfirmSave.ClientID%>').click();
                }]);
            }
        }

        function client_ItemDetails_ConfirmItemTypeChange_Click(shouldSave) {
            if (shouldSave) {
                $(document).trigger('onSaveItemDetails', [function () {
                    hidePopup('popupConfirmItemTypeChange');
                    itemObj.LoadData(function () {
                        itemCompleteLoadCallBack(itemObj);
                    });
                }]);
            }
            else {
                itemObj.LoadData(function () {
                    itemCompleteLoadCallBack(itemObj);
                    setGlobalDirty(false);
                    hidePopup('popupConfirmItemTypeChange');
                });
            }
        }



        $(document).on('onSaveItemDetails', function (event, callback) {
            if (Page_ClientValidate(itemValidationGroup)) {
                ShowOverlay();
                itemObj.PopulateSaveData();
                itemObj.CallSaveDataService(function () {
                    setGlobalDirty(false);
                    HideOverlay();
                    if (callback) {
                        callback();
                    }
                });
            }
        });

        $(document).on('onSaveItemDetailsBeforeChangeItemType', function (event, element) {
            if (getGlobalDirty()) {
                showPopup('popupConfirmItemTypeChange');
            }
            else {
                itemObj.ReloadItemType();
            }
        });

        $(document).on('onLoadItemDetailsAfterPin', function () {
            reloadUIAfterSave();
        });

        $(document).on('onLoadItemDetailsForDateFilteration', function (event, fromDate, toDate, hasDatefilteration, availableQty) {
            LoadBookingInfo(fromDate, toDate, hasDatefilteration);
            itemObj.LoadBookingInfo(availableQty);
            DisplayHeaderText(hasDatefilteration, fromDate, toDate);
        });


        function reloadUIAfterSave() {
            itemObj.LoadData(function () {
                itemCompleteLoadCallBack(itemObj);
                setGlobalDirty(false);
                hidePopup("popupConcurrencyQtyUpdateError");
            });
        }

        function SetDirtyValidationsForDynamicFields(specElement) {
            $(':input:not([hidden],:submit,:password,:button)', specElement).bind('change.dirtyValidationCompletedItem', function () {
                $("input[id$='hdnIsDirty']", specElement).val("True");
            });
        }

        function ShowOverlay() {
            $('#itemtypeOverlay').show();
        }

        function HideOverlay() {
            $('#itemtypeOverlay').hide();
        }
        </script>
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    <asp:PlaceHolder ID="plcHeaderLinks" runat="server">| <a id="lnkCompanyInventory"
        runat="server">Company Inventory</a> |
        <asp:HyperLink ID="hyperLinkMyBooking" runat="server">My Bookings</asp:HyperLink>
        <span runat="server" id="spnInventorySharing">|<asp:HyperLink ID="hyperLinkInventorySharing" runat="server">Manage Inventory</asp:HyperLink></span>
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <div style="display: none;" id="itemtypeOverlay">
        <div class="updateProgressOverlay">
        </div>
        <div class="updateProgressIcon">
        </div>
    </div>
    <sb:CompanyWarningDisplay runat="server" ID="sbCompanyWarningDisplay" />
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>
    <telerik:RadWindowManager ID="mgr" runat="server">
    </telerik:RadWindowManager>
    <asp:PlaceHolder ID="plcItemNotAvailable" Visible="false" runat="server">The Item you've
        requested does not exist. </asp:PlaceHolder>
    <sb:GroupBox ID="grpItem" runat="server">
        <TitleLeftContent>
            <div id="divHeaderLeft" style="float: left;">
            </div>
            <div style="float: left; margin-left: 4px;">
                <sb:HelpTip ID="helpTipItemDetails" Visible="true" runat="server" Width="470">
                    <p>
                        If you want to book this Item you can add it to a list in the Project panel by clicking
                        the (+) button.
                    </p>
                    <br />
                    <p>
                        It will then appear on the Pinboard tab.
                    </p>
                </sb:HelpTip>
            </div>
        </TitleLeftContent>
        <BodyContent>
            <div style="width: 695px; margin-right: 2px;" class="left">
                <asp:Panel ID="Panel1" DefaultButton="btnEnterKeyButton" runat="server">
                    <!--This is to prevent anything from happening when the enter key is pressed on any textbox-->
                    <asp:Button ID="btnEnterKeyButton" OnClientClick="return false;" runat="server" Text="Button"
                        Style="display: none;" />
                    <div class="dirtyValidationArea" style="margin-bottom: 10px;">
                        <asp:UpdatePanel ID="upnlWatchList" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div id="divWatchList" class="right" style="margin-right: 20px;">
                                    <asp:LinkButton ID="lnkAddToWatchList" runat="server" OnClick="lnkAddToWatchList_Click">Add to Watch List</asp:LinkButton>
                                    <asp:Literal ID="ltrlAddedToWatchList" runat="server">Added to Watch List</asp:Literal>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <div class="left" style="padding: 10px 0px; width: 100%;">
                            <sb:CompleteItemHeader ID="completeItemHeader" runat="server" DisplayMode="ItemDetails" />
                            <br style="clear: both;" />
                        </div>
                    </div>
                    <div>
                        <asp:UpdatePanel ID="upTabs" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <telerik:RadTabStrip ID="itemDetailsTabs" Width="700" MultiPageID="itemPages" runat="server">
                                    <Tabs>
                                        <telerik:RadTab runat="server" Text="Details" Value="Details">
                                        </telerik:RadTab>
                                        <telerik:RadTab runat="server" Text="Booking(1)" Value="Booking">
                                        </telerik:RadTab>
                                    </Tabs>
                                </telerik:RadTabStrip>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <div class="tabPage" style="width: 650px; min-height: 310px;">
                            <telerik:RadMultiPage ID="itemPages" runat="server" Width="100%">
                                <telerik:RadPageView ID="DetailsTab" runat="server">
                                    <asp:UpdatePanel ID="upnlFileUpload" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <div style="padding-right: 10px;">
                                                <sb:FileUpload runat="server" ID="uploadItemMedia" Title="New Upload" OnFileUploaded="uploadItemMedia_OnFileUploaded" />
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <div id="completeItemSpecArea">
                                        <div class="dirtyValidationArea" style="padding-left: 10px;">
                                            <sb:CompleteItem runat="server" ID="completeItem" DisplayMode="ItemDetails" />
                                        </div>
                                    </div>
                                    <br style="clear: both;" />
                                </telerik:RadPageView>
                                <telerik:RadPageView ID="BookingTab" runat="server">
                                    <div>
                                        <div style="padding-right: 10px; width: 60%; margin: 12px 0px;" class="left">
                                            <asp:HyperLink runat="server" ID="lnkContactBookingManager"></asp:HyperLink>
                                            handles booking enquiries for this Item.
                                        </div>
                                        <asp:UpdatePanel ID="upnlReports" runat="server">
                                            <ContentTemplate>
                                                <div class="right" style="margin: 5px; width: 30%;" id="divExportData" runat="server">
                                                    <sb:ExportData ID="exportData" OnExcelExportClick="exportData_ExcelExportClick" OnPDFExportClick="exportData_PDFExportClick"
                                                        runat="server" />
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <br style="clear: both;" />
                                        <asp:UpdatePanel ID="upnlBooking" runat="server" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <telerik:RadGrid ID="gvBookingList" EnableLinqExpressions="False" AutoGenerateColumns="false"
                                                    runat="server" AllowSorting="true" OnSortCommand="gvBookingList_SortCommand" OnNeedDataSource="gvBookingList_NeedDataSource">
                                                    <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                                                    <MasterTableView>
                                                        <NoRecordsTemplate>
                                                            <div class="noData">
                                                                No items found.
                                                            </div>
                                                        </NoRecordsTemplate>
                                                        <SortExpressions>
                                                            <telerik:GridSortExpression FieldName="FromDate" SortOrder="Ascending" />
                                                        </SortExpressions>
                                                        <Columns>
                                                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Booking"
                                                                HeaderStyle-Width="75px" SortExpression="BookingName" UniqueName="BookingName">
                                                                <ItemTemplate>
                                                                    <asp:Label runat="server" ID="lblProject"
                                                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("BookingName"), 15) %>'
                                                                        ToolTip='<%# ((string)Eval("BookingName")).Length > 15 ? Eval("BookingName") : string.Empty %>'></asp:Label>
                                                                </ItemTemplate>
                                                            </telerik:GridTemplateColumn>
                                                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Item Brief" ItemStyle-Width="75px"
                                                                HeaderStyle-Width="75px" SortExpression="ItemBrief" UniqueName="ItemBrief">
                                                                <ItemTemplate>
                                                                    <asp:Label runat="server" ID="lblItemBrief"
                                                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("ItemBrief"), 15) %>'
                                                                        ToolTip='<%# ((string)Eval("ItemBrief")).Length > 15 ? Eval("ItemBrief") : string.Empty %>'></asp:Label>
                                                                </ItemTemplate>
                                                            </telerik:GridTemplateColumn>
                                                            <telerik:GridTemplateColumn ReadOnly="true" HeaderText="Booked By"
                                                                HeaderStyle-Width="75px" SortExpression="BookedBy" UniqueName="BookedBy">
                                                                <ItemTemplate>
                                                                    <asp:HyperLink runat="server" ID="lnkBookedBy"
                                                                        Text='<%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("BookedBy"), 15) %>'
                                                                        ToolTip='<%# ((string)Eval("BookedBy")).Length > 15 ? Eval("BookedBy") : string.Empty %>'
                                                                        NavigateUrl='<%# "mailto:" + Eval("BookedByEmail") %>'></asp:HyperLink>
                                                                </ItemTemplate>
                                                            </telerik:GridTemplateColumn>
                                                            <telerik:GridBoundColumn ReadOnly="true" HeaderText="Status" ItemStyle-Width="40px"
                                                                HeaderStyle-Width="40px" DataField="Status" SortExpression="StatusSortOrder">
                                                            </telerik:GridBoundColumn>
                                                            <telerik:GridDateTimeColumn ReadOnly="true" HeaderText="From Date" ItemStyle-Width="55px"
                                                                HeaderStyle-Width="40px" DataField="FromDate" DataFormatString="{0:dd MMM yyyy}" SortExpression="FromDate">
                                                            </telerik:GridDateTimeColumn>
                                                            <telerik:GridDateTimeColumn ReadOnly="true" HeaderText="To Date" ItemStyle-Width="55px"
                                                                HeaderStyle-Width="40px" DataField="ToDate" DataFormatString="{0:dd MMM yyyy}" SortExpression="ToDate">
                                                            </telerik:GridDateTimeColumn>
                                                            <telerik:GridBoundColumn ReadOnly="true" HeaderText="Book Qty" ItemStyle-HorizontalAlign="Right" ItemStyle-Width="50px"
                                                                HeaderStyle-Width="40px" DataField="BookedQuantity" SortExpression="BookedQuantity">
                                                            </telerik:GridBoundColumn>
                                                        </Columns>
                                                    </MasterTableView>
                                                    <ClientSettings>
                                                        <Scrolling AllowScroll="True" ScrollHeight="250" SaveScrollPosition="True"></Scrolling>
                                                    </ClientSettings>
                                                </telerik:RadGrid>
                                                <div runat="server" id="divEmptyBookings">
                                                    <div class="lightNotice noData" style="padding-top: 50px; height: 230px;">
                                                        <p>
                                                            You're in luck...
                                                        </p>
                                                        <p>
                                                            This Item has no bookings.
                                                        </p>
                                                    </div>
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </div>
                                </telerik:RadPageView>
                            </telerik:RadMultiPage>
                        </div>
                    </div>
                </asp:Panel>
            </div>
            <div class="left" style="width: 222px;">
                <sb:InventoryBookingPanel ID="inventoryProjectPanel" runat="server" />
            </div>
            <div style="clear: both">
            </div>
        </BodyContent>
    </sb:GroupBox>
    <asp:UpdatePanel runat="server" UpdateMode="Always" ID="upnlBottom">
        <ContentTemplate>
            <sb:ItemDeletedWarning runat="server" ID="popupItemDeletedWarning" IsDefault="true" />
            <sb:InventorySharingRemovedWarning runat="server" ID="popupInventorySharingRemovedWarning" />
            <div style="margin-top: 10px; padding-right: 10px;">
                <asp:Button ID="btnDone" CssClass="ignoreDirtyFlag buttonStyle" OnClientClick="client_btnDone_Click(); return false;"
                    ValidationGroup="ItemFields" runat="server" Text="Save" />
                <asp:Button ID="btnDeleteItem" CssClass="buttonStyle" runat="server" Text="Delete Item" OnClick="btnDeleteItem_Click" />
                <asp:Button ID="btnCheckStopProcessing" runat="server" Style="display: none;" />
                <div id="itemDetailSavedNotice" class="inlineNotification right">
                    Changes saved.
                </div>
            </div>
            <sb:PopupBox ID="popupConfirmDeleteItem" Title="Delete Item" runat="server">
                <BodyContent>
                    <div style="width: 300px;">
                        Please confirm you would like to delete this Item permanently from the Inventory.
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnConfirmDelteItem" runat="server" CssClass="buttonStyle ignoreDirtyFlag"
                        OnClick="btnConfirmDelteItem_Click" Text="Confirm" />
                    <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                </BottomStripeContent>
            </sb:PopupBox>

            <sb:PopupBox ID="popUpConfirmationDeleteFutureBookings" Title="Delete Item" runat="server">
                <BodyContent>
                    <div style="width: 300px;">
                        This Item has at least one future booking. Please confirm you would like to delete it permanently from the Inventory. Users with future bookings will be notified.
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="Button1" runat="server" CssClass="buttonStyle ignoreDirtyFlag"
                        OnClick="btnConfirmationDeleteFutureBookings_Click" Text="Confirm" />
                    <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                </BottomStripeContent>
            </sb:PopupBox>

            <sb:PopupBox ID="popupItemIsPinned" Title="It's all about the timing" runat="server">
                <BodyContent>
                    <div style="width: 400px;">
                        This Item has just been added to
                        <asp:Literal ID="ltrProjectName" runat="server" />
                        by 
                <asp:Literal ID="ltrUserName" runat="server" />
                        so cannot be deleted just now.
                <br />
                        <br />
                        You can contact <a runat="server" id="lnkItemPinnedUserEmail"></a>
                        to let them know if it is no longer available.
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnDoneItemIsPinnedPopup" runat="server" CssClass="buttonStyle ignoreDirtyFlag"
                        OnClick="btnDoneItemIsPinnedPopup_Click" Text="Done" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupConfirmItemDetailSave" runat="server" Title="Save unsaved changes" ShowCornerCloseButton="false">
                <BodyContent>
                    <div style="width: 300px;">
                        You have unsaved changes in this page. Do you want to save them and proceed?<br />
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnCancelSave" CssClass="buttonStyle" runat="server"
                        Text="No" OnClick="btnCancelSave_Click" />
                    <input type="button" value="Yes" onclick="client_ItemDetails_btnConfirmSave_Click(); return false;" class="buttonStyle" />
                    <asp:Button ID="btnConfirmSave" CssClass="buttonStyle" runat="server"
                        Text="Yes" OnClick="btnConfirmSave_Click" Style="display: none;" />
                </BottomStripeContent>
            </sb:PopupBox>

            <sb:PopupBox ID="popupConfirmItemTypeChange" runat="server" Title="You're changing the Item Type" ShowCornerCloseButton="false">
                <BodyContent>
                    <div style="width: 300px;">
                        Any other changes you've made to the Item Details will also be saved now.<br />
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <input type="button" value="Cancel" onclick="client_ItemDetails_ConfirmItemTypeChange_Click(false); return false;" class="buttonStyle" />
                    <input type="button" value="Save Changes" onclick="client_ItemDetails_ConfirmItemTypeChange_Click(true); return false;" class="buttonStyle" />
                </BottomStripeContent>
            </sb:PopupBox>

            <sb:ErrorPopupBox ID="popupConcurrencyQtyUpdateError" runat="server" Title="Error" ShowCornerCloseButton="false" ErrorCode="QuantityUpdateFailed">
                <BodyContent>
                    <div style="width: 500px;">
                        There is a current booking for this Item.  Please check the Bookings tab and then try reducing the quantity once the booking has been resolved.
                        <br />
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnReloadConcurrencyBookingOverlap" CssClass="buttonStyle" runat="server" Text="Ok" OnClientClick="location.reload();" />
                </BottomStripeContent>
            </sb:ErrorPopupBox>

            <sb:ErrorPopupBox ID="popupInventoryLocationDeleted" runat="server" Title="Error" ShowCornerCloseButton="false" ErrorCode="InventoryLocationDeleted">
                <BodyContent>
                    <div style="width: 300px;">
                        Selected Inventory Location has already been deleted.
                        <br />
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <input type="button" value="Ok" onclick="location.reload();" class="buttonStyle" />
                </BottomStripeContent>
            </sb:ErrorPopupBox>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
