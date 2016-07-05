


function deleteImage(imageName){
	
	
	imageCache.getFile(imageName, {create: false}, function(entry) {
                       entry.remove(function() {
                                    //navigator.notification.alert(entry.toURL(), null, 'Entry deleted');
                                    console.log("Entry deleted"+entry.toURL());
                                    deleteImagefromDB(entry.name);
                                    }, onImageDeleteError);
                       
                       },function(msg){
                       console.log("nonono");
                       deleteImagefromDB(imageName);
                       
                       }
                       );
}



function onImageDeleteError(error){
    var msg = 'Image deletion error: ' + error.code;
    //navigator.notification.alert(msg, null, 'Image Deletion Error');
    console.log(msg );
}

function removeItem(itemId){
    
    markUnsavedItemtoDelete(itemId);
    
}

function markUnsavedItemtoDelete(itemId) {
    var success = function (tx, results) {
        markUnsavedImagestoDelete(itemId);
        console.log("markUnsavedItemtoDelete");
    }
    markItemTobedeleted(itemId, success);
}


function markUnsavedImagestoDelete(itemId) {
    var success = function (tx, results) {
        deleteUnsavedImages(itemId);
        console.log("markUnsavedImagestoDelete : ");
    }
    markImagesTobedeleted(itemId, success);
}

function deleteUnsavedImages(itemId) {
    var success = function (tx, results) {
        console.log("deleteUnsavedImages : "+ results.rows.length+"itemId"+itemId);
        var len = results.rows.length;
        for (var i = 0; i < len; i++) {
            var imageName=results.rows.item(i).filename;
            console.log(imageName);
            deleteImage(imageName);
        }
        deleteItemfromDB(itemId);
    }
    
    
    getImagesByitemId(itemId, success);
}

function markUnsavedImagesToDeleteForItemNotEditedAndSavedInDb(itemId) {
    
    var success = function (tx, results) {
        deleteUnsavedImages(itemId);
        console.log("markUnsavedImagestoDelete : ");
    }
    markUnsavedImagesOfEditedItemsTobedeleted(itemId, success);
}


function markUnsavedImagesToDeleteForAlreadyEditedAndSavedItemInDb(itemId) {
    
    var success = function (tx, results) {
    	deleteUnsavedImagesForAlreadyEditedAndSavedItemInDb(itemId);
        console.log("markUnsavedImagestoDelete : ");
    }
    markUnsavedImagesOfEditedItemsTobedeleted(itemId, success);
}

function deleteUnsavedImagesForAlreadyEditedAndSavedItemInDb(itemId) {
    var success = function (tx, results) {
        console.log("deleteUnsavedImages : "+ results.rows.length+"itemId"+itemId);
        alert(results.rows.length);
        var len = results.rows.length;
        for (var i = 0; i < len; i++) {
            var imageName=results.rows.item(i).filename;
            console.log(imageName);
            deleteImage(imageName);
        }
        
    }
    
    
    getNotSavedImagesByitemId(itemId, success);
}
