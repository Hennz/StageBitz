<%@ Page Language="C#" DisplayTitle="Pricing Plan History" AutoEventWireup="true" CodeBehind="PricingPlanHistory.aspx.cs" MasterPageFile="~/Content.master"
    Inherits="StageBitz.AdminWeb.Company.PricingPlanHistory" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="A1" href="~/Company/Companies.aspx" runat="server">Companies</a>
    |<a id="A2" href="~/Company/PricingPlanHistory.aspx" runat="server">Pricing Plan History</a>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <div class="right" style="margin-bottom: 5px; margin-right: 10px;">
                <asp:Button ID="btnSendToExcel" runat="server" CssClass="buttonStyle"
                    OnClick="btnSendToExcel_Click" Text="Send to Excel" />
            </div>
            <div style="clear: both;"></div>
    <asp:UpdatePanel runat="server" ID="upnlPricingHistory" UpdateMode="Conditional">
        <ContentTemplate>
            <sb:GroupBox ID="GroupBox" runat="server">
                <TitleLeftContent>
                    Pricing Plan History
                </TitleLeftContent>
                <TitleRightContent>
                    Show:
                 <div style="display: inline-block; width: 115px; margin-top: -10px; margin-bottom: -10px;">
                     <asp:DropDownList ID="ddlDisplayPricingPlans" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDisplayPricingPlans_SelectedIndexChanged">
                         <asp:ListItem runat="server" Value="All" Text="Show All"></asp:ListItem>
                         <asp:ListItem runat="server" Value="Invoice" Text="Invoice Only"></asp:ListItem>
                         <asp:ListItem runat="server" Value="CC" Text="Credit Card Only"></asp:ListItem>
                         <asp:ListItem runat="server" Value="Latest" Text="Latest Only"></asp:ListItem>
                     </asp:DropDownList>
                 </div>
                    <div style="clear: both;"></div>
                </TitleRightContent>
                <BodyContent>
                    <sb:SBRadGrid ID="gvPricingPlans" Width="100%" runat="server" OnSortCommand="gvPricingPlans_SortCommand"
                        OnItemDataBound="gvPricingPlans_ItemDataBound" AllowSorting="true" SortToolTip="Click to sort"
                        AllowPaging="true" SortedBackColor="Transparent" AllowAutomaticDeletes="True" HeaderStyle-VerticalAlign="Top"
                        AllowAutomaticInserts="false" OnNeedDataSource="gvPricingPlans_NeedDataSource" AutoGenerateColumns="False">
                        <SortingSettings SortToolTip="Click to sort"  EnableSkinSortStyles="true" />
                        <MasterTableView PageSize="50">
                            <NoRecordsTemplate>
                               <div class="noData">
                                    No data
                                </div>
                            </NoRecordsTemplate>
                            <PagerStyle AlwaysVisible="true" />
                            <SortExpressions>
                            <telerik:GridSortExpression FieldName="StartDate" SortOrder="Descending" />
                        </SortExpressions>
                            <Columns>
                                <telerik:GridTemplateColumn HeaderText="Company" HeaderStyle-Width="340" SortExpression="CompanyName">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="lnkCompany" NavigateUrl="#" runat="server"></asp:HyperLink>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                 <telerik:GridTemplateColumn HeaderText="Admin Name" HeaderStyle-Width="300" SortExpression="CompanyAdminName">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="lnkAdmin" NavigateUrl="#" runat="server"></asp:HyperLink>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn HeaderText="Project Level" HeaderStyle-Width="120"
                                    UniqueName="ProjectLevel" DataField="ProjectLevel">
                                </telerik:GridBoundColumn>
                                   <telerik:GridBoundColumn HeaderText="Inventory Level" HeaderStyle-Width="120"
                                    UniqueName="InventoryLevel" DataField="InventoryLevel">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Promotional Code" HeaderStyle-Width="90"
                                    UniqueName="PromotionalCode" DataField="PromotionalCode">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Education" HeaderStyle-Width="80"
                                    UniqueName="Educational" DataField="Educational">
                                </telerik:GridBoundColumn>
                                 <telerik:GridBoundColumn HeaderText="Period" HeaderStyle-Width="90"
                                    UniqueName="Period" DataField="Period">
                                </telerik:GridBoundColumn>
                                 <telerik:GridBoundColumn HeaderText="Start Date" HeaderStyle-Width="210"
                                    UniqueName="StartDate" DataField="StartDate">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Total Cost" HeaderStyle-Width="140"
                                 ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right"
                                    UniqueName="TotalCost" DataField="TotalCost">
                                </telerik:GridBoundColumn>
                                 <telerik:GridBoundColumn HeaderText="Payment method" HeaderStyle-Width="130"
                                    UniqueName="PaymentMethod" DataField="PaymentMethod">
                                </telerik:GridBoundColumn>
                            </Columns>
                        </MasterTableView>
                    </sb:SBRadGrid>
                </BodyContent>
            </sb:GroupBox>
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
