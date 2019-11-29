//namespace qLocalize {
//    var _appName = undefined;
//    var _syncCallback = {};
//    export function setApp(appName: string) {
//        if (!appName) {
//            throw ("appName can not be empty");
//        }
//        _appName = appName;
//    }
//    export function onSyncData(cb: () => void) {

//    }
//    export interface IRegLang {
//        res: (key: string) => string
//        appRes: (key: string) => string
//        gRes: (key: string) => string
//    }
//    class regLang implements IRegLang {
//        scope: any;
//        appName: any;
//        _lang: any;
//        constructor(scope) {
//            this.scope = scope;
//            this.appName = _appName;
//            this._lang = {};
//        }
//        res(value: string): any {
//            return {
//                appName: this.appName,
//                pageId: this.scope.$$urlInfo.relUrl.value,
//                value: value
//            };

//        }
//        appRes(value: string): any {
//            return {
//                appName: this.appName,
//                pageId: "-",
//                value: value
//            };
//        }
//        gRes(value: string): any {
//            return {
//                appName: "-",
//                pageId: "-",
//                value: value
//            };
//        }


//    }
//    export function apply<T, T1>(langCode: string, instance: T, regFn: (reg: IRegLang) => T1, cb?: (x: T & { $$lang: Partial<T1> }) => void): T & { $$lang: Partial<T1> } {
//        var localizePath = instance["$$urlInfo"].absUrl.value + `.localize.${langCode}.json`;
//        var existingData = {};
//        $.getJSON(localizePath, (data) => {
//            existingData = angular.merge(existingData, data)

//        });
//        if (!_appName) {
//            console.error("It looks like you forgot call qLocalize.setApp")
//        }
//        var regLangFn = new regLang(instance);
//        var data = regFn(regLangFn);
//        var keys = Object.keys(data);
//        instance["$$lang"] = {};
//        var syncLang = [];
//        for (var i = 0; i < keys.length; i++) {
//            instance["$$lang"][keys[i]] = data[keys[i]]["value"];
//            syncLang.push({
//                key: keys[i],
//                appName: data[keys[i]]["appName"],
//                pageId: data[keys[i]]["pageId"],
//                value: data[keys[i]]["value"]
//            });
//        }
//        $.getJSON(localizePath, (data) => {
//            instance["$$lang"] = angular.merge(instance["$$lang"], data)
//            instance["$$lang"].applyAsync();
//        });
//        console.log(syncLang);
//        return instance as T & { $$lang: Partial<T1> };
//    }
//}



