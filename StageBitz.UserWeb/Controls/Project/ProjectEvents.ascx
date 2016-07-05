<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectEvents.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Project.ProjectEvents" %>
<script type="text/javascript">
    function EnablebtnAddKeyDate() {
        if (document.getElementById('<%= txtEvent.ClientID %>').value.length > 0) {
            document.getElementById('<%= btnAddKeyDate.ClientID %>').disabled = false;
        }
        else {
            document.getElementById('<%= btnAddKeyDate.ClientID %>').disabled = true;
        }
    }
</script>
<div style="margin-bottom: 5px; width: 550px;">
    <div style="width: 100%; margin-bottom: 2px;">
        <div style="float: left; width: 25%;">
            Project Starts:
        </div>
        <div>
            Project Ends:
        </div>
    </div>
    <div style="width: 100%; display: inline; vertical-align: top;">
        <div style="float: left; padding-top: 2px; width: 25%;">
            <telerik:RadDatePicker ID="StartDate" runat="server">
            </telerik:RadDatePicker>
        </div>
        <div id="divEndDate" runat="server" style="padding-top: 2px; float: left;">
            <telerik:RadDatePicker ID="EndDate" runat="server">
            </telerik:RadDatePicker>
        </div>
        <div style="float: left; margin-top: 4px;">
            <sb:HelpTip ID="HelpTip3" runat="server">
                If a date is still To Be Confirmed, just leave the field blank and it will show as TBC. You can update it at any time in Manage Schedule from your Project Dashboard.
            </sb:HelpTip>
        </div>
    </div>
    <div style="clear: both; padding-top: 10px; width: 100%; height: auto;">
        <asp:CompareValidator Operator="GreaterThanEqual" ID="CompareValidator1" ControlToCompare="StartDate"
            ValidationGroup="valgroup" ControlToValidate="EndDate" runat="server" ErrorMessage="Start date should be before End date."></asp:CompareValidator>
    </div>
</div>
<div style="clear: both;">
</div>
<div style="height: auto;">
    <div id="divEditScheduleMsg" style="margin-bottom:5px;" visible="false" runat="server">
        <div style="float: left;">
            <strong>Edit Schedule :</strong>
        </div>
        &nbsp;
        <div style="float: left; margin-left: 4px;">
            <sb:HelpTip ID="HelpTip1" runat="server">
                Changes to the Schedule will be seen by all members of the Project Team for this
                Project etc.
            </sb:HelpTip>
        </div>
    </div>
