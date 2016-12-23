'use strict';

angular.module('app', [])
    .factory('assetService', function ($http) {
        var result = {
            async: function () {
                var promise = $http.get('/api/assets').then(function (response) {
                    return response.data;
                });

                return promise;
            }
        };
        return result;
    })
    .constant('config', {
        pageSize: 20,
        detailsUrl: function(assetId) {
            return '/asset/' + assetId;
        }
    })
    .controller('DirectoryCtrl', function ($scope, assetService, config, $filter) {
            var assetList = {
                page: 0,
                allItems:[],
                pagedItemsCount: 0,
                setPage: function(page) {
                    assetList.page = page;
                },
                resetToDefault: function() {
                    assetList.setPage(1);
                },
                next: function () {
                    console.log('next');
                    assetList.page++;
                },
                start: function() {
                    assetList.resetToDefault();
                }
            };
            
            var dataProcessing = {
                filterData: function(items, searchQuery) {
                    return $filter('filter')(assetList.allItems, searchQuery);
                },
                pageData:function(items) {
                    return $filter('limitTo')(items, assetList.pagedItemsCount);
                }
            }

            $scope.assetsToShow = function () {
                return dataProcessing.pageData(dataProcessing.filterData(assetList.allItems, $scope.searchQuery));
            }

            $scope.$watch('assetList.page', function () {
                assetList.pagedItemsCount = assetList.allItems.slice().splice(0, (assetList.page) * config.pageSize).length;
            });

            $scope.loading = true;
            $scope.assetList = assetList;

            $scope.detailsUrl = function(asset) {
                return config.detailsUrl(asset.AssetIds[0]);
            }

            $scope.showNextBtn = function() {
                var filteredDataCount = dataProcessing.filterData(assetList.allItems, $scope.searchQuery).length;
                return assetList.pagedItemsCount < filteredDataCount;
            }

            assetService.async().then(function (data) {
                assetList.allItems = data;
                assetList.start();
                $scope.loading = false;
            });

    })

;