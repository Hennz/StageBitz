<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompleteItemHeader.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Item.CompleteItemHeader" %>
<%@ Register Src="~/Controls/Common/DocumentList.ascx" TagName="DocumentList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Inventory/InventoryLocations.ascx" TagPrefix="sb" TagName="InventoryLocations" %>
<%@ Register Src="~/Controls/Inventory/ItemVisibilityToolTip.ascx" TagPrefix="sb" TagName="ItemVisibilityToolTip" %>
<%@ Register Src="~/Controls/Inventory/InventoryUpdateVisibility.ascx" TagPrefix="sb" TagName="InventoryUpdateVisibility" %>

<script type="text/javascript">
    function CompleteItemHeaderDirtyValidation_<%= this.ClientID %>() {
        var select = '#<%=itemDetailsHeaderArea.ClientID%> :not(:has(.dirtyValidationExclude)) :input:not([type=hidden],:submit,:password,:button)';
        $(document).off('change.dirtyValidationCompletedItem', select);
        $(document).on('change.dirtyValidationCompletedItem', select, function () {
            if (CanSetDirty(this)) {
                $("#<%= hdnIsDirty.ClientID %>").val("True");
            }
        });
    }

    function BookedQtyChanged(sender, eventArgs) {
        $(document).trigger('onQuantityChanged', eventArgs.get_newValue());
    }

    $(document).on("onAvailableQtyChanged", function (event, itemId, availableQty) {
        var numerickInput = $find("<%= txtBookedQty.ClientID %>")
        var currentQtyBooked = numerickInput.get_value();
        var qtyToSet = availableQty == 0 ? availableQty : availableQty < currentQtyBooked ? availableQty : currentQtyBooked != '' ? currentQtyBooked : 1;
        numerickInput.set_value(qtyToSet);
        //This is an edge scenareo. We need to call the service and make sure that the new quantity is sufficient.
        //if (availableQty < currentQtyBooked) {
        //    $(document).trigger('onValidateQuantityBookedFromService', [itemId, availableQty]);
        //}
        numerickInput.set_maxValue(availableQty);
        numerickInput.set_textBoxValue(qtyToSet);
        numerickInput.set_minValue(availableQty == 0 ? availableQty : 1);
    });

    function ShowEditArea(element) {
        if (element) {
            var jqElement = $(element);
            var elementId = jqElement.attr('id');

            var viewArea = jqElement.closest('.textMode');
            viewArea.hide();

            var editArea = viewArea.next('.editMode');
            $find($("input[id$='txtItemQuantity']", this.ItemHeaderElement).attr('id')).enable();
            editArea.show();
        }
    }

    function HideEditArea(element) {
        if (element) {
            var jqElement = $(element);
            var elementId = jqElement.attr('id');

            var editArea = jqElement.closest('.editMode');
            editArea.hide();

            var viewArea = editArea.prev('.textMode');
            viewArea.show();
        }
    }

    function UpDateQuantity(element) {
        // var itemObj = new StageBitz.UserWeb.Common.Scripts.ItemTypes.Item();
        var itemQuantity = $find($("input[id$='txtItemQuantity']", this.ItemHeaderElement).attr('id')).get_value();
        ShowOverlay();
        itemObj.PopulateSaveData();
        itemObj.CallSaveDataService(function () {

        });
        HideOverlay();
        HideEditArea(element);
        setGlobalDirty(false);
    }

    function ShowOverlay() {
        $('#itemtypeOverlay').show();
    }

    function HideOverlay() {
        $('#itemtypeOverlay').hide();
    }

</script>
<script type="text/javascript">
    $(function () {
        var txtCreatedFor = $("#<%=txtCreatedFor.ClientID%>");
        var companyId = "<%=this.CompanyId%>";
        if (txtCreatedFor.length > 0 && companyId > 0) {
            $("#<%=txtCreatedFor.ClientID%>").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "../Services/InventoryService/GetCreatedForSearchItems",
                        dataType: "json",
                        type: "POST",
                        data: {
                            CompanyId: companyId,
                            Keyword: request.term
                        },
                        success: function (data) {
                            response($.map(data, function (item) {
                                return {
                                    label: item,
                                    value: item
                                }
                            }));
                        }
                    });
                },
                minLength: 1,
                change: function (event, ui) { setGlobalDirty(true); }
            });
        }
    });
