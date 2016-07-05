//Global dirty flag. Do not access this from outside. Use set/getGlobalDirty() functions.
var __isGlobalDirty = false;

var __ignoreDirtyFlag = false;

function setGlobalDirty(isDirty) {
    __isGlobalDirty = isDirty;
    __ignoreDirtyFlag = false;
    
    //Set the value of the hidden field on master page.
    //Server-side page can refer to this field value via "IsPageDirty" property.
    if (isDirty) {
        $("#hdnGlobalDirtyFlagField").val("1");

        //Fire the extender method if it is implemented
        if (typeof (formBecameDirty) == typeof (Function)) {
            formBecameDirty();
        }
    }
    else {
        $("#hdnGlobalDirtyFlagField").val("");
    }
}

function getGlobalDirty() {
    return __isGlobalDirty;
}

//Document ready event for the whole website.
function globalDocumentReady() {

    //For all user input fields inside dirtyValidationArea elements,
    //update the onchange event to set the global dirty flag.
    $('.dirtyValidationArea :input:not([hidden],:submit,:password,:button)').bind('change.dirtyValidation', function () {
        setGlobalDirty(true)
    });

    //Remove dirty handling from manually excluded inputs.
    $('.dirtyValidationExclude, .dirtyValidationExclude :input:not(:hidden,:submit,:password,:button)').unbind('change.dirtyValidation');

    $('.ignoreDirtyFlag').click(function () {
        __ignoreDirtyFlag = true;
    });

    //We have to do browser detection here because of a bug in IE.
    //If a link on the page has href="javascript:...." (ASP.Net LinkButton is rendered like this), IE will incorrectly fire the window unload event when the link is clicked.
    //We have to maintain a special flag to track this scenario and to avoid displaying "Unsaved changes..." dialog unnecessarily.
    if (navigator.appName == 'Microsoft Internet Explorer') {

        $('[href^="javascript:"]').bind('click.ignoreDirtyFlag', function () {
            __ignoreDirtyFlag = true;
        });

        //Unbind the special event handler from excluded elements
        $('.ignoreDirtyFlagHrefProcessing [href^="javascript:"]').unbind('click.ignoreDirtyFlag');
    }
}

//This will be called on every page redirect, full postback or refresh.
function globalWindowUnload() {

    if (!__ignoreDirtyFlag && getGlobalDirty() == true) {
        return "You have unsaved changes on this page. If you leave this page, those changes will be lost.";
    }

    __ignoreDirtyFlag = false;

    return;
}

//Displays the element with the specified id, for a period of time.
function showNotification(elementId) {

    var elem = $("#" + elementId);

    elem.show();
    setTimeout(function () { elem.hide(); }, 3000);
}

function FormatCurrency(number) {

    var negative = false;

    if (number < 0) {
        negative = true;
        number = number * -1;
    }

    var nStr = number.toFixed(2) + '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }

    return (negative ? "(" : "") + "$" + x1 + x2 + (negative ? ")" : "")
}