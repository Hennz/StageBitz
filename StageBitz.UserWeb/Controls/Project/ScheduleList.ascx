<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScheduleList.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Project.ScheduleList" %>
<sb:GroupBox ID="GroupBox1" runat="server">
    <TitleLeftContent>
        <div style="float: left;">
           <span >Schedule</span>
        </div>
        <div style="float: left; ">
            <sb:HelpTip ID="HelpTip1" runat="server">
                     <ul>
                        <li>Every Project has Key Dates that can be entered via the project Dashboard.
                            </li>
                        <li>The next few Key Dates for each Project appear here.</li>
                        <li>As a date is past it is removed from the listing, allowing the next upcoming date to appear.</li>
                        <li>For a more detailed view of a Project's schedule, go to that Project Dashboard.</li>
                        <li>To edit Key Dates, go to that Projects Dashboard and select Manage Schedule.</li>
                    </ul>
            </sb:HelpTip>
        </div>
    </TitleLeftContent>
    <BodyContent>
          <div id="divNoEvents" runat="server" style="margin-top: 15px;margin-left:10px; margin-bottom: 5px; float: left;">
               No Upcoming Key Dates.
                    
            </div>
        <div id="divGridArea" runat="server" style="float: left; padding-bottom: 5px; padding-left: 20px;
            border: 0px Solid Black; width: 98%;">
            <div id="divLeft" runat="server" style="padding-top: 2px;line-height:20px; float: left;padding-right:10px; margin-right:10px;">
                <asp:GridView ID="gvEventsLeft"  Width="400" ShowHeader="false" OnRowDataBound="gvEvents_OnRowDataBound"
                    AutoGenerateColumns="false" runat="server">
                    <Columns>
                      <asp:TemplateField ItemStyle-Width="150" HeaderText="Key Date">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkProjectName" runat="server"></asp:LinkButton>
                                <asp:Label ID="lblProjectName" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-Width="130" HeaderText="Key Date">
                            <ItemTemplate>
                                <asp:Label ID="lblEventName" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-Width="70" HeaderText="Date">
                            <ItemTemplate>
                                <asp:Literal ID="litDate" runat="server"></asp:Literal>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div style="padding-left: 5px;line-height:20px;  padding-top: 2px; float: left;">
                <asp:GridView ID="gvRight" Width="400" ShowHeader="false" OnRowDataBound="gvEvents_OnRowDataBound"
                    AutoGenerateColumns="false" runat="server">
                    <Columns>
                         <asp:TemplateField ItemStyle-Width="150" HeaderText="Key Date">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkProjectName" runat="server"></asp:LinkButton>
                                <asp:Label ID="lblProjectName" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-Width="130" HeaderText="Key Date">
                            <ItemTemplate>
                                <asp:Label ID="lblEventName" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-Width="70" HeaderText="Date">
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