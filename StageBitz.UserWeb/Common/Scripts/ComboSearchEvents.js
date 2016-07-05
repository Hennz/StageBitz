namespace("StageBitz.UserWeb.Common.Scripts");

StageBitz.UserWeb.Common.Scripts.ComboSearchEvents = function () {
    var self = this;
    this.AddEventFiredBooking = false;
    this.FindButton;
    $(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { self.EndRequestHandler() });
    });    
};

StageBitz.UserWeb.Common.Scripts.ComboSearchEvents.prototype.OnItemsRequested = function (sender, eventArgs) {
    //iIf the suggestion list has no items, hide the empty list area.
    var itemCount = sender.get_items().get_count();
    if (itemCount == 0) {
        sender.hideDropDown();
    }
};

StageBitz.UserWeb.Common.Scripts.ComboSearchEvents.prototype.OnKeyPressing = function (sender, eventArgs) {
    if (eventArgs.get_domEvent().keyCode == 13) { //Enter key
        if (!this.AddEventFiredBooking) {
            this.AddEventFiredBooking = true;

            var button = $(this.FindButton);
            if (button) {
                button.click();
            }
        }
    }
};

StageBitz.UserWeb.Common.Scripts.ComboSearchEvents.prototype.OnClientFocus = function (sender, eventArgs) {
    sender.hideDropDown();
};

StageBitz.UserWeb.Common.Scripts.ComboSearchEvents.prototype.OnSelectedIndexChanged = function (sender, eventArgs) {
    if (!this.AddEventFiredBooking) {
        this.AddEventFiredBooking = true;

        var button = $(this.FindButton);
        if (button) {
            button.click();
        }
    }
};

StageBitz.UserWeb.Common.Scripts.ComboSearchEvents.prototype.EndRequestHandler = function () {
    this.AddEventFiredBooking = false;
};
