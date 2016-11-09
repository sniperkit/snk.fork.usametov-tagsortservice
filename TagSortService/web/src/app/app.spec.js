
describe('TagBundleUtilTest', function () {

    describe('Frequent Tags tests', function () {
        //	scope holds	items	on	the	controller 
        var scope = {};
        
        //var $window;

        beforeEach(function () {
            module('TagBundleUtil');
            
            scope.buffer_size = 1000;     
            var mocked_state_transitions = new Map();
            mocked_state_transitions.set('#tagBundleList', { "39": ["topTags", "freqTags"] });
            mocked_state_transitions.set('#freqTagsList', { '37': ["freqTags", "topTags"], '39': ["freqTags", "exclTags"] });
            mocked_state_transitions.set('#excludeTagsList', { '37': ["exclTags", "freqTags"] });
            scope.states_transition_matrix = mocked_state_transitions;
           
            //set states transition table in $window service, it will be used inside SetStateTranstions
            //states_dict.set('#topTagsList', { '39': ['topTags', 'associatedTags'] });
            //states_dict.set('#associatedTagsList', { '37': ['associatedTags', 'topTags'], '39': ['associatedTags', 'exclTags'] });
            //states_dict.set('#excludeTagsList', { '37': ['exclTags', 'associatedTags'] });

            scope.existingTagBundles = [
                { Id : "571db1d1083989dcf1e6e923", Name : "webdev" },
                { Id : "571da189083989dcf1e6e920", Name : "tools" },
                { Id : "571db019083989dcf1e6e921", Name : "cryptography" },
                { Id : "571db10d083989dcf1e6e922", Name : "moocs" },
                { Id : "571db2a3083989dcf1e6e924", Name : "virtualization" },
                { Id : "571db334083989dcf1e6e925", Name : "communication" },
                { Id : "571db452083989dcf1e6e926", Name : "sourcecode" },
                { Id : "571db58d083989dcf1e6e927", Name : "video" },
                { Id : "571db629083989dcf1e6e928", Name : "cryptocurrencies" },
                { Id : "571db6d5083989dcf1e6e929", Name : "linux" },
                { Id : "571db75a083989dcf1e6e92a", Name : "books" },
                { Id : "571db82c083989dcf1e6e92d", Name : "computer-networks" },
                { Id : "571dbbd6083989dcf1e6e92e", Name : "diy" },
                { Id : "571dbc70083989dcf1e6e92f", Name : "machine-learning" },
                { Id : "571dbd55083989dcf1e6e930", Name : "android" },
                { Id : "571dbfec083989dcf1e6e931", Name : "security" },
                { Id: "571dc0c5083989dcf1e6e932",  Name: "mstech" }];
                        
            //scope.bookmarksCollectionId = "57e13f7e84e39a17d49bb198";
            
            //scope.selectedTagBundle = {
            //    Id: "580f053784e39a3724a7ad13",
            //    Name: "trading",
            //    Tags: ["test1","test2"],
            //    ExcludeTags: ["xTest1","xTest2"]
            //};

            //scope.selectedTagBundleId = "580f053784e39a3724a7ad13";

            var location = {
                search: function () {
                    return {
                        'tagBundle': '580f053784e39a3724a7ad13',
                        'bookmarksCollectionId': '57e13f7e84e39a17d49bb198'
                    };
                }
            };

            inject(function ($controller) {
                $controller('tagBundleCtrl', { $scope: scope, $location: location });
            });

        });
        
        //start InitPage assertions
        //it('should define states_transition_matrix', function () {
            
        //    expect(scope.states_transition_matrix).toBeDefined();
        //});

        //it('should define buffer size', function () {

        //    expect(scope.buffer_size).toBeDefined();
        //});

        //it('should define existingTagBundles', function () {

        //    expect(scope.existingTagBundles).toBeDefined();
        //});

        it('should define bookmarksCollectionId', function () {

            spyOn(location, 'search').and.returnValue({
                'tagBundle': '580f053784e39a3724a7ad13',
                'bookmarksCollectionId': '57e13f7e84e39a17d49bb198'
            });

            expect(scope.GetBookmarksCollection()).toEqual('57e13f7e84e39a17d49bb198');
        });

        it('should define selectedTagBundleId', function () {

            spyOn(location, 'search').and.returnValue({
                'tagBundle': '580f053784e39a3724a7ad13',
                'bookmarksCollectionId': '57e13f7e84e39a17d49bb198'
            });

            expect(scope.GetSlctdTagBundleId()).toEqual('580f053784e39a3724a7ad13');
        });
        //end InitPage assertions

        //test moves here
        //var tagBundleCtrl, $location, $scope;
        var arrSrc = [{
            Count: 8,
            Tag: "test1_src"
        },
        {
            Count: 7,
            Tag: "test2_src"
        }];

        var arrTrg = [{ Count: -1, Tag: "_xTst1" }, { Count: -1, Tag: "_xTst2" }];

        it('should move from src to trg array and select next option', function () {
            var result = scope.move(arrSrc, arrTrg, "test1_src");
            expect(result.arrSrc.length).toEqual(1);
            expect(result.val2focus.Tag).toEqual("test2_src");
            console.log(result);
        });

        it('should move from src to trg array and select previous option', function () {
            var result = scope.move(arrSrc, arrTrg, "test2_src");
            expect(result.arrSrc.length).toEqual(1);
            expect(result.val2focus.Tag).toEqual("test1_src");
            //console.log(result);
        });

        it('should remove value from src and add to trg array ', function () {
            var result = scope.move(arrSrc, arrTrg, "test2_src");
            expect(result.arrSrc.length).toEqual(1);
            expect(result.arrTrg.length).toEqual(3);
            expect(result.arrTrg[result.arrTrg.length-1].Tag).toEqual("test2_src");
            //console.log(result.arrTrg);
        });

    });
});
