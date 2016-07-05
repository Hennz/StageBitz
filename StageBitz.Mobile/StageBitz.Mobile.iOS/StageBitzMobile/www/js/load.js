var root;
var imageCache;
var itemPostfixNumber = 0;
var appAPIVersion;
var serviceUrl;
var appGetPaused=false;
var noData=false;
var hasSync = false;
var userVoiceContactURL;

function init() {
    
	document.addEventListener("deviceready", deviceReady, false);
    
	// document.addEventListener("showkeyboard", keyboardup, false);
	// document.addEventListener("hidekeyboard", keyboarddown, false);
	// pictureSource=navigator.camera.PictureSourceType;
	// destinationType=navigator.camera.DestinationType;
	// delete init;
    
}
// function keyboardup(){
// //navigator.notification.alert("key up.", function () { });
// $("#home").live(function(e) {
// $(".ui-page.ui-body-c").css('height','200%');
// $(".ui-page.ui-body-c").css('margin-top','-55%');
// });
//
//
// }
// function keyboarddown(){
// //navigator.notification.alert("key up.", function () { });
// $("#home").live(function(e) {
// $(".ui-page.ui-body-c").css('height','200%');
// $(".ui-page.ui-body-c").css('margin-top','-55%');
// });
// }

function deviceReady() {
    if (window.device.platform == "iOS" &&  parseFloat(window.device.version) >= 7) {
        $('meta[name=viewport]').remove();
        $('head').append( '<meta name="viewport" content="user-scalable=no, initial-scale=1.0, maximum-scale=1.0, height=device-height, width=device-width, target-densityDpi=device-dpi"/>' );
    }
	document.addEventListener("backbutton", onBackKeyDown, false);
    document.addEventListener("resume", onResume, false);
	readServiceUrl();
    readAppVersion();
    ReadVoiceContactPage();
	getAppRoot();
	initializeDB();
    dropAndCreateSearchResultTable(function(tx,results){});
	checkPreAuth();
    removePendingtobeDeletedImagesAndItems();
    //checkSync();
	login();
    
    
	
}

function readServiceUrl() {
	$.ajax({
           url : "config.xml",
           dataType : "html",
           success : function(xmlResponse) {
           serviceUrl = $(xmlResponse).find('service').attr('url');
           
           },
           error : function(error) {
           console.log(error);
           },
           async : false
           });
}

function readAppVersion() {
	$.ajax({
           url : "config.xml",
           dataType : "html",
           success : function(xmlResponse) {
           appAPIVersion = $(xmlResponse).find('version').attr('versionnumber');
           
           },
           error : function(error) {
           console.log(error);
           },
           async : false
           });
}

function storesessionid() {
	var sessionid = 'stagebits' + (new Date).getTime();
	window.sessionStorage["sessionid"] = sessionid;
    
}

function login() {
    
	var login = $('#loginbutton');
    
	login.live('click', function() {
               
               handleLogin();
               return false;
               });
}
function handleLogin() {
	// disable the button so we can't resubmit while we wait.
	// $("#loginbutton", this).attr("disabled", "disabled");
    
	var username = $("#username").val();
	var password = $("#password").val();
    
	if (username == '' && password == '') {
        
		navigator.notification.alert(
                                     "Please enter your Email Address and Password to login.",
                                     function() {
                                     }, "Enter Login Details", "OK");
	}
    
	else if (username == '') {
        
		navigator.notification.alert("Please enter your Email Address to login.",
                                     function() {
                                     }, "Enter Login Details", "OK");
	} else if (password == '') {
		navigator.notification.alert("Please enter your Password to login.",
                                     function() {
                                     }, "Enter Login Details", "OK");
	} else if (username != '' && password != '') {
		$("#loginbutton").text("Signing in...");
		$("#loginbutton").addClass("disabled");
        $("#username").addClass("disabled");
        $("#password").addClass("disabled");
		requestDataAtLogin(username, password, appAPIVersion);
		// var dataO='{"Email": "' + username + '", "Pwd":"'+password+
		// '","Version":"'+appAPIVersion+'" }';
		//
		// $.ajax({
		// type : "POST",
		// url :
		// "http://192.168.94.46/StageBitz.Service/Security/AuthenticateUser",
		// data : dataO,
		// ContentType:"application/json; charset=utf-8",
		// dataType : "json",
		// processData : true,
		// success : function(data){alert("success");},
		// error : function(msg){alert(msg.status + ':' + msg.statusText);}
		// });
        
		// if (u == 'username' && p == 'stagebits') {
		// window.localStorage["username"] = u;
		// window.localStorage["password"] = p;
		// storesessionid();
		// $.mobile.changePage("home.html");
		// } else {
		// navigator.notification.alert("Your login failed. Please try login again.", function () { });
		// }
        
	}
    
}
function autoLogin() {
    
	var usertoken = window.localStorage["usertoken"];
	var p = window.localStorage["password"];
	if (u != '' && p != '') {
        
		if (u == 'username' && p == 'stagebits') {
            
			storesessionid();
			// $.mobile.changePage("HomeScreenB.html");
			navigator.splashscreen.hide();
		} else {
			navigator.notification.alert(
                                         "Your login failed. Please login again.", function() {
                                         });
		}
        
	}
    
}
function checkPreAuth() {
	// var form = $("#splash");
	if (window.localStorage["usertoken"] != undefined ) {
		var usertoken=window.localStorage["usertoken"];
		
		requestDataAtInitialization(usertoken, appAPIVersion);
		//$.mobile.changePage("home.html");
	} else {
		// roadcrew.goto("#login");
		$.mobile.changePage("login.html");
        
	}
}

    
	

