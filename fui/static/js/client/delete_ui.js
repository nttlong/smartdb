window.___bootstrapUI___= angular.module("q-ui", []);

var _dialog_root_url;
var ___uiReady__=undefined;
function uiReady(callback){
    ___uiReady__=callback;
}
var ___rootDir__=undefined;
function angularDefine(callback){
    if(!window.___bootstrapUI___){
        window.___bootstrapUI___= angular.module("q-ui", []);
    }
    callback(window.___bootstrapUI___);
}
function dialog_root_url(value) {
    _dialog_root_url = value;
}
function findScopeById(id){
    var eles=$(".ng-scope");
    var ret=undefined;
    for(var i=0;i<eles.length;i++){
        ret=angular.element(eles[i]).scope();
        if(ret.$id===id){
            break;
        }
        else {
            ret=undefined;
        }
    }
    return ret;
}
function dialog($scope) {
    if(angular.isNumber($scope)){
        $scope=findScopeById($scope);
    }
    function getScript(content) {
        if (content.indexOf("<body>") > -1) {
            var x = content.indexOf("<body>") + "<body>".length;
            var y = content.indexOf("</body>");
            content = content.substring(x, y);
        }
        var ret = [];
        var i = content.indexOf("<script>");
        while (i > -1) {
            var j = content.indexOf("</script>", i);
            var script = content.substring(i + "<script>".length, j);
            ret.push(script);
            content = content.substring(0, i) + content.substring(j + "</script>".length, content.length);
            i = content.indexOf("<script>");
        }
        return {
            scripts: ret,
            content: content
        };
    }
    function compile(scope, scripts, content,_params) {
        var subScope = scope.$new(true, scope);

        for (var i = 0; i < scripts.length; i++) {
            var fn = eval(scripts[i]);
            fn(subScope,_params);
        }
        var frm = $('<div><div class="modal fade" id="myModal" role="dialog">' +
            '<div class="modal-dialog">' +
            '<div class="modal-content">' +

            '<div class="modal-header">' +


            '<h4 class="modal-title"><img src=""/ style="height:40px"><span>...</span></h4>' +
            '<button type="button" class="close" data-dismiss="modal">&times;</button>' +
            '</div>' +
            '<div class="modal-body">' +

            '</div>' +
            '</div></div>'
        );
        var $ele = $("<div>" + content + "</div>");

        var child = $($ele.children()[0])
        frm.attr("title",child.attr("title"))
        frm.attr("icon",child.attr("icon"))
        $ele.children().appendTo(frm.find(".modal-body")[0]);
        subScope.$element=frm

        subScope.$watch(function () {
            return subScope.$element.find(".modal-body").children().attr("title");
        }, function (val) {
            if(angular.isDefined(val)){
                subScope.$element.find(".modal-title span").html(val);
            }
        });
        subScope.$watch(function () {
            return subScope.$element.find(".modal-body").children().attr("icon");
        }, function (val) {
            if(angular.isDefined(val)){
                subScope.$element.find(".modal-title img").attr("src", val);
            }
            else{
                subScope.$element.find(".modal-title img").hide()
            }

        });
        if(!$scope.$root.$compile){
            throw("Please use '$compile' at controller then set '$scope.$root.$compile=$compile'")
        }
        $scope.$root.$compile(frm.contents())(subScope);
        subScope.$element = $(frm.children()[0]);
        subScope.$applyAsync();

        return subScope;
    }
    function ret(scope) {
        var me = this;
        me.url = function (_url) {
            if (_dialog_root_url) {
                me._url = _dialog_root_url + "/" + _url;
            }
            else {
                me._url = _url;
            }

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
        me.done = function (callback) {

            var $mask = $("<div class='mask'></div>").appendTo("body");
            $.ajax({
                method: "GET",
                url: me._url,
                success: function (res) {
                    $mask.remove();
                    var ret = getScript(res);
                    var sScope = compile(scope, ret.scripts, ret.content, me._params);
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
                    sScope.$element.modal()
                        .on("hidden.bs.modal", function () {
                            sScope.$element.remove();

                        });

                    
                    sScope.$doClose = function () {
                        sScope.$element.modal('hide');
                    };
                    watch();
                },
                error: function (ex) {
                    $mask.remove();
                    //                    if(instance && instance.onAfterCall){
                    //                        instance.onAfterCall(me,sender);
                    //                    }
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
    }
    return new ret($scope);
}
function $url() {
    function ret() {
        var me = this;
        me.data = {};
        if (window.location.href.indexOf("#") > -1) {
            var ref = window.location.href.split('#')[1];
            var items = ref.split('&');
            for (var i = 0; i < items.length; i++) {
                me.data[items[i].split('=')[0]] = items[i].split('=')[1];
            }
        }
        me.param = function (key, value) {
            me.data[key] = value;
            return me;
        }
        me.clear = function () {
            me.data = {}
            return me;
        }
        me.url = function () {
            var ret = "";
            var keys = Object.keys(me.data);
            for (var i = 0; i < keys.length; i++) {
                ret += keys[i] + "=" + me.data[keys[i]] + "&"
            }
            return ret.substring(0, ret.length - 1);
        }
        me.apply = function () {
            window.location.href = "#" + decodeURIComponent( me.url());
        }

    }
    return new ret();
}
function history_navigator($scope) {
    var oldUrl;
    function historyMonitorStart(handler) {
        function run() {
            if (oldUrl != window.location.href) {

                if (historyChangeCallback.length > 0) {
                    if (window.location.href.indexOf('#') > -1) {
                        var data = {};
                        var url = window.location.href.split('#')[1];
                        var items = url.split('&');
                        var ret = {};
                        for (var i = 0; i < items.length; i++) {
                            data[items[i].split('=')[0]] = decodeURI(window["unescape"](items[i].split('=')[1]));
                        }
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
                                }, 300);

                            }
                        }

                    }
                    else {
                        historyChangeCallback[historyChangeCallback.length - 1]({});
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
                if (window.location.href.indexOf('#') == -1)
                    return {};
                var url = window.location.href.split('#')[1];
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

function ng_app(modulelName,controllerName,injection, fn,ngStartSymbol,ngEndSymbol) {
    injection.push("imageupload","q-ui");
    var app = angular.module(modulelName, injection, function ($interpolateProvider) {
        if(ngStartSymbol){
            $interpolateProvider.startSymbol(ngStartSymbol);
            $interpolateProvider.endSymbol(ngStartSymbol);
        }
        
    });
    var controller = app.controller(controllerName, ["$compile", "$scope", function ($compile, $scope) {
        $scope.$root.$compile = $compile;
        $scope.$root.$dialog =dialog;
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
    mdl.directive("qTemplate", ["$compile", function ($compile) {


        function loadUrl(url, handler) {
            var $mask = $("<div class='mask'></div>");
            $mask.appendTo("body");
            $.ajax({
                url: _appDirectiveSetRootUrl ? _appDirectiveSetRootUrl + "/" + url : url,
                method: "get",
                success: function (res) {
                    $mask.remove();
                    handler(undefined, { url: url, res: res });
                },
                error: function (ex) {
                    $mask.remove();
                    var tab = window.open('about:blank', '_blank');
                    // while(ex.responseText.indexOf(String.fromCharCode(10))>-1){
                    //     ex.responseText= ex.responseText.replace(String.fromCharCode(10),"<br/>");
                    // }
                    tab.document.write(ex.responseText); // where 'html' is a variable containing your HTML
                    tab.document.close();


                }
            })
        }
        function getScript(res) {
            var content = res.res;
            if (content.indexOf("<body>") > -1) {
                var x = content.indexOf("<body>") + "<body>".length;
                var y = content.indexOf("</body>", x);
                content = content.substring(x, y);
            }
            var ret = [];
            var i = content.indexOf("<script>");
            while (i > -1) {
                var j = content.indexOf("</script>", i);
                var script = content.substring(i + "<script>".length, j);
                ret.push(script);
                content = content.substring(0, i) + content.substring(j + "</script>".length, content.length);
                i = content.indexOf("<script>");
            }
            return {
                scripts: ret,
                content: content,
                url: res.url
            };
        }
        function compile(scope, scripts, content, url) {

            var subScope = scope.$new(true, scope);

            var $ele = $("<div>" + content + "</div>");
            subScope.$element = $ele.children();
            $compile($ele.contents())(subScope);
            subScope.$applyAsync();

            return {
                scope: subScope,
                run: function () {
                    if (scripts && (scripts.length > 0)) {
                        for (var i = 0; i < scripts.length; i++) {
                            try {

                                var fn = Function("var ret=" + scripts[i] + ";return ret")();
                                fn(subScope);
                            }
                            catch (ex) {
                                
                                console.log(scripts[i]);
                                throw ({
                                    error: ex,
                                    url: url
                                });
                            }
                        }
                    }
                }

            };
        }
        return {
            restrict: "ACE",
            link: function (scope, ele, attr) {
                
                attr.$observe("url", function (value) {

                    if (value === "") return;
                    if (window.__debug__) {
                        window.__debug__++;
                        value = value + "?"
                            + window.__debug__;
                    }
                    loadUrl(value, function (err, content) {
                        var ret = getScript(content);
                        var retObj = compile(scope, ret.scripts, ret.content, ret.url);
                        ele.empty();
                        retObj.scope.$element.appendTo(ele[0]);
                        function watch() {
                            if (!$.contains($("body")[0], retObj.scope.$element[0])) {
                                retObj.scope.$destroy();
                            }
                            else {
                                if (retObj.run) {
                                    setTimeout(function () {
                                        retObj.run();
                                        retObj.run = undefined;
                                    }, 50);

                                }
                                setTimeout(watch, 500);
                            }
                        }
                        watch();
                    });
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
                div.attr("ng-show", ele.attr("ng-show"))

            }
            if (ele.attr("ng-if")) {
                div.attr("ng-if", ele.attr("ng-if"))

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
                    ele.attr("xs-span", ele.attr("span"))
                }
                if (!ele.attr("sm-span")) {
                    ele.attr("sm-span", ele.attr("span"))
                }
                if (!ele.attr("md-span")) {
                    ele.attr("md-span", ele.attr("span"))
                }
                if (!ele.attr("lg-span")) {
                    ele.attr("lg-span", ele.attr("span"))
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

                    divRow.contents().appendTo(e[0])

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
    mdl.directive("formLayout", ["$parse", "$compile", function ($parse, $compile) {
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
                        init(decodeURIComponent(e.attr("data-template")));
                        e.removeAttr("data-template");
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

                    s.$apply();
                }
                watch();
            }
        };
    }]);
});