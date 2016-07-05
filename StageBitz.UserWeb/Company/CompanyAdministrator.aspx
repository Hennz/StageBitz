<%@ Page DisplayTitle="Manage Company Team" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="CompanyAdministrator.aspx.cs" Inherits="StageBitz.UserWeb.Company.CompanyAdministrator" %>

<%@ Register Src="~/Controls/Common/SearchUsers.ascx" TagName="SearchUsers" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Company/CompanyWarningDisplay.ascx" TagPrefix="sb" TagName="CompanyWarningDisplay" %>
<%@ Register Src="~/Controls/Project/CreateNewProjectLink.ascx" TagPrefix="sb" TagName="CreateNewProjectLink" %>
<%@ Register Src="~/Controls/Company/PackageLimitsValidation.ascx" TagPrefix="uc" TagName="PackageLimitsValidation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function ValidatePermissionLevels(sender, args) {
            args.IsValid = false;
            var rbtnPrimaryAdmin = $("input[id$='rbtnPrimaryAdmin']");
            var rbtnSecondaryAdmin = $("input[id$='rbtnSecondaryAdmin']");
            if (rbtnPrimaryAdmin && rbtnSecondaryAdmin) {
                args.IsValid = rbtnPrimaryAdmin.is(':checked') || rbtnSecondaryAdmin.is(':checked');
                sender.errormessage = "Please select Company Administrator permission from radio buttons.";
            }
        }

        function EnableApplyPermissionButton() {
            var rbtnPrimaryAdmin = $("input[id$='rbtnPrimaryAdmin']");
            if (rbtnPrimaryAdmin && rbtnPrimaryAdmin.is(':checked')) {
                $("#<%= btnApplyPermission.ClientID%>").removeAttr('disabled');
            }
            else {
                $("#<%= btnApplyPermission.ClientID%>").attr('disabled', 'disabled');
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    <span runat="server" id="spnNewProjectCreation">| <sb:CreateNewProjectLink runat="server" ID="lnkCreateNewProject" LinkText="Create New Project"/></span>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <telerik:RadWindowManager ID="RadWindowManager1" runat="server">
    </telerik:RadWindowManager>
    <asp:UpdatePanel ID="upnlProjectTeam" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <sb:CompanyWarningDisplay runat="server" id="sbCompanyWarningDisplay" />
            <uc:PackageLimitsValidation runat="server" ID="sbPackageLimitsValidation" />
            <sb:popupbox id="popupChangePremission" title="Permission Settings" runat="server">
                <bodycontent>
                    <asp:UpdatePanel ID="upnlAdmins" UpdateMode="Conditional" runat="server">
                        <ContentTemplate>
                            <div>Company Administrators</div>
                            <div style="padding-left:40px; padding-top:10px;">
                                <div runat="server" id="divPrimaryAdmin">
                                    <asp:RadioButton ID="rbtnPrimaryAdmin" runat="server" Text="Primary Administrator" GroupName="CompanyAdmins"/>
                                    <ul style="margin-top: 5px;">
                                        <li>Can change payment details and manage other Team Members.</li>
                                        <li>There can be only one person in this role.</li>
                                    </ul>
                                </div>
                                <div runat="server" id="divSecondaryAdmin">
                                    <asp:RadioButton ID="rbtnSecondaryAdmin" runat="server" Text="Secondary Administrator" GroupName="CompanyAdmins"/>
                                    <ul style="margin-top: 5px;">
                                        <li><p>Can make decisions about your StageBitz Company account, </p>
                                            <p>they can change your Company's contact details and create Projects.</p></li>
                                    </ul>
                                </div>
                            </div>                 
                            <asp:HiddenField runat="server" ID="hdnCompanyUserId"/>                 
                        </ContentTemplate>
                    </asp:UpdatePanel> 
                </bodycontent>
                <bottomstripecontent> 
                        <asp:Button ID="btnApplyPermission" CssClass="buttonStyle" runat="server" Text="Apply" OnClick="btnApplyPermission_Click" Enabled="false" />  
                        <input type="button" class="buttonStyle popupBoxCloser" value="Cancel" />  
                </bottomstripecontent>
            </sb:popupbox>
            <sb:SearchUsers ID="searchUsers" OnInvitationSent="searchUsers_InvitationSent" runat="server" />
            <telerik:RadGrid ID="gvCompanyAdministrators" ShowHeader="true" AllowSorting="true"
                AutoGenerateColumns="false" OnItemDataBound="gvCompanyAdministrators_ItemDataBound"
                OnItemCommand="gvCompanyAdministrators_ItemCommand" OnDeleteCommand="gvCompanyAdministrators_DeleteCommand"
                OnNeedDataSource="gvCompanyAdministrators_NeedDataSource" runat="server">
                <MasterTableView DataKeyNames="CompanyUserId,InvitationId,UserId" AllowNaturalSort="false"
                    Width="100%" AllowSorting="false" AllowMultiColumnSorting="true" TableLayout="Fixed">
                    <Columns>
                        <telerik:GridTemplateColumn HeaderText="Name" UniqueName="Name">
                            <ItemTemplate>
                                <asp:HyperLink runat="server" ID="lnkUserName" Target="_blank"></asp:HyperLink>
                                <asp:Label runat="server" ID="lblUserName"></asp:Label>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Position" UniqueName="Position">
                            <HeaderStyle Width="150" />
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn UniqueName="Permission" HeaderText="User Permission">
                            <ItemTemplate>
                                <div class="left">
                                    <asp:Literal ID="litPermission" runat="server"></asp:Literal>
                                </div>
                                <div class="right">
                                    <asp:Image ID="imgCompAdmin" Width="16" Height="16" ImageUrl="~/Common/Images/administrator.png"
                                        ToolTip="This user is also a Company Inventory Administrator" runat="server" Visible="false" />
                                    <asp:ImageButton CommandArgument='<%# Bind("CompanyUserId") %>' runat="server" ID="ibtnEditPermision" CommandName="EditPermission" ImageUrl="~/Common/Images/edit.png" Visible="false" ToolTip="Edit" />
                                </div>
                            </ItemTemplate>
                            <HeaderStyle Width="220" />
                            <ItemStyle Width="170" />
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn HeaderText="Status" HeaderStyle-Width="100" UniqueName="Status"
                            DataField="Status">
                        </telerik:GridBoundColumn>
                        <telerik:GridButtonColumn ConfirmText="Are you sure you want to remove?" ConfirmDialogType="RadWindow"
                            ConfirmTitle="Remove" ButtonType="ImageButton" ConfirmDialogHeight="140" CommandName="Delete"
                            Text="Delete" UniqueName="DeleteColumn">
                            <ItemStyle HorizontalAlign="Center" />
                            <HeaderStyle Width="30" />
                            <ItemStyle Width="30" />
                        </telerik:GridButtonColumn>
                    </Columns>
                </MasterTableView>
                <SortingSettings EnableSkinSortStyles="false" />
            </telerik:RadGrid>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
