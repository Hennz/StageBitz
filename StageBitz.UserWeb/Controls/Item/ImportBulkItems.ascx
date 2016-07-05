<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportBulkItems.ascx.cs"
    Inherits="StageBitz.UserWeb.Item.ImportBulkItems" %>
<%@ Register Src="~/Controls/Company/PackageLimitsValidation.ascx" TagPrefix="sb" TagName="PackageLimitsValidation" %>
<%@ Register Src="~/Controls/Inventory/InventoryLocations.ascx" TagPrefix="sb" TagName="InventoryLocations" %>
<script type="text/javascript">
    function onFileUploaded(sender, args) {
        $("#<%= btnhidden.ClientID %>").click();
    }

    function CancelUploadProcess() {
        $find("<%= radUploader.ClientID %>").deleteAllFileInputs();
    }

    function fileUploaderValidationFailed(sender, eventArgs) {
        $("#<%= fileUploaderError.ClientID %>").show();
    }

</script>
<asp:Button ID="btnImportItem" runat="server" CssClass="buttonStyle" OnClick="btnImportItem_Click"
    CausesValidation="false" Text="Import from list" />
<asp:UpdatePanel ID="upnlpopupImportItems" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <sb:PopupBox ID="popupImportItems" ShowCornerCloseButton="false" Title="Import Inventory Items from list"
            runat="server">
            <BodyContent>
                <div style="width: 800px;">
                    <div class="divBoxProjectEvents" style="padding: 5px;">
                        You can import an existing database of Items directly into StageBitz. 
                        You will need to upload each Item Type separately.
                        <br />
                        Here's how you do it.
                    <ol>
                        <li style="float: left">Download a sample template from <a id="hlnkDownloadTemplate" href="~/Common/Templates/ImportFromItemListTemplate.csv"
                            runat="server">here</a>.&nbsp;
                        </li>
                        <div class="left">
                            <sb:HelpTip ID="HelpTip2" runat="server">
                                <b>Our template is a CSV file.</b>
                                <br />
                                This can be opened, edited and saved by most spreadsheet programs.
                            </sb:HelpTip>

                        </div>
                        <div style="clear: both;"></div>
                        <li style="float: left">Save your document as a CSV file. &nbsp;
                        </li>

                        <div style="float: left; margin-right: 2px; padding-top: 2px;">
                            <sb:HelpTip ID="HelpTip3" runat="server">
                                To get your file as a CSV, either:
                                <ul>
                                    <li>Reformat your existing document to match our template and save as a CSV file, or
                                    </li>
                                    <li>Copy and paste your data into our CSV template.
                                    </li>
                                </ul>

                                Please ensure that you have only one Item Type and one location per file (Locations are optional, so don't worry if you don't have that listed.)
                            That is, if your original file has both Props and Costumue listed as stored in two different locations, you may need four files, e.g.:
                                    <ul>
                                        <li>Props in Store A.csv
                                        </li>
                                        <li>Props in Store B.csv
                                        </li>
                                        <li>Costumes in Store A.csv
                                        </li>
                                        <li>Costumes in Store B.csv
                                        </li>
                                    </ul>

                            </sb:HelpTip>
                        </div>
                        <div style="clear: both;"></div>
                        <li>Upload each file below and follow the prompts. <%--You'll shown a preview of your list and asked to confirm the Item Type and
                             Location for the import(confirming a Location is optional)--%>
                        </li>
                    </ol>

                        <div style="min-width: 270px; border: 0px Solid Red;">
                            <div id="divFile" runat="server" style="float: left; padding-top: 2px;">
                                File: &nbsp;
                            </div>
                            <div style="float: left;">
                                <telerik:RadAsyncUpload ID="radUploader" OnClientValidationFailed="fileUploaderValidationFailed"
                                    OnClientFileUploaded="onFileUploaded" AllowedFileExtensions="csv" OnFileUploaded="radUploader_FileUploaded"
                                    Width="150" MaxFileInputsCount="1" runat="server">
                                </telerik:RadAsyncUpload>
                            </div>
                            <asp:LinkButton ID="lnkShowUpload" OnClick="lnkShowUpload_Click" Text="Upload a new csv file"
                                runat="server"></asp:LinkButton>
                        </div>
                        <%--      <div style="float: left; margin-right: 2px; padding-top: 2px;">
                        <sb:HelpTip ID="HelpTip1" runat="server">
                            Using Bulk Upload:
                            <ol>
                                <li>You can upload any list you have in CSV format, but it must have the same column
                                    headers as the sample template provided. Alternatively, you can download our template
                                    and use that. </li>
                                <li>You will be shown a preview of your file before it is imported.</li>
                                <li>Choose the Item Type you would like the list to be added to. The whole list will be added to the chosen Item Type so it is important to separate Props from Costumes etc.</li>
                                <li>Select 'Confirm' to create your Inventory Items.</li>
                            </ol>
                        </sb:HelpTip>
                        </div>--%>
                        <div style="clear: both;">
                        </div>
                        <div id="fileUploaderError" runat="server" class="inputError" style="display: none; margin: 5px 0px 10px 0px;">
                            Invalid file.
                        <div class="grayText">
                            <asp:Literal ID="ltrlUploadTips" runat="server"></asp:Literal>
                        </div>
                        </div>
                    </div>
                    <div id="divItemsGrid" style="padding-top: 5px;" runat="server">

                        <table>
                            <tr>
                                <td>Item Type
                                </td>
                                <td>
                                    :&nbsp;
                                    <asp:DropDownList runat="server" ID="ddlItemType" Width="200" Height="25" ValidationGroup="InventoryItemsBulkUploadFileds" AppendDataBoundItems="true">
                                        <asp:ListItem Selected="True" Text="-- Please Select --" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="rfvItemType" runat="server" ControlToValidate="ddlItemType" ErrorMessage="Please select an Item Type."
                                        ValidationGroup="InventoryItemsBulkUploadFileds"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td>Location
                                </td>
                                <td>
                                    <span class="left" style="position:relative; top:4px;">
                                        :&nbsp;
                                    </span>
                                    <div class="left">
                                        <sb:InventoryLocations runat="server" Width="200" InventoryLocationDisplayMode="Generic" ValidationGroup="InventoryItemsBulkUploadFileds" ID="bulkImportItemsInvLocation" AccessKey="S" DisableViewOnlyLocations="true" />
                                    </div>
                                    <div style="padding-left:2px; padding-top:2px;" class="left">
                                        <sb:HelpTip ID="HelpTip1" runat="server">
                                            If you need to add a new location, please do that via Manage Inventory>Locations.
                                        </sb:HelpTip>
                                    </div>
                                    <div style="clear: both;"></div>
                                </td>
                            </tr>
                        </table>

                        <telerik:RadGrid ID="gvItems" runat="server" Width="800" SortedBackColor="Transparent"
                            AllowAutomaticDeletes="True" AllowAutomaticInserts="false" AllowAutomaticUpdates="True"
                            AutoGenerateColumns="False">
                            <MasterTableView AllowNaturalSort="false" DataKeyNames="RowNumber">
                                <Columns>
                                    <telerik:GridBoundColumn HeaderText="Item Name" ItemStyle-Width="250" DataField="Item Name" ItemStyle-VerticalAlign="Top">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn ItemStyle-Width="250" HeaderText="Description" DataField="Description" ItemStyle-VerticalAlign="Top">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn HeaderText="Quantity" ItemStyle-Width="150" DataField="Quantity" ItemStyle-VerticalAlign="Top">
                                    </telerik:GridBoundColumn>
                                </Columns>
                            </MasterTableView>
                            <ClientSettings>
                                <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="200" SaveScrollPosition="True"></Scrolling>
                            </ClientSettings>
                        </telerik:RadGrid>
                    </div>
                    <div id="divMsg" style="margin-top: 5px;" class="message error" runat="server">
                    </div>
                    <sb:PopupBox ID="popupSuccess" Title="Success" runat="server">
                        <BodyContent>
                            <div id="divSuccessMsg" runat="server" style="width: 400px;">
                            </div>
                        </BodyContent>
                        <BottomStripeContent>
                            <asp:Button ID="btnDone" runat="server" Text="Done" CssClass="buttonStyle" OnClick="ClosePopUp" />
                        </BottomStripeContent>
                    </sb:PopupBox>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button ID="btnConfirm" Visible="false" runat="server" Text="Confirm Import"
                    CssClass="buttonStyle" OnClick="ConfirmImport" ValidationGroup="InventoryItemsBulkUploadFileds" />
                <input type="button" onclick="CancelUploadProcess" value="Cancel" class="popupBoxCloser buttonStyle" />
                <asp:Button ID="btnhidden" Style="display: none;" runat="server" />
            </BottomStripeContent>
        </sb:PopupBox>
        <sb:PackageLimitsValidation runat="server" ID="sbPackageLimitsValidation" />
    </ContentTemplate>
</asp:UpdatePanel>
