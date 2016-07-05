


















//var mobileitemID;
//
//function capturePhoto() {
//    navigator.camera.getPicture(onPhotoURISuccess, onFail, { quality: 50, destinationType: Camera.DestinationType.FILE_URI });
//}
//
//function onPhotoURISuccess(imageURI) {
//    createFileEntry(imageURI);
//}
//
//function onFail(message) {
//    alert('Failed to load picture because: ' + message);
//}
//
//function createFileEntry(imageURI) {
//    window.resolveLocalFileSystemURI(imageURI, copyPhoto, fail);    
//}
//
//function copyPhoto(fileEntry) {
//	var d = new Date();
//    var n = d.getTime();
//    //new file name
//    var newFileName = n + ".jpg";
//    
//       root.getDirectory("ImageCache", {create: true, exclusive: false}, function(dir) { 
//                fileEntry.copyTo(dir, newFileName, onCopySuccess, fail); 
//            }, fail); 
//    
//}
//
//function onCopySuccess(entry) {
//	navigator.notification.alert(entry.fullPath);
//    console.log(entry.fullPath);
//    var date = new Date();
//    var time = d.getTime();
//    createImage(entry.name, time, mobileitemID);
//}
//
//function fail(error) {
//    console.log(error.code);
//}
//
//
//function removeImage(imageName) {
//
//	//markImageTobedeleted(imageName);
//	
//	db.transaction(function(tx) {
//
//		tx.executeSql('UPDATE mobileimage SET status = ? WHERE filename = ?',
//				[ "TobeDelete", imageName ], function(tx, results) {
//					// alert("success");
//					 deleteImage(imageName);
//				}, function(tx, error) {
//					alert("error");
//				});
//	});
//}
//
//function deleteImage(imageName){
//	
//	var name=imageName;
//	root.getFile(name, {create: false}, function(entry) {
//        entry.remove(function() {
//            navigator.notification.alert(entry.toURI(), null, 'Entry deleted');     
//            deleteImagefromDB(entry.name);
//        }, onImageDeleteError);
//        
//	},function(msg){
//		deleteImagefromDB(entry.name);
//	}
//	});
//}
//
//
//
//function onImageDeleteError(error){
//var msg = 'Image deletion error: ' + error.code;
//navigator.notification.alert(msg, null, 'Image Deletion Error');
//}

