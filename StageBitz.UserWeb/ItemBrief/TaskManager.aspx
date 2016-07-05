<%@ Page DisplayTitle="Task Manager" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="TaskManager.aspx.cs" Inherits="StageBitz.UserWeb.ItemBrief.TaskManager" %>

<%@ Register Src="~/Controls/Common/ExportData.ascx" TagName="ExportData" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectItemTypes.ascx" TagName="ProjectItemTypes" TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        if ('<%= this.CompanyId %>' != '0' && '<%= this.ProjectId %>' != '0') {
            var _gaq = _gaq || [];
            _gaq.push(['_setCustomVar', 1, 'Category', 'Project', 2]);
            _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyId %>', 2]);
            _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
            _gaq.push(['_setCustomVar', 4, 'ProjectId', '<%= this.ProjectId %>', 2]);
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    | <a id="lnkBookings" runat="server">Bookings</a>
    |<asp:HyperLink ID="hyperLinkTaskManager" CssClass="highlight" runat="server">Task Manager</asp:HyperLink>
    |<sb:projectupdateslink id="projectUpdatesLink" runat="server" />
    <sb:reportlist id="reportList" leftmargin="370" runat="server" />
</asp:Content>
<asp:Content ID="ContentNavigation" runat="server" ContentPlaceHolderID="PageTitleRight">
    <asp:PlaceHolder ID="plcItemTypeDD" runat="server">
        <sb:projectitemtypes id="projectItemTypes" runat="server" />
    </asp:PlaceHolder>
</asp:Content>


