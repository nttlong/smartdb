/// <reference path="types/jqlite.d.ts" />
/// <reference path="types/angular.d.ts" />
/// <reference path="types/select2/index.d.ts" />
/// <reference path="types/q.d.ts.ts" />
Q.angularDefine(function (mdl) {
    mdl.directive("s2", [
        "$parse",
        "$compile",
        "$interpolate",
        "$components",
        function ($parse, $compile, $interpolate, $components) {
            var internalUtils = /** @class */ (function () {
                function internalUtils() {
                }
                internalUtils.builData = function (data, cacheData, key, text) {
                    var ret = [];
                    for (var i = 0; i < data.length; i++) {
                        var item = data[i];
                        var pItem = {
                            id: item[key],
                            text: item[text]
                        };
                        cacheData["key_" + pItem.id] = item;
                        ret.push(pItem);
                    }
                    return ret;
                };
                return internalUtils;
            }());
            var clsSelect2 = /** @class */ (function () {
                function clsSelect2(s, e, a) {
                    var me = this;
                    this.select2 = undefined;
                    this.isChangeByBinding = false;
                    this.selectedValues = a.selectedValues;
                    this.rowTemplate = $(e[0]).find("#ext").attr("data-row-template");
                    this.scope = s;
                    this.msgOnSearch = a.msgOnSearch;
                    this.value = undefined;
                    this.keyField = a.key;
                    this.text = a.text;
                    this.displayText = s.$eval(a.displayText);
                    this.onSearch = a.onSearch;
                    this.ngSearchValue = a.ngSearchValue;
                    this.data = [];
                    this.cacheData = {};
                    this.oldSource = undefined;
                    this.placeholder = undefined;
                    this.options = {
                        ajax: {
                            transport: function (params, success, failure) {
                                success({ results: this.data });
                            }
                        },
                        escapeMarkup: undefined,
                        templateResult: undefined,
                        allowClear: true
                    };
                    this.isMulti = angular.isDefined(a.multi);
                    if (this.rowTemplate) {
                        this.rowTemplate = unescape(decodeURIComponent(this.rowTemplate));
                        this.options.escapeMarkup = function (val) {
                            return val;
                        };
                        this.options.templateResult = function (dataRow) {
                            if (!me.cacheData) {
                                me.cacheData = {};
                            }
                            if (Object.keys(me.cacheData).length > 0) {
                                var dataItem = me.cacheData["key_" + dataRow.id];
                                if (!dataItem)
                                    return;
                                var ss = s.$new();
                                ss["dataItem"] = dataItem;
                                var ret = $interpolate(me.rowTemplate)(ss);
                                return $("<div>" + ret + "</div>").contents();
                            }
                            else {
                                if (dataRow.id) {
                                    var sss = s.$new();
                                    sss["dataItem"] = me.cacheDataItems[dataRow.id.toLowerCase()];
                                    var ret1 = $interpolate(me.rowTemplate)(sss);
                                    return $("<div>" + ret1 + "</div>").contents();
                                }
                            }
                        };
                    }
                    this.dropdownContent = $(e[0]).find("#ext").attr("data-template");
                    if (this.dropdownContent) {
                        $(e[0]).find("#ext").removeAttr("data-template");
                        this.dropdownContent = unescape(decodeURIComponent(this.dropdownContent));
                        this.initDropdown();
                    }
                    if (this.isMulti) {
                        this.installMulti();
                    }
                }
                clsSelect2.prototype.installMulti = function () {
                    var me = this;
                    if (me.onSearch && me.isMulti) {
                        me.options.tokenSeparators = [","];
                        me.multiple = true;
                        me.options.minimumInputLength = 1;
                        me.options.ajax = {
                            transport: function (params, success, failure) {
                                var sender = {
                                    searchValue: params.data.term,
                                    done: function (data) {
                                        var bindData = {
                                            results: []
                                        };
                                        if (!me.cacheDataItems) {
                                            me.cacheDataItems = {};
                                        }
                                        if (!me.$$selectedKeys) {
                                            me.$$selectedKeys = {};
                                        }
                                        for (var i = 0; i < data.results.length; i++) {
                                            me.cacheDataItems[data.results[i][me.keyField].toLowerCase()] = data.results[i];
                                            bindData.results.push({
                                                id: data.results[i][me.keyField],
                                                text: data.results[i][me.text]
                                            });
                                        }
                                        success(bindData);
                                    }
                                };
                                var fn = me.scope.$eval(me.onSearch);
                                if (angular.isFunction(fn)) {
                                    fn(sender);
                                }
                            }
                        };
                    }
                };
                clsSelect2.prototype.initDropdown = function () {
                    var _this = this;
                    var me = this;
                    if (this.dropdownContent) {
                        this.isUseDropdownTemplate = true;
                        var dropdownParent = $("<div style='backgound-color:#fff;width:100%'>" + this.dropdownContent + "</div>");
                        this.dropdownParent = $(dropdownParent.children()[0]);
                    }
                    else {
                        if (this.onSearch && this.isMulti) {
                            this.options.tokenSeparators = [","];
                            this.multiple = true;
                            this.options.minimumInputLength = 1;
                            this.options.ajax = {
                                transport: function (params, success, failure) {
                                    var sender = {
                                        searchValue: params.data.term,
                                        done: function (data) {
                                            var bindData = {
                                                results: []
                                            };
                                            if (!me.cacheDataItems) {
                                                me.cacheDataItems = {};
                                            }
                                            if (!me.$$selectedKeys) {
                                                me.$$selectedKeys = {};
                                            }
                                            for (var i = 0; i < data.results.length; i++) {
                                                me.cacheDataItems[data.results[i][me.keyField].toLowerCase()] = data.results[i];
                                                bindData.results.push({
                                                    id: data.results[i][me.keyField],
                                                    text: data.results[i][me.text]
                                                });
                                            }
                                            success(bindData);
                                        }
                                    };
                                    var fn = me.scope.$eval(_this.onSearch);
                                    if (angular.isFunction(fn)) {
                                        fn(sender);
                                    }
                                }
                            };
                        }
                    }
                };
                clsSelect2.prototype.setValues = function (v) {
                    if (angular.isUndefined(v) || (v === null))
                        return;
                    if (!this.cacheDataItems) {
                        this.cacheDataItems = {};
                    }
                    var vals = [];
                    for (var i = 0; i < v.length; i++) {
                        this.cacheDataItems[v[i][this.keyField].toLowerCase()] = v[i];
                        var newOption = new Option(v[i][this.text], v[i][this.keyField], true, true);
                        this.cmp.append(newOption);
                        vals.push(v[i][this.keyField]);
                    }
                    this.cmp.val(vals);
                    this.cmp.trigger('change');
                };
                ;
                clsSelect2.prototype.setText = function (txt) {
                    var me = this;
                    if (!this.$$hasInit) {
                        setTimeout(function () {
                            me.select2["$container"].find(".select2-selection__rendered").html(txt);
                        }, 500);
                        this.$$hasInit = true;
                    }
                    else {
                        this.select2["$container"].find(".select2-selection__rendered").html(txt);
                    }
                };
                return clsSelect2;
            }());
            return {
                restrict: "ECA",
                template: "<div style=\"width:100%\"><div id=\"ext\" ng-transclude></div></div>",
                transclude: true,
                link: function (s, e, a) {
                    var component = new clsSelect2(s, e, a);
                    if (a.id) {
                        $components.assign(a.id, s, component);
                    }
                    if (a.multi && (!component.dropdownContent)) {
                        component.cmp = $("<select style='width:100%' ui-id='select' multiple='multiple'></select>");
                    }
                    else {
                        component.cmp = $("<select style='width:100%' ui-id='select'></select>");
                    }
                    component.cmp.appendTo(e[0]);
                    var opt = component.options;
                    component.$select2 = component.cmp.select2(opt);
                    component.select2 = component.$select2.data("select2");
                    component.dropdownContainer = component.select2.dropdown.$dropdownContainer;
                    component.dropdown = component.dropdownContainer.find(".select2-results");
                    component.select2TextEle = component.cmp.find(".select2-xl23-container");
                    if (component.dropdownParent) {
                        component.dropdownParent.appendTo("body");
                        component.dropdownParent.css({
                            "background-color": "#ffffff",
                            "float": "left"
                        });
                        component.dropdown.empty();
                        component.dropdownContainer.css({
                            "width": component.dropdownParent.width(),
                            height: component.dropdownParent.height()
                        });
                        component.dropdownParent.appendTo(component.dropdown[0]);
                    }
                    component.$select2.on("change", function (evt) {
                        if (component.isChangeByBinding)
                            return;
                        if ((!component.isUseDropdownTemplate) && (a.multi)) {
                            component.$$$isManualChange = true;
                            var selections = component.cmp.select2('data');
                            component.selectedItems = [];
                            component.selectedValues = [];
                            if (!component.$$selectedKeys) {
                                component.$$selectedKeys = {};
                            }
                            for (var i = 0; i < selections.length; i++) {
                                var item = {};
                                item[component.keyField] = selections[i].id;
                                item[component.text] = selections[i].text;
                                component.selectedValues.push(item);
                                component.selectedItems.push(component.cacheDataItems[selections[i].id.toLowerCase()]);
                                component.$$selectedKeys[selections[i]] = true;
                            }
                            if (a.selectedItems) {
                                $parse(a.selectedItems).assign(s, component.selectedItems);
                            }
                            if (a.ngModel) {
                                $parse(a.ngModel).assign(s, component.selectedValues);
                            }
                            var fn = s.$eval(a.ngChange);
                            if (angular.isFunction(fn)) {
                                fn(selections);
                            }
                            s.$applyAsync();
                            component.$$$isManualChange = false;
                            return;
                        }
                        var item = component.cacheData["key_" + $(evt.target).val()];
                        if (angular.isUndefined(item))
                            return;
                        if (a.ngModel) {
                            $parse(a.ngModel).assign(s, item[component.keyField]);
                        }
                        if (a.selectedData) {
                            $parse(a.selectedData).assign(s, item);
                        }
                        var fn = s.$eval(a.ngChange);
                        if (angular.isFunction(fn)) {
                            fn(item);
                        }
                        s.$applyAsync();
                    });
                    var watchDropdown = function () {
                        if (component.select2.dropdown === null)
                            return;
                        var editor = component.select2.dropdown.$dropdownContainer.find(".select2-search.select2-search--dropdown");
                        if (editor.parent().is(":visible")) {
                            var width = $(component.dropdown.children()[0]).width();
                            if (width < e.find(".select2-selection.select2-selection--single").width()) {
                                width = e.find(".select2-selection.select2-selection--single").width();
                            }
                            editor.css("width", width);
                            setTimeout(function () {
                                editor.parent().css({ "width": width + 2 });
                                editor.find("input").css({ "width": width - 8 });
                                if (!component.initSearchEvent) {
                                    editor.find('input.select2-search__field')[0].addEventListener("keyup", function () {
                                        var val = editor.find('input.select2-search__field').val();
                                        component.searchValue = val;
                                        if (component.ngSearchValue) {
                                            $parse(component.ngSearchValue).assign(component.scope, val);
                                        }
                                        if (component.onSearch) {
                                            var fn = component.scope.$eval(component.onSearch);
                                            if (angular.isFunction(fn)) {
                                                fn(val, component);
                                            }
                                        }
                                        component.scope.$applyAsync();
                                    });
                                    component.initSearchEvent = true;
                                }
                            }, 100);
                        }
                        else {
                            setTimeout(watchDropdown, 100);
                        }
                    };
                    component.$select2.on('select2:close', function (evt) {
                        component.$$$isManualChange = false;
                        watchDropdown();
                    });
                    component.$select2.on('select2:open', function (evt) {
                        component.$$$isManualChange = true;
                        var fn = undefined;
                        if (!component.hasLoaded) {
                            if (component.isUseDropdownTemplate) {
                                $compile(component.dropdown.contents())(s);
                                s.$applyAsync();
                                var editor = component.select2.dropdown.$dropdownContainer.find(".select2-search.select2-search--dropdown");
                                var width = component.dropdown.width();
                            }
                            fn = s.$eval(a.onFirstShowDropdown);
                            if (angular.isFunction(fn)) {
                                fn(component);
                            }
                            component.hasLoaded = true;
                        }
                        watchDropdown();
                        fn = s.$eval(a.onOpen);
                        if (angular.isFunction(fn)) {
                            fn(component);
                        }
                    });
                    var initData = component.oldSource = s.$eval(a.source);
                    if (angular.isDefined(initData)) {
                        component.data = internalUtils.builData(initData, component.cacheData, component.keyField, component.text);
                    }
                    component.value = s.$eval(a.ngModel);
                    s.$watch(a.displayText, function (v, o) {
                        if (angular.isDefined(v)) {
                            component.setText(v);
                        }
                    });
                    s.$watch(a.source, function (n, o) {
                        if (angular.isUndefined(n))
                            return;
                        if ((n !== component.oldSource) || (n.length !== component.oldSource.length)) {
                            component.oldSource = n;
                            component.data = internalUtils.builData(n, component.cacheData, component.keyField, component.text);
                            var opt = {
                                data: component.data,
                                templateResult: component.options.templateResult
                            };
                            component.cmp.select2(opt);
                        }
                    });
                    s.$watch(a.ngModel, function (o, v) {
                        if (component.isManualChange)
                            return;
                        if (angular.isUndefined(component.cmp))
                            return;
                        component.value = o;
                        if (!component.cmp)
                            return;
                        component.cmp.val(o);
                        component.cmp.trigger('change');
                    });
                    s.$watch(function () {
                        var v = s.$eval(a.source);
                        if (angular.isArray(v)) {
                            return v.length;
                        }
                    }, function (n, o) {
                        if (angular.isUndefined(n))
                            return;
                        if (n !== o) {
                            var v = s.$eval(a.source);
                            component.data = internalUtils.builData(v, component.cacheData, component.keyField, component.text);
                        }
                    });
                    a.$observe("placeholder", function (v) {
                        component.placeholder = v;
                    });
                }
            };
        }
    ]);
});
Q.angularDefine(function (mdl) {
    mdl.directive('s2Template', ["$parse", "$filter", function ($parse, $filter) {
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
                        }
                    };
                }
            };
        }]);
    mdl.directive('s2RowTemplate', ["$parse", "$filter", function ($parse, $filter) {
            return {
                restrict: "ECA",
                scope: false,
                compile: function (element, attributes) {
                    var originHtml = element.html();
                    element.empty();
                    return {
                        pre: function (s, e, a, c, t) {
                            e.parent().attr("data-row-template", encodeURIComponent(originHtml));
                            e.remove();
                        }
                    };
                }
            };
        }]);
    mdl.directive('templateHeader', ["$parse", "$filter", function ($parse, $filter) {
            return {
                restrict: "ECA",
                scope: false,
                compile: function (element, attributes) {
                    var originHtml = element.html();
                    element.empty();
                    return {
                        pre: function (s, e, a, c, t) {
                            e.parent().attr("data-template-header", encodeURIComponent(originHtml));
                            e.remove();
                        }
                    };
                }
            };
        }]);
});
//# sourceMappingURL=ui.e2s2.js.map