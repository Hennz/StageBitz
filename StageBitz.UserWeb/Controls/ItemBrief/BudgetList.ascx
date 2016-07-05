<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BudgetList.ascx.cs"
    Inherits="StageBitz.UserWeb.Controls.ItemBrief.BudgetList" %>
<div class="right boxBorder" style="width: 280px;">
    <div class="gridRow">
        <div class="left">
            <strong style="font-weight: bold;">Budget:</strong>
        </div>
        <div class="right" style="text-align: right;">            
            <asp:Literal ID="ltrlBudget" runat="server"></asp:Literal>
        </div>
    </div>
    <div class="gridAltRow">
        <div class="left">
            <strong>Expended:</strong>
        </div>
        <div class="right" style="text-align: right;">
            <asp:Literal ID="ltrlExpendedAmount" runat="server"></asp:Literal>
        </div>
    </div>
    <div class="gridRow">
        <div class="left">
            <strong>Remaining Expenses:</strong>
        </div>
        <div class="right" style="text-align: right;">
            <img runat="server" id="imgNoEstimatedCost" class="WarningIconForFinance" visible="false" src="~/Common/Images/NoExpendedCostWarning.png" />
            <asp:Literal ID="ltrlRemainingExpenses" runat="server"></asp:Literal>
        </div>
    </div>
    <div class="gridAltRow">
        <div class="left">
            <strong>BALANCE:</strong>
        </div>
        <div class="right" style="text-align: right;">
            <strong>
                <asp:Literal ID="ltrlBalanceAmount" runat="server"></asp:Literal></strong>
        </div>
    </div>
</div>
<div style="clear: both;">
</div>
