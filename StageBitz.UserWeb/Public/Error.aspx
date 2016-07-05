<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="StageBitz.UserWeb.Public.Error" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavigationContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <h1>Uh oh!</h1>

    <asp:PlaceHolder ID="plcGeneric" runat="server">
        <p>
            It would appear that StageBitz has a temporary case of stagefright. Our engineers have been notified and will
            investigate the issue. In the meantime please <a id="A1" href="~/Default.aspx" runat="server">return to the home page</a>.
            Thanks for your patience.
        </p>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="plcConcurrency" Visible="false" runat="server">
        <p>
            The information you were working on has just been updated by another user.
            Please view their changes and then make your own as required.
        </p>
        <br />
        <p><a id="lnkPage" runat="server">View Updates</a></p>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="plcClosedProject" Visible="false" runat="server">
        <p>This Project is finished and has been closed.</p>
        <br />
        <p>This means you will not be able to add or edit any information to this Project and it will no longer appear on your <a id="A2" href="~/Default.aspx" runat="server">Personal Dashboard.</a></p>
        <br />
        <p>Company Administrators are able to access a read only version of the Project from the Company Dashboard. Please contact your Primary Company Administrator if you have any questions.</p>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="plcItemDeleted" Visible="false" runat="server">
        <p>
            This Item was deleted by 
            <asp:Literal ID="ltrItemDeletedUser" runat="server" />
            on
            <asp:Literal ID="ltrDeletedDate" runat="server" />.
        </p>
        <br />
        <p>Please contact <a id="lnkItemDeletedUserEmail" runat="server" />if you have any questions. </p>
        <br />
        <p><a id="lnkInventoryPage" runat="server">Go to Inventory</a></p>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="plcItemNotVisibile" Visible="false" runat="server">
        <p>
            Sorry! You don’t have permission to view the details for this item. If in doubt, check with your Booking Manager,
            <asp:HyperLink runat="server" ID="lnkInventoryAdminUserProfile"></asp:HyperLink>.
        </p>
        <br />
        <p>
            <a id="lnkInventoryPageItemNotVisible" runat="server">Go to Inventory</a>
            <a id="lnkGotoDashboard" href="~/Default.aspx" runat="server" visible="false">Go to Dashboard</a>
        </p>
    </asp:PlaceHolder>

</asp:Content>



