/// <reference path="js/client/types/require.d.ts" />
var clsLibPath = /** @class */ (function () {
    function clsLibPath(rPath) {
        this.rootPath = rPath;
    }
    clsLibPath.prototype.loadCss = function (cssLinks) {
        var me = this;
        cssLinks.forEach(function (item, index) {
            var link = document.createElement("link");
            link.type = "text/css";
            link.rel = "stylesheet";
            link.href = me.rootPath + "/" + item;
            document.getElementsByTagName("head")[0].appendChild(link);
        });
    };
    clsLibPath.prototype.load = function (libPath) {
        var ret = new clsLibPathThen();
        ret.rootUrl = this.rootPath;
        ret.load(libPath);
        return ret;
    };
    clsLibPath.prototype.toArray = function () {
        return this.listOfPaths;
    };
    clsLibPath.prototype.ref = function (paths) {
        var _this = this;
        this.paths = paths;
        this.listOfPaths = [];
        var me = this;
        this.paths.forEach(function (item, index, lst) {
            _this.listOfPaths.push(me.rootPath + "/" + item);
        });
        return this;
    };
    return clsLibPath;
}());
var clsLibPathThen = /** @class */ (function () {
    function clsLibPathThen() {
        this.urls = [];
    }
    clsLibPathThen.prototype.thenLoad = function (libs) {
        var lst = [];
        var me = this;
        libs.forEach(function (item, index) {
            lst.push(me.rootUrl + "/" + item);
        });
        this.urls.push(lst);
        return this;
    };
    clsLibPathThen.prototype.then = function (handler) {
        this._handler = handler;
        var me = this;
        function run(index, callback) {
            if (index >= me.urls.length) {
                callback();
            }
            else {
                requirejs(me.urls[index], function () {
                    run(index + 1, callback);
                });
            }
        }
        run(0, function () {
            handler();
        });
        return me;
    };
    clsLibPathThen.prototype.load = function (libs) {
        var lst = [];
        var me = this;
        libs.forEach(function (item, index) {
            lst.push(me.rootUrl + "/" + item);
        });
        this.urls.push(lst);
        return this;
    };
    return clsLibPathThen;
}());
var reqUtils = {
    from: function (strPath) {
        return new clsLibPath(strPath);
    }
};
//# sourceMappingURL=requtils.js.map