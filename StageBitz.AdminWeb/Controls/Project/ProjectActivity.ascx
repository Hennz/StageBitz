<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectActivity.ascx.cs" Inherits="StageBitz.AdminWeb.Controls.Project.ProjectActivity" %>

<div>
    <asp:UpdatePanel runat="server" ID="upnlProjectTeamActivity" UpdateMode="Conditional">
        <ContentTemplate>
            <sb:GroupBox ID="GroupBox" runat="server">
                <TitleLeftContent>
                    Project/Team Activity
                </TitleLeftContent>
                <TitleRightContent>
                    Display:
                <telerik:RadMonthYearPicker runat="server" ID="monthfilter" AutoPostBack="true" OnSelectedDateChanged="monthfilter_SelectedDateChanged"></telerik:RadMonthYearPicker>
                </TitleRightContent>
                <BodyContent>

                    <table class="right">
                        <tr class="rgRow">
                            <td style="padding: 5px;">Number of Team Members Active: </td>
                            <td style="width: 100px;">
                                <asp:Label runat="server" ID="lblActiveTeamMembers"></asp:Label></td>
                        </tr>
                        <tr class="rgRow">
                            <td style="padding: 5px;">Number of Days with Activity: </td>
                            <td style="width: 100px;">
                                <asp:Label runat="server" ID="lblActiveDays"></asp:Label></td>
                        </tr>
                        <tr class="rgRow">
                            <td style="padding: 5px;">Number of Projects Accessed: </td>
                            <td style="width: 100px;">
                                <asp:Label runat="server" ID="lblAccessedProjects"></asp:Label></td>
                        </tr>
                    </table>
                    <br style="clear: both;" />
                    <br />

                    <sb:SBRadGrid ID="gvProjectTeamActivity" Width="100%" runat="server" OnSortCommand="gvProjectTeamActivity_SortCommand" OnPageIndexChanged="gvProjectTeamActivity_PageIndexChanged"
                        OnPageSizeChanged="gvProjectTeamActivity_PageSizeChanged" OnNeedDataSource="gvProjectTeamActivity_NeedDataSource"
                        AllowAutomaticInserts="false" AllowAutomaticDeletes="false" AutoGenerateColumns="false" AllowSorting="true" AllowPaging="true"
                        PagerStyle-AlwaysVisible="true">
                        <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                        <MasterTableView PageSize="50" AllowCustomPaging="true" AllowCustomSorting="true" AllowNaturalSort="false">
                            <SortExpressions>
                                <telerik:GridSortExpression FieldName="Date" SortOrder="Descending" />
                            </SortExpressions>
                            <PagerStyle AlwaysVisible="true" />
                            <NoRecordsTemplate>
                                <div class="noData">
                                    <asp:Label runat="server" ID="lblNoData" Text="No Data"></asp:Label>
                                    <asp:Label runat="server" ID="lblError" Text="" Visible="false" ForeColor="Red"></asp:Label>
                                </div>
                            </NoRecordsTemplate>
                            <Columns>
                                <telerik:GridTemplateColumn HeaderText="Date" HeaderStyle-Width="10%" SortExpression="Date" UniqueName="Date">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="lblDate"
                                            Text='<%# StageBitz.AdminWeb.Common.Helpers.Support.FormatDate(Eval("Date")) %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Project" HeaderStyle-Width="24%" UniqueName="Project">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="lblProject"
                                            Text='<%# StageBitz.AdminWeb.Common.Helpers.Support.TruncateString(Eval("ProjectName"), 24) %>'
                                            ToolTip='<%# Eval("ProjectName") %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="User" HeaderStyle-Width="20%" UniqueName="User">
                                    <ItemTemplate>
                                        <asp:HyperLink runat="server" ID="lnkUser"
                                            Text='<%# StageBitz.AdminWeb.Common.Helpers.Support.TruncateString(Eval("UserName"), 20) %>'
                                            ToolTip='<%# Eval("UserName") %>' NavigateUrl='<%# Eval("UserId", "~/User/UserDetails.aspx?ViewUserId={0}") %>'></asp:HyperLink> 
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Role" HeaderStyle-Width="13%" UniqueName="Role">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="lblRole"
                                            Text='<%# StageBitz.AdminWeb.Common.Helpers.Support.TruncateString(Eval("Role"), 13) %>'
                                            ToolTip='<%# Eval("Role") %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Permission" HeaderStyle-Width="18%" UniqueName="Permission">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="lblPermission"
                                            Text='<%# StageBitz.AdminWeb.Common.Helpers.Support.TruncateString(Eval("Permission"), 18) %>'
                                            ToolTip='<%# Eval("Permission") %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Session Total" HeaderStyle-Width="15%" SortExpression="SessionTotal" UniqueName="SessionTotal">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="lblSessionTotal"
                                            Text='<%# Eval("SessionTotal") %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                    </sb:SBRadGrid>
                </BodyContent>
            </sb:GroupBox>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
