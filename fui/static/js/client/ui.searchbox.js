Q.angularDefine(function (mdl) {
    mdl.directive("sb", ["$parse", function ($parse) {
            return {
                template: "<div style=\"width:100%;display:flex;flex-direction:row;padding:0\" class=\"search-box input-group\">\n                       \n                        <input type=\"text\" placeholder=\"\" style=\"flex:1;border:none !important\" id=\"txtSearch\"/>\n                            <button style=\"padding:4px;font-size:14px;display:none\" id=\"btnClear\" class=\"btn\">\n                                <span class=\"fa fa-times\"></span>\n                                <i class=\"bowtie-icon bowtie-close\"></i>\n                            </button>\n                            \n                            <button style=\"padding:4px;font-size:14px\" id=\"btnSearch\" class=\"btn\">\n                               \n                                <i class=\"bowtie-icon bowtie-search\"></i>     \n                            </button>\n                        </div>",
                replace: true,
                link: function (s, e, a) {
                    var isManualChange = false;
                    a.$observe("placeholder", function (v) {
                        e.find("#txtSearch").attr("placeholder", v);
                    });
                    e.find("#txtSearch").bind("keyup", function (evt) {
                        if (evt.keyCode === 13) {
                            evt.preventDefault();
                            evt.stopImmediatePropagation();
                            evt.stopPropagation();
                            var val = undefined;
                            if (angular.isDefined(e.find("#txtSearch").val())) {
                                val = e.find("#txtSearch").val();
                            }
                            $parse(a.ngModel).assign(s, val);
                            var fn = s.$eval(a.ngChange);
                            if (angular.isFunction(fn)) {
                                fn(val);
                            }
                            return;
                        }
                        if (e.find("#txtSearch").val() !== "") {
                            e.find("#btnClear").show();
                        }
                        else {
                            e.find("#btnClear").hide();
                        }
                    });
                    e.find("#btnSearch").bind("click", function (evt) {
                        $parse(a.ngModel).assign(s, e.find("#txtSearch").val());
                        var fn = s.$eval(a.ngChange);
                        if (angular.isFunction(fn)) {
                            fn(e.find("#txtSearch").val());
                        }
                    });
                    e.find("#btnClear").bind("click", function () {
                        isManualChange = true;
                        e.find("#txtSearch").val("");
                        e.find("#btnClear").hide();
                        $parse(a.ngModel).assign(s, undefined);
                        console.log("clear search");
                        var fn = s.$eval(a.ngChange);
                        if (angular.isFunction(fn)) {
                            fn(undefined);
                        }
                    });
                    s.$watch(a.ngModel, function (n, o) {
                        if (isManualChange)
                            return;
                        e.find("#txtSearch").val(n);
                        if (angular.isDefined(n)) {
                            e.find("#btnClear").show();
                        }
                        isManualChange = false;
                    });
                }
            };
        }]);
});
//# sourceMappingURL=ui.searchbox.js.map