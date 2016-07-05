<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactInventoryManager.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Inventory.ContactInventoryManager" %>
<script type="text/javascript">
    var row;
    var isValid;
    function ShowDeletePopup(rowToDelete) {
        row = rowToDelete;
        showPopup('popupRemoveItems', 101);
        return false;
    }

    function ConfirmRemoveItem() {
        var hdnValue = row.find("input[id$='_hdnIsDeleted']");
        hdnValue.val("true");
        row.hide();
        

        
        var rows = $("[id$='_gvItemDetails'] tr");
        if (rows.length - 1 == 0) {
            $("#divNoData").show();
            $("#divItemsGrid").hide();

        }

        hidePopup('popupRemoveItems');
        return false;
    }

    function ValidateFields() {
        var isEmpty = true;
        var rows = $("[id$='_gvItemDetails'] tr:gt(0)");
        rows.each(function (index) {
            var $fieldset = $(this);
            
            
            
            var val = $.trim($('input:text:eq(0)', $fieldset).val());
            if (val != '') {
                isEmpty = false;
            }
        });
        
        
        
        var hasGeneralQuestionNotProvided = ($.trim($('#<%= txtGeneralQuestion.ClientID %>').val()) == 'Ask a general question...' || $.trim($('#<%= txtGeneralQuestion.ClientID %>').val()) == '');
   
        if (hasGeneralQuestionNotProvided && isEmpty) {
            showPopup("popupError");
            return false;
        }
        else {
            return true;
        }

    }

    function EnableButton() {
        $('input[type="submit"]').removeAttr('disabled');
        $('#<%= btnSendEmail.ClientID %>').removeAttr('title');
    }


    function EnableSendEmail() {
        $('#<%= btnSendEmail.ClientID %>').attr("title", "Please enter your message");
        $('#<%= btnSendEmail.ClientID %>').attr('disabled', 'disabled');

        $('input[type="text"], textarea').keypress(function (e) {
            EnableButton();
        });

        $('input[type="text"], textarea').change(function (e) {
            EnableButton();
        });
    }


</script>

<asp:UpdatePanel runat="server" ID="upnlContactIM">
    <ContentTemplate>
        <sb:PopupBox ID="popupEmailSent" Width="800" Title="Email Sent" runat="server">
            <BodyContent>
                Your email has been sent, and a copy will also be sent to you.
            </BodyContent>
            <BottomStripeContent>
                <input type="button" value="OK" id="removeCancel" class="popupBoxCloser buttonStyle" runat="server" />
            </BottomStripeContent>
        </sb:PopupBox>
              
        <sb:PopupBox ID="popupContactInventoryManager" Height="600" Width="800" Title="Contact Booking Manager" runat="server">
            <BodyContent>
                <sb:PopupBox ID="popupError" Width="800" Title="Error" runat="server">
            <BodyContent>
                Please enter your message.
            </BodyContent>
            <BottomStripeContent>
                <input type="button" value="OK" id="Button2" class="popupBoxCloser buttonStyle" runat="server" />
            </BottomStripeContent>
        </sb:PopupBox>

                <b>What do you want know?</b>
                <br />

                Send an email to the Booking Manager...
                 <br />
                <div class="left">
                    <asp:TextBox TextMode="MultiLine" ID="txtGeneralQuestion" Width="350" Height="50" runat="server"></asp:TextBox>
                    <asp:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender1" TargetControlID="txtGeneralQuestion"
                        WatermarkText="Ask a general question..." runat="server">
                    </asp:TextBoxWatermarkExtender>
                </div>
                <div class="left" style="padding-left: 10px;">
                    <table>
                        <tr>
                            <td colspan="2"> <b> Contact Details</b>
                            </td>

                        </tr>
                        <tr>
                            <td>Booking Manager
                            </td>
                            <td>
                                : <asp:Label ID="lblInventoryManager" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Company </td>
                            <td>
                                : <asp:Label ID="lblCompany" runat="server"></asp:Label></td>
                        </tr>
                    </table>
                </div>
                <div style="clear: both; height: 20px;">
                </div>
                <div id="divNoData" class="noData" style="text-align: center; vertical-align: central; min-height: 350px; display: none;">
                    No Data

                </div>
                <div id="divItemsGrid" style="overflow-y: auto; overflow-x: hidden; height: 350px;">
                    <asp:Panel ID="Panel1" runat="server" Width="100%" DefaultButton="btnDoNothing">
                    <asp:Button ID="btnDoNothing" runat="server" OnClientClick="return false;" Style="display: none;" />
                    <telerik:RadGrid ID="gvItemDetails" EnableLinqExpressions="False" AutoGenerateColumns="false" Width="800" Height="348"
                        AllowSorting="false" OnItemDataBound="gvItemDetails_ItemDataBound" AllowAutomaticDeletes="true"
                        runat="server">
                        <MasterTableView AllowNaturalSort="false" TableLayout="Fixed" AllowMultiColumnSorting="true" Width="800">
                            <NoRecordsTemplate>
                                <div class="noData">
                                    No data
                                </div>
                            </NoRecordsTemplate>
                            <Columns>
                                <telerik:GridBoundColumn UniqueName="ItemId" DataField="ItemId" Visible="false"></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn UniqueName="ItemFullName" DataField="Name" Visible="false"></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn UniqueName="Name" DataField="Name" HeaderText="Name" HeaderStyle-Width="100" ></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn UniqueName="Description" DataField="Description"  HeaderText="Description" HeaderStyle-Width="110"></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="Quantity"
                                    HeaderText="Qty" HeaderStyle-Width="30px">
                                </telerik:GridBoundColumn>
                                <telerik:GridTemplateColumn
                                    HeaderText="Ask a specific question..." HeaderStyle-Width="150px">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtQuestion" runat="server" Width="210"></asp:TextBox>
                                        <asp:HiddenField ID="hdnIsDeleted" Value="false" runat="server"></asp:HiddenField>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderStyle-Width="35px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <asp:ImageButton runat="server" ID="imgbtnCancel" ToolTip="Remove" OnClientClick="return ShowDeletePopup($(this.parentNode.parentNode));"
                                            ImageUrl="~/Common/Images/dialog_cancel.png" />
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                        <ClientSettings>
                            <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="350" SaveScrollPosition="True"></Scrolling>
                        </ClientSettings>
                    </telerik:RadGrid>
                        </asp:Panel>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button ID="btnSendEmail" runat="server" CssClass="buttonStyle" OnClientClick="return ValidateFields();" OnClick="SendEmail" Text="Send Email" />
          
            </BottomStripeContent>
        </sb:PopupBox>
        <sb:PopupBox ID="popupRemoveItems" runat="server" Title="Remove Items">
            <BodyContent>
                <div style="width: 300px;">
                    Are you sure you want to remove this Item?<br />
                </div>
            </BodyContent>
            <BottomStripeContent>
                <input type="button" class="buttonStyle" value="Yes" onclick="return ConfirmRemoveItem();" />
                <input type="button" value="No" id="Button1" class="popupBoxCloser buttonStyle" runat="server" />
            </BottomStripeContent>
        </sb:PopupBox>
    </ContentTemplate>
</asp:UpdatePanel>

