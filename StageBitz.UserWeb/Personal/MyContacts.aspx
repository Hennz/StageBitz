<%@ Page DisplayTitle="My Contacts" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true" CodeBehind="MyContacts.aspx.cs" Inherits="StageBitz.UserWeb.Personal.MyContacts" %>

<%@ Register Src="~/Controls/Common/ListViewDisplaySettings.ascx" TagName="ListViewDisplaySettings"
    TagPrefix="sb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
|<a href="~/Personal/MyContacts.aspx" class="highlight" runat="server">Contacts</a>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">

    <div class="blueText" style="margin:10px 0px 5px 0px;">Search Contacts</div>

    <asp:UpdatePanel ID="upnlSearchCriteria" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <asp:Panel ID="pnlSearchCriteria" DefaultButton="btnFind" CssClass="BlueBoxesS" style="width: 920px; padding:5px; margin-bottom:20px;" runat="server">

                First Name: <asp:TextBox ID="txtFirstName" MaxLength="50" Width="150" style="margin-right:10px;" runat="server"></asp:TextBox>
                Last Name: <asp:TextBox ID="txtLastName" MaxLength="50" Width="150" style="margin-right:20px;" runat="server"></asp:TextBox>
                Email: <asp:TextBox ID="txtEmail" MaxLength="50" Width="170" runat="server"></asp:TextBox>
                
                <asp:Button ID="btnShowAll" runat="server" CssClass="buttonStyle" Text="Show All" OnClick="btnShowAll_Click" />
                <asp:Button ID="btnFind" runat="server" CssClass="buttonStyle" Text="Find" OnClick="btnFind_Click" />

            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdatePanel ID="upnlSearchResults" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
        
            <sb:GroupBox ID="GroupBox1" runat="server">
                <TitleLeftContent>
                    <asp:Literal ID="ltrlResultsTitle" runat="server"></asp:Literal>
                </TitleLeftContent>
                <TitleRightContent>
                    <sb:ListViewDisplaySettings ID="displaySettings" OnDisplayModeChanged="displaySettings_DisplayModeChanged" runat="server" />
                </TitleRightContent>
                <BodyContent>

                    <div id="divContactThumbs" runat="server" style="overflow-y:auto; max-height:360px;">
                        <asp:ListView ID="lvContacts" OnItemDataBound="lvContacts_ItemDataBound" DataKeyNames="UserId" runat="server">
                            <LayoutTemplate>
                                <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                            </LayoutTemplate>
                            <ItemTemplate>
                                <div class="thumbListItem" style="height:155px;">
                                    <asp:HyperLink ID="lnkUser" runat="server">
                                        <table>
                                            <tr>
                                                <td>
                                                    <sb:ImageDisplay ID="userThumbDisplay" ShowImagePreview="false" runat="server" />
                                                </td>
                                            </tr>
                                        </table>
                                        <div class="">
                                            <asp:Literal ID="ltrlUsername" runat="server"></asp:Literal>
                                        </div>
                                    </asp:HyperLink>
                                    <div class="smallText grayText" style="line-height:13px;">
                                        <asp:Literal ID="ltrlPosition" runat="server"></asp:Literal>
                                        <asp:Literal ID="ltrlCompany" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:ListView>
                    </div>

                    <telerik:RadGrid ID="gvContacts" Width="100%" EnableLinqExpressions="False" AutoGenerateColumns="false"
                        AllowSorting="true" OnItemDataBound="gvContacts_ItemDataBound" OnSortCommand="gvContacts_SortCommand"
                        OnNeedDataSource="gvContacts_NeedDataSource" runat="server">
                        <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                        <MasterTableView AllowNaturalSort="false" AllowMultiColumnSorting="true">
                            <SortExpressions>
                                <telerik:GridSortExpression FieldName="FullName" SortOrder="Ascending" />
                            </SortExpressions>
                            <Columns>
                                <telerik:GridTemplateColumn HeaderText="Name" SortExpression="FullName" ItemStyle-Width="150px"
                                    HeaderStyle-Width="150px">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="lnkUser" runat="server"></asp:HyperLink>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Primary Email" SortExpression="User.Email1" ItemStyle-Width="150px"
                                    HeaderStyle-Width="150px">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="lnkEmail" runat="server"></asp:HyperLink>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Position" HeaderText="Position" SortExpression="User.Position" ItemStyle-Width="150px"
                                    HeaderStyle-Width="150px">
                                </telerik:GridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                    </telerik:RadGrid>

                    <div id="divNoContacts" runat="server" visible="false" style="text-align:center; margin:10px;">
                        No contacts.
                    </div>

                    <div style="clear:both;"></div>

                </BodyContent>
            </sb:GroupBox>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
