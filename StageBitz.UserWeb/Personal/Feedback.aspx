<%@ Page DisplayTitle="Feedback" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true" CodeBehind="Feedback.aspx.cs" Inherits="StageBitz.UserWeb.Personal.Feedback" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
|<a href="~/Personal/MyContacts.aspx" runat="server">Contacts</a>|<a href="~/Company/MyCompanies.aspx" runat="server">Companies</a>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">

    <asp:UpdatePanel ID="upnlFeedback" runat="server">
        <ContentTemplate>

            <asp:PlaceHolder ID="plcFeedbackInput" runat="server">
                <div class="left" style="line-height:30px; width:100px;">
                Your Name:<br />
                Your Email:<br />
                Feedback:
                </div>

                <div class="left" style="line-height:30px; width:300px;">
                    <asp:Literal ID="ltrlName" runat="server"></asp:Literal><br />
                    <asp:Literal ID="ltrlEmail" runat="server"></asp:Literal><br />
                    <div style="height:130px;">
                        <asp:TextBox ID="txtFeedback" TextMode="MultiLine" Height="110" runat="server"></asp:TextBox><br />
                        <asp:RequiredFieldValidator ID="reqFeedback" ControlToValidate="txtFeedback" ValidationGroup="Feedback" runat="server" ErrorMessage="Enter your feedback."></asp:RequiredFieldValidator>
                    </div>
                    <asp:Button ID="btnSendFeedback" runat="server" OnClick="btnSendFeedback_Click" ValidationGroup="Feedback" CssClass="buttonStyle" Text="Send Feedback" />
                </div>

                <div style="clear:both;">
                </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="plcFeedbackSent" Visible="false" runat="server">
                <div class="notices" style="line-height:30px;">
                    <p><strong>Feedback sent.</strong></p>
                    Thank you for your feedback. 
                    <a href="~/Default.aspx" runat="server">Home</a>
                </div>
            </asp:PlaceHolder>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
