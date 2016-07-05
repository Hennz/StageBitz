namespace("StageBitz.UserWeb.Common.Scripts.ItemTypes");

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief = function () {
    // Initialize Properties
    this.FieldsHtml;
    this.ItemBrief;
    this.ItemBriefId;
    this.UserId;

    this.FieldsElement;
    this.ItemBriefHeaderElement;
    this.ItemBriefSpecElement;
    this.BudgetElement;

    this.Culture;
    this.IsReadOnly;
    this.CanSeeBudgetSummary;

    this.ItemBriefValuesChanged = [];
    this.ItemBriefValues;

    // Main Object
    this.ItemBriefDetails;
    this.ItemBriefDetailsToBeSaved = {};
    this.ItemDetailsToBeSaved = {};
    this.ItemDetails = {};

    this.CompleteItemTabObj;
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype = new StageBitz.UserWeb.Common.Scripts.ItemTypes.Base();
StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype.constructor = StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief;

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype.LoadData = function (callBack, itemTypeId) {
    var self = this;
    this.FieldsElement.empty();
    if (!itemTypeId) {
        itemTypeId = null;
    }

    $.ajax({
        url: "../Services/ItemTypeService/GetItemBriefDetails",
        data: { UserId: self.UserId, ItemBriefId: self.ItemBriefId, ItemTypeId : itemTypeId },
        type: "POST",
        async: true,
        success: function (data) {
            self.ItemBriefDetails = data;
            self.FieldsHtml = data.DisplayMarkUp;
            self.ItemBrief = data.ItemBriefInfo;
            self.ItemBriefValues = data.ItemBriefValues;

            self.IsReadOnly = data.IsReadOnly;
            self.CanSeeBudgetSummary = data.CanSeeBudgetSummary;
            self.ItemDetails = data.ItemDetails;
            self.CountryId = data.CountryId;

            if (callBack) {
                callBack();
            }
        },
        error: function (xhr, status, error) {
        }
    });
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype.LoadUIData = function () {
    var self = this;

    // Load Item Brief Header Data
    focusEdit_SetValue(this.ItemBrief.Name, $("span[id$='itemBriefNameEdit']", this.ItemBriefHeaderElement));
    $("*[id$='ltrlItemID']", this.ItemBriefHeaderElement).html(this.ItemBrief.ItemBriefId);
    $("input[id$='txtItemQuantity']", this.ItemBriefHeaderElement).val(this.ItemBrief.Quantity);
    $("input[id$='dtpkDueDate']", this.ItemBriefHeaderElement).val(this.ItemBrief.DueDate);
    this.SetItemBriefStatus();
    $("span[id$='lblItemBriefId']", this.ItemBriefHeaderElement).html(this.ItemBrief.ItemBriefId);

    var ddItemTypes = $("select[id$='ddItemTypes']", this.ItemBriefHeaderElement);
    ddItemTypes.val(this.ItemBrief.ItemBriefItemTypeId);

    ddItemTypes.unbind('change.LoadDynamicFields');
    ddItemTypes.bind('change.LoadDynamicFields', function () {
        var ddlItemType = $(this);
        var itemTypeId = ddlItemType.val();
        self.LoadData(function () {
            self.InitializeUI(self.FieldsHtml, self.FieldsElement, self.ItemBriefSpecElement, self.ItemBriefHeaderElement);
            ddlItemType.val(itemTypeId);
        }, itemTypeId);
    });

    // Specifications section
    $("textarea[id$='txtDescription']", this.ItemBriefSpecElement).val(this.ItemBrief.Description);
    $("textarea[id$='txtBrief']", this.ItemBriefSpecElement).val(this.ItemBrief.Brief);
    $("textarea[id$='txtUsage']", this.ItemBriefSpecElement).val(this.ItemBrief.Usage);
    $("textarea[id$='txtConsiderations']", this.ItemBriefSpecElement).val(this.ItemBrief.Considerations);

    $("input[id$='txtAct']", this.ItemBriefSpecElement).val(this.ItemBrief.Act);
    $("input[id$='txtScene']", this.ItemBriefSpecElement).val(this.ItemBrief.Scene);
    $("input[id$='txtPage']", this.ItemBriefSpecElement).val(this.ItemBrief.Page);
    $("input[id$='txtCategory']", this.ItemBriefSpecElement).val(this.ItemBrief.Category);
    $("input[id$='txtCharacter']", this.ItemBriefSpecElement).val(this.ItemBrief.Character);
    $("input[id$='txtPreset']", this.ItemBriefSpecElement).val(this.ItemBrief.Preset);
    $("input[id$='txtApprover']", this.ItemBriefSpecElement).val(this.ItemBrief.Approver);
    $("input[id$='txtRehearsalItem']", this.ItemBriefSpecElement).val(this.ItemBrief.RehearsalItem);

    // Budget Section
    if (this.CanSeeBudgetSummary) {
        // $("input[id$='txtBudget']", this.BudgetElement).val(FormatCurrency(this.ItemBrief.Budget, this.Culture));
        RadNumericTextBox_Jquery_SetValue($("input[id$='txtBudget']", this.BudgetElement), this.ItemBrief.Budget);
    }

    this.LoadDynamicFields(this.ItemBriefValues, this.ItemBriefSpecElement);
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype.SetItemBriefStatus = function () {
    var element = $("span[id$='lblItemStatus']", this.ItemBriefHeaderElement);
    element.html(this.ItemBrief.StatusCodeDescription);
    switch (this.ItemBrief.StatusCodeValue) {
        case "COMPLETED":
            element.addClass("itemCompletedStatus");
            element.removeClass("itemInProgressStatus");
            break;
        case "INPROGRESS":
            element.removeClass("itemCompletedStatus");
            element.addClass("itemInProgressStatus");
            break;
        default:
            element.removeClass("itemInProgressStatus");
            element.removeClass("itemCompletedStatus");
            break;
    }
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype.ChangeBudgetUI = function (budget, remainingExpenses, expendedAmount) {
    this.ItemBrief.Budget = budget;
    this.InitializeBudgetUI(remainingExpenses, expendedAmount);

};

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype.InitializeBudgetUI = function (remainingExpenses, expendedAmount) {
    $("div[id$='divBalanceAmount']", this.BudgetElement).html(FormatCurrency((this.ItemBrief.Budget - (remainingExpenses + expendedAmount)), this.Culture));
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype.ReadItemBriefData = function () {
    // Load Item Brief Header Data
    this.ItemBrief.Name = focusEdit_GetValue($("span[id$='itemBriefNameEdit']", this.ItemBriefHeaderElement));
    this.ItemBrief.Quantity = $("input[id$='txtItemQuantity']", this.ItemBriefHeaderElement).val();
    this.ItemBrief.DueDate = $("input[id$='dtpkDueDate']", this.ItemBriefHeaderElement).val();
    this.ItemBrief.ItemBriefItemTypeId = $("select[id$='ddItemTypes']", this.ItemBriefHeaderElement).val();

    // Specifications section
    this.ItemBrief.Description = $("textarea[id$='txtDescription']", this.ItemBriefSpecElement).val();
    this.ItemBrief.Brief = $("textarea[id$='txtBrief']", this.ItemBriefSpecElement).val();
    this.ItemBrief.Usage = $("textarea[id$='txtUsage']", this.ItemBriefSpecElement).val();
    this.ItemBrief.Considerations = $("textarea[id$='txtConsiderations']", this.ItemBriefSpecElement).val();

    this.ItemBrief.Act = $("input[id$='txtAct']", this.ItemBriefSpecElement).val();
    this.ItemBrief.Scene = $("input[id$='txtScene']", this.ItemBriefSpecElement).val();
    this.ItemBrief.Page = $("input[id$='txtPage']", this.ItemBriefSpecElement).val();
    this.ItemBrief.Category = $("input[id$='txtCategory']", this.ItemBriefSpecElement).val();
    this.ItemBrief.Character = $("input[id$='txtCharacter']", this.ItemBriefSpecElement).val();
    this.ItemBrief.Preset = $("input[id$='txtPreset']", this.ItemBriefSpecElement).val();
    this.ItemBrief.Approver = $("input[id$='txtApprover']", this.ItemBriefSpecElement).val();
    this.ItemBrief.RehearsalItem = $("input[id$='txtRehearsalItem']", this.ItemBriefSpecElement).val();

    // Budget Section
    if (this.CanSeeBudgetSummary) {
        $find($("input[id$='txtBudget']", this.BudgetElement).attr('id')).get_value();
    }
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype.SaveData = function (callBack) {
    this.ItemBriefValuesChanged = [];
    this.ItemBriefDetailsToBeSaved = {};

    var self = this;
    this.ReadFieldData(this.ItemBriefValues, this.ItemBriefValuesChanged, this.ItemBriefSpecElement);
    this.ReadItemBriefData();

    $.extend(true, this.ItemBriefDetailsToBeSaved, this.ItemBriefDetails);
    this.ItemBriefDetailsToBeSaved.DisplayMarkUp = '';
    this.ItemBriefDetailsToBeSaved.ItemBriefValues = this.ItemBriefValuesChanged;
    this.ItemBriefDetailsToBeSaved.UserId = self.UserId;
    this.ItemBriefDetailsToBeSaved.ItemDetails = this.ItemDetailsToBeSaved;

    this.CallSaveDataService(callBack);
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.ItemBrief.prototype.CallSaveDataService = function (callBack) {
    var self = this;

    $.ajax({
        url: "../Services/ItemTypeService/SaveItemBriefDetails",
        data: self.ItemBriefDetailsToBeSaved,
        type: "POST",
        async: true,
        success: function (data) {
            self.ProcessSaveResults(data, callBack);
            if (self.CompleteItemTabObj && data.ItemResultObject) {
                self.CompleteItemTabObj.ProcessSaveResults(data.ItemResultObject);
            }
        },
        error: function (xhr, status, error) {
        }
    });
};

