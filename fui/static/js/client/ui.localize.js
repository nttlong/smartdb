var qLocalize = {
    _hasSearchByDefaultValues: {
        gRes: {},
        appRes: {},
        res: {}
    },
    _appRes: {},
    _gRes: undefined,
    _appName: undefined,
    _onSyncData: undefined,
    _languageCode: undefined,
    _tenancy: undefined,
    _langData: undefined,
    _hostDir: undefined,
    _dateFormat: undefined,
    getDateFormat: function () {
        if (!qLocalize._dateFormat) {
            throw "It looks like you forgot call qLocalize.setDateFormat";
        }
        return qLocalize._dateFormat;
    },
    setDateFormat: function (value) {
        qLocalize._dateFormat = value;
    },
    createLangReader: function (scope) {
        function ret(scope) {
            this.scope = scope;
            this.appName = qLocalize._appName;
            this._lang = {};
        }
        ret.prototype.res = function (value) {
            var pageId = "-";
            if (this.scope.$$urlInfo) {
                pageId = this.scope.$$urlInfo.relUrl.value;
            }
            if ((qLocalize._tenancy !== null) && (qLocalize._tenancy !== "")) {
                pageId = pageId.substring(qLocalize._tenancy.length, pageId.length);
            }
            pageId = pageId.substring(qLocalize._appName.length, pageId.length);
            console.log(pageId);
            return {
                appName: this.appName,
                pageId: pageId,
                value: value
            };
        };
        ret.prototype.appRes = function (value) {
            return {
                appName: this.appName,
                pageId: '-',
                value: value
            };
        };

        ret.prototype.gRes = function (value) {
            return {
                appName: '-',
                pageId: '-',
                value: value
            };
        };
        return new ret(scope);
    },
    __loadResource: function () {
        if (qLocalize._appRes[qLocalize._appName] === undefined) {
            var url = window.location.protocol + "//" + window.location.host;
            if ((qLocalize._tenancy !== undefined) &&
                (qLocalize._tenancy !== undefined) &&
                (qLocalize._languageCode !== undefined)) {
                
                if (qLocalize._tenancy === "" || qLocalize._tenancy === null) {
                    url += "/default";
                }
                else {
                    url += "/apps";
                }
                url += "/" + qLocalize._appName + "/i18n/" + qLocalize._languageCode + ".json";
                $.getJSON(url, function (r) {
                    qLocalize._appRes[qLocalize._appName] = r;

                });
                if (qLocalize._gRes === undefined) {
                    $.getJSON(window.location.protocol + "//" + window.location.host + "/apps/i18n/" + qLocalize._languageCode + ".json", function (res) {
                        qLocalize._gRes = res;
                    });
                }
            }
        }
    },
    setHostDir: function (hostDir) {
        qLocalize._hostDir = hostDir;
    },
    setApp: function (appName) {
        qLocalize._appName = appName;
        qLocalize.__loadResource();
    },
    setTenancy: function (tenancy) {
        qLocalize._tenancy = tenancy;
        qLocalize.__loadResource();
    },
    setLanguageCode: function (name) {
        qLocalize._languageCode = name;
        qLocalize.__loadResource();
    },
    getLanguageCode: function () {
        return qLocalize._languageCode;
    },
    onSyncData: function (cb) {
        qLocalize._onSyncData = cb;
    },
    fixKey: function (k) {
        if (k === undefined) return undefined;
        if (k === null) return k;
        k = k.toLowerCase();
        while (k.substring(0, 1) === " ") k = k.substring(1, k.length);
        while (k.substring(k.length - 1, k.length) === " ") k = k.substring(0, k.length - 1);
        while (k.indexOf("  ") > -1) k = k.replace("  ", " ");
        return k;
    },
    loadLocalize: function (data) {
        function fxiKey(k) {
            if (k === undefined) {
                return k;
            }
            if (k === null) {
                return k;
            }
            k = k.toLowerCase();
            while (k.substring(0, 1) === " ") k = k.substring(1, k.length);
            while (k.substring(k.length - 1, k.length) === " ") k = k.substring(0, k.length - 1);
            while (k.indexOf("  ") > -1) k = k.replace("  ", " ");
            return k;
        }
        if (qLocalize._appRes[qLocalize._appName]) {
            var __keys = Object.keys(qLocalize._appRes[qLocalize._appName]);
            for (var i = 0; i < __keys.length; i++) {
                data.app[__keys[i]] = qLocalize._appRes[qLocalize._appName][__keys[i]].Translate || qLocalize._appRes[qLocalize._appName][__keys[i]].translate;
                var k = fxiKey(qLocalize._appRes[qLocalize._appName][__keys[i]].Default || [qLocalize._appName][__keys[i]].default);
                qLocalize._hasSearchByDefaultValues.appRes[k] = qLocalize._appRes[qLocalize._appName][__keys[i]].Translate || qLocalize._appRes[qLocalize._appName][__keys[i]].translate;
            }
        }
        if (qLocalize._gRes) {
            __keys = Object.keys(qLocalize._gRes);
            for (i = 0; i < __keys.length; i++) {
                data.global[__keys[i]] = qLocalize._gRes[__keys[i]].Translate || qLocalize._gRes[__keys[i]].translate;
                var k = fxiKey(qLocalize._gRes[__keys[i]].Default || qLocalize._gRes[__keys[i]].default);
                qLocalize._hasSearchByDefaultValues.appRes[k] = qLocalize._gRes[__keys[i]].Translate || qLocalize._gRes[__keys[i]].translate;
            }
        }
        console.log(qLocalize._hasSearchByDefaultValues);
    },
    apply: function (scope, regFn) {
        if (!qLocalize._languageCode) {
            throw "It looks like you forgot call qLocalize.setLanguageCode";
        }
        if (qLocalize._tenancy === undefined) {
            throw "It looks like you forgot call qLocalize.setTenancy";
        }
        if (scope["$$urlInfo"]) {
            var localizePath = scope["$$urlInfo"].absUrl.value + ".localize." + qLocalize._languageCode + ".json";
        }
        var regLangFn = qLocalize.createLangReader(scope);
        var data = regFn(regLangFn);
        var keys = Object.keys(data);
        scope["$$lang"] = {};
        scope.$localize = {
            app: {},
            global: {},
            res: {}
        };
        var syncLang = [];
        for (var i = 0; i < keys.length; i++) {
            if (data[keys[i]].appName === "-") {
                scope.$localize.global[keys[i]] = data[keys[i]]["value"];
            }
            else if (data[keys[i]].pageId === "-") {

                scope.$localize.app[keys[i]] = data[keys[i]]["value"];
            }
            else {
                scope.$localize.res[keys[i]] = data[keys[i]]["value"];
            }

            scope["$$lang"][keys[i]] = data[keys[i]]["value"];
            syncLang.push({
                key: keys[i],
                appName: data[keys[i]]["appName"],
                pageId: data[keys[i]]["pageId"],
                value: data[keys[i]]["value"],
                langCode: qLocalize._languageCode
            });
        }
        var sender = {
            data: syncLang,
            scope: scope,
            done: function (resData) {
                console.log(resData);
            }
        };
        qLocalize._onSyncData(sender);
        $.getJSON(localizePath, (data) => {
            scope["$$lang"] = angular.merge(instance["$$lang"], data);
            scope["$$lang"].applyAsync();
        });
        qLocalize.loadLocalize(scope.$localize); 
        return scope;
    },
    register: function (scope, data) {
        
        var urlLangOfPage = scope.$$urlInfo.relUrl.value;
        while (urlLangOfPage.substring(0, 1) === '/') {
            urlLangOfPage = urlLangOfPage.substring(1, urlLangOfPage.length);
        }
        if (qLocalize._tenancy !== null && qLocalize._tenancy !== "" && qLocalize._tenancy !== undefined) {
            urlLangOfPage = urlLangOfPage.substring(qLocalize._tenancy.length+1, urlLangOfPage.length);
        }
        if (qLocalize._hostDir !== null && qLocalize._hostDir !== "" && qLocalize._hostDir !== undefined) {
            urlLangOfPage = urlLangOfPage.substring(qLocalize._hostDir.length + 1, urlLangOfPage.length);
        }
        
        var urlGetJSONLocalize = window.location.protocol + "//" + window.location.host;
        if (qLocalize._tenancy !== null && qLocalize._tenancy !== "" && qLocalize._tenancy !== undefined) {
            urlGetJSONLocalize += "/" + qLocalize._tenancy;
        }
        urlGetJSONLocalize += "/i18n";
        urlGetJSONLocalize += "/" + urlLangOfPage + "." + qLocalize._languageCode + ".json";
       
        

        if (!qLocalize._languageCode) {
            throw "It looks like you forgot call qLocalize.setLanguageCode";
        }
        if (qLocalize._tenancy === undefined) {
            throw "It looks like you forgot call qLocalize.setTenancy";
        }
        if (scope["$$urlInfo"]) {
            var localizePath = scope["$$urlInfo"].absUrl.value + ".localize." + qLocalize._languageCode + ".json";
        }
        var regLangFn = qLocalize.createLangReader(scope);
        var keys = Object.keys(data);

        scope.$localize = {
            app: {},
            global: {},
            res: {}
        };
        $.getJSON(scope.$$urlInfo.absUrl.value + "." + qLocalize._languageCode + ".json", (data) => {
            var keys = Object.keys(data);
            qLocalize._hasSearchByDefaultValues.res[scope.$$urlInfo.relUrl.value] = {};
            for (var i = 0; i < keys.length; i++) {
                scope.$localize.res[keys[i]] = data[keys[i]].Translate || data[keys[i]].translate;
                var k = data[keys[i]].Default || data[keys[i]].default;
                qLocalize._hasSearchByDefaultValues.res[scope.$$urlInfo.relUrl.value][qLocalize.fixKey(k)] = data[keys[i]].Translate || data[keys[i]].translate;
            }
            console.log(qLocalize._hasSearchByDefaultValues);
            //scope.$localize.res = angular.merge(scope.$localize.res, data);
            scope.$applyAsync();
        });
        var syncLang = [];
        var _data = data.res || {};
        var _keys = Object.keys(_data);
        var pageId = "-";
        if (scope.$$urlInfo) {
            pageId = scope.$$urlInfo.relUrl.value;
        }
        var i = 0;
        for (i = 0; i < _keys.length; i++) {
            while (pageId.substring(0, 1) === '/') {
                pageId = pageId.substring(1, pageId.length);
            }
            if (qLocalize._tenancy !== "" && qLocalize._tenancy !== undefined && qLocalize._tenancy !== null) {
                pageId = pageId.substring(qLocalize._tenancy.length+1, pageId.length);
            }
            pageId = pageId.substring(qLocalize._appName.length + 1, pageId.length);
            scope.$localize.res[_keys[i]] = _data[_keys[i]];
            syncLang.push({
                key: _keys[i],
                appName: qLocalize._appName,
                pageId: pageId,
                value: _data[_keys[i]],
                langCode: qLocalize._languageCode
            });
        }
        _data = data.appRes || {};
        _keys = Object.keys(_data);
        for (i = 0; i < _keys.length; i++) {
            scope.$localize.app[_keys[i]] = _data[_keys[i]];
            syncLang.push({
                key: _keys[i],
                appName: qLocalize._appName,
                pageId: "-",
                value: _data[_keys[i]],
                langCode: qLocalize._languageCode
            });
        }
        _data = data.gRes || {};
        _keys = Object.keys(_data);
        for (i = 0; i < _keys.length; i++) {
            scope.$localize.global[_keys[i]] = _data[_keys[i]];
            syncLang.push({
                key: _keys[i],
                appName: "-",
                pageId: "-",
                value: _data[_keys[i]],
                langCode: qLocalize._languageCode
            });
        }

        var sender = {
            data: syncLang,
            scope: scope,
            done: function (resData) {
                console.log(resData);
            }
        };
        qLocalize._onSyncData(sender);
        qLocalize.loadLocalize(scope.$localize);
        scope.$res = function (value) {
           
            if (qLocalize._hasSearchByDefaultValues.res[scope.$$urlInfo.relUrl.value] &&
                qLocalize._hasSearchByDefaultValues.res[scope.$$urlInfo.relUrl.value][qLocalize.fixKey(value)]) {
                return qLocalize._hasSearchByDefaultValues.res[scope.$$urlInfo.relUrl.value][qLocalize.fixKey(value)];
            }
            if (qLocalize._hasSearchByDefaultValues.appRes[qLocalize.fixKey(value)]) {
                return qLocalize._hasSearchByDefaultValues.appRes[qLocalize.fixKey(value)];
            }
            if (qLocalize._hasSearchByDefaultValues.gRes[qLocalize.fixKey(value)]) {
                return qLocalize._hasSearchByDefaultValues.gRes[qLocalize.fixKey(value)];
            }
            return value;
        };
        scope.$appRes = function (value) {
            return scope.$res(value);
        }
        scope.$gRes = function (value) {
            return scope.$res(value);
        }
        //console.log(syncLang);
        return scope;
    }
}