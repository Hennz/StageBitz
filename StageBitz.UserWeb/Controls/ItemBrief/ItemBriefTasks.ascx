<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ItemBriefTasks.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.ItemBrief.ItemBriefTasks" %>
<telerik:RadWindowManager ID="RadWindowManager1" runat="server">
</telerik:RadWindowManager>
<script type="text/javascript">
    function DisableCheckBoxes() {
        var grid = $find("<%=gvTasks.ClientID %>");
        var masterTable = grid.get_masterTableView();
        var number = 0;

        for (var i = 0; i < masterTable.get_dataItems().length; i++) {
            var gridItemElement = masterTable.get_dataItems()[i].findElement("chkStatus");
            gridItemElement.disabled = true;
        }
    }

    function NetCostValueChanged(sender, args) {
        var netCostInput = $(sender._element);
        var taxInput = $find(netCostInput.parents('.CostArea').find('.TaxInput').get(-1).id);
        var totalCostInput = $find(netCostInput.parents('.CostArea').find('.TotalCostInput').get(-1).id);
        SetTotal(sender, taxInput, totalCostInput);
    }

    function TotalCostValueChanged(sender, args) {
        var totalCostInput = $(sender._element);
        var netCostInput = $find(totalCostInput.parents('.CostArea').find('.NetCostInput').get(-1).id);
        var taxInput = $find(totalCostInput.parents('.CostArea').find('.TaxInput').get(-1).id);
        ClearNetCostAndTax(netCostInput, taxInput)
    }

    function TaxValueChanged(sender, args) {
        var taxInput = $(sender._element);
        var netCostInput = $find(taxInput.parents('.CostArea').find('.NetCostInput').get(-1).id);
        var totalCostInput = $find(taxInput.parents('.CostArea').find('.TotalCostInput').get(-1).id);
        SetTotal(netCostInput, sender, totalCostInput);
    }

    function TotalCostCheckForEmpty(sender, args) {
        var element = $(sender._element);

        // If value is empty or null
        if (!sender.get_value()) {
            var netCostInput = $find(element.parents('.CostArea').find('.NetCostInput').get(-1).id);
            var taxInput = $find(element.parents('.CostArea').find('.TaxInput').get(-1).id);
            var totalCostInput = $find(element.parents('.CostArea').find('.TotalCostInput').get(-1).id);
            ClearNetCostAndTax(netCostInput, taxInput);
        }
    }

    // Common methods
    function SetTotal(netCostInput, taxInput, totalCostInput) {
        var total = netCostInput.get_value() + taxInput.get_value();
        if (total) {
            totalCostInput.set_value(total);
        }
    }

    function ClearNetCostAndTax(netCostInput, taxInput) {
        netCostInput.clear();
        taxInput.clear();
    }

