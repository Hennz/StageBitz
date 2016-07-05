<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WelcomeMessage.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Common.WelcomeMessage" %>
<%@ Register Src="~/Controls/Common/CountryList.ascx" TagName="CountryList" TagPrefix="uc1" %>

<script type="text/javascript">

    function showPopupSkipGetStartedConfirmation() {
        $("div.popupBoxOverlay", $("div[class='popupBox'][id$='popupSkipGetStartedConfirmation']")).css("z-index", "101");
        showPopup('popupSkipGetStartedConfirmation');
        return false;
    }

    function InitializeRadioButtons_<%= this.ClientID %>() {
        $(function () {
            HideAllRadioButtonHelpTips(0);
            $("input[type='radio']", "#<%= pnlPopupInner.ClientID%> .optGroup").unbind('click.FirstSelect');
            $("input[type='radio']", "#<%= pnlPopupInner.ClientID%> .optGroup").bind('click.FirstSelect', function () {

                var btnNext = $("input[id$='StartNextButton']", "#<%= pnlPopupInner.ClientID%>");
                if (btnNext.attr('disabled')) {
                    btnNext.removeAttr('disabled');
                    btnNext.removeClass('aspnetdisabled');
                }

                ShowRadioButtonHelpText($(this));

                if ($(this).parent('span').hasClass("skip")) {
                    btnNext.val('Finish');
                }
                else {
                    btnNext.val('Next');
                }
            });
        });
    }

    function ShowRadioButtonHelpText(element) {
        var speed = 500;
        HideAllRadioButtonHelpTips(speed);
        element.closest('.firstTimeLoginPopupOption').find('.RadioButton_HelpText').slideDown(speed);
    }

    function HideAllRadioButtonHelpTips(speed) {
        $(".firstTimeLoginPopupOption:not(:has(input[type='radio']:checked))").find('.RadioButton_HelpText').slideUp(speed);
    }
