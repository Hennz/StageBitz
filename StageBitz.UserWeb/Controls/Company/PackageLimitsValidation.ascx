<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PackageLimitsValidation.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Company.PackageLimitsValidation" %>
<asp:UpdatePanel runat="server" ID="upnlPopUps" UpdateMode="Conditional" RenderMode="Inline">
    <ContentTemplate>
        <sb:PopupBox ID="popupCreateNewProjectsDuringFreeTrail" runat="server" ShowCornerCloseButton="false" Title="Glad you're enjoying StageBitz!" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>You’re still in your Free Trial, which is limited to
                                <asp:Label runat="server" ID="lblProjectLimit"></asp:Label>.</b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        If you want to create a new Project, you just need to visit our Pricing Plan page and upgrade to the level of Project and Inventory that's right for you. 
                        Once you've confirmed your payment details, you can start creating new Projects.
                    </p>
                    <br />
                    <p>
                        Of course, your Free Trial is still free, so you won't be charged anything at all until that ends.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnUpgradeCreateNewProjectsDuringFreeTrail" OnClick="btnUpgrade_Click" class="buttonStyle" Text="Upgrade" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupCreateNewProjectsNoPricingPlan" runat="server" ShowCornerCloseButton="false" Title="You have reached your limits" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>
                                <asp:Label runat="server" ID="lblProjectLimitFreeTrailEnd" Text="You have finished your Free Trial."></asp:Label>
                                <asp:Label runat="server" ID="lblProjectLimitNoPackage" Text="You have not selected a Pricing Plan yet." Visible="false"></asp:Label>
                            </b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        If you want to create a new Project, you just need to visit our Pricing Plan page and upgrade to the level of Project and Inventory that's right for you. 
                        Once you've confirmed your payment details, you can start creating new Projects.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnUpgradeCreateNewProjectsAfterFreeTrailNoPricingPlan" OnClick="btnUpgrade_Click" class="buttonStyle" Text="Upgrade" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupProjectsNoPricePlan" runat="server" ShowCornerCloseButton="false" Title="Please choose a Pricing Plan" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>
                                <asp:Label runat="server" ID="lblReActivateProjectsNoPricePlan" Text="To reactivate this Project you will need to first choose a Pricing Plan and set up your payment details."></asp:Label>
                                <asp:Label runat="server" ID="lblReActivateProjectsNoPaymentDetails" Text="To reactivate this Project you will need to set up your payment details." Visible="false"></asp:Label>
                            </b>
                        </p>
                        <br />
                        <p>
                            You can do this from Pricing Plan Page.
                        </p>
                    </div>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnGotoPricingPlanReActivateProjects" OnClick="btnUpgrade_Click" class="buttonStyle" Text="Go to Pricing Plan Page" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

         <sb:PopupBox ID="popupReActivateProjectsUserLimitExceeded" runat="server" ShowCornerCloseButton="false" Title="Need more people on your team?" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div>
                        <p style="text-align: center;">
                            <b>
                                If you reactivate this Project with its full team, you'll exceed the number of active users your current subscription allows by <asp:Label runat="server" ID="lblPackageUserCount"></asp:Label> users.
                            </b>
                        </p>
                        <br />
                        <p style="text-align: left;">
                            But this is an easy to fix! You can either:
                            <ul>
                                <li>Go to the <asp:HyperLink runat="server" ID="lnkManageProjectTeam" Text="Manage Project Team" Target="_blank"></asp:HyperLink> page and reduce the number of team members to be reactivated; or</li>
                                <li>Upgrade to a Project level that includes the number of active users that is right for you. You can do that from our <asp:HyperLink runat="server" ID="lnkPricingPlan" Text="Pricing Plan" Target="_blank"></asp:HyperLink> page.</li>
                            </ul>
                        </p>
                    </div>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnUpgradeReActivateProjectsUserLimitExceeded" OnClick="btnUpgrade_Click" class="buttonStyle" Text="Upgrade" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupCheckProjectsLimits" runat="server" ShowCornerCloseButton="false" Title="You have reached your limits" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>You already have as many active Projects as your current subscription allows.</b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        All you need to do is to upgrade your Project level to the number of active Projects that is right for you. You can do that from our Pricing Plan page.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnUpgradeCheckProjectsLimits" OnClick="btnUpgrade_Click" class="buttonStyle" Text="Upgrade" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupFreeTrailUserLimitCompanyAdmin" runat="server" ShowCornerCloseButton="false" Title="Glad you're enjoying StageBitz!" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>You’re still in your Free Trial, which is limit of <%= this.DefaultFreeTrialUserCount %> active users.</b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        If you want to increase the number of active users you can have, you just need to visit our Pricing Plan page and upgrade to the level of Project and Inventory that's right for you. 
                        Once you've confirmed your payment details, you can start inviting new users.
                    </p>
                    <br />
                    <p>
                        Of course, your Free Trial is still free, so you won't be charged anything at all until that ends.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnUpgradeFreeTrailUserLimitCompanyAdmin" OnClick="btnUpgrade_Click" class="buttonStyle" Text="Upgrade" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupFreeTrailUserLimitProjectAdmin" runat="server" ShowCornerCloseButton="false" Title="Glad you're enjoying StageBitz!" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>You’re still in your Free Trial, which is limit of <%= this.DefaultFreeTrialUserCount %> active users.</b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        If you want to increase the number of active users you can have, you just need to send a request to <%= this.PrimaryCompanyAdmin %>,
                             your Primary Company Administrator, asking them to upgrade your account. 
                            As soon as they’ve done that you’ll be able to invite your other team members.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnFreeTrailUserLimitSendEmail" CommandName="UserLimitProjectAdmin" OnClick="btnSendEmail_Click" Text="Send Request" CssClass="buttonStyle" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupUserLimitCompanyAdmin" runat="server" ShowCornerCloseButton="false" Title="Need more people on your team?" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>
                                <asp:Label runat="server" ID="lblUserLimitCompanyAdmin" Text="You already have as many active users as your current subscription allows."></asp:Label>
                                <asp:Label runat="server" ID="lblUserLimitCompanyAdminFreeTrailEndNoPackage" Text="You have finished your Free Trial." Visible="false"></asp:Label>
                                <asp:Label runat="server" ID="lblUserLimitCompanyAdminNoPackage" Text="You have not selected a Pricing Plan yet." Visible="false"></asp:Label>
                            </b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        All you need to do is to upgrade your Project level to the number of active users that is right for you. You can do that from our Pricing Plan page.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnUpgradeUserLimitCompanyAdmin" OnClick="btnUpgrade_Click" class="buttonStyle" Text="Upgrade" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupUserLimitProjectAdmin" runat="server" ShowCornerCloseButton="false" Title="Need more people on your team?" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>
                                <asp:Label runat="server" ID="lblUserLimitProjectAdmin" Text="You already have as many active users as your current subscription allows."></asp:Label>
                                <asp:Label runat="server" ID="lblUserLimitProjectAdminFreeTrailEndNoPackage" Text="You have finished your Free Trial." Visible="false"></asp:Label>
                                <asp:Label runat="server" ID="lblUserLimitProjectAdminNoPackage" Text="You have not selected a Pricing Plan yet." Visible="false"></asp:Label>
                            </b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        If you would like to increase the number of active users, you can send a request to <%= this.PrimaryCompanyAdmin %>, your Primary Company Administrator, and ask them to upgrade.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnUserLimitSendEmail" CommandName="UserLimitProjectAdmin" OnClick="btnSendEmail_Click" Text="Send Request" CssClass="buttonStyle" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupFreeTrailInventoryLimitCompanyAdmin" runat="server" ShowCornerCloseButton="false" Title="Glad you're enjoying StageBitz!" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>You’re still in your Free Trial, which is limit of <%= this.DefaultFreeTrialInventoryLimit %> Inventory Items.</b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        If you want to increase the number of Items you can have in your Inventory, you just need to visit our Pricing Plan page and upgrade to the level of Project and Inventory that's right for you. 
                        Once you've confirmed your payment details, you can start adding new Items to your Inventory.
                    </p>
                    <br />
                    <p>
                        Of course, your Free Trial is still free, so you won't be charged anything at all until that ends.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnUpgradeFreeTrailInventoryLimitCompanyAdmin" OnClick="btnUpgrade_Click" class="buttonStyle" Text="Upgrade" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupFreeTrailInventoryLimitInventoryManager" runat="server" ShowCornerCloseButton="false" Title="Glad you're enjoying StageBitz!" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>You’re still in your Free Trial, which is limit of <%= this.DefaultFreeTrialInventoryLimit %> Inventory Items.</b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        If you want to increase the number of Items you can have in your Inventory, you just need to send a request to <%= this.PrimaryCompanyAdmin %>,
                             your Primary Company Administrator, asking them to upgrade your account. 
                            As soon as they’ve done that you’ll be able to add new Items to your Inventory.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnFreeTrailInventoryLimitSendEmail" CommandName="InventoryLimitInventoryManager" OnClick="btnSendEmail_Click" Text="Send Request" CssClass="buttonStyle" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupInventoryLimitCompanyAdmin" runat="server" ShowCornerCloseButton="false" Title="Need more Items in your Inventory?" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>
                                <asp:Label runat="server" ID="lblInventoryLimitCompanyAdmin" Text="You already have as many Items in your Inventory as your current subscription allows."></asp:Label>
                                <asp:Label runat="server" ID="lblInventoryLimitCompanyAdminFreeTrailEndNoPackage" Text="You have finished your Free Trial." Visible="false"></asp:Label>
                                <asp:Label runat="server" ID="lblInventoryLimitCompanyAdminNoPackage" Text="You have not selected a Pricing Plan yet." Visible="false"></asp:Label>
                            </b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        All you need to do is to upgrade your Inventory level to the size that is right for you. You can do that from our Pricing Plan page.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnUpgradeInventoryLimitCompanyAdmin" OnClick="btnUpgrade_Click" class="buttonStyle" Text="Upgrade" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>

        <sb:PopupBox ID="popupInventoryLimitInventoryManager" runat="server" ShowCornerCloseButton="false" Title="Need more Items in your Inventory?" Visible="false">
            <BodyContent>
                <div style="width: 550px;">
                    <div style="text-align: center;">
                        <p>
                            <b>
                                <asp:Label runat="server" ID="lblInventoryLimitInventoryManager" Text="You already have as many Items in your Inventory as your current subscription allows."></asp:Label>
                                <asp:Label runat="server" ID="lblInventoryLimitInventoryManagerFreeTrailEndNoPackage" Text="You have finished your Free Trial." Visible="false"></asp:Label>
                                <asp:Label runat="server" ID="lblInventoryLimitInventoryManagerNoPackage" Text="You have not selected a Pricing Plan yet." Visible="false"></asp:Label>
                            </b>
                        </p>
                        <p>
                            <b>But don't let that stop you!</b>
                        </p>
                    </div>
                    <br />
                    <p>
                        If you would like to increase the number of Inventory Items, you can send a request to <%= this.PrimaryCompanyAdmin %>, your Primary Company Administrator, and ask them to upgrade.
                    </p>
                </div>
            </BodyContent>
            <BottomStripeContent>
                <asp:Button runat="server" ID="btnInventoryLimitSendEmail" CommandName="InventoryLimitInventoryManager" OnClick="btnSendEmail_Click" Text="Send Request" CssClass="buttonStyle" />
                <input type="button" class="popupBoxCloser buttonStyle" value="Cancel" />
            </BottomStripeContent>
        </sb:PopupBox>
    </ContentTemplate>
</asp:UpdatePanel>
