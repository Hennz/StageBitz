<%@ Page Title="Create New Company" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true" CodeBehind="AddNewCompany.aspx.cs" Inherits="StageBitz.UserWeb.Company.AddNewCompany" %>

<%@ Register Src="~/Controls/Company/CompanyDetails.ascx" TagName="CompanyDetails" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Company/PaymentPackageSelector.ascx" TagName="PaymentPackageSelector"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Company/PaymentPackageSummary.ascx" TagName="PaymentPackageSummary"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Company/PaymentDetailsValidation.ascx" TagName="PaymentValidation"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Company/CompanyHeaderDetails.ascx" TagName="CompanyHeaderDetails" TagPrefix="uc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageTitleRight" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageContent" runat="server">
    <script type="text/javascript">
        function SetDirtyFalse() {
            setGlobalDirty(false);
            return true;
        }
    </script>
    <div class="dirtyValidationArea">
        <asp:UpdatePanel runat="server" ID="upnlWizard" UpdateMode="Conditional">
            <ContentTemplate>
                <h1>
                    <asp:Label ID="lblTitle" Style="position: relative; top: -20px;" runat="server"></asp:Label></h1>
                <asp:Wizard ID="wizard" EnableViewState="true" runat="server">
                    <WizardSteps>
                        <asp:TemplatedWizardStep>
                            <ContentTemplate>

                                <uc:CompanyHeaderDetails runat="server" ID="companyHeaderDetails" />
                                <uc:CompanyDetails DisplayMode="CreateNewCompany" runat="server" ID="ucCompanyDetails" />
                                </div>
                            </ContentTemplate>
                        </asp:TemplatedWizardStep>
                        <asp:WizardStep>
                            <div style="width: 520px;" class="left">
                               <asp:Label ID="lblMsg" runat="server"></asp:Label>
                            </div>
                            <div class="left">
                                <uc:PaymentPackageSummary ID="paymentPackageSummary" runat="server" />
                            </div>
                            <div style="clear: both"></div>
                            <uc:PaymentPackageSelector ID="paymentPackageSelector" runat="server" />
                            <uc:PaymentValidation runat="server" ID="paymentValidation" />

                        </asp:WizardStep>
                        <%--   <asp:WizardStep>
                        
                    </asp:WizardStep>--%>
                    </WizardSteps>

                    <StartNavigationTemplate>
                        <asp:Button ID="StartNextButton" CssClass="buttonStyle" runat="server" ValidationGroup="FieldsValidation" OnClick="StartNextButton_Click" CommandName="MoveNext"
                            Text="Next" />
                        <asp:Button ID="btnCancel" class="dirtyValidationExclude" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="buttonStyle" />
                    </StartNavigationTemplate>
                    <%--          <StepNavigationTemplate>
                       <asp:Button ID="StepNextButton" CssClass="buttonStyle" Enabled="false" runat="server" OnClick="StepNextButton_Click"
                        CommandName="MoveNext" Text="Next" />
                    <asp:Button ID="StepPreviousButton" CssClass="buttonStyle" runat="server" ValidationGroup="groupname" OnClick="StepPreviousButton_Click"
                        CausesValidation="False" CommandName="MovePrevious" Text="Previous" />
                 
                </StepNavigationTemplate>--%>
                    <FinishNavigationTemplate>
                        <asp:Button ID="FinishButton" runat="server" Enabled="false" ValidationGroup="groupname" OnClick="FinishButton_Click" CssClass="buttonStyle"
                            CommandName="MoveComplete" Text="Finish" />
                        <asp:Button ID="FinishPreviousButton" runat="server" OnClick="FinishPreviousButton_Click" CssClass="buttonStyle"
                            CausesValidation="False" CommandName="MovePrevious" Text="Previous" />

                    </FinishNavigationTemplate>
                </asp:Wizard>
                <sb:PopupBox ID="popupCancelMsg" Title="Cancel Company Creation" runat="server">
                    <BodyContent>
                        <div style="width: 500px;">
                            This will cancel creating your new Company. Are you sure?
                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="btnConfirmYes" runat="server" CssClass="buttonStyle" OnClientClick="return SetDirtyFalse();" OnClick="btnConfirmYes_Click" Text="Yes" />
                        <input type="button" class="popupBoxCloser buttonStyle" value="No" />

                    </BottomStripeContent>
                </sb:PopupBox>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
