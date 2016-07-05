<%@ Page DisplayTitle="Sharing" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="InventorySharing.aspx.cs" Inherits="StageBitz.UserWeb.Inventory.InventorySharing" %>

<%@ Register Src="~/Controls/Company/CompanyWarningDisplay.ascx" TagPrefix="sb" TagName="CompanyWarningDisplay" %>
<%@ Register Src="~/Controls/Inventory/ManageBookings.ascx" TagPrefix="sb" TagName="ManageBookings" %>
<%@ Register Src="~/Controls/Inventory/InventoryLocations.ascx" TagPrefix="sb" TagName="InventoryLocations" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Common/SearchUsers.ascx" TagPrefix="sb" TagName="SearchUsers" %>
<%@ Register Src="~/Controls/Inventory/InventoryLocationRoles.ascx" TagPrefix="sb" TagName="InventoryLocationRoles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .Tier2 {
            color: #cd7067 !important;
        }
    </style>
    <script src="<%# ResolveUrl("../Common/Scripts/ComboSearchEvents.js?v="+ this.ApplicationVersionString)  %>"
        type="text/javascript"></script>
    <script src="<%# ResolveUrl("../Common/Scripts/Inventory.js?v="+ this.ApplicationVersionString)  %>"
        type="text/javascript"></script>
    <script type="text/javascript">

        var cboSearchCompanyNameEvent = new StageBitz.UserWeb.Common.Scripts.ComboSearchEvents();
        cboSearchCompanyNameEvent.FindButton = "#<%= btnFind.ClientID %>";

        function cboSearchCompanyName_onItemsRequested(sender, eventArgs) {
            cboSearchCompanyNameEvent.OnItemsRequested(sender, eventArgs);
        };

        function cboSearchCompanyName_onKeyPressing(sender, eventArgs) {
            cboSearchCompanyNameEvent.OnKeyPressing(sender, eventArgs);
        };

        function cboSearch_OnClientFocus(sender, eventArgs) {
            cboSearchCompanyNameEvent.OnClientFocus(sender, eventArgs);
        };

        function cboSearchCompanyName_onSelectedIndexChanged(sender, eventArgs) {
            cboSearchCompanyNameEvent.OnSelectedIndexChanged(sender, eventArgs);
        };

        function InitializeRadioButtons() {
            $("input[type='radio']", $("div[id$='divCompanyList']")).attr("name", "companyList");
        }

        function DisableOptions(isCompanyReadOnly) {

            $("#<%=rdViewMyCompany.ClientID %>").attr("disabled", "disabled");
            $("#<%=rdViewOtherCompany.ClientID %>").attr("disabled", "disabled");
            $("#<%=rdBoth.ClientID %>").attr("disabled", "disabled");
            $("#<%=btnShare.ClientID %>").attr("disabled", "disabled");

            if (isCompanyReadOnly == 'False') {
                $("#<%=btnShare.ClientID %>").attr("title", "Please select a Company.");
                ClearRadioOptions();
            }
        }

        function ClearRadioOptions() {
            $("#<%=rdViewMyCompany.ClientID %>").removeAttr('checked');
            $("#<%=rdViewOtherCompany.ClientID %>").removeAttr('checked');
            $("#<%=rdBoth.ClientID %>").removeAttr('checked');

        }

        function InitializeCheckBoxes(isCompanyReadOnly) {
            $("[id$='rbtnCompany']", $("div[id$='divSelectedCompanySharingStatus']")).click(function () {

                var self = $(this);

                //Read the hidden Sharing statuses of companies
                var hdnAccessToMyCompanyStatusVal = self.siblings('input[id$="hdnAccessToMyCompanyStatusCodeId"]').val();
                var hdnAccessToSelectedCompanyStatusVal = self.siblings('input[id$="hdnAccessToSelectedCompanyStatusCodeId"]').val();
                var hdnCompanyId = self.siblings('input[id$="hdnCompanyId"]').val();
                $('input[id$=hdnSelectedCompanyId]').val(hdnCompanyId);

                //Reset the UI
                ClearRadioOptions();
                var hasDefaultOptionSet = false;
                if (isCompanyReadOnly == 'False') {
                    $("#<%=btnShare.ClientID %>").attr("title", "");
                    if (hdnAccessToMyCompanyStatusVal == 0) {
                        $("#<%=rdViewMyCompany.ClientID %>").prop('checked', 'checked');
                        hasDefaultOptionSet = true;
                        $("#<%=rdViewMyCompany.ClientID %>").removeAttr("disabled");
                    }
                    else {
                        $("#<%=rdViewMyCompany.ClientID %>").attr("disabled", "disabled");
                    }

                    if (hdnAccessToSelectedCompanyStatusVal == 0) {
                        if (!hasDefaultOptionSet) {

                            $("#<%=rdViewOtherCompany.ClientID %>").prop('checked', 'checked');

                        }

                        $("#<%=rdViewOtherCompany.ClientID %>").removeAttr("disabled");
                    }
                    else {

                        $("#<%=rdViewOtherCompany.ClientID %>").attr("disabled", "disabled");
                    }

                    if (hdnAccessToMyCompanyStatusVal == 0 && hdnAccessToSelectedCompanyStatusVal == 0) {
                        $("#<%=rdBoth.ClientID %>").removeAttr("disabled");
                    }
                    else {

                        $("#<%=rdBoth.ClientID %>").attr("disabled", "disabled");
                    }

                    //If a Single Radio button is enabled, then enable the button.
                    if ($("#<%=rdViewMyCompany.ClientID %>").is(':checked') == true || $("#<%=rdViewOtherCompany.ClientID %>").is(':checked') == true || $("#<%=rdBoth.ClientID %>").is(':checked') == true) {
                        $("#<%=btnShare.ClientID %>").removeAttr("disabled");
                    }
                }
            });

            $("[id$='rdViewMyCompany'],[id$='rdViewOtherCompany'],[id$='rdBoth']", "", $("div[id$='divOptonList']")).click(function () {
                var self = $(this);

                //If a Single Radio button is enabled, then enable the button.
                if ($("#<%=rdViewMyCompany.ClientID %>").is(':checked') == true || $("#<%=rdViewOtherCompany.ClientID %>").is(':checked') == true || $("#<%=rdBoth.ClientID %>").is(':checked') == true) {
                    $("#<%=btnShare.ClientID %>").removeAttr("disabled");
                }
                $("#<%=btnShare.ClientID %>").attr("title", "");

            });

        }

        function ConfigureLocationActions(shouldEnable, isRootNodeTriggered, isTier2Node) {
            if (shouldEnable) {
                var isCompanyReadOnly = "<%= IsCompanyReadOnly %>";
                var isInvenotyAdmin = "<%= this.HasInventoryAdminRights %>" == "True";

                if (isInvenotyAdmin || !isRootNodeTriggered) {
                    $("#<%=btnAddSubLoation.ClientID %>").removeAttr("disabled");
                }
                else {
                    $("#<%=btnAddSubLoation.ClientID %>").attr("disabled", "disabled");
                }

                if (!isRootNodeTriggered && isCompanyReadOnly == "False") {
                    $("#<%=btnEdit.ClientID %>").removeAttr("disabled");
                    if (isInvenotyAdmin || !isTier2Node) {
                        $("#<%=btnDelete.ClientID %>").removeAttr("disabled");
                        $("#<%=btnMove.ClientID %>").removeAttr("disabled");
                    }
                    else {
                        $("#<%=btnDelete.ClientID %>").attr("disabled", "disabled");
                        $("#<%=btnMove.ClientID %>").attr("disabled", "disabled");
                    }
                }

                if (isRootNodeTriggered) {
                    $("#<%=btnEdit.ClientID %>").attr("disabled", "disabled");
                    $("#<%=btnDelete.ClientID %>").attr("disabled", "disabled");
                    $("#<%=btnMove.ClientID %>").attr("disabled", "disabled");
                }
            }
            else {
                $("#<%=btnEdit.ClientID %>").attr("disabled", "disabled");
                $("#<%=btnDelete.ClientID %>").attr("disabled", "disabled");
                $("#<%=btnMove.ClientID %>").attr("disabled", "disabled");
                $("#<%=btnAddSubLoation.ClientID %>").attr("disabled", "disabled");
            }
        }

        function InitializeUI(sender, args) {
            var treeView = $find("<%= tvLocation.ClientID %>");
            var selectedNode = treeView.get_selectedNodes()[0];

            ConfigureUIforTreeTravel(selectedNode);

            $("input[id$='btnAddSubLoation']").click(function () {
                $('span[id$=lblErrorAddLocation]').removeClass("inputError Show");//to work with ie8
                $('span[id$=lblErrorAddLocation]').text('');
                $('input[id$=txtLocationName]').val('');

                showPopup('<%=popupAddLocation.ClientID%>');
                $('span[id$=spanAddLocation]').text(resultArray[0]);
                IntializeErrorMessages();
                document.getElementById("<%= spanAddLocation.ClientID %>").title = resultArray[1];

                $('input[id$=hdnLocationBreadCrumb]').val(resultArray[0]);
                return false;
            });

            $("input[id$='btnEdit']").click(function () {
                $('span[id$=lblErrorEditLocation]').removeClass("inputError");//to work with ie8
                $('span[id$=lblErrorEditLocation]').text('');
                showPopup('<%=popupEditLocation.ClientID%>');
                IntializeErrorMessages();
                $('input[id$=hdnLocationBreadCrumb]').val(resultArray[0]);
                $('input[id$=txtLocationNameEdit]').val(resultArray[2]);
                return false;
            });
        }

        var resultArray;
        function ClientNodeClicked(sender, eventArgs) {

            var selectedNode = eventArgs.get_node();
            ConfigureUIforTreeTravel(selectedNode);
        }

        function OnClientKeyPressing(sender, args) {
            var selectedNode = args.get_node();

            var key = args.get_domEvent().keyCode;
            if (key == "40")  // down-arrow 
            {
                selectedNode = selectedNode.get_nextVisibleNode();
                if (selectedNode != null)
                    ConfigureUIforTreeTravel(selectedNode);
            }
            else if (key == "38") { // up-arrow 
                var selectedPreviousNode = selectedNode._getPrevSelectableNode();
                if (selectedPreviousNode != null)
                    ConfigureUIforTreeTravel(selectedPreviousNode);
            }
        }

        function ConfigureUIforTreeTravel(selectedNode) {
            if (selectedNode != null) {
                var selectedNodeId = selectedNode.get_value();
                var isRootnode = selectedNode.get_level() == 0;
                var isTier2Node = selectedNode.get_level() == 1;
                ConfigureLocationActions(true, isRootnode, isTier2Node);
                $('input[id$=htnSelectedLocationId]').val(selectedNodeId);
                if (!isRootnode) {
                    var parentNodeId = selectedNode.get_parent().get_value();
                    $('input[id$=htnParentLocationId]').val(parentNodeId);
                }
                resultArray = BuildLocationBreadCrumb(selectedNode, 60);
            }
            else {
                ConfigureLocationActions(false, false);
            }
        }

        function EnableApplyPermissionButton() {
            $("#<%= btnUpdatePermission.ClientID%>").removeAttr('disabled');
        }

        $(document).on('<%= sbInventoryLocationRoles.ClientID%>_onLocationRoleSelect', function () {
            $("#<%= rbtnInventoryAdmin.ClientID%>").prop('checked', false);
            EnableApplyPermissionButton();
        });

        function SelectInventoryAdmin() {
            <%= sbInventoryLocationRoles.ClientID%>_UncheckAll();
            EnableApplyPermissionButton();
        }

        function ValidateInventoryRole(sender, args) {
            args.IsValid = <%= sbInventoryLocationRoles.ClientID%>_ValidateUISelection();
        }

    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    <asp:PlaceHolder ID="plcHeaderLinks" runat="server">| <a id="lnkCompanyInventory"
        runat="server">Company Inventory</a> |
        <asp:HyperLink ID="hyperLinkMyBooking" runat="server">My Bookings</asp:HyperLink>
        |
        <asp:HyperLink ID="hyperLinkInventorySharing" runat="server">Manage Inventory</asp:HyperLink>
    </asp:PlaceHolder>
    <asp:HiddenField runat="server" ID="hdnPendingSharingStatusCode" />
    <asp:HiddenField runat="server" ID="hdnActiveSharingStatusCode" />
    <asp:HiddenField runat="server" ID="hdnSelectedCompanyId" />
    <asp:HiddenField runat="server" ID="hdnIsCompanyReadOnly" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <sb:CompanyWarningDisplay runat="server" ID="sbCompanyWarningDisplay" />
    <sb:ProjectWarningPopup ID="projectWarningPopupInventory" runat="server"></sb:ProjectWarningPopup>

    <asp:UpdatePanel runat="server" ID="upnlTabs" UpdateMode="Conditional">
        <ContentTemplate>
            <telerik:RadTabStrip ID="inventorySharingTabs" Width="100%" MultiPageID="sharingPages" runat="server">
                <Tabs>
                    <telerik:RadTab runat="server" Text="Bookings" Value="Bookings">
                    </telerik:RadTab>
                    <telerik:RadTab runat="server" Text="Search" Value="Search">
                    </telerik:RadTab>
                    <telerik:RadTab runat="server" Text="Administration" Value="Administration">
                    </telerik:RadTab>
                    <telerik:RadTab runat="server" Text="Locations" Value="Locations">
                    </telerik:RadTab>
                    <telerik:RadTab runat="server" Text="Team" Value="Team">
                    </telerik:RadTab>
                </Tabs>
            </telerik:RadTabStrip>
            <div class="tabPage" style="width: 95%; min-height: 200px;">
                <telerik:RadMultiPage ID="sharingPages" runat="server" Width="100%">
                    <telerik:RadPageView ID="BookingsTab" runat="server">
                        <sb:ManageBookings runat="server" ID="sbManageBookings" DisplayMode="InventoryManager" />
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="SearchTab" runat="server">
                        <asp:UpdatePanel ID="upnlInventorySharing" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <sb:PopupBox ID="popupSharingDetails" runat="server" Title=" " Height="100">
                                    <BodyContent>
                                        <asp:Literal ID="litMsg" runat="server"></asp:Literal>
                                    </BodyContent>
                                    <BottomStripeContent>
                                        <asp:Button ID="btnOK" runat="server" CssClass="popupBoxCloser buttonStyle" Text="OK" />
                                    </BottomStripeContent>
                                </sb:PopupBox>

                                <h2>Let's start Sharing! </h2>
                                If you'd like all the props, costumes and scenery sitting in your storeroom to be out there getting used, why not let Companies 
                        in your area see what you have? Or ask them if you can peek into theirs? Search and select to start sharing now. 
                        <br />
                                <br />
                                <div style="margin-left: 20px;">
                                    <table style="width: 720px;">
                                        <tr style="height: 30px;">
                                            <td style="width: 160px;">
                                                <div style="float: left;">
                                                    Search For Companies:
                                                </div>
                                                <div style="float: left; margin-left: 4px;">
                                                    <sb:HelpTip ID="helpTipSearchCompanies" Visible="true" runat="server" Width="470">
                                                        <ul>
                                                            <li>To find a Company simply enter their name in search field and click 'Find'.</li>
                                                            <li>You can also search for Companies in a certain area by entering a city or country.</li>
                                                        </ul>
                                                    </sb:HelpTip>
                                                </div>
                                            </td>
                                            <td style="width: 220px;">
                                                <div class="searchbox rounded">
                                                    <div style="position: relative; top: 2px;">
                                                        <telerik:RadComboBox runat="server" ID="cboSearchCompanyName" AutoPostBack="false"
                                                            OnClientItemsRequested="cboSearchCompanyName_onItemsRequested" OnItemsRequested="cboSearchCompanyName_ItemsRequested"
                                                            OnClientKeyPressing="cboSearchCompanyName_onKeyPressing" OnClientSelectedIndexChanged="cboSearchCompanyName_onSelectedIndexChanged"
                                                            EnableLoadOnDemand="true" EmptyMessage="Enter a Company's name..." ChangeTextOnKeyBoardNavigation="true"
                                                            ShowWhileLoading="false" ShowToggleImage="false" MaxLength="100"
                                                            OnClientFocus="cboSearch_OnClientFocus">
                                                            <ExpandAnimation Type="None" />
                                                            <CollapseAnimation Type="None" />
                                                        </telerik:RadComboBox>
                                                    </div>
                                                </div>
                                            </td>
                                            <td style="width: 50px; text-align: center;">or</td>
                                            <td style="width: 220px;">
                                                <div class="searchbox rounded">
                                                    <asp:TextBox ID="txtSearchCompanyLocation" runat="server" />
                                                    <asp:TextBoxWatermarkExtender ID="textBoxWatermarkExtender" TargetControlID="txtSearchCompanyLocation"
                                                        WatermarkText="Enter a City or Country..." runat="server">
                                                    </asp:TextBoxWatermarkExtender>
                                                </div>
                                            </td>
                                            <td>
                                                <asp:Button ID="btnFind" OnClick="btnFind_Click" CssClass="buttonStyle" runat="server" Text="Find" /></td>
                                        </tr>
                                    </table>
                                </div>
                                <br />
                                <sb:GroupBox runat="server" ID="grpCompanies">
                                    <TitleLeftContent>
                                        <span class="boldText left">
                                            <asp:Literal runat="server" ID="ltrNoOfCompaniesFound" />
                                            found </span>
                                        <div style="float: left; margin-left: 4px; position: relative; top: -2px;">
                                            <sb:HelpTip ID="helpTip1" Visible="true" runat="server" Width="470">
                                                <ul>
                                                    <li>Once you have found the company you would like to share your Inventory with select the radio button next to its name. 
                                                        Then select how you would like to share with it at the bottom of the search panel. 
                                                    </li>
                                                    <li>If it has set up open sharing you do not need to request access to its Inventory as you can already add it to 
                                                        your available Inventories list on your Inventory search page. 
                                                    </li>
                                                </ul>
                                            </sb:HelpTip>
                                        </div>
                                    </TitleLeftContent>
                                    <BodyContent>
                                        <div id="divCompanyList" runat="server">
                                            <table style="width: 850px;">
                                                <tr>
                                                    <td>
                                                        <div style="overflow-y: auto; min-height: 150px; max-height: 480px;">
                                                            <telerik:RadListView ID="lvCompanyList" AllowPaging="True" runat="server" OnNeedDataSource="lvCompanyList_NeedDataSource"
                                                                ItemPlaceholderID="ProductsHolder" OnItemDataBound="lvCompanyList_ItemDataBound">
                                                                <EmptyDataTemplate>
                                                                    <div class="noData">
                                                                        You could try a different spelling or expanding your search area to find Companies. 
                                                                    </div>
                                                                </EmptyDataTemplate>
                                                                <LayoutTemplate>
                                                                    <asp:Panel ID="ProductsHolder" runat="server" />
                                                                </LayoutTemplate>
                                                                <ItemTemplate>
                                                                    <div class="thumbListItem" style="height: 170px; width: 150px;">
                                                                        <div style="display: table;">
                                                                            <div style="float: left; padding-left: 10px;">
                                                                                <sb:ImageDisplay ID="itemThumbDisplay" ShowImagePreview="false" runat="server" />
                                                                            </div>
                                                                            <div runat="server" id="divToolTip">
                                                                                <div runat="server" id="divAccessToMyCompany" style="z-index: 900; position: relative; left: -20px;">
                                                                                    <asp:Image ID="imgAccessToMyCompany" runat="server" />
                                                                                </div>
                                                                                <div runat="server" id="divAccessToSelectedCompany" style="z-index: 900; position: relative; left: -20px;">
                                                                                    <asp:Image ID="imgAccessToSelectedCompany" runat="server" />
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                        <div id="divSelectedCompanySharingStatus" style="display: table; text-align: left;">
                                                                            <div style="display: inline;">
                                                                                <asp:RadioButton ID="rbtnCompany" runat="server" />
                                                                                <asp:Label ID="lblCompanyName" runat="server" /><br />
                                                                                <asp:HiddenField runat="server" ID="hdnCompanyId" Value='<%# Bind("company.CompanyId") %>' />
                                                                                <asp:HiddenField runat="server" ID="hdnAccessToMyCompanyStatusCodeId" Value='<%# Bind("AccessToMyCompanyStatusCodeId") %>' />
                                                                                <asp:HiddenField runat="server" ID="hdnAccessToSelectedCompanyStatusCodeId" Value='<%# Bind("AccessToSelectedCompanyStatusCodeId") %>' />
                                                                            </div>
                                                                            <div style="margin-left: 24px;" class="smallText">
                                                                                <asp:Literal runat="server" ID="ltrCompanyCity" /><br />
                                                                                <asp:Literal runat="server" ID="ltrCompanyCountry" /><br />
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                </ItemTemplate>
                                                            </telerik:RadListView>
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <telerik:RadDataPager ID="pagerCompanyList" runat="server" PagedControlID="lvCompanyList"
                                                            PageSize="20" Width="870" OnPreRender="pagerCompanyList_PreRender">
                                                            <Fields>
                                                                <telerik:RadDataPagerButtonField FieldType="FirstPrev" />
                                                                <telerik:RadDataPagerButtonField FieldType="Numeric" PageButtonCount="5" />
                                                                <telerik:RadDataPagerButtonField FieldType="NextLast" />
                                                                <telerik:RadDataPagerPageSizeField PageSizeComboWidth="60" PageSizeText="Page size: "
                                                                    PageSizes="20,50,100" />
                                                                <telerik:RadDataPagerTemplatePageField>
                                                                    <PagerTemplate>
                                                                        <div style="text-align: right; color: #5a6779; padding-right: 10px;" id="listViewPagerCount">
                                                                            <asp:Label runat="server" ID="CurrentPageLabel" Text="<%# Container.Owner.TotalRowCount%>" />
                                                                            <asp:Label runat="server" ID="lblCompanyText" Text="Companies in" />
                                                                            <asp:Label runat="server" ID="TotalItemsLabel" Text="<%# Container.Owner.PageCount %>" />
                                                                            <asp:Label runat="server" ID="lblPagesText" Text="pages" />
                                                                        </div>
                                                                    </PagerTemplate>
                                                                </telerik:RadDataPagerTemplatePageField>
                                                            </Fields>
                                                        </telerik:RadDataPager>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </BodyContent>
                                </sb:GroupBox>
                                <div runat="server" style="margin-left: 260px;" id="divSharingSection">
                                    <h2>I want to start sharing with the selected Company. </h2>
                                    <div id="divOptonList" style="margin-left: 0px;" class="left">
                                        <asp:RadioButton ID="rdViewMyCompany" GroupName="SharingGroup" runat="server" />
                                        </br>
                            <asp:RadioButton ID="rdViewOtherCompany" GroupName="SharingGroup" runat="server" />
                                        </br>
                            <asp:RadioButton ID="rdBoth" GroupName="SharingGroup" runat="server" Text="Both!" />
                                    </div>
                                    <div style="margin-bottom: 10px; clear: both;"></div>

                                    <p><i>The Inventory Administrator of the selected Company will be notified via email.</i></p>

                                    <asp:Button ID="btnShare" OnClick="btnShare_Click" CssClass="buttonStyle" Text="Share" runat="server" />
                                </div>
                                <div style="clear: both;"></div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="AdminTab" runat="server">
                        <asp:UpdatePanel runat="server" ID="upnlAdminTab" UpdateMode="Conditional">
                            <ContentTemplate>
                                <span class="boldText">Manage who you share with!
                                </span>
                                <div style="width: 98%; background-color: #F2F0F4; padding: 10px; margin: 10px 0px;">
                                    <span class="boldText" style="float: left; padding: 5px; margin-left: 300px;">Can people search for my Inventory?
                                    </span>
                                    <div style="float: left; padding-left: 5px; padding-top: 5px;">
                                        <sb:HelpTip runat="server" ID="helptip2">
                                            <ul>
                                                <li>By allowing your Inventory to be seen in search results other Companies can find you to request access to your Inventory and share theirs with you.
                                                </li>
                                                <li>Hiding your Inventory means no one can find you to share their Inventory with you or request access to yours.
                                                </li>
                                            </ul>
                                        </sb:HelpTip>
                                    </div>
                                    <div style="clear: both; margin-left: 285px;" class="dirtyValidationArea">
                                        <asp:RadioButton runat="server" ID="rbtnYes" GroupName="ShowInSearchResults" Text="Yes my Company can be seen in search results." />
                                        <br />
                                        <asp:RadioButton runat="server" ID="rbtnNo" GroupName="ShowInSearchResults" Text="No my Company is hidden from other Companies." />
                                    </div>
                                    <div style="clear: both; margin-left: 415px; padding-top: 10px; margin-right: 400px;">
                                        <asp:Button runat="server" ID="btnSaveShowInSearchResults" Text="Save" OnClick="btnSaveShowInSearchResults_Click" CssClass="buttonStyle" />
                                        <br style="clear: both" />
                                    </div>
                                    <div style="clear: both; margin-left: 395px; padding-top: 5px; width: 100px;">
                                        <div id="showInSearchResultsSavedNotice" class="inlineNotification">
                                            Changes saved.
                                        </div>
                                    </div>
                                </div>
                                <br />
                                <span class="boldText" style="float: left; padding-bottom: 10px;">This is who you're sharing with:
                                </span>
                                <div style="float: left; padding-left: 5px;">
                                    <sb:HelpTip runat="server" ID="helptipSharingGrid">
                                        <ul>
                                            <li>Companies will appear here when you are sharing Inventories.
                                            </li>
                                            <li>This is where you can approve any requests for Companies to access your Inventory.
                                            </li>
                                            <li>You can remove a sharing connection at any time.
                                            </li>
                                        </ul>
                                    </sb:HelpTip>
                                </div>
                                <br style="clear: both;" />
                                <div>
                                    <telerik:RadGrid ID="gvManageSharings" Width="100%" EnableLinqExpressions="False" AutoGenerateColumns="false"
                                        AllowSorting="false" OnItemDataBound="gvManageSharings_ItemDataBound" OnItemCommand="gvManageSharings_ItemCommand"
                                        runat="server" Height="600">
                                        <MasterTableView AllowNaturalSort="false" TableLayout="Fixed">
                                            <NoRecordsTemplate>
                                                <div class="noData">
                                                    <b>You are not currently sharing Inventories with any Companies.</b>
                                                    <br />
                                                    To start sharing you can search for Companies in your area on search tab.
                                                </div>
                                            </NoRecordsTemplate>
                                            <SortExpressions>
                                                <telerik:GridSortExpression FieldName="Company.CompanyName" SortOrder="Ascending" />
                                            </SortExpressions>
                                            <Columns>
                                                <telerik:GridTemplateColumn UniqueName="CompanyName" HeaderStyle-Width="200px" HeaderText="Company Name" SortExpression="Company.CompanyName">
                                                    <ItemTemplate>
                                                        <%--<%# Bind("Company.CompanyId") %>--%>
                                                        <a runat="server" id="anchorForCompany"></a>
                                                        <asp:Label runat="server" ID="lblCompanyName"></asp:Label>
                                                        <span id="spanProjectSuspended" title="This Company is suspended." visible="false" runat="server" class="spanCompanySuspended"></span>
                                                        <asp:HiddenField ID="hdnCompanyId" runat="server" />
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn UniqueName="CanTheySeeOurs" HeaderStyle-Width="200px" HeaderText="Can they see ours?">
                                                    <ItemTemplate>
                                                        <%--ActionLink css classs is used in the code behind--%>
                                                        <asp:MultiView runat="server" ID="multiViewCanTheySeeOurs">
                                                            <asp:View runat="server" ID="viewYes1">
                                                                <span style="width: 100px;">Yes
                                                                </span>
                                                                <span style="text-align: center; width: 100px; padding-left: 150px;">
                                                                    <asp:LinkButton runat="server" ID="lbtnRemoveCanTheySeeOurs" Text="Remove" CssClass="ActionLink" CommandName="Remove_CanTheySeeOurs"></asp:LinkButton>
                                                                    <asp:Label runat="server" ID="lblRemoveCanTheySeeOurs" Text="Remove" CssClass="ActionLink" Visible="false"></asp:Label>
                                                                </span>
                                                            </asp:View>
                                                            <asp:View runat="server" ID="viewNo1">
                                                                <span style="width: 100px;">No
                                                                </span>
                                                            </asp:View>
                                                            <asp:View runat="server" ID="viewPending1">
                                                                <span style="width: 100px;">Pending
                                                                </span>
                                                                <span style="text-align: center; width: 100px; padding-left: 100px;">
                                                                    <asp:LinkButton runat="server" ID="lbtnApproveCanTheySeeOurs" Text="Approve" CssClass="ActionLink" CommandName="Approve_CanTheySeeOurs"></asp:LinkButton>
                                                                    <asp:Label runat="server" ID="lblApproveCanTheySeeOurs" Text="Approve" CssClass="ActionLink" Visible="false"></asp:Label>
                                                                    &nbsp;&nbsp;&nbsp;
                                                                    <asp:LinkButton runat="server" ID="lbtnDenyCanTheySeeOurs" Text="Deny" CssClass="ActionLink" CommandName="Deny_CanTheySeeOurs"></asp:LinkButton>
                                                                    <asp:Label runat="server" ID="lblDenyCanTheySeeOurs" Text="Deny" CssClass="ActionLink" Visible="false"></asp:Label>
                                                                </span>
                                                            </asp:View>
                                                        </asp:MultiView>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                                <telerik:GridTemplateColumn UniqueName="CanWeSeeTheirs" HeaderStyle-Width="200px" HeaderText="Can we see theirs?">
                                                    <ItemTemplate>
                                                        <%--ActionLink css classs is used in the code behind--%>
                                                        <asp:MultiView runat="server" ID="multiViewCanWeSeeTheirs">
                                                            <asp:View runat="server" ID="viewYes2">
                                                                <span style="width: 100px;">Yes
                                                                </span>
                                                                <span style="text-align: center; width: 100px; padding-left: 150px;">
                                                                    <asp:LinkButton runat="server" ID="lbtnRemoveCanWeSeeTheirs" Text="Remove" CssClass="ActionLink" CommandName="Remove_CanWeSeeTheirs"></asp:LinkButton>
                                                                    <asp:Label runat="server" ID="lblRemoveCanWeSeeTheirs" Text="Remove" CssClass="ActionLink" Visible="false"></asp:Label>
                                                                </span>
                                                            </asp:View>
                                                            <asp:View runat="server" ID="viewNo2">
                                                                <span style="width: 100px;">No</span>
                                                            </asp:View>
                                                            <asp:View runat="server" ID="viewPending2">
                                                                <span style="width: 100px;">Pending
                                                                </span>
                                                            </asp:View>
                                                        </asp:MultiView>
                                                    </ItemTemplate>
                                                </telerik:GridTemplateColumn>
                                            </Columns>
                                        </MasterTableView>
                                        <ClientSettings>
                                            <Scrolling AllowScroll="True" UseStaticHeaders="True" SaveScrollPosition="True"></Scrolling>
                                        </ClientSettings>
                                    </telerik:RadGrid>
                                    <sb:PopupBox ID="popupConfirm" runat="server" Title="Confirmation">
                                        <BodyContent>
                                            <div runat="server" id="divPopupConfirm" style="width: 200px; text-align: center;">
                                                Are you sure?
                                                <asp:Literal runat="server" ID="ltrConfirmationMsg" />
                                            </div>
                                        </BodyContent>
                                        <BottomStripeContent>
                                            <asp:Button ID="btnPopupConfirmAccept" runat="server" CssClass="buttonStyle" Text="Yes" OnClick="btnPopupConfirmAccept_Click" />
                                            <input type="button" class="popupBoxCloser buttonStyle" value="No" />
                                        </BottomStripeContent>
                                    </sb:PopupBox>
                                    <sb:PopupBox ID="popupError" runat="server" Title="Error!">
                                        <BodyContent>
                                            <div style="width: 350px; text-align: center;">
                                                <asp:Label runat="server" ID="lblErrorMsg"></asp:Label>
                                            </div>
                                        </BodyContent>
                                        <BottomStripeContent>
                                            <input type="button" class="popupBoxCloser reload buttonStyle" value="Ok" />
                                        </BottomStripeContent>
                                    </sb:PopupBox>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </telerik:RadPageView>
                    <telerik:RadPageView ID="LocationTab" runat="server">
                        <asp:UpdatePanel ID="upnlLocation" runat="server">
                            <ContentTemplate>
                                <asp:HiddenField ID="htnSelectedLocationId" runat="server" />
                                <asp:HiddenField ID="htnParentLocationId" runat="server" />
                                <asp:HiddenField ID="hdnLocationBreadCrumb" runat="server" />
                                <sb:PopupBox ID="popupAddLocation" runat="server" Title="Add Location" ShowCornerCloseButton="false">
                                    <BodyContent>
                                        <asp:Panel runat="server" ID="pnlAddLocations" DefaultButton="btnEnterKeyButtonAddPanel" Width="400">
                                            <!--This is to prevent anything from happening when the enter key is pressed on any textbox-->
                                            <asp:Button ID="btnEnterKeyButtonAddPanel" OnClientClick="return false;" runat="server" Text="Button"
                                                Style="display: none;" CssClass="buttonStyle" />
                                            Your new location will be added under
                                    <br />
                                            <b><span runat="server" id="spanAddLocation"></span></b>
                                            <br />
                                            <asp:Label ID="Label1" runat="server" AccessKey="N" AssociatedControlID="lblErrorAddLocation" Text="<u>N</u>ame: "></asp:Label>
                                            <asp:TextBox ID="txtLocationName" Width="250" runat="server" MaxLength="100"></asp:TextBox>
                                            <asp:RequiredFieldValidator runat="server" ID="rfvLocation" ControlToValidate="txtLocationName" ValidationGroup="LocationValGroup" ErrorMessage="*" ToolTip="Location name is required."></asp:RequiredFieldValidator>
                                            <div style="min-height: 20px;">
                                                <span id="lblErrorAddLocation" class="inputError" runat="server"></span>
                                            </div>
                                        </asp:Panel>
                                    </BodyContent>
                                    <BottomStripeContent>
                                        <asp:Button ID="btnAddLocation" AccessKey="A" OnClick="btnAddLocation_Click" ValidationGroup="LocationValGroup" CssClass="buttonStyle" runat="server" Text="Add" />
                                        <asp:Button ID="Button1" runat="server" CssClass="popupBoxCloser buttonStyle" Text="Cancel" />
                                    </BottomStripeContent>
                                </sb:PopupBox>

                                <sb:PopupBox ID="popupEditLocation" runat="server" Title="Edit Location" ShowCornerCloseButton="false">
                                    <BodyContent>
                                        <asp:Panel runat="server" ID="Panel1" DefaultButton="btnEnterKeyButtonEditPanel" Width="400">
                                            <!--This is to prevent anything from happening when the enter key is pressed on any textbox-->
                                            <asp:Button ID="btnEnterKeyButtonEditPanel" OnClientClick="return false;" runat="server" Text="Button"
                                                Style="display: none;" CssClass="buttonStyle" />
                                            <asp:Label ID="lblName" runat="server" AccessKey="N" AssociatedControlID="txtLocationNameEdit" Text="<u>N</u>ame: "></asp:Label>
                                            <asp:TextBox ID="txtLocationNameEdit" Width="250" runat="server" MaxLength="100"></asp:TextBox>
                                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator1" ControlToValidate="txtLocationNameEdit" ValidationGroup="LocationValGroupEdit" ErrorMessage="*" ToolTip="Location name is required."></asp:RequiredFieldValidator>
                                            <div style="min-height: 20px;">
                                                <span id="lblErrorEditLocation" class="inputError" runat="server"></span>
                                            </div>
                                        </asp:Panel>
                                    </BodyContent>
                                    <BottomStripeContent>
                                        <asp:Button ID="btnUpdate" OnClick="btnLocationNameEdit_Click" ValidationGroup="LocationValGroupEdit" CssClass="buttonStyle" runat="server" Text="Update" />
                                        <asp:Button ID="Button3" runat="server" CssClass="popupBoxCloser buttonStyle" Text="Cancel" />
                                    </BottomStripeContent>
                                </sb:PopupBox>

                                <sb:PopupBox ID="popupMoveLocation" runat="server" Title="Move Location" ShowCornerCloseButton="false" IsAutoFocus="false">
                                    <BodyContent>
                                        <div style="width: 400px;">
                                            <span class="left"><u>S</u>elect New Location:&nbsp;
                                            </span>
                                            <span class="left" style="position: relative; bottom: 5px;">
                                                <sb:InventoryLocations runat="server" ID="sbMoveInventoryLocations" ValidationGroup="MoveLocation" AccessKey="S" NextFocusInput="btnMoveLocation" DisableViewOnlyLocations="true" InventoryLocationDisplayMode="LocationAdmin" />
                                            </span>
                                            <br style="clear: both" />
                                            <asp:Label ID="lblMoveLocationError" CssClass="inputError" runat="server"></asp:Label>
                                        </div>
                                    </BodyContent>
                                    <BottomStripeContent>
                                        <asp:Button ID="btnMoveLocation" OnClick="btnMoveLocation_Click" CssClass="buttonStyle" runat="server" Text="Move" ValidationGroup="MoveLocation" />
                                        <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                                    </BottomStripeContent>
                                </sb:PopupBox>

                                <sb:PopupBox ID="popupDeleteLocation" runat="server" Title="Delete Location!">
                                    <BodyContent>
                                        <div style="width: 350px; text-align: left;">
                                            <asp:Label runat="server" ID="lblDeletLocationeMsg"></asp:Label>

                                        </div>
                                    </BodyContent>
                                    <BottomStripeContent>
                                        <asp:Button ID="btnConfirmDelete" OnClick="btnConfirmDelete_Click" CssClass="buttonStyle" runat="server" Text="Delete" />
                                        <a runat="server" id="lnkMoveItems" class="buttonStyle">Move Items
                                        </a>
                                        <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                                    </BottomStripeContent>
                                </sb:PopupBox>

                                <sb:PopupBox ID="popupMoveLocationConfirmUserRoles" runat="server" Title="Team roles are assigned to Managed Locations" ShowCornerCloseButton="false" IsAutoFocus="false">
                                    <BodyContent>
                                        <div style="width: 400px;">
                                            <div>
                                                <p>
                                                    <b>
                                                        <asp:Literal runat="server" ID="ltrlConfirmPopupHeaderStrip"></asp:Literal></b>
                                                </p>
                                                <p>
                                                    <asp:Literal runat="server" ID="ltrlConfirmPopupContentStrip"></asp:Literal>
                                                </p>
                                            </div>
                                        </div>
                                    </BodyContent>
                                    <BottomStripeContent>
                                        <asp:Button ID="btnConfirmMoveLocation" OnClick="btnConfirmMoveLocation_Click" CssClass="buttonStyle" runat="server" Text="Continue Move" />
                                        <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
                                    </BottomStripeContent>
                                </sb:PopupBox>

                                <div>
                                    <h3>Create, delete, rearrange and generally manage the locations throughout your Inventory here.</h3>
                                    <b>Select a location to perform an action below.</b>
                                    <br />
                                    <asp:Button ID="btnAddSubLoation" TabIndex="2" AccessKey="A" runat="server" Style="float: left;" CssClass="buttonStyle" Text="Add" />
                                    &nbsp;
                                <asp:Button ID="btnEdit" runat="server" TabIndex="3" Style="float: left;" AccessKey="E" CssClass="buttonStyle" Text="Edit" />
                                    &nbsp;
                                <asp:Button ID="btnDelete" runat="server" TabIndex="4" Style="float: left;" AccessKey="D" OnClick="btnDelete_Click" CssClass="buttonStyle" Text="Delete" />
                                    &nbsp;
                                <asp:Button ID="btnMove" runat="server" TabIndex="5" Style="float: left;" CssClass="buttonStyle" Text="Move" OnClick="btnMove_Click" AccessKey="M" OnClientClick="IntializeErrorMessages();" />
                                    &nbsp;
                            <br />
                                    <br />
                                    <div style="clear: both;"></div>
                                    <telerik:RadFormDecorator ID="formDecorator" runat="server" DecoratedControls="All" EnableRoundedCorners="false" DecorationZoneID="divRadTreeView" />
                                    <div style="overflow-y: auto; width: 70%; min-height: 400px; background-color: white; height: 400px;" id="divRadTreeView" class="left">
                                        <telerik:RadTreeView ID="tvLocation" AccessKey="T" TabIndex="1" DataValueField="LocationId" OnClientNodeClicked="ClientNodeClicked"
                                            OnClientKeyPressing="OnClientKeyPressing" DataFieldID="LocationId" DataTextField="LocationName" DataFieldParentID="ParentLocationId"
                                            runat="server" OnClientLoad="InitializeUI" OnNodeClick="tvLocation_NodeClick">
                                        </telerik:RadTreeView>
                                    </div>
                                    <div class="left" style="width: 25%; padding-left: 10px;" runat="server" visible="false" id="divLocationRoles">

                                        <p><b>This is a Managed Location</b></p>
                                        <p>Please use the Team tab to edit the roles for this location.</p>
                                        <br />

                                        <p>
                                            <b>Location Manager: </b>
                                            <asp:Label runat="server" ID="lblLocationManager"></asp:Label>
                                        </p>
                                        <br />

                                        <p><b>Inventory Staff: </b></p>
                                        <asp:ListBox runat="server" ID="lbxLocationStaff" Width="220" Height="100"></asp:ListBox>

                                        <p><b>Inventory Observers: </b></p>
                                        <asp:ListBox runat="server" ID="lbxLocationObservers" Width="220" Height="100"></asp:ListBox>

                                        <p style="text-align: center;">
                                            If you can't see a Team member's name, then they do not have access to this Managed Location.
                                        </p>
                                    </div>
                                    <br style="clear: both" />
                                </div>

                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </telerik:RadPageView>

                    <telerik:RadPageView ID="TeamTab" runat="server">
                        <telerik:RadWindowManager ID="RadWindowManager1" runat="server">
                        </telerik:RadWindowManager>
                        <asp:UpdatePanel ID="upnlInventoryTeam" UpdateMode="Conditional" runat="server">
                            <ContentTemplate>

                                <sb:PopupBox ID="popupChangePremission" Title="Team member's Inventory Roles" runat="server">
                                    <BodyContent>
                                        <div style="max-width: 600px; min-width: 550px;">
                                            <div style="width: 100%">
                                                <sb:InventoryLocationRoles runat="server" ID="sbInventoryLocationRoles" InventoryRolesDisplayMode="EditMode" />
                                                <br />
                                                <div runat="server" id="divInventoryAdminChangePremission">
                                                    <div style="text-align: center; width: 100%;">OR...</div>
                                                    <asp:RadioButton ID="rbtnInventoryAdmin" runat="server" Text="Inventory Administrator" GroupName="InventoryUserRole" CssClass="InventoryUserRole chkBold" />
                                                    <div style="padding: 5px 30px;">
                                                        There may only be one Inventory Administrator. They can:
                                                        <ul style="margin: 0px;">
                                                            <li>Invite new Inventory Team Members</li>
                                                            <li>Approve or deny booking requests</li>
                                                            <li>Receive all correspondence regarding bookings</li>
                                                            <li>Do everything Inventory Staff can do</li>
                                                        </ul>
                                                        <asp:HiddenField runat="server" ID="hdnCompanyUserId" />
                                                    </div>
                                                </div>
                                                <div runat="server" id="divInventoryAdminChangePremissionEmail">
                                                    <div style="padding: 10px 100px; text-align: center;">
                                                        <i>We’ll send your Team Member an email to update them on their new role(s).</i>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </BodyContent>
                                    <BottomStripeContent>
                                        <asp:Button ID="btnUpdatePermission" CssClass="buttonStyle" runat="server"
                                            OnClick="btnUpdatePermission_Click" Text="Save" />
                                        <input type="button" class="buttonStyle popupBoxCloser" value="Cancel" />
                                    </BottomStripeContent>
                                </sb:PopupBox>

                                <sb:PopupBox ID="popupLocationManagerAlreadyExist" Title="Just checking..." runat="server">
                                    <BodyContent>
                                        <div style="max-width: 400px;">
                                            <asp:Literal ID="ltrExistingLocationManagers" runat="server"></asp:Literal>
                                            If you continue, your new team member will become the Location Manager and the previous Location Manager will be made Inventory Staff instead.
                            Both team members will receive an email. Do you want to continue? 
                                        </div>
                                    </BodyContent>
                                    <BottomStripeContent>
                                        <asp:Button ID="btnConfirmDowngradeExistingLocationManager" CssClass="buttonStyle" runat="server" ValidationGroup="InventoryInvitation"
                                            OnClick="btnConfirmDowngradeExistingLocationManager_Click" Text="Send Invitation" />
                                        <input type="button" class="buttonStyle popupBoxCloser" value="Cancel" />
                                    </BottomStripeContent>
                                </sb:PopupBox>
                                <sb:SearchUsers ID="sbSearchUsers" OnInvitationSent="sbSearchUsers_InvitationSent" runat="server" DisplayMode="InventoryTeam" />

                                <telerik:RadGrid ID="gvInventoryTeam" ShowHeader="true" AllowSorting="true"
                                    AutoGenerateColumns="false" OnItemDataBound="gvInventoryTeam_ItemDataBound"
                                    OnItemCommand="gvInventoryTeam_ItemCommand" OnDeleteCommand="gvInventoryTeam_DeleteCommand"
                                    OnNeedDataSource="gvInventoryTeam_NeedDataSource" runat="server">
                                    <MasterTableView DataKeyNames="CompanyUserId,InvitationId,UserId" AllowNaturalSort="false"
                                        Width="100%" AllowSorting="false" AllowMultiColumnSorting="true" TableLayout="Fixed">
                                        <Columns>
                                            <telerik:GridTemplateColumn HeaderText="Name" UniqueName="Name">
                                                <ItemTemplate>
                                                    <asp:HyperLink runat="server" ID="lnkUserName" Target="_blank"></asp:HyperLink>
                                                    <asp:Label runat="server" ID="lblUserName"></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn HeaderText="Position" UniqueName="Position">
                                                <HeaderStyle Width="150" />
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn UniqueName="StageBitzRole" HeaderText="Highest Stagebitz Role">
                                                <HeaderTemplate>
                                                    <span class="left" style="padding-right: 5px;">Highest Stagebitz Role</span>
                                                    <span class="left">
                                                        <sb:HelpTip runat="server" ID="sbHelpTipRolesHeader">
                                                            This shows the most senior role for a Team Member across all the Managed Locations they may be invited to. 
                                                            For example, if a team member is an Inventory Observer in one Managed Location and Inventory Staff in another, 
                                                            they will show here as Inventory Staff.
                                                        </sb:HelpTip>
                                                    </span>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <div class="left">
                                                        <asp:Label runat="server" ID="lblPermission"></asp:Label>
                                                        <asp:LinkButton runat="server" ID="lbtnPermission" CommandName="ViewPermission" CommandArgument='<%# string.Concat(Eval("CompanyUserId"), "|", Eval("InvitationId")) %>'></asp:LinkButton>
                                                    </div>
                                                    <div class="right">
                                                        <asp:Image ID="imgCompAdmin" Width="16" Height="16" ImageUrl="~/Common/Images/administrator.png"
                                                            ToolTip="This user is also a Company Administrator" runat="server" Visible="false" />
                                                        <asp:ImageButton CommandArgument='<%# Bind("CompanyUserId") %>' runat="server" ID="ibtnEditPermision"
                                                            CommandName="EditPermission" ImageUrl="~/Common/Images/edit.png" Visible="false" ToolTip="Edit" />
                                                    </div>
                                                </ItemTemplate>
                                                <HeaderStyle Width="220" />
                                                <ItemStyle Width="170" />
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn HeaderText="Status" HeaderStyle-Width="100" UniqueName="Status"
                                                DataField="Status">
                                            </telerik:GridBoundColumn>
                                            <telerik:GridButtonColumn ConfirmText="Are you sure you want to remove?" ConfirmDialogType="RadWindow"
                                                ConfirmTitle="Remove" ButtonType="ImageButton" ConfirmDialogHeight="140" CommandName="Delete"
                                                Text="Delete" UniqueName="DeleteColumn">
                                                <ItemStyle HorizontalAlign="Center" />
                                                <HeaderStyle Width="30" />
                                                <ItemStyle Width="30" />
                                            </telerik:GridButtonColumn>
                                        </Columns>
                                    </MasterTableView>
                                    <SortingSettings EnableSkinSortStyles="false" />
                                </telerik:RadGrid>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </telerik:RadPageView>
                </telerik:RadMultiPage>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