</script>
<asp:UpdatePanel runat="server" ID="upnlWizard">
    <ContentTemplate>
        <sb:PopupBox ID="popupFirstTimeLoginDirect" runat="server" Height="100" ShowCornerCloseButton="false" ShowBottomStripe="false" Title="Welcome to StageBitz">
            <BodyContent>
                <asp:Panel runat="server" ID="pnlPopupInner" Width="850">
                    <asp:Wizard ID="wizard" EnableViewState="true" runat="server" Width="100%" DisplaySideBar="false">
                        <WizardSteps>
                            <asp:WizardStep ID="Step1">
                                <div class="firstTimeLoginPopupDiv">
                                    <div class="firstTimeLoginPopupSubHeading">
                                        Let’s get you started.
                                        <br />
                                        <br />
                                        <span style="font-size: smaller; padding-top: 10px;">First choose to join an existing team or start a new one.</span>
                                    </div>
                                    <div>
                                        <span class="firstTimeLoginPopupSectionHeader">Join a Team:&nbsp;</span>
                                        <span class="firstTimeLoginPopupSectionHeaderText">If someone has asked you to sign up because they’re about to invite you to their team, this is the option for you.</span>
                                        <div class="firstTimeLoginPopupOptions">
                                            <div class="firstTimeLoginPopupOption">
                                                <asp:RadioButton runat="server" ID="rbtnExpectingInvitation" Text="I’m expecting an invitation to join an existing Company, Project or Inventory." GroupName="optGroup" CssClass="optGroup skip" />
                                                <div class="RadioButton_HelpText">
                                                    <p>
                                                        - When the Company Administrator has invited you , the invitation will appear on your dashboard, and you’ll get an email.
                                                    </p>
                                                    <p>
                                                        - Just make sure you tell them which email address you signed up with!
                                                    </p>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div>
                                        <span class="firstTimeLoginPopupSectionHeader">Start a Team:&nbsp;</span>
                                        <span class="firstTimeLoginPopupSectionHeaderText">If you would like to start a new Company and make use of the free trial Project or the free Inventory starter plan select one of these options.</span>
                                        <div class="firstTimeLoginPopupOptions">
                                            <div class="firstTimeLoginPopupOption">
                                                <asp:RadioButton runat="server" ID="rbtnCreateNewProject" Text="I want to trial creating a new Project." GroupName="optGroup" CssClass="optGroup" />
                                                <div class="RadioButton_HelpText">
                                                    <p>
                                                        - You will be asked to create a Company and a Project. The Project is Free for <%= this.FreeTrialWeeks %> weeks.
                                                    </p>
                                                </div>
                                            </div>
                                            <div class="firstTimeLoginPopupOption">
                                                <asp:RadioButton runat="server" ID="rbtnCreateInventory" Text="I want to trial building a new Inventory." GroupName="optGroup" CssClass="optGroup" />
                                                <div class="RadioButton_HelpText">
                                                    <p>
                                                        - You will be asked to create a Company, Up to <%= StageBitz.Common.Utils.GetSystemValue("DefaultInventoryCountForCompany") %> Items are free.
                                                    </p>
                                                </div>
                                            </div>
                                            <div class="firstTimeLoginPopupOption">
                                                <asp:RadioButton runat="server" ID="rbtnCreateProjectAndInventory" Text="I want to trial creating both a Project and an Inventory." GroupName="optGroup" CssClass="optGroup" />
                                                <div class="RadioButton_HelpText">
                                                    <p>
                                                        - You will be asked to create a Company and a Project. 
                                                    </p>
                                                    <p>
                                                        &nbsp;&nbsp;&nbsp;&nbsp;The Project is Free for <%= this.FreeTrialWeeks %> weeks and you can create up to <%= StageBitz.Common.Utils.GetSystemValue("DefaultInventoryCountForCompany") %> Items for free.
                                                    </p>
                                                </div>
                                            </div>
                                            <br />
                                            <div>
                                                <p><b>You will be able to explore the other areas of StageBitz at any point down the track.</b></p>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </asp:WizardStep>
                            <asp:WizardStep ID="Step2">
                                <asp:Label Font-Size="Smaller" ID="lblFreeTrialText" runat="server"></asp:Label>
                                <div class="firstTimeLoginPopupParagraph">
                                    <p>
                                        <asp:Label runat="server" ID="lblPackageInfo"></asp:Label>
                                    </p>
                                    <br />
                                    <div style="margin-left: 130px; text-align: left;">
                                        <p>
                                            <b>
                                                <asp:Label runat="server" ID="lblCompanyCreateDescription"></asp:Label></b>
                                        </p>
                                        You can change the name of the Company at any time.
                                    <br />
                                    </div>
                                    <div style="margin-left: 140px; text-align: left;">
                                        <div class="left" style="padding-top: 5px; padding-left: 10px;">
                                            Company Name:&nbsp;
                                        </div>
                                        <div class="left" style="display: inline-block; width: 200px;">
                                            <asp:TextBox ID="txtCompanyName" MaxLength="50" runat="server"></asp:TextBox>
                                        </div>
                                        <div class="left" style="padding-top: 7px; margin-left: 2px; min-height: 17px;">
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtCompanyName"
                                                ValidationGroup="FieldsValidation" runat="server" ErrorMessage="*" ToolTip="Company Name is required."></asp:RequiredFieldValidator>
                                        </div>
                                        <div style="clear: both;"></div>
                                        <div class="left" style="padding-top: 5px; width: 100px; padding-left: 10px;">
                                            Country:&nbsp;
                                        </div>
                                        <div class="left" style="display: inline-block; width: 300px;">
                                            <uc1:CountryList ID="ucCountryList" ValidationGroup="FieldsValidation" DropDownWidth="200" runat="server" ValidationErrorMessage="*" ValidationToolTip="Please select a Country." />
                                            <br />

                                        </div>
                                        <div style="clear: both;"></div>

                                    </div>
                                    <div style="font-size: 10px; margin-left: 125px;" id="divCurrencyNote" runat="server">
                                        This will set the currency symbol for this Company.
                                    </div>

                                    <div id="divProjectCreateArea" runat="server">
                                        <br />
                                        <div style="margin-left: 130px; text-align: left;">
                                            <b>Now you can create your Project.</b>
                                            <br />
                                            Again, you can change this later on.<br />
                                        </div>
                                        <div style="text-align: right">
                                            <div class="left" style="display: inline-block; padding-top: 5px; padding-left: 150px; width: 103px;">
                                                Project Name: &nbsp;
                                            </div>
                                            <div class="left" style="display: inline-block; width: 200px;">
                                                <asp:TextBox ID="txtProjectName" MaxLength="100" runat="server"></asp:TextBox>
                                            </div>
                                            <div class="left" style="padding-top: 8px; padding-left: 2px;">
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="txtProjectName"
                                                    ValidationGroup="FieldsValidation" runat="server" ErrorMessage="*" ToolTip="Project Name is required."></asp:RequiredFieldValidator>
                                            </div>
                                            <div style="clear: both;"></div>
                                        </div>
                                    </div>
                                </div>
                                <div style="text-align: center; padding-top: 10px">
                                    Hover over me for your first tool tip... 
                        <div style="display: inline-block; margin-top: 3px;">
                            <sb:HelpTip ID="HelpTipFirstTimeLoginDirect" runat="server">
                                <div style="float: left; width: 310px; line-height: 15px;">
                                    <p>
                                        Creating a Company means you are now the Primary Company Administrator.
                                        <asp:Label runat="server" ID="lblHelpTipInfo1"></asp:Label>
                                    </p>
                                    <br />
                                    <p>
                                        <i>
                                            <asp:Label runat="server" ID="lblHelpTipInfo2"></asp:Label>
                                        </i>
                                    </p>
                                </div>
                            </sb:HelpTip>
                        </div>
                                    <br />
                                    <br />
                                    Now you can start adding Props, Costumes, Scenery and all those other Bitz that make up your
                                    <br />
                                    <asp:Literal runat="server" ID="ltrlWizardStep2Footer"></asp:Literal>
                                </div>
                            </asp:WizardStep>
                        </WizardSteps>

                        <StartNavigationTemplate>
                            <asp:Button ID="StartNextButton" CssClass="buttonStyle" runat="server" OnClick="StartNextButton_Click" CommandName="MoveNext"
                                Text="Next" ValidationGroup="FieldsValidation" Enabled="false" />
                        </StartNavigationTemplate>
                        <FinishNavigationTemplate>
                            <asp:Button ID="FinishButton" runat="server" OnClick="FinishButton_Click" CssClass="buttonStyle"
                                CommandName="MoveComplete" Text="Finish" ValidationGroup="FieldsValidation" />
                            <asp:Button ID="FinishPreviousButton" runat="server" OnClick="FinishPreviousButton_Click" CssClass="buttonStyle"
                                CausesValidation="False" CommandName="MovePrevious" Text="Previous" />
                        </FinishNavigationTemplate>
                    </asp:Wizard>
                </asp:Panel>
            </BodyContent>
            <BottomStripeContent>
            </BottomStripeContent>
        </sb:PopupBox>
    </ContentTemplate>
