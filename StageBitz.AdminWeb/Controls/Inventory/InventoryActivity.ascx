<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryActivity.ascx.cs" Inherits="StageBitz.AdminWeb.Controls.Inventory.InventoryActivity" %>


<div>
    <asp:UpdatePanel runat="server" ID="upnlInventoryActivities" UpdateMode="Conditional">
        <ContentTemplate>
            <sb:GroupBox ID="GroupBox" runat="server">
                <TitleLeftContent>
                    Inventory Activity
                </TitleLeftContent>
                <TitleRightContent>
                    Display:
                <telerik:RadMonthYearPicker runat="server" ID="monthfilter" AutoPostBack="true" OnSelectedDateChanged="monthfilter_SelectedDateChanged"></telerik:RadMonthYearPicker>
                </TitleRightContent>
                <BodyContent>

                    <table class="right">
                        <tr class="rgRow">
                            <td style="padding: 5px;">Number of Times Accessed: </td>
                            <td style="width: 100px;">
                                <asp:Label runat="server" ID="lblInventoryPageViews"></asp:Label></td>
                        </tr>
                        <tr class="rgRow">
                            <td style="padding: 5px;">Number of Users Accessed: </td>
                            <td style="width: 100px;">
                                <asp:Label runat="server" ID="lblInventoryUserViews"></asp:Label></td>
                        </tr>
                    </table>
                    <br style="clear: both;" />
                    <br />

                    <sb:SBRadGrid ID="gvInventoryActivity" Width="100%" runat="server"
                        AllowAutomaticInserts="false" AllowAutomaticDeletes="false" AutoGenerateColumns="false">
                        <SortingSettings SortToolTip="Click to sort" SortedBackColor="Transparent" EnableSkinSortStyles="true" />
                        <MasterTableView>
                            <Columns>
                                <telerik:GridBoundColumn HeaderText="Item Type" HeaderStyle-Width="20%"
                                    UniqueName="ItemType" DataField="ItemType">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Quantity" HeaderStyle-Width="20%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center"
                                    UniqueName="Quantity" DataField="Quantity">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Manually Added" HeaderStyle-Width="20%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center"
                                    UniqueName="ManuallyAdded" DataField="ManuallyAdded">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Created In Project" HeaderStyle-Width="20%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center"
                                    UniqueName="CreatedInProject" DataField="CreatedInProject">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn HeaderText="Currently Booked" HeaderStyle-Width="20%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center"
                                    UniqueName="Booked" DataField="Booked">
                                </telerik:GridBoundColumn>
                            </Columns>
                        </MasterTableView>
                    </sb:SBRadGrid>
                </BodyContent>
            </sb:GroupBox>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
