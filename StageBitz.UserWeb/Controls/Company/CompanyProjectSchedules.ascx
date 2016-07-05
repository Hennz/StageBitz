<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompanyProjectSchedules.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Company.CompanyProjectSchedules" %>
<sb:GroupBox ID="GroupBox1" runat="server">
    <TitleLeftContent>
        Schedule
        <sb:HelpTip ID="HelpTip1" runat="server">
            <p>
                You will see...
            </p>
        </sb:HelpTip>
    </TitleLeftContent>
    <BodyContent>
        <div style="float: left; height: 100px; width: 100%;">
            <div style="margin-left:10px; width: 45%; float: left; border: 0px Solid Black;">
                <asp:GridView ID="gvEventsLeft" CellPadding="10" runat="server" AutoGenerateColumns="false">
                    <Columns>
                     <asp:HyperLinkField HeaderStyle-Width="150" DataNavigateUrlFields="ProjectId" 
                            DataTextField="ProjectName" DataNavigateUrlFormatString="~/Project/ProjectDashboard.aspx?projectid={0}" />
                        <asp:BoundField HeaderStyle-Width="150" DataField="EventName" />
                        <asp:BoundField HeaderStyle-Width="150" DataField="EventDate" />
                    </Columns>
                     <EmptyDataTemplate>
                        There are no upcoming Key Dates
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
            <div style="width: 45%; float: left; border-left: 1px Solid Black; padding-left: 10px;
                margin-left: 10px;">
                <asp:GridView ID="gvEventsRight" runat="server" AutoGenerateColumns="false">
                    <Columns>
                        <asp:HyperLinkField HeaderStyle-Width="150" DataNavigateUrlFields="ProjectId" 
                            DataTextField="ProjectName" DataNavigateUrlFormatString="~/Project/ProjectDashboard.aspx?projectid={0}" />
                        <asp:BoundField HeaderStyle-Width="150" DataField="EventName" />
                        <asp:BoundField HeaderStyle-Width="150" DataField="EventDate" />
                    </Columns>
                   
                </asp:GridView>
            </div>
        </div>
        <div style="clear: both;">
        </div>
    </BodyContent>
</sb:GroupBox>
