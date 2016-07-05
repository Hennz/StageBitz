<%@ Page Title="" Language="C#" MasterPageFile="~/Content.master" AutoEventWireup="true"
    CodeBehind="ProjectNotifications.aspx.cs" Inherits="StageBitz.UserWeb.Project.ProjectNotifications" %>

<%@ Register Src="~/Controls/Project/ProjectUpdatesLink.ascx" TagName="ProjectUpdatesLink" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningDisplay.ascx" TagName="ProjectWarningDisplay" TagPrefix="sb" %>
<%@ Register Src="~/Controls/ItemBrief/ReportsList.ascx" TagName="ReportList" TagPrefix="sb" %>
<%@ Register Src="~/Controls/Project/ProjectWarningPopup.ascx" TagName="ProjectWarningPopup" TagPrefix="sb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        if ('<%= this.CompanyId %>' != '0' && '<%= this.ProjectID %>' != '0') {
            var _gaq = _gaq || [];
            _gaq.push(['_setCustomVar', 1, 'Category', 'Project', 2]);
            _gaq.push(['_setCustomVar', 2, 'CompanyId', '<%= this.CompanyId %>', 2]);
            _gaq.push(['_setCustomVar', 3, 'UserId', '<%= this.UserID %>', 2]);
            _gaq.push(['_setCustomVar', 4, 'ProjectId', '<%= this.ProjectID %>', 2]);
        }
    </script>

    <script id="NotificationTemplateNew" type="text/x-jsrender">
        <div class="{{:Style}}">
            <div class="content">
                <div class="divNotificationId" style="display: none;">
                    {{:NotificationId}}
                </div>
                <div style="float: left; width: 19px; height: 19px;">
                    {{if ImageURL}}
                        <img id="imgOperation" src="{{:ImageURL}}" />
                    {{/if}}
                </div>
                <div style="float: left; display: inline-block; text-align: left; margin-top: -2px;">
                    <a href="{{:ModuleHref}}"
                        id="linkModule" target="_blank" class="lightBlueText">{{:ModuleName}}</a>
                </div>
                <div style="clear: both; padding-left: 20px;">
                    {{:Message}} 
                </div>
                <div class="grayText" style="padding-left: 20px; padding-top: 5px; padding-bottom: 5px; font-size: 11px; border-bottom: 1px solid #E8E8E8;">
                    {{:EventDate}}
                </div>
            </div>
        </div>
    </script>
    <script type="text/javascript">
        //Initialize variables
        var prev = 0;
        var notificationIdLast = 0;
        var notificationIdPrev = 0;
        var showMyNotifications = false;
        var showMyNotificationsClicked = false;
        var styleChangeTimeOut = 5000; //Start changing background for new notifiacations after 5 secs
        var styleFadeoutTime = 3000; //Fadeout effect for 3 secs


        var projectId = 0;
        function OnShowMyNotificationsCheckedChanged() {
            $("#container").html("");
            $("#lnkLiveUpdate").hide();
            showMyNotificationsClicked = true;
            LoadNotifications();
        }

        function ShowNewNotifications() {
            $("#lnkLiveUpdate").hide();
            var params = '{"lastNotificationId":"-1","ShowMyNotifications":"' + showMyNotifications + '","GetLatestNotificationsOnly":"' + true + '","IsScroll":"' + false + '","showMyNotificationsClicked":"' + showMyNotificationsClicked + '","projectId":"' + projectId + '"}';

            $.ajax({
                type: "POST",
                url: '<%=ResolveUrl("~/Project/ProjectNotifications.aspx/GetNotificationData") %>', //  "Project/ProjectNotifications.aspx/GetDate",
                data: params,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    if (msg.d[0] != null) {
                        var htmlString = $("#NotificationTemplateNew").render(msg.d);
                        $('#container .content:first').parent('div').before(htmlString);
                        $("#divNotifications").scrollTop(0);
                        setTimeout(function () {
                            $(".NotificationBlockWithBackground").animate({ backgroundColor: '#f2f0f4' }, styleFadeoutTime);
                        }, styleChangeTimeOut);

                        //$(".NotificationBlockWithBackground").animate({ backgroundColor: 'white' }, 7000)
                    }
                    else {
                        $("#Message").show();
                        $("#Message").text("Woohoo! You've read all the important notifications.");
                    }
                }
            });
        }

        function LoadNotifications() {
            var checkbox1 = $("#<%= chkIncludeMyNotifications.ClientID %>");
            var domCheckbox1 = checkbox1[0];
            showMyNotifications = domCheckbox1.checked;
            var initialParams = '{"lastNotificationId":"-1","ShowMyNotifications":"' + showMyNotifications + '","GetLatestNotificationsOnly":"' + false + '","IsScroll":"' + false + '","showMyNotificationsClicked":"' + showMyNotificationsClicked + '","projectId":"' + projectId + '"}';
            //Calls the webmethod via a JSON request and get JSON data array.
            //Generate the html needed using JSRender engine and inject to the container div.
            $("#Message").hide();
            $.ajax({
                type: "POST",
                url: '<%=ResolveUrl("~/Project/ProjectNotifications.aspx/GetNotificationData") %>',
                data: initialParams,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    //Data access successfully. 
                    //var all = { data: msg };                     
                    if (msg.d[0] != null) {
                        notificationIdLast = msg.d[msg.d.length - 1]["NotificationId"];
                        //$("#Message").text("Last NotificationId" + notificationIdLast);
                        var htmlString = $("#NotificationTemplateNew").render(msg.d);
                        //Inject the generated HTML to the container DIV
                        $("#container").html(htmlString);
                        $("#divNoData").hide();
                        setTimeout(function () {
                            $(".NotificationBlockWithBackground").animate({ backgroundColor: '#f2f0f4' }, styleFadeoutTime);
                        }, styleChangeTimeOut);
                        //$(".NotificationBlockWithBackground").animate({ backgroundColor: 'white' }, 7000)
                    }
                    else {
                        $("#divNoData").show();
                    }
                }
            });

            (function PollForNotificationCount() {
                setTimeout(function () {
                    var paramsToSend = '{"ShowMyNotifications":"' + showMyNotifications + '","projectId":"' + projectId + '"}';
                    $.ajax({
                        type: "POST",
                        data: paramsToSend,
                        url: '<%=ResolveUrl("~/Project/ProjectNotifications.aspx/GetNewNotificationCount") %>',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        async: "true",
                        success: function (response) {
                            if (response.d == 0) {
                                $("#lnkLiveUpdate").hide();
                            }
                            else if (response.d == 1) {
                                //$("#lnkLiveUpdate").show();
                                $("#lnkLiveUpdate").fadeIn('slow');
                                $("#lnkLiveUpdate").text('Click to see ' + response.d + ' new notification');
                            }
                            else if (response.d > 1) {
                                //$("#lnkLiveUpdate").show();                                
                                $("#lnkLiveUpdate").fadeIn('slow');
                                $("#lnkLiveUpdate").text('Click to see ' + response.d + ' new notifications');
                            }
                            PollForNotificationCount();
                        },
                        error: function () {
                            PollForNotificationCount();
                        }
                    });
                }, 5000);
            })();
        }

        $(document).ready(

        //DIV showing the message "Loading..." is hidden initially
        //The message will be shown when records are fetched with AJAX 
        //when user has scrolled to the bottom of the DIV scrollbar 
         function () {


             projectId = GetUrlVarsArray()["projectid"];
             //Hide the progress div on initiation
             $("#divProgress").hide();
             $("#divNoData").hide();
             $("#Message").hide();
             $("#lnkLiveUpdate").hide();
             LoadNotifications();

             // Read a page's GET URL variables and return them as an associative array.
             function GetUrlVarsArray() {
                 var vars = [], hash;
                 var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
                 for (var i = 0; i < hashes.length; i++) {
                     hash = hashes[i].split('=');
                     vars.push(hash[0]);
                     vars[hash[0]] = hash[1];
                 }
                 return vars;
             }
             //Function triggres at scrolling
             $("#divNotifications").scroll(function () {
                 //triggering point is when the difference of the heights of the TABLE 
                 //and DIV match the DIV's scrollTop value
                 if ($("#container").height() - this.scrollTop < $("#divNotifications").height()) {

                     $("#divProgress").show();
                     //get last notification Id to track next fetch
                     notificationIdLast = $('#container .content:last').children(".divNotificationId").html();

                     var params = '{"lastNotificationId":"' + notificationIdLast + '","ShowMyNotifications":"' + showMyNotifications + '","GetLatestNotificationsOnly":"' + false + '","IsScroll":"' + true + '","showMyNotificationsClicked":"' + showMyNotificationsClicked + '","projectId":"' + projectId + '"}';
                     $.ajax({
                         type: "POST",
                         url: '<%=ResolveUrl("~/Project/ProjectNotifications.aspx/GetNotificationData") %>',
                         data: params,
                         contentType: "application/json; charset=utf-8",
                         dataType: "json",
                         success: function (msg) {
                             notificationIdPrev = notificationIdLast;
                             //Access data                                                          
                             if (msg.d[0] != null) {
                                 notificationIdLast = msg.d[msg.d.length - 1]["NotificationId"];

                                 if (notificationIdLast < notificationIdPrev) {
                                     var htmlString = $("#NotificationTemplateNew").render(msg.d);
                                     $('#container .content:last').parent('div').after(htmlString);
                                 }
                             }
                             else {
                                 $("#Message").show();
                                 $("#Message").text("Woohoo! You've read all the important notifications.");
                             }
                         }
                     });
                     showMyNotificationsClicked = false;
                     $("#divProgress").hide();
                 }
             });
         });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
    | <a id="lnkCompanyInventory" runat="server">Company Inventory</a>
    |<a id="lnkBookings" runat="server">Bookings</a>
    |<a id="linkTaskManager" runat="server">Task Manager</a>
    |<sb:ProjectUpdatesLink ID="projectUpdatesLink" CssClass="highlight" runat="server" />
    <sb:ReportList ID="reportList" LeftMargin="365" runat="server" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageTitleRight" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageContent" runat="server">

    <sb:ProjectWarningDisplay ID="warningDisplay" runat="server" />
    <sb:ProjectWarningPopup ID="projectWarningPopup" runat="server"></sb:ProjectWarningPopup>

    <div style="height: 25px;">
        <div style="float: left; border: 0px Solid black; margin-top: 5px; width: 60%; text-align: right;"
            id="divLiveUpdate">
            <a id="lnkLiveUpdate" class="NotificationCountBox" href="javascript:ShowNewNotifications();"></a>
        </div>
        <div style="float: right; text-align: right; padding-top: 5px; display: inline-block; border: 0px Solid black; width: 25%;">
            <asp:CheckBox ID="chkIncludeMyNotifications" onclick="OnShowMyNotificationsCheckedChanged()"
                Text="Include my changes" runat="server" />
        </div>
    </div>
    <div style="height: 10px; clear: both; margin-bottom: 10px; border-bottom: 1px solid #E8E8E8;">
    </div>
    <div id="divNotifications" style="clear: both; overflow: auto; height: 480px;">
        <div id="container">
        </div>
        <div id="divNoData" class="noData">
            You don't have any notifications
        </div>
    </div>
    <div style="height: 10px; clear: both; border-bottom: 1px solid #E8E8E8;">
    </div>
    <div id="divProgress" class="noData">
        Loading....
    </div>
    <div style="height: 10px;">
        <div id="Message" style="text-align: center; color: #C0C3C6; padding-top: 3px;">
        </div>
    </div>
    <div class="buttonArea" style="text-align: right; display: inline-block; float: right;">
        <asp:Button ID="btnDone" OnClick="Redirect" runat="server" Text="Done" />
    </div>
</asp:Content>
