<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryBusinessCard.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Inventory.InventoryBusinessCard" %>
<script type="text/javascript">
    function ShowEditArea(element) {
        if (element) {
            var jqElement = $(element);
            var elementId = jqElement.attr('id');

            var viewArea = jqElement.closest('.textMode');
            viewArea.hide();

            var editArea = viewArea.next('.editMode');
            editArea.show();

            $(document).trigger('onShowEditArea_InventoryBusinessCard', [elementId]);
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

            var qty = $('span[id$="lblQuantity"]', viewArea).text().trim();
            if (!isNaN(qty)) {
                $find($('input[id$="rntxtQuntity"]', editArea).attr('id')).set_value(qty);
            }

            $(document).trigger('onHideEditArea_InventoryBusinessCard', [elementId]);
        }
    }

    $(document).on('onShowEditArea_InventoryBusinessCard', function (e, elementId) {
        var qtyRow = $("#<%= trQuantity.ClientID%>");
        var ibtnShowClicked = qtyRow.find(':has(#' + elementId + ')');
        if (ibtnShowClicked.length == 0) {
            var ibtnHide = qtyRow.find('*[id$="lbtnCancel"]').get(-1);
            if (ibtnHide) {
                HideEditArea(ibtnHide);
            }
        }
    });
</script>
<div class="left">
    <sb:ImageDisplay ID="thumbItem" ShowImagePreview="true" runat="server" />
</div>
<div style="text-align: left; margin-left: 10px;" class="left">
    <asp:HyperLink Font-Size="12" ID="lnkName" runat="server" Visible="false" Target="_blank"></asp:HyperLink>
    <asp:Label ID="lblName" Font-Size="12" runat="server"></asp:Label>
    <table style="margin-top: 2px; line-height: 20px; margin-bottom: 2px;">
        <tr visible="false" id="trBookingFrom" runat="server">
            <td>From
            </td>
            <td>:
            </td>
            <td colspan="2">
                <asp:Literal ID="litFromDate" runat="server"></asp:Literal>
            </td>
        </tr>
        <tr visible="false" id="trBookingTo" runat="server">
            <td>To
            </td>
            <td>:
            </td>
            <td colspan="2">
                <asp:Literal ID="litToDate" runat="server"></asp:Literal>
            </td>
        </tr>
        <tr id="trQuantity" runat="server">
            <td style="width: 20px;">Quantity
            </td>
            <td>:
            </td>
            <td>
                <div class="textMode left">
                    <asp:Label ID="lblQuantity" runat="server"></asp:Label>
                    <a onclick="ShowEditArea(this); return false;" style="text-decoration: none; cursor:pointer;" title="Edit" runat="server" id="lbtnEdit">
                        <i class="fa fa-pencil"></i>
                    </a>
                </div>
                <div class="editMode left" style="display: none;" id="divQtyEdit" runat="server">
                    <telerik:RadNumericTextBox runat="server" ID="rntxtQuntity" ShowSpinButtons="true" Width="50">
                    </telerik:RadNumericTextBox>
                    of
                    <asp:Label runat="server" ID="lblAvailableQty"></asp:Label>
                    <asp:LinkButton runat="server" ID="lbtnOk" OnClick="lbtnOk_Click" Font-Underline="false" ToolTip="Save">
                        <i class="fa fa-check"></i>
                    </asp:LinkButton>
                    <a onclick="HideEditArea(this); return false;" style="text-decoration: none; cursor:pointer;" title="Cancel" runat="server" id="lbtnCancel">
                        <i class="fa fa-times"></i>
                    </a>
                </div>
            </td>
        </tr>
        <tr id="trStatus" runat="server">
            <td>Status
            </td>
            <td>:
            </td>
            <td>
                <asp:Literal ID="litStatus" runat="server"></asp:Literal>
            </td>
        </tr>
        <tr id="trLocation" runat="server">
            <td>Location
            </td>
            <td>:
            </td>
            <td>
                <asp:Label ID="lblLocation" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trCompany" runat="server">
            <td>Company
            </td>
            <td>:
            </td>
            <td>
                <asp:Label ID="lblcompany" runat="server"></asp:Label>
            </td>
        </tr>
    </table>
    <div runat="server" class="BusinessCardDescription" id="lblDescription">
    </div>
</div>
