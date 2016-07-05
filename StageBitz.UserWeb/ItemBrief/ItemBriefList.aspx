<%@ Page DisplayTitle="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="ItemBriefList.aspx.cs" Inherits="StageBitz.UserWeb.ItemBrief.ItemBriefList" %>

<%@ Register Src="~/Controls/Common/ListViewDisplaySettings.ascx" TagName="ListViewDisplaySettings"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ImportBulkItemBriefs.ascx" TagName="ImportbulkItems"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/ExportData.ascx" TagName="ExportData" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectItemTypes.ascx" TagName="ProjectItemTypes"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
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

        function OnClientResponseError(sender, args) {
            args.set_cancelErrorAlert(true);
        }

        function ClearItemBriefDuplicateErrorMessages(index) {
            var grid = $find('<%= gvItemList.ClientID %>');
            var masterTable = grid.get_masterTableView();
            var row = masterTable.get_dataItems()[0];

            var lblErrorMsgForDuplicateItemBriefs = masterTable.get_dataItems()[index].findElement("lblErrorMsgForDuplicateItemBriefs");
            if (lblErrorMsgForDuplicateItemBriefs != null) {
                lblErrorMsgForDuplicateItemBriefs.innerHTML = "";

            }

        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>| <a id="lnkBookings" runat="server">Bookings</a> |<asp:HyperLink
        ID="hyperLinkTaskManager" runat="server">Task Manager</asp:HyperLink>
    |<sb:ProjectUpdatesLink ID="projectUpdatesLink" runat="server" />
    <sb:ReportList ID="reportList" runat="server" />
</asp:Content>
<asp:Content ID="ContentNavigation" runat="server" ContentPlaceHolderID="PageTitleRight">
    <asp:PlaceHolder ID="plcItemTypeDD" runat="server">
        <sb:ProjectItemTypes ID="projectItemTypes" runat="server" />
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <sb:ProjectWarningDisplay ID="warningDisplay" runat="server" />
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>
    <div class="left">
        <asp:UpdatePanel ID="upnlFindItems" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <asp:Panel ID="pnlFindItems" DefaultButton="btnFind" runat="server">
                    <table>
                        <tr>
                            <td>
                                <div class="searchbox rounded" style="height: 22px; padding-bottom: 2px;">
                                    <span>
                                        <asp:TextBox ID="txtFindName" Width="350" MaxLength="100" runat="server"></asp:TextBox>
                                        <asp:TextBoxWatermarkExtender ID="wmeFindName" TargetControlID="txtFindName" runat="server">
                                        </asp:TextBoxWatermarkExtender>
                                        <div style="display: inline; position: relative; top: 2px; right: 2px;">
                                            <asp:ImageButton ID="ibtnClearSearch" runat="server" CausesValidation="false" Text=""
                                                ImageUrl="~/Common/Images/button_cancel.png"
                                                OnClick="ibtnClearSearch_Click" />
                                        </div>
                                    </span>
                                </div>
                            </td>
                            <td>
                                <asp:Button ID="btnFind" runat="server" CssClass="buttonStyle" OnClick="btnFind_Click"
                                    CausesValidation="false" Text="Find" />
                            </td>
                            <td>
                                <%--<asp:Button ID="btnShowAll" runat="server" CssClass="buttonStyle" OnClick="btnShowAll_Click"
                                    CausesValidation="false" Text="Show All" />--%>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div class="right">
        <asp:UpdatePanel ID="upnlImportBulkItems" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <sb:ImportbulkItems ID="ImportbulkItemsControl" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div class="right" style="margin-right: 2px;">
        <sb:ExportData ID="exportData" OnExcelExportClick="exportData_ExcelExportClick" OnPDFExportClick="exportData_PDFExportClick"
            runat="server" />
    </div>
    <div style="clear: both;">
    </div>
    <asp:UpdatePanel ID="upnlItemList" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <sb:GroupBox ID="grpItemBriefs" runat="server">
                <TitleLeftContent>
                    <asp:Literal ID="ltrlItemListTitle" runat="server"></asp:Literal>
                </TitleLeftContent>
                <TitleRightContent>
                    <sb:ListViewDisplaySettings ID="displaySettings" OnDisplayModeChanged="displaySettings_DisplayModeChanged"
                        runat="server" />
                </TitleRightContent>
                <BodyContent>
                    <asp:Panel ID="pnlAddItem" DefaultButton="btnAdd" runat="server">

                        <table style="width: 100%; border-collapse: collapse; margin-bottom: 5px;">
                            <tr class="gridAltRow" style="padding-bottom: 0px; padding-top: 0px; height: auto;">
                                <td>&nbsp;Name:
                                </td>
                                <td>&nbsp;Description:
                                </td>
                                <td>&nbsp;Qty:
                                </td>
                                <td>&nbsp;Category:
                                </td>

                                <td>&nbsp;Character:
                                </td>
                                <td>&nbsp;Preset:
                                </td>
                                <td>&nbsp;Rehearsal:
                                </td>

                                <td>&nbsp;Act:
                                </td>
                                <td>&nbsp;Scn:
                                </td>
                                <td>&nbsp;Pg:
                                </td>
                                <td></td>
                            </tr>
                            <tr class="gridAltRow">
                                <td>
                                    <asp:TextBox ID="txtName" Width="110" MaxLength="100" runat="server"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqName" runat="server" ControlToValidate="txtName"
                                        SkinID="Hidden" ValidationGroup="ItemBriefFields" ErrorMessage="Name is required."></asp:RequiredFieldValidator>
                                    <asp:CustomValidator ID="cusvalName" runat="server" ControlToValidate="txtName" SkinID="Hidden"
                                        OnServerValidate="cusvalName_ServerValidate" ValidationGroup="ItemBriefFields"
                                        ErrorMessage="Item brief name already exists."></asp:CustomValidator>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtDescription" Width="110" runat="server"></asp:TextBox>
                                </td>
                                <td>
                                    <telerik:RadNumericTextBox ID="txtQuantity" Width="40" runat="server">
                                    </telerik:RadNumericTextBox>
                                    <asp:RangeValidator ID="rngItemQuantity" runat="server" ControlToValidate="txtQuantity"
                                        SkinID="Hidden" Style="top: 0px;" MinimumValue="1" MaximumValue="99999999999"
                                        ValidationGroup="ItemBriefFields" ErrorMessage="Quantity cannot be zero."></asp:RangeValidator>
                                    &nbsp;
                                </td>
                                <td>
                                    <asp:TextBox ID="txtCategory" Width="90" MaxLength="100" runat="server"></asp:TextBox>
                                </td>

                                <td>
                                    <asp:TextBox ID="txtCharacter" Width="90" MaxLength="100" runat="server"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtPreset" Width="90" MaxLength="100" runat="server"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtRehearsal" Width="90" MaxLength="100" runat="server"></asp:TextBox>
                                </td>

                                <td>
                                    <asp:TextBox ID="txtAct" Width="32" MaxLength="10" runat="server"></asp:TextBox>
                                    &nbsp;
                                </td>
                                <td>
                                    <asp:TextBox ID="txtScene" Width="32" MaxLength="10" runat="server"></asp:TextBox>
                                    &nbsp;
                                </td>
                                <td>
                                    <asp:TextBox ID="txtPage" Width="32" MaxLength="10" runat="server"></asp:TextBox>
                                    &nbsp;
                                </td>
                                <td>
                                    <asp:Button ID="btnAdd" runat="server" ValidationGroup="ItemBriefFields" OnClick="btnAdd_Click"
                                        Text="Add" CssClass="buttonStyle" />
                                </td>
                            </tr>
                        </table>
                        <asp:ValidationSummary ID="validationSummaryItemAdd" ValidationGroup="ItemBriefFields"
                            DisplayMode="List" CssClass="message error" runat="server" />
                    </asp:Panel>
                    <telerik:RadToolTipManager ID="tooltipManager" Width="140px" Height="120px" ShowDelay="2000" OnClientResponseError="OnClientResponseError"
                        Position="MiddleRight" OnAjaxUpdate="tooltipManager_AjaxUpdate" runat="server">
                    </telerik:RadToolTipManager>
                    <telerik:RadGrid ID="gvItemList" Width="915" EnableLinqExpressions="False" AutoGenerateColumns="false"
                        OnUpdateCommand="gvItemList_UpdateCommand" AllowSorting="true" OnItemDataBound="gvItemList_ItemDataBound"
                        OnSortCommand="gvItemList_SortCommand" AllowAutomaticUpdates="True" OnNeedDataSource="gvItemList_NeedDataSource"
                        runat="server">
                        <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                        <MasterTableView EditMode="InPlace" Width="915" DataKeyNames="ItemBrief.ItemBriefId,ItemBrief.LastUpdatedDate"
                            AllowNaturalSort="false" AllowMultiColumnSorting="true">
                            <NoRecordsTemplate>
                                <div class="noData">
                                    No data
                                </div>
                            </NoRecordsTemplate>
                            <SortExpressions>
                                <telerik:GridSortExpression FieldName="ItemBrief.Name" SortOrder="Ascending" />
                            </SortExpressions>
                            <Columns>
                                <telerik:GridTemplateColumn HeaderText="Name" SortExpression="ItemBrief.Name" ItemStyle-Width="100px"
                                    HeaderStyle-Width="100px">
                                    <ItemTemplate>
                                        <a id="lnkItemBriefDetails" runat="server" href="#"></a>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <div style="float: left; width: 78%">
                                            <asp:TextBox runat="server" ID="tbItemBriefName" MaxLength="80" Text='<%# Bind("ItemBrief.Name") %>'>
                                            </asp:TextBox>
                                        </div>
                                        <div style="margin-top: 10px; float: left; margin-left: 5px;" class="gridRowValidator">
                                            <asp:Label Text="*" class="inputError" Visible="false" runat="server" ID="lblErrorMsgForDuplicateItemBriefs"
                                                Style="float: left;"></asp:Label>
                                        </div>
                                        <div style="margin-top: 10px; float: left; margin-left: 5px;" class="gridRowValidator">
                                            <asp:RequiredFieldValidator ID="rqdItemBriefName" ControlToValidate="tbItemBriefName"
                                                ErrorMessage="*" ToolTip="Name is required." runat="server">
                                            </asp:RequiredFieldValidator>
                                        </div>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Description" HeaderText="Description" ItemStyle-Width="140"
                                    HeaderStyle-Width="140">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="tbDescription" Text='<%# Bind("ItemBrief.Description") %>'>
                                        </asp:TextBox>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Quantity" SortExpression="ItemBrief.Quantity"
                                    HeaderText="Qty" ItemStyle-Width="45" HeaderStyle-Width="45">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <HeaderStyle HorizontalAlign="Left" />
                                    <EditItemTemplate>
                                        <div style="float: left;">
                                            <telerik:RadNumericTextBox ID="tbItemQuantity" Width="20" runat="server">
                                                <EnabledStyle PaddingTop="0" PaddingBottom="0" PaddingLeft="0" PaddingRight="0" />
                                            </telerik:RadNumericTextBox>
                                        </div>
                                        <div style="float: left; padding-left: 2px;" class="gridRowValidator">
                                            <asp:RangeValidator ID="rngItemQuantity" runat="server" ControlToValidate="tbItemQuantity"
                                                Style="top: 0px;" MinimumValue="1" MaximumValue="99999999999" ErrorMessage="*"
                                                ToolTip="Quantity cannot be zero."></asp:RangeValidator>
                                        </div>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Category" SortExpression="ItemBrief.Category"
                                    HeaderText="Category" ItemStyle-Width="75" HeaderStyle-Width="75">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <HeaderStyle HorizontalAlign="Left" />
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="tbCategory" MaxLength="100" Text='<%# Bind("ItemBrief.Category") %>'>
                                        </asp:TextBox>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>

                                <telerik:GridTemplateColumn UniqueName="Character" SortExpression="ItemBrief.Character" HeaderText="Character"
                                    ItemStyle-Width="75px" HeaderStyle-Width="75">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="tbCharacter" MaxLength="100" Text='<%# Bind("ItemBrief.Character") %>'>
                                        </asp:TextBox>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Preset" SortExpression="ItemBrief.Preset" HeaderText="Preset"
                                    ItemStyle-Width="55px" HeaderStyle-Width="55">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="tbPreset" MaxLength="100" Text='<%# Bind("ItemBrief.Preset") %>'>
                                        </asp:TextBox>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Rehearsal" SortExpression="ItemBrief.RehearsalItem" HeaderText="Rehearsal"
                                    ItemStyle-Width="75px" HeaderStyle-Width="75">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="tbRehearsal" MaxLength="100" Text='<%# Bind("ItemBrief.RehearsalItem") %>'>
                                        </asp:TextBox>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>

                                <telerik:GridTemplateColumn UniqueName="Act" SortExpression="ItemBrief.Act" HeaderText="Act"
                                    ItemStyle-Width="40px" HeaderStyle-Width="40">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="tbAct" MaxLength="10" Text='<%# Bind("ItemBrief.Act") %>'>
                                        </asp:TextBox>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Scene" HeaderText="Scn" ItemStyle-Width="40"
                                    HeaderStyle-Width="40">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="tbScene" MaxLength="10" Text='<%# Bind("ItemBrief.Scene") %>'>
                                        </asp:TextBox>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Page" SortExpression="ItemBrief.Page" HeaderText="Pg"
                                    ItemStyle-Width="40" HeaderStyle-Width="40">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="tbPage" MaxLength="10" Text='<%# Bind("ItemBrief.Page") %>'>
                                        </asp:TextBox>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Status" SortExpression="StatusSortOrder" UniqueName="Status">
                                    <ItemStyle HorizontalAlign="Right" Width="95" />
                                    <HeaderStyle Width="95" HorizontalAlign="Center" />
                                    <ItemTemplate>
                                        <img runat="server" id="imgNoEstimatedCost"  class="WarningIconForFinance" title="Please check you've entered an estimated cost for each task." visible="false" src="~/Common/Images/NoExpendedCostWarning.png" />
                                        <asp:Literal ID="litStatus" Text='<%# Bind("Status") %>' runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridEditCommandColumn ButtonType="ImageButton" UniqueName="EditCommandColumn">
                                    <HeaderStyle Width="70" />
                                    <ItemStyle Width="70" HorizontalAlign="Center" CssClass="MyImageButton" />
                                </telerik:GridEditCommandColumn>
                            </Columns>
                        </MasterTableView>
                        <ClientSettings>
                            <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="300" SaveScrollPosition="True"></Scrolling>
                        </ClientSettings>
                    </telerik:RadGrid>
                    <div id="divThumbList" runat="server" style="height: 300px; overflow-y: auto;">
                        <asp:ListView ID="lvItemThumbList" OnItemDataBound="lvItemThumbList_ItemDataBound"
                            runat="server">
                            <LayoutTemplate>
                                <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                            </LayoutTemplate>
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkItemBrief" CssClass="thumbListItem" runat="server">
                                    <table>
                                        <tr>
                                            <td>
                                                <sb:ImageDisplay ID="itemBriefThumbDisplay" ShowImagePreview="false" runat="server" />
                                            </td>
                                        </tr>
                                    </table>
                                    <div>
                                        <%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("ItemBrief.Name") ,15) %>
                                    </div>
                                </asp:HyperLink>
                            </ItemTemplate>
                        </asp:ListView>
                    </div>
                </BodyContent>
            </sb:GroupBox>
        </ContentTemplate>
    </asp:UpdatePanel>
    <sb:PopupBox ID="popupItemBriefRemoved" Title="Item Type has been removed" Height="100"
        runat="server">
        <BodyContent>
            <div runat="server" id="divCompleteItemBriefRemove" style="white-space: nowrap;">
                You cannot add Item Briefs to this Item Type, because it has been removed.
            </div>
        </BodyContent>
        <BottomStripeContent>
            <asp:Button ID="btnConcurrentConfirmation" CssClass="ignoreDirtyFlag buttonStyle"
                OnClick="btnItemAlreadyRemoved_Click" runat="server" Text="Ok" />
        </BottomStripeContent>
    </sb:PopupBox>
</asp:Content>
