namespace("StageBitz.UserWeb.Common.Scripts.BookingDetails");

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader = function () {
    this.BookingId;
    this.ItemTypeId;
    this.UserId;
    this.CompanyId;
    this.MonthToConsider;
    this.BookingHeader;
    this.BookingDetails;
    this.GridTable;
    this.HeaderUIElement;
    this.UIElement;
    this.GridView;
    this.ButtonConfirm;
    this.DatePicker;
    this.IsToDateEdit;
    this.popupConflictErrorID;
    this.ToDay;
    this.IsFreshLoad;
    this.IsHeaderClick;
    this.IsInventoryManager;
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.LoadData = function () {

    this.ShowOverlay();
    this.ButtonConfirm.attr("disabled", "disabled");
    var self = this;
    self.IsFreshLoad = true;
    $.ajax({
        url: "../Services/InventoryService/GetBookingDetails",
        data: { UserId: this.UserId, BookingId: this.BookingId, ItemTypeId: this.ItemTypeId, CompanyId: this.CompanyId, IsToDateEdit: this.IsToDateEdit, IsInventoryManager: this.IsInventoryManager },
        type: "POST",
        async: true,
        success: function (data) {
            self.BookingHeader = data;
            self.BookingDetails = data.BookingDetailToEditList;
            self.LoadHeaderUI();
            if (data.BookingDetailToEditList.length > 0) {
                var currentDate = ParseDate(data.DataDisplayStartDate);
                // var currentDate1 = new Date(data.DataDisplayStartDate);                
                self.DatePicker.set_selectedDate(currentDate);

            }
            else {
                self.HandleLoadData();
                self.HideOverlay(false);
            }
        },
        error: function (xhr, status, error) {
            self.HideOverlay(false);
        }
    });
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.SetNewDate = function () {
    if (!this.IsFreshLoad) {
        this.ClearHeaderUI();
    }
    
    this.LoadUI(true);
    this.HideOverlay(!this.IsFreshLoad);
    this.IsFreshLoad = false;
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.LoadHeaderUI = function () {
    var self = this;
    $("span[id$='lblBookingName']", this.HeaderUIElement).text(self.BookingHeader.BookingName);
    $("span[id$='lblBookingNumber']", this.HeaderUIElement).text(self.BookingHeader.BookingNumber);
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.ClearHeaderUI = function () {
    this.GridTable.closest('table').find('th').each(function () {
        var col = this;
        var headerChkBox;
        if ($.isNumeric(col.innerText)) {
            headerChkBox = $(this).find('input[id$="hchkBookingDetail' + $.trim(col.innerText) + '"]');

            headerChkBox.prop("disabled", false);
            headerChkBox.prop("checked", false);

            $(this).find('input[type = hidden]').val('');
        }
    });
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.HandleLoadData = function () {
    if (this.BookingDetails.length > 0) {
        this.GridView.set_dataSource(this.BookingDetails);
        this.GridView.dataBind();
    }
    else {
        var divArea = this.UIElement.find('div[id=DataArea]');
        divArea.hide();
        var divNoData = this.UIElement.find('div[id=divNoData]');
        divNoData.show();
    }
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.ConfigureColumns = function (currentPeriod) {

    var numberOfDaysInMonth = new Date(currentPeriod.getFullYear(), currentPeriod.getMonth() + 1, 0).getDate();
    var numb = 31;
    var nonNumericColumnCount = 4;//This include Hidden ItemBookingId, ItemBriefName and ItemName columns

    if (this.CompanyId > 0)
        nonNumericColumnCount = 3;//Since ItemBriefName column is hidden (Access from Project pages)

    var table = this.GridTable.closest('table');


    var columnCount = this.GridView.get_columns().length;
    //Redisplay all the CheckBox columns starting from 1        
    var count = columnCount;
    while (count > nonNumericColumnCount) {
        this.GridView.showColumn(count-1);
        count--;
    }

    
    var diff = (columnCount - nonNumericColumnCount) - numberOfDaysInMonth;
    while (diff > 0) {
        this.GridView.hideColumn(columnCount - diff);
        diff--;
    }

    
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.LoadUI = function (shouldConfigureColumns) {

    var self = this;
    this.HandleLoadData();
    var columnIndex = 0;
    var dateValue = 1;
    var index = 0;
    var isNotAvailable;
    var isNotEditable;
    var isToDatePassed;
    var isFromDatePassed;
    var currentPeriod = new Date(this.DatePicker.get_selectedDate());

    if (shouldConfigureColumns)
        this.ConfigureColumns(currentPeriod);

    var toDayParaObj = ParseDate(self.ToDay);
    var toDayPara = new Date(toDayParaObj.getFullYear(), toDayParaObj.getMonth(), toDayParaObj.getDate());
    $.each(self.BookingDetails, function () {
        var bookingDetail = this;
        var itemBookingId = bookingDetail.ItemBookingId;

        //Get the tr which has the hidden ItemBookingId
        var parentRow = $('td:contains("' + itemBookingId + '")', self.GridTable).parent("tr");

        var frmDatetmp = ParseDate(bookingDetail.FromDate);//First convert to a javascript Date object. Then we ned to drop the time component
        var fromDate = new Date(frmDatetmp.getFullYear(), frmDatetmp.getMonth(), frmDatetmp.getDate()).getTime();

        var toDatetmp = ParseDate(bookingDetail.ToDate);
        var toDate = new Date(toDatetmp.getFullYear(), toDatetmp.getMonth(), toDatetmp.getDate()).getTime();

        var currentDate = new Date(currentPeriod.getFullYear(), currentPeriod.getMonth(), dateValue);//need to increase it. Therefore not going call getTime().
        var currentDateTime = currentDate.getTime();

        //Read the Margin Dates
        var leftMarginDate = new Date(ParseDate(bookingDetail.LeftMarginDate)).getTime();
        var rightMarginDate = new Date(ParseDate(bookingDetail.RightMarginDate)).getTime();

        $(parentRow).find('td').each(function () {
            var currenttd = this;
            $(this).removeClass('EditBookingDetailDisabledChkBox');
            $(this).removeAttr('title');
            var th = self.GridTable.closest('table').find('th').eq(columnIndex);
            //reset the value
            var hchck = th.find('input[type = checkbox]');
            if (hchck.length > 0) {
                index++;
                //Insert a Checkbox
                var checkbox = document.createElement('input');
                checkbox.type = "checkbox";
                checkbox.value = dateValue;
                checkbox.id = "chkGridDetail" + index;
                $(checkbox).change(function () {                    
                    self.ButtonConfirm.removeAttr("disabled");
                    var selectedDate = self.DatePicker.get_selectedDate();
                    self.ClearHeaderUI();
                    
                    var newDate = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), $(checkbox).is(':checked') ? this.value : self.IsToDateEdit ? parseInt(this.value) - 1 : parseInt(this.value) + 1);

                    //Get the hidden ItemBookingId and update the local DS
                    var itemBookingId = $('td:first', $(this).closest('tr')).text();
                    $.each(self.BookingDetails, function () {
                        if (this.ItemBookingId == itemBookingId) {
                            if (self.IsToDateEdit) {
                                this.ToDate = newDate;
                            }
                            else {
                                this.FromDate = newDate;
                            }
                            this.IsLocallyEdited = true;

                            self.LoadUI(false);
                            return false;//brake
                        }
                    });
                });


                var hiddenHasInactiveChkBoxInColumCtrl = th.find('input[type = hidden]');

                if (fromDate <= currentDateTime && currentDateTime <= toDate) {
                    checkbox.setAttribute('checked', true);
                    $(checkbox).prop('checked', true);
                    if (!hchck.is(':disabled') && (hiddenHasInactiveChkBoxInColumCtrl.val() != 'true' || self.IsHeaderClick))
                        hchck.prop('checked', true);
                }
                else {
                    hchck.prop("checked", false);
                    hiddenHasInactiveChkBoxInColumCtrl.val('true');
                }

                if (self.IsToDateEdit) { //Either this can be disabled or enabled

                    //Then Dates above "RightMargin" or below "Current From" ,"ToDay" needs to be disabled.
                    isNotAvailable = (bookingDetail.RightMarginDate != null && currentDateTime >= rightMarginDate);
                    isNotEditable =  currentDateTime < toDayPara.getTime();
                    isFromDatePassed = currentDateTime <= fromDate;
                    if (isNotAvailable || isNotEditable || isFromDatePassed) {
                        checkbox.setAttribute("disabled", true);
                        $(this).addClass('EditBookingDetailDisabledChkBox');
                        if (isNotAvailable)
                            $(this).attr('title', 'This Item is unavailable on this date.');
                        else {
                            if (isFromDatePassed) {
                                $(this).attr('title', 'Please select a date after the From date.');
                            }
                            else {
                            }
                            $(this).attr('title', 'Please select a future date.');
                        }
                        //if disabled, then the header should also needs to be disabled
                        hchck.prop("disabled", true);
                        hchck.prop("checked", false);
                    }
                }
                else {
                    isNotAvailable = bookingDetail.LeftMarginDate != null && currentDateTime <= leftMarginDate;
                    isNotEditable = currentDateTime < toDayPara.getTime();
                    isToDatePassed = currentDateTime >= toDate;
                    if (isNotAvailable || isNotEditable || isToDatePassed) {
                        //Then Dates below "LeftMargin" , "ToDay" needs to be disabled.
                        checkbox.setAttribute("disabled", true);
                        $(this).addClass('EditBookingDetailDisabledChkBox');
                        if (isNotAvailable)
                            $(this).attr('title', 'This Item is unavailable on this date.');
                        else {
                            if (isToDatePassed)
                                $(this).attr('title', 'Please select a date before the To date.');
                            else
                                $(this).attr('title', 'Please select a future date.');
                        }

                        //if disabled, then the header should also needs to be disabled
                        hchck.prop("disabled", true);
                        hchck.prop("checked", false);
                    }
                }

                currentDate.setDate(currentDate.getDate() + 1);
                currentDateTime = currentDate.getTime();
                this.appendChild(checkbox);
                dateValue++;
            }
            else if (columnIndex != 0) { //Except the hidden id
                //Truncate the name, if exceed max number of characters and display a tooltip
                if ($(currenttd).text().length > 11) {
                    TruncateString($(currenttd), 11);
                }

                if (th.hasClass("DateClass")) {
                    if (self.IsToDateEdit) {
                        $(currenttd).text(FormatDate(ParseDate(bookingDetail.ToDate)));
                    }
                    else {
                        $(currenttd).text(FormatDate(ParseDate(bookingDetail.FromDate)));
                    }
                }

                if (bookingDetail.HasError) {
                    //Apply the error class if contains any errors
                    var thForNext = self.GridTable.closest('table').find('th').eq($(this).next('td').index());
                    if ($.isNumeric(thForNext.text())) {
                        //if it is the ItemName column display an error icon at the right most
                        var elem = document.createElement("img");
                        elem.setAttribute("src", "../Common/Images/error.png");
                        elem.setAttribute("height", "14");
                        elem.setAttribute("width", "14");
                        elem.setAttribute("alt", "error");
                        elem.setAttribute("title", "This Item has had a new Booking by someone else. Its availability has been updated.");
                        elem.style.cssFloat = 'right';
                        this.appendChild(elem);
                    }
                }
            }
            columnIndex++;
        });
        columnIndex = 0;
        dateValue = 1;
    });

    initializeDirtyValidationInputs($(document));
    //this.HideOverlay();
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.ShowOverlay = function () {
    $('#itemtypeOverlay', this.UIElement).show();
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.HideOverlay = function (showDelay) {
    if(showDelay)
        $('#itemtypeOverlay', this.UIElement).delay(1000).hide(0);
    else
        $('#itemtypeOverlay', this.UIElement).hide();
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.SaveChanges = function () {
    this.ShowOverlay();
    var self = this;
    var editedList = [];
    //iterate the DataSource and get edited records
    $.each(this.BookingDetails, function () {
        if (this.IsLocallyEdited == true) {
            if (self.IsToDateEdit)
                editedList.push({ ItemBookingId: this.ItemBookingId, NewDate: this.ToDate.toDateString() });
            else {
                editedList.push({ ItemBookingId: this.ItemBookingId, NewDate: this.FromDate.toDateString() });
            }
        }
    });

    //Now call the service method
    var param = { UserId: this.UserId, IsToDateEdit: this.IsToDateEdit, IsInventoryManager: this.IsInventoryManager, ItemTypeId: this.ItemTypeId, BookingId: this.BookingId, CompanyId: this.CompanyId, BookingDetailsEditedList: editedList };

    $.ajax({
        type: "POST",
        url: "../Services/InventoryService/SaveBookingDetails",
        data: JSON.stringify(param),
        async: true,
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (savedResult) {
            if (savedResult.Status == "OK") {
                if (savedResult.HasBookingLinesError) {
                    //Display the Error popup
                    self.HideOverlay(false);
                    self.popupConflictErrorID.show();
                    self.popupConflictErrorID.find('input[id$="btnOKpopupConflictError"]').bind("click", function () {
                        self.popupConflictErrorID.hide();
                        self.ReloadDataAfterSave(self, savedResult)
                    });
                }
                else {
                    self.ReloadDataAfterSave(self, savedResult);
                }

            }
            else {
                //display error popup

            }
        }
    });
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.ReloadDataAfterSave = function (self, savedResult) {

    self.BookingHeader = savedResult.BookingEdit;
    self.BookingDetails = savedResult.BookingEdit.BookingDetailToEditList;
    self.LoadHeaderUI();
    self.LoadUI(true);
    self.ButtonConfirm.attr("disabled", "disabled");
    showNotification('BookingDetailsSavedNotice');
    setGlobalDirty(false);
    self.HideOverlay(false);
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.NavigatePrevious = function () {
    this.ShowOverlay();
    var selectedDate = this.DatePicker.get_selectedDate();
    selectedDate.setMonth(selectedDate.getMonth() - 1);
    this.DatePicker.set_selectedDate(selectedDate);    
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.NavigateNext = function () {
    this.ShowOverlay();
    var selectedDate = this.DatePicker.get_selectedDate();
    selectedDate.setMonth(selectedDate.getMonth() + 1);
    this.DatePicker.set_selectedDate(selectedDate);
};

StageBitz.UserWeb.Common.Scripts.BookingDetails.BookingHeader.prototype.HeaderDateClicked = function (date, isChecked) {
    //Update the Global DS
    //Build the Date
    var self = this;
    this.IsHeaderClick = true;
    self.ButtonConfirm.removeAttr("disabled");
    var selectedDate = this.DatePicker.get_selectedDate();
    var toDayParaObj = ParseDate(self.ToDay);
    var toDayPara = new Date(toDayParaObj.getFullYear(), toDayParaObj.getMonth(), toDayParaObj.getDate()).getTime();
    var newDate = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), isChecked ? date : self.IsToDateEdit ? parseInt(date) - 1 : parseInt(date) + 1);
    var newDateTiks = newDate.getTime();
    var toDate;
    var leftMargin;
    var rightMargin
    $.each(self.BookingDetails, function () {
        var bookingDetail = this;
        toDate = ParseDate(bookingDetail.ToDate).getTime();
        leftMargin = ParseDate(bookingDetail.LeftMarginDate);
        rightMargin = ParseDate(bookingDetail.RightMarginDate);
        if (self.IsToDateEdit) {
            if ((bookingDetail.LeftMarginDate == null || (bookingDetail.LeftMarginDate != null && leftMargin.getTime() <= newDateTiks)) && newDateTiks >= toDayPara && (bookingDetail.RightMarginDate == null || (newDateTiks <= rightMargin.getTime()))) {
                bookingDetail.IsLocallyEdited = true;
                bookingDetail.ToDate = newDate;
            }
        }
        else if ((bookingDetail.LeftMarginDate == null || (bookingDetail.LeftMarginDate != null && leftMargin.getTime() <= newDateTiks)) && newDateTiks >= toDayPara && newDateTiks <= toDate) {
            bookingDetail.IsLocallyEdited = true;
            bookingDetail.FromDate = newDate;
        }
    });
    this.LoadUI(false);
    this.IsHeaderClick = false;
};
