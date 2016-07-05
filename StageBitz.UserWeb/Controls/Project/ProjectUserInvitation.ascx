<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectUserInvitation.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Project.ProjectUserInvitation" %>
<script type="text/javascript">
    function PermissionSelection(radioProjectAdmin, radioProjectStaff, radioProjectObserver, chkBudgetSummary, isAdmin) {
        var radioProjectAdminInput = $("#" + radioProjectAdmin);
        var radioProjectStaffInput = $("#" + radioProjectStaff);
        var radioProjectObserverInput = $("#" + radioProjectObserver);
        var chkBudgetSummaryInput = $("#" + chkBudgetSummary);

        if (radioProjectAdminInput.is(':checked') && chkBudgetSummaryInput.is(':checked')) {
            chkBudgetSummaryInput.attr('disabled', 'disabled');
        }

        if (!isAdmin) {
            radioProjectAdminInput.change(function () {
                if (radioProjectAdminInput.is(':checked')) {
                    chkBudgetSummaryInput.get(0).checked = true;
                    chkBudgetSummaryInput.attr('disabled', 'disabled');
                }
            });

            radioProjectStaffInput.change(function () {
                if (radioProjectStaffInput.is(':checked')) {
                    chkBudgetSummaryInput.removeAttr('disabled');
                }
            });

            radioProjectObserverInput.change(function () {
                if (radioProjectObserverInput.is(':checked')) {
                    chkBudgetSummaryInput.removeAttr('disabled');
                }
            });
        }
    }
</script>
<div>
    <asp:Literal ID="ltrlProjectTeamInvitationText" runat="server"></asp:Literal>
    <div style="display: inline-block;">
        <sb:HelpTip ID="HelpTip1" Width="300" runat="server">
            <p>
                Staff members can edit information and Observers can read only.
            </p>
            <br />
            <p>
                Either way you can choose who can view Budget Information.
            </p>
        </sb:HelpTip>
    </div>

    <table style="margin-top: 10px;">
        <tr id="trProjInviteName" runat="server">
            <td style="vertical-align: top; padding-top: 5px; width:80px;">Name:
            </td>
            <td>
                <asp:TextBox ID="txtProjInvitePersonName" Width="170" MaxLength="100" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="reqProjInvitePersonName" ControlToValidate="txtProjInvitePersonName" SkinID="Hidden"
                    runat="server" ErrorMessage="Please enter the new user's name so we can address them properly in the email." Display="Dynamic"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td style="vertical-align: top; padding-top: 5px; width:80px;">Role:
            </td>
            <td>
                <asp:TextBox ID="txtProjectRole" Width="170" MaxLength="100" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="reqProjectRole" ControlToValidate="txtProjectRole" SkinID="Hidden"
                    runat="server" ErrorMessage="Please enter Project Role."></asp:RequiredFieldValidator>
            </td>
        </tr>
    </table>
    <table>
        <tr>
            <td style="width: 80px;">Permission:
            </td>
            <td style="width: 150px;" runat="server" id="columnProjectAdminHeader">Project Administrator
            </td>
            <td style="width: 60px;">Staff
            </td>
            <td style="width: 80px;">Observer</td>
            <td style="width: 150px;">See Budget Information</td>
        </tr>
        <tr>
            <td style="width: 80px;"></td>
            <td runat="server" id="columnProjectAdmin" style="padding-left:40px;">
                <asp:RadioButton ID="radioProjectAdmin" GroupName="ProjectPermission"
                 runat="server" />
            </td>
            <td>
                  &nbsp;<asp:RadioButton ID="radioProjectStaff" GroupName="ProjectPermission"
                    runat="server" />
            </td>
            <td>
              &nbsp;&nbsp; <asp:RadioButton ID="radioProjectObserver" GroupName="ProjectPermission" Checked="true"
                    runat="server" />
            </td>
            <td style="padding-left:55px;">
              
                <asp:CheckBox ID="chkBudgetSummary" runat="server" />
            </td>
        </tr>
        <tr>
            <td colspan="4">
                <asp:ValidationSummary ID="valsumProjTeamInvitation" ShowSummary="true"
                    DisplayMode="List" CssClass="message error" runat="server"/>
            </td>
        </tr>
    </table>

</div>

