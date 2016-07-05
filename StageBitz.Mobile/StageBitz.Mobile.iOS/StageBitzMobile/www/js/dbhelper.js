/**
 *
 */
var db;

function initializeDB() {
    db = sqlitePlugin.openDatabase({name: "StageBitzDBs"});
    console.log("db: " + db);
    
    //db.executeSql('CREATE TABLE IF NOT EXISTS items (id integer primary key, name text)');
    //db.executeSql('INSERT INTO items VALUES(2, "Item2")');
    db.transaction(function(tx){
                   tx.executeSql('CREATE TABLE IF NOT EXISTS mobileitem (mobileitemid text primary key, sbitemid integer, itemname text, typeid integer, companyid integer, description text, quantity integer, status text, createdDate text)',[]);
                   tx.executeSql('CREATE TABLE IF NOT EXISTS itemtype (typeid integer primary key, typename text)',[]);
                   tx.executeSql('CREATE TABLE IF NOT EXISTS mobileimage (mobileimageid integer primary key autoincrement, sbimageid integer, mobileitemid text, filename text, createdDate text, status text)',[]);
                   tx.executeSql('CREATE TABLE IF NOT EXISTS companylist (companyid integer primary key, companyname text, cancreate integer)',[]);
                   tx.executeSql('CREATE TABLE IF NOT EXISTS codevalues (codevalue integer primary key, description text)',[]);
                   //console.log("transaction2");
                   });
    return db;
}
function dropAndCreateTables(){
    
    db.transaction(function(tx){
                   tx.executeSql('DROP TABLE IF EXISTS itemtype');
                   tx.executeSql('DROP TABLE IF EXISTS companylist');
                   tx.executeSql('CREATE TABLE IF NOT EXISTS itemtype (typeid integer primary key, typename text)',[]);
                   tx.executeSql('CREATE TABLE IF NOT EXISTS companylist (companyid integer primary key, companyname text, cancreate integer)',[]);
                   
                   //console.log("transaction2");
                   });
    return true;
}

function dropAndCreateSearchResultTable(success){
    db.transaction(function(tx){
                   tx.executeSql('DROP TABLE IF EXISTS searchresult');
                   tx.executeSql('CREATE TABLE IF NOT EXISTS searchresult (sbitemid integer , count integer primary key autoincrement, sbitemname text, sbitemnameprev text, description text, quantity integer, status integer)',[],success,error);
                   });
}

function error(tx, error) {
    console.log("error");
}

function getCompanyDetails(success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM companylist ORDER BY companyname COLLATE NOCASE ASC',[],
                                 success, error);
                   });
    
}

function getItemTypeList(success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM itemtype ORDER BY typename COLLATE NOCASE ASC',[],
                                 success, error);
                   }
                   );
}