function getAppRoot() {
	if (device.platform == "Android") {
		window.resolveLocalFileSystemURI(
                                         "file:///data/data/stagebitz.android.stagebitzmobile",
                                         onSuccess, onError);
        
	} else if (device.platform == "iOS") {
		window.requestFileSystem(LocalFileSystem.PERSISTENT, 0, function(
                                                                         fileSystem) {
                                 root = fileSystem.root;
                                 onSuccess(root);
                                 }, function(evt) { // error get file system
                                 console.log(evt.target.error.code);
                                 });
	}
}

function onSuccess(entry) {
	root = entry;
	root.getDirectory("ImageCache", {
                      create : true,
                      exclusive : false
                      }, onImageCacheCreateSuccess, onImageCacheCreateFail);
}

function onImageCacheCreateSuccess(dir) {
    
	imageCache = dir;
	// alert(dir.fullPath.toString());
}
function onImageCacheCreateFail(error) {
	//alert(error.code);
    console.log("onImageCacheCreateFail"+error.code );
    
}

function onError(error) {
	console.log("Error"+error.code );
}

function removePendingtobeDeletedImagesAndItems(){

    var success= function (tx, results) {
        var len = results.rows.length;
        for (var i = 0; i < len; i++) {
            var imageName=results.rows.item(i).filename;
            console.log("getImagestobeDelete");
            deleteImage(imageName);
        }
       
    }
   
        //deleteImage(imageName);
    getImagestobeDelete("SYNCED", success);
    getImagestobeDelete("DELETED", success);
    getImagestobeDelete("DRAFT", success);
    //deleteAllFreshItemsfromDB();
    removeFreshbutNotHavingAnyImages();
    deleteAllRemovedItemsfromDB();
   // deleteAllSyncedItemsfromDB();
}

function removeFreshbutNotHavingAnyImages(){
    
    getAllFreshItems(getAllFreshItemsSuccess);
    
}

function getAllFreshItemsSuccess(tx, results){
    
    
     
    var len = results.rows.length;
    for (var i = 0; i < len; i++) {
        var mobileItem=results.rows.item(i).mobileitemid;
        console.log("getItems");
        checkIsThereAreImageEditsForFreshItem(mobileItem);
    }
    
    
}

function checkIsThereAreImageEditsForFreshItem(mobileItemId){
    var success = function (tx, results){
        
        if(results.rows.item(0).imagecount > 0){
            //update item in db
            
        }else{
            //delete item in db
            deleteItemfromDB(mobileItemId);
        }
        
    }
    getImageAddCountForExistingItem(mobileItemId, success);
    
}

function onBackKeyDown(e) {
	if ($.mobile.activePage[0].id == "home"
        || $.mobile.activePage[0].id == "login") {
		e.preventDefault();
		navigator.app.exitApp();
	} else {
		navigator.app.backHistory();
	}
    
}


function onResume() {
    if ($.mobile.activePage[0].id == "home" && noData==true ){
        
        setTimeout(function() {
                   navigator.splashscreen.show();
                   document.location="index.html";// TODO: do your thing!
                   }, 0);
    }
    setTimeout(function() {
               appGetPaused=true;// TODO: do your thing!
               }, 0);
   
    
    
}

function checkSync(setSyncIcon){
    getUnsyncedItemCount(function(tx,results){
                         if(results.rows.length > 0){
                         var itemCount = results.rows.item(0).itemcount;
                         console.log(itemCount);
                         getUnsyncedImageCount(function(tx,results){
                                               if(results.rows.length > 0){
                                               var imageCount = results.rows.item(0).imagecount;
                                               
                                               if(itemCount == 0 && imageCount == 0){
                                                    setSyncIcon(false);
                                               }else{
                                                    setSyncIcon(true);
                                               }
                                               }
                                               });
                         }
                         });
}

function ReadVoiceContactPage(){
    
    $.ajax({
           url : "config.xml",
           dataType : "html",
           success : function(xmlResponse) {
           userVoiceContactURL = $(xmlResponse).find('contacturl').attr('value');
           },
           error : function(error) {
           },
           async : false
           });
    userVoiceContactURL = encodeURI(userVoiceContactURL);
    
}

function GoToVoiceContactPage(){
    window.open(userVoiceContactURL, '_blank', 'location=yes');
}