<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">

    <sb:projectwarningdisplay id="warningDisplay" runat="server" />
    <sb:ProjectWarningPopup id="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>

    <div>
        <br />
        <div id="div1" style="width: 49%; padding: 3px;">
            <div runat="server" id="divTitle" style="float: left;">
                <h4>
                    <asp:Literal ID="litTitleAT" runat="server"></asp:Literal>&nbsp;
                </h4>
            </div>
            <div style="width: 30px; float: left;">
                <sb:HelpTip id="HelpTip1" runat="server">
                    <p>
                        <strong>These tasks are from your
                            <asp:Literal runat="server" ID="litItemtype1"></asp:Literal>
                            Briefs.</strong>
                    </p>
                    <ol>
                        <li>Any tasks from your
                            <asp:Literal runat="server" ID="litItemtype2" />
                            Briefs you have yet to mark as complete will appear on
                            this list. </li>
                        <li>To add tasks to a List, just tick them, then select ‘Add Selected Tasks’ next to
                            the List you wish to add them to. </li>
                        <li>Tasks that have already been added to lists will appear in grey</li>
                        <li>You can add Tasks to more than one List; just hover on the Task to see which Lists
                            it’s on.</li>
                    </ol>
                </sb:HelpTip>
            </div>
            <div style="float: right;">
                <sb:ExportData id="ExportData1" runat="server" />
            </div>
        </div>
        <asp:UpdatePanel ID="upnlTaskManager" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <div id="divActiveTaskList" style="float: left; width: 49%; padding: 3px; clear: both;">
                    <div style="border: 1px solid grey; overflow-y: auto; overflow-x: hidden; min-height: 200px; clear: both; max-height: 500px;">
                        <asp:GridView ID="gridviewTaskList" runat="server" DataKeyNames="ItemBriefTaskId"
                            Width="100%" AutoGenerateColumns="False" BorderWidth="1px" ShowHeader="false"
                            CellPadding="0" BorderStyle="None" OnRowDataBound="gridviewTaskList_RowDataBound">
                            <Columns>
                                <asp:TemplateField>
                                    <ItemStyle Width="2%" />
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkBoxSelect" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField>
                                    <ItemStyle Width="30%" />
                                    <ItemTemplate>                                        
                                        <asp:Label ID="lblItemDescription" runat="server"></asp:Label>
                                        <telerik:RadToolTip runat="server" ID="radToolTipLists1" HideEvent="Default" CssClass="rtWrapperContent"
                                            Position="MiddleRight" Animation="Fade" AutoCloseDelay="0" RelativeTo="Element"
                                            TargetControlID="lblItemDescription">
                                            <asp:ListView ID="listViewTaskListToolTip1" runat="server">
                                                <LayoutTemplate>
                                                    <asp:Literal ID="litTitle" Text="Task List(s)" runat="server"></asp:Literal>
                                                    <div style="margin-left: -20px;">
                                                        <ul>
                                                            <li runat="server" id="itemPlaceholder"></li>
                                                        </ul>
                                                    </div>
                                                </LayoutTemplate>
                                                <ItemTemplate>
                                                    <li>                                                        
                                                        <%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("Name") ,25) %>
                                                    </li>
                                                </ItemTemplate>
                                            </asp:ListView>
                                        </telerik:RadToolTip>
                                    </ItemTemplate>
                                    <ItemStyle Font-Size="11px" />
                                </asp:TemplateField>
                                <asp:TemplateField>
                                    <ItemStyle Width="2%" />
                                    <ItemTemplate>
                                        <img runat="server" id="imgNoEstimatedCost" class="WarningIconForFinance" title="You haven't entered an estimated cost for this task." src="~/Common/Images/NoExpendedCostWarning.png" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Label ID="lblTaskDescription" runat="server"></asp:Label>
                                        <telerik:RadToolTip runat="server" ID="radToolTipLists2" HideEvent="Default" Position="MiddleRight"
                                            Animation="Fade" AutoCloseDelay="0" RelativeTo="Element" TargetControlID="lblTaskDescription">
                                            <asp:ListView ID="listViewTaskListToolTip2" runat="server">
                                                <LayoutTemplate>
                                                    <asp:Literal ID="litTitle" Text="Task List(s)" runat="server"></asp:Literal>
                                                    <div style="margin-left: -20px;">
                                                        <ul>
                                                            <li runat="server" id="itemPlaceholder"></li>
                                                        </ul>
                                                    </div>
                                                </LayoutTemplate>
                                                <ItemTemplate>
                                                    <li>
                                                        <%# StageBitz.UserWeb.Common.Helpers.Support.TruncateString(Eval("Name") ,25) %>
                                                    </li>
                                                </ItemTemplate>
                                            </asp:ListView>
                                        </telerik:RadToolTip>
                                    </ItemTemplate>
                                    <ItemStyle Font-Size="11px" />
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="noData" style="text-align: center;">
                                    No data
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="upnlTaskList" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <div id="divLists" style="float: right; width: 49%; margin-top: -25px;">
                    <div>
                        <div style="width: 88px; float: left;">
                            <h4>Manage Lists</h4>
                        </div>
                        <div style="width: 30px; float: left;">
                            <sb:HelpTip id="HelpTip2" runat="server">
                                <p>
                                    <strong>Create Lists to sort your Tasks into useful groups. For example, you might create:</strong>
                                </p>
                                <ol>
                                    <li>‘Fred’s Jobs’ ‘Buyer’ or ‘Carpentry’ (to assign tasks to a person or department)
                                    </li>
                                    <li>‘Hardware’, ‘Furniture’ or ‘eBay’ (to group types of purchases)</li>
                                    <li>‘Post-Project’ or ‘Daily Maintenance’ (to create tasks for a period in time)</li>
                                </ol>
                            </sb:HelpTip>
                        </div>
                    </div>
                    <div style="clear: both;">
                        <asp:Panel ID="pnlAddList" DefaultButton="btnAddList" runat="server">
                            <table>
                                <tr>
                                    <td>Name:&nbsp;
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtListName" Width="250px" MaxLength="100" runat="server"></asp:TextBox>
                                    </td>
                                    <td>
                                        <asp:Button ID="btnAddList" CssClass="buttonStyle" runat="server" Text="Add" ValidationGroup="FieldsValidation"
                                            OnClick="btnAddList_Click" />
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <div>
                            <asp:Label ID="lblError" CssClass="message error" runat="server"></asp:Label>
                            <asp:RequiredFieldValidator CssClass="message error" ID="rqdListName" runat="server"
                                ControlToValidate="txtListName" ErrorMessage="List Name is required." ValidationGroup="FieldsValidation"></asp:RequiredFieldValidator>
                        </div>
                        <br />
                        <div style="min-height: 200px; overflow-y: auto;">
                            <asp:ListView ID="listViewTaskLists" runat="server" OnItemCommand="listViewTaskLists_ItemCommand"
                                OnItemDataBound="listViewTaskLists_ItemDataBound">
                                <LayoutTemplate>
                                    <div style="width: 100%;" runat="server" id="itemPlaceholder">
                                    </div>
                                </LayoutTemplate>
                                <ItemTemplate>
                                    <div style="height: 40px;" class='<%# (Container.DataItemIndex % 2 == 0)?"gridAltRow":"gridRow" %>'>
                                        <div style="float: left;">
                                            <asp:Button ID="btnAddSelectedTask" CssClass="buttonStyle" Style="padding-top: -5px;"
                                                runat="server" Text="Add Selected Tasks" CommandName="AddTasks" CommandArgument='<%# Eval("TaskListId") %>' />
                                        </div>
                                        <div style="display: inline-block; padding-left: 5px; padding-top: 5px;">
                                            <a id="linkTaskList" runat="server"></a>
                                        </div>
                                        <div style="padding-left: 5px; font-size: 11px; text-align: right;">
                                            <asp:Literal ID="litTasks" runat="server"></asp:Literal>&nbsp;
                                            <asp:Literal ID="litActiveTasks" runat="server"></asp:Literal>&nbsp;
                                            <asp:Literal ID="litCompletedTasks" runat="server"></asp:Literal>
                                            <asp:Literal ID="litEmpty" Text="Empty" Visible="false" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                </ItemTemplate>
                                <EmptyDataTemplate>
                                    <div class="noData">
                                    </div>
                                </EmptyDataTemplate>
                            </asp:ListView>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div style="clear: both;">
        </div>
    </div>
    <div style="float: right;">
        <asp:Button ID="btnDone" CssClass="buttonStyle" runat="server" Text="Done" OnClick="btnDone_Click" />
    </div>
</asp:Content>
