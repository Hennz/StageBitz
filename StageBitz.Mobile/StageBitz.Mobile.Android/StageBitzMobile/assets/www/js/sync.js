var itemCountSynced;
var itemCountFailed;
var itemCount = 0;
var imageCount = 0;
function synchronize(setProgress,updateSyncCount){
    itemCountSynced = 0;
    itemCountFailed = 0;
    setProgress("Uploading process started...","");
    console.log("syncing...");
    
	syncInturruptedItems(setProgress,updateSyncCount);
	//syncNewItems(setProgress);
}

function setSyncCount(updateSyncCount){
    getUnsyncedItemCount(function(tx,results){
                         if(results.rows.length > 0){
                         itemCount = results.rows.item(0).itemcount;
                         getUnsyncedImageCount(function(tx,results){
                                               if(results.rows.length > 0){
                                               imageCount = results.rows.item(0).imagecount;
                                               updateSyncCount(itemCount,imageCount);
                                                    enableSyncButton();
                                               }
                                               });
                         }
                         });
}

function enableSyncButton(){
    if(imageCount == 0 && itemCount== 0){
        $('#synchronize').addClass('disabled');
    }else{
        $('#synchronize').removeClass('disabled');
    }
}

function syncNewItems(setProgress,updateSyncCount){
	getUnSyncedItems(function(tx,results){
                     var len = results.rows.length;
                     
                     if(len > 0){
                     //for(var i = 0; i<len; i++){
                     
                     function syncItem(i){
                     if(i<len){
                     
                     var item = results.rows.item(i);
                     //upload item details
                     
                     console.log(item.status);
                     if(item.status != "FRESH"){
                     setProgress("Uploading Item: " + item.itemname +"...","");
                     uploadItemDetails((item.sbitemid==""?0:item.sbitemid), item.mobileitemid, item.itemname, item.typeid, item.companyid, item.description, item.quantity, item.createdDate, function(data){
                                       if(data.Status == "OK" || data.Status == "ITEMEXIST" || data.Status == "ITEMEDITED"){
                                       //if item is successfully created in the server or item already exists, update the item details in db and sync images for the item
                                       //setProgress(data.Status);
                                       if(data.Status == "ITEMEDITED"){
                                            setProgress(data.Message,"error");
                                       }
                                       updateSyncCount(--itemCount,imageCount);
                                       syncItemImages(item.mobileitemid, (data.Id==0?item.sbitemid:data.Id), item.companyid, setProgress, syncItem, i+1,updateSyncCount);
                                       updateSyncedItem(item.mobileitemid, (data.Id==0?item.sbitemid:data.Id), "SYNCED", function(tx,result){
                                                        
                                                        console.log("upload sucess..."+ data.Status);
                                                        
                                                        });
                                       }else if(data.Status == "INVALIDAPP"){
                                            setProgress("Upload failed.","error");
                                            setProgress(data.Message,"error");
                                       }else if(data.Status == "ITEMDELETED"){
                                            console.log("Deleted Item");
                                            updateSyncCount(--itemCount,imageCount);
                                            setProgress(data.Message,"error");
                                            updateSyncedItem(item.mobileitemid, (item.sbitemid==""?0:item.sbitemid), "SYNCED", deleteItemAndImages(item.mobileitemid,syncItem,i+1));
                                       }else {
                                       itemCountFailed = itemCountFailed +1;
                                       setProgress(data.Message,"error");
                                       syncItem(i+1);
                                       console.log("upload fail..." + data.Status);
                                       updateSyncedItem(item.mobileitemid, (item.sbitemid==""?0:item.sbitemid), "SYNCFAILED", function(tx,result){
                                                        
                                                        
                                                        });
                                       }
                                       }, function(msg){
                                       itemCountFailed = itemCountFailed +1;
                                       setProgress("Upload failed.","error");
                                       syncItem(i+1);
                                       console.log("upload fail..."+ msg.status);
                                       updateSyncedItem(item.mobileitemid, (item.sbitemid==""?0:item.sbitemid), "SYNCFAILED", function(tx,result){
                                                        
                                                        
                                                        });},function(){
                                                        enableSyncButton();
                                                        setProgress("<br>Internet connection lost while uploading Items to the server. Please try again once your device connects to the Internet.","error");
                                                        }
                                                        );
                                       
                     
                     }else{
                                console.log("Fresh");
                                getImagesFromMobileItemID(item.mobileitemid,function(tx,results){
                                               console.log(results.rows.length);
                                               if(results.rows.length > 0){
                                                    setProgress("Uploading Item: " + item.itemname +"...","");
                                                    syncItemImages(item.mobileitemid, item.sbitemid, item.companyid, setProgress,syncItem, i+1,updateSyncCount);
                                               }else{
                                                    syncItem(i+1);
                                               }
                                               });
                     }
                     }else{
                     //updateSyncCount(itemCount,imageCount);
                     setProgress("</br>Upload complete.<br/><br/>"+(itemCountSynced>0?itemCountSynced+" Item"+(itemCountSynced==1?"":"s")+" uploaded successfully":"")+(itemCountFailed==0?"":(itemCountSynced==0?"":" and ")+itemCountFailed+" Item"+(itemCountFailed==1?" ":"s ")+"failed to upload")+".","");
                     enableSyncButton();
                     setProgress((itemCountFailed==0?"":"<br>Failed uploads can happen for a number of reasons including a lost connection. They are stored in your phone's memory so you can try re-syncing them later."));
                     setProgress("");
                     }
                     }
                     //var i = 0;
                     syncItem(0);
                     //}
                     }else{
                     //updateSyncCount(itemCount,imageCount);
                     setProgress("</br>Upload complete.<br/><br/>"+(itemCountSynced>0?itemCountSynced+" Item"+(itemCountSynced==1?"":"s")+" uploaded successfully":"")+(itemCountFailed==0?"":(itemCountSynced==0?"":" and "+itemCountFailed+" ")+"Item"+(itemCountFailed==1?" ":"s ")+"failed to upload")+".","");
                     enableSyncButton();
                     setProgress((itemCountFailed==0?"":"<br>Failed uploads can happen for a number of reasons including a lost connection. They are stored in your phone's memory so you can try re-syncing them later."));
                     setProgress("");
                     }
                     });
	
}

