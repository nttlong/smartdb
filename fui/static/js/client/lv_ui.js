var lv;
(function (lv) {
    var ui;
    (function (ui) {
        function toggleMenu(ele, selector) {
            $(ele).find(selector).toggleClass("show");
        }
        ui.toggleMenu = toggleMenu;
        function toggleColapse(ele) {
            var container = $($(ele).parents(".dbParentFunction")[0]);
            container.find(".childGroup").toggle();
            var indicator = container.find("#menu-item-indicator");
            if (indicator.hasClass("bowtie-chevron-down-light")) {
                indicator.removeClass("bowtie-chevron-down-light")
                    .addClass("bowtie-chevron-right-light");
            }
            else {
                indicator.removeClass("bowtie-chevron-right-light")
                    .addClass("bowtie-chevron-down-light");
            }
        }
        ui.toggleColapse = toggleColapse;
        ui.mdl = angular.module("lv-ui", []);
        ui.mdl.directive("vBox", [function () {
                return {
                    template: "<div class=\"item\">\n                                <div class=\"form-group\">\n                                    <label control-id=\"lblCaption\"></label>\n                                    \n                                </div>\n                                <div ng-transclude style=\"display:none\" control-id=\"container\"></div>\n                           </div>",
                    transclude: true,
                    replace: true,
                    link: function (s, e, a) {
                        a.$observe("title", function (v) {
                            $(e[0]).find('[control-id="lblCaption"]').html(v);
                        });
                        s.$watch(function () {
                            return $(e[0]).find('[control-id="container"]').children().length;
                        }, function (v, o) {
                            if (v > 0) {
                                var ele = $(e[0]).find('[control-id="container"]').children()[0];
                                $(ele).appendTo($(e[0]).find(".form-group")[0]);
                                autoFormat(ele);
                            }
                        });
                        var autoFormat = function (ele) {
                            if ((ele.tagName === "INPUT") &&
                                (ele.attributes["type"].value !== "button")) {
                                $(ele).addClass("style-input form-control ip_txt");
                            }
                            if ((ele.tagName === "TEXTAREA")) {
                                $(ele).addClass("style-input form-control ip_txt");
                            }
                        };
                    }
                };
            }]);
        ui.mdl.directive("hContainer", [function () {
                return {
                    restrict: "ECA",
                    template: "<div class=\"box_02 col_2 clearfix\" ng-transclude></div>",
                    transclude: true,
                    replace: true,
                    link: function (s, e, a) {
                        a.$observe("cols", function (v) {
                            alert(v);
                        });
                    }
                };
            }]);
        ui.mdl.directive("hBox", [function () {
                return {
                    restrict: "ECA",
                    template: "<div class=\"box_02  clearfix\" ng-transclude></div>",
                    transclude: true,
                    replace: true,
                    link: function (s, e, a) {
                        a.$observe("cols", function (v) {
                            $(e[0]).addClass("col_" + v);
                        });
                    }
                };
            }]);
        ui.mdl.directive("vContainer", [function () {
                return {
                    template: "<div class=\"box_content\">\n                                <dl class=\"mb-0\">\n                                    <dt>\n                                        <a class=\"title_02 bg_gray_01 \" control-id=\"indicator\">\n                                            <i class=\"bowtie-icon bowtie-navigate-back-circle float-left ic_arrow\"></i>\n                                            <span control-id=\"lblCaption\"></span>\n                                        </a>\n                                    </dt>\n                                    <dd>\n                                        <div class=\"multi-collapse\" ng-transclude control-id=\"content\"></div>\n                                    </dd>\n                                </dl>    \n                            </div>",
                    replace: true,
                    transclude: true,
                    restrict: "ECA",
                    link: function (s, e, a) {
                        e.find('[control-id="indicator"]').click(function (evt) {
                            e.find('[control-id="indicator"]').toggleClass("collapsed");
                            e.find('[control-id="content"]').toggleClass("collapse");
                        });
                        a.$observe("title", function (v) {
                            e.find("[control-id='lblCaption']").html(a.title);
                        });
                    }
                };
            }]);
        ui.mdl.directive("appToolbar", ["$parse", function ($parse) {
                return {
                    replace: true,
                    template: "\n                            <ul id=\"divHeader\" class=\"box_btn mb-0\">\n                                <li id=\"btnRefresh\" class=\"btn_function \" style=\"display:none\"><a class=\"btn_refresh btn_30\"><i class=\"bowtie-icon bowtie-navigate-refresh\"></i></a></li>\n                                <li id=\"btnAdd\" class=\"btn_function \" title=\"Th\u00EAm m\u1EDBi\" style=\"display:none\"><a  class=\"btn_add btn_30 \"><i class=\"bowtie-icon bowtie-math-plus\"></i></a></li>\n                                <li id=\"btnCopy\" class=\"btn_function \" title=\"Sao ch\u00E9p\" style=\"display:none\"><a class=\"btn_add btn_30 \"><i class=\"bowtie-icon bowtie-edit-copy\"></i></a></li>\n                                <li id=\"btnEdit\" class=\"btn_function \" title=\"C\u1EADp nh\u1EADt\" style=\"display:none\"><a  class=\"btn_edit btn_30 btn_function\"><i class=\"bowtie-icon bowtie-edit-outline\"></i></a></li>\n                                <li id=\"btnDelete\" class=\"btn_function \" title=\"Xo\u00E1 b\u1ECF\" style=\"display:none\"><a  class=\"btn_del btn_30 btn_function\"><i class=\"bowtie-icon bowtie-edit-delete\"></i></a></li>\n                                <li id=\"btnExport\" class=\"btn_function \" title=\"Export to Excel\" style=\"display:none\"><a  class=\"btn_del btn_30 btn_function\"><i class=\"bowtie-icon bowtie-storyboard\"></i></a></li>\n                                <li id=\"btnImport\" class=\"btn_function \" title=\"Import from Excel\" style=\"display:none\"><a  class=\"btn_del btn_30 btn_function\"><i class=\"bowtie-icon bowtie-column-option\"></i></a></li>\n                                \n                                <li id=\"txtSearchBox\">\n                                    <div class=\"btn_search\">\n                                        <div class=\"input-group\">\n                                            <input id=\"inputSearch\" type=\"text\" class=\"form-control\" placeholder=\"\">\n                                            <span class=\"input-group-btn\">\n                                                <button  class=\"btn\" type=\"button\"><i class=\"bowtie-icon bowtie-search\"></i></button>\n                                            </span>\n                                        </div>\n                                    </div>\n                                </li>\n                                <li id=\"btnSetting\" class=\"setting dropdown btn_function \" style=\"display:none\">\n                                    <a class=\"dropdown-toggle btn_refresh btn_30\" data-toggle=\"dropdown\" role=\"button\" aria-haspopup=\"true\" aria-expanded=\"false\" title=\"Setting\"> <i class=\"bowtie-icon bowtie-settings-gear-outline\"></i></a>\n                                    <ul class=\"dropdown-menu animated\" style=\"right: 0px; left: 0px; top: 0px; position: absolute; transform: translate3d(0px, 30px, 0px); will-change: transform;\" x-placement=\"bottom-start\">\n                                        <li style=\"width: 100%;\">\n                                            <div class=\"message-center\">\n                                                <a ng-click=\"callMethod('')\">\n                                                    <div class=\"mail-content\"><span class=\"mail-desc\">abc</span> </div>\n                                                </a>\n                                                <a ng-click=\"callMethod('')\">\n                                                    <div class=\"mail-content\"><span class=\"mail-desc\">xyz</span> </div>\n                                                </a>\n                                            </div>\n                                        </li>\n                                    </ul>\n                                </li>\n                            </ul>",
                    link: function (s, e, a) {
                        $("#application-toolbar").empty();
                        var btnRefresh = $(e[0]).find("#btnRefresh");
                        var btnAdd = $(e[0]).find("#btnAdd");
                        var btnCopy = $(e[0]).find("#btnCopy");
                        var btnEdit = $(e[0]).find("#btnEdit");
                        var btnDelete = $(e[0]).find("#btnDelete");
                        var txtSearchBox = $(e[0]).find("#txtSearchBox");
                        var btnExport = $(e[0]).find("#btnExport");
                        var btnImport = $(e[0]).find("#btnImport");
                        if (a.onRefresh) {
                            btnRefresh.show();
                            btnRefresh.click(function (evt) {
                                s.$eval(a.onRefresh);
                            });
                        }
                        if (a.onAddNew) {
                            btnAdd.show();
                            btnAdd.click(function (evt) {
                                s.$eval(a.onAddNew);
                            });
                        }
                        if (a.onCopy) {
                            btnCopy.show();
                            btnCopy.click(function (evt) {
                                s.$eval(a.onCopy);
                            });
                        }
                        if (a.onEdit) {
                            btnEdit.show();
                            btnEdit.click(function (evt) {
                                s.$eval(a.onEdit);
                            });
                        }
                        if (a.onDelete) {
                            console.log(a.onDelete);
                            btnDelete.show();
                            btnDelete.click(function (evt) {
                                s.$eval(a.onDelete);
                            });
                        }
                        if (a.onSearch) {
                            if (!a.ngModelSearch) {
                                throw "ng-model-search was not found if on-search is enable";
                            }
                            txtSearchBox.show();
                            txtSearchBox.find("#inputSearch").keydown(function (evt) {
                                if (evt.keyCode === 13) {
                                    $parse(a.ngModelSearch).assign(s, txtSearchBox.find("#inputSearch").val());
                                    s.$eval(a.onSearch);
                                    evt.preventDefault();
                                    evt.stopImmediatePropagation();
                                }
                            });
                            txtSearchBox.find(".btn").click(function (evt) {
                                $parse(a.ngModelSearch).assign(s, txtSearchBox.find("#inputSearch").val());
                                s.$eval(a.onSearch);
                            });
                        }
                        if (a.onExport) {
                            btnExport.show();
                            btnExport.click(function (evt) {
                                s.$eval(a.onExport);
                            });
                        }
                        if (a.onImport) {
                            btnImport.show();
                            btnImport.click(function (evt) {
                                s.$eval(a.onImport);
                            });
                        }
                        $(e[0]).appendTo($("#application-toolbar")[0]);
                    }
                };
            }]);
        ui.mdl.directive("qB", [function () {
                return {
                    restrict: "ECA",
                    replace: true,
                    transclude: true,
                    template: "<button class=\"btn btn_save_02\">\n                            <i class=\"bowtie-icon\"></i>\n                            <span ng-transclude></span>\n                        </button>",
                    link: function (s, e, a) {
                        $(e[0]).find(".bowtie-icon").addClass("bowtie-" + a.icon);
                        a.$observe("icon", function (v) {
                            $(e[0]).find(".bowtie-icon").addClass("bowtie-" + v);
                        });
                    }
                };
            }]);
        ui.mdl.directive("cB", ["$parse", function ($parse) {
                return {
                    template: "<div class=\"form-group\" style=\"min-height: 30px\">\n                                <label for=\"isDefault\">\n                                    <i control-id=\"icon\" class=\"bowtie-icon bowtie-checkbox-empty\" style=\"color: black;font-size:18px;padding-right:10px;\"></i> \n                                    <span control-id=\"lbl\"></span>\n                                 </label>\n                            </div>",
                    replace: true,
                    link: function (s, e, a) {
                        var cmp = {
                            titleTrue: a.titleTrue,
                            titleFalse: a.titleFalse,
                            val: false,
                            label: $(e[0]).find('[control-id="lbl"]'),
                            icon: $(e[0]).find('[control-id="icon"]')
                        };
                        $(e[0]).find("a").focus(function (evt) {
                            $(e[0]).addClass("debug");
                        });
                        $(e[0]).find("a").blur(function (evt) {
                            $(e[0]).removeClass("debug");
                        });
                        s.$watch(a.ngModel, function (v, o) {
                            if (v) {
                                cmp.icon.removeClass("bowtie-checkbox-empty").addClass("bowtie-checkbox");
                                cmp.label.html(cmp.titleTrue);
                            }
                            else {
                                cmp.icon.addClass("bowtie-checkbox-empty").removeClass("bowtie-checkbox");
                                cmp.label.html(cmp.titleFalse);
                            }
                        });
                        $(e[0]).click(function (evt) {
                            cmp.val = !cmp.val;
                            $parse(a.ngModel).assign(s, cmp.val);
                            s.$applyAsync();
                            //var v = cmp.val;
                            //if (v) {
                            //    cmp.icon.removeClass("bowtie-checkbox-empty").addClass("bowtie-checkbox");
                            //    cmp.label.html(cmp.titleTrue);
                            //}
                            //else {
                            //    cmp.icon.addClass("bowtie-checkbox-empty").removeClass("bowtie-checkbox");
                            //    cmp.label.html(cmp.titleFalse);
                            //}
                        });
                        a.$observe("titleTrue", function (v) {
                            cmp.titleTrue = v;
                        });
                        a.$observe("titleFalse", function (v) {
                            cmp.titleFalse = v;
                        });
                    }
                };
            }]);
    })(ui = lv.ui || (lv.ui = {}));
})(lv || (lv = {}));
//# sourceMappingURL=lv_ui.js.map