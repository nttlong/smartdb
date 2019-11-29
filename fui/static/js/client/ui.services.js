/// <reference path="types/summernote.d.ts" />
/// <reference path="types/angular/index.d.ts" />
Q.angularDefine(function (mdl) {
    mdl.service("$ui", [function () {
            var clsUiService = /** @class */ (function () {
                function clsUiService() {
                }
                clsUiService.prototype.createContextMenu = function (target, popupMenu, onHide) {
                    var pos = this.getPosition(target);
                    $(popupMenu).css({
                        "position": "absolute",
                        left: -5000,
                        "float": "left"
                    }).show();
                    var rect = {
                        w: $(popupMenu).outerWidth(),
                        h: $(popupMenu).outerHeight(),
                        x: pos.x,
                        y: pos.y
                    };
                    if (rect.x + rect.w + 10 > $(window).innerWidth()) {
                        rect.x = $(window).innerWidth() - rect.w;
                    }
                    if (rect.y + rect.h + 10 > $(window).innerHeight()) {
                        rect.y = $(window).innerHeight() - rect.h;
                    }
                    $(popupMenu).css({
                        "display": "block",
                        left: rect.x,
                        top: rect.y,
                        "position": "absolute",
                        "float": "none",
                        "z-index": "40000"
                    });
                    var mouseMove = function (evt) {
                        var x = evt.pageX;
                        var y = evt.pageY;
                        var rb = {
                            x: $(popupMenu).position().left,
                            y: $(popupMenu).position().top,
                            w: $(popupMenu).width(),
                            h: $(popupMenu).height()
                        };
                        if ((x < rb.x) ||
                            (y < rb.y) ||
                            (x > rb.w + rb.x) ||
                            (y > rb.h + rb.y)) {
                            $(popupMenu).hide();
                            $(window).unbind("mousemove", mouseMove);
                            onHide();
                        }
                        else {
                        }
                    };
                    $(document).bind("mousemove", mouseMove);
                    $(popupMenu).appendTo("body");
                    //$(popupMenu).bind("mousemove", evt => {
                    //    evt.stopImmediatePropagation();
                    //    evt.stopPropagation();
                    //    return false;
                    //});
                };
                clsUiService.prototype.getPosition = function (ele) {
                    var el = ele;
                    var el2 = el;
                    var curtop = 0;
                    var curleft = 0;
                    if (document.getElementById || document.all) {
                        do {
                            curleft += el.offsetLeft - el.scrollLeft;
                            curtop += el.offsetTop - el.scrollTop;
                            el = el.offsetParent;
                            el2 = el2.parentNode;
                            while (el2 != el) {
                                curleft -= el2.scrollLeft;
                                curtop -= el2.scrollTop;
                                el2 = el2.parentNode;
                            }
                        } while (el.offsetParent);
                    }
                    else if (document["layers"]) {
                        curtop += el["y"];
                        curleft += el["x"];
                    }
                    return { x: curleft, y: curtop };
                };
                return clsUiService;
            }());
            var ret = new clsUiService();
            return ret;
        }]);
    mdl.service("$hookKeyPress", [function () {
            return function (eleObj, callback) {
                var onKeyPress = function (evt) {
                    var ret = callback(evt.keyCode);
                    if (ret === false) {
                        evt.preventDefault();
                        evt.stopImmediatePropagation();
                        evt.stopPropagation();
                    }
                };
                function apply(ele) {
                    function watch() {
                        if ($.contains($("body")[0], ele[0])) {
                            $(window).keydown(function (e) {
                                if ($.contains(ele[0], e.target) || (e.target === ele[0])) {
                                    $(window).bind("keydown", onKeyPress);
                                }
                                else {
                                    $(window).unbind("keydown", onKeyPress);
                                }
                            });
                            $(window).click(function (e) {
                                if ($.contains(ele[0], e.target) || (e.target === ele[0])) {
                                    $(window).bind("keydown", onKeyPress);
                                }
                                else {
                                    $(window).unbind("keydown", onKeyPress);
                                }
                            });
                            setTimeout(function () { ele.trigger("click"); }, 500);
                        }
                        else {
                            $(window).unbind("keydown", onKeyPress);
                            setTimeout(watch, 10);
                        }
                    }
                    watch();
                }
                new apply(eleObj);
            };
        }]);
    mdl.service("$textFormat", [function () {
            return function () {
                var _args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    _args[_i] = arguments[_i];
                }
                var args = _args;
                if (_args.length === 0)
                    return;
                if (angular.isUndefined(_args[0]))
                    return "";
                if (angular.isArray(_args[0])) {
                    args = _args[0];
                }
                var txt = args[0].toString();
                var processTxt = "";
                for (var i = 1; i < args.length; i++) {
                    while (txt.lastIndexOf("{" + (i - 1) + "}") > -1) {
                        txt = txt.replace("{" + (i - 1) + "}", args[i]);
                    }
                }
                while (txt.indexOf("\\{") > -1) {
                    txt = txt.replace("\\{", "{");
                }
                while (txt.indexOf("\\}") > -1) {
                    txt = txt.replace("\\}", "}");
                }
                return txt;
            };
        }]);
    mdl.directive("htmlContent", [function () {
            return {
                restrict: "A",
                link: function (s, e, a) {
                    a.$observe("htmlContent", function (n) {
                        e.html(n);
                    });
                }
            };
        }]);
    mdl.directive("sn", [function () {
            return {
                restrict: "ECA",
                link: function (s, e, a) {
                    $(e[0]).summernote({
                        callbacks: {
                            onBlur: function () {
                                alert($(e[0]).summernote('code'));
                            }
                        }
                    });
                    $(e[0]).summernote("code", "your text");
                }
            };
        }]);
});
//# sourceMappingURL=ui.services.js.map