function createItem(itemName, date, companyId) {
    /* create mobileitemid using device ID+created Time */
    var mobileitemid = device.uuid + date.getTime() ;
    db.transaction(function (tx) {
                   tx.executeSql('INSERT INTO mobileitem(mobileitemid, itemname, companyid,status) VALUES(?,?,?,?) ', [mobileitemid, itemName, companyId, "DRAFT"],
                                 function success(tx, results) {
                                 console.log("successCreate");
                                 }, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
    return mobileitemid;
}

function saveItem(mobileitemid, itemName, typeid, companyId, description, quantity, date) {
    /* create mobileitemid using device ID+created Time */
    var createdTime = date.getTime();
    db.transaction(function (tx) {
                   tx.executeSql('UPDATE mobileitem set itemname = ? , typeid = ?, companyid = ?, description = ?, quantity = ?, createddate = ?, status = ? WHERE mobileitemid  = ?', [itemName, typeid, companyId, description, quantity, createdTime, "DIRTY", mobileitemid],
                                 function success(tx, results) {
                                 console.log("successUpdateItem");
                                 saveImage(mobileitemid);
                                 }, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
    
    
}

function saveImage(mobileitemid){
    
    db.transaction(function (tx) {
                   tx.executeSql('UPDATE mobileimage set status = ? WHERE mobileitemid  = ? AND status= ?', ["DIRTY", mobileitemid, "DRAFT"],
                                 function success(tx, results) {
                                 
                                 console.log("successUpdateImage: DIRTY");
                                 }, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
}
//-------Testing methods...
function createImage(imageName, date, mobileitemID) {
    /* create mobileitemid using device ID+created Time */
    //var mobileimageid = mobileitemID + date.getTime() ;
    
    var time=date.getTime();
    db.transaction(function (tx) {
                   tx.executeSql('INSERT INTO mobileimage (mobileitemid, filename, createdDate, status) VALUES(?,?,?,?) ', [mobileitemID, imageName, time, "DRAFT"],
                                 function success(tx, results) {
                                 //alert(results.rows.item(0).status);
                                 console.log("createImage:itemid:"+mobileitemID);
                                 }, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("img db create error");
                                 });
                   });
    
}

function createItemTypes(itemTypeID, itemName){
    
    db.transaction(function(tx) {
                   
                   tx.executeSql('INSERT INTO itemtype(typeid,typename) VALUES(?, ?)',
                                 [ itemTypeID, itemName ], function(tx, results) {
                                 console.log("createItemTypes");
                                 }, function(tx, error) {
                                 //alert("error");
                                 });
                   });
}

function createCompanyList(companyID, companyName, isCUser){
    
    db.transaction(function(tx) {
                   
                   tx.executeSql('INSERT INTO companylist(companyid,companyname,cancreate) VALUES(?, ?, ?)',
                                 [ companyID, companyName, isCUser ], function(tx, results) {
                                 console.log("createCompanyList");
                                 }, function(tx, error) {
                                 //alert("error");
                                 });
                   });
}

function deleteImagefromDB(imageName){
	console.log("Deletefromname");
    db.transaction(function(tx) {
                   
                   tx.executeSql('DELETE FROM mobileimage WHERE filename = ?',
                                 [ imageName ], function(tx, results) {
                                 console.log("deleteImagefromDB");
                                 }, function(tx, error) {
                                 //alert("error");
                                 });
                   });
	
}

function deleteImageFromId(imageId, success){
	console.log("DeletefromId");
    db.transaction(function(tx) {
                   
                   tx.executeSql('DELETE FROM mobileimage WHERE mobileimageid = ?',
                                 [ imageId ], success, function(tx, error) {
                                 //alert("error");
                                 });
                   });
	
}

function deleteItemfromDB(itemId){
	
    db.transaction(function(tx) {
                   
                   tx.executeSql('DELETE FROM mobileitem WHERE mobileitemid= ?',
                                 [ itemId ], function(tx, results) {
                                 console.log("deleteItemfromDB");
                                 }, function(tx, error) {
                                 //alert("error");
                                 });
                   });
	
}

function markImageTobedeleted(imageName, success){
	
    db.transaction(function(tx) {
                   
                   tx.executeSql('UPDATE mobileimage SET status = ? WHERE filename = ?',
                                 [ "DELETED", imageName ], success, error);
                   });
    
}

function markItemTobedeleted(itemId, success){
	
    db.transaction(function(tx) {
                   
                   tx.executeSql('UPDATE mobileitem SET status = ? WHERE mobileitemid = ?',
                                 [ "DELETED", itemId ], success, error);
                   });
    
}

function markImagesTobedeleted(itemId, success) {
    
    db.transaction(function (tx) {
                   
                   tx.executeSql('UPDATE mobileimage SET status = ? WHERE mobileitemid = ?',
                                 ["DELETED", itemId], success, error);
                   });
    
}

function getImagesByitemId(itemId, success) {
    
    db.transaction(function(tx) {
                   
                   tx.executeSql('SELECT * FROM mobileimage WHERE mobileitemid = ?',
                                 [ itemId ], success, error);
                   });
    
}

function getImagestobeDelete(status, success){
    
    db.transaction(function(tx) {
                   
                   tx.executeSql('SELECT * FROM mobileimage WHERE status = ?',
                                 [ status ], success, error);
                   });
    
    
}

function deleteAllRemovedItemsfromDB(){
    
    db.transaction(function(tx) {
                   
                   tx.executeSql('DELETE FROM mobileitem WHERE status = ?',
                                 [ "DELETED" ], function(tx, results) {
                                 console.log("deleteAllRemovedItemsfromDB");
                                 }, function(tx, error) {
                                 //alert("error");
                                 });
                   });
    
}
function deleteAllSyncedItemsfromDB(){
    
    db.transaction(function(tx) {
                   
                   tx.executeSql('DELETE FROM mobileitem WHERE status = ?',
                                 [ "SYNCED" ], function(tx, results) {
                                 console.log("deleteAllSyncedItemsfromDB");
                                 }, function(tx, error) {
                                 //alert("error");
                                 });
                   });
    
}

function deleteAllFreshItemsfromDB(){
    
    db.transaction(function(tx) {
                   
                   tx.executeSql('DELETE FROM mobileitem WHERE status = ?',
                                 [ "FRESH" ], function(tx, results) {
                                 console.log("deleteAllFreshItemsfromDB");
                                 }, function(tx, error) {
                                 //alert("error");
                                 });
                   });
    
}

function getAllFreshItems(success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM mobileitem WHERE status="FRESH"',[],success, error);
                   });
}
function getUnSyncedItems(success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM mobileitem WHERE status IN ("DIRTY","SYNCFAILED","FRESH")',[],
                                 success, error);
                   }
                   );
}

function getSyncedItems(success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM mobileitem WHERE status = "SYNCED"',[],
                                 success, error);
                   }
                   );
}