function syncInturruptedItems(setProgress,updateSyncCount){
    
	getSyncedItems(function(tx,results){
                   var len = results.rows.length;
                   if(len > 0){
                   
                   function syncInturruptedItem(i){
                   
                   if(i<len){
                   var item = results.rows.item(i);
                   //upload item details
                   setProgress("Uploading Item: " + item.itemname+"...","");
                   
                   syncItemImages(item.mobileitemid, item.sbitemid, item.companyid, setProgress,syncInturruptedItem, i+1,updateSyncCount);
                   }else{
                   syncNewItems(setProgress,updateSyncCount);
                   }
                   }
                   
                   //var i = 0;
                   syncInturruptedItem(0);
                   }else{
                   syncNewItems(setProgress,updateSyncCount);
                   }
                   
                   });
}

var hasFailed;
var lenImages;
function syncItemImages(mobileItemId, sbItemId, companyId, setProgress, syncNextItem, nextItemNo,updateSyncCount){
    hasFailed = 0;
	getUnSyncedImages(mobileItemId, function(tx,results){
                      lenImages = results.rows.length;
                      
                      if(lenImages > 0){
                      //for(var i=0; i<len; i++){
                      function syncImage(i){
                      var image = results.rows.item(i);
                      //read image from mobile storage
                      if(image.status == "DELETE"){
                      
                      setProgress("Deleting Image...","");
                      uploadImageData(image.sbimageid, sbItemId, mobileItemId+image.mobileimageid, image.filename, "", companyId, "Item", 1, "", image.createdDate, function(data){
                                      imageUploadSuccess(data,updateSyncCount,setProgress,image.status,image.mobileimageid,"",mobileItemId,syncNextItem,nextItemNo,syncImage,i+1);
                                      
                                      },function(msg){
                                      imageUploadFail(setProgress, image.status,syncNextItem,nextItemNo,syncImage,i+1 );
                                      },function(){
                                      enableSyncButton();
                                      setProgress("<br>Internet connection lost while uploading Items to the server. Please try again once your device connects to the Internet.","error");
                                      });
                      
                      }else{
                      readImageData(image.filename, function(imageContent){
                                    //console.log(imageContent);
                                    
                                    var imageData = imageContent.split(";base64,");
                                    var imageMeta = imageData[0].split("/");
                                    
                                    setProgress("Uploading Image: " + image.filename +"...","");
                                    uploadImageData(0, sbItemId, mobileItemId+image.mobileimageid, image.filename, imageMeta[1], companyId, "Item", 1, imageData[1], image.createdDate, function(data){
                                                    imageUploadSuccess(data,updateSyncCount,setProgress,image.status,image.mobileimageid,image.filename,mobileItemId,syncNextItem,nextItemNo,syncImage,i+1);
                                                    
                                                    },function(msg){
                                                    imageUploadFail(setProgress, image.status,syncNextItem,nextItemNo,syncImage,i+1 );
                                                    },function(){
                                                    enableSyncButton();
                                                    setProgress("<br>Internet connection lost while uploading Items to the server. Please try again once your device connects to the Internet.","error");
                                                    });
                                    
                                    });
                      }
                      }
                      
                      //var i = 0;
                      syncImage(0);
                      //}
                      }else{
                      
                      setProgress("Item uploaded.","");
                      itemCountSynced = itemCountSynced+1;
                      deleteItemfromDB(mobileItemId);
                      syncNextItem(nextItemNo);
                      }
                      });
}


