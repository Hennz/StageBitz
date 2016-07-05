<%@ Page DisplayTitle="Companies" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="Companies.aspx.cs" Inherits="StageBitz.AdminWeb.Company.Companies" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="A1" href="~/Company/Companies.aspx" runat="server">Companies</a>
|<a id="A2" href="~/Company/PricingPlanHistory.aspx" runat="server">Pricing Plan History</a>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <sb:SBRadGrid ID="gvCompanies" Width="100%" runat="server" OnSortCommand="gvCompanies_SortCommand"
                OnItemDataBound="gvCompanies_ItemDataBound" AllowSorting="true" SortToolTip="Click to sort"
                AllowPaging="true" SortedBackColor="Transparent" AllowAutomaticDeletes="True"
                AllowAutomaticInserts="false" OnNeedDataSource="gvCompanies_NeedDataSource" AutoGenerateColumns="False">
                <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                <MasterTableView PageSize="50">
                    <PagerStyle AlwaysVisible="true" />
                    <Columns>
                        <telerik:GridTemplateColumn HeaderText="Name" HeaderStyle-Width="270" SortExpression="Name">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkName" NavigateUrl="#" runat="server"></asp:HyperLink>
                            </ItemTemplate> 
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn HeaderText="Project Count" HeaderStyle-Width="140"
                            ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" UniqueName="projectCount"
                            DataField="projectCount">
                        </telerik:GridBoundColumn>
                        <telerik:GridTemplateColumn HeaderText="Primary Admin" HeaderStyle-Width="140" SortExpression="PAdmin">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkPAdmin" NavigateUrl="#" runat="server"></asp:HyperLink>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn HeaderText="Country" HeaderStyle-Width="120" UniqueName="Country"
                            DataField="Country">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn HeaderText="City" HeaderStyle-Width="120" ItemStyle-Width="140"
                            UniqueName="City" DataField="City">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn HeaderText="State" HeaderStyle-Width="100" ItemStyle-Width="100"
                            UniqueName="State" DataField="State">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn HeaderText="Date Created" HeaderStyle-Width="130" UniqueName="CreatedDate"
                            DataField="CreatedDate">
                        </telerik:GridBoundColumn>
                    </Columns>
                </MasterTableView>
            </sb:SBRadGrid>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
