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

    initializeDirtyValidationInputs($(document));

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

function initializeDirtyValidationInputs(element) {
    //For all user input fields inside dirtyValidationArea elements,
    //update the onchange event to set the global dirty flag.
    $('.dirtyValidationArea :input:not([type=hidden],:submit,:password,:button)', element).bind('change.dirtyValidation', function () {
        // Refer RadNumericTextBox_Jquery_SetValue()
        if (CanSetDirty(this)) {
            setGlobalDirty(true);
        }
    });

    //Remove dirty handling from manually excluded inputs.
    $('.dirtyValidationExclude, .dirtyValidationExclude :input:not([type=hidden],:submit,:password,:button)', element).unbind('change.dirtyValidation');

    $('.ignoreDirtyFlag', element).click(function () {
        __ignoreDirtyFlag = true;
    });
}

function CanSetDirty(element) {
    return !$(element).hasClass("SetValueInProgress");
}

function IntializeErrorMessages() {
    $('.inputError:not(:empty)').addClass('Show');
    $('.inputError:empty').removeClass('Show');
}

//This will be called on every page redirect, full postback or refresh.
function globalWindowUnload() {

    var prm = Sys.WebForms.PageRequestManager.getInstance();

    if (!__ignoreDirtyFlag && getGlobalDirty() == true && (!prm || !prm._processingRequest)) {
        return "You have unsaved changes on this page. If you leave this page, those changes will be lost.";
    }

    __ignoreDirtyFlag = false;

    return;
}

//Displays the element with the specified id, for a period of time.
function showNotification(elementId, delay) {
    if (!delay || isNaN(delay)) {
        delay = 3000;
    }

    var elem = $("#" + elementId);

    elem.show();
    setTimeout(function () { elem.hide(); }, delay);
}

function FormatCurrency(number, culture) {
    $.preferCulture(culture);
    return $.format(number, "c");
}

function ToggleDropdownMenuPopup(launcher, popup, align) {

    if (!align) {
        align = "right";
    }

    if (popup.css("display") == "none") {

        popup.show();

        var popupPosition = {
            "my": align + " top",
            "at": align + " bottom",
            "of": launcher,
            "collision": "fit"
        };
        //show the menu directly over the placeholder
        //We have to run this twice because of a problem in IE. It takes 2 executions to get IE to apply correct positioning logic.
        popup.position(popupPosition);
        popup.position(popupPosition);

        //bring the menu to the front
        popup.css("z-index", 99999);

        popup.focus();
    }
    else {
        popup.hide();
    }
}




function updateQueryStringParameter(uri, key, value) {
    var re = new RegExp("([?|&])" + key + "=.*?(&|$)", "i");
    separator = uri.indexOf('?') !== -1 ? "&" : "?";
    if (uri.match(re)) {
        return uri.replace(re, '$1' + key + "=" + value + '$2');
    }
    else {
        return uri + separator + key + "=" + value;
    }
}

function AnimateDiv(element, width, height) {
    $("#" + element).animate(
        {
            width: width,
            height: height
        }, {
            duration: 500,
            complete: function () {
                $("#" + element).height('auto');
            }
        });
}

function DisablePostBackForLit(ddCountry) {
    var selected_value = $("#" + ddCountry).val();
    if (selected_value == 0) {
        __doPostBack(ddCountry, '');
    }
    else {
        return false;
    }
}

function Calculate(Luhn) {
    var sum = 0;
    for (i = 0; i < Luhn.length; i++) {
        sum += parseInt(Luhn.substring(i, i + 1));
    }
    var delta = new Array(0, 1, 2, 3, 4, -4, -3, -2, -1, 0);
    for (i = Luhn.length - 1; i >= 0; i -= 2) {
        var deltaIndex = parseInt(Luhn.substring(i, i + 1));
        var deltaValue = delta[deltaIndex];
        sum += deltaValue;
    }
    var mod10 = sum % 10;
    mod10 = 10 - mod10;
    if (mod10 == 10) {
        mod10 = 0;
    }
    return mod10;
}

function ValidateCreditCardDetails(sender, args) {
    var Luhn = $("#" + sender.controltovalidate).val();

    Luhn = Luhn.replace(/^\s+|\s+|\s+$/g, "");
    var LuhnLength = Luhn.length;

    if (LuhnLength == 0) {
        args.IsValid = true;
        return;
    }
    else if (LuhnLength != 16) {
        args.IsValid = false;
        return;
    }

    var LuhnDigit = parseInt(Luhn.substring(Luhn.length - 1, Luhn.length));
    var LuhnLess = Luhn.substring(0, Luhn.length - 1);
    if (Calculate(LuhnLess) == parseInt(LuhnDigit)) {
        args.IsValid = true;
        return;
    }
    args.IsValid = false;
}


function namespace(namespaceString) {
    var parts = namespaceString.split('.'),
        parent = window,
        currentPart = '';

    for (var i = 0, length = parts.length; i < length; i++) {
        currentPart = parts[i];
        parent[currentPart] = parent[currentPart] || {};
        parent = parent[currentPart];
    }

    return parent;
};

// Hack for display multiple tooltips on a page in same time.
function showMultipleToolTips(cssClass) {
    $(".RadToolTip." + cssClass).show();
    $(".RadToolTip." + cssClass).css('visibility', 'visible');
};

function TruncateString(obj, count) {
    var textName = obj.text();
    obj.attr("title", textName);
    obj.text($.trim(textName.substring(0, count).split(" ").join(" ") + "..."));
}

function FormatDate(date) {
    return $.datepicker.formatDate('dd M yy', date);
}

function ParseDate(date) {
    if (!(date instanceof Date) && date != null) {
        var parts = date.match(/(\d+)/g);
        return new Date(parts[0], parts[1] - 1, parts[2]);
    }
    return date;
}

function RadComboBox_Jquery_SetText(element, value) {
    var input = element.find("input[type=text]");
    input.addClass("SetValueInProgress");
    var radComboBox = $find(element.attr('id'));
    radComboBox.set_text(value);
    input.removeClass("SetValueInProgress");
};

$.extend($.ui.autocomplete.prototype, {
    _renderItem: function (ul, item) {
        var term = this.element.val(),
            html = item.label.replace(term, "<b>$&</b>");
        return $("<li class='sbAutocomplete'></li>")
            .data("item.autocomplete", item)
            .append($("<a></a>").html(html))
            .appendTo(ul);
    }
});


function RadNumericTextBox_Jquery_SetValue(element, value, isDisable) {
    element.addClass("SetValueInProgress");
    var objTextbox = $find(element.attr('id'));
    objTextbox.set_value(value);
    if (isDisable) {
        objTextbox.disable();
    }
    else {
        objTextbox.enable();
    }

    element.removeClass("SetValueInProgress");
}



