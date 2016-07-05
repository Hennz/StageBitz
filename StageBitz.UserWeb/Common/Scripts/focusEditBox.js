function focusEdit_SwitchToEditMode(textboxClientID, labelClientID) {

    $("#" + labelClientID).hide();

    var txtInput = $("#" + textboxClientID);

    txtInput.show();
    setCursorPosition(document.getElementById(textboxClientID), 0);
    
}

function focusEdit_SwitchToDisplayMode(textboxClientID, labelClientID, maxLength) {

    var lblDisplay = $("#" + labelClientID);
    var txtInput = $("#" + textboxClientID);

    var fullText = txtInput.val();

    if (maxLength == 0 || fullText.length <= maxLength) {

        if ($.trim(fullText).length == 0) {
            lblDisplay.html("&nbsp;");
        }
        else {
            lblDisplay.html(fullText);
        }
        lblDisplay.removeAttr('title');
    }
    else {
        var truncatedStr = fullText.substring(0, maxLength);
        lblDisplay.html($.trim(truncatedStr) + "...");
        lblDisplay.attr('title', fullText);
    }

    lblDisplay.show();
    txtInput.hide();
}

function setCursorPosition(elem, pos) {
    if (elem.setSelectionRange) {
        elem.focus();
        elem.setSelectionRange(pos, pos);
    } else if (elem.createTextRange) {
        var range = elem.createTextRange();
        range.collapse(true);
        range.moveEnd('character', pos);
        range.moveStart('character', pos);
        range.select();
        elem.focus();
    }
}

function focusEdit_SetValue(value, focusBox) {
    var length = $("span", focusBox).attr('data-displaylength');

    if (value.length > length) {
        $("span", focusBox).attr('title', value);
    }

    $($("span", focusBox)).html(setTruncatedLabelText(value, length));
    $($("input", focusBox)).val(value);
}

function focusEdit_GetValue(focusBox) {
    return $($("input", focusBox)).val();
}

function setTruncatedLabelText(text, length)
{
    text = text.trim();
    var truncatedText = '';
    if (length == 0 || text.length <= length)
    {
        truncatedText = (text.length == 0) ? "&nbsp;" : text;
    }
    else
    {
        truncatedText = text.substring(0, length) + "...";
    }

    return truncatedText;
}