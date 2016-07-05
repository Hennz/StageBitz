namespace("StageBitz.UserWeb.Common.Scripts");

StageBitz.UserWeb.Common.Scripts.BookingDetails = function () {
    this.BookingId;
    this.ItemTypeId;
    this.BookingName;
    this.BookingNumber;
    this.UserId;
    this.CompanyId;
    this.BookingLines = {};
    this.MonthToConsider;
}

function ReturnItem(itemBookingId, IsSelect, UserId, callback) {
    //Check if the Item can be pinned

    if (itemBookingId > 0) {

        //call the WebMethod
        var param = { ItemBookingId: itemBookingId, IsSelect: IsSelect, UserId: UserId };

        $.ajax({
            type: "POST",
            url: "../Services/InventoryService/ReturnItem",
            data: JSON.stringify(param),
            async: true,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                if (callback) {
                    callback(msg);
                }
            }
        });
    }
}

function PickUpItem(itemBookingId, IsSelect, UserId, callback) {
    //Check if the Item can be pinned
    if (itemBookingId > 0) {

        //call the WebMethod
        var param = { ItemBookingId: itemBookingId, IsSelect: IsSelect, UserId: UserId };

        $.ajax({
            type: "POST",
            url: "../Services/InventoryService/PickUpItem",
            data: JSON.stringify(param),
            async: true,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                if (callback) {
                    callback(msg);
                }
            }

        });
    }
}

function ReleaseItem(itemBookingId, UserId, callback) {
    //Check if the Item can be pinned

    if (itemBookingId > 0) {

        //call the WebMethod
        var param = { ItemBookingId: itemBookingId, UserId: UserId };

        $.ajax({
            type: "POST",
            url: "../Services/InventoryService/ReleaseItem",
            data: JSON.stringify(param),
            async: true,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                if (callback) {
                    callback(msg);
                }
            }

        });
    }
}

function ApproveBooking(itemBookingId, UserId, callback) {
    if (itemBookingId > 0) {

        //call the WebMethod
        var param = { ItemBookingId: itemBookingId, UserId: UserId };

        $.ajax({
            type: "POST",
            url: "../Services/InventoryService/ApproveBooking",
            data: JSON.stringify(param),
            async: true,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                if (callback) {
                    callback(msg);
                }
            }

        });
    }
}

function RejectBooking(itemBookingId, UserId, callback) {
    if (itemBookingId > 0) {

        //call the WebMethod
        var param = { ItemBookingId: itemBookingId, UserId: UserId };

        $.ajax({
            type: "POST",
            url: "../Services/InventoryService/RejectBooking",
            data: JSON.stringify(param),
            async: true,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                if (callback) {
                    callback(msg);
                }
            }

        });
    }
}


function InventoryLocations_Jquery_SetValue(element, value, isDisable) {
    if (element.length > 0) {
        var jTree = $("*[id$='rtvLocations']", element);
        var jTextbox = $("*[id$='txtLocation']", element);

        var tree = $find(jTree.prop('id'));
        var node = tree.findNodeByValue(value);
        if (node) {
            node.select();
            var parent = node.get_parent();
            while (parent && parent.expand) {
                parent.expand();
                parent = parent.get_parent();
            }

            var locationBreadcrumbArray = BuildLocationBreadCrumb(node, 25);
            if (locationBreadcrumbArray.length > 1) {
                jTextbox.val(locationBreadcrumbArray[0]);
                jTextbox.prop('title', locationBreadcrumbArray[1]);
            }
            
        }

        // Make readonly
        if (typeof isDisable != 'undefined' && isDisable != null) {
            if (isDisable) {
                jTextbox.attr('disabled', 'disabled');
            }
            else {
                jTextbox.removeAttr('disabled');

                // Remove disabled tier2 nodes.
                var tier2Nodes = tree.findNodeByValue(null).get_nodes().toArray();
                $.each(tier2Nodes, function () {
                    if (!this.get_isEnabled()) {
                        this.set_visible(false);
                    }
                });
            }
        }
    }
}

function InventoryLocations_Jquery_GetValue(element) {
    if (element.length > 0) {
        var jTree = $("*[id$='rtvLocations']", element);
        var tree = $find(jTree.prop('id'));
        if (tree.get_selectedNodes().length > 0) {
            return tree.get_selectedNodes()[0].get_value();
        }

        return null;
    }
}

function BuildLocationBreadCrumb(node, length) {
    var isRootnode = node.get_level() == 0;
    var pathText = node.get_text();
    var arr = [];
    var pathText = node.get_text();
    if (!isRootnode) {
        var currentObject = node.get_parent();
        while (currentObject) {

            // get_parent() will return null when we reach the treeview
            if (typeof currentObject.get_text == 'function') {
                pathText = currentObject.get_text() + " > " + pathText;
            }

            if (typeof currentObject.get_parent == 'function') {
                currentObject = currentObject.get_parent();
            }
            else
                break;
        }
    }
    var tirmText = pathText.split("").reverse().join("").substring(0, length).split(" ").join(" ").split("").reverse().join("");
    arr.push(pathText.length > length ? "..." + tirmText : "" + tirmText);
    arr.push(pathText);
    arr.push(node.get_text());
    return arr;
}


