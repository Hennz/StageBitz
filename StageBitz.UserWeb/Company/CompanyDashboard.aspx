<%@ Page Title="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="CompanyDashboard.aspx.cs" Inherits="StageBitz.UserWeb.Company.CompanyDashboard" %>

<%@ Register TagPrefix="uc" TagName="ProjectList" Src="~/Controls/Project/ProjectList.ascx" %>
<%@ Register TagPrefix="uc1" TagName="CompanyProjectSchedules" Src="~/Controls/Company/CompanyProjectSchedules.ascx" %>
<%@ Register TagPrefix="uc" TagName="ScheduleList" Src="~/Controls/Project/ScheduleList.ascx" %>
<%@ Register Src="~/Controls/Company/CompanyWarningDisplay.ascx" TagPrefix="uc" TagName="CompanyWarningDisplay" %>
<%@ Register Src="~/Controls/Project/CreateNewProjectLink.ascx" TagPrefix="sb" TagName="CreateNewProjectLink" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    <span runat="server" id="spanCreateNewProject">|<sb:CreateNewProjectLink runat="server" ID="lnkCreateNewProject" LinkText="Create New Project"/>
        |<a id="lnkExportFiles" runat="server">Export Files</a>
    </span>
    
   
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageTitleRight" runat="server">
    <sb:ImageDisplay ID="idCompanyLogo" Visible="false" runat="server" />
    <asp:Image ID="imgCompanies" ImageUrl="~/Common/Images/folder_Thumb.png" runat="server"
        Visible="false" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <uc:CompanyWarningDisplay runat="server" ID="sbCompanyWarningDisplay" />
    <p style="text-align: right;">
        <a id="linkCompanyDetails" runat="server">Company Details</a>
        <span runat="server" id="spanFinancialDetails">| <a id="lnkFinancialDetails" runat="server">Company Billing</a></span>   
    </p>
    <br />
    <uc:ScheduleList ID="scheduleList" runat="server" />
    <uc:ProjectList ID="companyProjectList" DisplayMode="CompanyDashboard" runat="server" />
    <div>
        <sb:GroupBox ID="GroupBox1" runat="server">
            <TitleLeftContent>
                Administration
            </TitleLeftContent>
            <BodyContent>
            
          <div class="left" style="margin-top: 10px; margin-left:10px; padding: 10px;">
              <fieldset class="fieldsetCD">
            <legend>Company Management</legend>
             <div style="text-align:right; padding-right:5px;">
                        <a id="lnkManageAdministrators" runat="server">Manage Company Team </a>
                    </div>
                    <div style="clear: both;">
                    </div>                 
             <div style="text-align:left;padding-right:5px;margin-left: 5px; margin-top:5px;">
                    <asp:Literal ID="ltNoOfAdministrators" runat="server"></asp:Literal>
              <asp:Literal ID="ltPrimaryAdmin" runat="server"></asp:Literal>
             </div>
                </fieldset>
          </div>
          <div class="left" style="margin-top: 10px; padding: 10px;">          
          <fieldset class="fieldsetCD">
                <legend>Company Inventory</legend>
                <div style="padding-top:2%;padding-left:10%;padding-right:10%;">
                               <asp:Literal ID="ltNoOfInventoryItems" runat="server"></asp:Literal> 
                         <br />     
                <div style="padding-right:100px;margin-top:10px;" class="buttonArea">
                <asp:Button ID="btnSearchInventory" OnClick="btnSearchInventory_Click" Text="Search Inventory" runat="server"/>
                </div>
                <br />         
                </div>      
               </fieldset>           
            </div>                             
               <div style="clear:both;"></div>
            </BodyContent>

        </sb:GroupBox>
    </div>
</asp:Content>
