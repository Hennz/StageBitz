namespace("StageBitz.UserWeb.Common.Scripts.ItemTypes");

StageBitz.UserWeb.Common.Scripts.ItemTypes.Base = function () {
    this.IsReadOnly;
    this.CountryId = 0;
    this.StopProcessingBtn;
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Base.prototype.LoadDynamicFields = function (valueObject, element) {
    // Dynamic Data
    $(valueObject).each(function () {
        if (this.FieldOptionId) {
            $("*[data-field='" + this.FieldId + "']", element).val(this.FieldOptionId);
        }
        else {
            $("*[data-field='" + this.FieldId + "']", element).val(this.Value);
        }
    });
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Base.prototype.ReadFieldData = function (valuesObj, valuesChangedObj, element) {
    // Select All Dynamic fields
    var fieldElements = $("*[data-field]", element);

    $(fieldElements).each(function () {
        var fieldId = $(this).attr("data-field");
        var fieldValue = $(this).val();
        if (fieldId) {
            var isNew = true;
            $(valuesObj).each(function () {
                if (this.Id > 0) {
                    var changeField = {};
                    $.extend(true, changeField, this);
                    if (fieldId == this.FieldId) {
                        if (this.FieldOptionId) {
                            if (fieldValue != this.FieldOptionId) {
                                changeField.FieldOptionId = fieldValue;
                                valuesChangedObj.push(changeField);
                            }
                        }
                        else {
                            if (fieldValue != this.Value) {
                                changeField.Value = fieldValue;
                                valuesChangedObj.push(changeField);
                            }
                        }

                        isNew = false;
                    }
                }
            });

            if (isNew && fieldValue) {
                var newFiledValue;
                if ($(this).is("select")) {
                    newFiledValue = { FieldId: fieldId, Value: '', FieldOptionId: fieldValue, ItemValueId: 0, Id: 0 };
                }
                else {
                    newFiledValue = { FieldId: fieldId, Value: fieldValue, FieldOptionId: null, ItemValueId: 0, Id: 0 };
                }

                valuesChangedObj.push(newFiledValue);
            }
        }
    });
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Base.prototype.InitializeUI = function (fieldsHtml, fieldsElement, itemSpecElement, itemHeaderElement) {
    var self = this;
    fieldsElement.empty();
    fieldsElement.append(fieldsHtml);

    $('.remove_dynamic_field', fieldsElement).remove();

    $("div.jqxexpander", fieldsElement).accordion({
        collapsible: true,
        heightStyle: "content",
        active: false
    });

    $("input.datePicker").datepicker({
        changeMonth: true,
        changeYear: true,
        dateFormat: 'dd M yy'
    }).keydown(function (e) {
        if (!(e.which == 46 || e.which == 8)) {
            e.preventDefault();
        }
    });

    $("select.cultureBased[data-field] option", itemSpecElement).filter(function () {
        return $(this).attr('data-countryid') == self.CountryId;
    }).attr('selected', 'selected');

    var selectedOption = $("select.cultureBased[data-field] option[selected]", itemSpecElement);
    if (selectedOption.length == 0) {
        $("select.cultureBased[data-field] option[data-countryid = 0]", itemSpecElement).attr('selected', 'selected');
    }

    this.LoadUIData();

    // Make fields readonly
    if (this.IsReadOnly) {
        $(':input:not([hidden],:submit,:button)', itemSpecElement).attr('disabled', 'disabled');
        $(':not(:has(.ignoreSetReadOnly)) :input:not([hidden],:submit,:button)', itemHeaderElement).attr('disabled', 'disabled');
    }
    else {
        $(':input:not([hidden],:submit,:button)', itemSpecElement).removeAttr('disabled');
        $(':not(:has(.ignoreSetReadOnly)) :input:not([hidden],:submit,:button)', itemHeaderElement).removeAttr('disabled');
    }

    initializeDirtyValidationInputs(itemSpecElement);
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Base.prototype.LoadUIData = function () {
    // must override.
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Base.prototype.StopProcessing = function () {
    if (this.StopProcessingBtn) {
        this.StopProcessingBtn.click();
        this.HideOverlay();
    }
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Base.prototype.ProcessSaveResults = function (data, callBack) {
    if (data.Status == "OK") {
        if (callBack) {
            callBack();
        }
    }
    else if (data.Status == "NOTOK") {
    }
    else if (data.Status == "STOPPROCESS") {
        this.StopProcessing();
    }
    else if (data.Status == "CONCURRENCY") {
        if (data.ErrorCode && showErrorPopup(data.ErrorCode)) {
            this.HideOverlay();
        }
    }
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Base.prototype.ShowOverlay = function () {
};

StageBitz.UserWeb.Common.Scripts.ItemTypes.Base.prototype.HideOverlay = function () {
};
