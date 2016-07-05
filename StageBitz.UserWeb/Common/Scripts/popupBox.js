/*
Client script required by PopupBox.cs control
(Depends on jquery and jquery-ui libraries)
*/

function showPopup(id, zIndex) {

    var popupBoxDialogContainer = $('*[id$="' + id + '"] .popupBoxDialogContainer');  //$('#' + id + " .popupBoxDialogContainer");
    var popupBoxTitlebar = $('*[id$="' + id + '"] .popupBoxTitlebar'); // $('*[id$="' + id + '"]')
    var hdnVisualState = $('*[id$="' + id + '_VisualState"]');

    //Set the titlebar as the drag handle of the popupbox
    var cornerCloseButtonSelector = '*[id$="' + id + '"] .popupBoxCornerCloseButton';

    // Destroy existing draggable attachments
    // popupBoxDialogContainer.draggable('destroy'); //Commented after Jquery.UI v1.10.3
    if (popupBoxTitlebar.length > 0) { // if there is no title disabling draggable for the container
        popupBoxDialogContainer.draggable({ handle: popupBoxTitlebar, cancel: cornerCloseButtonSelector });
    }

    //If there are any elements marked with 'popupBoxCloser' class, update their click events
    //so that when they are clicked, popupbox will be closed automatically.
    var closerButtons = $('*[id$="' + id + '"] .popupBoxCloser');

    //Unbind existing click event handler attachments
    closerButtons.unbind("click");
    closerButtons.click(function () {
        hidePopup(id);
        if ($(this).hasClass("reload")) {
            location.reload();
        }
        return false;
    });

    //Set the visual state to remember the popup visibility
    hdnVisualState.val('1');

    //Make the popup visible
    //$('.popupBox#' + id).toggle(true);
    $('*[id$="' + id + '"].popupBox').show();


    //Center the popupbox in screen
    var popupBoxDialog = $('*[id$="' + id + '"] .popupBoxDialog');
    popupBoxDialogContainer.css("top", "50%");
    popupBoxDialogContainer.css("left", "50%");
    popupBoxDialogContainer.css("margin-top", "-" + (popupBoxDialog.height() / 2) + "px");
    popupBoxDialogContainer.css("margin-left", "-" + (popupBoxDialog.width() / 2) + "px");

    if (zIndex && !isNaN(zIndex)) {
        $("div.popupBoxOverlay", $("div[class='popupBox'][id$='" + id + "']")).css("z-index", zIndex);
        $("div.popupBoxDialogContainer", $("div[class='popupBox'][id$='" + id + "']")).css("z-index", zIndex + 1);
    }

    var inputsElements = $(':input:visible', popupBoxDialogContainer);
    if (popupBoxDialogContainer.hasClass('AutoFocus') && inputsElements.length > 0) {
        inputsElements.first().focus();
    }
    else {
        popupBoxDialogContainer.focus();
    }
}

function hidePopup(id) {

    //Set the visual state to remember the popup visibility
    var hdnVisualState = $('*[id$="' + id + '_VisualState"]');
    hdnVisualState.val('');

    $('*[id$="' + id + '"].popupBox').hide();
    $('body').focus();
}

function showErrorPopup(errorCodeId) {
    if (errorCodeId) {
        var popupId = $(".popupBox[data-errorcode=" + errorCodeId + "][data-isdefault]").first().attr("id");
        if (!popupId) {
            var popupId = $(".popupBox[data-errorcode=" + errorCodeId + "]").first().attr("id");
        }

        if (popupId) {
            showPopup(popupId, 1001);
            return true;
        }
        else {
            return false;
        }
    }
    else {
        return false;
    }
}