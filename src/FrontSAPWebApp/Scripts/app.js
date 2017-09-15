/// <reference path="https://ajax.googleapis.com/ajax/libs/angularjs/1.6.5/angular.js" />

var valuesApp = angular.module('valuesApp', ['AdalAngular']);
Logging = {
    level: 3,
    log: function (message) {
        console.log(message);
    }
};
valuesApp.config(['$locationProvider', '$httpProvider', 'adalAuthenticationServiceProvider',
    function ($locationProvider, $httpProvider, adalProvider) {
        var endpoints = {
            "http://localhost:57042": "https://demoappsad.onmicrosoft.com/BackEndApiService"
            //Servidor
        };
        adalProvider.init({
            instance: 'https://login.microsoftonline.com/',
            tenant: '{yourtenant}.onmicrosoft.com',
            clientId: '587f306c-c43d-4172-93f3-77b6e17d1bd6', //Cliente
            endpoints: endpoints
        }, $httpProvider);
        $locationProvider.html5Mode(true);
    }]);



var valuesController = valuesApp.controller('valuesController', [
    '$scope', '$http', 'adalAuthenticationService',
    function ($scope, $http, adalService) {
        $scope.login = function () {
            adalService.login();
        }
        $scope.logout = function () {
            adalService.logOut();
        }

        $scope.getControllervalue = function () {
            $http.get("http://localhost:57042/api/Values/1").then(function (result) {
                $scope.backmodel = result.data;
            });
        }

    }]);
