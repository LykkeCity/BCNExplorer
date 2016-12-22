'use strict';

angular.module('app', [])
    .factory('assetService', function ($http) {
        var result = {
            async: function () {
                // $http returns a promise, which has a then function, which also returns a promise
                var promise = $http.get('/api/assets').then(function (response) {
                    console.log(response);
                    return response.data;
                });
                return promise;
            }
        };
        return result;
    })
    .controller('DirectoryCtrl', function ($scope, assetService) {
        $scope.loading = true;
        assetService.async().then(function(data) {
            $scope.assets = data;
            $scope.loading = false;
        });
    });