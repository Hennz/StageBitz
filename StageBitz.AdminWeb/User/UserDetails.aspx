<%@ Page DisplayTitle="User Details" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="UserDetails.aspx.cs" Inherits="StageBitz.AdminWeb.User.UserDetails" %>

<%@ Register Src="~/Controls/Personal/UserActivity.ascx" TagPrefix="uc" TagName="UserActivity" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <div id="divDetails">
        <table style="line-height: 25px;">
            <tr>
                <td style="min-width: 100px;">First Name:
                </td>
                <td style="min-width: 600px;">
                    <strong>
                        <asp:Literal ID="ltrlFirstName" runat="server"></asp:Literal>
                    </strong>
                </td>
                <td>Status:
                </td>
                <td>
                    <asp:Literal ID="ltrlStatus" runat="server"></asp:Literal>
                </td>
            </tr>
            <tr>
                <td>Last Name:
                </td>
                <td>
                    <strong>
                        <asp:Literal ID="ltrlLastName" runat="server"></asp:Literal>
                    </strong>
                </td>
                <td>Last Login:
                </td>
                <td>
                    <asp:Literal ID="ltrlLastLogInDate" runat="server"></asp:Literal>
                </td>
            </tr>
            <tr>
                <td>Login Name:
                </td>
                <td>
                    <asp:Literal ID="ltrlLoginName" runat="server"></asp:Literal>
                </td>
            </tr>
        </table>
    </div>
    <br />
    <div style="width: 45%; float: left;" id="divContacts">
        <sb:GroupBox ID="GroupBox1" runat="server">
            <TitleLeftContent>
                Contact Details
            </TitleLeftContent>
            <BodyContent>
                <div style="height: 280px; overflow: auto;">
                    <table style="line-height: 25px;">
                        <tr>
                            <td style="min-width: 100px;">Primary Email:
                            </td>
                            <td>
                                <asp:Label ID="lblEmail1" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Secondary Email:
                            </td>
                            <td>
                                <asp:Label ID="lblEmail2" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Phone 1:
                            </td>
                            <td>
                                <asp:Label ID="lblPhone1" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Phone 2:
                            </td>
                            <td>
                                <asp:Label ID="lblPhone2" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Address Line 1:
                            </td>
                            <td>
                                <asp:Label ID="lblAddressLine1" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Address Line 2:
                            </td>
                            <td>
                                <asp:Label ID="lblAddressLine2" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>City:
                            </td>
                            <td>
                                <asp:Label ID="lblCity" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>State:
                            </td>
                            <td>
                                <asp:Label ID="lblState" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Postal/Zip Code:
                            </td>
                            <td>
                                <asp:Label ID="lblPostalCode" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>Country:
                            </td>
                            <td>
                                <asp:Label ID="lblCountry" runat="server"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </div>
            </BodyContent>
        </sb:GroupBox>
    </div>
    <div style="float: left; margin-left: 35px; width: 51%" id="divProjects">
        <sb:GroupBox ID="GroupBox2" runat="server">
            <TitleLeftContent>
                Project Details
            </TitleLeftContent>
            <BodyContent>
                <div style="height: 280px; line-height: 20px; overflow-y: auto;">
                    <asp:ListView ID="lvUserCompanies" OnItemDataBound="lvUserCompanies_OnItemDataBound"
                        runat="server" AutoGenerateColumns="false">
                        <LayoutTemplate>
                            <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                        </LayoutTemplate>
                        <EmptyDataTemplate>
                            No projects found.
                        </EmptyDataTemplate>
                        <ItemTemplate>
                            <div>
                                <asp:LinkButton ID="lnkCompanyName" runat="server" />&nbsp
                                <asp:Label ID="lblRole" CssClass="RoleIndicater" runat="server"></asp:Label>
                                <div style="position: relative; top: 3px; display: inline;">
                                    <asp:Image ID="imgCompAdmin" Width="16" Height="16" ImageUrl="~/Common/Images/administrator.png"
                                        ToolTip="This user is also a Company Inventory Administrator" runat="server" Visible="false" />
                                </div>
                            </div>
                            <div style="margin-left: 20px;">
                                <asp:GridView ID="gvProjects" OnRowDataBound="gvProjects_OnDataBound" ShowHeader="false"
                                    AutoGenerateColumns="false" runat="server">
                                    <Columns>
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:Label ID="lblName" runat="server"></asp:Label>
                                                <asp:Label ID="lblRole" CssClass="RoleIndicater" runat="server"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                            <br />
                        </ItemTemplate>
                    </asp:ListView>
                </div>
            </BodyContent>
        </sb:GroupBox>
    </div>
    <div style="clear: both;">
    </div>
    <uc:UserActivity runat="server" ID="ucUserActivity" />
</asp:Content>
