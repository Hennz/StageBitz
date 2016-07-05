<%@ Page DisplayTitle="Manage Project Team" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="ProjectTeam.aspx.cs" Inherits="StageBitz.UserWeb.Project.ProjectTeam" %>

<%@ Register Src="~/Controls/Common/SearchUsers.ascx" TagName="SearchUsers" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay" TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectUserInvitation.ascx" TagName="UserInvitation" TagPrefix="uc"%>
<%@ Register Src="~/Controls/Company/PackageLimitsValidation.ascx" TagPrefix="sb" TagName="PackageLimitsValidation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
     <script type="text/javascript">
         if ('<%= this.CompanyId %>' != '0' && '<%= this.ProjectId %>' != '0') {
             var _gaq = _gaq || [];
             _gaq.push(['_setCustomVar', 1, 'Category', 'Project', 2]);
             _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyId %>', 2]);
             _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
             _gaq.push(['_setCustomVar', 4, 'ProjectId', '<%= this.ProjectId %>', 2]);
         }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
| <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    | <a id="lnkBookings" runat="server">Bookings</a>
|<a id="linkTaskManager" runat="server">Task Manager</a> 
|<sb:ProjectUpdatesLink ID="projectUpdatesLink" runat="server" />
<sb:ReportList ID="reportList" runat="server"/>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <telerik:RadWindowManager ID="RadWindowManager1" runat="server">
    </telerik:RadWindowManager>

    <sb:ProjectWarningDisplay ID="warningDisplay" runat="server" />    
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>
    <sb:PackageLimitsValidation runat="server" ID="sbPackageLimitsValidation" />

    <sb:SearchUsers ID="searchUsers" OnInvitationSent="searchUsers_InvitationSent" runat="server" />
    <asp:UpdatePanel ID="upnlProjectTeam" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <sb:PopupBox ID="popupProjectAdminConfirmation" Title="Confirm Project Administrator"
                ShowCornerCloseButton="false" runat="server">
                <BodyContent>
                    <div style="width: 300px;">
                        Are you sure you want to make this user the Project Administrator?<br />
                        <div class="grayText" id="divProjectAdminConfirmMessage"  runat="server" style="margin-top: 10px;">
                            </div>
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <asp:Button ID="btnConfirmProjectAdmin" CssClass="buttonStyle" runat="server" Text="Yes"
                        OnClick="btnConfirmProjectAdmin_Click" />
                    <%--<input type="button" class="popupBoxCloser buttonStyle" value="No" />--%>
                    <asp:Button ID="btnCancelProjectAdmin" CssClass="buttonStyle" runat="server" Text="No"
                        OnClick="btnCancelProjectAdmin_Click" />
                </BottomStripeContent>
            </sb:PopupBox>
            <telerik:RadGrid ID="rgProjectTeam" ShowHeader="true" AllowSorting="true" AutoGenerateColumns="false"
                OnItemDataBound="rgProjectTeam_ItemDataBound" OnItemCommand="rgProjectTeam_ItemCommand"
                OnDeleteCommand="rgProjectTeam_DeleteCommand" OnNeedDataSource="rgProjectTeam_NeedDataSource"
                OnSortCommand="rgProjectTeam_SortCommand" OnUpdateCommand="rgProjectTeam_UpdateCommand"
                 AllowAutomaticDeletes="True" AllowAutomaticInserts="false" AllowAutomaticUpdates="True" runat="server">
                <MasterTableView DataKeyNames="UserId, ProjectUserId, InvitationId" AllowNaturalSort="false"
                    EditMode="InPlace">
                    <Columns>
                        <telerik:GridTemplateColumn HeaderText="Name" UniqueName="FullName" SortExpression="FullName" ItemStyle-Height="25px">
                            <ItemTemplate>
                                <asp:HyperLink runat="server" ID="lnkUserName" Target="_blank"></asp:HyperLink>
                                <asp:Label runat="server" ID="lblUserName"></asp:Label>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Role" UniqueName="Role" SortExpression="Role" ItemStyle-Width="130">
                            <%--<EditItemTemplate>
                                <asp:TextBox ID="txtRole" style="margin:0px; width:120px;" MaxLength="100" runat="server"></asp:TextBox>
                            </EditItemTemplate>--%>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Permission" UniqueName="Permission" ItemStyle-Width="150">
                            <ItemTemplate>
                                <div class="left">
                                    <%--<asp:DropDownList ID="ddlPermission" AutoPostBack="true" OnSelectedIndexChanged="ddlPermission_SelectedIndexChanged"
                                        Style="margin: 0px;" Width="120px" runat="server">
                                    </asp:DropDownList>--%>
                                    <asp:Literal ID="ltrlPermission" runat="server"></asp:Literal>
                                </div>
                                <div class="right">
                                    <asp:Image ID="imgCompAdmin" Width="16" Height="16" ImageUrl="~/Common/Images/administrator.png"
                                        ToolTip="This user is also a Company Administrator" runat="server" Visible="false" />
                                </div>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <div class="left">
                                    <asp:Literal ID="ltrlPermission" runat="server"></asp:Literal>
                                </div>
                                <div class="right">
                                    <asp:Image ID="imgCompAdmin" Width="16" Height="16" ImageUrl="~/Common/Images/administrator.png"
                                        ToolTip="This user is also a Company Administrator" runat="server" Visible="false" />
                                </div>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn HeaderText="Status" UniqueName="Status" DataField="StatusCode.Description"
                            SortExpression="" ReadOnly="true">
                        </telerik:GridBoundColumn>
                        <telerik:GridTemplateColumn HeaderText="Active" UniqueName="Active" HeaderStyle-HorizontalAlign="Center"
                            ItemStyle-HorizontalAlign="Center" ItemStyle-Width="30">
                            <ItemTemplate>
                                <asp:CheckBox ID="chkActive" AutoPostBack="true" OnCheckedChanged="chkActive_CheckedChanged"    
                                    runat="server" />
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                         <telerik:GridTemplateColumn UniqueName="EditPermission" HeaderStyle-HorizontalAlign="Center"
                            ItemStyle-HorizontalAlign="Center" ItemStyle-Width="30">
                            <ItemTemplate>
                                   <asp:ImageButton runat="server" ID="imgbtnEditPermission" CommandName="EditPermission" 
                        ImageUrl="~/Common/Images/edit.png" />
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridButtonColumn ConfirmText="Are you sure you want to remove this member?"
                            ConfirmDialogType="RadWindow" ConfirmTitle="Remove" ButtonType="ImageButton"
                            ConfirmDialogHeight="140" CommandName="Delete" Text="Delete" UniqueName="DeleteColumn">
                            <ItemStyle HorizontalAlign="Center" Width="30" />
                            <HeaderStyle Width="30" />
                        </telerik:GridButtonColumn>
                    </Columns>
                </MasterTableView>
                <SortingSettings EnableSkinSortStyles="false" />
            </telerik:RadGrid>
                    <sb:PopupBox ID="popupEditUserPermission" runat="server" Title="Permission Settings" Height="100">
            <bodycontent> 
                <uc:UserInvitation runat="server" ID="ucUserInvitationPopup"/>
            </bodycontent>
            <BottomStripeContent>
                <asp:Button ID="btnApplyUserPermission" CssClass="buttonStyle"
                runat="server" Text="Apply" OnClick="btnApplyUserPermission_Click" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />       
<%--                <asp:Button ID="btnCancelConfirmCreatePDF" CssClass="buttonStyle" runat="server"
                    Text="Cancel" OnClientClick="hidePopup('popupEditUserPermission');"/>--%>
            </BottomStripeContent>
        </sb:PopupBox>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
