/*
Client script required by PopupBox.cs control
(Depends on jquery and jquery-ui libraries)
*/

function showPopup(id) {

    var popupBoxDialogContainer = $('#' + id + " .popupBoxDialogContainer");
    var popupBoxTitlebar = $('#' + id + " .popupBoxTitlebar");
    var hdnVisualState = $('#' + id + "_VisualState");

    //Set the titlebar as the drag handle of the popupbox
    var cornerCloseButtonSelector = '#' + id + " .popupBoxCornerCloseButton";

    //Destroy existing draggable attachments
    //popupBoxDialogContainer.draggable('destroy');//Commented after Jquery.UI v1.10.3
    popupBoxDialogContainer.draggable({ handle: popupBoxTitlebar, cancel: cornerCloseButtonSelector });

    //If there are any elements marked with 'popupBoxCloser' class, update their click events
    //so that when they are clicked, popupbox will be closed automatically.
    var closerButtons = $('#' + id + " .popupBoxCloser");

    //Unbind existing click event handler attachments
    closerButtons.unbind("click");
    closerButtons.click(function () { hidePopup(id); return false; });

    //Set the visual state to remember the popup visibility
    hdnVisualState.val('1');

    //Make the popup visible
    $('.popupBox#' + id).show();

    //Center the popupbox in screen
    var popupBoxDialog = $('#' + id + " .popupBoxDialog");
    popupBoxDialogContainer.css("top", "50%");
    popupBoxDialogContainer.css("left", "50%");
    popupBoxDialogContainer.css("margin-top", "-" + (popupBoxDialog.height() / 2) + "px");
    popupBoxDialogContainer.css("margin-left", "-" + (popupBoxDialog.width() / 2) + "px");
}

function hidePopup(id) {

    //Set the visual state to remember the popup visibility
    var hdnVisualState = $('#' + id + "_VisualState");
    hdnVisualState.val('');

    $('.popupBox#' + id).hide();
}