function getUnSyncedImages(mobileItemId, success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM mobileimage WHERE mobileitemid=? AND status IN ("DIRTY","DELETE")',[mobileItemId],
                                 success, error);
                   }
                   );
}

function updateSyncedItem(mobileItemId, sbItemId, status, success){
    db.transaction(function(tx){
                   tx.executeSql('UPDATE mobileitem SET sbitemid=? , status=? WHERE mobileitemid=?',[sbItemId, status, mobileItemId],
                                 success, error);
                   }
                   );
}

function updateSyncedImage(mobileImageId, sbImageId, status, success){
    db.transaction(function(tx){
                   tx.executeSql('UPDATE mobileimage SET sbimageid=? , status=? WHERE mobileimageid=?',[sbItemId, status, mobileItemId],
                                 success, error);
                   }
                   );
}

function formatDateTime(date){
    return date.getFullYear() + "-" + (date.getMonth()+1) + "-" + date.getDate() + " " +date.getHours()+':'+date.getMinutes()+':'+date.getSeconds();
}

function getUnsyncedItemCount(success){
    console.log("getting count");
    db.transaction(function(tx){
                   tx.executeSql('SELECT count(*) as itemcount FROM mobileitem WHERE status IN ("DIRTY","SYNCFAILED")',[],
                                 success, error);
                   }
                   );
}

function getUnsyncedImageCount(success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT count(*) as imagecount FROM mobileimage WHERE status IN ("DIRTY","DELETE")',[],
                                 success, error);
                   }
                   );
}

function mockItemsAndImagesDeleted(){
    
    db.transaction(function (tx) {
                   
                   tx.executeSql('UPDATE mobileimage SET status = ?',
                                 ["DELETED"], function(tx, results) {
                                 console.log("mockImagesDeleted");
                                 }, function(tx, error) {
                                 //alert("error");
                                 });
                   });
    db.transaction(function(tx) {
                   
                   tx.executeSql('UPDATE mobileitem SET status = ? ',
                                 [ "DELETED"], function(tx, results) {
                                 console.log("mockItemsDeleted");
                                 }, function(tx, error) {
                                 //alert("error");
                                 });
                   });
}

function getExistingitemLastUpdatedDate(sbItemId, success){
    
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM mobileitem WHERE sbitemid=?',[sbItemId],
                                 success, error);
                   }
                   );
    
}

function getImageAddCountForExistingItem(mobileItemId, success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT count(*) as imagecount FROM mobileimage WHERE mobileitemid = ?',[mobileItemId],
                                 success, error);
                   }
                   );
    
    
}

