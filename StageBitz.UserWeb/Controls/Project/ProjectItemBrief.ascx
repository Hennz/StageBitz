<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectItemBrief.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Project.ProjectItemBrief" %>
<script type="text/javascript">
    $(document).bind('click', function (e) {
        var $clicked = $(e.target);

        if (!$clicked.parents().hasClass("itemTypePopup") && !$clicked.hasClass("PopupLink")) {
            $(".itemTypePopup").slideUp(0);
        }
    });

    $(document).ready(function () {
        $(".itemTypePopup").slideUp(0);

        $(".AddNewLink").bind("click", function (e) {
            $(".itemTypePopup").slideDown("slow");
            return false;
        });

        $(".AddNewLink").bind("mouseenter mouseleave", function (e) {
            $(this).toggleClass("over");
        });


        displayAndHideRemoveButtonOnHover();

    });

    var prm = Sys.WebForms.PageRequestManager.getInstance();  //To load js functions after the update panel update

    prm.add_endRequest(function () {
        displayAndHideRemoveButtonOnHover();
    });

    function displayAndHideRemoveButtonOnHover() {
        $(".divItemTypeBox").bind("mouseenter", function (e) {
            //alert("here");
            //$(this).toggleClass("over");
            //   alert($(this).children(".btnRemoveItemTypes"));
            $(this).children(".btnRemoveItemTypes").show();
            $(this).children(".btnRemoveItemTypesDisabled").show();


        });

        $(".divItemTypeBox").bind("mouseleave", function (e) {
            //$(this).toggleClass("over");
            //   alert($(this).children(".btnRemoveItemTypes"));
            $(this).children(".btnRemoveItemTypes").hide();
            $(this).children(".btnRemoveItemTypesDisabled").hide();


        });


    }

    function slideUpItemTypes() {
        $(".itemTypePopup").slideUp(0);
    }



</script>
<sb:GroupBox ID="GroupBox1" runat="server">
    <TitleLeftContent>
        <div style="float: left; width: 805px;">
            Project Items
        </div>
        <div id="divItemType" style="float: right;" runat="server">
            <a href='#' class="AddNewLink PopupLink" title="Add an ItemType to the Project">Item
                Types
                <img id="Img1" class="PopupLink" src="~/Common/Images/downArrowSmall.png" alt="Add Item Types"
                    runat="server" />
            </a>
            <asp:UpdatePanel ID="UpdatePanelItemTypesDropDown" runat="server">
                <ContentTemplate>
                    <div id="divAddItemType" class="itemTypePopup" style="z-index: 101">
                        <asp:ListView ID="lvItemTypes" OnItemCommand="lvItemTypes_ItemCommand" runat="server">
                            <EmptyDataTemplate>
                                <div style="color: Gray; border: 1px solid gray; width: 142px; padding: 3px;">
                                    No Item Types to add
                                </div>
                            </EmptyDataTemplate>
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkbtnItemType" ClientIDMode="AutoID" CssClass="DropDownMenuItem textStyle"
                                    CommandName="AddItemType" CommandArgument='<%# Eval("ItemTypeId") %>' runat="server"><%# Eval("Name")%></asp:LinkButton>
                            </ItemTemplate>
                        </asp:ListView>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </TitleLeftContent>
    <BodyContent>
        <asp:UpdatePanel ID="upnlItemTypes" UpdateMode="Conditional" runat="server">
            <ContentTemplate>
                <div id="divNoItemTypes" runat="server" visible="false" style="text-align: center; padding-top: 12px; padding-right: 12px; padding-bottom: 12px; padding-left: 12px;">
                    <strong>What do you need to organise for this Project?</strong>
                    <br />
                    <br />
                    Just use the 'Item Types' drop down to add Item Types
                    <br />
                    (like Props, Costumes etc) right here on your Project Dashboard.
                </div>
                <div id="divListViewItemTypes" style="max-height: 600px; overflow-y: auto; overflow-x: hidden;"
                    runat="server">
                    <asp:ListView ID="lvProjectItemTypes" runat="server" OnItemDataBound="lvProjectItemTypes_ItemDataBound">
                        <LayoutTemplate>
                            <div>
                                <ul class="boxedItemTypeDisplay">
                                    <li runat="server" id="itemPlaceholder"></li>
                                </ul>
                            </div>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <li>
                                <div runat="server" id="divitemtypebox" class="divItemTypeBox">
                                    <asp:LinkButton ID="btnRemoveItemType" CssClass="btnRemoveItemTypes" runat="server" Text="X" OnClick="btnRemoveItemType_Clicked" ClientIDMode="AutoID" />
                                    <a id="linkItemBrief" runat="server">

                                        <asp:PlaceHolder ID="plcProjectItembrief" runat="server">
                                            <br />
                                            <div class="ProjectItemTile">
                                                <asp:Literal ID="litItemTypeName" runat="server"></asp:Literal>
                                            </div>
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
                                            <br />
                                        </asp:PlaceHolder>

                                    </a>
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:ListView>
                </div>
                <sb:PopupBox ID="popupItemBriefConcurrentScenario" Title="Item Type has been removed" Height="100"
                    runat="server">
                    <BodyContent>
                        <div runat="server" id="divCompleteItemBriefRemove" style="white-space: nowrap;">
                            This Item has already been removed.
                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="btnConcurrentConfirmation" CssClass="ignoreDirtyFlag buttonStyle"
                            OnClick="btnConcurrentConfirmation_Click" runat="server" Text="Ok" />
                    </BottomStripeContent>
                </sb:PopupBox>
                <sb:PopupBox ID="popupItemAlreadyHasBrief" Title="Item Briefs have been added to this Item Type" Height="100"
                    runat="server">
                    <BodyContent>
                        <div runat="server" id="divItemAlreadyHasBrief" style="white-space: nowrap;">
                            You've already created Item Briefs for this Item Type.<br /> Please move these to another Item Type or delete them and then try again.

                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="btnItemAlreadyHasBrief" CssClass="ignoreDirtyFlag buttonStyle"
                            OnClick="btnItemAlreadyHasBrief_Click" runat="server" Text="Ok" />
                    </BottomStripeContent>
                </sb:PopupBox>
                <sb:PopupBox ID="popupItemDeleteConfirmation" Title="Remove Item Type" Height="100"
                    runat="server">
                    <BodyContent>
                        <asp:HiddenField ID="hdnItemTypeId" runat="server" />
                        <div runat="server" id="divItemDeleteConfirmation" style="white-space: nowrap;">
                            Please confirm you would like to remove this Item Type permanently from the Project. 

                        </div>
                    </BodyContent>
                    <BottomStripeContent>
                        <asp:Button ID="btnItemDeleteConfirmation" CssClass="ignoreDirtyFlag buttonStyle"
                            OnClick="btnItemDeleteConfirmation_Click" runat="server" Text="Confirm" />
                        <asp:Button ID="btnItemDeleteCancel" CssClass="ignoreDirtyFlag buttonStyle"
                            OnClick="btnItemDeleteCancel_Click" runat="server" Text="Cancel" />
                        
                    </BottomStripeContent>
                </sb:PopupBox>
            </ContentTemplate>
        </asp:UpdatePanel>

    </BodyContent>
</sb:GroupBox>
