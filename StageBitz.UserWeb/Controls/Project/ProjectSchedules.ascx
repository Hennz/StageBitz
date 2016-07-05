<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectSchedules.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Project.ProjectSchedule" %>
<sb:GroupBox ID="GroupBox1" runat="server">
    <TitleLeftContent>
        <div style="float: left;">
            <span class="boldText">Schedule</span>
        </div>
        <div style="float: left;">
            <sb:HelpTip ID="HelpTip1" runat="server">
                You will see up to 10 of the next Key Dates entered for your Project here. To edit
                Key Dates, just select 'Manage Schedule'.
            </sb:HelpTip>
        </div>
    </TitleLeftContent>
    <BodyContent>
        <div style="padding: 5px;">
            <div style="float: left; margin-right: 5px;">
                <b>Project Starts: </b>
            </div>
            <div style="float: left; min-width: 300px; text-align: left;">
                <asp:Literal ID="startDate" runat="server"></asp:Literal>
            </div>
            <div style="float: left; margin-right: 5px;">
                <b>Project Ends: </b>
            </div>
            <div style="float: left; min-width: 310px;">
                <asp:Literal ID="endDate" runat="server">&nbsp;</asp:Literal>
            </div>
            <div style="float: left;">
                <a id="lnkSchedule" runat="server">Manage Schedule </a>
            </div>
            <div style="clear: both;">
            </div>
            <div style="margin-top: 15px; margin-bottom: 5px; float: left;">
                <strong>
                    <asp:Literal ID="ltlEventFeedback" Text="Upcoming Key Dates:" Visible="false" runat="server"></asp:Literal></strong>
                <asp:Literal ID="ltlNoEventFeedback" Text="No Upcoming Key Dates." Visible="false"
                    runat="server"></asp:Literal>
            </div>
        </div>
        <div id="divGridArea" runat="server" style="float: left; padding-bottom: 5px; padding-left: 20px;
            border: 0px Solid Black; width: 98%;">
            <div id="divLeft" runat="server" style="padding-top: 2px; float: left;">
                <asp:GridView ID="gvEventsLeft" Width="280" ShowHeader="false" OnRowDataBound="gvEvents_OnRowDataBound"
                    AutoGenerateColumns="false" runat="server">
                    <Columns>
                        <asp:TemplateField ItemStyle-Width="200" HeaderText="Key Date">
                            <ItemTemplate>
                                <asp:Label ID="lblEventName" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-Width="80" HeaderText="Date">
                            <ItemTemplate>
                                <asp:Literal ID="litDate" runat="server"></asp:Literal>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div id="divMiddle" runat="server" style="padding-left: 5px; padding-top: 2px; float: left;">
                <asp:GridView ID="gvMiddle" Width="280" ShowHeader="false" OnRowDataBound="gvEvents_OnRowDataBound"
                    AutoGenerateColumns="false" runat="server">
                    <Columns>
                        <asp:TemplateField ItemStyle-Width="200" HeaderText="Key Date">
                            <ItemTemplate>
                                <asp:Label ID="lblEventName" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-Width="80" HeaderText="Date">
                            <ItemTemplate>
                                <asp:Literal ID="litDate" runat="server"></asp:Literal>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div style="padding-left: 5px; padding-top: 2px; float: left;">
                <asp:GridView ID="gvRight" Width="280" ShowHeader="false" OnRowDataBound="gvEvents_OnRowDataBound"
                    AutoGenerateColumns="false" runat="server">
                    <Columns>
                        <asp:TemplateField ItemStyle-Width="200" HeaderText="Key Date">
                            <ItemTemplate>
                                <asp:Label ID="lblEventName" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-Width="80" HeaderText="Date">
                            <ItemTemplate>
                                <asp:Literal ID="litDate" runat="server"></asp:Literal>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
        <div style="clear: both;">
        </div>
    </BodyContent>
</sb:GroupBox>
