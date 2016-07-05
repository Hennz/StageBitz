<%@ Page DisplayTitle="Manage Skills" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="Skills.aspx.cs" Inherits="StageBitz.AdminWeb.User.Skills" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <telerik:RadWindowManager ID="mgr" runat="server">
    </telerik:RadWindowManager>
    <div class="left" style="width: 46%;">
        <h2>
            Default Skills</h2>
        <asp:UpdatePanel ID="upnlDefaultSkills" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <asp:Panel ID="pnlAddNewSkill" Height="35px" CssClass="sideErrorContainer" DefaultButton="btnAddSkill"
                    runat="server">
                    <asp:TextBox ID="txtNewSkill" Width="180" MaxLength="100" runat="server"></asp:TextBox>
                    <asp:Button ID="btnAddSkill" runat="server" CssClass="buttonStyle" Style="float: none;"
                        Text="Add Skill" ValidationGroup="NewDefaultSkill" OnClick="btnAddSkill_Click" />
                    <asp:RequiredFieldValidator ID="reqNewSkillName" ControlToValidate="txtNewSkill"
                        ErrorMessage="* Skill name is required." ToolTip="Skill name is required." ValidationGroup="NewDefaultSkill"
                        runat="server">
                    </asp:RequiredFieldValidator>
                </asp:Panel>
                <sb:PopupBox ID="popupDefaultSkillMessage" Title="Manage Skills" runat="server">
                    <BodyContent>
                        A skill with the same name already exists.
                    </BodyContent>
                    <BottomStripeContent>
                        <input id="btnDefaultSkillOK" type="button" value="OK" class="buttonStyle popupBoxCloser" />
                    </BottomStripeContent>
                </sb:PopupBox>
                <asp:Panel ID="Panel1" DefaultButton="btnDummy" runat="server">
                    <sb:SBRadGrid ID="gvDefaultSkills" Width="100%" runat="server" OnSortCommand="gvDefaultSkills_SortCommand"
                        OnItemDataBound="gvDefaultSkills_ItemDataBound" OnDeleteCommand="gvDefaultSkills_DeleteCommand"
                        OnUpdateCommand="gvDefaultSkills_UpdateCommand" AllowSorting="true" SortToolTip="Click to sort"
                        AllowPaging="true" SortedBackColor="Transparent" OnNeedDataSource="gvDefaultSkills_NeedDataSource"
                        AutoGenerateColumns="False" AllowAutomaticDeletes="True">
                        <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                        <MasterTableView DataKeyNames="SkillId" AllowNaturalSort="false" EditMode="InPlace"
                            PageSize="50">
                            <PagerStyle AlwaysVisible="true" />
                            <NoRecordsTemplate>
                                <div class="noData">No records to display.</div>
                            </NoRecordsTemplate>
                            <Columns>
                                <telerik:GridTemplateColumn HeaderText="Skill" SortExpression="Name" UniqueName="Name"
                                    ItemStyle-Width="230px" HeaderStyle-Width="230px">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <div style="float: left; width: 90%">
                                            <asp:TextBox runat="server" ID="txtName" MaxLength="100" Text='<%# Bind("Name") %>'
                                                Style="margin: 2px;">
                                            </asp:TextBox>
                                        </div>
                                        <div style="float: left; margin-top: 5px;">
                                            &nbsp;
                                            <asp:RequiredFieldValidator ID="reqSkillName" ControlToValidate="txtName" ErrorMessage="*"
                                                ToolTip="Skill name is required." runat="server">
                                            </asp:RequiredFieldValidator>
                                        </div>
                                    </EditItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn HeaderText="Usage" DataField="Usage" SortExpression="Usage"
                                    ReadOnly="true">
                                    <HeaderStyle HorizontalAlign="Center" Width="55" />
                                    <ItemStyle HorizontalAlign="Center" Width="55" />
                                </telerik:GridBoundColumn>
                                <telerik:GridEditCommandColumn ButtonType="ImageButton" UniqueName="EditCommandColumn">
                                    <HeaderStyle Width="45" />
                                    <ItemStyle Width="45" HorizontalAlign="Center" />
                                </telerik:GridEditCommandColumn>
                                <telerik:GridButtonColumn ConfirmText="Are you sure you want to delete this skill from the system?"
                                    ConfirmDialogType="RadWindow" ConfirmDialogHeight="100" ConfirmTitle="Delete"
                                    ButtonType="ImageButton" CommandName="Delete" Text="Delete" UniqueName="DeleteColumn">
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle Width="30" />
                                    <ItemStyle Width="30" />
                                </telerik:GridButtonColumn>
                            </Columns>
                            <SortExpressions>
                                <telerik:GridSortExpression FieldName="Name" />
                            </SortExpressions>
                        </MasterTableView>
                    </sb:SBRadGrid>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div class="right" style="width: 46%;">
        <h2>
            Custom Skills</h2>
        <div class="grayText" style="height: 35px;">
            Skills that have been added by users.</div>
        <asp:UpdatePanel ID="upnlCustomSkills" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <asp:Panel ID="Panel2" DefaultButton="btnDummy" runat="server">
                    <sb:SBRadGrid ID="gvCustomSkills" Width="100%" runat="server" OnSortCommand="gvCustomSkills_SortCommand"
                        OnItemDataBound="gvCustomSkills_ItemDataBound" OnItemCommand="gvCustomSkills_ItemCommand"
                        AllowSorting="true" SortToolTip="Click to sort" AllowPaging="true" SortedBackColor="Transparent"
                        OnNeedDataSource="gvCustomSkills_NeedDataSource" AutoGenerateColumns="False"
                        AllowAutomaticDeletes="True">
                        <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                        <MasterTableView DataKeyNames="UserSkillId" AllowNaturalSort="false" EditMode="InPlace" PageSize="50">
                            <PagerStyle AlwaysVisible="true" />
                            <NoRecordsTemplate>
                                <div class="noData">No records to display.</div>
                            </NoRecordsTemplate>
                            <Columns>
                                <telerik:GridTemplateColumn HeaderText="Skill" UniqueName="Name" SortExpression="Name"
                                    ReadOnly="true">
                                    <HeaderStyle Width="230" />
                                    <ItemStyle Width="230" />
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn HeaderText="Usage" DataField="Usage" SortExpression="Usage"
                                    ReadOnly="true">
                                    <HeaderStyle HorizontalAlign="Center" Width="55" />
                                    <ItemStyle HorizontalAlign="Center" Width="55" />
                                </telerik:GridBoundColumn>
                                <telerik:GridButtonColumn ConfirmText="Are you sure you want to add this custom Skill to the default Skill list?"
                                    ConfirmDialogType="RadWindow" ConfirmDialogHeight="160" ConfirmTitle="Add to Default Skills"
                                    ButtonType="ImageButton" ImageUrl="~/Common/Images/Keep.png" CommandName="Keep"
                                    Text="Add to Default Skills" UniqueName="KeepColumn">
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle Width="30" />
                                    <ItemStyle Width="30" />
                                </telerik:GridButtonColumn>
                                <telerik:GridButtonColumn ConfirmText="Are you sure you want to ignore this custom Skill?"
                                    ConfirmDialogType="RadWindow" ConfirmDialogHeight="160" ConfirmTitle="Ignore Skill"
                                    ButtonType="ImageButton" ImageUrl="~/Common/Images/Ignore.png" CommandName="Ignore"
                                    Text="Ignore" UniqueName="IgnoreColumn">
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle Width="30" />
                                    <ItemStyle Width="30" />
                                </telerik:GridButtonColumn>
                            </Columns>
                            <SortExpressions>
                                <telerik:GridSortExpression FieldName="Name" />
                            </SortExpressions>
                        </MasterTableView>
                    </sb:SBRadGrid>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <asp:Button ID="btnDummy" runat="server" Text="Dummy" Style="display: none;" OnClientClick="return false;" />
    <div style="clear: both;">
    </div>
</asp:Content>