</asp:UpdatePanel>

<sb:PopupBox ID="popupFirstTimeLoginInvited" runat="server" Height="100" ShowTitleBar="false">
    <BodyContent>
        <div class="firstTimeLoginPopupHeading">Welcome to StageBitz </div>
        <div class="firstTimeLoginPopupSubHeading">Let's get you started.</div>
        <br />
        <div class="firstTimeLoginPopup" style="width: 550px;">
            <p>
                You've been invited to be part of a Team working in StageBitz. Stagebitz is an online tool 
            specially designed to let you;
            </p>
            <div style="text-align: left;">
                <ul>
                    <li>Collaborate with your colleagues to get all the props, costumes and other 'bitz' you need for each of your projects; and</li>
                    <div style="height: 5px;"></div>
                    <li>Collate all those Items for each Company into a central Inventory, ready for the next Project.</li>
                </ul>

            </div>

            <p>
                You will find invitations to Projects and Companies on your Personal Dashboard (the first screen you'll see).
            </p>
            <br />
            <p>
                And you'll find helpful tips on using StageBitz everywhere - with the 'Feedback & Support' tab at the bottom 
                of the screen and our tooltips. 
            </p>
            <br />
            Hover over me for your first tool tip...   
                <div style="display: inline-block; margin-top: 3px;">
                    <sb:HelpTip ID="HelpTipFirstTimeLoginInvited" runat="server">
                        <div style="float: left; width: 310px; line-height: 20px;">
                            It's easy to find the help you need in StageBitz. Just hover on any of the
                            <div style="display: inline-block;" class="helpTipIcon"></div>
                            icons, or click on the 
                        Feedback and Support tab that's at the edge of each screen.
                        </div>
                    </sb:HelpTip>
                </div>
            <br />
            <br />
            OK, let's get things organised!
        </div>
    </BodyContent>
    <BottomStripeContent>
        <asp:Button ID="btnGetStartedInvited" CssClass="buttonStyle" runat="server"
            OnClick="btnGetStartedInvited_Click" Text="Get Started" />
    </BottomStripeContent>
</sb:PopupBox>
