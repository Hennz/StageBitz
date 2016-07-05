<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectWarningPopup.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Project.ProjectWarningPopup" %>

<asp:UpdatePanel runat="server" UpdateMode="Always">
    <ContentTemplate>
        <sb:PopupBox ID="popupProjectSuspendedWarning" Title="This Project is currently Suspended" runat="server" ShowCornerCloseButton="false">
            <BodyContent>
                <div style="width: 600px;">
                    You'll still be able to view all your information, but you won't be able to create, delete or edit any information.
                    If you have any questions, please contact
                    <asp:Literal runat="server" ID="ltrlPrimaryAdminProjectSuspended"></asp:Literal>, your Primary Company Administrator.
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnReload" Text="Reload Page" OnClick="btnReload_Click" CssClass="buttonStyle" />
            </BottomStripeContent>
        </sb:PopupBox>
        <sb:PopupBox ID="popupProjectCloseWarning" Title="This Project is finished and has been closed" runat="server" ShowCornerCloseButton="false">
            <BodyContent>
                <div style="width: 600px;">
                    <p>
                        <asp:Label runat="server" ID="lblClosedBy"></asp:Label>
                        has just closed this Project.
                    </p>
                    <br />
                    <p>This means you will not be able to add or edit any information to this Project and it will no longer appear on your Personal Dashboard.</p>
                    <br />
                    <p>
                        <asp:Label runat="server" ID="lblCompanyName"></asp:Label>‘s Administrators are able to access a read only version of the Project from the&nbsp;
                        Company Dashboard. Please contact your Primary Company Administrator,&nbsp;
                        <asp:Literal runat="server" ID="ltrlPrimaryAdmin"></asp:Literal>, if you have any questions.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnGotoMyDashboard" Text="Go to my Dashboard" OnClick="btnGotoMyDashboard_Click" CssClass="buttonStyle" />
            </BottomStripeContent>
        </sb:PopupBox>
        <sb:PopupBox ID="popupProjectCloseWarningInventory" Title="The Project giving you access to this Inventory has been closed" runat="server" ShowCornerCloseButton="false">
            <BodyContent>
                <div>
                    <div style="width: 600px;">
                        <p>
                            <asp:Label runat="server" ID="lblClosedByInventory"></asp:Label>
                            has just closed the Project you are working on for
                        <asp:Label runat="server" ID="lblCompanyNameInventory1"></asp:Label>.
                        </p>
                        <br />
                        <p>
                            This means you will not be able to add or edit any information to this Project and&nbsp;
                        it will no longer appear on your Personal Dashboard.
                        </p>
                        <br />
                        <p>
                            <asp:Label runat="server" ID="lblCompanyNameInventory2"></asp:Label>’s Administrators are able to access a read only version of the Project from the Company Dashboard.&nbsp;
                            Please contact your Primary Company Administrator,
                        <asp:Literal runat="server" ID="ltrlPrimaryAdminInventory"></asp:Literal>, if you have any questions.
                        </p>
                        <br />
                        <p>As this Project has been closed you no longer have access to this Company’s inventory and will be taken to your Personal Dashboard.</p>
                    </div>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnGotoMyDashboardInventory" Text="Go to my Dashboard" OnClick="btnGotoMyDashboard_Click" CssClass="buttonStyle" />
            </BottomStripeContent>
        </sb:PopupBox>
        <sb:ErrorPopupBox ID="popupNoEditPermission" Title="Change to your Permissions" runat="server" ShowCornerCloseButton="false" ErrorCode="NoEditPermissionForInventory" >
            <BodyContent>
                <div>
                    <div style="width: 300px;">
                        <p>
                            Your permission level in the Inventory has just been changed.  An email has been sent to you with the details. 
                        </p>
                    </div>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <input value="Ok" type="button" onclick="location.reload();" class="buttonStyle" runat="server" id="btnOk" />
                <asp:HyperLink CssClass="buttonStyle" NavigateUrl="~/Default.aspx" runat="server" Text="Ok" Visible="false" ID="lnkOk"></asp:HyperLink>
            </BottomStripeContent>
        </sb:ErrorPopupBox>
        <sb:ErrorPopupBox ID="popupItemNotVisible" Title="Item Update" runat="server" ShowCornerCloseButton="false" ErrorCode="ItemNotVisible" >
            <BodyContent>
                <div>
                    <div style="width: 300px;">
                        <p>
                            The visibility settings for this Item have just been changed. You will not have access to this Item once you close this message.  Please contact your Inventory Administrator, 
                            <asp:HyperLink runat="server" ID="lnkContactInventoryAdmin"></asp:HyperLink>, if you have any questions.
                        </p>
                    </div>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:HyperLink CssClass="buttonStyle" NavigateUrl="~/Default.aspx" runat="server" Text="Ok" ID="lnkOnVisibility"></asp:HyperLink>
            </BottomStripeContent>
        </sb:ErrorPopupBox>
    </ContentTemplate>
</asp:UpdatePanel>
