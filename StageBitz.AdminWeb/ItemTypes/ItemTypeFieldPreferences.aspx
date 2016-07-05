<%@ Page DisplayTitle="Field Preferences" Language="C#" AutoEventWireup="true" CodeBehind="ItemTypeFieldPreferences.aspx.cs" MasterPageFile="~/Content.master"
    Inherits="StageBitz.AdminWeb.ItemTypes.ItemTypeFieldPreferences" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function ItemTypeFieldsValues() {
            this.ItemTypeId = 0;
            this.Fields = [];
        }

        function TestMethod() {
            showPopup('popupSavingChanges');
            var masterTable = $find("<%=gvItemTypeFieldPreference.ClientID%>").get_masterTableView();

            var dataItems = masterTable.get_dataItems();
            //var dataArray = new Array(dataItems.length);
            var associativeArray = new Array();
            var objectArray = new Array();


            for (var rowIndex = 0; rowIndex < dataItems.length; rowIndex++) {
                var cells = dataItems[rowIndex]._element.cells;
                //dataArray[rowIndex] = new Array(cells.length);
                for (var cellIndex = 4; cellIndex < cells.length; cellIndex++) {
                    var cellObj = $(cells[cellIndex]);
                    if (cellObj) {
                        var isSelected = $('input', cellObj).is(':checked');
                        if (isSelected) {
                            var fieldId = $('span', cellObj).attr('data-fieldid');
                            var itemTypeId = $('span', cellObj).attr('data-itemtypeid');

                            if (typeof associativeArray[itemTypeId] == 'undefined') {
                                associativeArray[itemTypeId] = new Array();
                                (associativeArray[itemTypeId]).push(fieldId);
                            }
                            else {
                                (associativeArray[itemTypeId]).push(fieldId);
                            }
                        }
                    }
                }
            }


            for (key in associativeArray) {
                var itemTypeFieldsValues = new ItemTypeFieldsValues();
                itemTypeFieldsValues.ItemTypeId = key;
                itemTypeFieldsValues.Fields = associativeArray[key]
                objectArray.push(itemTypeFieldsValues);

            }

            var stringifiedContent = JSON.stringify(objectArray);
            var param = '{"objectList" : ' + stringifiedContent + '}';
            //var x = associativeArray;
            $.ajax({
                type: 'POST',
                data: param,
                url: '<%=ResolveUrl("~/ItemTypes/ItemTypeFieldPreferences.aspx/SaveItemFieldPreferences")%>',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    hidePopup('popupSavingChanges');
                    showPopup('popupSavedChanges');
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert(xhr.status);
                    //alert(thrownError);
                },
            });
        }

        function SaveItemTypeFields() {


        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageNavigationLinks" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageContent" runat="server">

    <sb:SBRadGrid ID="gvItemTypeFieldPreference" runat="server" Width="940"
        HeaderStyle-VerticalAlign="Top" Height="600px" AutoGenerateColumns="false" OnItemDataBound="gvItemTypeFieldPreference_ItemDataBound"
        OnItemCreated="gvItemTypeFieldPreference_ItemCreated">
        <MasterTableView PageSize="50" TableLayout="Fixed" EnableColumnsViewState="false">
            <GroupByExpressions>
                <telerik:GridGroupByExpression>
                    <SelectFields>
                        <telerik:GridGroupByField FieldName="GroupName" />
                    </SelectFields>
                    <GroupByFields>
                        <telerik:GridGroupByField FieldName="GroupId" />
                    </GroupByFields>
                </telerik:GridGroupByExpression>
            </GroupByExpressions>
            <Columns>
                <telerik:GridBoundColumn DataField="ProjectUsingField" ItemStyle-HorizontalAlign="Center"
                    UniqueName="ProjectUsingField" HeaderText="Projects using field" HeaderStyle-Width="80">
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="ItemTypeUsingField" ItemStyle-HorizontalAlign="Center"
                    UniqueName="ItemTypeUsingField" HeaderText="Item Types using field" HeaderStyle-Width="80">
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="Field" UniqueName="Field" HeaderText="Field" HeaderStyle-Width="150">
                </telerik:GridBoundColumn>
            </Columns>
        </MasterTableView>
        <ClientSettings Resizing-AllowColumnResize="true" Scrolling-AllowScroll="true" Scrolling-FrozenColumnsCount="4" Scrolling-ScrollHeight="550" Scrolling-UseStaticHeaders="true" Scrolling-SaveScrollPosition="true"></ClientSettings>
    </sb:SBRadGrid>
    <br />
    <asp:Button runat="server" ID="btnSaveFieldPreferences" CssClass="buttonStyle" Text="Save Preferences" OnClientClick="TestMethod();return false;" />

    <sb:PopupBox ID="popupSavedChanges" Width="900" runat="server" Title="Changes Saved" ShowCornerCloseButton="false">
        <BodyContent>
            <div style="width:250px;">
                Your Field Preferences have been saved.
            </div>
        </BodyContent>
        <BottomStripeContent>
            <input type="button" class="buttonStyle popupBoxCloser" onclick="location.reload(true);" value="OK" />
        </BottomStripeContent>
    </sb:PopupBox>

    <sb:PopupBox ID="popupSavingChanges" Width="900" runat="server" Title="Saving Changes" ShowCornerCloseButton="false">
        <BodyContent>
            <div style="width:250px;">
                Saving Changes. Please Wait...
            </div>
        </BodyContent>
    </sb:PopupBox>

</asp:Content>
