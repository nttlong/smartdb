window.___bootstrapUI___ = angular.module("q-ui", []);
window.__utils__ = {
    errors: [],
    $$$views$$$: {},
    absolutePath: function (base, relative) {
        var stack = base.split("/"),
            parts = relative.split("/");
        stack.pop(); // remove current file name (or empty string)
        // (omit if "base" is the current folder without trailing slash)
        for (var i = 0; i < parts.length; i++) {
            if (parts[i] == ".")
                continue;
            if (parts[i] == "..")
                stack.pop();
            else
                stack.push(parts[i]);
        }
        return stack.join("/");
    },
    getAbsPath: function (path) {
        var index = path.indexOf("/../");
        if (index > -1) {
            return __utils__.absolutePath(path.substring(0, index), path.substring(index, path.length))
        }
        else {
            return path;
        }
    },
    loadScriptContent: function (url, cb) {
        var $mask = $("<div class='mask'></div>");
        $mask.appendTo("body");
        $.ajax({
            url: url,
            method: "get",
            success: function (res, textStatus, request) {

                $mask.remove();
                cb(null, res)

            },
            error: function (ex) {

                $mask.remove();
                if (ex.status == 200) {
                    cb(null, ex.responseText);
                    return;
                }
                cb(ex);
            }
        });
    },
    getScript: function (currentUrl, res, cb) {
        var rScript = /(\<script\s\s*src\s*\=\s*('|").+('|")\s*\>\s*\<\/script\>|\<script>(\n)*.+(\n)*\<\/script\>)/;
        var rInline = /\<script\s+src\=(\'|\")(.)*\>/;
        var content = res.res;

        if (content.indexOf("<body>") > -1) {
            var x = content.indexOf("<body>") + "<body>".length;
            var y = content.indexOf("</body>", x);
            content = content.substring(x, y);
        }
        var ret = [];

        var r = rScript.exec(content);
        while (r != null) {
            var startIndex = r.index;

            var endIndex = content.indexOf("</script>", startIndex + 1) + "</script>".length;
            var scriptContent = content.substring(startIndex, endIndex);
            var isInline = rInline.exec(scriptContent);
            if (isInline) {

                var scriptSource = scriptContent;
                if (scriptSource.indexOf("'") > -1) {
                    scriptSource = scriptSource.split("'")[1].split("'")[0];
                }
                if (scriptSource.indexOf('"') > -1) {
                    scriptSource = scriptSource.split('"')[1].split('"')[0];
                }
                r = rScript.exec(content);
                var urlRoot = "";
                var items = currentUrl.split('/');
                for (var i = 0; i < items.length - 1; i++) {
                    urlRoot += items[i] + "/";
                }
                scriptSource = scriptSource.replace("~/", "");
                if (scriptSource.indexOf(window.location.protocol + "//" + window.location.host) === -1) {
                    var scriptUrl = urlRoot + scriptSource;
                    if (scriptUrl.indexOf("://") > -1) {
                        items = scriptUrl.split("://");
                        scriptUrl = items[1];
                        while (scriptUrl.indexOf("//") > -1) {
                            scriptUrl = scriptUrl.replace("//", "/");
                        }
                        scriptUrl = items[0] + "://" + scriptUrl;
                    }
                    else {
                        scriptUrl = window.location.protocol + "//" + window.location.host + "/" + scriptUrl;
                    }
                    ret.push({ url: scriptUrl });
                }
                else {
                    ret.push({ url: scriptSource });
                }
                content = content.replace(r[0], "");
            }
            else {


                ret.push({ scriptContent: scriptContent.replace("<script>", "").replace("</script>", "") });
                content = content.replace(scriptContent, "");
            }

            r = rScript.exec(content);

        }


        var scriptObjs = ret;
        var retData = {
            scripts: [],
            content: content,
            url: res.url
        };
        function getScriptFromFile(scriptObjs, cb, index) {
            var index = index || 0;
            if (scriptObjs.length === index) {
                cb(retData);
            }
            else {
                if (scriptObjs[index].scriptContent) {
                    retData.scripts.push(scriptObjs[index].scriptContent);
                    getScriptFromFile(scriptObjs, cb, index + 1);
                }
                else {
                    window.__utils__.loadScriptContent(scriptObjs[index].url, function (err, res) {

                        if (err) {
                            console.error(err);
                        }
                        else {
                            retData.scripts.push(res);
                        }
                        getScriptFromFile(scriptObjs, cb, index + 1);
                    });
                }

            }
        }
        getScriptFromFile(scriptObjs, cb);
        return retData;
    },
    getUrlInfo: function (url) {
        if (url.indexOf(window.location.protocol + "//" + window.location.host) == 0) {
            debugger;
            var prefix = window.location.protocol + "//" + window.location.host
            url = url.substring(prefix.length, url.length)
        }
        url = url.split('?')[0];
        console.log(url, "before");
        url = __utils__.getAbsPath(url);
        console.log(url, "after");
        while (url.substring(url.length - 1, url.length) === "/") {
            url = url.substring(0, url.length - 1);
        }
        if (url.indexOf(".html") > -1) {
            url = url.substring(0, url.indexOf(".html"));
            var items = url.split("/");
            url = "";
            for (var i = 0; i < items.length - 1; i++) {
                url += items[i] + "/";
            }
            url = url.substring(0, url.length - 1);
            if (window.location.protocol + "//" + window.location.host === url) {
                return {
                    host: window.location.protocol + "//" + window.location.host,
                    relUrl: {
                        ref: "",
                        value: ""
                    },
                    absUrl: {
                        ref: url,
                        value: url
                    }
                };
            }
            else {
                var root = window.location.protocol + "//" + window.location.host;
                var fullUrl = root +  url + "/" + items[items.length - 1] + ".html";
                var fullRef = root +  url;
                var retInfo = {
                    host: root,
                    relUrl: {
                        ref: url,
                        value: fullUrl.substring(root.length, fullUrl.length)
                    },
                    absUrl: {
                        ref: fullRef,
                        value: fullUrl
                    }
                }
                return retInfo;
            }
        }
        if (window.location.protocol + "//" + window.location.host === url) {
            return {
                host: window.location.protocol + "//" + window.location.host,
                relUrl: {
                    ref: "",
                    value: ""
                },
                absUrl: {
                    ref: url,
                    value: url
                }
            };
        }
        else if (!window.__utils__.hasStart) {
            window.__utils__.hasStart = true;
            var root = window.location.protocol + "//" + window.location.host;
            var fullUrl = url;
            var retInfo = {
                host: root,
                relUrl: {
                    ref: url.substring(root.length, url.length),
                    value: fullUrl.substring(root.length, fullUrl.length)
                },
                absUrl: {
                    ref: url,
                    value: fullUrl
                }
            }
            return retInfo;
        }
        var ret = {
            host: window.location.protocol + "//" + window.location.host,
            relUrl: {
                ref: "",
                value: ""
            },
            absUrl: {
                ref: "",
                value: ""
            }
        };
        if (url.length > ret.host.length) {
            if (url.substring(0, ret.host.length).toLowerCase() === ret.host.toLowerCase()) {
                url = url.substring(ret.host.length + 1, url.length);
            }
        }
        var prefix = url.split('?')[0].split("://")[0];
        var tail = url.split('?')[0].split("://")[1];
        if (!tail) {
            tail = url;
            prefix = "";
        }
        if (tail) {
            while ((tail.indexOf("//") > -1)) {
                tail = tail.replace("//", "/");
            }
            var x = tail.split('/');
            ret.absUrl.ref = prefix + "://";
            ret.absUrl.value = prefix + "://";
            for (var i = 0; i < x.length; i++) {
                ret.absUrl.value += "/" + x[i];
            }
            for (var i = 0; i < x.length - 1; i++) {
                ret.absUrl.ref += "/" + x[i];
            }
        }
        ret.absUrl.ref = ret.absUrl.ref.replace(":////", "://");
        ret.absUrl.value = ret.absUrl.value.replace(":////", "://");
        ret.absUrl.ref = ret.absUrl.ref.replace(":///", "://");
        ret.absUrl.value = ret.absUrl.value.replace(":///", "://");
        ret.relUrl.ref = ret.absUrl.ref.substring(ret.host.length + 2, ret.absUrl.ref.length);
        ret.relUrl.value = ret.absUrl.value.substring(ret.host.length + 2, ret.absUrl.value.length);
        if (ret.absUrl.ref.indexOf(ret.host) == -1) {
            ret.relUrl.ref = ret.absUrl.ref.replace("://", "");
            ret.relUrl.value = ret.absUrl.value.replace("://", "");
            ret.absUrl.ref = ret.host + "/" + ret.relUrl.ref;
            ret.absUrl.value = ret.host + "/" + ret.relUrl.value;
        }
        console.log(ret);
        return ret;
    },
    applyGetViewId: function (s) {
        s.$getViewId = function () {
            if (s.$viewId) {
                return s.$viewId;
            }
            else {
                var parent = s.$parent;
                while (parent !== s.$root) {
                    if (parent.$viewId) {
                        return parent.$viewId;
                    }
                    else {
                        parent = parent.$parent;
                    }
                }
            }
        }
        s.$getView = function () {
            var viewId = s.$getViewId();
            if (viewId) {
                return window.__utils__.$$$views$$$[viewId];
            }
        }
    }
}
var __loginUrl = undefined;
function setLoginUrl(url) {
    __loginUrl = url;
}
function redirectToLogin() {
    if (!__loginUrl) {
        console.error("It looks like you forgot call 'setLoginUrl' with loginUrl param");
    }
    else {
        window.location.href = __loginUrl + "?ReturnUrl=" + encodeURIComponent(escape(window.location.href));

    }
}
var _dialog_root_url;
var ___uiReady__ = undefined;
function uiReady(callback) {
    ___uiReady__ = callback;
}
var ___rootDir__ = undefined;

function angularDefine(callback) {
    if (!window.___bootstrapUI___) {
        window.___bootstrapUI___ = angular.module("q-ui", []);
    }
    callback(window.___bootstrapUI___);
}
window.Q = {
    angularDefine: angularDefine
}
function dialog_root_url(value) {
    _dialog_root_url = value;
}
function applyFindEle(scope) {
    scope.$getUi = function (id) {
        if (!scope.$$ui) {
            console.error("'" + id + "' was not found in '" + scope.$$urlInfo.absUrl.value + "'");
            return;
        }
        if (!scope.$$ui[id]) {
            console.error("'" + id + "' was not found in '" + scope.$$urlInfo.absUrl.value + "'");
            return;
        }
        return scope.$$ui[id];

    }
    scope.$findEle = function (selector, callback) {
        function run() {
            var eles = scope.$element.find(selector);
            if (eles.length > 0) {
                if ($.contains($("body")[0], eles[0])) {
                    callback(eles);
                }
                else {
                    setTimeout(run, 300);
                }
            }
            else {
                setTimeout(run, 300);
            }
        };
        run();
    }
}
function findScopeById(id) {
    var eles = $(".ng-scope");
    var ret = undefined;
    for (var i = 0; i < eles.length; i++) {
        ret = angular.element(eles[i]).scope();
        if (ret.$id === id) {
            break;
        }
        else {
            ret = undefined;
        }
    }
    return ret;
}
function dialog($scope, $compile, $injector) {

    if (angular.isNumber($scope)) {
        $scope = findScopeById($scope);
    }
    function getScript(rootUrl, content, cb) {
        if (content.indexOf("<body>") > -1) {
            var x = content.indexOf("<body>") + "<body>".length;
            var y = content.indexOf("</body>");
            content = content.substring(x, y);
        }
        var ret = [];
        window.__utils__.getScript(rootUrl, { res: content }, cb);

        return {
            scripts: ret,
            content: content
        };
    }
    function compile(scope, scripts, content, _params, urlInfo, width, height, isAdForm) {
        var subScope = scope.$new(true, scope);
        subScope.$$urlInfo = urlInfo;
        subScope.$$ui = {};
        subScope.$ajax = {};
        subScope.$inputParams = _params;

        var $ele = $("<div>" + content + "</div>");
        var viewId = $($ele.children()[0]).attr("view-id");
        if (!isAdForm) {
            subScope.$viewId = viewId;
        }
        window.__utils__.applyGetViewId(subScope);

        var child = $($ele.children()[0]);
        var strStyle = "";
        if (child.attr("style")) {
            strStyle = "style=\"" + child.attr("style") + "\"";
            child.removeAttr("style");
        }
        var width = child.attr("dialog-width");
        if (!strStyle) {
            strStyle = 'style="' + strStyle + '"';
        }
        var strDialogStyle = "";


        var frm = $('<div><div class="modal fade"  role="dialog" id="' + (child.attr("id") || "dialog") + '">' +
            '<div class="modal-dialog">' +
            '<div class="modal-content" >' +

            '<div class="modal-header">' +


            '<h4 class="modal-title"><img src=""/ style="height:40px"><span>...</span></h4>' +
            '<button type="button" class="close" data-dismiss="modal">&times;</button>' +
            '</div>' +
            '<div class="modal-body">' +

            '</div>' +
            '<div class="modal-footer">' +

            '</div>'
        );
        frm.find(".modal-dialog").css({ width: width + "px" });



        var initCode = child.attr("ng-init");
        frm.attr("title", child.attr("title"));
        frm.attr("icon", child.attr("icon"));
        if ($ele.find(".modal-footer").length > 0) {
            $ele.find(".modal-footer").children().appendTo(frm.find(".modal-footer")[0]);
            $ele.find(".modal-footer").remove();
        }
        else {
            frm.find(".modal-footer").hide();
        }
        //if (width) {
        //    $ele.children().css("width", width);
        //}
        if (height) {
            $ele.children().css("height", height);
        }
        $ele.children().appendTo(frm.find(".modal-body")[0]);

        subScope.$element = frm;
        applyFindEle(subScope);
        subScope.$watch(function () {
            return subScope.$element.find(".modal-body").children().attr("title");
        }, function (val) {
            if (angular.isDefined(val)) {
                subScope.$element.find(".modal-title span").html(val);
            }
        });
        subScope.$watch(function () {
            return subScope.$element.find(".modal-body").children().attr("icon");
        }, function (val) {
            if (angular.isDefined(val)) {
                subScope.$element.find(".modal-title img").attr("src", val);
            }
            else {
                subScope.$element.find(".modal-title img").hide();
            }

        });
        var $cmp = $compile || $scope.$root.$compile;
        if (!$cmp) {
            throw ("Please use '$compile' at controller then set '$scope.$root.$compile=$compile'");
        }
        $cmp(frm.contents())(subScope);
        subScope.$element = $(frm.children()[0]);
        for (var i = 0; i < scripts.length; i++) {

            var fn = eval(scripts[i]);
            if (angular.isFunction(fn)) {
                fn(subScope, _params);
            }
            if (angular.isArray(fn)) {
                var injectionNames = [];
                for (var j = 0; j < fn.length - 1; j++) {
                    $injector.get(fn[j]);
                    injectionNames.push($injector.get(fn[j]));
                }
                injectionNames.push(subScope);
                injectionNames.push(_params);
                fn[fn.length - 1].apply(undefined, injectionNames);
            }

        }
        subScope.$applyAsync();

        subScope.$eval(initCode);
        subScope.$parent.$on("$destroy", function () {
            subScope.$doClose();
        });

        return subScope;
    }
    function ret(scope) {
        var me = this;
        me.url = function (_url) {


            me._url = _url;
            return me;
        };
        me.params = function (data) {
            me._params = data;
            return me;
        };
        me.onClose = function (callback) {
            me.__close = callback;
            return me;
        };
        me.asView = function () {
            me.__isAsView__ = true;
            me.done();
        }
        me.done = function (callback) {
            if (!scope.$$urlInfo) {
                scope.$$urlInfo = window.__utils__.getUrlInfo(window.location.href.split('#')[0].split('?')[0] + "/");
            }
            var $mask = $("<div class='mask'></div>").appendTo("body");
            var currenturl = me._url;
            console.log(currenturl);
            if (currenturl.indexOf("://") == -1) {
                if ((me._url.length >= 2) && (me._url.substring(0, 2) === "~/")) {
                    currenturl = scope.$$urlInfo.absUrl.ref + "/" + me._url.substring(2, me._url.length);
                }
                else if (currenturl.substring(0, 2) === "./") {
                    currenturl = window.location.protocol + "//" + window.location.host + "/" + me._url.substring(2, me._url.length);
                }
                else {
                    if (_dialog_root_url && (me._url.indexOf("://") == -1)) {
                        currenturl = _dialog_root_url + "/" + scope.$$urlInfo.relUrl.ref + "/" + me._url;
                    }
                    else {
                        currenturl = scope.$$urlInfo.absUrl.ref + "/" + me._url;
                    }
                }
            }
            function loadContent(currenturl,res) {
                getScript(currenturl, res, function (ret) {
                    var urlInfo = window.__utils__.getUrlInfo(currenturl);
                    var sScope = compile(scope, ret.scripts, ret.content, me._params, urlInfo, me._width, me._height, me.__isAsView__);
                    sScope.$$url = me._url.split("?")[0];
                    if (callback) {
                        callback(sScope);
                    }
                    sScope.$element.appendTo("body");
                    function watch() {
                        if (!$.contains($("body")[0], sScope.$element[0])) {

                            if (me.__close) {
                                var fn = scope.$eval(me.__close);
                                if (angular.isFunction(fn)) {
                                    fn(sScope);
                                }
                                me.__close = undefined;

                            }
                            sScope.$destroy();
                            return;
                        }
                        else {
                            setTimeout(watch, 500);
                        }
                    }
                    watch();


                    sScope.$element.modal({
                        backdrop: 'static',
                        keyboard: false
                    })
                        .on("hidden.bs.modal", function (event) {

                            sScope.$element.remove();

                        });


                    sScope.$doClose = function () {
                        sScope.$element.modal('hide');
                    };
                    $(sScope.$element[0]).draggable({
                        "handle": ".modal-header"
                    });
                    $(sScope.$element[0]).find('.modal-content').resizable({
                        minHeight: 300,
                        minWidth: 300
                    });
                    setTimeout(function () {
                        $(".modal-backdrop.fade.show").hide();
                        $(".modal-backdrop").hide();
                        $(".modal").css({
                            "background-color": "transparent !important",
                            "background": "transparent !important",
                        })

                        $(sScope.$element[0]).css("background-color", "transparent");

                    }, 400);

                    watch();

                });
            }
            $.ajax({
                method: "GET",
                url: currenturl,

                headers: { 'OpenerUrl': scope.$$url || "." },
                success: function (res, textStatus, request) {
                    $mask.remove();
                    //var x = currenturl;
                    loadContent(currenturl, res);
                   

                },
                error: function (ex) {
                    $mask.remove();
                    if (ex.status == 200) {
                        loadContent(currenturl, ex.responseText);
                        return;
                    }
                    if (ex.status === 401) {
                        redirectToLogin();
                        return;
                    }
                    if (ex.status === 404) {
                        var tab = window.open('about:blank', '_blank');
                        var msg = "'" + me._url + "' was not found";
                        tab.document.write(msg); // where 'html' is a variable containing your HTML
                        tab.document.close();
                        return;
                    }
                    var tab = window.open('about:blank', '_blank');
                    if (ex.responseText.indexOf("<!DOCTYPE html>") === -1) {
                        while (ex.responseText.indexOf(String.fromCharCode(10)) > -1) {
                            ex.responseText = ex.responseText.replace(String.fromCharCode(10), "<br/>");
                        }
                    }
                    tab.document.write(ex.responseText); // where 'html' is a variable containing your HTML
                    tab.document.close();
                    if (callback) {
                        callback(ex, undefined);
                    }

                }
            });
        };
        me.size = function (width, height) {
            me._width = width;
            me._height = height;
            return me;
        };
        me.modal = function () {
            me._isModal = true;
            return me;
        }
    }
    return new ret($scope);
}
var ___url__has__key___ = "#$";
function $url() {


    //var encrypted = CryptoJS.AES.encrypt(myString, myPassword);
    //var decrypted = CryptoJS.AES.decrypt(encrypted, myPassword);
    function ret() {
        var me = this;
        me.data = {};
        if (window.location.href.indexOf("?") > -1) {
            if (window.location.href.split("?")[1] !== "") {
                var ref = window.location.href.split('?!')[1].split('#')[0];
                if (ret != "") {
                    ref = atob(decodeURIComponent(ref));
                }

                var items = ref.split('&');
                for (var i = 0; i < items.length; i++) {
                    me.data[items[i].split('=')[0]] = items[i].split('=')[1];
                }
            }
        }
        me.param = function (key, value) {
            console.log(key, value);
            me.data[key] = value;
            return me;
        }
        me.def = function (key, value) {
            return me.param(key, value);
        }
        me.remove = function (key) {
            me.data[key] = undefined;
            return me;
        }
        me.rem = function (key) {
            return me.remove(key);
        }
        me.clear = function () {
            me.data = {}
            return me;
        }
        me.cls = function () {
            return me.clear();
        }
        me.url = function () {
            var ret = "";
            var keys = Object.keys(me.data);
            for (var i = 0; i < keys.length; i++) {
                if (angular.isDefined(me.data[keys[i]])) {
                    ret += keys[i] + "=" + me.data[keys[i]] + "&"
                }
            }
            var retStr = ret.substring(0, ret.length - 1);
            return "!" + encodeURIComponent(btoa(retStr));
        }
        me.go = function ($event) {
            window.history.pushState("", "", "?" + me.url());
            if ($event) {
                $event.stopPropagation();
            }
            return false;
        }
        me.href = function () {
            return me.url();
        }
        me.apply = function () {
            window.location.href = "#" + me.url();
        }
        me.toString = function () {
            return me.url();
        }
        me.uri = function () {
            return escape(encodeURIComponent(window.location.href));
        }
        me.action = function ($event) {

            window.history.pushState("", "", "?" + me.url());
            if ($event) {
                $event.stopPropagation();
            }
            return false;
        }

    }
    return new ret();
}
function history_navigator($scope) {
    var oldUrl;
    function historyMonitorStart(handler) {
        function getData() {
            var data = {};
            if (window.location.href.indexOf('?!') == -1) {
                return data;
            }
            var url = window.location.href.split('?!')[1].split('&')[0];
            if ((window.location.href.split('?')[1] !== "") && (url !== "")) {
                url = atob(decodeURIComponent(url));
            }
            else {
                url = "";
            }
            var items = url.split('&');
            var ret = {};
            for (var i = 0; i < items.length; i++) {
                data[items[i].split('=')[0]] = decodeURI(window["unescape"](items[i].split('=')[1]));
            }
            return data;
        }
        function run() {
            if (oldUrl !== window.location.href) {

                if (historyChangeCallback.length > 0) {
                    if (window.location.href.indexOf('?!') > -1) {
                        var data = getData();
                        for (var i = 0; i < historyChangeCallback.length; i++) {
                            historyChangeCallback[i](data);
                        }
                        var keys = Object.keys($scope.$history.events);
                        for (var i = 0; i < keys.length; i++) {
                            if (!$scope.$history.events[keys[i]].hasStartCall) {
                                var obj = {
                                    key: keys[i],
                                    data: data,
                                    done: function () {
                                        if ($scope.$history.events[obj.key])
                                            $scope.$history.events[obj.key].handler(obj.data);
                                    }
                                }
                                setTimeout(function () {
                                    obj.done();
                                }, 10);

                            }
                        }

                    }
                    else {
                        var keys = Object.keys($scope.$history.events);
                        if (keys.length > 0) {
                            for (var i = 0; i < keys.length; i++) {
                                if (!$scope.$history.events[keys[i]].hasStartCall) {
                                    var obj = {
                                        key: keys[i],
                                        data: data,
                                        done: function () {
                                            if ($scope.$history.events[obj.key])
                                                $scope.$history.events[obj.key].handler({});
                                        }
                                    }
                                    setTimeout(function () {
                                        obj.done();
                                    }, 300);

                                }
                            }
                        }
                        else {
                            historyChangeCallback[historyChangeCallback.length - 1]({});
                        }
                    }
                }
                else {
                   var data = getData();
                    var keys = Object.keys($scope.$history.events);
                    for (var i = 0; i < keys.length; i++) {
                        if (!$scope.$history.events[keys[i]].hasStartCall) {
                            var obj = {
                                key: keys[i],
                                data: data,
                                done: function () {
                                    if ($scope.$history.events[obj.key])
                                        $scope.$history.events[obj.key].handler(obj.data);
                                }
                            }
                            setTimeout(function () {
                                obj.done();
                            }, 10);

                        }
                    }
                }
                oldUrl = window.location.href;
            }
            setTimeout(run, 100);
        }
        run();
    }

    var historyChangeCallback = [];
    function applyHistory(scope) {

        scope.$history = {
            isStart: true,
            events: {},
            data: function () {
                if (window.location.href.split("?")[1] === "") {
                    return {};
                }
                if (window.location.href.indexOf('?') === -1)
                    return {};
                if (window.location.href.indexOf('?!') === -1)
                    return {};
                var url = window.location.href.split('?!')[1].split('#')[0];
                if (url != "") {
                    url = atob(decodeURIComponent(url));
                }
                var items = url.split('&');
                var ret = {};
                for (var i = 0; i < items.length; i++) {
                    ret[items[i].split('=')[0]] = decodeURI(window["unescape"](items[i].split('=')[1]));
                }
                return ret;
            },
            change: function (callback) {
                var _data = scope.$history.data();
                callback(_data);
                scope.$$$$historyCallback = callback;
                historyChangeCallback.push(callback);

            },
            redirectTo: function (bm) {
                window.location.href = bm;
            },
            onChange: function (subScope, handler) {

                scope.$history.events["scope_" + subScope.$id] = {
                    handler: handler,
                    hasStartCall: true,
                    scope: subScope
                };
                subScope.$on("$destroy", function () {
                    delete scope.$history.events["scope_" + subScope.$id];
                });
                if (scope.$history.events["scope_" + subScope.$id].hasStartCall) {
                    handler(scope.$history.data());
                    scope.$history.events["scope_" + subScope.$id].hasStartCall = false;
                }

            }
        };
        function URLObject(obj) {
            obj.$url = this;
            var me = this;
            me.data = obj.$history.data();
            me.clear = function () {
                me.data = {};
                return me;
            };
            me.item = function (key, value) {
                if (!me.data) {
                    me.data = {};
                }
                me.data[key] = value;
                return me;
            };
            me.done = function () {
                var keys = Object.keys(me.data);
                var retUrl = "";
                for (var i = 0; i < keys.length; i++) {
                    retUrl += keys[i] + "=" + window["escape"](encodeURI(me.data[keys[i]])) + "&";
                }
                retUrl = window.location.href.split('#')[0] + '#' + retUrl.substring(0, retUrl.length - 1);
                return retUrl;
            };
            var x = 1;
        }
        new URLObject(scope);
        historyMonitorStart(historyChangeCallback);
    }
    return new applyHistory($scope);
}

function ng_app(modulelName, controllerName, injection, fn, ngStartSymbol, ngEndSymbol) {
    injection.push("imageupload", "q-ui");
    var app = angular.module(modulelName, injection, function ($interpolateProvider) {
        if (ngStartSymbol) {
            $interpolateProvider.startSymbol(ngStartSymbol);
            $interpolateProvider.endSymbol(ngStartSymbol);
        }

    });
    var controller = app.controller(controllerName, ["$compile", "$scope", function ($compile, $scope) {
        $scope.$root.$compile = $compile;
        $scope.$root.$dialog = dialog;
        history_navigator($scope.$root);
        fn($scope);


        $scope.$applyAsync();
    }]);
}
var _appDirectiveSetRootUrl;
function appDirectiveSetRootUrl(url) {
    _appDirectiveSetRootUrl = url;
}
window.___bootstrapUI___ = angular.module("q-ui", []);
angularDefine(function (mdl) {
    mdl.service("$loadScriptContent", [function () {

        return window.__utils__.loadScriptContent;

    }]);
    mdl.service("$history", [function () {

        return history_navigator;
    }]);
    mdl.service("$nav", [function () {
        var root = angular.element(".ng-scope").scope().$root;
        if (!root.$history) {
            history_navigator(root);
        }

        return root.$history;
    }]);
    mdl.service("$url", [function () {

        return $url;
    }]);
});
angularDefine(function (mdl) {
    mdl.service("$scriptExtractor", [function () {

        return window.__utils__.getScript


    }]);
    mdl.service("$dialog", ["$compile", "$injector", function ($compile, $injector) {

        return function (scope) {
            return dialog(scope, $compile, $injector);
        }


    }]);
});
angularDefine(function (mdl) {
    mdl.directive("qTemplate", ["$compile", "$scriptExtractor", "$injector",
        function ($compile, $scriptExtractor, $injector) {

            function loadUrl(url, handler, scope) {
                
                var currentUrl = undefined;
                if (url.indexOf(window.location.protocol + "//" + window.location.host + "/") == 0) {
                    currentUrl = url;
                }
                else {
                    if ((url.length >= 2) && ((url.substring(0, 2) === "~/") || (url.substring(0, 2) === "./"))) {
                        currentUrl = scope.$$urlInfo.absUrl.ref + "/" + url.substring(2, url.length);
                    }
                    else {
                        if (_appDirectiveSetRootUrl) {
                            currentUrl = _appDirectiveSetRootUrl ? _appDirectiveSetRootUrl + "/" + url : url;
                        }
                        else {

                            currentUrl = scope.$$urlInfo.absUrl.ref + "/" + url;
                        }
                    }
                }
                var $mask = $("<div class='mask'></div>");
                $mask.appendTo("body");
                $.ajax({
                    url: currentUrl,
                    method: "get",
                    headers: { 'OpenerUrl': scope.$$url || "." },
                    success: function (res, textStatus, request) {
                        $mask.remove();
                        handler(undefined, { fullUrl: currentUrl, url: url, res: res });
                    },
                    error: function (ex) {
                        $mask.remove();
                        if (ex.status === 200) {
                            redirectToLogin();
                            handler(undefined, { fullUrl: currentUrl, url: url, res: ex.responseText });
                            return;
                        }
                        if (ex.status === 401) {
                            redirectToLogin();
                            return;
                        }
                        if (ex.status === 404) {
                            var tab = window.open('about:blank', '_blank');
                            var msg = "'" + (_appDirectiveSetRootUrl ? _appDirectiveSetRootUrl + "/" + url : url) + "' was not found";
                            tab.document.write(msg); // where 'html' is a variable containing your HTML
                            tab.document.close();
                            return;
                        }
                        var tab = window.open('about:blank', '_blank');
                        tab.document.write(ex.responseText); // where 'html' is a variable containing your HTML
                        tab.document.close();
                    }
                });
            }


            function compile(scope, scripts, content, url, urlInfo, attr) {

                var appName = undefined;
                var controllerName = undefined;
                var subScope = undefined;
                var ngInit = undefined;
                var $ele = $("<div>" + content + "</div>");
                var viewId = undefined;
                if ($($ele.children()[0]).attr("view-id")) {
                    viewId = $($ele.children()[0]).attr("view-id");
                }
                if ($($ele.children()[0]).attr("ng-app")) {
                    appName = $($ele.children()[0]).attr("ng-app");
                    controllerName = $($ele.children()[0]).attr("ng-controller");

                }
                ngInit = $($ele.children()[0]).attr("ng-init");
                $($ele.children()[0]).attr("style", attr.viewStyle);
                $($ele.children()[0]).addClass(attr.viewClass);
                if (appName) {
                    var fn = Function("var ret=" + scripts[scripts.length - 1] + ";return ret")();
                    fn();
                    scripts.pop();
                    angular.bootstrap($ele.children()[0], [appName]);
                    subScope = angular.element($ele.children()[0]).scope();
                    subScope.$element = $ele.children();
                    subScope.$$urlInfo = urlInfo;
                    subScope.$viewId = viewId;
                    applyFindEle(subScope);
                    window.__utils__.applyGetViewId(subScope);
                    if (viewId) {
                        window.__utils__.$$$views$$$[viewId] = subScope;
                    }

                }
                else {
                    var subScope = scope.$new(true, scope);
                    window.__utils__.applyGetViewId(subScope);
                    subScope.$viewId = viewId;
                    if (viewId) {
                        window.__utils__.$$$views$$$[viewId] = subScope;
                    }
                    subScope.$$urlInfo = urlInfo;
                    subScope.$$ui = {};
                    subScope.$ajax = {};
                    subScope.$element = $ele.children();
                    applyFindEle(subScope);
                    $compile($ele.contents())(subScope);
                    subScope.$applyAsync();
                }
                return {
                    scope: subScope,
                    run: function () {

                        if (scripts && (scripts.length > 0)) {
                            for (var i = 0; i < scripts.length; i++) {
                                try {
                                    var fn = Function("var ret=" + scripts[i] + String.fromCharCode(13) + String.fromCharCode(10) + ";return ret;");
                                    if (angular.isFunction(fn)) {
                                        fn = fn(subScope);
                                    }
                                    if (angular.isFunction(fn)) {
                                        fn = fn(subScope);
                                    }
                                    if (angular.isArray(fn)) {
                                        var injections = [];
                                        for (var j = 0; j < fn.length - 1; j++) {
                                            injections.push($injector.get(fn[j]));
                                        }
                                        injections.push(subScope);
                                        fn[fn.length - 1].apply(undefined, injections);
                                    }

                                }
                                catch (ex) {
                                    ex.url = url;
                                    throw ({exception:ex,url:url});
                                    break;
                                }
                            }
                        }

                        if (ngInit) {
                            var fnNgInit = subScope.$eval(ngInit);
                            if (angular.isFunction(fnNgInit)) {
                                fnNgInit(subScope);
                            }
                        }

                    }

                };
            }
            return {
                restrict: "ACE",
                link: function (scope, ele, attr) {
                    if (!scope.$$urlInfo) {
                        scope.$$urlInfo = window.__utils__.getUrlInfo(window.location.href.split('#')[0].split('?')[0] + "/")
                    }
                    attr.$observe("url", function (value) {
                        if (value === "") return;
                        var currentUrl = undefined;
                        if ((value.indexOf(window.location.protocol + "//" + window.location.host + "/") === 0) ||
                            (value.indexOf("http://") === 0) ||
                            (value.indexOf("https://") === 0)) {
                            currentUrl = value;
                        }
                        else {
                            if ((value.length >= 2) && (value.substring(0, 2) === "~/")) {
                                currentUrl = scope.$$urlInfo.absUrl.ref + "/" + value.substring(2, value.length);
                            }
                            else {
                                if (_appDirectiveSetRootUrl) {
                                    currentUrl = _appDirectiveSetRootUrl ? _appDirectiveSetRootUrl + "/" + value : value;
                                }
                                else {

                                    currentUrl = scope.$$urlInfo.absUrl.ref + "/" + value;
                                }
                            }
                        }

                        if (window.__debug__) {
                            window.__debug__++;
                            value = value + "?" + window.__debug__;
                        }
                        loadUrl(value, function (err, content) {
                            $scriptExtractor(currentUrl, content, function (ret) {
                                var urlInfo = window.__utils__.getUrlInfo(currentUrl);
                                var retObj = compile(scope, ret.scripts, ret.content, ret.url, urlInfo, attr);
                                ele.empty();
                                var sender = {
                                    ele: retObj.scope.$element,
                                    scope: retObj.scope,
                                    done: function () {
                                        retObj.scope.$element.appendTo(ele[0]);
                                        retObj.scope.$$url = value.split("?")[0];
                                        //setTimeout(function () { retObj.scope.$element.show();}, 100);
                                        function watch() {
                                            if (!$.contains($("body")[0], retObj.scope.$element[0])) {
                                                retObj.scope.$destroy();
                                            }
                                            else {
                                                if (retObj.run) {
                                                    setTimeout(function () {
                                                        try {
                                                            retObj.run();
                                                        }
                                                        catch (ex) {
                                                            retObj.run = undefined;
                                                            console.error(ex.url);
                                                            throw (ex.exception);
                                                            return;
                                                        }
                                                        if (__utils__.errors.length > 0) {
                                                            for (var i = 0; i < __utils__.errors.length; i++) {
                                                                throw (__utils__.errors[i]);
                                                            }
                                                            __utils__.errors = [];
                                                            retObj.run = undefined;
                                                            return;
                                                        }
                                                        retObj.run = undefined;
                                                    }, 50);

                                                }
                                                setTimeout(watch, 500);
                                            }
                                        }
                                        watch();
                                    }
                                };
                                if (attr.onBeforeRender) {
                                    var fn = scope.$eval(attr.onBeforeRender);
                                    if (angular.isFunction(fn)) {
                                        fn(sender);
                                    }
                                }
                                else {
                                    sender.done();
                                }
                                //retObj.scope.$element.hide();

                            });

                        }, scope);
                    });
                }
            };
        }]);
});
function makeUpForm(divRow, a) {

    // divRow.hide();

    var rows = divRow.children();
    for (var x = 0; x < rows.length; x++) {
        var eles = $(rows[x]).children();
        var tmpDiv = $("<div></div>");

        for (var i = 0; i < eles.length; i++) {
            var div = $("<div class='form-element'></div>");
            var ele = $(eles[i]);
            if (ele.attr("ng-show")) {
                div.attr("ng-show", ele.attr("ng-show"));

            }
            if (ele.attr("ng-if")) {
                div.attr("ng-if", ele.attr("ng-if"));

            }
            if ((ele[0].tagName === "LABEL") ||
                ((ele[0].tagName === "SPAN"))) {
                ele.addClass("control-label");
            }
            if ((ele[0].tagName === "INPUT") &&
                (((ele[0].type === "text") ||
                    (ele[0].type === "number") ||
                    (ele[0].type === "select") ||
                    (ele[0].type === "email") ||
                    (ele[0].type === "password")
                ))) {
                ele.addClass("form-control");
            }
            if ((ele[0].tagName === "INPUT") &&
                (((ele[0].type === "button") ||
                    (ele[0].type === "submit")
                ))) {
                ele.addClass("btn");
            }
            if (ele.attr("span")) {
                if (!ele.attr("xs-span")) {
                    ele.attr("xs-span", ele.attr("span"));
                }
                if (!ele.attr("sm-span")) {
                    ele.attr("sm-span", ele.attr("span"));
                }
                if (!ele.attr("md-span")) {
                    ele.attr("md-span", ele.attr("span"));
                }
                if (!ele.attr("lg-span")) {
                    ele.attr("lg-span", ele.attr("span"));
                }
            }
            $(eles[i]).appendTo(div[0]);
            div.appendTo(tmpDiv[0]);
        }
        $(rows[x]).addClass("row form-element")
        tmpDiv.contents().appendTo(rows[x]);

    }
    for (var x = 0; x < rows.length; x++) {
        var eles = $(rows[x]).children();
        var mdCols = (a.mdCols || "3,9").split(',');
        var smCols = (a.smCols || "4,8").split(',');
        var lgCols = (a.lgCols || "2,4,2,4").split(',');
        var xsCols = (a.xsCols || "12").split(',');
        var mdIndex = 0;
        var smIndex = 0;
        var lgIndex = 0;
        var xsIndex = 0;

        var mdTotal = 0;
        var smTotal = 0;
        var lgTotal = 0;
        var xsTotal = 0;
        for (var i = 0; i < eles.length; i++) {
            if ($($(eles[i]).children()[0]).attr("break") !== undefined) {
                console.log(lgTotal);
                var _xs = 12 - xsTotal % 12;
                var _sm = 12 - smTotal % 12;
                var _md = 12 - mdTotal % 12;
                var _lg = 12 - lgTotal % 12;
                $(eles[i]).addClass("col-xs-" + _xs);
                $(eles[i]).addClass("col-sm-" + _sm);
                $(eles[i]).addClass("col-md-" + _md);
                $(eles[i]).addClass("col-lg-" + _lg);
                $(eles[i]).css("border", "solid 4px red");
                $(eles[i]).css("clear", "right");
                mdIndex = 0;
                smIndex = 0;
                lgIndex = 0;
                xsIndex = 0;

                mdTotal = 0;
                smTotal = 0;
                lgTotal = 0;
                xsTotal = 0;
            }
            else {
                var xsValue = xsCols[xsIndex] * 1;
                if ($($(eles[i]).children()[0]).attr("xs-span")) {
                    var xsSpanValue = $($(eles[i]).children()[0]).attr("xs-span") * 1;
                    for (var j = 1; j < xsSpanValue; j++) {
                        xsIndex++;
                        if (xsIndex < xsCols.length) {
                            xsValue += xsCols[xsIndex] * 1;
                        }
                    }
                }
                xsTotal += xsValue;
                $(eles[i]).addClass("col-xs-" + xsValue);
                var smValue = smCols[mdIndex] * 1;
                if ($($(eles[i]).children()[0]).attr("sm-span")) {
                    var smSpanValue = $($(eles[i]).children()[0]).attr("sm-span") * 1;
                    for (var j = 1; j < smSpanValue; j++) {
                        smIndex++;
                        if (smIndex < smCols.length) {
                            smValue += smCols[smIndex] * 1;
                        }
                    }
                }
                smTotal += smValue;
                $(eles[i]).addClass("col-sm-" + smValue);
                var mdValue = mdCols[mdIndex] * 1;
                if ($($(eles[i]).children()[0]).attr("md-span")) {
                    var mdSpanValue = $($(eles[i]).children()[0]).attr("md-span") * 1;
                    for (var j = 1; j < mdSpanValue; j++) {
                        mdIndex++;
                        if (mdIndex < mdCols.length) {
                            mdValue += mdCols[mdIndex] * 1;
                        }
                    }
                }
                mdTotal += mdValue;
                $(eles[i]).addClass("col-md-" + mdValue);
                var lgValue = lgCols[lgIndex] * 1;
                if ($($(eles[i]).children()[0]).attr("lg-span")) {
                    var lgSpanValue = $($(eles[i]).children()[0]).attr("lg-span") * 1;
                    for (var j = 1; j < lgSpanValue; j++) {
                        lgIndex++;
                        if (lgIndex < lgCols.length) {
                            lgValue += lgCols[lgIndex] * 1;
                        }
                    }
                }
                lgTotal += lgValue;
                $(eles[i]).addClass("col-lg-" + lgValue);
                if (mdIndex + 1 < mdCols.length) {
                    mdIndex++;
                }
                else {
                    mdIndex = 0;
                }
                if (smIndex + 1 < smCols.length) {
                    smIndex++;
                }
                else {
                    smIndex = 0;
                }
                if (lgIndex + 1 < lgCols.length) {
                    lgIndex++;
                }
                else {
                    lgIndex = 0;
                }
                if (xsIndex + 1 < xsCols.length) {
                    xsIndex++;
                }
                else {
                    xsIndex = 0;
                }
            }

        }
    }
    return divRow;
}
angularDefine(function (mdl) {
    mdl.directive("formData", ["$parse", "$compile", function ($parse, $compile) {
        return {
            restrict: "ECA",
            transclude: true,
            priority: 0,
            template: "<div ng-transclude></div>",
            replace: true,
            link: function (s, e, a) {


                function watch() {
                    if (e.attr("data-template")) {
                        init(decodeURIComponent(e.attr("data-template")));
                        e.removeAttr("data-template");
                    }
                    else {
                        setTimeout(watch, 10);
                    }
                }
                function init(html) {

                    var subScope = s.$new();
                    subScope.data = s.$eval(a.source);
                    s.$watch(a.source, function (o, v) {
                        console.log(v);
                        subScope._ = o;
                        subScope.$applyAsync();

                    })
                    var divRow = $("<div></div>");
                    divRow.html(html);
                    $compile(divRow.contents())(subScope);
                    divRow = makeUpForm(divRow, a);

                    divRow.contents().appendTo(e[0]);

                    s.$apply();
                }
                watch();
            }
        }
    }]);
    mdl.directive("formTemplate", [function () {
        return {
            restrict: "ECA",
            scope: false,
            compile: function (element, attributes) {
                var originHtml = element.html();
                element.empty();
                return {
                    pre: function (s, e, a, c, t) {
                        e.parent().attr("data-template", encodeURIComponent(originHtml));
                        e.remove();

                    },
                    post: function (s, e, a, c, t) {

                    }
                };
            }
        };
    }]);
    mdl.directive("formLayout", ["$parse", "$compile", "$timeout", function ($parse, $compile, $timeout) {
        return {
            restrict: "ECA",
            template: "<div ng-transclude></div>",
            transclude: true,
            replace: true,
            scope: false,
            priority: 0,

            link: function (s, e, a) {


                function watch() {
                    if (e.attr("data-template")) {
                        try {
                            init(decodeURIComponent(e.attr("data-template")));
                            e.removeAttr("data-template");
                        }
                        catch (ex) {
                            console.error(ex);
                        }
                    }
                    else {
                        setTimeout(watch, 10);
                    }
                }
                function init(html) {
                    var divRow = $("<div></div>");
                    divRow.html(html);

                    divRow = makeUpForm(divRow, a);
                    $compile(divRow.contents())(s);

                    divRow.contents().appendTo(e[0]);
                    $timeout(function () {
                        s.$apply();
                    }, 0)
                    //if (s.$$phase !== "$apply") {
                    //    s.$apply();
                    //}
                }
                watch();
            }
        };
    }]);
    mdl.service("$callback", [function () {

        var cmp = {
            onCall: function (callback) {
                cmp.__onCall = callback;
            }
        };
        window["$callback-binding-services"] = cmp;
        return cmp;

    }]);
    mdl.service("$components", ["$parse", function ($parse) {
        return {
            assign: function (id, scope, cmp, prefix) {
                console.log(id);
                prefix = prefix || "$$ui";
                var items = id.split('.');
                var str = prefix + "." + id;
                if (items.length > 1) {
                    str = id.substring(0, id.length - items[items.length - 1].length) + prefix + "." + items[items.length - 1];
                }
                $parse(str).assign(scope, cmp);
            },
            watchOnScreen: function (ele, callback) {
                function ret(ele) {
                    function run(callback2) {
                        if ($.contains($("body")[0], ele[0])) {
                            callback2();
                            delete run;
                        }
                        else {
                            setTimeout(run, 300);
                        }
                    };
                    var me = this;
                    run(function () {
                        callback();
                        delete me
                    })
                }
                return new ret(ele);
            }
        };

    }]);
    /*
     * <ajax 
     *      url=... 
     *      [method='POST|GET|DELETE..'] 
     *      [ng-model=...]
     *      [ng-change=...]
     *      [ng-model-error=...]
     *      [on-complete=...]
     *      [on-error=...]
     *      />
     * */
    mdl.directive("ajax", ["$parse", "$components", function ($parse, $components) {
        var cmpCallback = window["$callback-binding-services"];

        return {
            restrict: "ECA",
            replace: true,
            template: "<div style='display:none'></div>",
            link: function (s, e, a) {

                var urlCaller = s.$eval(a.urlCaller) || s.$parent.$$urlInfo.relUrl.value;

                var cmp = {
                    url: s.$eval(a.url) + "?caller-id=" + encodeURIComponent(urlCaller),
                    urlCaller: s.$eval(a.urlCaller),
                    callId: a.callId,

                    done: function (callback) {
                        cmp._done = callback;
                        return cmp;
                    },
                    caller: function (Params) {
                        if (angular.isFunction(Params)) {
                            cmp.params = (s.$eval(a.postData) || s.$eval(a.params));
                        }
                        else {
                            cmp.params = Params || (s.$eval(a.postData) || s.$eval(a.params));
                        }

                        var postUrl = s.$eval(a.url) || s.$$urlInfo.absUrl.value;
                        if (postUrl.indexOf("?") == -1) {
                            postUrl += "?caller-id=" + encodeURIComponent(urlCaller);
                        }
                        else {
                            postUrl += "&caller-id=" + encodeURIComponent(urlCaller);
                        }
                        if (!postUrl) {
                            throw ("Can not post with undefined url");
                            return;
                        }
                        var sender = new function () {
                            var me = this;
                            this.url = postUrl;
                            this.id = cmp.callId;
                            this.data = cmp.params;
                            this.method = a.method || "POST";
                            this.noMask = a.noMask;
                            this.done = function (callback) {
                                me._onDone = callback;

                                return this;
                            }

                            this.emit = function (err, res) {
                                if ((!err) || (err == null)) {
                                    if (a.ngModel) {
                                        $parse(a.ngModel).assign(s, res);

                                    }
                                    var fn = s.$eval(a.ngChange);
                                    if (angular.isFunction(fn)) {
                                        fn(res);
                                    }
                                    var fnComplete = s.$eval(a.onComplete);
                                    if (angular.isFunction(fnComplete)) {
                                        fnComplete(res);
                                    }
                                    if (me._onDone) {
                                        me._onDone(res);
                                    }
                                    s.$applyAsync();
                                }
                                else {
                                    if (a.ngModelError) {
                                        $parse(a.ngModelError).assgin(s, res);
                                    }
                                    if (a.onError) {
                                        var fn = s.$eval(a.onError);
                                        if (angular.isFunction(fn)) {
                                            fn(res);
                                        }
                                    }
                                }
                            }


                        }

                        return sender;
                    },
                    call: function (Params) {
                        var sender = cmp.caller(Params);
                        sender.done(cmp._done);
                        cmpCallback.__onCall(sender);
                    }
                };
                if (a.id) {
                    $components.assign(a.id, s, cmp, "$ajax")

                }
                s.$watch(function () {
                    var x = s.$eval(a.params);
                    return JSON.stringify(x);
                }, function (v, o) {
                    if (v !== o) {
                        cmp.call();
                    }
                });
            }
        };
    }]);

});
angularDefine(function (mdl) {
    mdl.directive("bsTabs", ["$parse", "$components", "$compile", function ($parse, $components, $compile) {
        return {
            restrict: "ECA",
            template: "<div style='width:100%' class='tabs-container'><ul class='nav nav - tabs' id='header'></ul><div class='tab-content' id='content'></div><div style='display:none' ng-transclude id='tabs'></div></div>",
            transclude: true,
            replace: true,
            link: function (s, e, a) {
                var cmp = {
                    headersElement: $(e[0]).find("#header"),
                    contentElement: $(e[0]).find("#content")
                };
                var tabs = JSON.parse($(e[0]).find("#tabs").attr("data-tab-template"));
                cmp.tabs = [];

                ;
                for (var i = 0; i < tabs.length; i++) {
                    if (tabs[i]) {
                        cmp.tabs.push({
                            tilte: tabs[i].title,
                            content: decodeURIComponent(tabs[i].template)
                        });
                        var li = $("<li></li>");
                        var a = $("<a></a>");
                        a.appendTo(li[0]);
                        a.attr("href", "javascript:void(0)");
                        a.html(tabs[i].title);
                        a.attr("tab-id", i);
                        li.appendTo(cmp.headersElement[0]);
                        a.on("click", function (evt) {
                            var index = $(evt.target).attr("tab-id") * 1;
                            if (cmp.currentTabIndex === index) return;
                            cmp.currentTabIndex = index;
                            if (!cmp.tabs[index].isCompile) {
                                var tabContent = $("<div>" + cmp.tabs[index].content + "</div>");
                                $compile(tabContent.contents())(s);
                                s.$applyAsync();
                                tabContent.contents().appendTo(cmp.contentElement[0]);
                                cmp.tabs[index].isCompile = true;
                            }
                            cmp.contentElement.children().each(function (i) {
                                if (i == index) {
                                    $(cmp.contentElement.children()[index]).show();
                                }
                                else {
                                    $(cmp.contentElement.children()[i]).hide();
                                }

                            });


                        });
                        cmp.tabs[i].aElement = a;
                    }
                }
                $compile(cmp.headersElement.contents())(s);

                s.$apply();
                cmp.tabs[0].aElement.trigger("click");
                if (a.id) {
                    $components.assign(a.id, s, cmp);
                }
                $(e[0]).find("#tabs").remove();

            }
        }
    }]);
    mdl.directive("bsTab", [function () {
        return {
            restrict: "ECA",
            scope: false,
            compile: function (element, attributes) {
                var title = element.attr("title");
                var originHtml = element.html();
                element.empty();
                return {
                    pre: function (s, e, a, c, t) {

                        if (!e.parent().attr("data-tab-template")) {
                            var txt = [{
                                template: encodeURIComponent(originHtml),
                                title: title
                            }];
                            e.parent().attr("data-tab-template", JSON.stringify(txt));
                            e.remove();
                        }
                        else {
                            var txt = JSON.parse(e.parent().attr("data-tab-template"));
                            txt.push({
                                template: encodeURIComponent(originHtml),
                                title: title
                            });
                            e.parent().attr("data-tab-template", JSON.stringify(txt));
                            e.remove();
                        }


                    }
                };
            }
        };
    }]);
});
angularDefine(function (mdl) {
    mdl.service("$webServiceCallbackConfig", [function () {
        var rootUrl = undefined;
        var _onBeforeComplete = undefined;
        var _onBeforeCall = undefined;
        var _onAfterCall = undefined;
        var _onValidateResponse = undefined;
        var _onError = undefined;
        var ws = {
            onBeforeSend: function (value) {
                if (!angular.isFunction(value)) {
                    console.error('param in onBeforeSend must be a function with one param (onBeforeSend(function(sender){})')
                }
                _onBeforeSend = value;
                return ws;
            },
            onError: function (value) {
                if (!angular.isFunction(value)) {
                    console.error('param in onError must be a function with one param (onError(function(sender){})')
                }
                _onError = value;
                return ws;
            },
            onBeforeCall: function (value) {
                if (!angular.isFunction(value)) {
                    console.error('param in onBeforeCall must be a function with one param (onBeforeCall(function(sender){})')
                }
                _onBeforeCall = value;
                return ws;
            },
            onAfterCall: function (value) {
                if (!angular.isFunction(value)) {
                    console.error('param in onAfterCall must be a function with one param (onAfterCall(function(sender){})')
                }
                _onAfterCall = value;
                return ws;
            },
            onValidateResponse: function (value) {
                if (!angular.isFunction(value)) {
                    console.error('param in onValidateResponse must be a function with one param (onValidateResponse(function(sender){})')
                }
                _onValidateResponse = value;
                return ws;
            },
            onBeforeComplete: function (value) {
                _onBeforeComplete = value;
                return ws;
            },
            setRootUrl: function (value) {
                rootUrl = value;
            },
            getRootUrl: function () {
                return rootUrl;
            },
            getCaller: function (scope) {
                function ret(scope) {
                    var me = this;
                    me.scope = scope;
                    me.noMask = function (value) {
                        me._noMask = value;
                        return me;
                    }
                    me.url = function (value) {
                        me._url = value;
                        return me;
                    };
                    me.call = function (serverId) {
                        me._call = serverId;
                        return me;
                    };
                    me.getCall = function () {
                        return me._call;
                    }
                    me.data = function (data) {
                        me._data = data;
                        return me;
                    };
                    me.method = function (value) {
                        me._method = value;
                        return me;
                    }
                    me.done = function (callback) {
                        var sender = {};
                        if (!_onBeforeCall) {
                            console.error("Please call onBeforeCall in angular service name $webServiceCallbackConfig");
                        }
                        _onBeforeCall(me);
                        var mask = undefined;
                        if (!me._noMask) {
                            mask = $("<div class='mask'></div>");
                            mask.appendTo("body");
                        }

                        $.ajax({
                            processData: false,
                            type: me._method,
                            url: me._url,
                            data: JSON.stringify(me._data),
                            success: function (res) {
                                if (!_onAfterCall) {
                                    console.error("Please call onAfterCall in angular service name $webServiceCallbackConfig");
                                    return;
                                }
                                _onAfterCall(me);
                                if (!me._noMask) {
                                    mask.remove();
                                }
                                var data = res;
                                if (angular.isString(res)) {
                                    data = Function("", "return " + res)();
                                }
                                if (!_onValidateResponse) {
                                    console.error("Please call onValidateResponse (has 2 params) in angular service name $webServiceCallbackConfig");
                                    return;
                                }
                                _onValidateResponse(data, callback);
                            },
                            error: function (res) {
                                if (!me._noMask) {
                                    mask.remove();
                                }
                                if (res.status === 200) {
                                    if (res.responseText.substring(0, 1) === "{") {
                                        var data = Function("", "return " + res.responseText)();
                                        _onValidateResponse(data, callback);
                                        return;
                                    }
                                    else {
                                        _onValidateResponse(res.responseText, callback);
                                        return;
                                    }
                                }
                                if (!_onError) {
                                    console.error("Please call onError in angular service name $webServiceCallbackConfig");
                                    return;
                                }
                                _onError(res);
                            },
                            dataType: "json",
                            contentType: "application/json; charset=utf-8"

                        });
                    }
                    me.setHeader = function (key, value) {
                        if (!me._header) {
                            me._header = {};
                        }
                        me._header[key] = value;
                        return me;
                    }
                }
                return new ret(scope);
            }
        }
        return ws;
    }]);
    mdl.service("$post", ["$webServiceCallbackConfig", function ($apiConfig) {

        return function (scope, serverMethod, postData, callback, noMask) {
            if (!serverMethod) {
                return new ret(scope);
            }
            else {
                var caller = $apiConfig.getCaller(scope);
                var url = $apiConfig.getRootUrl() || scope.$$urlInfo.absUrl.value;
                caller.method("POST");
                caller.url(url);
                caller.call(serverMethod);
                caller.data(postData);

                caller.noMask(noMask);
                caller.done(callback);
            }
        }
    }]);
});
angularDefine(function (mdl) {
    mdl.service("$navigator", [function () {
        __navigator__ = {};
        __navigator__onChange = undefined;

        return function (arg) {
            if (angular.isFunction(arg)) {
                __navigator__onChange = arg;
                history_navigator(__navigator__);
                __navigator__.$history.change(function (data) {
                    if (__navigator__onChange) {
                        __navigator__onChange(data);
                    }
                });
            }
            else {
                history_navigator(arg);
            }
        }
    }]);
    mdl.directive("qPartial", ["$http", "$compile", "$timeout", function ($http, $compile, $timeout) {
        return {
            replace: true,
            scope: false,
            link: function (scope, ele, attr) {
                if (scope.$root == scope.$parent) {
                    if (!scope.$$urlInfo) {
                        scope.$$urlInfo = {
                            absUrl: {
                                ref: window.location.href.split('#')[0].split('?')[0],
                                value: window.location.href.split('#')[0].split('?')[0],
                            },
                            relUrl: {
                                ref: window.location.href.split('#')[0].split('?')[0].split("://")[1],
                                value: window.location.href.split('#')[0].split('?')[0].split("://")[1],
                            }
                        }
                    }
                }
                var url = attr.url;
                if (url.length > 2 && (url.substring(0, 2) === "~/")) {
                    url = url.substring(2, url.length);
                }
                url = scope.$$urlInfo.absUrl.ref + "/" + url;
                $http({
                    method: 'GET',
                    url: url
                }).then(function successCallback(response) {
                    var eles = $("<div>" + response.data + "</div>");
                    $compile(eles.contents())(scope);
                    $(ele[0]).replaceWith(eles.children());
                    //$timeout(function () {

                    //    scope.$applyAsync();
                    //}, 100);
                    try {
                        scope.$applyAsync();
                    }
                    catch (ex) {
                        console.error(scope);
                        throw (ex);
                    }


                }, function errorCallback(response) {
                    $(ele[0]).replaceWith("<p>" + url + "</br>" + response.statusText + "</p>");
                    console.error(attr.url + "," + response.statusText);

                });
            }
        }
    }])
});
angularDefine(function (mdl) {
    mdl.service("$cookie", [function () {
        function createCookie(name, value, days) {
            var expires;

            if (days) {
                var date = new Date();
                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                expires = "; expires=" + date.toGMTString();
            } else {
                expires = "";
            }
            document.cookie = encodeURIComponent(name) + "=" + encodeURIComponent(value) + expires + "; path=/";
        }

        function readCookie(name) {
            var nameEQ = encodeURIComponent(name) + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) === ' ')
                    c = c.substring(1, c.length);
                if (c.indexOf(nameEQ) === 0)
                    return decodeURIComponent(c.substring(nameEQ.length, c.length));
            }
            return null;
        }

        function eraseCookie(name) {
            createCookie(name, "", -1);
        }
        return {
            create: function (name, value, days) {
                createCookie(name, value, days);
            },
            getValue: function (name) {
                return readCookie(name);
            },
            remove: function (name) {
                eraseCookie(name);
            }
        }
    }])
});
angularDefine(function (mdl) {
    mdl.service("$msg", [function () {
        return {
            success: function (msg) {
                toastr.success(msg);
            },
            error: function (msg) {
                toastr.error(msg);
            }
            ,
            info: function (msg) {
                toastr.info(msg);
            }
        }
    }]);
    mdl.service("$ext", [function () {
        return {
            apply: function (scope, type) {
                return angular.extend(scope, new type());
            },
            cast: function (scope) { return scope; }


        }
    }]);
    mdl.service("$extend", [function () {
        return function (obj, t) {
            return angular.extend(obj, t);
        }

    }])
    mdl.service("$cast", [function () {
        return function (obj, t) {
            return obj;
        }

    }])
    mdl.service("$expand", [function () {
        return function (obj) {
            return function () {
                return obj;
            }
        }
    }]);
});
angularDefine(function (mdl) {
    function FileObject() {

    };
    FileObject.prototype.setChunkSize = function (value) {
        this._chunkSize = value;
    };
    FileObject.prototype.humanFileSize = function (bytes, si) {
        var thresh = si ? 1000 : 1024;
        if (Math.abs(bytes) < thresh) {
            return bytes + ' B';
        }
        var units = si
            ? ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB']
            : ['KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];
        var u = -1;
        do {
            bytes /= thresh;
            ++u;
        } while (Math.abs(bytes) >= thresh && u < units.length - 1);
        return bytes.toFixed(1) + ' ' + units[u];
    };
    FileObject.prototype.onReadFile = function (callback) {
        this._onReadFile = callback;
    };
    FileObject.prototype.onFinish = function (callback) {
        this._onFinish = callback;
    };
    FileObject.prototype.onReading = function (callback) {
        this._onReading = callback;
    };
    FileObject.prototype.onError = function (callback) {
        this._onError = callback;
    };
    FileObject.prototype.readFile = function (event) {
        this.fileSize = event.target.files[0].size;
        this.strFileSize = this.humanFileSize(this.fileSize, "MB");
        this.fileName = event.target.files[0].name;

        var _file = event.target.files[0];
        var r = new FileReader();
        var _offset = 0;
        this._chunkSize = this._chunkSize | (3 * 1024);
        var length = this._chunkSize | 1024;
        var numOfChunk = Math.floor(this.fileSize / this._chunkSize);
        var chunks = [];
        for (var i = 0; i < numOfChunk; i++) {
            chunks.push(this._chunkSize);
        }
        if (this.fileSize % this._chunkSize > 0) {
            chunks.push(this.fileSize - this._chunkSize * numOfChunk);
        }
        this.chunks = chunks;
        var me = this;
        var _sender = {
            target: this,
            emit: function (error) {
                if (error) {
                    if (me._onError) {
                        me._onError(error);
                    }
                    else {
                        throw (error);
                    }
                    return;
                }
                function _arrayBufferToBase64(buffer) {
                    var binary = '';
                    var bytes = new Uint8Array(buffer);
                    var len = bytes.byteLength;
                    for (var i = 0; i < len; i++) {
                        binary += String.fromCharCode(bytes[i]);
                    }
                    return window.btoa(binary);
                }
                function chunkReaderBlock(off, index, file) {
                    if (index < chunks.length) {
                        var blob = file.slice(off, off + chunks[index]);
                        r.onload = function (e) {
                            var buffer = _arrayBufferToBase64(e.target.result);
                            var sender = {
                                target: me,
                                numOfChunks: chunks.length,
                                length: me.fileSize,
                                readingSize: off + chunks[index],
                                index: index,
                                buffer: buffer,
                                percent: ((off + chunks[index]) / me.fileSize) * 100,
                                emit: function (error) {
                                    sender.error = error;
                                    if (!error) {
                                        chunkReaderBlock(off + chunks[index], index + 1, file);
                                    }
                                    else {
                                        if (me._onError) {
                                            me._onError(error);
                                        }
                                        else {
                                            throw (error);
                                        }
                                    }
                                }
                            };
                            if (me._onReading) {
                                me._onReading(sender);

                            }
                            else {
                                chunkReaderBlock(off + chunks[index], index + 1, file);
                            }
                        };
                        r.readAsArrayBuffer(blob);
                    }
                    else {
                        if (me._onFinish) {
                            me._onFinish(me);
                        }
                    }
                }

                chunkReaderBlock(0, 0, _file);
            }
        }
        if (this._onReadFile) {
            this._onReadFile(_sender);
        }


    };
    mdl.service("$fileReader", [function () {

        return new FileObject();

    }]);
    mdl.service("$utils", [
        "$webServiceCallbackConfig",
        "$post",
        "$url",
        "$dialog",
        '$msg',
        "$fileReader",
        "$nav",
        "$expand",
        "$extend",
        "$cast",
        "$hookKeyPress",
        "$textFormat",
        function ($wsConfig, $post, $url, $dialog, $msg, $fileReader, $nav, $extCast, $extend, $cast, $hookKeyPress, $textFormat) {
            console.log($textFormat);
            /**
             * interface IUtils {
                $wsConfig: IWebServiceCallbackConfig;
                $post: IPost;
                $url: IUrl;
                $dialog: IDialog;
                $msg: IMsg;
                $fileReader: IFileReader;
                $nav: INav;
                $extCast: IExpand;
                $extend: IExtend;
                $cast: ICast;
            }
             */
            return {
                $wsConfig: $wsConfig,
                $post: $post,
                $url: $url,
                $dialog: $dialog,
                $msg: $msg,
                $fileReader: $fileReader,
                $nav: $nav,
                $extCast: $extCast,
                $extend: $extend,
                $cast: $cast,
                $hookKeyPress: $hookKeyPress,
                $textFormat: $textFormat
            }
        }])
});
angularDefine(function (mdl) {
    mdl.service("$findScope", [function () {
        return findScopeById(scopeIdOrScope);
    }]);
});
var quicky = {
    cast: function (obj) {
        return obj;
    },
    extend: function (obj, t) {
        return angular.extend(obj, t);
    }
}
var Q = {
    angularDefine: function (callback) {
        return angularDefine(callback);
    }
}