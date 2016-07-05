<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExportFiles.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.ExportFiles" %>
<sb:GroupBox ID="GroupBox1" runat="server">
    <TitleLeftContent>
        Export files from...
    </TitleLeftContent>
    <BodyContent>
        <p><b>Create a zipped folder of all the export data and attached files for each Company Inventory or Inventory listed below.</b></p>

        <ul>
            <li>If you have a lot of information to export, it may take some time to create the zipped folders.
            </li>
            <li>Folders will be removed after 
                <asp:Literal ID="litDays" runat="server"></asp:Literal>
                days to save storage space.
            </li>
        </ul>

        <asp:Timer ID="Timer1" runat="server" Interval="30000" OnTick="Timer1_Tick">
        </asp:Timer>
        <asp:UpdatePanel ID="upnl" runat="server">
            <ContentTemplate>
                <sb:PopupBox Title="Remove file" ID="popDeleteConfirm" runat="server">
                    <BodyContent>
                        Are you sure you want to remove this zipped file?
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="btnRemoveFile" CssClass="buttonStyle" runat="server" Text="Yes" OnClick="btnRemoveFile_Click" />
                        <input type="button" class="ignoreDirtyFlag popupBoxCloser buttonStyle" value="No" />
                    </BottomStripeContent>
                </sb:PopupBox>
                <telerik:RadGrid ID="gvExportFiles" OnItemDataBound="gvExportFiles_ItemDataBound" Width="850" OnItemCommand="gvExportFiles_ItemCommand" AutoGenerateColumns="false" ShowHeader="false" runat="server">
                    <MasterTableView>
                        <NoRecordsTemplate>
                            <div class="noData">
                                No data to display.
                            </div>
                        </NoRecordsTemplate>
                        <Columns>

                            <telerik:GridTemplateColumn UniqueName="EntityName" HeaderText="EntityName"
                                HeaderStyle-Width="300px">
                                <ItemTemplate>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>

                            <telerik:GridTemplateColumn>
                                <ItemTemplate>
                                    <asp:Button ID="lnkGenerate" Style="float: left;" CssClass="buttonStyle" CommandName="GenerateExportFile" runat="server" Enabled='<%# (int)Eval("ExportFileId") == 0 %>' Text="Generate"></asp:Button>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderStyle-Width="850" ItemStyle-Height="27" HeaderText="">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hdnExportFileId" Value='<%# Eval("ExportFileId") %>' runat="server" />
                                    <asp:HiddenField ID="hdnRelatedTable" Value='<%# Eval("RelatedTable") %>' runat="server" />
                                    <asp:HiddenField ID="hdnRelatedId" Value='<%# (int)Eval("RelatedId") %>' runat="server" />
                                    <asp:Literal ID="litStatus" runat="server" Visible='<%# (int)Eval("ExportFileStatusCodeId") == queuedStatusCodeId %>'>
                                       Generating now <span style="font-size: smaller;">(but don't worry - you can close this screen and it will still keep working!)</span>
                                         <i class="fa fa-spinner fa-spin"></i>
                                    </asp:Literal>

                                    <div runat="server" visible='<%# (int)Eval("ExportFileStatusCodeId") == completedStatusCodeId  %>'>
                                        <i class="fa fa-folder-open fa-lg"></i>
                                        <asp:Literal ID="litFileSize" runat="server" Text='<%#string.Concat( (string)Eval("RelatedTable") == "Company" ? " Company Inventory Information Zipped Folder - " :
                                        " Project Information Zipped Folder - ", String.Format(new StageBitz.Common.FileSizeFormatProvider(), "{0:fs}", (double)Eval("FileSize") ," KB"))  %>'></asp:Literal>
                                        &nbsp;&nbsp;&nbsp;&nbsp;
                                        <asp:LinkButton ID="lnkDownload" CommandName="DownLoad" runat="server" Font-Underline="false" ToolTip="Download" Text="Download">
                                        <i class="fa fa-download fa-lg"></i>
                                        </asp:LinkButton>
                                        &nbsp;
                                        <asp:LinkButton ID="lnkDelete" CommandName="Delete" runat="server" Font-Underline="false" ToolTip="Delete" Text="Delete">
                                        <i class=" fa fa fa-trash-o fa-lg"></i>
                                        </asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
            </Triggers>
        </asp:UpdatePanel>
    </BodyContent>
</sb:GroupBox>
