<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectWarningDisplay.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Project.ProjectWarningDisplay" %>
<%@ Register Src="~/Controls/Common/SetupCreditCardDetails.ascx" TagName="SetUpCreditCardDetails" TagPrefix="uc" %>
<%@ Register Src="~/Controls/Common/CompanyPaymentFailedWarning.ascx" TagPrefix="uc" TagName="CompanyPaymentFailedWarning" %>

<asp:UpdatePanel ID="upnlProjectWarningDisplay" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <asp:MultiView ID="noticesMultiView" runat="server">
            <asp:View ID="View1" runat="server">
                <!--Radio button notification-->
                <div class="projectWarningNotice">
                    <div style="margin-top: 10px; margin-left: 20px;">
                        <b>You have
                            <%= RemainingDays %>
                             remaining of your free trial. What would you like to do?</b><br />
                        <br />
                        <div style="margin-left: 20px;">
                            <asp:RadioButton ID="rdContinueProject" GroupName="freetrial" Text="I want to keep working on this Project!"
                                Font-Bold="True" runat="server" /><br />
                            <div style="margin-left: 40px; margin-top: 5px;">
                                When you select 'OK', we will ask you to choose from a selection of Pricing Plan and confirm the Company's payment details. You will not be charged until the end of the free trial.
                            </div>
                            <%--<br />
                            <asp:RadioButton ID="rdSuspendProject" GroupName="freetrial" Text="I don't need this Project any more."
                                Font-Bold="true" runat="server" /><br />
                            <div style="margin-left: 40px; margin-top: 5px;">
                                When you select 'OK', this Project will be suspended. You will still be able to view
                                it, just not edit or add any information. If you would ever like to reactivate this
                                Project, you can do so from Company Billing.
                            </div>--%>
                            <div style="text-align: right;margin-bottom:5px;">
                                <asp:Button ID="btnConfirmFreeTrialContinue" OnClick="HandleUserResponse" runat="server"
                                    Style="vertical-align: bottom;" CssClass="buttonStyle" Text="OK" />
                            </div>
                            <br style="clear:both" />
                        </div>
                        <sb:PopupBox ID="popupRequiredNotification" Title="Select an option" runat="server">
                            <BodyContent>
                                <div style="min-width: 200px; max-width: 500px;">
                                    It is required to select an option.
                                </div>
                            </BodyContent>
                            <BottomStripeContent>
                                <input type="button" class="buttonStyle popupBoxCloser" value="OK" />
                            </BottomStripeContent>
                        </sb:PopupBox>
                    </div>
                </div>
            </asp:View>
            <asp:View ID="View2" runat="server">
                <!--B-->
                <div class="projectWarningNotice">
                    <uc:CompanyPaymentFailedWarning runat="server" id="ucCompanyPaymentFailedWarningGracePeriodCompanyAdmin" />
                </div>
            </asp:View>
            <asp:View ID="View3" runat="server">
                <!--C-->
                <div class="projectWarningNotice">
                    <uc:CompanyPaymentFailedWarning runat="server" id="ucCompanyPaymentFailedWarningGracePeriodNonCompanyAdmin" />
                </div>
            </asp:View>
            <asp:View ID="View4" runat="server">
                <!--D-->
                <div class="projectWarningNotice">
                    <uc:CompanyPaymentFailedWarning runat="server" id="ucCompanyPaymentFailedWarningPaymentFailedCompanyAdmin" />
                </div>
            </asp:View>
            <asp:View ID="View5" runat="server">
                <!--E-->
                <div class="projectWarningNotice">
                    <uc:CompanyPaymentFailedWarning runat="server" id="ucCompanyPaymentFailedWarningPaymentFailedNonCompanyAdmin" />
                </div>
            </asp:View>
            <asp:View ID="View6" runat="server">
                <!--D1-->
                <div class="projectWarningNotice">
                    <p>
                        This Project has been suspended. You can access all the information already here;
                        you just won’t be able to add or change anything. It’s easy to get it going again
                        though; just go to your Company Billing page and select ‘Reactivate’.
                    </p>
                    <p>
                        If you need any assistance, please contact <a href="mailto:<%= SupportEmail %>"><%= SupportEmail %></a> and we'll be happy to help.
                    </p>
                    <p>
                        <a href="<%= CompanyFinancialUrl %>">Go to Company Billing</a>
                    </p>
                </div>
            </asp:View>
            <asp:View ID="View7" runat="server">
                <!--D2-->
                <div class="projectWarningNotice">
                    <p>
                        This Project has been suspended. You can access all the information already here.
                        you just won’t be able to add or change anything. It’s easy to get it going again
                        though; just ask your StageBitz Company Administrator, <a href="mailto:<%= CompanyAdminEmail %>"><%= CompanyAdminName %></a> to reactivate it from Company Billing.
                    </p>
                </div>
            </asp:View>
            <asp:View ID="View8" runat="server">
                <!--F-->
                <div class="projectWarningNotice">
                   <div style="margin-top:10px;margin-left:20px;">
                        <b><%=ProjectClosedBy%> has closed this Project.</b> <br />
                        Don't worry, you can still access all the information; you just won't be able to add or change anything.<br /><br />
                
                    <i>  <b> COMING SOON: "Clone Project".</b> <br />
                        This will allow you to make a copy of a Closed Project so that you can take up where you left off for the next season, tour or revival without over-writing the 
                        original Project. If you need to do this before we release 'Clone Project', please contact <a href="mailto:<%= SupportEmail %>"><%= SupportEmail %></a> and we'll be happy to help. </i>
                  </div>   
                </div>
            </asp:View>
        </asp:MultiView>
    </ContentTemplate>
</asp:UpdatePanel>
