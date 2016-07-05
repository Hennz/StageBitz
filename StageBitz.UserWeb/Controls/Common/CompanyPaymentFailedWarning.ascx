<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompanyPaymentFailedWarning.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Common.CompanyPaymentFailedWarning" %>

<asp:Panel runat="server" ID="pnlCompanyPaymentFailedCompanyAdmin" Visible="false">
    <div>
        The StageBitz account for <%= CompanyName %> is currently suspended due to a failed payment. 
            You'll still be able to view all your information, but you won't be able to create, delete or edit any information. 
                It's easy to get it going again by updating your payment details on your <a href="<%= CompanyFinancialUrl %>">Company Billing Page</a>.
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlCompanyPaymentFailedNonCompanyAdmin" Visible="false">
    <div>
        The StageBitz account for <%= CompanyName %> is currently suspended. 
            You'll still be able to view all your information, but you won't be able to create, delete or edit any information. 
                If you have any questions, please contact 
        <a href="mailto:<%= PrimaryCompanyAdminEmail %>"><%= PrimaryCompanyAdminName %></a>, your Primary Company Administrator.
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlCompanyPaymentFailedGracePeriodCompanyAdmin" Visible="false">
    <div>
        We were unable to process payment for <%= CompanyName %> on <%= PaymentFailedDate %>. 
            We don't want to hold you up, so your team can keep working for up to <%= RemainingDays %> days while you update the payment details. 
                If you need any assistance, please contact <a href="mailto:<%= SupportEmail %>"><%= SupportEmail %></a>  and we'll be happy to help.
        <p>
            <a href="<%= CompanyFinancialUrl %>">Go to Company Billing</a>
        </p>
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlCompanyPaymentFailedGracePeriodNonCompanyAdmin" Visible="false">
    <div>
        We were unable to process payment for <%= CompanyName %> on <%= PaymentFailedDate %>. 
            Your StageBitz Company Administrator, 
               <a href="mailto:<%= PrimaryCompanyAdminEmail %>"><%= PrimaryCompanyAdminName %></a>
                    , will need to update the payment details. We don't want to hold you up, so your team can keep working for up to <%= RemainingDays %> days while the payment details are being updated.
    </div>
</asp:Panel>
