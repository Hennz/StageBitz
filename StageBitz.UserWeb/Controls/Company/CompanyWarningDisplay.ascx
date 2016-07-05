<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompanyWarningDisplay.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Project.CompanyWarningDisplay" %>
<%@ Register Src="~/Controls/Common/CompanyPaymentFailedWarning.ascx" TagPrefix="uc" TagName="CompanyPaymentFailedWarning" %>

<asp:UpdatePanel ID="upnlCompanyWarningDisplay" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <asp:MultiView ID="noticesMultiView" runat="server">
            <asp:View ID="View1" runat="server">
                <div class="companyWarningNotice">
                    <div>
                        The StageBitz account for
                        <asp:Label runat="server" ID="lblCompanyNameSBAdminSuspendCA"></asp:Label>
                        is currently suspended. You'll still be able to view all your information, 
                                but you won't be able to create, delete or edit any information. 
                                    If you need any assistance please contact <asp:HyperLink runat="server" ID="lnkContactSBSupport"></asp:HyperLink> and we’ll be happy to help.
                    </div>
                </div>
            </asp:View>
            <asp:View ID="View8" runat="server">
                <div class="companyWarningNotice">
                    <div>
                        The StageBitz account for
                        <asp:Label runat="server" ID="lblCompanyNameSBAdminSuspendNonCA"></asp:Label>
                        is currently suspended. You'll still be able to view all your information, 
                                but you won't be able to create, delete or edit any information. If you have any questions, please contact
                                    <asp:HyperLink runat="server" ID="lnkPrimaryComapnyAdmin"></asp:HyperLink>, your Primary Company Administrator.
                    </div>
                </div>
            </asp:View>
            <asp:View ID="View2" runat="server">
                <div class="companyWarningNotice">
                    <div runat="server" id="divFreeTrialEndedCA">
                        You have finished your Free Trial without choosing a pricing plan so your Inventory and Projects are temporarily suspended. 
                                You'll still be able to view all your information, but you won't be able to create, delete or edit any information. 
                                    It's easy to get it going again, just choose from a selection of Pricing Plans and enter your payment details on your 
                                        <asp:HyperLink runat="server" ID="lnkPricingPlanPage" Text="Pricing Plan page"></asp:HyperLink>.
                    </div>
                    <div runat="server" id="divFreeTrialEndedNonCA">
                        <asp:Label runat="server" ID="lblFreeTrialEndedCompanyName"></asp:Label>
                            has finished their free trial so the Inventory and Projects are temporarily suspended.
                                You'll still be able to view all your information, but you won't be able to create, delete or edit any information. 
                                    It's easy to get it going again, just contact <asp:HyperLink runat="server" ID="lnkFreeTrialEndedContactPrimaryComapnyAdmin"></asp:HyperLink>, 
                                        your Primary Company Administrator.
                    </div>
                </div>
            </asp:View>
            <asp:View ID="View3" runat="server">
                <div class="companyWarningNotice">
                    <uc:CompanyPaymentFailedWarning runat="server" ID="ucCompanyPaymentFailedWarningGracePeriodCompanyAdmin" />
                </div>
            </asp:View>
            <asp:View ID="View4" runat="server">
                <div class="companyWarningNotice">
                    <uc:CompanyPaymentFailedWarning runat="server" ID="ucCompanyPaymentFailedWarningGracePeriodNonCompanyAdmin" />
                </div>
            </asp:View>
            <asp:View ID="View5" runat="server">
                <div class="companyWarningNotice">
                    <uc:CompanyPaymentFailedWarning runat="server" ID="ucCompanyPaymentFailedWarningPaymentFailedCompanyAdmin" />
                </div>
            </asp:View>
            <asp:View ID="View6" runat="server">
                <div class="companyWarningNotice">
                    <uc:CompanyPaymentFailedWarning runat="server" ID="ucCompanyPaymentFailedWarningPaymentFailedNonCompanyAdmin" />
                </div>
            </asp:View>
            <asp:View ID="View7" runat="server">
                <div class="companyWarningNotice">
                    <div runat="server" id="divNopamentOptionNonCA">
                        The StageBitz account for <asp:Label runat="server" ID="lblNoPaymentOptionCompanyName"></asp:Label> is currently suspended. 
                            You'll still be able to view all your information, but you won't be able to create, delete or edit any information. 
                                If you have any questions, please contact <asp:HyperLink runat="server" ID="lnkNopamentOptionContactPrimaryComapnyAdmin"></asp:HyperLink>, 
                                    your Primary Company Administrator.
                    </div>
                    <div runat="server" id="divNopamentOptionCA">
                        Your 100% promotional code has expired and your payment details are not set up so your Inventory and Projects are temporarily suspended. 
                            You'll still be able to view all your information, but you won't be able to create, delete or edit any information. 
                                It's easy to get it going again, just confirm your pricing plan and enter your payment details on your  
                                    <asp:HyperLink runat="server" ID="lnkNopamentOptionPricingPlanPage" Text="Pricing Plan page"></asp:HyperLink>.
                    </div>
                </div>
            </asp:View>
        </asp:MultiView>
    </ContentTemplate>
</asp:UpdatePanel>
