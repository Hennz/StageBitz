<%@ Page DisplayTitle="Activations Pending" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="ActivationsPending.aspx.cs" Inherits="StageBitz.AdminWeb.User.ActivationsPending" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <div style="font-weight: bold; padding-bottom: 2px;">
        <asp:Literal ID="litPendingInvitationsCount" runat="server"></asp:Literal>
    </div>
    <div>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <telerik:RadGrid ID="gvUsers" Width="100%" runat="server" SortedBackColor="Transparent"
                    OnItemCommand="gvUsers_ItemCommand" AutoGenerateColumns="False">
                    <MasterTableView>
                        <PagerStyle AlwaysVisible="true" />
                        <Columns>
                            <telerik:GridBoundColumn HeaderText="First Name" UniqueName="FirstName" DataField="FirstName"
                                ItemStyle-Width="230px" HeaderStyle-Width="230px">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn HeaderText="Last Name" UniqueName="LastName" DataField="LastName"
                                ItemStyle-Width="240px" HeaderStyle-Width="240px">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn HeaderText="Email" UniqueName="Email" DataField="Email"
                                ItemStyle-Width="240px" HeaderStyle-Width="240px">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="Registered Date" UniqueName="RegisteredDate"
                                DataField="RegisteredDate" ItemStyle-Width="240px" HeaderStyle-Width="240px">
                                <ItemTemplate>
                                    <%# StageBitz.AdminWeb.Common.Helpers.Support.FormatDate(Eval("RegisteredDate"))%>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn HeaderText="Status" UniqueName="Status" DataField="Status"
                                ItemStyle-Width="240px" HeaderStyle-Width="240px">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn UniqueName="ResendEmail">
                                <ItemStyle Width="93" />
                                <ItemTemplate>
                                    <asp:Button ID="btnResend" CssClass="buttonStyle" runat="server" Text="Resend" CommandName="Resend"
                                        CommandArgument='<%# DataBinder.Eval(Container.DataItem,"UserID") %>' />
                                </ItemTemplate>
                                <HeaderStyle Width="93" />
                            </telerik:GridTemplateColumn>
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
                <%-- <div>
                    <asp:Button ID="btnResend" Text="Re-Send Activation Emails" Visible="false" CssClass="buttonStyle"
                        runat="server" OnClick="btnResend_Click" OnClientClick="return confirm('Are you sure you want re-send activation emails?');" />
                </div>--%>
                <sb:PopupBox ID="popupResendFailed" Title="Resend Failed" runat="server">
                    <BodyContent>
                        <div>
                            <asp:Label ID="lblError"  Visible="true" runat="server" />
                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="Button2" runat="server" CssClass="popupBoxCloser buttonStyle" Text="OK" />
                    </BottomStripeContent>
                </sb:PopupBox>
                <sb:PopupBox ID="popupResendSucess" Title="Resend Success" runat="server">
                    <BodyContent>
                        <div>
                            <asp:Label ID="lblInfo"  Visible="true" runat="server" />
                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="Button1" runat="server" CssClass="popupBoxCloser buttonStyle" Text="OK" />
                    </BottomStripeContent>
                </sb:PopupBox>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
