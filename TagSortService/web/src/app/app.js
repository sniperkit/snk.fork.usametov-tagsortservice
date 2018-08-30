/*
Sniperkit-Bot
- Status: analyzed
*/


var tagBundleModule = angular.module("TagBundleUtil", []).controller
    ("tagBundleCtrl", ['$scope', '$location', '$window', 'tagRepository'
    , function ($scope, $location, $window, tagRepository) {
        
        $scope.states_transition_matrix = {};
        $scope.buffer_size = $scope.buffer_size ? $scope.buffer_size : 1000;

        var tagMover = function (x) {
            //src array and target array names are specified in states transition matrix
            var src_trg_arr = $scope.states_transition_matrix.get(x.srcId)[x.keyCode];            
            
            $scope[src_trg_arr[1]] = getSafeArray($scope[src_trg_arr[1]]);
            $scope[src_trg_arr[0]] = getSafeArray($scope[src_trg_arr[0]]);

            var res = $scope.move($scope[src_trg_arr[0]], $scope[src_trg_arr[1]], x.slctValue);
            
            $scope[src_trg_arr[0]] = res.arrSrc;
            $scope[src_trg_arr[1]] = res.arrTrg;

            $scope.$apply();

            if(res.val2focus)
                angular.element(x.srcId).val(res.val2focus.Tag);
        }

        var getSafeArray = function(arr){
            if(typeof arr === 'undefined')
                return [];
            else 
                return arr;
        }

        // move val from src array to target array returning next value in src array to focus
        $scope.move = function (arrSrc, arrTrg, val) {
            
            var nextVal2focus = getNextVal(arrSrc, val);            
            var newSrc = arrSrc.filter(t=> t.Tag != val);            
            var termCount = arrSrc.filter(t=> t.Tag == val);
            //and add it to target array            
            arrTrg = termCount.concat(arrTrg);            
            
            return { arrSrc: newSrc, arrTrg: arrTrg, val2focus: nextVal2focus };
        }

        //get index of value in src array, then use this index to get next value
        var getNextVal = function (arrSrc, term) {
            var fndIdx = getIdx(arrSrc, term);
            var result = fndIdx > 0 ? arrSrc[fndIdx - 1] :
                    arrSrc.length > 1 ? arrSrc[fndIdx + 1] : null;

            return result;
        }

        var getIdx = function (arrSrc, val) {
            return arrSrc.findIndex(t=> t.Tag == val);
        }

        //hook tagmover routine to element's keyup event 
        var arrowKeyHandler = function (listIdSelector) {
            var srcList = $(listIdSelector);

            var arrowKeyUp = Rx.Observable.fromEvent(srcList, 'keyup')
                                          .filter(e=>e.keyCode == 37 || e.keyCode == 39)
                                          .map(function (e) {
                                              return {
                                                  keyCode: e.keyCode
                                                  , srcId: listIdSelector
                                                  , slctValue: srcList.val()
                                              };
                                          });

            arrowKeyUp.subscribe(tagMover);
        }
           
        //state transitions are provided by $window service
        //keys in state transtions dictionary represent element ids (see select boxes below) 
        //to which we hook our keyup event handlers
        $scope.SetStateTranstions = function () {
            $scope.states_transition_matrix = $window.states_dict;
            $scope.arrowKeySrcSelectors = Array.from($scope.states_transition_matrix.keys());
            //wire arrowKeyHandler to select boxes
            angular.forEach($scope.arrowKeySrcSelectors, function (selector) { arrowKeyHandler(selector); });
        }

        $scope.SaveTagBundleAndExcludeList = function () {
            resolvePromise(tagRepository.saveTagBundle
                            ($scope.selectedTagBundleId, 
                             $scope.topTags,
                             $scope.exclTags,
                             $scope.exclTagBundles.split(','))
                           , function (response) {
                               console.log("SaveTagBundleAndExcludeList, response status", response.status);
                           });
        }
              
        //this will be called for newly created tag bundles
        $scope.saveExcludeList = function () {
            resolvePromise(tagRepository.saveExcludeList($scope.selectedTagBundleId,$scope.exclTags)
                           , function (response) {
                               console.log("saveExcludeList, response status", response.status);
                           });
        }
    
        $scope.addEditTagBundleName = function(){
            if(typeof $scope.selectedTagBundleId === 'undefined'
                || $scope.selectedTagBundleId == 'new')
            {
                resolvePromise(tagRepository.createTagBundle
                                            ($scope.newTagBundleName, $scope.bookmarksCollectionId)
                               , function (response) {
                                   console.log("edit TagBundle Name, response status", response.status);
                                   $scope.ReloadPage();
                               });
            }
            else{
                resolvePromise(tagRepository.saveTagBundleName($scope.selectedTagBundleId,$scope.newTagBundleName)
                               , function (response) {
                                   console.log("saveTagBundleName, response status", response.status);
                               });
            }
        }

        var resolvePromise = function (promise, successFn) {
            Rx.Observable.fromPromise(promise)
                        .subscribe(successFn, function (err) {
                            console.log('Error: %s', err);
                        }
                        , null);
        }

        $scope.SetMostFrequentTags = function () {
        
            var bundleId = $scope.GetSlctdTagBundleId();
            //console.log("$scope.exclTagBundles",$scope.exclTagBundles);

            var promise = tagRepository.getMostFrequentTags(bundleId, $scope.exclTagBundles, $scope.buffer_size);
                
            resolvePromise(promise, function (response) {
                $scope.freqTags = response.data; 
                console.log("freq tags",response.data);
                $scope.$apply();
            });
       
        }
   
        $scope.SetTagAssociations = function () {
            
            var bundleId = $scope.GetSlctdTagBundleId();
            var promise = tagRepository.getTagAssociations(bundleId, $scope.buffer_size);
        
            resolvePromise(promise, function (response) {
                //$scope.associatedTags = response.data;
                $scope.freqTags = response.data;//using same array for associated terms
                console.log("associated tags", response.data);
                $scope.$apply();
            });
        }
    
        $scope.ReloadPage = function (selectedTagBundleId) {
            //set url    
            if(selectedTagBundleId){
                $location.search({ tagBundle: selectedTagBundleId
                , bookmarksCollectionId : $scope.GetBookmarksCollection() });
            }
            else{
                $location.search({ bookmarksCollectionId : $scope.GetBookmarksCollection() });
            }
                        
            $window.location.reload();
        }

        $scope.GetSlctdTagBundleId = function (bundleId) {
            
            return $location.search()['tagBundle'] 
                                        ? $location.search()['tagBundle']
                                        : bundleId;        
        }

        $scope.GetBookmarksCollection = function () {
            
            return $location.search()['bookmarksCollectionId'];        
        }

        $scope.InitPage = function (funcArray) {

            var bookmarksCollectionId = $scope.GetBookmarksCollection();
            if(typeof bookmarksCollectionId === "undefined")
            {
                alert("bookmarksCollectionId is undefined!");
                return;
            }

            var promise = tagRepository.getTagBundles(bookmarksCollectionId);

            resolvePromise(promise, function (response) {
                $scope.existingTagBundles = response.data;
                console.log("tag bundles", response.data);
                var slctBundleId = $scope.GetSlctdTagBundleId(response.data ? response.data[0].Id : null);
                angular.forEach(funcArray, function (func)
                {
                    func(slctBundleId);
                });
            
                $scope.selectedTagBundleId = slctBundleId;
                $scope.bookmarksCollectionId = bookmarksCollectionId;
            
                $scope.$apply();
            });
        }
                
        $scope.SetTagBundle = function (bundleId) {
        
            var promise = tagRepository.getTagBundleById(bundleId);

            resolvePromise(promise, function (response) {
                $scope.topTags = response.data.Tags;
                $scope.exclTags = response.data.ExcludeTags;
                $scope.exclTagBundles = response.data.ExcludeTagBundles.join(',');
                console.log("scope", $scope);
                $scope.$apply();
            });
        }
                
        $scope.InitFreqTagsModel = function () {
            $scope.SetStateTranstions();
            $scope.InitPage
            ([
                $scope.SetTagBundle                        
             ]
            );        
        }

        $scope.InitAddEditTagBundle = function () {        
            $scope.InitPage([]); 
        }

    }]).factory("tagRepository", ['$http', function ($http) {
       
        $http.defaults.headers.common = {};
        $http.defaults.headers.common["Content-Type"] = "application/json";
        $http.defaults.headers.common['Authorization'] = 'dWthdGF5OmJvbGF2ZXJzb24=';

        $http.defaults.headers.post = {};
        $http.defaults.headers.put = {};
        $http.defaults.headers.patch = {};

        var baseUrl = "http://localhost:61091/BookmarkCollectionRepository.svc/";
        
        var getTagBundleById = function (bundleId) {
            //console.log("get tagBundleById", bundleId);

            var promise = $http({
                url: baseUrl+"tagBundle/?id=" + bundleId
                , method: "GET"
                , withCredentials: true
            });

            return promise;
        };

        var getTagBundles = function (bookmarksCollectionId) {
            var promise = $http({
                url: baseUrl+"tagBundleNames?bookmarksCollectionId="+bookmarksCollectionId,
                method: "GET"
                //,headers: headers   
            });

            return promise;
        };

        var getMostFrequentTags = function (bundleId, excludeTagBundleNames, bufferSize) {
                            
            var xTagBundles = excludeTagBundleNames ? excludeTagBundleNames : "";             
            var url = baseUrl+
                    "NextMostFrequentTags/?tagBundleId=" + bundleId +
                    "&excludeTagBundleNames=" + xTagBundles +
                    "&limit="+bufferSize;
            
            var promise = $http({
                url: url,
                method: "GET"
            });

            return promise;                
        };

        var getTagAssociations = function (tagBundleId, bufferSize) {
            var promise = $http({ 
                url: baseUrl+"AssociatedTerms/?tagBundleId="+tagBundleId+"&bufferSize="+bufferSize,
                method: "GET"
            });

            return promise;                
        }
        
        var saveExcludeList = function (tagBundleId, exclTags) {

            console.log('tagBundle in saving exclTags', tagBundleId);
            var promise = $http({
                url: baseUrl+"tagBundle/UpdateExcludeList",
                method: "POST",
                data:
               {
                   "Id": tagBundleId                 
                 , "ExcludeTags": exclTags
               }
            });

            return promise;
        }
        
        var saveTagBundle = function (tagBundleId, topTags, exclTags, exclTagBundles) {
                
            var tagBundle = {
                "tagBundle":
                   {
                       "Id": tagBundleId
                     , "Tags": topTags
                     , "ExcludeTags": exclTags     
                     , "ExcludeTagBundles": exclTagBundles  
                   }
            };

            console.log('tagBundle to save', tagBundle);

            var promise = $http({
                url: baseUrl + "tagBundle/updateById",
                method: "POST",
                data: tagBundle
            });

            return promise;
        }
        
        var createTagBundle = function (tagBundleName, bookmarksCollectionId) {
            
            var tagBundle = {
                "tagBundle":
                   {
                       "Name": tagBundleName
                     , "BookmarksCollectionId": bookmarksCollectionId                     
                   }
            };

            var promise = $http({
                url: baseUrl + "tagBundle/create",
                method: "POST",
                data: tagBundle
            });

            return promise;
        }

        var saveTagBundleName = function(bundleId, newTagBundleName) {
            
            var tagBundle = {
                "tagBundle":
                   {                       
                       "Name": newTagBundleName
                     , "Id": bundleId
                   }
            };

            var promise = $http({
                url: baseUrl + "tagBundle/editName",
                method: "POST",
                data: tagBundle
            });

            return promise;
        }

        var tagService = {            
            getTagBundleById: getTagBundleById,
            getTagBundles: getTagBundles,
            getMostFrequentTags: getMostFrequentTags,
            getTagAssociations: getTagAssociations,                        
            saveTagBundle: saveTagBundle,
            saveExcludeList: saveExcludeList,
            createTagBundle: createTagBundle,
            saveTagBundleName: saveTagBundleName
        };
        
        return tagService;
    }]);

