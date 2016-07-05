<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserSkills.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Personal.UserSkills" %>

<script type="text/javascript">

    //This is maintained to prevent add event being fired multiple times for a single user action.
    //(The event can be fired twice when pressing 'Enter' kry in the suggestion list)
    var addEventFired = false;

    function cboSkills_onItemsRequested(sender, eventArgs) {

        //iIf the suggestion list has no items, hide the empty list area.
        var itemCount = sender.get_items().get_count();

        if (itemCount == 0)
            sender.hideDropDown();
    }

    function cboSkills_onSelectedIndexChanged(sender, eventArgs) {

        if (!addEventFired) {
            addEventFired = true;
            $("#<%= btnAdd.ClientID %>").click();
        }
    }

    function cboSkills_onKeyPressing(sender, eventArgs) {

        if (eventArgs.get_domEvent().keyCode == 13) { //Enter key
            if (!addEventFired) {
                addEventFired = true;
                $("#<%= btnAdd.ClientID %>").click();
            }
        }
    }

    function setSkillAddedStatus() {
        //This hidden field value will be used to verify whether the postback has occured from a skill addition or not.
        //Otherwise ComboBox selected index changed event may fired after any other postback.
        $("#<%= hdnSkillAdded.ClientID %>").val("1");
    }
    
</script>


<asp:UpdatePanel ID="upnlUserSkillsAdd" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
    
        <telerik:RadComboBox runat="server" ID="cboSkills" AutoPostBack="false" OnItemsRequested="cboSkills_ItemsRequested" OnSelectedIndexChanged="cboSkills_SelectedIndexChanged"
            OnClientItemsRequested="cboSkills_onItemsRequested" OnClientSelectedIndexChanged="cboSkills_onSelectedIndexChanged" OnClientKeyPressing="cboSkills_onKeyPressing"
            EnableLoadOnDemand="true" EmptyMessage="Type a skill..." ChangeTextOnKeyBoardNavigation="true" ShowWhileLoading="false" ShowToggleImage="false" MaxLength="100">
            <ExpandAnimation Type="None" />
            <CollapseAnimation Type="None" />
        </telerik:RadComboBox>
        <asp:Button ID="btnAdd" runat="server" Text="Add" CssClass="buttonStyle" style="margin-left:5px; float:none;" OnClientClick="setSkillAddedStatus();" />
        <asp:HiddenField ID="hdnSkillAdded" runat="server" />

        <asp:Literal ID="ltrlStatus" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>

<div style="margin-top:5px; width:430px; max-height:350px; overflow-x:hidden; overflow-y:auto;">
<asp:UpdatePanel ID="upnlUserSkillList" UpdateMode="Conditional" runat="server">
    <ContentTemplate>

        <asp:ListView ID="lvSkillsEditable" OnItemDataBound="lvSkillsEditable_ItemDataBound" OnItemDeleting="lvSkillsEditable_ItemDeleting" DataKeyNames="UserSkillId, UniqueId" runat="server">
            <LayoutTemplate>
                <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
            </LayoutTemplate>
            <ItemTemplate>
                <div class="skillItem">
                    <asp:Label ID="lblSkillName" CssClass="skillLabel" runat="server"><%# Eval("SkillName") %></asp:Label>
                    <asp:LinkButton ID="lnkbtnDelete" ClientIDMode="AutoID" CssClass="skillDeleteIcon" CommandName="Delete" runat="server">x</asp:LinkButton>
                </div>
            </ItemTemplate>
        </asp:ListView>

        
        <asp:ListView ID="lvSkillsReadOnly" OnItemDataBound="lvSkillsReadOnly_ItemDataBound" runat="server">
            <LayoutTemplate>
                <div style="line-height:20px;">
                    <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                </div>
            </LayoutTemplate>
            <EmptyDataTemplate>
                <div style="color:Gray;">
                    No skills listed.
                </div>
            </EmptyDataTemplate>
            <ItemTemplate>
                <asp:Label ID="lblSkillName" runat="server"><%# Eval("SkillName") %></asp:Label><br />
            </ItemTemplate>
        </asp:ListView>

    </ContentTemplate>
</asp:UpdatePanel>
</div>

