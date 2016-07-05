<%@ Page DisplayTitle="System Settings" Language="C#" MasterPageFile="~/Content.master"
    AutoEventWireup="true" CodeBehind="SystemSettings.aspx.cs" Inherits="StageBitz.AdminWeb.System.SystemSettings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">
    <asp:UpdatePanel ID="upnlNotification" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <sb:PopupBox ID="popupNotification" Title="System Settings" runat="server">
                <BodyContent>
                    <div style="min-width: 250px; max-width: 400px; min-height: 30px; max-height: 400px;
                        overflow-y: auto;">
                        <asp:Literal ID="ltrlNotification" runat="server"></asp:Literal>
                    </div>
                </BodyContent>
                <BottomStripeContent>
                    <input type="button" value="OK" class="buttonStyle popupBoxCloser" />
                </BottomStripeContent>
            </sb:PopupBox>
        </ContentTemplate>
    </asp:UpdatePanel>
    <telerik:RadTabStrip ID="rtsSettings" Width="930" MultiPageID="settingsMultiPage"
        runat="server">
        <Tabs>
            <telerik:RadTab runat="server" Text="System Date" Value="SystemDate">
            </telerik:RadTab>
            <telerik:RadTab runat="server" Text="Advanced" Value="Advanced">
            </telerik:RadTab>
        </Tabs>
    </telerik:RadTabStrip>
    <div class="tabPage">
        <telerik:RadMultiPage ID="settingsMultiPage" runat="server">
            <telerik:RadPageView ID="systemDateTabPage" runat="server">
                <div style="width: 480px; float: left;">
                    <asp:UpdatePanel ID="upnlDateSettings" UpdateMode="Conditional" runat="server">
                        <ContentTemplate>
                            <asp:PlaceHolder ID="plcSystemDate" runat="server">System Date and Time:
                                <telerik:RadDateTimePicker ID="dtpkSystemDate" Width="200" runat="server">
                                </telerik:RadDateTimePicker>
                                <div style="color: Gray; margin-top: 5px;">
                                    (Leave blank to use the current date and time)</div>
                                <br style="clear: both;" />
                                <div style="height: 20px; margin-top: 50px;">
                                    <asp:Button ID="btnSave" OnClick="btnSave_Click" CssClass="buttonStyle" runat="server"
                                        Text="Save" />
                                    <div id="saveNotice" class="inlineNotification right" style="line-height: 30px;">
                                        Changes saved.</div>
                                </div>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="plcNoSettings" runat="server">No Settings to configure. </asp:PlaceHolder>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div style="width: 400px; float: right; margin-top: -10px;">
                    <asp:Timer ID="Timer1" runat="server" Interval="30000" OnTick="CheckForUpdate">
                    </asp:Timer>
                    <asp:UpdatePanel ID="upnlLastRunDates" UpdateMode="Conditional" runat="server">
                        <ContentTemplate>
                            <fieldset style="float: right; width: 360px;">
                                <legend>System Processes Last Run Dates</legend>
                                <div style="float: right;">                         
                                        <asp:Label ID="litStatus" Text="(Processing..)" Font-Size="Smaller" Visible="false" runat="server"></asp:Label>
                                    <asp:ImageButton ID="imgRefresh" Style="float: right;" ToolTip="Refresh" OnClick="RefreshClick"
                                        runat="server" ImageUrl="~/Common/Images/refreshIcon.gif" />
                                </div>
                                <div style="text-align: left; line-height: 30px;">
                                    <strong>Daily Process : </strong>
                                    <asp:Literal ID="ltrlDaily" runat="server"></asp:Literal>
                                    <br />
                                    <strong>Monthly Billing Process : </strong>
                                    <asp:Literal ID="ltrlMonthly" runat="server"></asp:Literal>
                                    <br />
                                    <br />
                                    <asp:Button ID="btnResetProcesses" CssClass="buttonStyle" ToolTip="Reset last run dates to a past date."
                                        OnClick="btnResetProcesses_Click" runat="server" Text="Reset Last Run Dates" />
                                </div>
                            </fieldset>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </telerik:RadPageView>
            <telerik:RadPageView ID="advancedTabPage" runat="server">
                <asp:UpdatePanel ID="upnlAdvancedSettings" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <table style="width: 100%;">
                            <tr id="trClearFinanceData" runat="server">
                                <td>
                                    <asp:Button ID="btnClearFinanceData" runat="server" OnClick="btnClearFinanceData_Click"
                                        CssClass="buttonStyle" Style="float: left; width: 95%;" OnClientClick="return confirm('Are you sure want to clear all financial data from the database? This cannot be undone.');"
                                        Text="Clear Financial Data" />
                                </td>
                                <td style="color: gray">
                                    Clears all data related to financial activities. Marks all projects as Free Trial
                                    Projects expiring in 6 weeks from now on.
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    &nbsp;
                                </td>
                                <td>
                                    &nbsp;
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 205px;">
                                    <asp:Button ID="btnClearRefDataCache" runat="server" OnClick="btnClearRefDataCache_Click"
                                        CssClass="buttonStyle" Style="float: left; width: 95%;" OnClientClick="return confirm('Are you sure want to clear the reference data cache?');"
                                        Text="Clear Referance Data Cache" />
                                </td>
                                <td style="color: gray">
                                    <strong>Developers Only.</strong> Clears system value and code value cache.
                                </td>
                            </tr>
                        </table>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </telerik:RadPageView>
        </telerik:RadMultiPage>
        <div style="clear: both;">
        </div>
    </div>
</asp:Content>
