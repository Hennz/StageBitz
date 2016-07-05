<%@ Page DisplayTitle="Company Details" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="CompanyDetails.aspx.cs" Inherits="StageBitz.AdminWeb.Company.CompanyDetails" %>

<%@ Register Src="~/Controls/Common/TransactionSearch.ascx" TagName="TransactionSearch"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Inventory/InventoryActivity.ascx" TagName="InventoryActivity"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Project/ProjectActivity.ascx" TagPrefix="uc" TagName="ProjectActivity" %>
<%@ Register Src="~/Controls/Company/CompanyPaymentPackageTestConfigurations.ascx" TagPrefix="uc" TagName="CompanyPaymentPackageTestConfigurations" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <div class="left" style="line-height: 25px; margin-right: 10px; width: 120px;">
        Company Name:<br />
        Created By:<br />
        Created Date:<br />
    </div>
    <div class="left" style="line-height: 25px; width: 500px;">
        <strong>
            <asp:Label ID="lblCompanyName" runat="server"></asp:Label></strong><br />
        <asp:Label ID="lblCreatedBy" runat="server"></asp:Label><br />
        <asp:Literal ID="ltrlCreatedDate" runat="server"></asp:Literal><br />
    </div>
    <div style="margin-top:10px;width:200px;float:left;">
        <asp:CheckBox ID="chkSuspendCompany" Text="Suspend Company's Activity" AutoPostBack="true" OnCheckedChanged="chkSuspendCompany_CheckedChanged" runat="server" />
    </div>
        <sb:PopupBox ID="popupConfirmSuspendReactivate" Title="Are you Sure?" runat="server" ShowCornerCloseButton="false">
        <BodyContent>
            <div runat="server" id="divSuspend">
                This will suspend all of this Company's Projects
            </div>
                <div runat="server" id="divActivate">
                This will allow this comapny to reactivate their Projects
            </div>
        </BodyContent>
        <BottomStripeContent>
            <asp:Button ID="btnConfirmSuspedReactivate" runat="server" CssClass="buttonStyle" Text="Confirm" OnClick="btnConfirmSuspedReactivate_Click" />
            <asp:Button ID="btnCancelSuspendReactivate" runat="server" CssClass="buttonStyle" Text="Cancel" OnClick="btnCancelSuspendReactivate_Click" />
        </BottomStripeContent>
    </sb:PopupBox>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div class="right">
        <img id="imgPaymentError" runat="server" src="~/Common/Images/errorBig.png" />
    </div>
    <div style="clear: both; height: 10px;">
    </div>
    <div class="left" style="width: 49%;">
        <sb:GroupBox ID="GroupBox1" runat="server">
            <TitleLeftContent>
                Contact Details
            </TitleLeftContent>
            <BodyContent>
                <div class="left" style="line-height: 25px; margin-right: 10px; width: 120px; height: 200px;">
                    Address Line 1:<br />
                    Address Line 2:<br />
                    City:<br />
                    State:<br />
                    Postal/Zip Code:<br />
                    Country:<br />
                    Phone:<br />
                    Website:<br />
                </div>
                <div class="left" style="line-height: 25px; width: 260px; overflow: auto; white-space: nowrap;">
                    <asp:Label ID="lblAddressLine1" runat="server"></asp:Label><br />
                    <asp:Label ID="lblAddressLine2" runat="server"></asp:Label><br />
                    <asp:Label ID="lblCity" runat="server"></asp:Label><br />
                    <asp:Label ID="lblState" runat="server"></asp:Label><br />
                    <asp:Label ID="lblPostCode" runat="server"></asp:Label><br />
                    <asp:Label ID="lblCountry" runat="server"></asp:Label><br />
                    <asp:Label ID="lblPhone" runat="server"></asp:Label><br />
                    <asp:Label ID="lblWebsite" runat="server"></asp:Label><br />
                </div>
                <div style="clear: both;">
                </div>
            </BodyContent>
        </sb:GroupBox>
    </div>
    <div class="right" style="width: 49%;">
        <sb:GroupBox ID="GroupBox2" runat="server">
            <TitleLeftContent>
                Company Team
            </TitleLeftContent>
            <BodyContent>
                <div style="line-height: 25px; padding-left: 5px; height: 200px; overflow-y: auto;">
                    <asp:ListView ID="lvAdmins" OnItemDataBound="lvAdmins_ItemDataBound" runat="server">
                        <LayoutTemplate>
                            <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkAdmin" runat="server"></asp:HyperLink>
                            <asp:Label ID="lblRole" CssClass="RoleIndicater" runat="server">(Primary)</asp:Label>
                            <div style="position: relative; top: 3px; display: inline;">
                                <asp:Image ID="imgCompAdmin" Width="16" Height="16" ImageUrl="~/Common/Images/administrator.png"
                                    ToolTip="This user is also a Company Inventory Administrator" runat="server" Visible="false" />
                            </div>
                            <br />
                        </ItemTemplate>
                    </asp:ListView>
                </div>
            </BodyContent>
        </sb:GroupBox>
    </div>
    <br style="clear: both;" />
    <uc:ProjectActivity runat="server" ID="ucProjectActivity" />
    <uc:InventoryActivity ID="ucInventoryActivity" runat="server" />
    <uc:TransactionSearch ID="transactionSearch" runat="server" />
    <uc:CompanyPaymentPackageTestConfigurations ID="uccompanyPaymentPackageTestConfigurations" runat="server" />
</asp:Content>
