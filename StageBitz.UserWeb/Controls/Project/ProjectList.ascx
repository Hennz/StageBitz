<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectList.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Project.ProjectList" %>
<%@ Register Src="~/Controls/Common/InvitationViewer.ascx" TagName="InvitationViewer"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/ListViewDisplaySettings.ascx" TagName="ListViewDisplaySettings"
    TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/CreateNewProjectLink.ascx" TagPrefix="sb" TagName="CreateNewProjectLink" %>

<asp:UpdatePanel ID="upnlProjectList" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <script type="text/javascript">
            $(function () { $("a.lnkProject").on('click', function () { return false; }); });

            function RadToolTip_OnClientShow(sender, eventArgs) {
                showMultipleToolTips("showOnLoad");
            }

            function RadToolTip_OnClientBeforeShow(sender, eventArgs) {
                sender._popupBehavior.set_keepInScreenBounds(false);
            }

        </script>
        <sb:InvitationViewer ID="invitationViewer" OnInvitationStatusChanged="invitationViewer_InvitationStatusChanged"
            runat="server" />
        <sb:GroupBox ID="GroupBox1" runat="server">
            <TitleLeftContent>
                <asp:Label ID="lblLeftHeader" runat="server" Text="Label"></asp:Label>
                <sb:HelpTip ID="HelpTip1" runat="server">
                    <p>
                        <strong>Here's how you can get started.</strong>
                    </p>
                    <ol>
                        <li>Projects may only be created by a Company. If you are already a StageBitz Company
                            Administrator, select the appropriate Company from the Companies section above
                            and create a new Project from there. </li>
                        <li>
                        You can create a Company account by following the 'Create New Company' link in the Companies 
                            section above. 
                        <li>You can wait for someone to invite you to their Project.</li>
                    </ol>
                </sb:HelpTip>
            </TitleLeftContent>
            <TitleRightContent>
                <sb:ListViewDisplaySettings ID="displaySettings" OnDisplayModeChanged="displaySettings_DisplayModeChanged"
                    runat="server" />
            </TitleRightContent>
            <BodyContent>
                <div style="text-align: right;">
                    <sb:CreateNewProjectLink runat="server" ID="hLinkAddNewProject" LinkText="Create New Project" Visible="false" style="margin: 5px;" />
                </div>
                <div id="divNoProjects" runat="server" visible="false" style="text-align: center; padding-top: 12px; padding-right: 12px; padding-bottom: 12px; padding-left: 12px;">
                    There are no active Projects for your Company.
                    <br />
                    <span runat="server" id="spanCreateNewProject">Ready to get started?
                        <sb:CreateNewProjectLink runat="server" ID="lnkCreateNewProject" LinkText="Create a Project" />
                        now.</span>
                </div>

                <div id="divNoProjectsUserDashBoard" runat="server" visible="false" style="padding-top: 12px; padding-right: 12px; padding-bottom: 12px; padding-left: 12px;">
                    Here's where you'll see any Projects that you are currently invited to, and any new invitations to Projects.
                    <br />
                    <br />
                    If you want to start a new Project:
                    <br />
                    <ul>
                        <li>See the Companies Section (just above here)</li>
                        <li>Open the Company that you want to create the Project for, and</li>
                        <li>Select 'Create New Project'. Next time you're back here, the Project will appear.</li>
                    </ul>
                </div>

                <div id="divNotification" runat="server" visible="false" class="message success" style="margin-bottom: 5px;">
                </div>

                <div id="divListViewProjects" style="max-height: 600px; overflow-y: auto; overflow-x: hidden;"
                    runat="server">
                    <asp:ListView ID="lvProjects" runat="server" OnItemDataBound="lvProjects_ItemDataBound"
                        OnItemCommand="lvProjects_ItemCommand">
                        <LayoutTemplate>
                            <div>
                                <ul class="boxedItemDisplay">
                                    <li runat="server" id="itemPlaceholder"></li>
                                </ul>
                            </div>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <li>
                                <asp:PlaceHolder ID="plcProjectMemberView" runat="server">
                                    <div id="divNotificationArea" runat="server" class="projectTileNotificationArea">
                                        <div id="divProjectWarning" title="There is a message on this Project's dashboard." visible="false" runat="server" class="boxProjectWarning"></div>
                                        <div id="divProjectSuspended" title="This Project is Suspended." visible="false" runat="server" class="boxProjectSuspended"></div>
                                        <a id="lnkNotificationCount" runat="server" title="View Project Updates" class="boxNotificationBadge" style="color: White !important;">0</a>
                                    </div>
                                    <asp:HyperLink ID="lnkBtnProject" runat="server"></asp:HyperLink>
                                    <b>
                                        <asp:Label ID="lblProject" runat="server" Visible="false"></asp:Label></b>
                                    <br />
                                    <asp:HyperLink ID="lnkBtnCompany" CssClass="smallText" runat="server"></asp:HyperLink>
                                    <span id="spnCompany" class="smallText" runat="server"></span>
                                    <br />
                                    <br />
                                    <strong>
                                        <asp:Literal ID="litItems" runat="server"></asp:Literal></strong>
                                    <br />
                                    <br />
                                    <asp:Literal ID="litCompleted" runat="server"></asp:Literal>
                                    <br />
                                    <asp:Literal ID="litInProgress" runat="server"></asp:Literal>
                                    <br />
                                    <asp:Literal ID="litNotstarted" runat="server"></asp:Literal>
                                    <br />
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="plcInvitationView" runat="server">
                                    <asp:HyperLink ID="lnkBtnProjectInvt" runat="server"></asp:HyperLink>
                                    <asp:Literal ID="litProjectName" runat="server"></asp:Literal>
                                    <br />
                                    <span class="smallText">
                                        <asp:HyperLink ID="lnkBtnCompanyInvt" CssClass="smallText" runat="server"></asp:HyperLink>
                                        <asp:Literal ID="litCompanyName" runat="server"></asp:Literal></span>
                                    <br />
                                    <br />
                                    <span style="font-style: italic; color: Red;">Invite Pending...</span>
                                    <br />
                                    <br />
                                    <br />
                                    <asp:LinkButton ID="lnkbtnViewInvite" runat="server" CommandName="ViewInvite" ClientIDMode="AutoID">View Invite</asp:LinkButton>
                                    <br />
                                    <br />
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="plcClosedProjectsView" runat="server">
                                    <div class="projectTileNotificationArea">
                                        <div title="This Project is Closed." class="boxProjectClosed"></div>
                                    </div>
                                    <asp:HyperLink ID="lnkProjectClosed" runat="server"></asp:HyperLink>
                                    <b>
                                        <asp:Label ID="lblProjectClosed" runat="server" Visible="false"></asp:Label></b>
                                    <br />
                                    <br />
                                    <strong>Closed</strong>
                                    <br />
                                    <br />
                                    On:&nbsp;<asp:Label ID="lblClosedDate" runat="server"></asp:Label>
                                    <br />
                                    By:&nbsp;<asp:Label ID="lblClosedBy" runat="server"></asp:Label>
                                    <br />
                                    <div style="min-height: 32px;"></div>
                                </asp:PlaceHolder>
                            </li>
                        </ItemTemplate>
                    </asp:ListView>

                    <%--this tooltip is for the 'lvProjects' list view--%>
                    <telerik:RadToolTip runat="server" ID="helptipProjectLink" VisibleOnPageLoad="true" HideEvent="ManualClose"
                        TargetControlID="lnkBtnProject" Position="BottomRight" CssClass="showOnLoad"
                        OnClientShow="RadToolTip_OnClientShow" OffsetY="-5" Visible="false" ShowEvent="FromCode"
                        OnClientBeforeShow="RadToolTip_OnClientBeforeShow">
                        <div style="text-align: left; padding: 10px;">
                            <p><b>This is your free trial Project.</b></p>
                            <br />
                            <div style="padding-left: 10px;">
                                <p>
                                    - Follow this link to the Project Dashboard and explore the Project Management tools.
                                </p>
                            </div>
                        </div>
                    </telerik:RadToolTip>
                </div>
                <div style="max-height: 300px; overflow-y: auto; overflow-x: hidden;">
                    <div id="divProjectInvitations" runat="server">
                        <div>
                            <h4>Project Invitations</h4>
                        </div>
                        <telerik:RadGrid ID="gvProjectInvitations" EnableLinqExpressions="False" AllowSorting="false"
                            OnItemDataBound="gvProjectInvitations_ItemDataBound" AutoGenerateColumns="false" OnItemCommand="gvProjectInvitations_ItemCommand"
                            runat="server">
                            <MasterTableView AllowNaturalSort="false" CellPadding="0" CellSpacing="0" AllowMultiColumnSorting="false"
                                HeaderStyle-Font-Size="11px" Font-Size="11px">
                                <NoRecordsTemplate>
                                    <div class="noData">
                                        No data
                                    </div>
                                </NoRecordsTemplate>
                                <Columns>
                                    <telerik:GridTemplateColumn HeaderText="Project Name" ItemStyle-Width="30%" HeaderStyle-Width="30%">
                                        <ItemTemplate>
                                            <a id="lnkProject" runat="server" href="#"></a>
                                            <asp:Literal ID="litProject" Visible="false" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn HeaderText="Company Name" ItemStyle-Width="30%" HeaderStyle-Width="30%">
                                        <ItemTemplate>
                                            <a id="lnkCompany" runat="server" href="#"></a>
                                            <asp:Literal ID="litCompany" Visible="false" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn DataField="ProjectRole" HeaderText="Project Role" ItemStyle-Width="30%"
                                        HeaderStyle-Width="30%">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridTemplateColumn HeaderText="" ItemStyle-Width="15%" HeaderStyle-Width="15%">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkbtnViewInvite" runat="server" CommandName="ViewInvite">View Invite</asp:LinkButton>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                </Columns>
                            </MasterTableView>
                        </telerik:RadGrid>
                        <br />
                    </div>
                    <div id="divGridViewProjects" runat="server">
                        <div>
                            <h4 runat="server" id="h4ActiveProjects">Active Projects</h4>
                        </div>
                        <telerik:RadGrid ID="gvProjects" EnableLinqExpressions="False" AllowSorting="false"
                            OnItemDataBound="gvProjects_ItemDataBound" AutoGenerateColumns="false" runat="server">
                            <MasterTableView AllowNaturalSort="false" CellPadding="0" CellSpacing="0" AllowMultiColumnSorting="false">
                                <NoRecordsTemplate>
                                    <div class="noData">
                                        No data
                                    </div>
                                </NoRecordsTemplate>
                                <Columns>
                                    <telerik:GridTemplateColumn HeaderText="Project Name" ItemStyle-Width="25%" HeaderStyle-Width="25%">
                                        <ItemTemplate>
                                            <a id="lnkProject" runat="server" href="#"></a>
                                            <asp:Label ID="lblProject" runat="server" Visible="false"></asp:Label>
                                            <a id="lnkNotificationCount" runat="server" title="View Project Updates" class="inlineNotificationBadge" style="color: White !important;">0</a>
                                            <div id="divProjectWarning" title="There is a message on this Project's dashboard." visible="false" runat="server" class="inlineBoxProjectWarning"></div>
                                            <div id="divProjectSuspended" title="This Project is Suspended." visible="false" runat="server" class="inlineBoxProjectSuspended"></div>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn HeaderText="Company Name" ItemStyle-Width="18%" HeaderStyle-Width="18%">
                                        <ItemTemplate>
                                            <a id="lnkCompany" runat="server" href="#"></a>
                                            <asp:Literal ID="litCompany" Visible="false" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn DataField="ProjectRole" HeaderText="Project Role" ItemStyle-Width="10%"
                                        HeaderStyle-Width="10%">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridTemplateColumn HeaderText="Ends" ItemStyle-Width="11%" HeaderStyle-Width="11%">
                                        <ItemTemplate>
                                            <%# StageBitz.UserWeb.Common.Helpers.Support.FormatDate(Eval("EndDate")) %>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn DataField="ItemCount" HeaderText="Item Briefs" ItemStyle-Width="10%"
                                        HeaderStyle-Width="10%">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="CompletedItemCount" HeaderText="Completed" ItemStyle-Width="8%"
                                        HeaderStyle-Width="8%">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="InProgressItemCount" HeaderText="In Progress"
                                        SortExpression="StatusSortOrder" ItemStyle-Width="9%" HeaderStyle-Width="9%">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="NotStartedItemCount" HeaderText="Not Started" HeaderStyle-Width="13%"
                                        SortExpression="StatusSortOrder">
                                    </telerik:GridBoundColumn>
                                </Columns>
                            </MasterTableView>
                        </telerik:RadGrid>
                    </div>
                    <div id="divGridViewClosedProjects" runat="server" visible="false">
                        <br />
                        <div>
                            <h4>Closed Projects</h4>
                        </div>
                        <telerik:RadGrid ID="gvClosedProjects" EnableLinqExpressions="False" AllowSorting="false"
                            OnItemDataBound="gvClosedProjects_ItemDataBound" AutoGenerateColumns="false" runat="server">
                            <MasterTableView AllowNaturalSort="false" CellPadding="0" CellSpacing="0" AllowMultiColumnSorting="false">
                                <NoRecordsTemplate>
                                    <div class="noData">
                                        No data
                                    </div>
                                </NoRecordsTemplate>
                                <Columns>
                                    <telerik:GridTemplateColumn HeaderText="Project Name" HeaderStyle-Width="28%">
                                        <ItemTemplate>
                                            <a id="lnkProject" runat="server" href="#"></a>
                                            <asp:Label ID="lblProject" runat="server" Visible="false"></asp:Label>
                                            <div title="This Project is Closed." class="inlineBoxProjectClosed"></div>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn HeaderText="Company Name" HeaderStyle-Width="18%">
                                        <ItemTemplate>
                                            <a id="lnkCompany" runat="server" href="#"></a>
                                            <asp:Literal ID="litCompany" Visible="false" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn HeaderText="Closed By" HeaderStyle-Width="21%">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="lblClosedBy"
                                                ToolTip='<%# Eval("ClosedByName") %>' Text='<%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("ClosedByName"), 25) %>'></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn HeaderText="Closed On" HeaderStyle-Width="11%">
                                        <ItemTemplate>
                                            <%# StageBitz.UserWeb.Common.Helpers.Support.FormatDate(Eval("ClosedOn")) %>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn DataField="ItemCount" HeaderText="Item Briefs" HeaderStyle-Width="10%">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="CompletedItemCount" HeaderText="Completed" HeaderStyle-Width="8%">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="InProgressItemCount" HeaderText="In Progress"
                                        SortExpression="StatusSortOrder" HeaderStyle-Width="9%">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="NotStartedItemCount" HeaderText="Not Started" HeaderStyle-Width="10%"
                                        SortExpression="StatusSortOrder">
                                    </telerik:GridBoundColumn>
                                </Columns>
                            </MasterTableView>
                        </telerik:RadGrid>
                    </div>
                </div>
            </BodyContent>
        </sb:GroupBox>
    </ContentTemplate>
</asp:UpdatePanel>
