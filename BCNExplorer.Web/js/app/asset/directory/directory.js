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
        detailsUrl: '/asset/'
    })
    .controller('DirectoryCtrl', function ($scope, assetService, config) {
            var pagination = {
                page: 0,
                allItems:[],
                pagedItemsCount: 0,
                setPage: function(page) {
                    pagination.page = page;
                },
                resetToDefault: function() {
                    pagination.setPage(1);
                },
                next: function () {
                    console.log('next');
                    pagination.page++;
                },
                start: function() {
                    pagination.resetToDefault();
                },
                showNextBtn: function () {
                    return true;
                }
            };

            $scope.$watch('pagination.page', function () {
                pagination.pagedItemsCount = pagination.allItems.slice().splice(0, (pagination.page) * config.pageSize).length;
            });

            $scope.loading = true;
            $scope.pagination = pagination;

            $scope.detailsUrl = function(asset) {
                return config.detailsUrl + asset.AssetIds[0];
            }

            assetService.async().then(function (data) {
                pagination.allItems = data;
                pagination.start();
                $scope.loading = false;
            });

    })

;