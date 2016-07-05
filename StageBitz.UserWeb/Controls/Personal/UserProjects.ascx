<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserProjects.ascx.cs" Inherits="StageBitz.UserWeb.Controls.Personal.UserProjects" %>

<asp:UpdatePanel ID="upnlUserProjects" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
 
        <telerik:RadGrid ID="gvProjects" Width="100%" EnableLinqExpressions="False" AutoGenerateColumns="false"
            AllowSorting="true" OnItemDataBound="gvProjects_ItemDataBound" OnSortCommand="gvProjects_SortCommand"
            OnNeedDataSource="gvProjects_NeedDataSource" runat="server">
            <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
            <MasterTableView AllowNaturalSort="false" AllowMultiColumnSorting="true">
                <SortExpressions>
                    <telerik:GridSortExpression FieldName="Project.ProjectName" SortOrder="Ascending" />
                </SortExpressions>
                <Columns>
                    <telerik:GridTemplateColumn UniqueName="Project" HeaderText="Project" SortExpression="Project.ProjectName" ItemStyle-Width="150px"
                        HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkProject" runat="server"></asp:HyperLink>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn UniqueName="Company" HeaderText="Company" SortExpression="Company.CompanyName" ItemStyle-Width="150px"
                        HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkCompany" runat="server"></asp:HyperLink>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn UniqueName="Permission" HeaderText="Permission" SortExpression="PermissionType.SortOrder" ItemStyle-Width="150px"
                        HeaderStyle-Width="150px">
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn UniqueName="ProjectRole" HeaderText="Project Role" SortExpression="ProjectRole" ItemStyle-Width="150px"
                        HeaderStyle-Width="150px">
                    </telerik:GridTemplateColumn>
                </Columns>
            </MasterTableView>
        </telerik:RadGrid>

    </ContentTemplate>
</asp:UpdatePanel>
