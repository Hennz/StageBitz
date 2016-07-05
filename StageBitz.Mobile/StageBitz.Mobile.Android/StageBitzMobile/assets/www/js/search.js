var isSearching = false;
function doSearch(companyId,searchText,itemTypeId, viewedResultCount, appendSearchResults, appendNoMore,appendNoData,enableSearch){
    isSearching = true;
    loadSearchResults(searchText, itemTypeId, companyId, viewedResultCount,
                      function(data){
                      if(data.Status == "OK"){
                      console.log("ok");
                      if(data.MobileSearchItems.length > 0){
                      if(data.HasMoreResults == 1){
                      displaySearchResults(0,data.MobileSearchItems, appendSearchResults, function(){},enableSearch);
                      }else{
                      displaySearchResults(0,data.MobileSearchItems, appendSearchResults, appendNoMore,enableSearch);
                      }
                      }else{
                      console.log("noData");
                      appendNoData("No results found.");
                      isSearching = false;
                      enableSearch();
                      }
                      }else{
                      console.log(data.Status);
                      appendNoData(data.Message);
                      isSearching = false;
                      enableSearch();
                      }
                      },
                      
                      function(msg){
                      console.log("server error");
                      appendNoData("Search failed. Please try again later.");
                      isSearching = false;
                      enableSearch();
                      },
                      function(){
                      console.log("noConnection");
                      isSearching = false;
                      enableSearch();
                      });
}


function displaySearchResults(i,searchItems, appendSearchResults, appendLast,enableSearch){
    
    getItemFromSBItemId(searchItems[i].ItemId, function(tx,results){
                        var len = results.rows.length;
                        var status = (len > 0? "1":"0");
                        
                        if(len > 0){
                            if(results.rows.item(0).status == "FRESH" || results.rows.item(0).status == "SYNCED"){
                               getImagesFromMobileItemID(results.rows.item(0).mobileitemid,function(tx,imgresults){
                                                         console.log("Fresh" + imgresults.rows.length);
                                                if(imgresults.rows.length > 0){
                                                         saveSearchResult(searchItems[i].ItemId, searchItems[i].Name, searchItems[i].Name, searchItems[i].Quantity, searchItems[i].Description, status, function(tx,result){});
                                                         appendSearchResults(searchItems[i].ItemId, searchItems[i].Name, searchItems[i].Quantity, searchItems[i].Description, status);
                                                }else{
                                                         saveSearchResult(searchItems[i].ItemId, searchItems[i].Name, searchItems[i].Name, searchItems[i].Quantity, searchItems[i].Description, "0", function(tx,result){});
                                                         appendSearchResults(searchItems[i].ItemId, searchItems[i].Name, searchItems[i].Quantity, searchItems[i].Description, "0");
                                                }
                                                next(i,appendSearchResults,searchItems,appendLast,enableSearch);
                                                         
                                });
                            }else{
                               saveSearchResult(searchItems[i].ItemId, results.rows.item(0).itemname, searchItems[i].Name,results.rows.item(0).quantity, results.rows.item(0).description, status, function(tx,result){console.log("success");});
                               appendSearchResults(searchItems[i].ItemId, results.rows.item(0).itemname, results.rows.item(0).quantity, results.rows.item(0).description, status);
                               next(i,appendSearchResults,searchItems,appendLast,enableSearch);
                            }
                            
                        
                            
                        }else{
                        saveSearchResult(searchItems[i].ItemId, searchItems[i].Name, searchItems[i].Name, searchItems[i].Quantity, searchItems[i].Description, status, function(tx,result){});
                        appendSearchResults(searchItems[i].ItemId, searchItems[i].Name, searchItems[i].Quantity, searchItems[i].Description, status);
                        next(i,appendSearchResults,searchItems,appendLast,enableSearch);
                        }
                        
                        });
}


function next(i,appendSearchResults,searchItems,appendLast,enableSearch){
    if(i==searchItems.length-1){
        appendLast();
        isSearching = false;
        enableSearch();
    }
    if(i<searchItems.length){
        displaySearchResults(i+1,searchItems, appendSearchResults, appendLast,enableSearch);
    }
}

