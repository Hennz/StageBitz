<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportBulkItemBriefs.ascx.cs"
    Inherits="StageBitz.UserWeb.ItemBrief.ImportBulkItemBriefs" %>
<script type="text/javascript">
    function onFileUploaded(sender, args) {
        $("#<%= btnhidden.ClientID %>").click();
    }

    function CancelUploadProcess() {
        //        hidePopup("<%= this.ClientID %>popupImageUploader");
        $find("<%= radUploader.ClientID %>").deleteAllFileInputs();
    }

    function fileUploaderValidationFailed(sender, eventArgs) {
        $("#<%= fileUploaderError.ClientID %>").show();
    }

</script>
<asp:Button ID="btnImportItemBrief" runat="server" CssClass="buttonStyle" OnClick="btnImportItemBrief_Click"
    CausesValidation="false" Text="Import from list" />
<asp:UpdatePanel ID="upnlpopupImportItems" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <sb:PopupBox ID="popupImportItems" ShowCornerCloseButton="false" Title="Import from list"
            runat="server">
            <BodyContent>
                <div class="divBoxProjectEvents" style="padding: 5px;">
                    <div style="min-width: 270px; float: left; border: 0px Solid Red;">
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
                    <div style="float: left; margin-right: 2px; padding-top: 2px;">
                        <sb:HelpTip ID="HelpTip1" runat="server">
                            Using Bulk Upload:
                            <ol>
                                <li>You can upload any list you have in CSV format, but it must have the same column
                                    headers as the sample template provided. Alternatively, you can download our template
                                    and use that. </li>
                                <li>You will be shown a preview of your file before it is imported.</li>
                                <li><asp:Literal ID="lithelpTip" runat="server"></asp:Literal> </li>
                            </ol>
                        </sb:HelpTip>
                    </div>
                    <div style="float: left; vertical-align: middle; padding-top: 2px;">
                        <div style="float: left;">
                            You can download a sample template from <a id="hlnkDownloadTemplate" href="~/Common/Templates/ImportFromItemBriefListTemplate.csv"
                                runat="server">here</a>.
                        </div>
                    </div>
                    <div style="clear: both;">
                    </div>
                    <div id="fileUploaderError" runat="server" class="inputError" style="display: none;
                        margin: 5px 0px 10px 0px;">
                        Invalid file.
                        <div class="grayText">
                            <asp:Literal ID="ltrlUploadTips" runat="server"></asp:Literal>
                        </div>
                    </div>
                </div>
                <div id="divItemsGrid" style="padding-top: 5px;" runat="server">
                    <telerik:RadGrid ID="gvItems" runat="server" Width="850" SortedBackColor="Transparent"
                        AllowAutomaticDeletes="True" AllowAutomaticInserts="false" AllowAutomaticUpdates="True"
                        AutoGenerateColumns="False">
                        <MasterTableView AllowNaturalSort="false" DataKeyNames="RowNumber">
                            <Columns>
                                <telerik:GridBoundColumn HeaderText="Item Name" ItemStyle-Width="120" DataField="Item Name">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn ItemStyle-Width="120" HeaderText="Description" DataField="Description">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Quantity" ItemStyle-Width="50" DataField="Quantity">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Category" ItemStyle-Width="50" DataField="Category">
                                </telerik:GridBoundColumn>                                                                
                                <telerik:GridBoundColumn HeaderText="Character" ItemStyle-Width="50" DataField="Character">
                                </telerik:GridBoundColumn>                                 
                                <telerik:GridBoundColumn HeaderText="Preset" ItemStyle-Width="50" DataField="Preset">
                                </telerik:GridBoundColumn>                               
                                <telerik:GridBoundColumn HeaderText="Rehearsal" ItemStyle-Width="50" DataField="Rehearsal">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Act" ItemStyle-Width="50" DataField="Act">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Scene" ItemStyle-Width="50" DataField="Scene">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Page" ItemStyle-Width="50" DataField="Page">
                                </telerik:GridBoundColumn>
                            </Columns>
                        </MasterTableView>
                        <ClientSettings>
                            <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="200" SaveScrollPosition="True">
                            </Scrolling>
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
            </BodyContent>
            <BottomStripeContent>
                <asp:Button ID="btnConfirm" Visible="false" runat="server" Text="Confirm Import"
                    CssClass="buttonStyle" OnClick="ConfirmImport" />
                <input type="button" onclick="CancelUploadProcess" value="Cancel" class="popupBoxCloser buttonStyle" />
                <asp:Button ID="btnhidden" Style="display: none;" runat="server" />
            </BottomStripeContent>
        </sb:PopupBox>
    </ContentTemplate>
</asp:UpdatePanel>
