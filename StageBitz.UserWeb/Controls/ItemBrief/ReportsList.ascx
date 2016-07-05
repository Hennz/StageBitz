<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportsList.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.ItemBrief.ReportsList" %>
<script type="text/javascript">

    $(document).bind('click', function (e) {
            var $clicked = $(e.target);
            if ($clicked.hasClass("reportDropDown")) {
                $(".reportsPopup").slideDown("fast");
            } else {
                $(".reportsPopup").slideUp(0);
            }
        });

        $(document).ready(function () {
            $(".reportsPopup").slideUp(0);
        });


</script>
<div id="divReportList"  runat="server" style="display:inline;">
    | <a href='#' class="reportDropDown" title="View Project Reports">Reports
        <img id="Img1" class="reportDropDown" src="~/Common/Images/downArrowSmall_blue.png" alt="report drop down"
            runat="server" />
    </a>
    <div id="divReportsPopup" runat="server" class="reportsPopup" style="display: none;">
        <asp:HyperLink ID="hyperLinkItemisedPurchaseReport" runat="server" Style="display: block;
            height: 20px;">Itemised Purchase Report</asp:HyperLink>
        <asp:HyperLink ID="hyperLinkBudgetSummary" runat="server" Style="display: block;
            height: 20px;">Budget Summary Report</asp:HyperLink>
    </div>
</div>
