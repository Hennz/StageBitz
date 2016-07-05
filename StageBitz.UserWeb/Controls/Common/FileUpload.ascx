<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileUpload.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Common.FileUpload" %>

<%@ Register Src="~/Controls/Item/ItemDeletedWarning.ascx" TagName="ItemDeletedWarning" TagPrefix="sb" %>

<script type="text/javascript">

    //<!--Dynamic client ID is added to javascript function names to avoid conflicts
    //with other instaces of the FileUpload control on the same page.-->

    function <%= this.ClientID %>showFileUploader() {

        //Remove any previously uploaded file statuses
        $find("<%= radUploader.ClientID %>").deleteAllFileInputs();

        //Clear the File label textbox
        $('#<%= txtName.ClientID %>').val("");

        //Clear the accept status
        $("#<%= hdnAcceptStatus.ClientID %>").val("");

        showPopup('<%= this.ClientID %>popupFileUploader');
    }

    function <%= this.ClientID %>fileUploaderFileSelected(sender, args) {

        var txtName = $("#<%= txtName.ClientID %>");

        var fullname = args.get_fileName();
        var displayname = fullname;

        //If there's a file extension, get the name without it.
        if (fullname.indexOf('.') != -1) {
            displayname = fullname.substring(0, fullname.lastIndexOf('.'))
        }

        //Truncate the file name according to textbox max length.
        var maxlength = txtName.attr("maxlength");

        if (displayname.length > maxlength) {
            displayname = displayname.substring(0, maxlength);
        }

        txtName.val(displayname);

        $("#<%= fileUploaderError.ClientID %>").hide();
    }

    function <%= this.ClientID %>OnClientFileUploaded(sender, eventArgs) {

        $("#<%= btnOK.ClientID %>").removeAttr("disabled");
    }

    function <%= this.ClientID %>fileUploaderFileRemoved(sender, eventArgs) {

        $("#<%= txtName.ClientID %>").val("");

        $("#<%= fileUploaderError.ClientID %>").hide();
        $("#<%= btnOK.ClientID %>").attr("disabled", "disabled");
    }

    function <%= this.ClientID %>fileUploaderValidationFailed(sender, eventArgs) {

        $("#<%= fileUploaderError.ClientID %>").show();

        $("#<%= btnOK.ClientID %>").attr("disabled", "disabled");
    }

    function <%= this.ClientID %>setUploadAcceptedStatus() {

        //Set the hidden field value to indicate the server that the user have accepted the uploaded files.
        $("#<%= hdnAcceptStatus.ClientID %>").val("1");
    }

    function <%= this.ClientID %>hideUploader() {

        hidePopup("<%= this.ClientID %>popupFileUploader");
        $find("<%= radUploader.ClientID %>").deleteAllFileInputs();
    }

</script>

<asp:Button ID="btnUploadLaunch" runat="server" CssClass="buttonStyle" Text="Upload File" />
<asp:LinkButton ID="lnkbtnUploadLaunch" Visible="false" runat="server">Upload File</asp:LinkButton>

<asp:UpdatePanel ID="upnlUploader" UpdateMode="Conditional" runat="server">
    <ContentTemplate>

        <sb:PopupBox ID="popupFileUploader" runat="server">
            <BodyContent>

                <table>
                    <tr>
                        <td style="vertical-align:top;">
                            File:
                        </td>
                        <td>
                            <div  style="width:240px; overflow:hidden;">
                                <telerik:RadAsyncUpload ID="radUploader" OnFileUploaded="radUploader_FileUploaded" Width="240" MaxFileInputsCount="1" runat="server">
                                </telerik:RadAsyncUpload>   

                                <div id="fileUploaderError" runat="server" class="inputError" style="display:none; margin:5px 0px 10px 0px;padding:5px 5px;">
                                    <span style="font-weight:bold">Invalid file.</span>
                                    <div >
                                        <asp:Literal ID="ltrlUploadTips" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                        </td>
                    </tr>
                    <tr id="trDocumentLabel" runat="server">
                        <td>
                            Label:&nbsp;
                        </td>
                        <td>
                            <asp:TextBox ID="txtName" CssClass="" MaxLength="200" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                </table>

            </BodyContent>
            <BottomStripeContent>
                <asp:Button ID="btnOK" runat="server" disabled="disabled" CssClass="buttonStyle" OnClick="btnOK_Click" Text="OK" />
                <asp:Button ID="btnCancel" runat="server" class="buttonStyle" Text="Cancel" />
                <asp:HiddenField ID="hdnAcceptStatus" runat="server" />
            </BottomStripeContent>
        </sb:PopupBox>
        <sb:ItemDeletedWarning ID="popupItemDeletedWarning" runat="server"/>
    </ContentTemplate>
</asp:UpdatePanel>