function imageUploadSuccess(data,updateSyncCount,setProgress,status,mobileImageId,fileName,mobileItemId,syncNextItem,nextItemNo,syncImage,nextImageNo){
    console.log(data.Status);
    if(data.Status == "OK" || data.Status == "EXIST"){
        updateSyncCount(itemCount,--imageCount);
        if(data.Message ==""){
            setProgress("Image "+(status == "DELETE"?"deleted.":"uploaded."),"");
        }else{
            setProgress(data.Message,"");
        }
        if(status=="DELETE"){
            updateSyncedImage(mobileImageId, data.Id, "SYNCED",deleteImageFromId(mobileImageId,function(){}));
        }else{
            updateSyncedImage(mobileImageId, data.Id, "SYNCED",deleteImage(fileName));
        }
    }else if(data.Status == "INVALIDAPP"){
        setProgress("Upload failed.","error");
        setProgress(data.Message,"error");
    }else if(data.Status == "ITEMDELETED"){
        setProgress(data.Message,"error");
    }else{
        setProgress(data.Message,"error");
        hasFailed = 1;
    }
    console.log(nextImageNo + "::: "+lenImages);
    if(nextImageNo == lenImages){
        if(data.Status == "ITEMDELETED"){
            deleteItemAndImages(mobileItemId,syncNextItem,nextItemNo);
        }else{
        if(hasFailed==0){
            setProgress("Item uploaded.","");
            itemCountSynced = itemCountSynced+1;
            deleteItemfromDB(mobileItemId);
        }else{
            itemCountFailed = itemCountFailed +1;
            setProgress("Some Images could not be uploaded/deleted.","error");
        }
        //console.log("nextItem" + nextItemNo);
        syncNextItem(nextItemNo);
        }
    }else{
        if(data.Status == "ITEMDELETED"){
            deleteItemAndImages(mobileItemId,syncNextItem,nextItemNo);
        }else{
        //console.log("nextImage"+nextImageNo);
            syncImage(nextImageNo);
        }
    }
}


function imageUploadFail(setProgress, status,syncNextItem,nextItemNo,syncImage,nextImageNo ){
    setProgress("Image "+(status == "DELETE"?"deletion":"upload")+" failed.","error");
    hasFailed = 1;
    if(nextImageNo == lenImages){
        setProgress("Some Images could not be uploaded/deleted.","error");
        syncNextItem(nextItemNo);
    }else{
        syncImage(nextImageNo);
    }
    
}

function readImageData(fileName, success){
	imageCache.getFile(fileName, null, function(fileEntry){
                       fileEntry.file(function(file){
                                      reader = new FileReader();
                                      reader.readAsDataURL(file);
                                      reader.onloadend = function(evt) {
                                      success(evt.target.result);
                                      }
                                      }, error);
                       }, error);
}

function deleteItemAndImages(mobileItemId, next,nextNo){
    console.log("Deleting:"+mobileItemId);
    getUnSyncedImages(mobileItemId, function(tx,results){
                      len = results.rows.length;
                      console.log(len);
                      itemCountFailed = itemCountFailed +1;
                      if(len > 0){
                            imageCount  = imageCount - len;
                            updateSyncCount(itemCount,imageCount);
                            function deleteNextImage(i){
                                if(i<len){
                      console.log(results.rows.item(i).status);
                                    if(results.rows.item(i).status == "DELETE"){
                                        deleteImageFromId(results.rows.item(i).mobileimageid,function(){});
                                    }else{
                                        deleteImage(results.rows.item(i).filename);
                                    }
                                    deleteNextImage(i+1);
                                }else{
                                    deleteItemfromDB(mobileItemId);
                                    next(nextNo);
                                }
                            }
                            deleteNextImage(0);
                      }else{
                        deleteItemfromDB(mobileItemId);
                        next(nextNo);
                      }
        });
    
}