</script>
<asp:UpdatePanel ID="up" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlAddTask" DefaultButton="btnAddTask" runat="server">
            <div style="float: left; margin-right: 15px; padding-top: 5px; text-align: left;
                margin: 5px;">
                Description:
            </div>
            <div style="float: left; margin: 5px;">
                <asp:TextBox ID="txtDescription" Width="300" runat="server"></asp:TextBox>
            </div>
            <div style="float: left; margin-left: 20px; padding-top: 10px;">
                Estimated Cost:
            </div>
            <div style="float: left; margin-left: 10px; padding-top: 8px;">
                <telerik:RadNumericTextBox SkinID="Currency" Width="100" ID="txtEstimatedCost" runat="server">
                </telerik:RadNumericTextBox>
            </div>
            <div style="margin-left: 35px; float: left; padding-top: 3px;">
                <asp:Button ID="btnAddTask" ValidationGroup="validation" runat="server" CssClass="buttonStyle"
                    OnClick="AddTask" Text="Add" />
            </div>
            <div style="clear: both;">
            </div>
            <div style="height: 5px; float: left; width: 100%;">
                <div style="float: left; width: 200px; margin-left: 75px;">
                    &nbsp;
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" ControlToValidate="txtDescription"
                        ValidationGroup="validation" runat="server" ErrorMessage="Description is required."></asp:RequiredFieldValidator>
                </div>
                <div style="float: left; margin-left: 235px;">
                    
                </div>
                <div style="clear: both;">
                </div>
            </div>
            <div style="clear: both; height: 10px; margin-bottom: 5px;">
            </div>
        </asp:Panel>
        <asp:Panel runat="server" Width="100%" DefaultButton="btnDoNothing">
            <asp:Button ID="btnDoNothing" runat="server" OnClientClick="return false;" Style="display: none;" />
            <telerik:RadGrid ID="gvTasks" Width="100%" runat="server" OnSortCommand="gvTasks_SortCommand"
                OnItemDataBound="gvTasks_ItemDataBound" AllowSorting="true" AllowAutomaticDeletes="True"
                AllowAutomaticInserts="false" OnNeedDataSource="gvTasks_NeedDataSource" AllowAutomaticUpdates="True"
                OnUpdateCommand="gvTasks_UpdateCommand" AutoGenerateColumns="False" OnDeleteCommand="gvTasks_ItemDeleted">
                <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                <EditItemStyle CssClass="CostArea" />
                <MasterTableView Width="100%" AllowNaturalSort="false" EditMode="InPlace" DataKeyNames="ItemBriefTask.ItemBriefTaskId,ItemBriefTask.LastUpdatedDate">
                    <NoRecordsTemplate>
                        <div class="noData">
                            No data
                        </div>
                    </NoRecordsTemplate>
                    <Columns>
                        <telerik:GridTemplateColumn SortExpression="SortOrder" UniqueName="Status" HeaderText="Complete">
                            <ItemStyle Width="45" />
                            <ItemTemplate>
                                <asp:CheckBox ID="chkStatus" OnClick="JavaScript:DisableCheckBoxes();" OnCheckedChanged="ChangeTaskStatus"
                                    AutoPostBack="true" runat="server" />
                            </ItemTemplate>
                            <HeaderStyle Width="45" />
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Item Name" SortExpression="ItemBriefName"
                            Visible="false" HeaderStyle-HorizontalAlign="Left" UniqueName="ItemName">
                            <HeaderStyle Width="80" />
                            <ItemTemplate>
                                <asp:HyperLink ID="hyperLinkItem" Target="_blank" runat="server"></asp:HyperLink>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Description" UniqueName="Description">
                            <ItemStyle Width="93" />
                            <ItemTemplate>
                            </ItemTemplate>
                            <HeaderStyle Width="93" />
                            <EditItemTemplate>
                                <div style="float: left; width: 70%">
                                    <asp:TextBox runat="server" ID="tbDescription">
                                </asp:TextBox>
                                </div>
                                &nbsp;
                                  <div style="float: left; margin-top: 7px;">
                                        <asp:RequiredFieldValidator ID="RqdDescription" ControlToValidate="tbDescription"
                                        ErrorMessage="*" ToolTip="Description is required." runat="server">
                                    </asp:RequiredFieldValidator>
                                </div>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Vendor" UniqueName="Vendor" EditFormColumnIndex="1">
                            <ItemStyle Width="55" />
                            <HeaderStyle Width="55" />
                            <ItemTemplate>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox runat="server" Width="80" MaxLength="250" ID="tbVendor">
                                </asp:TextBox>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Est Cost" SortExpression="ItemBriefTask.EstimatedCost"
                            DataType="System.Decimal" HeaderStyle-HorizontalAlign="Right" UniqueName="EstimatedCost">
                            <ItemStyle HorizontalAlign="Right" Width="55" />
                            <HeaderStyle Width="55" />
                            <ItemTemplate>
                                <img runat="server" id="imgNoEstimatedCost" title="You haven't entered an estimated cost for this task." visible="false" src="~/Common/Images/NoExpendedCostWarning.png" />
                                <%#  StageBitz.UserWeb.Common.Helpers.Support.FormatCurrency(Eval("ItemBriefTask.EstimatedCost"),CultureName)%>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <telerik:RadNumericTextBox runat="server" ID="tbEstimatedCost" SkinID="Currency"
                                    Width="60" DbValue='<%# Bind("ItemBriefTask.EstimatedCost") %>'>
                                </telerik:RadNumericTextBox>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Net Cost" SortExpression="ItemBriefTask.NetCost"
                            DataType="System.Decimal" HeaderStyle-HorizontalAlign="Right" UniqueName="NetCost"
                            EditFormColumnIndex="1">
                            <ItemStyle HorizontalAlign="Right" Width="60" />
                            <HeaderStyle Width="60" />
                            <ItemTemplate>
                                <%#  StageBitz.UserWeb.Common.Helpers.Support.FormatCurrency(Eval("ItemBriefTask.NetCost"),CultureName)%>
                            </ItemTemplate>
                            <EditItemTemplate>
                                    <telerik:RadNumericTextBox runat="server" ID="tbNetCost" SkinID="Currency" Width="75"
                                        DbValue='<%# Bind("ItemBriefTask.NetCost") %>' CssClass="NetCostInput">
                                        <ClientEvents OnBlur="NetCostValueChanged" />
                                    </telerik:RadNumericTextBox>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Tax" SortExpression="ItemBriefTask.Tax" HeaderStyle-HorizontalAlign="Right"
                            DataType="System.Decimal" UniqueName="Tax" EditFormColumnIndex="1">
                            <ItemStyle HorizontalAlign="Right" Width="50" />
                            <HeaderStyle Width="50" />
                            <ItemTemplate>
                                <%#  StageBitz.UserWeb.Common.Helpers.Support.FormatCurrency(Eval("ItemBriefTask.Tax"),CultureName)%>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <telerik:RadNumericTextBox runat="server" ID="tbTax" SkinID="Currency" Width="50"
                                    DbValue='<%# Bind("ItemBriefTask.Tax") %>' CssClass="TaxInput">
                                     <ClientEvents OnBlur="TaxValueChanged" />
                                </telerik:RadNumericTextBox>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Total Cost" SortExpression="Total" HeaderStyle-HorizontalAlign="Right"
                            DataType="System.Decimal" UniqueName="Total" EditFormColumnIndex="1">
                            <ItemStyle HorizontalAlign="Right" Width="50" />
                            <HeaderStyle Width="50" />
                            <ItemTemplate>
                                <%#  StageBitz.UserWeb.Common.Helpers.Support.FormatCurrency(Eval("Total"),CultureName)%>
                            </ItemTemplate>
                            <EditItemTemplate>
                                    <telerik:RadNumericTextBox runat="server" ID="tbTotal" SkinID="Currency" Width="75"
                                        DbValue='<%# Bind("Total") %>' CssClass="TotalCostInput">
                                        <ClientEvents OnKeyPress="TotalCostValueChanged" />
                                        <ClientEvents OnValueChanged="TotalCostCheckForEmpty" />
                                    </telerik:RadNumericTextBox>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridEditCommandColumn ButtonType="ImageButton" UniqueName="EditCommandColumn">
                            <ItemStyle Width="35" HorizontalAlign="Center" CssClass="MyImageButton" />
                            <HeaderStyle Width="35" />
                        </telerik:GridEditCommandColumn>
                        <telerik:GridButtonColumn ConfirmDialogType="RadWindow" ConfirmTitle="Delete" ButtonType="ImageButton"
                            ConfirmDialogHeight="100" CommandName="Delete" Text="Delete" UniqueName="DeleteColumn">
                            <ItemStyle HorizontalAlign="Center" CssClass="MyImageButton" />
                            <ItemStyle Width="30" />
                            <HeaderStyle Width="30" />
                        </telerik:GridButtonColumn>
                    </Columns>
                </MasterTableView>
                <ClientSettings>
                    <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="300" SaveScrollPosition="True">
                    </Scrolling>
                </ClientSettings>
            </telerik:RadGrid>
            <telerik:GridTextBoxColumnEditor ID="txtBoxEditorVendor" runat="server" TextBoxStyle-Width="130px" />
        </asp:Panel>
        <sb:PopupBox ID="popupEditTask" Title="Complete Task Details" runat="server">
            <BodyContent>
                <asp:Panel runat="server" DefaultButton="btnEnterKeyButton" Style="white-space: nowrap;" CssClass="CostArea">
                    <table>
                        <tr>
                            <td style="width: 100px;">
                                Vendor
                            </td>
                            <td style="width: 250px;">
                                <asp:TextBox ID="txtVendor" MaxLength="250" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Net Cost
                            </td>
                            <td>
                                <telerik:RadNumericTextBox SkinID="Currency" Width="100" ID="txtNetCost" runat="server" CssClass="NetCostInput">
                                    <ClientEvents OnBlur="NetCostValueChanged" />
                                </telerik:RadNumericTextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Tax
                            </td>
                            <td style="padding-top: 5px;">
                                <telerik:RadNumericTextBox SkinID="Currency" Width="100" ID="txtTax" runat="server" CssClass="TaxInput">
                                    <ClientEvents OnBlur="TaxValueChanged" />
                                </telerik:RadNumericTextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Total Cost
                            </td>
                            <td style="padding-top: 5px;">
                                <telerik:RadNumericTextBox SkinID="Currency" Width="100" ID="txtTotalCost" runat="server" CssClass="TotalCostInput">
                                    <ClientEvents OnKeyPress="TotalCostValueChanged" />
                                    <ClientEvents OnValueChanged="TotalCostCheckForEmpty" />                                    
                                </telerik:RadNumericTextBox>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </BodyContent>
            <BottomStripeContent>
                <div style="text-align: right;">
                    <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />         
                    <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="buttonStyle" OnClick="SavePopup" />
                    <!--This is to prevent anything from happening when the enter key is pressed on any textbox-->
                    <asp:Button ID="btnEnterKeyButton" OnClientClick="return false;" runat="server" Text="Button"
                        Style="display: none;" />
                </div>
            </BottomStripeContent>
        </sb:PopupBox>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="btnAddTask" EventName="Click" />
    </Triggers>
</asp:UpdatePanel>
