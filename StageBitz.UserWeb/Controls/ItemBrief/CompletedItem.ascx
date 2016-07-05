<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompletedItem.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.ItemBrief.CompletedItem" %>
<%@ Register Src="~/Controls/Common/FileUpload.ascx" TagName="FileUpload" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/DocumentList.ascx" TagName="DocumentList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/DocumentPreview.ascx" TagName="DocumentPreview"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Item/CompleteItem.ascx" TagName="CompleteItem" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Item/CompleteItemHeader.ascx" TagName="CompleteItemHeader"
    TagPrefix="sb" %>

<script type="text/javascript">
    function btnConfirmDetails_Click() {
        $(document).trigger('onCompleteItemPopupConfirmed', ['<%=popupItemComplete.ClientID%>']);
    };

    function btnConfirmCompleteItemBrief_Click() {
        $(document).trigger('onConfirmSaveItemBriefBeforeShowCompleteItemPopup', ['<%= popupConfirmCompleteItemBrief.ClientID%>', '<%=popupItemComplete.ClientID%>']);
    };

    function btnCancelConfirmCompleteItemBrief_Click() {
        $(document).trigger('onCancelConfirmSaveItemBriefBeforeShowCompleteItemPopup', ['<%= popupConfirmCompleteItemBrief.ClientID%>', '<%=popupItemComplete.ClientID%>']);
    };

    function btnSaveItemTab_Click() {
        $(document).trigger('onConfirmSaveItemTabBeforeShowCompleteItemPopup', ['<%= popupConfirmSaveItemTab.ClientID%>', '<%=popupItemComplete.ClientID%>']);
    };

    function btnCancelSaveItemTab_Click() {
        $(document).trigger('onCancelSaveItemTabBeforeShowCompleteItemPopup', ['<%= popupConfirmSaveItemTab.ClientID%>', '<%=popupItemComplete.ClientID%>']);
    };

    function ShowBookingOverlapContactIMPopup(message) {
        $('#<%=lblBookingOverlapContactIM.ClientID%>').text(message);
        if (!$('#<%=txtContactIMEmailBody.ClientID%>').data('jqte')) {
            $('#<%=txtContactIMEmailBody.ClientID%>').jqte({ source: false, link: false, unlink: false });
        }

        $('#<%=txtContactIMEmailBody.ClientID%>').jqteVal('');
        $("#<%= reqContactIMEmailBody.ClientID %>").css("display", "none");

        $('#<%=divContactIM.ClientID%>').unbind('keypress.ContactIM');
        $('#<%=divContactIM.ClientID%>').bind('keypress.ContactIM', function (e) {
            e.stopPropagation();
        });

        showPopup('popupBookingOverlapContactInventoryManager');
    }

    function EncodeBookingOverlapContactIMContent() {
        $('#<%=txtContactIMEmailBody.ClientID%>').val($('<div/>').text($('#<%=txtContactIMEmailBody.ClientID%>').val()).html());
    }
</script>
<asp:UpdatePanel runat="server" ID="uplCompleteItemBlankNotice" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Literal ID="litForNotYetKeptItem" runat="server"></asp:Literal>
        <div id="divBlankNotice" class="lightNotice" runat="server">


            <div style="text-align: center;">
                <h2>Is this Item ready to go?</h2>
            </div>
            <div style="padding-right: 150px; padding-left: 180px;">
                Once you're happy this Item is complete and ready for use, just select the button
                <br />
                below.
                <br />
                <br />
                This will:
                <br />
                <ul>
                    <li>Change its Status to Complete</li>
                    <li runat="server" id="liInventoryLimitReachedMsg">Create or update its listing in the Company Inventory</li>
                </ul>
                If something changes later (as it often does!) and you need to make changes to it,
                <br />
                the status will automatically change back to 'In Progress' when you add a new Task.
            </div>
            <div style="padding-top: 25px; text-align: center;">
                <asp:Button Text="I'm ready to complete this Item!" CssClass="buttonStyle" Style="float: none;"
                    OnClick="btnVerifyBeforeComplete_Click" runat="server" ID="btnVerifyBeforeComplete" />
            </div>

        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:Panel ID="pnlCompletedItemTab" runat="server" Width="900">
    <p>
        <asp:UpdatePanel runat="server" ID="uplCompleteItemTabContent" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Literal runat="server" ID="litNormalText"></asp:Literal>
                <div runat="server" style="font-style: italic; font-weight: bold;" visible="false" id="divOriginalVersionText">
                    This is a Historical Snapshot of the 
                    <asp:Label ID="lblItemName" runat="server" />
                    <asp:HyperLink ID="lnkItemName" Target="_blank" runat="server"></asp:HyperLink>

                    <asp:Literal ID="litProjCloseDate" runat="server"></asp:Literal>

                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </p>
    <br />
    <div class="dirtyValidationExclude" id="itemCompleteTabDiv">
        <sb:CompleteItemHeader runat="server" ID="cihItemCompleteTab" DisplayMode="ItemBriefDetails" />
        <br style="clear: both" />
        <sb:CompleteItem runat="server" ID="ciItemCompleteTab" OnCompleteItemDocumentListDocumentChanged="ciItemCompleteTab_CompleteItemDocumentListDocumentChanged" DisplayMode="ItemBriefDetails" />
        <br style="clear: both" />
    </div>
    <asp:UpdatePanel runat="server" ID="uplCompleteItemTabButton" UpdateMode="Conditional">
        <ContentTemplate>
            <div style="position: relative; bottom: 8px; right: 15px; margin-bottom: 10px;">
                <asp:Button runat="server" ID="btnCompleteItem" Text="Complete and update Item"
                    CssClass="buttonStyle" OnClick="btnCompleteItem_Click" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>

