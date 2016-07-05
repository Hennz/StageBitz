<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectAdministration.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Company.ProjectAdministration" %>
<%@ Register TagPrefix="uc" TagName="BudgetList" Src="~/Controls/ItemBrief/BudgetList.ascx" %>
<sb:GroupBox ID="GroupBox1" runat="server">
    <TitleLeftContent>
        Administration
    </TitleLeftContent>
    <BodyContent>
        <div runat="server" id="divBudgetPanel" class="left" style="margin-top: 10px; padding: 10px;">
            <fieldset style="width:278px;">
                <legend>Budget</legend>
                <div style="margin-top: 10px;">
                    <uc:BudgetList ID="budgetList" runat="server" />
                </div>
            </fieldset>
        </div>
        <div class="left" style="margin-top: 10px; padding: 10px;">
            <fieldset>
                <legend>Project Information</legend>
                <div style="text-align: right;">
                    <a id="lnkProjectDetails" runat="server">Project Details </a>
                </div>
                <br />
                <div style="text-align: left; margin-bottom: 2px;">
                    <strong>Locations: </strong>
                </div>
                <div id="divLocationGrid" runat="server" style="max-height: 150px; width: 100%; text-align: left;
                    margin-bottom: 5px;">
                    <asp:GridView ID="gvLocations" ShowHeader="false" OnRowDataBound="gvLocations_OnRowDataBound"
                        AutoGenerateColumns="false" runat="server">
                        <Columns>
                            <asp:TemplateField ItemStyle-Width="60%" HeaderText="Name">
                                <ItemTemplate>
                                    <asp:Label ID="lblName" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    &nbsp;&nbsp;
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Location">
                                <ItemTemplate>
                                    <asp:Label ID="lblLocation" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div style="text-align: left;">
                    <asp:Literal ID="ltlMsg" runat="server"></asp:Literal>
                </div>
            </fieldset>
        </div>
        <div class="left" style="margin-top: 10px; padding: 10px;">
            <fieldset>
                <legend>Project Team</legend>
                <div style="text-align: right;">
                    <a id="lnkManageTeam" runat="server">Manage Project Team</a>
                </div>
                <div style="text-align: center; margin-top: 30px;">
                    <asp:Literal ID="ltrlProjectTeam" runat="server"></asp:Literal>
                </div>
            </fieldset>
        </div>
        <div style="clear: both;">
        </div>
    </BodyContent>
</sb:GroupBox>
