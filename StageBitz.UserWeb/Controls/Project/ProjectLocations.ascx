<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectLocations.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Project.ProjectLocations" %>
<script type="text/javascript">
    $(document).ready(function () {
        var button = $("#<%= btnAddLocation.ClientID %>");
        button.attr("disabled", "disabled");
        var selected_value = $("#ddProjectDateTypes").val();
        $("#divEndDate").hide();
    });

    function EnablebtnAddLocation() {
        if (document.getElementById('<%= txtName.ClientID %>').value.length > 0 || document.getElementById('<%= txtLocation.ClientID %>').value.length > 0) {
            document.getElementById('<%= btnAddLocation.ClientID %>').disabled = false;
        }
        else {
            document.getElementById('<%= btnAddLocation.ClientID %>').disabled = true;
        }
    }


</script>
<div class="divBoxProjectEvents" style="text-align: left; width: 98%;">
    <asp:UpdatePanel ID="upLocation" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
             <asp:Panel ID="pnlSearchCriteria" DefaultButton="btnAddLocation" runat="server">
            <div style="width: 150px; float: left;">
                Name:
            </div>
            <div style="padding-left: 10px;">
                &nbsp;&nbsp; Location:
            </div>
            <div style="float: left; width: 150px; height: 40px;">
                <asp:TextBox ID="txtName" MaxLength="100" onchange="EnablebtnAddLocation()"
                    Width="150" runat="server"></asp:TextBox>
                <asp:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender1" TargetControlID="txtName"
                    WatermarkText="e.g. Studio 1..." runat="server">
                </asp:TextBoxWatermarkExtender>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtName"
                    ValidationGroup="LocValgroup" runat="server" ErrorMessage="Name is required."></asp:RequiredFieldValidator>
            </div>
            <div style="float: left; padding-left: 10px; width: 180px; height: 40px;">
                <asp:TextBox ID="txtLocation" MaxLength="100" 
                    onchange="EnablebtnAddLocation()" Width="180" runat="server"></asp:TextBox>
                <asp:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender2" TargetControlID="txtLocation"
                    WatermarkText="e.g. 3 Smith St, Smithville" runat="server">
                </asp:TextBoxWatermarkExtender>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="txtLocation"
                    ValidationGroup="LocValgroup" runat="server" ErrorMessage="Location is required."></asp:RequiredFieldValidator>
            </div>
            <div style="float: left; padding-left: 20px; height: 40px;">
                <asp:Button ID="btnAddLocation" CssClass="buttonStyle"  OnClick="AddToLocationGrid" ValidationGroup="LocValgroup"
                    Text="Add" runat="server"></asp:Button>
            </div>
             <div style="float: left; padding-left: 4px; padding-top:5px;">
            <sb:HelpTip ID="helpTipProjectDetailsLocations" runat="server">
                Locations will be seen by all members of the Project Team working
                on this Project.
            </sb:HelpTip>
        </div>
             <div style="clear: both; height: 1px;">
            </div>
            <telerik:RadGrid ID="gvLocations" Width="100%" GridLines="None" runat="server" AllowAutomaticDeletes="True"  SkinID="Purple"
                OnItemDataBound="gvLocations_ItemDataBound" AllowAutomaticInserts="false" OnNeedDataSource="gvLocations_NeedDataSource"
                AllowAutomaticUpdates="True" OnUpdateCommand="gvLocations_UpdateCommand" AutoGenerateColumns="False"
                OnDeleteCommand="gvLocations_ItemDeleted">
                <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                <%--<ValidationSettings EnableValidation="true" ValidationGroup="valgroup" CommandsToValidate="PerformInsert,Update" />--%>
                <MasterTableView EditMode="InPlace" TableLayout="Fixed" DataKeyNames="ProjectLocationId,LastUpdatedDate" Width="100%">
                    <NoRecordsTemplate>
                        <div class="noData">
                            No data
                        </div>
                    </NoRecordsTemplate>
                    <Columns>
                        <%--                <telerik:GridBoundColumn DataField="Name" MaxLength="100" HeaderText="Name" UniqueName="Name"
                            ColumnEditorID="GridTextBoxColumnEditor1">
                            <HeaderStyle Width="250" />
                            <ItemStyle Width="240" />
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn DataField="Location" MaxLength="100" HeaderText="Location"
                            UniqueName="Location" ColumnEditorID="GridTextBoxColumnEditor2">
                        </telerik:GridBoundColumn>--%>
                        <telerik:GridTemplateColumn HeaderText="Name" UniqueName="Name">
                            <ItemStyle Width="45%" />
                            <ItemTemplate>
                            </ItemTemplate>
                            <HeaderStyle Width="45%" />
                            <EditItemTemplate>
                                <div style="float: left; width: 80%">
                                    <asp:TextBox runat="server" MaxLength="100" ID="tbName">                                
                                    </asp:TextBox>
                                </div>
                                <div style="float: left; margin-top: 5px;">
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="tbName" 
                                        runat="server" ErrorMessage="*" ToolTip="Name is required."></asp:RequiredFieldValidator>
                                </div>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Location" UniqueName="Location">
                            <ItemStyle Width="40%" />
                            <ItemTemplate>
                            </ItemTemplate>
                            <HeaderStyle Width="52%" />
                            <EditItemTemplate>
                                <div style="float: left; width: 85%">
                                    <asp:TextBox runat="server" MaxLength="100" ID="tbLocation">
                                    </asp:TextBox>
                                </div>
                                <div style="float: left; margin-top: 5px;">
                                  
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="tbLocation" 
                                        runat="server" ErrorMessage="*" ToolTip="Location is required."></asp:RequiredFieldValidator>
                                </div>
                            </EditItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridEditCommandColumn ButtonType="ImageButton" UniqueName="EditCommandColumn">
                            <HeaderStyle Width="60" />
                            <ItemStyle Width="60" HorizontalAlign="Center" CssClass="MyImageButton" />
                        </telerik:GridEditCommandColumn>
                        <telerik:GridButtonColumn ConfirmText="Are you sure you want to delete this location?"
                            ConfirmDialogType="RadWindow" ConfirmTitle="Delete" ButtonType="ImageButton"
                            ConfirmDialogHeight="100" CommandName="Delete" Text="Delete" UniqueName="DeleteColumn">
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
            <telerik:GridTextBoxColumnEditor ID="GridTextBoxColumnEditor1" runat="server" TextBoxStyle-Width="240">
            </telerik:GridTextBoxColumnEditor>
            <telerik:GridTextBoxColumnEditor ID="GridTextBoxColumnEditor2" runat="server" />
                 </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnAddLocation" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</div>
