<%@ Page Title="" Language="C#" MasterPageFile="~/Content.Master" AutoEventWireup="true"
    DisplayTitle="Create New Project" CodeBehind="AddNewProject.aspx.cs" Inherits="StageBitz.UserWeb.Project.AddNewProject" %>

<%@ Register Src="~/Controls/Project/ProjectEvents.ascx" TagName="Event" TagPrefix="uc1" %>
<%@ Register Src="~/Controls/Project/ProjectLocations.ascx" TagName="Location" TagPrefix="uc2" %>

<%@ Register Src="~/Controls/Company/PackageLimitsValidation.ascx" TagPrefix="uc1" TagName="PackageLimitsValidation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">

    <telerik:RadWindowManager ID="mgr" runat="server">
    </telerik:RadWindowManager>
    <uc1:PackageLimitsValidation runat="server" ID="ucPackageLimitsValidation" />
    <asp:UpdatePanel ID="upd" runat="server">
        <ContentTemplate>


            <div style="float: left; width: 100%;">
                <div style="float: left; padding-top: 5px; margin-right: 15px; margin-top: 10px!important">
                    <b>Project Name:</b>
                </div>
                <div style="float: left; margin-bottom: 0px; margin-top: 10px!important">
                    <asp:TextBox ID="txtProjectName" MaxLength="100" Width="230" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtProjectName"
                        ValidationGroup="valgroup" runat="server" ErrorMessage="Project name is required."></asp:RequiredFieldValidator>
                    <span id="errormsg" style="top:0px !important" class="inputError" runat="server"></span>
                </div>  
            </div>

            <div style="width: 45%; float: left">
                <div style="float: left;">
                    <h2>Schedule</h2>
                </div>
                &nbsp;
                        <div style="float: left; margin-left: 4px; padding-top: 10px;">
                            <sb:HelpTip ID="HelpTip1" runat="server">
                                Schedule: List key dates like 'Technical Rehearsal', 'Opening Night', 'First reading'
                                etc.
                            </sb:HelpTip>
                        </div>
                <div style="clear: both;">
                </div>

                <uc1:Event ID="ucProjectEvents" runat="server" />

            </div>
            <div style="width: 10px; float: left">&nbsp; </div>
            <div style="width: 50%; margin-top: 53px; float: left">
                <div style="float: left;">
                    <h2>Locations</h2>
                </div>
                &nbsp;
                        <div style="float: left; margin-left: 4px; padding-top: 10px;">
                            <sb:HelpTip ID="HelpTip3" runat="server">
                                Location: List the details for any venue or location that your team might need to
                                know about, like 'Workshop', 'Rehearsal venue', 'Soundstage' etc.
                            </sb:HelpTip>
                        </div>
                <div style="clear: both;">
                </div>
                <uc2:Location ID="ucLocation" runat="server" />
            </div>

            <div style="clear: both;"></div>
            <div class="buttonArea" style="text-align: right; margin-top: 5px;">
                <asp:Button ID="btnCreateNextProject" ValidationGroup="valgroup" OnClick="CreateNextProject"
                    Text="Save" runat="server" />
                <asp:Button ID="btnCancel" CausesValidation="false" Text="Cancel" runat="server" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>