</script>
<div style="width: 17%; padding-top: 5px;" class="left">
    <asp:UpdatePanel ID="upnlItemBriefThumb" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <table>
                <tr>
                    <td style="text-align: center;">
                        <sb:ImageDisplay ID="thumbItemBrief" ShowImagePreview="true" runat="server" />
                        <asp:HiddenField ID="hdnDefaultImageId" runat="server" />
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
                                        <span class="grayText">Click an image to set it as item's preview image.</span>
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
<div style="width: 81.5%; margin-top: 5px;" class="left" runat="server" id="itemDetailsHeaderArea">
    <table id="itemBriefDetailsTable" style="padding-right: 1%;" class="left">
        <tr>
            <td style="width: 120px; padding-top: 4px;" valign="top">Name
            </td>
            <td style="width: 5px; padding-top: 4px;" valign="top">:
            </td>
            <td valign="top" style="min-width:200px;">
                <sb:FocusEditBox ID="itemNameEdit" DisplayMaxLength="50" runat="server">
                    <TextBox MaxLength="100" CssClass="largeText" ValidationGroup="ItemFields" Width="200"></TextBox>
                    <DisplayLabel CssClass="focusEditField largeText"></DisplayLabel>
                </sb:FocusEditBox>
                <span style="position: relative; bottom: 2px;"></span>
                <sb:ItemVisibilityToolTip runat="server" ID="sbItemVisibilityToolTip" />
                <asp:UpdatePanel runat="server" ID="upnlVisibility" RenderMode="Inline">
                    <ContentTemplate>
                        <asp:LinkButton runat="server" ID="lbtnChangeVisibility" Text="Change Visibility" OnClick="lbtnChangeVisibility_Click" Font-Size="11px"></asp:LinkButton>
                        <asp:Label runat="server" ID="lblChangeVisibility" Text="Change Visibility" Font-Size="11px" Font-Underline="true" ToolTip="Please complete or remove the booking for this item before changing its visibility."></asp:Label>
                        <sb:PopupBox runat="server" Title="Change Visibility" ID="popupChangeVisibility">
                            <BodyContent>
                                <div class="dirtyValidationExclude" style="width:300px;">
                                    <sb:InventoryUpdateVisibility runat="server" ID="sbInventoryUpdateVisibility" />
                                </div>
                            </BodyContent>
                            <BottomStripeContent>                                
                                <asp:Button CssClass="buttonStyle" ID="btnUpdateVisibility" runat="server" OnClick="btnUpdateVisibility_Click" Text="Ok"/>
                                <input type="button" value="Cancel" class="popupBoxCloser buttonStyle" runat="server" />
                            </BottomStripeContent>
                        </sb:PopupBox>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div style="padding-top: 8px">
                    <asp:UpdatePanel runat="server" ID="upnlReqName">
                        <ContentTemplate>
                            <asp:RequiredFieldValidator ID="reqName" runat="server" ValidationGroup="ItemFields"
                                ErrorMessage="Item name is required."></asp:RequiredFieldValidator>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </td>
        </tr>
        <tr>
            <td valign="top" style="padding-top: 2px;">Item Type
            </td>
            <td valign="top" style="padding-top: 2px;">:
            </td>
            <td valign="top">
                <div runat="server" id="divItemTypeSelect" visible="false">
                    <sb:DropDownListOPTGroup Width="150" ID="ddItemTypes" runat="server">
                    </sb:DropDownListOPTGroup>
                </div>
                <div runat="server" id="divItemTypeStatic" visible="false" style="position: relative; top: 2px;">
                    <asp:Label ID="lblItemType" runat="server"></asp:Label>
                </div>
            </td>
        </tr>
        <tr>
            <td valign="top" style="padding-top: 2px;">Quantity
            </td>
            <td valign="top" style="padding-top: 2px;">:
            </td>
            <td>
                <div style="margin-right: 2px;" class="left">
                    <div class="textMode left">
                        <telerik:RadNumericTextBox ID="txtItemQuantity" CssClass="ignoreSetReadOnly" ShowSpinButtons="true" Width="60"
                            runat="server" ValidationGroup="ItemFields">
                        </telerik:RadNumericTextBox>
                    </div>


                    <div id="divQtyEdit" style="display: none; padding-left: 4px;" class="textMode left">
                        <a onclick="ShowEditArea(this); return false;" style="text-decoration: none; cursor: pointer;" title="Edit" runat="server" id="lbtnEdit">
                            <i class="fa fa-pencil"></i>
                        </a>
                    </div>
                    <div class="editMode left" style="padding-left: 4px; display: none;" id="divQtyEditSave" runat="server">

                        <asp:LinkButton runat="server" ID="lbtnOk" OnClientClick="UpDateQuantity(this);return false;" Font-Underline="false" ToolTip="Save">
                        <i class="fa fa-check"></i>
                        </asp:LinkButton>
                        <a onclick="HideEditArea(this); return false;" style="text-decoration: none; cursor: pointer;" title="Cancel" runat="server" id="lbtnCancel">
                            <i class="fa fa-times"></i>
                        </a>
                    </div>

                </div>
                <div style="padding-left: 130px;" runat="server" id="divBookedQty">
                    <div class="left">
                        Available: 
                    </div>
                    <div id="divAvailableQty" class="left" style="padding-left: 2px;">
                    </div>
                    <div class="left" style="padding-left: 30px;">
                        Book Quantity:
                        <telerik:RadNumericTextBox ID="txtBookedQty" CssClass="dirtyValidationExclude ignoreSetReadOnly" ShowSpinButtons="true" Width="60"
                            runat="server">
                            <ClientEvents OnValueChanged="BookedQtyChanged" />
                        </telerik:RadNumericTextBox>
                    </div>
                    <div style="clear: both;"></div>
                </div>
                <div style="padding-top: 4px;">
                    <asp:UpdatePanel runat="server" ID="upnlReqQuantity">
                        <ContentTemplate>
                            <asp:RangeValidator ID="rngItemQuantity" runat="server" ControlToValidate="txtItemQuantity"
                                Style="top: 0px;" MinimumValue="0" MaximumValue="99999999999" ValidationGroup="ItemFields"
                                ErrorMessage="Quantity cannot be negative."></asp:RangeValidator>
                            <asp:RequiredFieldValidator ID="reqQuantity" runat="server" ControlToValidate="txtItemQuantity"
                                ValidationGroup="ItemFields" ErrorMessage="Quantity is required."></asp:RequiredFieldValidator>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </td>
        </tr>
        <tr runat="server" visible="false" id="trItemStatus">
            <td style="padding-top: 2px; white-space: nowrap; padding-right: 3px;">Current Status
            </td>
            <td style="padding-top: 2px;">:
            </td>
            <td>
                <asp:Label runat="server" ID="lblItemStatus"></asp:Label>
            </td>
        </tr>
        <tr runat="server" id="trLocation">
            <td>Inventory Location
            </td>
            <td>:
            </td>
            <td>
                <%--<asp:TextBox TextMode="MultiLine" ID="locationEdit" ValidationGroup="ItemFields" runat="server" Width="200" Rows="1"></asp:TextBox>--%>
                <sb:InventoryLocations runat="server" ID="sbInventoryLocations" AccessKey="S" ValidationGroup="ItemFields" CssClass="ignoreSetReadOnly" InventoryLocationDisplayMode="Admin" DisableViewOnlyLocations="true"/>
            </td>
        </tr>
        <tr runat="server" id="trCreatedFor">
            <td>Created For
            </td>
            <td>:
            </td>
            <td>
                <asp:TextBox runat="server" ID="txtCreatedFor" Width="200" MaxLength="100"></asp:TextBox>
            </td>
        </tr>
    </table>
    <div class="left" style="width: 45%;" id="divDescription" runat="server">
        Description:<br />
        <asp:TextBox ID="txtDescription" TextMode="MultiLine" Rows="6" runat="server" ValidationGroup="ItemFields"></asp:TextBox>
    </div>
    <br style="clear: both;" />
    <asp:HiddenField runat="server" ID="hdnIsDirty" Value="False" />

</div>
