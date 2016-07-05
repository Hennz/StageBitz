<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserActivity.ascx.cs" Inherits="StageBitz.AdminWeb.Controls.Personal.UserActivity" %>


<div>
    <asp:UpdatePanel runat="server" ID="upnlProjectTeamActivity" UpdateMode="Conditional">
        <ContentTemplate>
            <sb:GroupBox ID="GroupBox" runat="server">
                <TitleLeftContent>
                    Activity
                </TitleLeftContent>
                <TitleRightContent>
                    Display:
                <telerik:RadMonthYearPicker runat="server" ID="monthfilter" AutoPostBack="true" OnSelectedDateChanged="monthfilter_SelectedDateChanged"></telerik:RadMonthYearPicker>
                </TitleRightContent>
                <BodyContent>

                    <table class="right">
                        <tr class="rgRow">
                            <td style="padding: 5px;">Number of Days Active: </td>
                            <td style="width: 100px;">
                                <asp:Label runat="server" ID="lblActiveDays"></asp:Label></td>
                        </tr>
                        <tr class="rgRow">
                            <td style="padding: 5px;">Number of Project Accessed: </td>
                            <td style="width: 100px;">
                                <asp:Label runat="server" ID="lblAccessedProjects"></asp:Label></td>
                        </tr>
                        <tr class="rgRow">
                            <td style="padding: 5px;">Number of Companies Accessed: </td>
                            <td style="width: 100px;">
                                <asp:Label runat="server" ID="lblAccessedCompanies"></asp:Label></td>
                        </tr>
                    </table>
                    <br style="clear: both;" />
                    <br />

                    <sb:SBRadGrid ID="gvUserActivity" Width="100%" runat="server" OnSortCommand="gvUserActivity_SortCommand" OnPageIndexChanged="gvUserActivity_PageIndexChanged" 
                        OnPageSizeChanged="gvUserActivity_PageSizeChanged" OnNeedDataSource="gvUserActivity_NeedDataSource"
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
                                <telerik:GridTemplateColumn HeaderText="Project" HeaderStyle-Width="20%" UniqueName="Project">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="lblProject"
                                            Text='<%# StageBitz.AdminWeb.Common.Helpers.Support.TruncateString(Eval("ProjectName"), 20) %>'
                                            ToolTip='<%# Eval("ProjectName") %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Role" HeaderStyle-Width="15%" UniqueName="Role">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="lblRole"
                                            Text='<%# StageBitz.AdminWeb.Common.Helpers.Support.TruncateString(Eval("Role"), 13) %>'
                                            ToolTip='<%# Eval("Role") %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Permission" HeaderStyle-Width="20%" UniqueName="Permission">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="lblPermission"
                                            Text='<%# StageBitz.AdminWeb.Common.Helpers.Support.TruncateString(Eval("Permission"), 18) %>'
                                            ToolTip='<%# Eval("Permission") %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Company" HeaderStyle-Width="20%" UniqueName="Company">
                                    <ItemTemplate>
                                        <asp:HyperLink runat="server" ID="lnkCompany"
                                            Text='<%# StageBitz.AdminWeb.Common.Helpers.Support.TruncateString(Eval("CompanyName"), 20) %>'
                                            ToolTip='<%# Eval("CompanyName") %>' NavigateUrl='<%# Eval("CompanyId", "~/Company/CompanyDetails.aspx?CompanyId={0}") %>'></asp:HyperLink> 
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