tagBundleModule.controller("bookmarksCtrl", ['$scope', 'bookmarkRepository', function ($scope, bookmarkRepository) {

    var resolvePromise = function (promise, successFn) {
        Rx.Observable.fromPromise(promise)
                    .subscribe(successFn, function (err) {
                        console.log('Error: %s', err);
                    }
                    , null);
    }

    var promise = bookmarkRepository.getBookmarkCollections();

    resolvePromise(promise, function (response) {
        
        $scope.bookmarkCollections = response.data;       
        console.log("bookmarkCollections", response.data);       

        $scope.bookmarksCollectionId = response.data[0].Id;

        $scope.$apply();
    });

    $scope.ReloadBookmarkCollection = function(bookmarksCollectionId){

        console.log("bookmarksCollectionId", bookmarksCollectionId);

        if(bookmarksCollectionId){
            $location.search({ bookmarksCollectionId :bookmarksCollectionId });
            $window.location.reload();
        }                                        
    }

}]).factory('bookmarkRepository', ['$http', function ($http) {

    $http.defaults.headers.common = {};
    $http.defaults.headers.common["Content-Type"] = "application/json";
    $http.defaults.headers.common['Authorization'] = 'dWthdGF5OmJvbGF2ZXJzb24=';

    $http.defaults.headers.post = {};
    $http.defaults.headers.put = {};
    $http.defaults.headers.patch = {};

    var baseUrl = "http://localhost:61091/BookmarkCollectionRepository.svc/";

    var getBookmarkCollections = function () {

        var promise = $http({
            url: baseUrl + "bookmarkCollections/",
            method: "GET"
        });

        return promise;
    }

    var bookmarkService = {
        getBookmarkCollections: getBookmarkCollections        
    }

    return bookmarkService;

}]).config(['$httpProvider', function($httpProvider) {
    
    $httpProvider.defaults.headers.common = {};
    $httpProvider.defaults.headers.post = {};
    $httpProvider.defaults.headers.put = {};
    $httpProvider.defaults.headers.patch = {};
}]);

