<%@ Page DisplayTitle="Users" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="Users.aspx.cs" Inherits="StageBitz.AdminWeb.User.Users" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    |<a id="linkActivationPendingUsers" href="~/User/ActivationsPending.aspx" runat="server">Activations Pending</a>
    |<a id="lnkManageSkills" href="~/User/Skills.aspx" runat="server">Manage Skills</a>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <asp:Button ID="btnExporttoExcel" Text="Export to Excel" CssClass="buttonStyle" OnClick="btnExporttoExcel_Click" runat="server" />
    <h2>&nbsp;</h2>
    <asp:UpdatePanel runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="Panel1" runat="server">
                <sb:SBRadGrid ID="gvUsers" Width="100%" runat="server" OnSortCommand="gvUsers_SortCommand"
                    OnItemDataBound="gvUsers_ItemDataBound" AllowSorting="true" SortToolTip="Click to sort"
                    AllowPaging="true" SortedBackColor="Transparent" AllowAutomaticDeletes="True"
                    AllowAutomaticInserts="false" OnNeedDataSource="gvUsers_NeedDataSource" AutoGenerateColumns="False">
                    <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                    <MasterTableView PageSize="50" AllowNaturalSort="false">
                        <PagerStyle AlwaysVisible="true" />
                        <SortExpressions>
                            <telerik:GridSortExpression FieldName="LastLogIn" SortOrder="Descending" />
                        </SortExpressions>
                        <Columns>
                            <telerik:GridTemplateColumn HeaderText="First Name" SortExpression="FirstName" ItemStyle-Width="225px"
                                HeaderStyle-Width="225px">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkFirstName" NavigateUrl="#" runat="server"></asp:HyperLink>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Last Name" SortExpression="LastName" ItemStyle-Width="230px"
                                HeaderStyle-Width="230px">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkLastName" runat="server"></asp:HyperLink>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Status" SortExpression="Status" ItemStyle-Width="150px"
                                HeaderStyle-Width="150px">
                                <ItemTemplate>
                                    <asp:Literal ID="litStatus" runat="server"></asp:Literal>
                                    <asp:Image ID="imgAdmin" Visible="false" Width="16" Height="16" ImageUrl="~/Common/Images/administrator.png"
                                        ToolTip="StageBitz Administrator" runat="server" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn HeaderText="Country" HeaderStyle-Width="100" ItemStyle-Width="90"
                                UniqueName="Country" DataField="Country">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn HeaderText="City" HeaderStyle-Width="80" ItemStyle-Width="70"
                                UniqueName="City" DataField="City">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn HeaderText="State" HeaderStyle-Width="80" ItemStyle-Width="70"
                                UniqueName="State" DataField="State">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn HeaderText="Registration Date" UniqueName="RegisteredDate" HeaderStyle-Width="175"
                                ItemStyle-Width="105" DataField="RegisteredDate">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn HeaderText="Last Logon" UniqueName="LastLogIn" HeaderStyle-Width="130"
                                ItemStyle-Width="105" DataField="LastLogIn">
                            </telerik:GridBoundColumn>

                        </Columns>
                    </MasterTableView>
                </sb:SBRadGrid>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
