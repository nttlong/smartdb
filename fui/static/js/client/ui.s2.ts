/// <reference path="types/jqlite.d.ts" />
/// <reference path="types/angular.d.ts" />
/// <reference path="types/select2/index.d.ts" />
/// <reference path="types/q.d.ts.ts" />
/// <reference path="../../../node_modules/@types/jquery/index.d.ts" />

Q.angularDefine(mdl => {
    /*
     <s2 data-source="models.company" data-key="id" data-text="name" ng-model="..." ></s2>
     */
    mdl.directive("s2",
        [
            "$parse",
            "$compile",
            "$interpolate",
            "$components",
            (
                $parse: angular.IParseService,
                $compile: angular.ICompileService,
                $interpolate: angular.IInterpolateService,
                $components: Q.services.utils.IComponent
            ) => {
                class internalUtils {
                    static builData(data: any, cacheData: any, key: any, text: any): Array<{ id, text }> {
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
                    }

                }

                class clsSelect2 {
                    installMulti(): any {

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
                    }
                    isUseDropdownTemplate: boolean;
                    dropdownParent: JQuery;
                    multiple: boolean;
                    $$selectedKeys: any;
                    cmp: JQuery;
                    select2: {
                        dropdown: {
                            $dropdownContainer: any
                        }
                    };
                    $select2: JQuery;
                    dropdownContainer: any;
                    dropdown: any;
                    select2TextEle: JQuery;
                    isChangeByManualTrigger: boolean;
                    $$$isManualChange: boolean;
                    selectedItems: any[];
                    initSearchEvent: any;
                    searchValue: any;
                    hasLoaded: any;
                    isManualChange: any;
                    initDropdown(): any {
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
                                    transport: (params, success, failure) => {
                                        var sender = {
                                            searchValue: params.data.term,
                                            done: (data) => {
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
                                        var fn = me.scope.$eval(this.onSearch);
                                        if (angular.isFunction(fn)) {
                                            fn(sender);
                                        }
                                    }
                                };

                            }
                        }
                    }
                    isChangeByBinding: boolean;
                    selectedValues: any;
                    rowTemplate: string;
                    scope: Q.IQScope;
                    msgOnSearch: any;
                    value: any;
                    keyField: any;
                    text: any;
                    displayText: any;
                    onSearch: any;
                    ngSearchValue: any;
                    data: any[];
                    cacheData: {};
                    oldSource: any;
                    placeholder: any;
                    options: {
                        ajax: {
                            transport: (params: any, success: any, failure: any) => void;
                        },
                        escapeMarkup: any,
                        templateResult: any,
                        tokenSeparators?: Array<string>,
                        minimumInputLength?: number,
                        allowClear?: boolean
                    };
                    isMulti: boolean;
                    $$hasInit: any;
                    cacheDataItems: {};
                    dropdownContent: string;
                    setValues(v) {
                        var me = this;
                        if (angular.isUndefined(v) || (v === null)) return;
                        if (!me.cacheDataItems) {
                            me.cacheDataItems = {};
                        }
                        var vals = [];
                       
                        for (var i = 0; i < v.length; i++) {
                            me.cacheDataItems[v[i][me.keyField].toLowerCase()] = v[i];
                            var newOption = new Option(v[i][me.text], v[i][me.keyField], true, true);
                            me.cmp.append(newOption);
                            vals.push(v[i][me.keyField]);

                        }
                        me.cmp.val(vals);
                        me.cmp.trigger('change');
                    };

                    setText(txt: string) {
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
                    }
                    constructor(s: Q.IQScope, e: JQLite, a: angular.IAttributes) {
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
                            this.options.escapeMarkup = (val) => {
                                return val;
                            };
                            this.options.templateResult = function (dataRow) {
                                if (!me.cacheData) {
                                    me.cacheData = {};
                                }
                                if (Object.keys(me.cacheData).length > 0) {
                                    var dataItem = me.cacheData["key_" + dataRow.id];
                                    if (!dataItem) return;
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
                }
                return {
                    restrict: "ECA",
                    template: `<div style="width:100%"><div id="ext" ng-transclude></div></div>`,
                    transclude: true,
                    link: (s: Q.IQScope, e, a) => {

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
                        var opt: any = component.options

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
                        component.$select2.on("change", (evt) => {
                            if (component.isChangeByManualTrigger) {
                                component.isChangeByManualTrigger = false;
                                return;
                            }
                            component.$$$isManualChange = true;
                            if (component.isChangeByBinding) return;
                            if ((!component.isUseDropdownTemplate) && (a.multi)) {
                               
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
                                
                                return;
                            }

                            var item = component.cacheData["key_" + $(evt.target).val()];
                            if (angular.isUndefined(item)) return;
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
                        var watchDropdown = () => {
                            if (component.select2.dropdown === null) {

                              
                                return;
                            };
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
                        }
                        component.$select2.on('select2:close', (evt) => {
                            component.$$$isManualChange = false;
                            watchDropdown();
                        });
                        component.$select2.on('select2:open', (evt) => {
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
                        s.$watch(a.displayText, (v: string, o: string) => {
                            if (angular.isDefined(v)) {
                                component.setText(v);
                            }
                        });
                        var data = s.$eval(a.source);
                        if (data && data.length > 0) {
                            component.oldSource = data;
                            component.data = internalUtils.builData(data, component.cacheData, component.keyField, component.text);
                            var opt: any = {
                                data: component.data,
                                templateResult: component.options.templateResult
                            };
                            component.cmp.select2(opt);
                        }
                        s.$watch(a.source, (n: [], o: []) => {
                            if (angular.isUndefined(n)) return;
                            if ((n !== component.oldSource) || (n.length !== component.oldSource.length)) {
                                component.oldSource = n;
                                component.data = internalUtils.builData(n, component.cacheData, component.keyField, component.text);
                                var opt: any = {
                                    data: component.data,
                                    templateResult: component.options.templateResult
                                };
                                component.cmp.select2(opt);
                            }
                        });
                        s.$watch(a.ngModel, (o: string, v: string) => {
                            if (o === v) {
                                return;
                            }
                            if (component.$$$isManualChange) {
                                component.$$$isManualChange = false;
                                return;
                            }
                            if (angular.isUndefined(component.cmp)) return;
                            component.value = o;
                            if (!component.cmp) return;
                            component.cmp.val(o);
                            component.isChangeByManualTrigger = true;
                            component.cmp.trigger('change');
                        });
                        s.$watch(() => {
                            var v = s.$eval(a.source);
                            if (angular.isArray(v)) {
                                return v.length;
                            }
                        }, (n, o) => {
                            if (angular.isUndefined(n)) return;
                            if (n !== o) {
                                var v = s.$eval(a.source);

                                component.data = internalUtils.builData(v, component.cacheData, component.keyField, component.text);

                            }
                        });
                        a.$observe("placeholder", function (v) {
                            component.placeholder = v;
                        });
                    }
                }
            }])
});
Q.angularDefine(mdl => {
    mdl.directive('s2Dropdown', ["$parse", "$filter", function ($parse, $filter) {
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
    mdl.directive('s2Row', ["$parse", "$filter", function ($parse, $filter) {
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
    mdl.directive('s2Header', ["$parse", "$filter", function ($parse, $filter) {
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
Q.angularDefine(mdl => {
    mdl.directive("dp", [
        "$components",
        "$parse",
        "$timeout",
        function (
            $components: Q.services.utils.IComponent,
            $parse: angular.IParseService,
            $timeout: angular.ITimeoutService
        ) {
        return {
            restrict: 'ACE',
            template: `<div class="input-group date btn_directive">
                            <input  id="dateInput"
                                class="iput form-control">
                                <span class="input-group-addon">
                                <i class="bowtie-icon bowtie-calendar"></i></span></div>`,
            replace: true,
            link: function (s: angular.IScope, e, a) {
                var component = {
                    _isManualChange: false,
                    getValue: () => {

                    }
                }
                var dp = $(e[0]).find("#dateInput")["datepicker"]({
                    format: qLocalize.getDateFormat().toLocaleLowerCase(),
                    autoclose: true
                }).on("changeDate", (v) => {
                    component._isManualChange = true;
                    $parse(a.ngModel).assign(s, v.date);
                    if (a.ngChange) {
                        s.$eval(a.ngChange);
                    }
                    s.$applyAsync();
                });
                var v = s.$eval(a.ngModel);
                var cmp = dp.data("datepicker");
                component.getValue = () => {
                    return dp.data("datepicker").getDates()[0];
                }
                if (a.id) {
                    $components.assign(a.id, s, cmp);
                }
                if (angular.isDate(v)) {
                    cmp.setDate(v);
                }
                if (angular.isString(v)) {
                    cmp.setDate(new Date(v));
                }
                s.$watch(a.ngModel, function (v, o) {

                    if (component._isManualChange) {
                        component._isManualChange = false;
                        return;
                    }
                    var cmp = dp.data("datepicker");
                    if (angular.isDate(v)) {
                        cmp.setDate(v);
                    }
                    if (angular.isString(v)) {
                        cmp.setDate(new Date(v));
                    }
                    
                });
               
            }
        }
    }]);
})