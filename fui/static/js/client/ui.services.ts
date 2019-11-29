/// <reference path="types/summernote.d.ts" />
/// <reference path="types/angular/index.d.ts" />
Q.angularDefine(mdl => {
    mdl.service("$ui", [() => {

        class clsUiService implements Q.services.IUI {
            createContextMenu(target: Element, popupMenu: Element, onHide: () => void) {
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
                var mouseMove = (evt: JQueryEventObject) => {
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
                }
                $(document).bind("mousemove", mouseMove);

                $(popupMenu).appendTo("body");
                //$(popupMenu).bind("mousemove", evt => {
                //    evt.stopImmediatePropagation();
                //    evt.stopPropagation();
                //    return false;
                //});

            }
            getPosition(ele: Element): { x: number; y: number; } {
                var el = ele as HTMLElement;
                var el2 = el;
                var curtop = 0;
                var curleft = 0;
                if (document.getElementById || document.all) {
                    do {
                        curleft += el.offsetLeft - el.scrollLeft;
                        curtop += el.offsetTop - el.scrollTop;
                        el = el.offsetParent as HTMLElement;
                        el2 = el2.parentNode as HTMLElement;
                        while (el2 != el) {
                            curleft -= el2.scrollLeft;
                            curtop -= el2.scrollTop;
                            el2 = el2.parentNode as HTMLElement;
                        }
                    } while (el.offsetParent);

                } else if (document["layers"]) {
                    curtop += el["y"];
                    curleft += el["x"];
                }
                return { x: curleft, y: curtop };
            }


        }
        var ret = new clsUiService();
        return ret;
    }]);
    mdl.service("$hookKeyPress", [() => {
        return (eleObj: JQuery, callback: (keyCode: number, hotKeys?: { isCtrl: boolean, isShift: boolean, isAlter: boolean }) => boolean) => {
            var onKeyPress = (evt) => {
                var ret = callback(evt.keyCode);
                if (ret === false) {
                    evt.preventDefault();
                    evt.stopImmediatePropagation();
                    evt.stopPropagation();
                }
            }

            function apply(ele: JQuery) {
                function watch() {
                   
                    if ($.contains($("body")[0], ele[0])) {
                        $(window).keydown((e:any) => {
                            if ($.contains(ele[0], e.target) || (e.target === ele[0])) {
                                $(window).bind("keydown", onKeyPress);
                            }
                            else {
                                
                                $(window).unbind("keydown", onKeyPress);
                            }
                        });
                        $(window).click((e: any) => {
                            if ($.contains(ele[0], e.target) || (e.target===ele[0])) {
                                $(window).bind("keydown", onKeyPress);
                            }
                            else {
                                
                                $(window).unbind("keydown", onKeyPress);
                            }
                        });
                        setTimeout(() => { ele.trigger("click"); }, 500)
                    }
                    else {
                        $(window).unbind("keydown", onKeyPress);
                        setTimeout(watch, 10);
                    }
                }
                watch();
            }
            new apply(eleObj);

        }
    }]);
    mdl.service("$textFormat", [() => {
        return function (..._args) {
            var args = _args;
            if (_args.length === 0) return;
            if (angular.isUndefined(_args[0])) return "";
            if (angular.isArray(_args[0])) {
                args = _args[0];
            }
            var txt: string = args[0].toString();
            var processTxt = "";
            for (var i = 1; i < args.length; i++) {
                while (txt.lastIndexOf(`{${(i - 1)}}`) > -1) {
                    txt = txt.replace(`{${(i - 1)}}`, args[i]);
                }
            }
            while (txt.indexOf("\\{") > -1) {
                txt = txt.replace("\\{", "{");
            }
            while (txt.indexOf("\\}") > -1) {
                txt = txt.replace("\\}", "}");
            }
            return txt;
        }
    }]);
    mdl.directive("htmlContent", [() => {
        return {
            restrict: "A",
            link: (s, e, a) => {
                a.$observe<string>("htmlContent", (n: string) => {
                    e.html(n);
                });
            }
        }
    }]);
    mdl.directive("sn", [() => {

        return {
            restrict: "ECA",
            link: (s, e, a) => {
                $(e[0]).summernote({
                    callbacks: {
                        onBlur: () => {
                            alert($(e[0]).summernote('code'))
                        }
                    }
                });
                $(e[0]).summernote("code", "your text");
            }
        }
    }])
});