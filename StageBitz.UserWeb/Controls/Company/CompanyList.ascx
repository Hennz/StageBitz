<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompanyList.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.Company.CompanyList" %>
<%@ Register Src="~/Controls/Common/InvitationViewer.ascx" TagName="InvitationViewer"
    TagPrefix="sb" %>
<asp:UpdatePanel ID="up" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <script type="text/javascript">
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
                <asp:Literal runat="server" Text="Companies and Inventories" />
                <sb:HelpTip ID="HelpTip1" runat="server">
                    <div style="width: 375px;">
                        <strong>Only Companies can own Projects and Inventories</strong>
                        <ol>
                            <li><b>If your company already has a StageBitz account,</b> talk to the Company Administrator
                                to get a new Project started or to get access to the Inventory.</li>
                            <li><b>If you need a new StageBitz account for your Company,</b> you can create one for it by 
                                following the 'Create New Company' link.</li>
                        </ol>
                    </div>
                </sb:HelpTip>

            </TitleLeftContent>
            <BodyContent>
                <p style="text-align: right;">
                    <a id="linkCreateNewCompany" href="~/Company/AddNewCompany.aspx" runat="server">Create
                     New Company </a>
                </p>
                <div id="divNotification" runat="server" visible="false" class="message success" style="margin-bottom: 5px;">
                </div>
                <div style="max-height: 500px; overflow-y: auto; overflow-x: hidden;">
                    <asp:Panel runat="server" ID="pnlToolTipLocation" Style="width: 100px;"></asp:Panel>
                    <telerik:RadToolTip runat="server" ID="helptipWaitingForInvitation" VisibleOnPageLoad="true" HideEvent="ManualClose"
                        TargetControlID="pnlToolTipLocation" Position="TopRight" CssClass="showOnLoad" OnClientShow="RadToolTip_OnClientShow" Visible="false"
                        ShowEvent="FromCode" OnClientBeforeShow="RadToolTip_OnClientBeforeShow">
                        <div style="text-align: left; padding: 10px;">
                            <p><b>This is your Personal Dashboard. Here you will be able to access any Companies, Inventories or Projects you have access to.</b></p>
                            <br />
                            <p><b>So what happens next?</b></p>
                            <br />
                            <div style="padding-left: 10px;">
                                <p>
                                    - You will be sent an email letting you know when you have been invited to join a Project or the Company team.
                                </p>
                                <p>
                                    - Just let the Company Administrator know your login email so they can invite you.
                                </p>
                                <p>
                                    - When you receive the invite email simply follow the link to this page and you will see the invite ready to accept.
                                </p>
                            </div>
                        </div>
                    </telerik:RadToolTip>
                    <asp:ListView ID="lvCompanies" runat="server" OnItemCommand="lvCompanies_ItemCommand"
                        OnItemDataBound="lvCompanies_ItemDataBound">
                        <LayoutTemplate>
                            <div runat="server" id="itemPlaceholder"></div>
                            <br style="clear: both;" />
                        </LayoutTemplate>
                        <ItemTemplate>
                            <div class="thumbListItem" style="width: 160px; height: 170px;">
                                <asp:PlaceHolder ID="plcHolderPending" runat="server">
                                    <asp:LinkButton ID="linkViewInviteBorder" ClientIDMode="AutoID" CommandName="ViewInvite"
                                        Style="text-decoration: none;" runat="server">
                                        <table id="Table1" runat="server">
                                            <tr>
                                                <td>
                                                    <asp:Image ID="imgPendingCompanies" runat="server" ImageUrl="~/Common/Images/folder_Thumb.png" Visible="false" />
                                                    <sb:ImageDisplay Visible="false" ID="idPendingCompanies" runat="server" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:LinkButton>
                                    <asp:Label ID="lblPendingCompanyName" runat="server" /><br />
                                    <span style="font-style: italic; color: Red;">Invite Pending...</span>
                                    <br />
                                    <asp:LinkButton ID="lnkbtnViewInvite" ClientIDMode="AutoID" CommandName="ViewInvite"
                                        runat="server">View Invite</asp:LinkButton>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="plcHolderCompanies" runat="server">
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Image ID="imgCompanies" runat="server" ImageUrl="~/Common/Images/folder_Thumb.png" Visible="false" />
                                                <sb:ImageDisplay ID="idCompanies" Visible="false" runat="server" />
                                            </td>
                                        </tr>
                                    </table>
                                    <div style="text-align: center;">
                                        <asp:Label ID="lblCompanyName" runat="server" /><br />

                                        <div class="left" style="text-align: center; margin-left: 10px; min-width: 20px;">
                                            &nbsp;
                                              <asp:Image ID="imgCompanyUsers" Width="16" Height="16" ImageUrl="~/Common/Images/administrator.png"
                                                  runat="server" Visible="false" />
                                        </div>
                                        <div class="left" id="divLinks" runat="server" style="text-align: center; width: 63%;">
                                            <span id="dashboardLink" runat="server"><a id="lnkCompanies" runat="server" class="smallText">Dashboard</a>&nbsp;|&nbsp;</span><a runat="server" id="lnkInventory" class="smallText">Inventory</a>
                                        </div>
                                        <div style="clear: both;"></div>
                                    </div>
                                </asp:PlaceHolder>
                            </div>
                        </ItemTemplate>
                    </asp:ListView>
                </div>
                <div id="divNoCompanies" runat="server" visible="false" style="padding-top: 12px; margin-left: 30px; padding-right: 12px; padding-bottom: 12px; padding-left: 12px;">
                    <p><b>Here's where you'll see any Companies that you are working with.</b></p>
                    <ul>
                        <li>If you’re invited to a Company, the invitation will appear here.</li>
                        <li>To create a new Company, just select the link at the right.</li>
                    </ul>
                </div>

                <%--this tooltip is for the 'lvCompanies' list view--%>
                <telerik:RadToolTip runat="server" ID="helpTipCompanyList" VisibleOnPageLoad="true" HideEvent="ManualClose"
                    TargetControlID="lnkInventory" Position="TopRight" CssClass="showOnLoad" OnClientShow="RadToolTip_OnClientShow"
                    OffsetY="-5" Visible="false" ShowEvent="FromCode" OnClientBeforeShow="RadToolTip_OnClientBeforeShow">
                    <div style="text-align: left; padding: 10px; max-width: 700px; white-space: normal;">
                        <p><b>These links give you access to:</b></p>
                        <br />
                        <div style="padding-left: 10px;">
                            <p>
                                - The Company Dashboard, where you can create new Projects and manage the Company.
                            </p>
                            <p>
                                <asp:Label runat="server" ID="lblTooltipInventoryInfo"></asp:Label>
                            </p>
                        </div>
                    </div>
                </telerik:RadToolTip>
            </BodyContent>
        </sb:GroupBox>
    </ContentTemplate>
</asp:UpdatePanel>