function updateExistingItem(mobileitemid, itemName, typeid, description, quantity, date) {
    
    db.transaction(function (tx) {
                   tx.executeSql('UPDATE mobileitem set itemname = ? , typeid = ?,  description = ?, quantity = ?, createddate = ?, status = ? WHERE mobileitemid  = ?', [itemName, typeid, description, quantity, date, "FRESH", mobileitemid],
                                 function success(tx, results) {
                                 console.log("successUpdateExistingItem");
                                 
                                 }, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
}
//temp method used to test
function addExistingItem(){
    var date = new Date();
    
    var time=date.getTime();
    var mobileitemid = device.uuid + date.getTime() ;
    db.transaction(function (tx) {
                   tx.executeSql('INSERT INTO mobileitem(mobileitemid, sbitemid, itemname, companyid,createdDate,status) VALUES(?,?,?,?,?,?) ', [mobileitemid, 18009, "comp", 722, time,"FRESH"],
                                 function success(tx, results) {
                                 
                                 console.log("successCreate");
                                 }, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
    
}
function getImagesForExistingItem(mobileItemId, success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT *  FROM mobileimage WHERE mobileitemid = ? AND status="DIRTY"',[mobileItemId],
                                 success, error);
                   }
                   );
    
    
}

function checkIsItemExistInSyncMode(mobileItemId, success){
    
    db.transaction(function(tx){
                   tx.executeSql('SELECT *  FROM mobileitem WHERE mobileitemid = ? AND status = "SYNCED"',[mobileItemId],
                                 success, error);
                   }
                   );
    
}

function saveExistingItem(mobileitemid, sbitemid, itemname, typeid, companyid, description, quantity, status, createdDate, success){
    
    
    db.transaction(function (tx) {
                   tx.executeSql('INSERT INTO mobileitem(mobileitemid, sbitemid, itemname, typeid,  companyid, description, quantity, status, createdDate) VALUES(?,?,?,?,?,?,?,?,?) ', [mobileitemid, sbitemid, itemname, typeid, companyid, description, quantity, status, createdDate],
                                 success, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
    
    
}
function updateExistingItemWhenbSaving(mobileitemid, itemname, typeid, companyid, description, quantity, status, createdDate, success) {
    /* create mobileitemid using device ID+created Time */
    
    
    db.transaction(function (tx) {
                   tx.executeSql('UPDATE mobileitem set itemname = ? , typeid = ?, companyid = ?, description = ?, quantity = ?, createddate = ?,status = ? WHERE mobileitemid  = ?',
                                 [itemname, typeid, companyid, description, quantity, createdDate, status, mobileitemid],
                                 success, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
    
    
}

function updateExistingItemWithoutStatus(mobileitemid, itemname, typeid, companyid, description, quantity, createdDate, success) {
    /* create mobileitemid using device ID+created Time */
    //var createdTime = date.getTime();
    
    db.transaction(function (tx) {
                   tx.executeSql('UPDATE mobileitem set itemname = ? , typeid = ?, companyid = ?, description = ?, quantity = ?, createddate = ? WHERE mobileitemid  = ?',
                                 [itemname, typeid, companyid, description, quantity, createdDate, mobileitemid],
                                success, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
    
    
}
function deleteSyncExistingItemfromDB(itemId, success){
	
    db.transaction(function(tx) {
                   
                   tx.executeSql('DELETE FROM mobileitem WHERE mobileitemid= ?',
                                 [ itemId ], success, function(tx, error) {
                                 //alert("error");
                                 });
                   });
	
}

function updateImagesMobileItemId(oldMobileItemId, newMobileItemId){
    
    db.transaction(function(tx){
                   tx.executeSql('UPDATE mobileimage SET mobileitemid =? , status="DIRTY" WHERE mobileitemid=?',[oldMobileItemId, newMobileItemId],
                                 function success(tx, results) {
                                 
                                 console.log("updateImagesMobileItemId");
                                 }, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
}
function saveImageForExistingItem(imageName, date, mobileitemID) {
    /* create mobileitemid using device ID+created Time */
    //var mobileimageid = mobileitemID + date.getTime() ;
   
    var time=date.getTime();
    db.transaction(function (tx) {
                   tx.executeSql('INSERT INTO mobileimage (mobileitemid, filename, createdDate, status) VALUES(?,?,?,?) ', [mobileitemID, imageName, time, "DIRTY"],
                                 function success(tx, results) {
                                 //alert(results.rows.item(0).status);
                                 console.log("createImage:itemid:"+mobileitemID);
                                 }, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("img db create error");
                                 });
                   });
    
}

function getItemDetailsByMobileItemId(mobileItemId, success){
    
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM mobileitem WHERE mobileitemid = ?',[mobileItemId],
                                 success, error);
                   }
                   );
    
}

function markUnsavedImagesOfEditedItemsTobedeleted(itemId, success) {
    
    db.transaction(function (tx) {
                   
                   tx.executeSql('UPDATE mobileimage SET status = ? WHERE mobileitemid = ? AND status = ? ',
                                 ["DELETED", itemId, "DRAFT"], success, error);
                   });
    
}


function getNotSavedImagesByitemId(itemId, success) {
    
    db.transaction(function(tx) {
                   
                   tx.executeSql('SELECT * FROM mobileimage WHERE mobileitemid = ? AND status= ?',
                                 [ itemId, "DRAFT" ], success, error);
                   });
    
}


function saveSearchResult(itemId, itemName, itemnameprev, quantity, description, status,success){
    db.transaction(function(tx) {
                   tx.executeSql('INSERT INTO searchresult(sbitemid, sbitemname, sbitemnameprev, description, quantity, status) VALUES(?, ?,?, ?,?,?)',
                                 [ itemId, itemName, itemnameprev, description, quantity, status ], success, function(tx, error) {
                                 console.log("error");
                                 });
                   });
}


function getSearchResults(success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM searchresult ORDER BY count COLLATE NOCASE ASC',[],success, error);
                   });
}

function getItemFromSBItemId(sbitemid,success){
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM mobileitem WHERE sbitemid=?',[sbitemid],success, error);
                   });
    
}

function getImagesFromMobileItemID(mobileitemid,success){
    console.log("getting Images");
    db.transaction(function(tx){
                   tx.executeSql('SELECT * FROM mobileimage WHERE mobileitemid = ? AND status IN ("DIRTY","DELETE")',[mobileitemid],success, error);
                   });
}

function checkImageIsAlreadydeleted(documentMediaId, success){

    db.transaction(function(tx){
                   tx.executeSql('SELECT count(*) as deleteimagecount FROM mobileimage WHERE sbimageid = ?',[documentMediaId],
                                 success, error);
                   }
                   );

}

function deleteServerImage(mobileitemID, documentMediaId) {

    var date =new Date();
    var time=date.getTime();
    db.transaction(function (tx) {
                   tx.executeSql('INSERT INTO mobileimage (mobileitemid, sbimageid, createdDate, status) VALUES(?,?,?,?) ', [mobileitemID, documentMediaId, time, "DELETE"],
                                 function success(tx, results) {
                                 
                                 console.log("deketeImage:itemid:"+mobileitemID+" docID"+documentMediaId);
                                 }, function error(tx, error) {
                                 
                                 console.log("img db create error");
                                 });
                   });
    
}
function changeItemStatusToSync(mobileitemid) {
    /* create mobileitemid using device ID+created Time */
    var createdTime = date.getTime();
    db.transaction(function (tx) {
                   tx.executeSql('UPDATE mobileitem set status = "SYNCED" WHERE mobileitemid  = ?', [mobileitemid],
                                 function success(tx, results) {
                                 console.log("changeItemStatusToSync");
                                 
                                 }, function error(tx, error) {
                                 mobileitemid = null;
                                 console.log("error");
                                 });
                   });
    
    
}
function updateSearchResult(itemId, itemname, quantity, description, status,success){
    
    db.transaction(function(tx) {
                   tx.executeSql('UPDATE searchresult SET sbitemname=?, description=?, quantity=?, status=? WHERE sbitemid = ?',
                                 [itemname, description, quantity, status, itemId], success, function(tx, error) {
                                 console.log("error");
                                 });
                   });
}

function updateSearchResultStatus(itemId,status,success){
    db.transaction(function(tx) {
                   tx.executeSql('UPDATE searchresult SET status=? WHERE sbitemid = ?',
                                 [status, itemId], success, function(tx, error) {
                                 console.log("error");
                                 });
                   });
}

function defaultSuccess(tx, results){
    
    console.log("default success");

}