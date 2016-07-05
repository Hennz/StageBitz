<%@ Page DisplayTitle="Billing" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="CompanyFinancialDetails.aspx.cs" Inherits="StageBitz.UserWeb.Company.CompanyFinancialDetails" %>

<%@ Register Src="~/Controls/Common/SetupCreditCardDetails.ascx" TagName="SetUpCreditCardDetails"
    TagPrefix="uc" %>

<%@ Register Src="~/Controls/Company/PlanMonitor.ascx" TagName="PlanMonitor"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Company/CompanyWarningDisplay.ascx" TagPrefix="uc" TagName="CompanyWarningDisplay" %>
<%@ Register Src="~/Controls/Project/CreateNewProjectLink.ascx" TagPrefix="sb" TagName="CreateNewProjectLink" %>
<%@ Register Src="~/Controls/Company/PackageLimitsValidation.ascx" TagPrefix="sb" TagName="PackageLimitsValidation" %>
<%@ Register Src="~/Controls/Common/FutureRequestNotificationMessage.ascx" TagPrefix="sb" TagName="FutureRequestNotificationMessage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    <span runat="server" id="spnCreateNewProject">|
        <sb:CreateNewProjectLink runat="server" ID="lnkCreateNewProject" LinkText="Create New Project" />
    </span>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageTitleRight" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageContent" runat="server">
    <uc:CompanyWarningDisplay runat="server" ID="sbCompanyWarningDisplay" />
    <sb:FutureRequestNotificationMessage runat="server" ID="sbFutureRequestNotificationMessage" />
    <asp:UpdatePanel ID="upnlPaymentsDetails" runat="server">
        <ContentTemplate>
            <div>
                <div style="width: 560px; height: 100px; float: left;">
                    <div style="float: left; width: 100%;">
                        <asp:MultiView ID="multStatusCompany" ActiveViewIndex="0" runat="server">
                            <asp:View ID="view1" runat="server">
                                <asp:Literal ID="ltrlStatus" runat="server"></asp:Literal>
                            </asp:View>
                            <asp:View ID="view2" runat="server">
                                <asp:Image ID="Image1" ImageUrl="~/Common/Images/success.png" ToolTip="Active" runat="server" />
                            </asp:View>
                            <asp:View ID="view3" runat="server">
                                <div style="height: 10px;">
                                    <asp:Image ID="imgPaymentFailed" Style="float: left;" ImageUrl="~/Common/Images/error.png"
                                        runat="server" />
                                    <div style="float: left; margin-left: 3px; margin-top: 3px;">
                                        <%-- <strong style="color: Red;">Failed</strong> |--%>
                                        <asp:Literal ID="ltrlExpiresOn" runat="server"></asp:Literal>
                                        <asp:PlaceHolder ID="plcPayNow" runat="server">|
                                                    <asp:LinkButton ID="lnkbtnPayNow" OnClick="lnkbtnPayNow_Click" runat="server">Pay Now</asp:LinkButton>
                                        </asp:PlaceHolder>
                                    </div>
                                </div>
                            </asp:View>
                        </asp:MultiView>
                        <asp:Label ID="lblPaymentStatus" Visible="false" runat="server" CssClass="right"
                            Style="color: Gray; font-size: 85%; display: block; margin-top: 2px;"></asp:Label>
                    </div>
                    <div style="float: left; margin-right: 5px; margin-top: 10px;">
                        <b>Payment Details:</b>


                    </div>
                    <div style="float: left; width: 400px; margin-top: 10px;">
                        <uc:SetUpCreditCardDetails ID="setUpCreditCardDetails" OnPaymentDetailsUpdated="UpdateProjectStatusesAfterPaymentSetUp"
                            runat="server" />
                    </div>
                    <div style="float: left; margin-top: 10px;"> 
                        <b>Transaction History: &nbsp; </b>
                        <a id="lnkFinancialHistory" runat="server">View</a>
                    </div>
                    <div style="clear: both;"></div>
                    <div style="margin-top: 10px;" id="divSubscriptionPeriod" runat="server" visible="false">
                        <b>Subscription period:  &nbsp; </b>
                        <asp:Label ID="lblSubscriptionPeriod" runat="server"></asp:Label>
                    </div>
                    <div style="margin-top: 10px;" id="divRenewsOn" runat="server" visible="false">
                        <b>Renews on:  &nbsp; </b>
                        <asp:Label ID="lblRenewsOn" runat="server"></asp:Label>
                    </div>
                    <%--  <div style="clear: both;">--%>
                    <%--</div>--%>
                </div>
                <div style="width: 370px; float: left;">
                    <asp:UpdatePanel runat="server" ID="upnlPlanMonitor" UpdateMode="Conditional">
                        <ContentTemplate>
                            <uc:PlanMonitor ID="planMonitor" runat="server" />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div style="clear: both;">
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upnlProjects" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <sb:PopupBox ID="popupMakePayment" Title="Confirm Payment" runat="server">
                <BodyContent>
                    <div style="width: 300px;">
                        Are you sure you want to make the payment?
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnMakePayment" CssClass="buttonStyle" runat="server" Text="Yes"
                        OnCommand="MakePayment" />
                    <input type="button" class="popupBoxCloser buttonStyle" value="No" />

                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupSuspendReActivate" runat="server">
                <BodyContent>
                    <div id="divMsgSuspendReActivate" runat="server" style="width: 300px;">
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                    <asp:Button ID="btnSuspendReActivate" CssClass="buttonStyle" runat="server" Text="Confirm"
                        OnCommand="SuspendReActivate" />
                </BottomStripeContent>
            </sb:PopupBox>
            <sb:PopupBox ID="popupNotification" Title="Set up Payments" runat="server">
                <BodyContent>
                    <div style="min-width: 200px; max-width: 500px;">
                        <asp:Literal ID="ltrlNotification" runat="server"></asp:Literal>
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <input type="button" class="buttonStyle popupBoxCloser" value="OK" />
                </BottomStripeContent>
            </sb:PopupBox>
            <br />
            <sb:GroupBox ID="grpProjects" runat="server">
                <TitleLeftContent>
                    Projects
                </TitleLeftContent>
                <TitleRightContent>
                    Displaying:
                        <div style="display: inline-block; width: 115px; margin-top: -10px; margin-bottom: -10px;">
                            <asp:DropDownList ID="ddlDisplayProjects" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDisplayProjects_SelectedIndexChanged">
                                <asp:ListItem runat="server" Text="Active Projects"></asp:ListItem>
                                <asp:ListItem runat="server" Text="Closed Projects"></asp:ListItem>
                                <asp:ListItem runat="server" Text="All Projects"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    <div style="clear: both;"></div>
                </TitleRightContent>
                <BodyContent>
                    <sb:PackageLimitsValidation runat="server" ID="sbPackageLimitsValidation" />
                    <telerik:RadGrid ID="gvProjects" Width="100%" EnableLinqExpressions="False" AutoGenerateColumns="false"
                        AllowSorting="true" OnItemDataBound="gvProjects_ItemDataBound" OnSortCommand="gvProjects_SortCommand"
                        AllowAutomaticUpdates="True" OnNeedDataSource="gvProjects_NeedDataSource" OnItemCommand="gvProjects_ItemCommand"
                        runat="server">
                        <MasterTableView DataKeyNames="ProjectId" AllowNaturalSort="false" EditMode="InPlace"
                            TableLayout="Fixed">
                            <NoRecordsTemplate>
                                <div class="noData">
                                    There are no <%=ddlDisplayProjects.SelectedIndex==2?"Projects":ddlDisplayProjects.SelectedItem.Text %> for your Company
                                </div>
                            </NoRecordsTemplate>
                            <SortExpressions>
                                <telerik:GridSortExpression FieldName="ProjectName" SortOrder="Ascending" />
                                <telerik:GridSortExpression FieldName="SortOrder" SortOrder="Ascending" />
                            </SortExpressions>
                            <Columns>
                                <telerik:GridTemplateColumn UniqueName="ProjectName" SortExpression="ProjectName"
                                    HeaderText="Project Name" HeaderStyle-Width="200px">
                                    <ItemTemplate>
                                        <a id="lnkProject" runat="server" href="#"></a>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="ProjectStatus" HeaderStyle-Width="100px" SortExpression="ProjectStatus"
                                    HeaderText="Status">
                                    <ItemTemplate>
                                        <asp:MultiView ID="multStatus" ActiveViewIndex="0" runat="server">
                                            <asp:View ID="view1" runat="server">
                                                <asp:Literal ID="ltrlStatus" runat="server"></asp:Literal>
                                            </asp:View>
                                            <asp:View ID="view2" runat="server">
                                                <asp:Image ID="Image1" ImageUrl="~/Common/Images/success.png" ToolTip="Active" runat="server" />
                                            </asp:View>
                                            <asp:View ID="view3" runat="server">
                                                <div style="height: 10px;">
                                                    <div style="float: left; margin-left: 3px; margin-top: 3px;">
                                                        <%-- <strong style="color: Red;">Failed</strong> |--%>
                                                        <asp:Literal ID="ltrlExpiresOn" runat="server"></asp:Literal>
                                                    </div>
                                                </div>
                                            </asp:View>
                                        </asp:MultiView>
                                        <asp:Label ID="lblPaymentStatus" Visible="false" runat="server" CssClass="right"
                                            Style="color: Gray; font-size: 85%; display: block; margin-top: 2px;"></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="ProjectStatusAction" HeaderText="" HeaderStyle-Width="80px"
                                    HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkSuspend" Text="Suspend" Visible="false" CommandName="Suspend"
                                            runat="server"></asp:LinkButton>
                                        <asp:LinkButton ID="lnkReActivate" Text="Reactivate" Visible="false" CommandName="ReActivate"
                                            runat="server"></asp:LinkButton>
                                        <asp:Label ID="lblDiabledReActivate" Style="color: #999" Visible="false" Enabled="false" runat="server">
                                                Reactivate
                                        </asp:Label>
                                        <asp:Label ID="lblDisabledSuspend" Style="color: #999" Visible="false" Enabled="false" runat="server">
                                                Suspend
                                        </asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                    </telerik:RadGrid>
                </BodyContent>
            </sb:GroupBox>
            <br />
            <sb:GroupBox ID="grpInventory" runat="server">
                <TitleLeftContent>
                    Inventory
                </TitleLeftContent>
                <BodyContent>
                    <telerik:RadGrid ID="gvInventory" Width="100%" EnableLinqExpressions="false" AutoGenerateColumns="false" AllowSorting="true"
                        OnNeedDataSource="gvInventory_NeedDataSource" runat="server">
                        <MasterTableView AllowNaturalSort="false">
                            <NoRecordsTemplate>
                                <div class="noData">
                                    There are no Items in your companies Inventory
                                </div>
                            </NoRecordsTemplate>
                            <SortExpressions>
                                <telerik:GridSortExpression FieldName="ItemType" SortOrder="Ascending" />
                            </SortExpressions>
                            <Columns>
                                <telerik:GridBoundColumn HeaderText="ItemType" DataField="ItemType" UniqueName="ItemType"
                                    SortExpression="ItemType" HeaderStyle-Width="20%">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Number of Items" DataField="TotalItems" UniqueName="TotalItems"
                                    AllowSorting="false" HeaderStyle-Width="20%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Available" DataField="AvailableItems" UniqueName="AvailableItems"
                                    AllowSorting="false" HeaderStyle-Width="20%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Pinned" DataField="PinnedItems" UniqueName="PinnedItems"
                                    AllowSorting="false" HeaderStyle-Width="20%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="In Use" DataField="InUseItems" UniqueName="InUseItems"
                                    AllowSorting="false" HeaderStyle-Width="20%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                </telerik:GridBoundColumn>
                            </Columns>
                        </MasterTableView>
                    </telerik:RadGrid>
                </BodyContent>
            </sb:GroupBox>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
