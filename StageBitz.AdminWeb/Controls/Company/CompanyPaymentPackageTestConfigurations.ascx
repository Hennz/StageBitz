<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompanyPaymentPackageTestConfigurations.ascx.cs" Inherits="StageBitz.AdminWeb.Controls.Company.CompanyPaymentPackageTestConfigurations" %>
<sb:GroupBox ID="gbSearchResults" runat="server">
    <TitleLeftContent>
        Payment Package Data Simulator
    </TitleLeftContent>
    <BodyContent>
        <asp:UpdatePanel runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <table id="tblPlanMonitor" style="width: 650px; height: 100px;" runat="server">
                    <tr>
                        <td>&nbsp;
                        </td>
                        <td>
                            <b>Plan Includes up to</b>
                        </td>
                        <td style="text-align: center;">
                            <b>Currently using</b>
                        </td>
                        <td>&nbsp
                        </td>
                        <td style="width: 150px">&nbsp
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <b>Projects</b>
                        </td>
                        <td>
                            <asp:Literal ID="litActiveProjects" runat="server"></asp:Literal>
                        </td>
                        <td style="text-align: center;">
                            <telerik:RadNumericTextBox ID="txtCurrentProjectCount" Width="50" runat="server"></telerik:RadNumericTextBox>
                        </td>
                        <td>
                            <asp:Button ID="btnAddProjects" CssClass="buttonStyle" runat="server" Width="100" ValidationGroup="ProjValidation" OnClick="btnAddProjects_Click" Text="Add Projects" />
                        </td>
                        <td>
                            <div id="divMsgProjects" class="inlineNotification right" style="line-height: 30px;">
                                Changes saved.
                            </div>
                            <asp:RequiredFieldValidator runat="server" ID="rqdProjects" ControlToValidate="txtCurrentProjectCount" ValidationGroup="ProjValidation" ErrorMessage="Project Count is Required"></asp:RequiredFieldValidator>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <b>Users</b>
                        </td>
                        <td>
                            <asp:Literal ID="litActiveUsers" runat="server"></asp:Literal>
                        </td>
                        <td style="text-align: center;">

                            <telerik:RadNumericTextBox ID="txtCurrentUserCount" Width="50" runat="server"></telerik:RadNumericTextBox>

                        </td>
                        <td style="text-align: center;">
                            <asp:Button ID="btnAddUsers" CssClass="buttonStyle" runat="server" Width="100" ValidationGroup="UserValidation" OnClick="btnAddUsers_Click" Text="Add Users" />
                        </td>
                        <td>
                            <div id="divMsgUsers" class="inlineNotification right" style="line-height: 30px;">
                                Changes saved.
                            </div>
                            <asp:RequiredFieldValidator runat="server" ID="rqdUsers" ControlToValidate="txtCurrentUserCount" ValidationGroup="UserValidation" ErrorMessage="User Count is Required"></asp:RequiredFieldValidator>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <b>Items</b>
                        </td>
                        <td>
                            <asp:Literal ID="litInventoryItems" runat="server"></asp:Literal>
                        </td>
                        <td style="text-align: center;">
                            <telerik:RadNumericTextBox ID="txtInvCurrentCount" Width="50" runat="server"></telerik:RadNumericTextBox>

                        </td>
                        <td style="text-align: center;">
                            <asp:Button ID="btnAddItems" CssClass="buttonStyle" runat="server" ValidationGroup="ItemsValidation" Width="100" OnClick="btnAddItems_Click" Text="Add Items" />
                        </td>
                        <td>
                            <div id="divMsgItems" class="inlineNotification right" style="line-height: 30px;">
                                Changes saved.
                            </div>
                            <asp:RequiredFieldValidator runat="server" ID="rqdItems" ControlToValidate="txtInvCurrentCount" ValidationGroup="ItemsValidation" ErrorMessage="Inventory Item Count is Required"></asp:RequiredFieldValidator>
                        </td>
                    </tr>
                </table>
                <sb:PopupBox ID="popupError" runat="server">
                    <BodyContent>
                        <asp:Literal ID="litError" runat="server"></asp:Literal>
                    </BodyContent>
                </sb:PopupBox>
            </ContentTemplate>
        </asp:UpdatePanel>
    </BodyContent>
</sb:GroupBox>
