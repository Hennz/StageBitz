<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ItemAttachments.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Common.ItemAttachments" %>

<%@ Register Src="~/Controls/Common/FileUpload.ascx" TagName="FileUpload" TagPrefix="sb" %>
<%--<%@ Register Src="~/Controls/Common/AttachHyperlink.ascx" TagName="AttachHyperlink" TagPrefix="sb" %>--%>
<%@ Register Src="~/Controls/Common/DocumentPreview.ascx" TagName="DocumentPreview"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/AttachHyperlink.ascx" TagPrefix="sb" TagName="AttachHyperlink" %>
<sb:DocumentPreview ID="documentPreviewReadOnly" FunctionPrefix="ReadOnlyModeDP" IsReadOnly="true" IsTextboxsDisabled="true" runat="server" />
<sb:DocumentPreview ID="documentPreviewEditable" FunctionPrefix="EditModeModeDP" OnDocumentDeleted="documentPreview_DocumentDeleted" OnDocumentAttributesChanged="documentPreview_DocumentChanged" runat="server" />
<asp:UpdatePanel ID="upnlDocumentList" UpdateMode="Conditional" runat="server">
    <ContentTemplate>

        <div style="margin-top: 10px; padding-right: 10px;">
            <!--'ignoreDirtyFlag' css class will allow the button to postback
        without the browser warning message, even if the form is dirty.Same validation group is used in ItemBriefTask control.-->
            <div style="float: left; width: 850px;">
                <sb:FileUpload ID="fileUpload" OnFileUploaded="fileUpload_FileUploaded" Title="New Upload "
                    runat="server" />
                <sb:AttachHyperlink ID="newHyperLink" runat="server" OnItemChanged="newHyperLink_ItemChanged" />
                <asp:Button ID="btnAddLink" CssClass="buttonStyle" OnClick="btnAddLink_Click" runat="server" Text="Attach Hyperlink" />

            </div>
            <div style="display: inline-block; float: left; position: relative; top: 8px">
                <sb:HelpTip ID="HelpTip1" runat="server" Width="300">
                    You can upload any file type or attach a hyperlink to a website.
                </sb:HelpTip>

            </div>
        </div>
        <div style="clear: both; height: 5px;">
        </div>
        <div id="divNodivItemBriefAttachments" class="lightNotice" runat="server">
            <div style="text-align: center;">
                <h2>A picture is worth a thousand words!
                    <br />
                    Any good documentation can save hours of 'discussion'...
                </h2>
                <br />
            </div>
            <div style="text-align: left; padding-left: 230px;">
                Use the 'New Upload' button to attach all sorts of files to your
                <asp:Literal runat="server" ID="ltrItemTypeName"></asp:Literal>
                Brief.
                <br />
                <br />
                How about...
                <ul>
                    <li>Design sketches? </li>
                    <li>Technical drawings?</li>
                    <li>Reference materials - images, articles etc?</li>
                    <li>Instructions for use and maintenance (hand wash only!)?</li>
                    <li>A video of how the item gets used?</li>
                    <li>Photos of the Item preset, or packed into its storage?</li>
                    <li>Recipes for consumable items?</li>
                </ul>
                <br />
                The list is endless!
            </div>
        </div>
        <div id="divNoProjectAttachments" class="lightNotice" runat="server">
            <div style="text-align: center;">
                <h2>A picture is worth a thousand words!
                    <br />
                    Any good documentation can save hours of 'discussion'...
                </h2>
                <br />
            </div>
            <div style="text-align: left; padding-left: 230px;">
                Use the 'New Upload' button to attach all sorts of files about your
                <asp:Literal runat="server" ID="Literal1"></asp:Literal>
                Project.
                <br />
                <br />
                How about...
                <ul>
                    <li>Technical drawings?</li>
                    <li>Schedules?</li>
                    <li>Contact lists?</li>
                    <li>Reference materials - images,articles,etc?</li>
                    <li>Hyperlinks to web pages or files?</li>
                </ul>
                <br />
                The list is endless!
            </div>

        </div>
        <div id="divAttachments" runat="server" style="max-height: 280px; overflow-y: auto">

            <asp:ListView ID="lvAttachments" OnItemDataBound="lvAttachments_ItemDataBound" DataKeyNames="DocumentMediaId,ItemBriefItemDocumentMediaId" GroupItemCount="1"
                runat="server" ClientIDMode="AutoID">
                <LayoutTemplate>
                    <table>
                        <asp:PlaceHolder ID="groupPlaceholder" runat="server"></asp:PlaceHolder>
                    </table>
                </LayoutTemplate>
                <ItemTemplate>
                    <td>
                        <div id="divLinks" visible="false" runat="server">
                            <div style="width: 150px; padding-left: 30px;" class="left">
                                <asp:LinkButton ID="HyperLinkLinkButton" runat="server" OnClick="HyperLinkLinkButton_Click">
                                    <img src="../../Common/Images/FileTypes/hyperlink.png" runat="server" id="linkImage" style="width: 108px; height: 100px" />
                                </asp:LinkButton>
                            </div>
                            <div id="divLinkBlock" runat="server" style="width: 600px; padding-top: 20px;" class="left">
                                <a id="lblLinkURL" runat="server" target="_blank"></a>
                                <br />
                                <div id="extraLinkDetails" runat="server" style="font-size: 70%;">
                                    <asp:Label ID="lblHyper" runat="server"> Click on the link to open it or the icon to make changes.</asp:Label><br />
                                    <asp:Label ID="lblLinkedDate" runat="server"></asp:Label><br />
                                    <asp:Label ID="lblLinkedBy" runat="server"></asp:Label>
                                </div>
                                <div id="divDescription" runat="server" visible="false">
                                    <br />
                                    Click on the link to open it or the icon to make changes.
                                </div>
                            </div>
                            <div style="clear: both; height: 10px;">
                            </div>
                        </div>
                        <div id="divDocAttachments" runat="server">
                            <div style="width: 150px; padding-left: 30px;" class="left">
                                <asp:LinkButton ID="lnkbtnAttachment" runat="server">
                                    <sb:ImageDisplay ID="thumbAttachment" ShowImagePreview="true" runat="server" />
                                </asp:LinkButton>
                                <div>
                                </div>
                            </div>
                            <div style="width: 400px; padding-top: 10px;" class="left" id="divAttachmentBlock" runat="server">
                                <asp:Label ID="lblAttachmentName" runat="server"></asp:Label>
                                <br />
                                <span style="font-size: 70%;">
                                    <asp:Literal ID="litAttachmentType" runat="server"></asp:Literal>
                                    File<br />
                                </span>

                                <div id="extraDetails" runat="server" style="font-size: 70%;">
                                    <asp:Label ID="lblUploadedDate" runat="server"></asp:Label><br />
                                    <asp:Label ID="lblUploadedBy" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div style="width: 200px; padding-top: 10px;" class="left" id="divCheckInclude" runat="server">
                                <asp:CheckBox ID="chkInclude" Text="Include in Complete Item" OnCheckedChanged="chkInclude_OnCheckedChanged"
                                    AutoPostBack="true" runat="server" />
                            </div>
                            <div style="clear: both; height: 10px;">
                            </div>
                        </div>
                    </td>
                </ItemTemplate>
                <GroupTemplate>
                    <tr>
                        <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                    </tr>
                </GroupTemplate>
            </asp:ListView>
        </div>
        <div style="clear: both;">
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
