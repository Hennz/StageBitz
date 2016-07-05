namespace("StageBitz.UserWeb.Common.Scripts.ItemTypes");

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item = function () {
    // Initialize Properties
    this.FieldsHtml;
    this.ItemId;
    this.ItemBriefId;
    this.UserId;
    this.FromDate;
    this.ToDate;
    this.HasDatefilteration;
    this.BookQty;
    this.DisplayMode;

    this.FieldsElement;
    this.ItemHeaderElement;
    this.ItemSpecElement;

    this.Culture;
    this.IsReadOnly;
    this.ItemValuesChanged = [];
    this.ItemValues;

    // Main Object
    this.ItemDetails;
    this.ItemDetailsToBeSaved = {};
    this.DocumentMediaIds;

    this.RelatedTable;
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype = new StageBitz.UserWeb.Common.Scripts.ItemTypes.Base();
StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.constructor = StageBitz.UserWeb.Common.Scripts.ItemTypes.Item;

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.DisplayMode = {
    ItemDetails: 0,
    ItemBriefDetails: 1
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.CanSaveQuantity = function () {
    return this.ItemDetails.IsEditableToAdminOnly;
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.LoadData = function (callBack, itemTypeId) {
    var self = this;
    this.FieldsElement.empty();
    if (!itemTypeId) {
        itemTypeId = null;
    }

    var param = { UserId: self.UserId, ItemId: self.ItemId, ItemBriefId: self.ItemBriefId, ItemTypeId: itemTypeId, FromDate: self.FromDate, ToDate: self.ToDate };

    $.ajax({
        url: "../Services/ItemTypeService/GetItemDetails",
        data: param,
        type: "POST",
        async: true,
        success: function (data) {
            self.ItemDetails = data;
            self.FieldsHtml = data.DisplayMarkUp;
            self.IsReadOnly = data.IsReadOnly;
            self.CountryId = data.CountryId;

            if (callBack) {
                callBack();
            }
        },
        error: function (xhr, status, error) {
        }
    });
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.PopulateProperties = function () {
    this.FieldsElement.empty();

    this.FieldsHtml = this.ItemDetails.DisplayMarkUp;
    this.ItemValues = this.ItemDetails.ItemValues;
    this.IsReadOnly = this.ItemDetails.IsReadOnly;
    this.ItemId = this.ItemDetails.ItemId;
    this.CountryId = this.ItemDetails.CountryId;
};


StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.LoadBookingInfo = function (availableQty) {

    $("div[id$='divAvailableQty']", this.ItemHeaderElement).html(availableQty);

    if (this.HasDatefilteration == "True")
        $("div[id$='divBookedQty']", this.ItemHeaderElement).show();
    else
        $("div[id$='divBookedQty']", this.ItemHeaderElement).hide();

    var bookedQty = $("input[id$='txtBookedQty']", this.ItemHeaderElement);
    if (bookedQty.length > 0) {
        var bookedQtyObj = bookedQty.get(0).id;
        //Set Min/Max values for Quantity Booked
        $find(bookedQtyObj).set_maxValue(availableQty == 0 ? 0 : availableQty);
        $find(bookedQtyObj).set_minValue(availableQty == 0 ? 0 : 1);
        $find(bookedQtyObj).set_value(availableQty == 0 ? 0 : this.BookQty);
    }
}

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.LoadUIData = function () {
    var self = this;

    // Load Item Header Data
    focusEdit_SetValue(this.ItemDetails.Name, $("span[id$='itemNameEdit']", this.ItemHeaderElement));

    RadNumericTextBox_Jquery_SetValue($("input[id$='txtItemQuantity']", this.ItemHeaderElement), this.ItemDetails.Quantity, !this.ItemDetails.IsEditableToAdminOnly);

    var lblItemStatus = $("span[id$='lblItemStatus']", this.ItemHeaderElement);
    if (lblItemStatus) {
        var trItemStatus = lblItemStatus.closest('tr');
        if (this.ItemDetails.ItemStatus) {
            trItemStatus.show();
            lblItemStatus.html(this.ItemDetails.ItemStatus);
        }
        else {
            trItemStatus.hide();
        }
    }

    self.LoadBookingInfo(this.ItemDetails.AvailableQty);
    //if (this.ItemDetails.AvailableQty == 0)
    //    bookedQty.val(0);
    //else
    //    bookedQty.val(self.BookQty);
    var inventoryLocationInput = $("*[id$='sbInventoryLocations']", this.ItemHeaderElement);
    InventoryLocations_Jquery_SetValue(inventoryLocationInput, this.ItemDetails.LocationId, !this.ItemDetails.IsEditableToAdminOnly);
    $("input[id$='txtCreatedFor']", this.ItemHeaderElement).val(this.ItemDetails.CreatedFor);
    $find($("input[id$='txtItemQuantity']", this.ItemHeaderElement).attr('id')).set_minValue(this.ItemDetails.MinQuantity);
    var ddItemTypes = $("select[id$='ddItemTypes']", this.ItemHeaderElement);
    ddItemTypes.val(this.ItemDetails.ItemTypeId);

    var lblItemType = $("span[id$='lblItemType']", this.ItemHeaderElement).text(this.ItemDetails.ItemTypeName);

    ddItemTypes.unbind('blur.LoadDynamicFields');
    ddItemTypes.bind('blur.LoadDynamicFields', function () {
        var ddlItemType = $(this);

        // Save only if there is change.
        if (ddlItemType.prop('data-changed')) {
            $(document).trigger('onSaveItemDetailsBeforeChangeItemType', [ddlItemType]);
            ddlItemType.removeProp('data-changed');
        }
    });

    ddItemTypes.unbind('change.LoadDynamicFields');
    ddItemTypes.bind('change.LoadDynamicFields', function () {
        // Add data-changed attribute if any change happend
        var ddlItemType = $(this);
        ddlItemType.prop('data-changed', 'changed');
    });

    // Specifications section
    if (this.DisplayMode == StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.DisplayMode.ItemDetails) {
        $("textarea[id$='txtDescription']", this.ItemSpecElement).val(this.ItemDetails.Description);

        // Set location manager
        var contactBookingManager = $("a[id$='lnkContactBookingManager']");
        contactBookingManager.text(this.ItemDetails.BookingManagerName);
        if (this.ItemDetails.BookingManagerName.length > 20) {
            TruncateString(contactBookingManager, 20);
        }

        contactBookingManager.attr('href', 'mailto:' + this.ItemDetails.BookingManagerEmail);
    }
    else if (this.DisplayMode == StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.DisplayMode.ItemBriefDetails) {
        $("textarea[id$='txtDescription']", this.ItemHeaderElement).val(this.ItemDetails.Description);
    }

    // Dynamic Data
    this.LoadDynamicFields(this.ItemValues, this.ItemSpecElement);
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.ReadItemData = function () {
    // Load Item Header Data
    this.ItemDetails.Name = focusEdit_GetValue($("span[id$='itemNameEdit']", this.ItemHeaderElement));
    this.ItemDetails.Quantity = $find($("input[id$='txtItemQuantity']", this.ItemHeaderElement).attr('id')).get_value();
    this.ItemDetails.LocationId = InventoryLocations_Jquery_GetValue($("*[id$='sbInventoryLocations']", this.ItemHeaderElement));
    this.ItemDetails.ItemTypeId = $("select[id$='ddItemTypes']", this.ItemHeaderElement).val();
    this.ItemDetails.CreatedFor = $("input[id$='txtCreatedFor']", this.ItemHeaderElement).val();

    // Specifications section
    if (this.DisplayMode == StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.DisplayMode.ItemDetails) {
        this.ItemDetails.Description = $("textarea[id$='txtDescription']", this.ItemSpecElement).val();
    }
    else if (this.DisplayMode == StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.DisplayMode.ItemBriefDetails) {
        this.ItemDetails.Description = $("textarea[id$='txtDescription']", this.ItemHeaderElement).val();
    }
}

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.PopulateSaveData = function () {
    this.ItemValuesChanged = [];
    this.ItemDetailsToBeSaved = {};

    var self = this;
    this.ReadFieldData(this.ItemValues, this.ItemValuesChanged, this.ItemSpecElement);
    this.ReadItemData();

    $.extend(true, this.ItemDetailsToBeSaved, this.ItemDetails);
    this.ItemDetailsToBeSaved.DisplayMarkUp = '';
    this.ItemDetailsToBeSaved.ItemValues = this.ItemValuesChanged;
    this.ItemDetailsToBeSaved.UserId = self.UserId;
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.CallSaveDataService = function (callBack) {
    var self = this;
    this.ItemDetailsToBeSaved.RelatedTable = this.RelatedTable;

    $.ajax({
        url: "../Services/ItemTypeService/SaveItemDetails",
        data: self.ItemDetailsToBeSaved,
        type: "POST",
        async: true,
        success: function (data) {
            self.ProcessSaveResults(data, callBack);
        },
        error: function (xhr, status, error) {
        }
    });
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Item.prototype.CompleteItem = function (callBack) {
    var self = this;
    this.ItemDetailsToBeSaved.RelatedTable = this.RelatedTable;

    $.ajax({
        url: "../Services/ItemTypeService/SaveAndCompleteItemDetails",
        data: self.ItemDetailsToBeSaved,
        type: "POST",
        async: true,
        success: function (data) {
            self.ProcessSaveResults(data, callBack);
        },
        error: function (xhr, status, error) {
        }
    });
};