</div>
<div class="divBoxProjectEvents" style="text-align: left; width: 98%;">
    <asp:UpdatePanel ID="upnl" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <div style="width: 190px; float: left;">
                Event Name:
            </div>
            <div style="float: left;">
                Date:
            </div>
            <div style="clear: both;">
            </div>
            <div style="float: left;">
                <asp:TextBox ID="txtEvent" onkeypress="EnablebtnAddKeyDate()" onchange="EnablebtnAddKeyDate()"
                    MaxLength="100" Width="170" runat="server"></asp:TextBox>
                <asp:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender1" TargetControlID="txtEvent"
                    WatermarkText="e.g. Rehearsals start..." runat="server">
                </asp:TextBoxWatermarkExtender>
            </div>
            <div id="divEventDate" style="float: left; margin-left: 7px; padding-top: 2px;">
                &nbsp;
                <telerik:RadDatePicker ID="EventDate" runat="server">
                </telerik:RadDatePicker>
            </div>
            <div style="float: left; margin-top: 4px;">
                <sb:HelpTip ID="HelpTip2" runat="server">
                    Date is TBC, when no date is being selected.
                </sb:HelpTip>
            </div>
            <div style="float: left;">
                <asp:Button ID="btnAddKeyDate" CssClass="buttonStyle" ValidationGroup="keyDate" OnClick="AddToEventGrid"
                    Text="Add" runat="server"></asp:Button>
            </div>
            <div style="clear: both;">
            </div>
            <div style="height: 12px; width: 295px; float: left;">
                <asp:RequiredFieldValidator ValidationGroup="keyDate" ID="RequiredFieldValidator2"
                    ControlToValidate="txtEvent" runat="server" ErrorMessage="Key Date is required."></asp:RequiredFieldValidator>
            </div>
            <div style="height: 12px; float: left;">
                <asp:RequiredFieldValidator ValidationGroup="keyDate" ID="RequiredFieldValidator4"
                    Enabled="false" ControlToValidate="EventDate" runat="server" ErrorMessage="Date is required."></asp:RequiredFieldValidator>
            </div>
            <div style="clear: both;">
            </div>
            <telerik:RadGrid ID="gvEvents" GridLines="None" Width="100%" runat="server" AllowAutomaticDeletes="True"  AllowSorting="true"  SortToolTip="Click to sort"
                OnItemDataBound="gvEvents_ItemDataBound" AllowAutomaticInserts="false" OnNeedDataSource="gvEvents_NeedDataSource" 
                AllowAutomaticUpdates="True" OnUpdateCommand="gvEvents_UpdateCommand" AutoGenerateColumns="False"
                OnDeleteCommand="gvEvents_ItemDeleted">
                <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                <MasterTableView EditMode="InPlace" DataKeyNames="ProjectEventId,LastUpdatedDate">
                    <NoRecordsTemplate>
                        <div class="noData">
                            No data
                        </div>
                    </NoRecordsTemplate>
                    <Columns>
                        <telerik:GridTemplateColumn HeaderText="Event Name" SortExpression="EventName" UniqueName="EventName">
                            <ItemTemplate>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <div style="float: left; width: 80%">
                                    <asp:TextBox runat="server" ID="tbEventName" MaxLength="100" Text='<%# Bind("EventName") %>'>
                                    </asp:TextBox>
                                </div>
                                <div style="float: left; margin-top: 5px;">
                                    &nbsp;
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="tbEventName" 
                                        ErrorMessage="*" ToolTip="Key Date is required." runat="server">
                                    </asp:RequiredFieldValidator>
                                </div>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn UniqueName="EventDate" HeaderText="Date" DataType="System.DateTime"  SortExpression="EventDate" EditFormColumnIndex="1">
                            <HeaderStyle Width="170" />
                            <ItemStyle Width="170" />
                            <ItemTemplate>
                                <%# DisplayEventDate(StageBitz.UserWeb.Common.Helpers.Support.FormatDate(Eval("EventDate")))%>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <telerik:RadDatePicker runat="server" ID="tbEventDate" Width="40px" SelectedDate='<%# Bind("EventDate") %>'>
                                </telerik:RadDatePicker>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridEditCommandColumn ButtonType="ImageButton" UniqueName="EditCommandColumn">
                            <HeaderStyle Width="70" />
                            <ItemStyle Width="70" HorizontalAlign="Center" CssClass="MyImageButton" />
                        </telerik:GridEditCommandColumn>
                        <telerik:GridButtonColumn ConfirmText="Are you sure you want to delete this event?"
                            ConfirmDialogType="RadWindow" ConfirmDialogHeight="100" ConfirmTitle="Delete"
                            ButtonType="ImageButton" CommandName="Delete" Text="Delete" UniqueName="DeleteColumn">
                            <ItemStyle HorizontalAlign="Center" CssClass="MyImageButton" />
                            <HeaderStyle Width="60" />
                            <ItemStyle Width="60" />
                        </telerik:GridButtonColumn>
                    </Columns>
                </MasterTableView>
                <ClientSettings>
                    <Scrolling AllowScroll="True" UseStaticHeaders="True" SaveScrollPosition="True">
                    </Scrolling>
                </ClientSettings>
            </telerik:RadGrid>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnAddKeyDate" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</div>
