namespace lv {
    export namespace ui {
        export function toggleMenu(ele: JQuery, selector: string) {
            $(ele).find(selector).toggleClass("show");
        }
        export function toggleColapse(ele: JQuery) {
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
        export var mdl = angular.module("lv-ui", []);
        mdl.directive("vBox", [() => {
            return {
                template: `<div class="item">
                                <div class="form-group">
                                    <label control-id="lblCaption"></label>
                                    
                                </div>
                                <div ng-transclude style="display:none" control-id="container"></div>
                           </div>`,
                transclude: true,
                replace: true,
                link: (s, e, a) => {
                    a.$observe<string>("title", v => {
                        $(e[0]).find('[control-id="lblCaption"]').html(v);
                    });
                    s.$watch(() => {
                        return $(e[0]).find('[control-id="container"]').children().length
                    }, (v, o) => {
                        if (v > 0) {
                            var ele = $(e[0]).find('[control-id="container"]').children()[0];
                            $(ele).appendTo($(e[0]).find(".form-group")[0]);
                            autoFormat(ele);
                        }
                    });
                    var autoFormat = (ele: HTMLElement) => {
                        if ((ele.tagName === "INPUT") &&


                            (ele.attributes["type"].value !== `button`)) {
                            $(ele).addClass("style-input form-control ip_txt");
                        }
                        if ((ele.tagName === "TEXTAREA")) {
                            $(ele).addClass("style-input form-control ip_txt");
                        }
                    }
                }
            }
        }]);
        mdl.directive("hContainer", [() => {

            return {
                restrict: "ECA",
                template: `<div class="box_02 col_2 clearfix" ng-transclude></div>`,
                transclude: true,
                replace: true,
                link: (s, e, a) => {
                    a.$observe("cols", v => {
                        alert(v);
                    });
                }
            }

        }]);
        mdl.directive("hBox", [() => {

            return {
                restrict: "ECA",
                template: `<div class="box_02  clearfix" ng-transclude></div>`,
                transclude: true,
                replace: true,
                link: (s, e, a) => {
                    a.$observe("cols", v => {
                        $(e[0]).addClass(`col_${v}`)
                    });
                }
            }

        }])
        mdl.directive("vContainer", [() => {

            return {
                template: `<div class="box_content">
                                <dl class="mb-0">
                                    <dt>
                                        <a class="title_02 bg_gray_01 " control-id="indicator">
                                            <i class="bowtie-icon bowtie-navigate-back-circle float-left ic_arrow"></i>
                                            <span control-id="lblCaption"></span>
                                        </a>
                                    </dt>
                                    <dd>
                                        <div class="multi-collapse" ng-transclude control-id="content"></div>
                                    </dd>
                                </dl>    
                            </div>`,
                replace: true,
                transclude: true,
                restrict: "ECA",
                link: (s, e, a) => {
                    e.find('[control-id="indicator"]').click(evt => {
                        e.find('[control-id="indicator"]').toggleClass("collapsed");
                        e.find('[control-id="content"]').toggleClass("collapse");
                    });
                    a.$observe("title", (v) => {
                        e.find("[control-id='lblCaption']").html(a.title);
                    });

                }
            }

        }]);
        mdl.directive("appToolbar", ["$parse", ($parse: angular.IParseService) => {

            return {
                replace: true,
                template: `
                            <ul id="divHeader" class="box_btn mb-0">
                                <li id="btnRefresh" class="btn_function " style="display:none"><a class="btn_refresh btn_30"><i class="bowtie-icon bowtie-navigate-refresh"></i></a></li>
                                <li id="btnAdd" class="btn_function " title="Thêm mới" style="display:none"><a  class="btn_add btn_30 "><i class="bowtie-icon bowtie-math-plus"></i></a></li>
                                <li id="btnCopy" class="btn_function " title="Sao chép" style="display:none"><a class="btn_add btn_30 "><i class="bowtie-icon bowtie-edit-copy"></i></a></li>
                                <li id="btnEdit" class="btn_function " title="Cập nhật" style="display:none"><a  class="btn_edit btn_30 btn_function"><i class="bowtie-icon bowtie-edit-outline"></i></a></li>
                                <li id="btnDelete" class="btn_function " title="Xoá bỏ" style="display:none"><a  class="btn_del btn_30 btn_function"><i class="bowtie-icon bowtie-edit-delete"></i></a></li>
                                <li id="btnExport" class="btn_function " title="Export to Excel" style="display:none"><a  class="btn_del btn_30 btn_function"><i class="bowtie-icon bowtie-storyboard"></i></a></li>
                                <li id="btnImport" class="btn_function " title="Import from Excel" style="display:none"><a  class="btn_del btn_30 btn_function"><i class="bowtie-icon bowtie-column-option"></i></a></li>
                                
                                <li id="txtSearchBox">
                                    <div class="btn_search">
                                        <div class="input-group">
                                            <input id="inputSearch" type="text" class="form-control" placeholder="">
                                            <span class="input-group-btn">
                                                <button  class="btn" type="button"><i class="bowtie-icon bowtie-search"></i></button>
                                            </span>
                                        </div>
                                    </div>
                                </li>
                                <li id="btnSetting" class="setting dropdown btn_function " style="display:none">
                                    <a class="dropdown-toggle btn_refresh btn_30" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false" title="Setting"> <i class="bowtie-icon bowtie-settings-gear-outline"></i></a>
                                    <ul class="dropdown-menu animated" style="right: 0px; left: 0px; top: 0px; position: absolute; transform: translate3d(0px, 30px, 0px); will-change: transform;" x-placement="bottom-start">
                                        <li style="width: 100%;">
                                            <div class="message-center">
                                                <a ng-click="callMethod('')">
                                                    <div class="mail-content"><span class="mail-desc">abc</span> </div>
                                                </a>
                                                <a ng-click="callMethod('')">
                                                    <div class="mail-content"><span class="mail-desc">xyz</span> </div>
                                                </a>
                                            </div>
                                        </li>
                                    </ul>
                                </li>
                            </ul>`,
                link: (s, e, a) => {

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
                        btnRefresh.click(evt => {
                            s.$eval(a.onRefresh);
                        });
                    }
                    if (a.onAddNew) {
                        btnAdd.show();
                        btnAdd.click(evt => {
                            s.$eval(a.onAddNew);
                        });
                    }
                    if (a.onCopy) {
                        btnCopy.show();
                        btnCopy.click(evt => {
                            s.$eval(a.onCopy);
                        });
                    }
                    if (a.onEdit) {
                        btnEdit.show();
                        btnEdit.click(evt => {
                            s.$eval(a.onEdit);
                        });
                    }
                    if (a.onDelete) {
                        console.log(a.onDelete);
                        btnDelete.show();
                        btnDelete.click(evt => {
                            s.$eval(a.onDelete);
                        });
                    }
                    if (a.onSearch) {
                        if (!a.ngModelSearch) {
                            throw "ng-model-search was not found if on-search is enable";
                        }
                        txtSearchBox.show();
                        txtSearchBox.find("#inputSearch").keydown(evt => {
                            if (evt.keyCode === 13) {
                                $parse(a.ngModelSearch).assign(s, txtSearchBox.find("#inputSearch").val());
                                s.$eval(a.onSearch);
                                evt.preventDefault();
                                evt.stopImmediatePropagation();
                            }
                        });
                        txtSearchBox.find(".btn").click(evt => {
                            $parse(a.ngModelSearch).assign(s, txtSearchBox.find("#inputSearch").val());
                            s.$eval(a.onSearch);
                        });
                    }
                    if (a.onExport) {
                        btnExport.show();
                        btnExport.click(evt => {
                            s.$eval(a.onExport);
                        });
                    }
                    if (a.onImport) {
                        btnImport.show();
                        btnImport.click(evt => {
                            s.$eval(a.onImport);
                        });
                    }
                    $(e[0]).appendTo($("#application-toolbar")[0]);
                }
            }

        }]);
        mdl.directive("qB", [() => {
            return {
                restrict: "ECA",
                replace: true,
                transclude: true,
                template: `<button class="btn btn_save_02">
                            <i class="bowtie-icon"></i>
                            <span ng-transclude></span>
                        </button>`,
                link: (s, e, a) => {
                    $(e[0]).find(".bowtie-icon").addClass(`bowtie-${a.icon}`);
                    a.$observe("icon", v => {
                        $(e[0]).find(".bowtie-icon").addClass(`bowtie-${v}`);

                    });

                }
            }
        }]);
        mdl.directive("cB", ["$parse", ($parse: angular.IParseService) => {
            return {
                template: `<div class="form-group" style="min-height: 30px">
                                <label for="isDefault">
                                    <i control-id="icon" class="bowtie-icon bowtie-checkbox-empty" style="color: black;font-size:18px;padding-right:10px;"></i> 
                                    <span control-id="lbl"></span>
                                 </label>
                            </div>`,
                replace: true,
                link: (s, e, a) => {
                    var cmp = {
                        titleTrue: a.titleTrue,
                        titleFalse: a.titleFalse,
                        val: false,
                        label: $(e[0]).find('[control-id="lbl"]'),
                        icon: $(e[0]).find('[control-id="icon"]')

                    };
                    $(e[0]).find("a").focus(evt => {
                        $(e[0]).addClass("debug");
                    });
                    $(e[0]).find("a").blur(evt => {
                        $(e[0]).removeClass("debug");
                    });

                    s.$watch<boolean>(a.ngModel, (v, o) => {
                        if (v) {
                            cmp.icon.removeClass("bowtie-checkbox-empty").addClass("bowtie-checkbox");
                            cmp.label.html(cmp.titleTrue);
                        }
                        else {
                            cmp.icon.addClass("bowtie-checkbox-empty").removeClass("bowtie-checkbox");
                            cmp.label.html(cmp.titleFalse);
                        }
                    });
                    $(e[0]).click(evt => {
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
                    a.$observe("titleTrue", v => {
                        cmp.titleTrue = v;

                    })
                    a.$observe("titleFalse", v => {
                        cmp.titleFalse = v;
                    })
                }
            }
        }]);
    }
}