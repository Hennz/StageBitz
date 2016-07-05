<%@ Page DisplayTitle="Manage Discount Codes" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="ManageDiscountCodes.aspx.cs" Inherits="StageBitz.AdminWeb.Company.ManageDiscountCodes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function addDiscount_DuratonChanged() {
            var dateInput = $("#<%= lblweek.ClientID %>");
            var txtDuration = $("#<%= txtDuration.ClientID %>");

            if (txtDuration.val() == 1) {
                dateInput.html("(Week)");
            }
            else {
                dateInput.html("(Weeks)");
            }

            setGlobalDirty(true);
        }

        function durationOnValueChanged(sender, args) {
            setGlobalDirty(true);
        }

        function expireDate_onDateSelected(sender, args) {
            setGlobalDirty(true);
        }

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
            <div class="dirtyValidationArea">
                <asp:UpdatePanel ID="uplAddDisCode" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <sb:GroupBox ID="GroupBox1" runat="server">
                    <TitleLeftContent>
                        Add Discount Code
                    </TitleLeftContent>
                    <BodyContent>
                        <div style="line-height: 15px;">
                            <div class="left" style="width: 170px;">
                                Code:
                                <asp:TextBox ID="txtDiscountCode" Width="100" ValidationGroup="AddDiscountvalGroup"
                                    MaxLength="20" runat="server"></asp:TextBox>
                                <img id="imgDiscountCodeError" visible="false" style="margin-left: 2px;" runat="server"
                                    src="~/Common/Images/error - Small.png" />
                                <asp:RegularExpressionValidator ID="regExdDisCodeAdd" ValidationGroup="AddDiscountvalGroup"
                                    runat="server" ControlToValidate="txtDiscountCode" ErrorMessage=" *" ToolTip="Minimum length required is 4."
                                    ValidationExpression=".{4}.*" />
                                <asp:RequiredFieldValidator ID="RqdDisCodeAdd" ValidationGroup="AddDiscountvalGroup"
                                    ControlToValidate="txtDiscountCode" runat="server" ErrorMessage=" *" ToolTip="Code is required."></asp:RequiredFieldValidator>
                            </div>
                            <div class="left" style="width: 135px;padding-top: 5px;">
                                Discount:
                                <telerik:RadNumericTextBox SkinID="Percentage" ValidationGroup="AddDiscountvalGroup"
                                    MaxValue="100" runat="server" Width="60" ID="txtDiscountPercentage">
                                </telerik:RadNumericTextBox>
                                <asp:RequiredFieldValidator ID="RqdDisPercentageAdd" ControlToValidate="txtDiscountPercentage"
                                    ValidationGroup="AddDiscountvalGroup" runat="server" ErrorMessage=" *" ToolTip="Discount is required."></asp:RequiredFieldValidator>
                            </div>
                            <div class="left ignoreDirtyFlagHrefProcessing" style="width: 180px;padding-top: 5px;">
                                Duration:
                                <telerik:RadNumericTextBox SkinID="MinNumberOne" Value="1" ShowSpinButtons="true"
                                    ValidationGroup="AddDiscountvalGroup" runat="server" Width="60" ID="txtDuration">
                                    <ClientEvents OnValueChanged="addDiscount_DuratonChanged" />
                                    <IncrementSettings InterceptMouseWheel="false" />
                                </telerik:RadNumericTextBox>
                                <span id="lblweek" runat="server" style="font-size: 10px;">(Week)</span>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" ControlToValidate="txtDuration"
                                    ValidationGroup="AddDiscountvalGroup" runat="server" ErrorMessage="*" ToolTip="Duration is required."></asp:RequiredFieldValidator>
                                <img id="imgDurationError" visible="false" style="margin-left: 2px;" runat="server"
                                    src="~/Common/Images/error - Small.png" />
                            </div>
                            <div class="left" style="width: 220px;padding-top: 5px;">
                                <div class="left" style="padding-right:3px;">Expiry Date:</div>
                                <div style="margin-top:-2px; width:140px;" class="left">
                                    <telerik:RadDatePicker runat="server" Width="120px" ID="dtExpireDate" ClientEvents-OnDateSelected="expireDate_onDateSelected">
                                    </telerik:RadDatePicker>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" ControlToValidate="dtExpireDate"
                                        ValidationGroup="AddDiscountvalGroup" runat="server" ErrorMessage="*" ToolTip="Valid expiry date is required."></asp:RequiredFieldValidator>
                                    <div style="top: -20px; height: 10px; padding-top: 5px; float: right; position: relative; left: 1px;">
                                            &nbsp;
                                            <asp:Image ID="imgExpireDate" Visible="false" runat="server" ImageUrl="~/Common/Images/error - Small.png" />
                                    </div>
                                </div>
                            </div>
                            <div class="left ignoreDirtyFlagHrefProcessing" style="width: 160px;padding-top: 5px;">
                                Instance Count:
                                <telerik:RadNumericTextBox Type="Number" SkinID="MinNumberOne" Value="1" ShowSpinButtons="true"
                                    ValidationGroup="AddDiscountvalGroup" runat="server" Width="50" ID="txtInstanceCount">
                                    <NumberFormat AllowRounding="True" KeepNotRoundedValue="False"></NumberFormat>
                                     <ClientEvents OnValueChanged="addDiscount_DuratonChanged" />
                                </telerik:RadNumericTextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator5" ControlToValidate="txtInstanceCount"
                                    ValidationGroup="AddDiscountvalGroup" runat="server" ErrorMessage="*" ToolTip="Instance Count is required."></asp:RequiredFieldValidator>
                            </div>
                            <asp:Button ID="btnAdd" OnClick="AddDiscountCode" CssClass="buttonStyle" ValidationGroup="AddDiscountvalGroup"
                                runat="server" Text="Add" />
                        </div>
                        <div style="clear: both;"> 
                        </div>
                    </BodyContent>
                </sb:GroupBox>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <asp:UpdatePanel ID="uplAddDisList" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="dirtyValidationExclude" style="position:relative; right: -79%;">
                            <asp:CheckBox ID="chkHideExpiredCode" runat="server" AutoPostBack="true" 
                                Text="  Hide Expired Discount Codes" TextAlign="Right" Font-Bold="true" 
                                oncheckedchanged="chkHideExpiredCode_CheckedChanged" Checked="true"/>
                        </div>
                        <br />
                        <sb:PopupBox ID="popupDiscountUsage" runat="server">
                    <BodyContent>
                        <sb:SBRadGrid ID="gvDiscountUsage" Width="600" EnableLinqExpressions="False" AutoGenerateColumns="false"
                            AllowSorting="true" OnItemDataBound="gvDiscountCodeUsage_ItemDataBound" OnNeedDataSource="gvDiscountUsage_NeedDataSource"
                            runat="server" AllowPaging="true" ShowPageSizeComboBox="false">
                            <SortingSettings SortToolTip="Click to sort" EnableSkinSortStyles="true" />
                            <MasterTableView AllowNaturalSort="false" DataKeyNames="DiscountCodeUsageId" HeaderStyle-Height="20px"
                                TableLayout="Fixed" PageSize="20">
                                <SortExpressions>
                                    <telerik:GridSortExpression FieldName="CompanyName" SortOrder="Ascending"/>
                                </SortExpressions>
                                <PagerStyle AlwaysVisible="true"/>
                                <NoRecordsTemplate>
                                    <div class="noData">
                                        No data</div>
                                </NoRecordsTemplate>
                                <Columns>
                                    <telerik:GridTemplateColumn SortExpression="CompanyName" HeaderText="Company Name" UniqueName="CompanyName" HeaderStyle-Width="300">
                                        <ItemTemplate>
                                            <asp:HyperLink ID="lnkCompanyName" runat="server"></asp:HyperLink>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn HeaderText="Start Date" SortExpression="StartDate" DataField="StartDate">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn HeaderText="End Date" SortExpression="EndDate" DataField="EndDate">
                                    </telerik:GridBoundColumn>
                                </Columns>
                            </MasterTableView>
                        </sb:SBRadGrid>
                    </BodyContent>
                    <BottomStripeContent>
                        <input type="button" class="buttonStyle popupBoxCloser" value="OK" />
                    </BottomStripeContent>
                </sb:PopupBox>
                <telerik:RadWindowManager ID="mgr" runat="server">
                </telerik:RadWindowManager>
                <sb:SBRadGrid ID="gvDiscountCodes" Width="100%" EnableLinqExpressions="False" AutoGenerateColumns="false" OnCancelCommand="gvDiscountCodes_OnCancelCommand"
                    AllowSorting="true" OnItemDataBound="gvDiscountCodes_ItemDataBound" AllowPaging="true"
                    AllowAutomaticDeletes="True" AllowAutomaticUpdates="true" OnDeleteCommand="gvDiscountCodes_ItemDeleted"
                    OnNeedDataSource="gvDiscountCodes_NeedDataSource" OnUpdateCommand="gvDiscountCodes_UpdateCommand"
                    OnItemCommand="gvDiscountCodes_ItemCommand" runat="server">
                    <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                    <MasterTableView DataKeyNames="DiscountCode.DiscountCodeID,DiscountCode.LastUpdatedDate" AllowNaturalSort="false"
                        EditMode="InPlace" TableLayout="Fixed">
                        <SortExpressions>
                              <telerik:GridSortExpression FieldName="DiscountCode.CreatedDate" SortOrder="Descending"/>
                        </SortExpressions>
                        <PagerStyle AlwaysVisible="true" />
                        <NoRecordsTemplate>
                            <div class="noData">
                                No data</div>
                        </NoRecordsTemplate>
                        <Columns>
                            <telerik:GridTemplateColumn SortExpression="DiscountCode.Code" HeaderText="Code" UniqueName="Code">
                                <ItemStyle HorizontalAlign="Center" Width="110" />
                                <ItemTemplate>
                                    <%--<asp:Image ID="imgDiscountCodeExpired" ToolTip="Code is expired" Visible="false"
                                    Style="margin-left: 2px;" runat="server" ImageUrl="~/Common/Images/warning.gif" />--%>
                                    <asp:LinkButton ID="lnkDiscountCode" Visible="false" CommandName="ViewDiscountUsage"
                                        runat="server"></asp:LinkButton>
                                    <asp:Label ID="litDiscountCode" Visible="false" runat="server"></asp:Label>
                                </ItemTemplate>
                                <HeaderStyle HorizontalAlign="Center" Width="110" />
                                <EditItemTemplate>
                                    <div class="left" style="padding-left: 15px;">
                                        <asp:TextBox runat="server" Width="75" MaxLength="20" ValidationGroup="valGroup"
                                            ID="txtDiscountCode">                                
                                        </asp:TextBox>
                                        <asp:Label ID="litDiscountCode" Visible="false" runat="server"></asp:Label>
                                        <asp:RegularExpressionValidator ID="regExMinLengthEdit" ValidationGroup="valGroup"
                                            runat="server" ControlToValidate="txtDiscountCode" ErrorMessage="*" ToolTip="Minimum length required is 4"
                                            ValidationExpression=".{4}.*" />
                                        <asp:RequiredFieldValidator ID="rqdCodeEditMode" ValidationGroup="valGroup" ControlToValidate="txtDiscountCode"
                                            runat="server" ErrorMessage="*" ToolTip="Discount Code is required"></asp:RequiredFieldValidator>
                                        <div style="clear: both;">
                                        </div>
                                    </div>
                                    <div class="left" style="padding-top: 4px;">
                                        &nbsp;
                                        <asp:Image ID="imgDiscountCodeError" Visible="false" runat="server" ImageUrl="~/Common/Images/error - Small.png" />
                                    </div>
                                </EditItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn SortExpression="DiscountCode.Discount" HeaderText="Discount" UniqueName="Discount">
                                <ItemStyle HorizontalAlign="Center" Width="90" />
                                <ItemTemplate>
                                </ItemTemplate>
                                <HeaderStyle HorizontalAlign="Center" Width="90" />
                                <EditItemTemplate>
                                    <telerik:RadNumericTextBox SkinID="Percentage" runat="server" ValidationGroup="valGroup"
                                        Width="58" ID="txtDiscountPercentage">
                                        <NumberFormat AllowRounding="True" KeepNotRoundedValue="False"></NumberFormat>
                                    </telerik:RadNumericTextBox>
                                    <asp:Literal ID="litDiscountPercentage" Visible="false" runat="server"></asp:Literal>
                                    <asp:RequiredFieldValidator ID="rqdDiscountEdit" ControlToValidate="txtDiscountPercentage"
                                        ValidationGroup="valGroup" runat="server" ErrorMessage="*" ToolTip="Discount is required"></asp:RequiredFieldValidator>
                                </EditItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn SortExpression="DiscountCode.Duration" HeaderText="Duration" UniqueName="Duration">
                                <ItemStyle HorizontalAlign="Center" Width="100" />
                                <ItemTemplate>
                                </ItemTemplate>
                                <HeaderStyle HorizontalAlign="Center" Width="100" />
                                <EditItemTemplate>
                                    <div class="left ignoreDirtyFlagHrefProcessing" style="width: 90px; padding-left: 20px;">
                                        <telerik:RadNumericTextBox SkinID="MinNumberOne" MaxValue="100" runat="server" ShowSpinButtons="true"
                                            ValidationGroup="valGroup" Width="60" ID="txtDuration" ClientEvents-OnValueChanged="durationOnValueChanged">
                                            <NumberFormat AllowRounding="True" KeepNotRoundedValue="False"></NumberFormat>
                                        </telerik:RadNumericTextBox>
                                        <asp:Literal ID="litDuration" Visible="false" runat="server"></asp:Literal>
                                        <asp:RequiredFieldValidator ID="rqdDurationEdit" ControlToValidate="txtDuration"
                                            ValidationGroup="valGroup" runat="server" ErrorMessage="*" ToolTip="Duration is required."></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="left" style="width: 10px;">
                                        <asp:Image ToolTip="Invalid duration" ID="imgDurationError" Visible="false" runat="server"
                                            ImageUrl="~/Common/Images/error - Small.png" />
                                    </div>
                                </EditItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn SortExpression="DiscountCode.CreatedDate" DataType="System.DateTime" HeaderText="Created Date"
                                UniqueName="CreatedDate">
                                <ItemStyle HorizontalAlign="Center" Width="140" />
                                <ItemTemplate>
                                </ItemTemplate>
                                <HeaderStyle HorizontalAlign="Center" Width="140" />
                                <EditItemTemplate>
                                    <div>
                                        <asp:Literal ID="litCreatedDate" runat="server"></asp:Literal>
                                    </div>
                                </EditItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn SortExpression="DiscountCode.ExpireDate" DataType="System.DateTime" HeaderText="Expiry Date"
                                UniqueName="ExpireDate">
                                <ItemStyle HorizontalAlign="Center" Width="130" />
                                <ItemTemplate>
                                </ItemTemplate>
                                <HeaderStyle HorizontalAlign="Center" Width="130" />
                                <EditItemTemplate>
                                    <div style="width: 120px; height: 19px; position: relative; top: 5px;">
                                        <div style="width:120px">
                                            <telerik:RadDatePicker runat="server" ValidationGroup="valGroup" Width="150" ID="dtExpireDate"  ClientEvents-OnDateSelected="expireDate_onDateSelected">
                                            </telerik:RadDatePicker>
                                            <asp:Literal ID="litExpireDate" Visible="false" runat="server"></asp:Literal>
                                        </div>
                                        <div style="float: right; width: 2px; top: -20px; position: relative; height:2px;">
                                            <asp:RequiredFieldValidator ID="rqdExpireEdit" ValidationGroup="valGroup" ControlToValidate="dtExpireDate"
                                                runat="server" ErrorMessage="*" ToolTip="Valid expiry date is required"></asp:RequiredFieldValidator>
                                        </div>
                                        <div style="top: -20px; height: 10px; padding-top: 5px; float: right; position: relative; left: 15px;">
                                            &nbsp;
                                            <asp:Image ID="imgExpireDate" Visible="false" runat="server" ImageUrl="~/Common/Images/error - Small.png" />
                                        </div>
                                    </div>
                                </EditItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn SortExpression="DiscountCode.InstanceCount" HeaderText="Instance Count"
                                UniqueName="InstanceCount">
                                <ItemStyle HorizontalAlign="Center" Width="100" />
                                <ItemTemplate>
                                </ItemTemplate>
                                <HeaderStyle HorizontalAlign="Center" Width="100" />
                                <EditItemTemplate>
                                    <div class="left ignoreDirtyFlagHrefProcessing" style="width: 70px; padding-left: 30px;">
                                        <telerik:RadNumericTextBox Type="Percent" SkinID="MinNumberOne" MaxValue="100" runat="server"
                                            ValidationGroup="valGroup" ShowSpinButtons="true" Width="50" ID="txtInstanceCount">
                                            <NumberFormat AllowRounding="True" KeepNotRoundedValue="False"></NumberFormat>
                                             <ClientEvents OnValueChanged="addDiscount_DuratonChanged" />
                                        </telerik:RadNumericTextBox>
                                        <asp:RequiredFieldValidator ID="rqdInstanceCountEdit" ControlToValidate="txtInstanceCount"
                                            ValidationGroup="valGroup" runat="server" ErrorMessage="*" ToolTip="Instance count is required."></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="left" style="width: 0px;">
                                        <asp:Image ID="imgInstanceCountError" Visible="false" runat="server" ImageUrl="~/Common/Images/error - Small.png" />
                                    </div>
                                </EditItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridEditCommandColumn ButtonType="ImageButton" UniqueName="EditCommandColumn">
                                <ItemStyle Width="60" HorizontalAlign="Center" CssClass="MyImageButton" />
                                <HeaderStyle Width="60" />
                            </telerik:GridEditCommandColumn>
                            <telerik:GridButtonColumn ConfirmDialogType="RadWindow" ConfirmTitle="Delete" ButtonType="ImageButton"
                                ConfirmText="Are you sure you want to delete this discount code?" ConfirmDialogHeight="100"
                                CommandName="Delete" Text="Delete" UniqueName="DeleteColumn">
                                <ItemStyle HorizontalAlign="Center" CssClass="MyImageButton" />
                                <ItemStyle Width="30" />
                                <HeaderStyle Width="30" />
                            </telerik:GridButtonColumn>
                        </Columns>
                    </MasterTableView>
                    <ValidationSettings ValidationGroup="valGroup" />

                </sb:SBRadGrid>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
</asp:Content>
