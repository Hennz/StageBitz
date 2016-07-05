<%@ Page DisplayTitle="Item Brief" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="ItemBriefDetails.aspx.cs" Inherits="StageBitz.UserWeb.ItemBrief.ItemBriefDetails" %>

<%@ Register Src="~/Controls/Common/FileUpload.ascx" TagName="FileUpload" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/DocumentList.ascx" TagName="DocumentList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/DocumentPreview.ascx" TagName="DocumentPreview"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ItemBriefTasks.ascx" TagName="ItemBriefTasks"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/CompletedItem.ascx" TagName="CompletedItem"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/ItemAttachments.ascx" TagName="Attachments"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/PinnedItems.ascx" TagName="PinnedItems" TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="<%# ResolveUrl("../Common/Scripts/jquery-te-1.4.0.min.js?v="+ this.ApplicationVersionString) %>"></script>
    <link href="<%# ResolveUrl("../Common/Styles/jqueryte/jquery-te-1.4.0.css?v="+ this.ApplicationVersionString) %>" rel="stylesheet" />
    <script src="<%# ResolveUrl("../Common/Scripts/ComboSearchEvents.js?v="+ this.ApplicationVersionString) %>"></script>
    <script src="<%# ResolveUrl("../Common/Scripts/Inventory.js?v="+ this.ApplicationVersionString) %>"></script>
    <link href="<%# ResolveUrl("../Common/Styles/ItemTypes.css?v="+ this.ApplicationVersionString) %>" rel="stylesheet" />
    <script src="<%# ResolveUrl("../Common/Scripts/ItemTypes/Base.js?v="+ this.ApplicationVersionString) %>"></script>
    <script src="<%# ResolveUrl("../Common/Scripts/ItemTypes/ItemBrief.js?v="+ this.ApplicationVersionString) %>"></script>
    <script src="<%# ResolveUrl("../Common/Scripts/ItemTypes/Item.js?v="+ this.ApplicationVersionString) %>"></script>

    <script type="text/javascript">
        if ('<%= this.CompanyId %>' != '0' && '<%= this.ProjectId %>' != '0') {
            var _gaq = _gaq || [];
            _gaq.push(['_setCustomVar', 1, 'Category', 'Project', 2]);
            _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyId %>', 2]);
            _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
            _gaq.push(['_setCustomVar', 4, 'ProjectId', '<%= this.ProjectId %>', 2]);
        }
    </script>
    <script type="text/javascript">
        var itemBriefObj;
        var completeItemTabObj;
        var completeItemPopupObj;
        var culture = '<%=this.Currency%>';
        var userId = '<%= this.UserID %>';
        var itemBriefId = '<%= this.ItemBriefId %>';

        var loadCallBack = function () {
            
            if (typeof RemainingExpenses != 'undefined' && typeof ExpendedAmount != 'undefined')
                itemBriefObj.InitializeBudgetUI(RemainingExpenses, ExpendedAmount);
            itemBriefObj.InitializeUI(itemBriefObj.FieldsHtml, itemBriefObj.FieldsElement, itemBriefObj.ItemBriefSpecElement, itemBriefObj.ItemBriefHeaderElement);

            if (itemBriefObj.ItemDetails) {
                completeItemTabObj.ItemDetails = itemBriefObj.ItemDetails;
                itemCompleteLoadCallBack(completeItemTabObj);
            }

            HideOverlay();
        };

        function ShowOverlay() {
            $('#itemtypeOverlay').show();
        }

        function HideOverlay() {
            $('#itemtypeOverlay').hide();
        }

        var itemCompleteLoadCallBack = function (obj) {
            obj.PopulateProperties();
            obj.IsReadOnly = itemBriefObj.IsReadOnly || !obj.ItemDetails.CanEditInItemBrief;
            obj.InitializeUI(obj.FieldsHtml, obj.FieldsElement, obj.ItemSpecElement, obj.ItemHeaderElement);
            $("input[id$='hdnIsDirty']", obj.ItemSpecElement).val("False");
            $("input[id$='hdnIsDirty']", obj.ItemHeaderElement).val("False");
            HideOverlay();
        }

        $(document).ready(function () {
            itemBriefObj = new StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief();
            itemBriefObj.UserId = userId;
            itemBriefObj.ItemBriefId = itemBriefId;
            itemBriefObj.FieldsElement = $("#divDynamicFields");
            itemBriefObj.ItemBriefHeaderElement = $("#itemBriefDetailsTable").parent();
            itemBriefObj.ItemBriefSpecElement = $('div[id$="SpecificationTab"]');
            itemBriefObj.BudgetElement = $("div[id$='divBudget']");
            itemBriefObj.Culture = culture;
            itemBriefObj.StopProcessingBtn = $("#<%=btnCheckStopProcessing.ClientID %>");
            itemBriefObj.HideOverlay = function () { HideOverlay(); }
            itemBriefObj.ShowOverlay = function () { ShowOverlay(); }

            var itemCompleteTab = $("#itemCompleteTabDiv");
            completeItemTabObj = new StageBitz.UserWeb.Common.Scripts.ItemTypes.Item();
            completeItemTabObj.UserId = userId;
            completeItemTabObj.FieldsElement = $(".DynamicFields", itemCompleteTab);
            completeItemTabObj.ItemHeaderElement = $("table[id$='itemBriefDetailsTable']", itemCompleteTab).parent().parent();
            completeItemTabObj.ItemSpecElement = $('div[id$="pnlCompletedItem"]', itemCompleteTab);
            completeItemTabObj.Culture = culture;
            completeItemTabObj.DisplayMode = StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.DisplayMode.ItemBriefDetails;
            completeItemTabObj.StopProcessingBtn = $("#<%=btnCheckStopProcessing.ClientID %>");
            completeItemTabObj.HideOverlay = function () { HideOverlay(); }
            completeItemTabObj.ShowOverlay = function () { ShowOverlay(); }
            completeItemTabObj.RelatedTable = "Project";
            completeItemTabObj.ItemBriefId = itemBriefId;

            itemBriefObj.CompleteItemTabObj = completeItemTabObj;

            ShowOverlay();
            itemBriefObj.LoadData(loadCallBack);
        });

        function SaveItemBriefDetailsPage() {
            if (IsItemBriefPageValied() && !itemBriefObj.IsReadOnly) {
                if (completeItemTabObj.ItemDetails) {
                    completeItemTabObj.PopulateSaveData();
                    itemBriefObj.ItemDetailsToBeSaved = completeItemTabObj.ItemDetailsToBeSaved;
                }

                ShowOverlay();
                itemBriefObj.SaveData(function () {
                    itemBriefObj.LoadData(loadCallBack);
                    setGlobalDirty(false);
                    setItemTabDirty(false);
                    showNotification('itemBriefSavedNotice');
                });
            }
        }

        function OnTelerikControlValueChanged(sender, eventArgs) {
            setGlobalDirty(true);
        }

        function DisplayItemBriefStatus(codeId, codeValue, codeDescription) {
            itemBriefObj.ItemBrief.StatusCodeDescription = codeDescription;
            itemBriefObj.ItemBrief.StatusCodeValue = codeValue;
            itemBriefObj.SetItemBriefStatus();
        }

        function InitializeCompleteItemPopup() {
            var itemCompletePopup = $("#itemCompletePopupDiv");
            completeItemPopupObj = new StageBitz.UserWeb.Common.Scripts.ItemTypes.Item();
            completeItemPopupObj.UserId = userId;
            completeItemPopupObj.ItemBriefId = itemBriefId;
            completeItemPopupObj.FieldsElement = $(".DynamicFields", itemCompletePopup);
            completeItemPopupObj.ItemHeaderElement = $("table[id$='itemBriefDetailsTable']", itemCompletePopup).parent().parent();
            completeItemPopupObj.ItemSpecElement = $('div[id$="pnlCompletedItem"]', itemCompletePopup);
            completeItemPopupObj.Culture = culture;
            completeItemPopupObj.DisplayMode = StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.DisplayMode.ItemBriefDetails;
            completeItemPopupObj.StopProcessingBtn = $("#<%=btnCheckStopProcessing.ClientID %>");
            completeItemPopupObj.HideOverlay = function () { HideOverlay(); }
            completeItemPopupObj.ShowOverlay = function () { ShowOverlay(); }
            completeItemPopupObj.RelatedTable = "Project";

            ShowOverlay();
            completeItemPopupObj.LoadData(function () {
                itemCompleteLoadCallBack(completeItemPopupObj);
            });
        }
        // confirm item complete events
        $(document).on("onCompleteItemPopupConfirmed", function (event, popup) {
            if (IsCompleteItemPopupValied()) {
                ShowOverlay();
                completeItemPopupObj.PopulateSaveData();
                completeItemPopupObj.ItemDetailsToBeSaved.DocumentMediaIds = $('input[id$="hdnDocumentIds"]', completeItemPopupObj.ItemSpecElement).val();
                completeItemPopupObj.ItemDetailsToBeSaved.DefaultImageId = $('input[id$="hdnDefaultImageId"]', completeItemPopupObj.ItemHeaderElement).val();
                completeItemPopupObj.CompleteItem(function () {
                    hidePopup(popup);
                    showHideCompleteItemTab(true);
                    itemBriefObj.LoadData(loadCallBack);
                    setItemTabDirty(false);

                    // Refresh the pinboard tab and complete item tab.
                    $('input[id$="btnHiddenUpdateTabs"]').click();
                });
            }
        });

        $(document).on('onConfirmSaveItemBriefBeforeShowCompleteItemPopup', function (event, popup, completeItemPopup) {
            if (IsItemBriefPageValied()) {
                ShowOverlay();
                itemBriefObj.SaveData(function () {
                    itemBriefObj.LoadData(function () {
                        loadCallBack();
                        InitializeCompleteItemPopup();
                        setGlobalDirty(false);
                        hidePopup(popup);
                        showPopup(completeItemPopup);
                    });
                });
            }
        });

        $(document).on('onCancelConfirmSaveItemBriefBeforeShowCompleteItemPopup', function (event, popup, completeItemPopup) {
            ShowOverlay();
            itemBriefObj.LoadData(function () {
                loadCallBack();
                InitializeCompleteItemPopup();
                hidePopup(popup);
                showPopup(completeItemPopup);
            });
        });

        $(document).on('onConfirmSaveItemTabBeforeShowCompleteItemPopup', function (event, popup, completeItemPopup) {
            if (IsItemBriefPageValied()) {
                ShowOverlay();
                completeItemTabObj.PopulateSaveData();
                completeItemTabObj.CallSaveDataService(function () {
                    InitializeCompleteItemPopup();
                    hidePopup(popup);
                    setItemTabDirty(false);
                    showPopup(completeItemPopup);
                });
            }
        });

        $(document).on('onCancelSaveItemTabBeforeShowCompleteItemPopup', function (event, popup, completeItemPopup) {
            InitializeCompleteItemPopup();
            hidePopup(popup);
            showPopup(completeItemPopup);
        });

        // Item Pin tab events
        $(document).on('onClientInformItemBriefDetailToSaveCompleteItem', function () {
            if (IsItemBriefPageValied()) {
                ShowOverlay();
                completeItemTabObj.PopulateSaveData();
                completeItemTabObj.CallSaveDataService(function () {
                    completeItemTabObj.LoadData(function () {
                        itemCompleteLoadCallBack(completeItemTabObj);
                        setItemTabDirty(false);
                    });
                });
            }
        });

        $(document).on('onClientInformItemBriefDetailToReloadCompleteItemTab', function (event, showEmptyText, isReleaseItem) {
            if (showEmptyText) {
                showHideCompleteItemTab(false);
                setItemTabDirty(false);
                completeItemTabObj.ItemId = 0;
                completeItemTabObj.ItemBriefId = itemBriefId;
            }
            else {
                if (isReleaseItem) {
                    completeItemTabObj.ItemId = 0;
                }

                showHideCompleteItemTab(true);
                ShowOverlay();
                completeItemTabObj.LoadData(function () {
                    itemCompleteLoadCallBack(completeItemTabObj);
                });
            }
        });


        function QuantityChanged(sender, eventArgs) {
            var budget = $find("<%= txtBudget.ClientID %>").get_value();
            setGlobalDirty(true);
            itemBriefObj.ChangeBudgetUI(budget, RemainingExpenses, ExpendedAmount);
        }

        //function OnRadDatePickerError(sender, args) {
        //    setGlobalDirty(true);
        //}

        // override getGlobalDirty method in the golbal.js
        getGlobalDirty = function () {
            var isDirty = __isGlobalDirty;
            var hiddens = $("input[id$='hdnIsDirty']", "#itemCompleteTabDiv");
            $.each(hiddens, function () {
                var tempDirty = $(this).val() == "True";
                isDirty = isDirty || tempDirty;
            });

            return isDirty;
        }

        function ConfirmSavePDFCreate() {
            if (IsItemBriefPageValied()) {
                ShowOverlay();
                itemBriefObj.SaveData(function () {
                    itemBriefObj.LoadData(loadCallBack);
                    hidePopup('popupConfirmCreatePDF');
                    setGlobalDirty(false);
                    $('input[id$="btnCreatePDFHidden"]').click();
                });
            }
        }

        function setItemTabDirty(isDirty) {
            var hiddens = $("input[id$='hdnIsDirty']", "#itemCompleteTabDiv");
            $.each(hiddens, function () {
                if (isDirty) {
                    $(this).val("True");
                }
                else {
                    $(this).val("False");
                }
            });
        }

        function btnRemoveCompleteItem_Click() {
            $find("<%=itemBriefPages.ClientID %>").set_selectedIndex(4);
            $find("<%=itemBriefTabs.ClientID%>").findTabByText("Complete Item").select();
            hidePopup('<%=popupItemBriefRemoveConfirmation.ClientID%>');
        };

        function showHideCompleteItemTab(show) {
            if (show) {
                $('div[id$="divBlankNotice"]').hide();
                $('div[id$="pnlCompletedItemTab"]').show();
            }
            else {
                $('div[id$="divBlankNotice"]').show();
                $('div[id$="pnlCompletedItemTab"]').hide();
            }
        }

        function client_btnConfirmCompleteItemTab_Click() {
            if (IsItemBriefPageValied()) {
                ShowOverlay();
                completeItemTabObj.PopulateSaveData();
                completeItemTabObj.CallSaveDataService(function () {
                    completeItemTabObj.LoadData(function () {
                        itemCompleteLoadCallBack(completeItemTabObj);
                        hidePopup('popCompleteItemTabDirty');
                        setItemTabDirty(false);
                        $('input[id$="btnConfirmCompleteItemTabHidden"]').click();
                    });
                });
            }
        }

        function client_btnCreatePDF_Click() {
            if (IsItemBriefPageValied()) {
                if (getGlobalDirty()) {
                    showPopup('popupConfirmCreatePDF');
                }
                else {
                    if (itemBriefObj.IsReadOnly) {
                        $('input[id$="btnCreatePDFHiddenNoDirtyCheck"]').click();
                    }
                    else {
                        $('input[id$="btnCreatePDFHidden"]').click();
                    }
                }
            }
        }

        function client_btnCancelCreatePDF_Click() {
            if (IsItemBriefPageValied()) {
                hidePopup('popupConfirmCreatePDF');
                $('input[id$="btnCreatePDFHiddenNoDirtyCheck"]').click();
            }
        }

        function IsItemBriefPageValied() {
            var itemBriefValidationGroup = '<%=btnSave.ValidationGroup%>';
            return Page_ClientValidate(itemBriefValidationGroup);
        }

        function IsCompleteItemPopupValied() {
            var itemValidationGroup = '<%=btnSave.ValidationGroup%>' + 'Popup';
            return Page_ClientValidate(itemValidationGroup);
        }

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    <asp:PlaceHolder ID="plcHeaderLinks" runat="server">| <a id="lnkCompanyInventory"
        runat="server">Company Inventory</a> | <a id="lnkBookings" runat="server">Bookings</a> |<asp:HyperLink ID="hyperLinkTaskManager" runat="server">Task Manager</asp:HyperLink>
        |<sb:ProjectUpdatesLink ID="projectUpdatesLink" runat="server" />
        <sb:ReportList ID="reportList" runat="server" />
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <div style="display: none;" id="itemtypeOverlay">
        <div class="updateProgressOverlay">
        </div>
        <div class="updateProgressIcon">
        </div>
    </div>
    <telerik:RadWindowManager ID="mgr" runat="server">
    </telerik:RadWindowManager>
    <sb:ProjectWarningDisplay ID="warningDisplay" runat="server" />
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>
    <asp:PlaceHolder ID="plcItemBriefNotAvailable" Visible="false" runat="server">The Item
        Brief you've requested does not exist in this Project. </asp:PlaceHolder>
    <asp:PlaceHolder ID="plcNavigationButtons" runat="server">
        <div style="float: right; position: relative; top: 10px;">
            <table>
                <tr>
                    <td style="width: 40px" valign="top">
                        <div>
                            <asp:HyperLink ImageUrl="~/Common/Images/left_Slide.png" ID="lnkPrevious" ToolTip="Previous"
                                runat="server"></asp:HyperLink>
                            <asp:ImageButton ID="imgbtnPreviousDisabled" Enabled="false" Visible="false" ImageUrl="~/Common/Images/left_Slide_Disabled.png"
                                Style="cursor: default;" runat="server" />
                        </div>
                    </td>
                    <td>&nbsp;&nbsp;
                    </td>
                    <td style="width: 40px" valign="top">
                        <div>
                            <asp:HyperLink ImageUrl="~/Common/Images/right_Slide.png" ID="lnkNext" ToolTip="Next"
                                runat="server"></asp:HyperLink>
                            <asp:ImageButton ID="imgbtnNextDisabled" Visible="false" Enabled="false" ImageUrl="~/Common/Images/right_Slide_Disabled.png"
                                Style="cursor: default;" runat="server" />
                        </div>
                    </td>
                </tr>
            </table>
        </div>
    </asp:PlaceHolder>
    <asp:Panel ID="pnlItemBrief" DefaultButton="btnEnterKeyButton" runat="server">
        <!--This is to prevent anything from happening when the enter key is pressed on any textbox-->
        <asp:Button ID="btnEnterKeyButton" OnClientClick="return false;" runat="server" Text="Button"
            Style="display: none;" />
        <sb:DocumentPreview ID="documentPreview" OnDocumentAttributesChanged="documentPreview_DocumentChanged"
            OnDocumentDeleted="documentPreview_DocumentChanged" runat="server" FunctionPrefix="ItemDetails" />
        <div class="dirtyValidationArea" style="margin-bottom: 10px;">
            <div class="left" style="padding: 10px 0px;">
                <div class="left" style="width: 140px; padding-top: 5px;">
                    <asp:UpdatePanel ID="upnlItemBriefThumb" UpdateMode="Conditional" runat="server">
                        <ContentTemplate>
                            <table>
                                <tr>
                                    <td style="text-align: center;">
                                        <sb:ImageDisplay ID="thumbItemBrief" ShowImagePreview="true" runat="server" FunctionPrefix="ItemDetails" />
                                    </td>
                                </tr>
                                <tr id="trChangePreviewImage" runat="server">
                                    <td style="text-align: center;">
                                        <asp:UpdatePanel ID="upnlImagePicker" UpdateMode="Conditional" runat="server">
                                            <ContentTemplate>
                                                <asp:LinkButton ID="lnkbtnChangePreviewImage" OnClick="lnkbtnChangePreviewImage_Click"
                                                    runat="server" CssClass="smallText">Change Image</asp:LinkButton>
                                                <sb:PopupBox ID="popupImagePicker" Title="Select a preview image" runat="server">
                                                    <BodyContent>
                                                        <div class="boxBorder" style="overflow-x: scroll; max-width: 500px; min-width: 350px; padding: 5px 0px; margin-bottom: 5px; white-space: nowrap;">
                                                            <sb:DocumentList ID="imagePickerDocumentList" OnDocumentPicked="imagePickerDocumentList_DocumentPicked"
                                                                AllowPickingDocuments="true" ShowImagesOnly="true" runat="server" />
                                                        </div>
                                                    </BodyContent>
                                                    <BottomStripeContent>
                                                        <span class="grayText">
                                                            <asp:Literal ID="litSetImageInfo" runat="server"></asp:Literal>
                                                        </span>
                                                    </BottomStripeContent>
                                                </sb:PopupBox>
                                            </ContentTemplate>
                                            <Triggers>
                                                <asp:AsyncPostBackTrigger ControlID="lnkbtnChangePreviewImage" EventName="Click" />
                                            </Triggers>
                                        </asp:UpdatePanel>
                                    </td>
                                </tr>
                            </table>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="left" style="width: 410px;">
                    <sb:FocusEditBox ID="itemBriefNameEdit" Width="400" DisplayMaxLength="50" runat="server">
                        <TextBox MaxLength="100" CssClass="largeText"></TextBox>
                        <DisplayLabel CssClass="focusEditField largeText"></DisplayLabel>
                    </sb:FocusEditBox>
                    <asp:UpdatePanel ID="upnlCustomValidator" UpdateMode="Conditional" runat="server">
                        <ContentTemplate>
                            <div style="margin-top: 5px;">
                                <asp:RequiredFieldValidator ID="reqName" runat="server" ValidationGroup="ItemBriefFields"
                                    ErrorMessage="Name is required."></asp:RequiredFieldValidator>
                                <asp:CustomValidator ID="cusvalName" runat="server" OnServerValidate="cusvalName_ServerValidate"
                                    ValidationGroup="ItemBriefFields" ErrorMessage="Name already exists in this Project."></asp:CustomValidator>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <table id="itemBriefDetailsTable" style="width: 100%;">
                        <tr>
                            <td style="width: 70px;">Item Type
                            </td>
                            <td style="width: 5px;">:
                            </td>
                            <td>
                                <div runat="server" id="divItemTypeSelect" visible="false">
                                    <sb:DropDownListOPTGroup Width="150" ID="ddItemTypes" runat="server">
                                    </sb:DropDownListOPTGroup>
                                </div>
                                <div runat="server" id="divItemTypeStatic" visible="false">
                                    <asp:Label ID="lblItemType" runat="server"></asp:Label>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 70px;">ID
                            </td>
                            <td style="width: 5px;">:
                            </td>
                            <td>
                                <asp:Label ID="lblItemBriefId" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Quantity
                            </td>
                            <td>:
                            </td>
                            <td>
                                <telerik:RadNumericTextBox ID="txtItemQuantity" CssClass="focusEditField dirtyValidationExclude"
                                    Width="60" runat="server">
                                    <EnabledStyle PaddingTop="0" PaddingBottom="0" PaddingLeft="0" PaddingRight="0" />
                                    <ClientEvents OnValueChanged="OnTelerikControlValueChanged" />
                                </telerik:RadNumericTextBox>
                                <asp:RangeValidator ID="rngItemQuantity" runat="server" ControlToValidate="txtItemQuantity"
                                    Style="top: 0px;" MinimumValue="1" MaximumValue="99999999999" ValidationGroup="ItemBriefFields"
                                    ErrorMessage="Quantity cannot be zero."></asp:RangeValidator>
                            </td>
                        </tr>
                        <tr>
                            <td>Status
                            </td>
                            <td>:
                            </td>
                            <td>
                                <asp:Label ID="lblItemStatus" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Due Date
                            </td>
                            <td>:
                            </td>
                            <td>
                                <input runat="server" id="dtpkDueDate" type="text" class="datePicker" style="width: 150px;" readonly />
                                <%--<telerik:RadDatePicker ID="dtpkDueDate" CssClass="dirtyValidationExclude" runat="server">
                                            <ClientEvents OnDateSelected="OnTelerikControlValueChanged" />
                                            <DateInput ID="dtInputDueDate" runat="server">
                                                <ClientEvents OnError="OnRadDatePickerError" />
                                            </DateInput>
                                        </telerik:RadDatePicker>--%>
                            </td>
                        </tr>
                    </table>
                </div>
                <br style="clear: both;" />
            </div>
            <div class="right boxBorder" style="width: 350px; margin-right: 5px; margin-top: 10px;" runat="server" id="divBudget">
                <asp:UpdatePanel ID="uppnlBudget" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <div class="gridRow">
                            <div class="left" style="width: 40%">
                                <strong style="font-weight: bold;">Budget:</strong>
                            </div>
                            <div class="right" style="width: 60%; text-align: right;">
                                <telerik:RadNumericTextBox ID="txtBudget" SkinID="Currency" CssClass="focusEditField largeText dirtyValidationExclude"
                                    Style="margin-top: -2px;" Width="110" runat="server">
                                    <EnabledStyle PaddingTop="0" PaddingBottom="1" PaddingRight="0" PaddingLeft="0" />
                                    <%--<ClientEvents OnValueChanged="QuantityChanged" />--%>
                                    <ClientEvents OnBlur="QuantityChanged" />
                                </telerik:RadNumericTextBox>
                            </div>
                        </div>
                        <div class="gridAltRow">
                            <div class="left" style="width: 50%">
                                <strong>Expended:</strong>
                            </div>
                            <div id="divExpendedAmount" runat="server" class="right" style="width: 50%; text-align: right;">
                            </div>
                        </div>
                        <div class="gridRow">
                            <div class="left" style="width: 50%">
                                <strong>Remaining Expenses:</strong>
                            </div>
                            <div id="divRemainingExpenses" runat="server" class="right" style="width: 50%; text-align: right;">
                                <img runat="server" id="imgNoEstimatedCost" class="WarningIconForFinance" title="Please check you've entered an estimated cost for each task." visible="false" src="~/Common/Images/NoExpendedCostWarning.png" />
                                <asp:Literal ID="litRemainingExpenses" runat="server"></asp:Literal>
                            </div>

                        </div>
                        <div class="gridAltRow">
                            <div class="left" style="width: 50%">
                                <strong>BALANCE:</strong>
                            </div>
                            <strong>
                                <div id="divBalanceAmount" runat="server" class="right" style="width: 50%; text-align: right;">
                                    <%--<asp:Literal ID="ltrlBalanceAmount" runat="server"></asp:Literal>--%>
                                </div>
                            </strong>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <br style="clear: both" />
        </div>
        <div>
            <asp:UpdatePanel ID="upTabs" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <telerik:RadTabStrip ID="itemBriefTabs" Width="920" MultiPageID="itemBriefPages"
                        runat="server">
                        <Tabs>
                            <telerik:RadTab runat="server" Text="Specifications" Value="Specifications">
                            </telerik:RadTab>
                            <telerik:RadTab runat="server" Text="Tasks" Value="Tasks">
                            </telerik:RadTab>
                            <telerik:RadTab runat="server" Text="Attachments" Value="Attachments">
                            </telerik:RadTab>
                            <telerik:RadTab runat="server" Text="Pinboard" Value="PinBoard">
                            </telerik:RadTab>
                            <telerik:RadTab runat="server" Text="Complete Item" Value="CompletedItem">
                            </telerik:RadTab>
                        </Tabs>
                    </telerik:RadTabStrip>
                </ContentTemplate>
                <%--this is used to have the create pdf button inside the update panel--%>
                <%--<Triggers>
                            <asp:PostBackTrigger ControlID="btncreatepdf" />
                        </Triggers>--%>
            </asp:UpdatePanel>
            <div class="tabPage" style="width: 880px;">
                <telerik:RadMultiPage ID="itemBriefPages" runat="server">
                    <telerik:RadPageView ID="SpecificationTab" runat="server">
                        <div class="left dirtyValidationArea" style="width: 45%;">
                            Description:<br />
                            <asp:TextBox ID="txtDescription" TextMode="MultiLine" Rows="4" runat="server"></asp:TextBox>
                            <br />
                            <br />
                            Brief:<br />
                            <asp:TextBox ID="txtBrief" TextMode="MultiLine" Rows="4" runat="server"></asp:TextBox>
                            <br />
                        </div>
                        <div class="    dirtyValidationArea right" style="width: 45%;">
                            Usage:<br />
                            <asp:TextBox ID="txtUsage" TextMode="MultiLine" Rows="4" runat="server"></asp:TextBox>
                            <br />
                            <br />
                            Notes:
                                            <br />
                            <asp:TextBox ID="txtConsiderations" TextMode="MultiLine" Rows="4" runat="server"></asp:TextBox>
                        </div>
                        <br style="clear: both;" />
                        <div class="dirtyValidationArea">
                            <table style="width: 45%;" class="left">
                                <tr>
                                    <td style="width: 130px;">First Appears:
                                    </td>
                                    <td>Act
                                                        <asp:TextBox ID="txtAct" Width="30" MaxLength="10" runat="server"></asp:TextBox>
                                        &nbsp;&nbsp;Scene
                                                        <asp:TextBox ID="txtScene" Width="30" MaxLength="10" runat="server"></asp:TextBox>
                                        &nbsp;&nbsp;Page
                                                        <asp:TextBox ID="txtPage" Width="30" MaxLength="10" runat="server"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Category:
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtCategory" MaxLength="100" runat="server"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Character:
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtCharacter" MaxLength="100" runat="server"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Preset:
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtPreset" MaxLength="100" runat="server"></asp:TextBox>
                                    </td>
                                </tr>
                            </table>
                            <table style="width: 45%; margin-left: 86px;" class="left">
                                <tr>
                                    <td>Approver:
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtApprover" MaxLength="100" runat="server"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Rehearsal Item:
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtRehearsalItem" MaxLength="100" runat="server"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Source:
                                    </td>
                                    <td>
                                        <asp:DropDownList ID="ddlSource" runat="server"></asp:DropDownList>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <br style="clear: both;" />
                        <div id="divDynamicFields" class="dirtyValidationArea" style="padding-top: 20px;">
                            <img src="../Common/Images/in_progress.gif" />
                        </div>
                        <br style="clear: both;" />
                        <div class="buttonArea" style="margin-top: 10px;">
                            <asp:UpdatePanel runat="server" ID="upnlPdf">
                                <ContentTemplate>
                                    <asp:Button ID="btnCreatePDF" runat="server" OnClientClick="client_btnCreatePDF_Click();"
                                        Text="Create PDF" CssClass="ignoreDirtyFlag" ValidationGroup="ItemBriefFields" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                            <asp:Button ID="btnCreatePDFHidden" runat="server" OnClick="btnCreatePDF_Click"
                                Text="" CssClass="ignoreDirtyFlag" ValidationGroup="ItemBriefFields" Style="display: none;" />
                            <asp:Button ID="btnCreatePDFHiddenNoDirtyCheck" runat="server" OnClick="btnCreatePDFHiddenNoDirtyCheck_Click"
                                Text="" CssClass="ignoreDirtyFlag" ValidationGroup="ItemBriefFields" Style="display: none;" />
                        </div>
                        <br style="clear: both;" />
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="TasksTab" runat="server">
                        <div style="height: 405px;">
                            <sb:ItemBriefTasks ID="ItemBriefTasks" DisplayMode="ItemBriefTaskListView" runat="server" />
                        </div>
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="AttachmentsTab" runat="server">
                        <div style="padding-bottom: 10px;">
                            <sb:Attachments ID="attachments" runat="server" />
                        </div>
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="PinboardTab" runat="server">
                        <div style="padding-bottom: 10px;">
                            <div class="right" style="position: relative; bottom: 5px; display: inline-block;">
                                <sb:HelpTip ID="helpTipProjectPanel" Visible="true" runat="server" Width="470">
                                    <p>
                                        You can pin Items to this Project from the Company Inventory using the Project Panel.
                                    </p>
                                    <br />
                                    <p>
                                        Pin any Items you think might work for this Item Brief, then use the 'Tick' to confirm the booking, or the 'X' to release the Item back to the Inventory.
                                    </p>
                                </sb:HelpTip>
                            </div>
                            <br style="clear: both;" />
                            <asp:UpdatePanel runat="server" ID="upnlPinBoard">
                                <ContentTemplate>
                                    <sb:PinnedItems ID="pinnedItems" runat="server" />
                                    <asp:Button runat="server" ID="btnHiddenUpdateTabs" OnClick="btnHiddenUpdateTabs_Click" Style="display: none;" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="CompletedItemTab" runat="server">
                        <div style="padding-bottom: 10px;">
                            <sb:CompletedItem ID="completedItem" runat="server" />
                            <span style="clear: both; margin-bottom: 5px; margin-top: 5px;">&nbsp;</span>
                        </div>
                    </telerik:RadPageView>
                </telerik:RadMultiPage>
            </div>
        </div>
        <div style="margin-top: 10px; padding-right: 10px;">
            <asp:UpdatePanel ID="upnlButtonsAndPopups" runat="server">
                <ContentTemplate>
                    <!--'ignoreDirtyFlag' css class will allow the button to postback
        without the browser warning message, even if the form is dirty.Same validation group is used in ItemBriefTask control.-->

                    <asp:Button ID="btnSave" CssClass="ignoreDirtyFlag buttonStyle"
                        ValidationGroup="ItemBriefFields" runat="server" Text="Save" OnClientClick="SaveItemBriefDetailsPage(); return false;" />
                    <asp:Button ID="btnRemove" CssClass="buttonStyle" runat="server" Text="Remove" OnClick="btnRemove_Click" />
                    <asp:Button ID="btnCheckStopProcessing" runat="server" Style="display: none;" />
                    <div id="itemBriefSavedNotice" class="inlineNotification right">
                        Changes saved.
                    </div>
                    <sb:PopupBox ID="popupItemBriefRemoveConfirmation" Title="Remove Item Brief" Height="100"
                        runat="server">
                        <BodyContent>
                            <div runat="server" id="divCompleteItemBriefRemove" visible="false" style="white-space: nowrap;">
                                Are you sure you want to permanently remove this Item Brief?
                            </div>
                            <div id="divNotCompleteItemBriefRemove" runat="server" visible="false">
                                <div style="font-weight: bold; margin-left: 5px; font-size: medium;">
                                    Before you remove the
                                    <asp:Literal ID="ltrItemType1" runat="server"></asp:Literal>
                                    Brief from this Project...
                                </div>
                                <div style="margin-top: 10px; margin-left: 5px;">
                                    Would you like to keep a record of it in your Company Inventory in case you need
                                    it for other Projects?
                                </div>
                                <div style="margin-left: 30px; margin-top: 25px;">
                                    <div style="font-weight: bold;">
                                        If you would like to keep it, please select 'Complete Item' and then you can remove
                                        it from the Project.
                                    </div>
                                    <div style="margin-left: 10px; font-style: italic;">
                                        The details for this
                                        <asp:Literal ID="ltrItemType2" runat="server"></asp:Literal>
                                        Brief will be kept in the Company Inventory.
                                        <br />
                                    </div>
                                    <div style="font-weight: bold; margin-top: 10px;">
                                        If not please select 'Confirm'.
                                    </div>
                                    <div style="margin-left: 10px; font-style: italic;">
                                        This will remove all information about this
                                        <asp:Literal ID="ltrItemType3" runat="server"></asp:Literal>
                                        Brief from StageBitz.
                                    </div>
                                </div>
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <%--<input type="button" class="popupBoxCloser buttonStyle" value="No" />--%>
                            <asp:Button ID="btnRemoveConfirmComplete" CssClass="ignoreDirtyFlag buttonStyle"
                                OnClick="btnRemoveConfirmComplete_Click" runat="server" Text="Yes" Visible="false" />
                            <asp:Button ID="btnRemoveConfirmNotComplete" CssClass="ignoreDirtyFlag buttonStyle"
                                OnClick="btnRemoveConfirmComplete_Click" runat="server" Text="Confirm" Visible="false" />
                            <asp:Button ID="btnRemoveCompleteItem" CssClass="ignoreDirtyFlag buttonStyle"
                                runat="server" Text="Complete Item" Visible="false" OnClientClick="btnRemoveCompleteItem_Click();" />
                            <input type="button" id="removeCancel" class="popupBoxCloser buttonStyle" runat="server" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>
    <asp:UpdatePanel runat="server" ID="upnlPopups">
        <ContentTemplate>
            <sb:PopupBox ID="popCompleteItemTabDirty" runat="server" Title="Save unsaved changes">
                <BodyContent>
                    <div style="width: 300px;">
                        You have unsaved changes in Complete Item tab. Do you want to save them and proceed?<br />
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="Button3" CssClass="buttonStyle" runat="server" Text="Yes" OnClientClick="client_btnConfirmCompleteItemTab_Click();" />
                    <asp:Button ID="btnConfirmCompleteItemTabHidden" CssClass="buttonStyle" runat="server" Text="" OnClick="btnConfirmCompleteItemTab_Click" Style="display: none;" />
                    <asp:Button ID="btnCancelConfirmCompleteItemTab" CssClass="buttonStyle" runat="server" Text="No" OnClick="btnCancelConfirmCompleteItemTab_Click" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupConfirmCreatePDF" runat="server" Title="Save unsaved changes">
                <BodyContent>
                    <div style="width: 300px;">
                        You have unsaved changes in this page. Do you want to save them and proceed?<br />
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnConfirmCreatePDF" CssClass="buttonStyle" runat="server" Text="Yes" OnClientClick="ConfirmSavePDFCreate(); return false;" />
                    <asp:Button ID="btnCancelConfirmCreatePDF" CssClass="buttonStyle" runat="server" Text="No" OnClientClick="client_btnCancelCreatePDF_Click(); return false;" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:ErrorPopupBox ID="popupConcurrencyBookingOverlap" runat="server" Title="Hold on... someone else needs this information, as well" ShowCornerCloseButton="false" ErrorCode="NoEditPermissionForItemInItemBrief">
                <bodycontent>
                    <div style="width: 500px;">
                        There is either another booking in the system for this Item or your booking has just finished, so we can't update the details right now.
                        However, you can email the Inventory Administrator and let them know about your suggested change and they can update the listing later.
                        <br />
                    </div>
                </bodycontent>
                <bottomstripecontent>
                    <asp:Button ID="btnReloadConcurrencyBookingOverlap" CssClass="buttonStyle" runat="server" Text="Ok" OnClick="btnReloadConcurrencyBookingOverlap_Click" />
                </bottomstripecontent>
            </sb:ErrorPopupBox>
            
            <sb:ErrorPopupBox runat="server" ID="popupPinnedDelayed" Title="This Item has been delayed." ErrorCode="ItemBookingDelayed">
                <BodyContent>
                    <div style="width: 500px;">
                        This Item is delayed due to another booking being overdue. Contact the Booking Manager
                        <asp:HyperLink runat="server" ID="lnkPopupPinnedDelayedContactIM"></asp:HyperLink> for details.
                    </div>
                </BodyContent>
                <BottomStripeContent>
                     <input type="button" class="ignoreDirtyFlag popupBoxCloser buttonStyle" value="Ok" />
                </BottomStripeContent>
            </sb:ErrorPopupBox>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