<div>
    <sb:PopupBox ID="popupItemComplete" Title="Complete Item" runat="server" Width="800">
        <BodyContent>
            <div style="padding-left: 13px;">
                <h2>Is this right?</h2>
                <asp:UpdatePanel runat="server" ID="uplCompleteItemPopupContent" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div style="width: 800px;">
                            <div runat="server" id="divInventoryLimitNotReached">
                                <p>
                                    What you see here is how this Item will appear in your Company Inventory, so take
                                a few seconds to decide which files should be attached or
                                improve the description to make your life easier when you need to find it next time!
                                </p>
                            </div>
                            <div runat="server" id="divInventoryLimitReached">
                                <p>
                                    You've reached the Item limit for your Inventory subscription. We'll keep a record of this Item, but you'll need to upgrade your subscription or reduce your current number of Items to be able to view it.
                                     What you see 
                            here is how this Item will appear so take a few seconds to check the details to make sure you have an accurate record for the future.                                    
                                </p>
                            </div>

                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div style="width: 800px; padding-top: 10px;" class="dirtyValidationExclude" id="itemCompletePopupDiv">
                    <sb:CompleteItemHeader runat="server" ID="cihItemCompletePopup" DisplayMode="ItemBriefDetails" />
                    <br style="clear: both" />
                    <div style="max-height: 300px; overflow-y: scroll;">
                        <sb:CompleteItem runat="server" ID="ciItemCompletePopup" OnCompleteItemDocumentListDocumentChanged="ciItemCompletePopup_CompleteItemDocumentListDocumentChanged" DisplayMode="ItemBriefDetails" />
                    </div>
                    <br style="clear: both" />
                </div>
            </div>
        </BodyContent>
        <BottomStripeContent>
            <div style="padding-right: 15px;">
                <asp:Button ID="btnConfirmDetails" CssClass="buttonStyle" runat="server" Text="Confirm Details" OnClientClick="btnConfirmDetails_Click(); return false;" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </div>
        </BottomStripeContent>
    </sb:PopupBox>
    <asp:UpdatePanel runat="server" ID="uplCompleteItemPopups" UpdateMode="Conditional">
        <ContentTemplate>
            <sb:PopupBox ID="popupErrorItemNotKept" Title="Complete Item" runat="server">
                <BodyContent>
                    It's decision time... Before you can complete this you'll need to confirm if you
                    wish to use an Item
                    <br />
                    booked from the Inventory that is currently showing on the Pinboard Tab.
                    
                    Once you've done that the<br />
                    details will appear here for you to check.
                </BodyContent>
                <BottomStripeContent>
                    <div style="padding-right: 15px;">
                        <input type="button" class="popupBoxCloser buttonStyle" value="OK" />
                    </div>
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupConfirmCompleteItemBrief" runat="server" Title="Save unsaved changes">
                <BodyContent>
                    <div style="width: 300px;">
                        You have unsaved changes in this page. Do you want to save them and proceed?<br />
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnCancelConfirmCompleteItemBrief" CssClass="buttonStyle" runat="server"
                        Text="No" OnClientClick="btnCancelConfirmCompleteItemBrief_Click();return false;" />
                    <asp:Button ID="btnConfirmCompleteItemBrief" CssClass="buttonStyle" runat="server"
                        Text="Yes" OnClientClick="btnConfirmCompleteItemBrief_Click();return false;" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupConfirmSaveItemTab" runat="server" Title="Save unsaved changes">
                <BodyContent>
                    <div style="width: 300px;">
                        You have unsaved changes in this Item. Do you want to save them and proceed?<br />
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnCancelSaveItemTab" CssClass="buttonStyle" runat="server" Text="No"
                        OnClientClick="btnCancelSaveItemTab_Click(); return false;" />
                    <asp:Button ID="btnSaveItemTab" CssClass="buttonStyle" runat="server" Text="Yes"
                        OnClientClick="btnSaveItemTab_Click(); return false;" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupBookingOverlapContactBookingManager" runat="server" Title="Email the Booking Manager about changes made to this Item?" ValidateRequestMode="Disabled">
                <BodyContent>
                    <div style="width: 650px;">
                        <p>
                            <asp:Label runat="server" ID="lblBookingOverlapContactIM"></asp:Label>
                        </p>
                        <div class="HtmlEditorWrap dirtyValidationExclude" id="divContactIM" runat="server">
                            <asp:TextBox TextMode="MultiLine" runat="server" ID="txtContactIMEmailBody" ValidateRequestMode="Disabled" ValidationGroup="ContactIMEmail"></asp:TextBox>
                        </div>
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:RequiredFieldValidator ID="reqContactIMEmailBody" runat="server" ControlToValidate="txtContactIMEmailBody"
                                ValidationGroup="ContactIMEmail" ErrorMessage="Please enter your changes."></asp:RequiredFieldValidator>
                    <asp:Button ID="btnSendEmailToBookingManager" CssClass="buttonStyle" runat="server" Text="Send"
                        OnClick="btnSendEmailToBookingManager_Click" OnClientClick="EncodeBookingOverlapContactIMContent()" ValidationGroup="ContactIMEmail" />
                    <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                </BottomStripeContent>
            </sb:PopupBox>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
