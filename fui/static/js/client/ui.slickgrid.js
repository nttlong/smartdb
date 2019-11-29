/// <reference path="types/q.d.ts.ts" />
/// <reference path="types/slickgrid/index.d.ts" />
/// <reference path="types/jquery/index.d.ts" />
Q.angularDefine(function (mdl) {
    mdl.directive('sGrid', [
        "$parse",
        "$filter",
        "$compile",
        "$interpolate",
        "$components",
        function ($parse, $filter, $compile, $interpolate, $components) {
            var gridIndex = 0;
            var _sw = (function getScrollbarWidth() {
                // Creating invisible container
                var outer = document.createElement('div');
                outer.style.visibility = 'hidden';
                outer.style.overflow = 'scroll'; // forcing scrollbar to appear
                outer.style.msOverflowStyle = 'scrollbar'; // needed for WinJS apps
                document.body.appendChild(outer);
                // Creating inner element and placing it in the container
                var inner = document.createElement('div');
                outer.appendChild(inner);
                // Calculating difference between container's full width and the child width
                var scrollbarWidth = (outer.offsetWidth - inner.offsetWidth);
                // Removing temporary elements from the DOM
                outer.parentNode.removeChild(outer);
                return scrollbarWidth;
            })();
            var clsEditorConfig = /** @class */ (function () {
                function clsEditorConfig() {
                }
                clsEditorConfig.prototype.doEdit = function (cmp, evt, data) {
                    var me = this;
                    if (me.tagName === "INPUT") {
                        if (me.type === "checkbox") {
                            var val = evt.target.checked;
                            var dataRow = cmp.dataView.getItem(data.row);
                            $parse(me.ngModel).assign(dataRow, val);
                            cmp.scope.$applyAsync();
                            if (cmp.attr.ngChange) {
                                var fn = cmp.scope.$eval(cmp.attr.ngChange);
                                if (angular.isFunction(fn)) {
                                    fn(cmp, cmp.data);
                                }
                            }
                        }
                    }
                    throw new Error("Method not implemented.");
                };
                return clsEditorConfig;
            }());
            var clsColumnConfig = /** @class */ (function () {
                function clsColumnConfig() {
                }
                return clsColumnConfig;
            }());
            var internalServcies = /** @class */ (function () {
                function internalServcies() {
                }
                internalServcies.resetColumnWidth = function (cmp, w) {
                    var v = cmp.ele.find("columns").html();
                    cmp.cols = internalServcies.buildColumns(v, w - Number(_sw), cmp.scope, cmp.isTree, cmp);
                    cmp._cols = internalServcies.copy(cmp.cols);
                    var cols = cmp.grid.getColumns();
                    for (var i = 0; i < cols.length; i++) {
                        cols[i].width = cmp._cols[i].width;
                    }
                    cmp.grid.setColumns(cols);
                };
                internalServcies.watchWidth = function (cmp, handler) {
                    if (cmp.ignoreFixColumnWidth) {
                        return;
                    }
                    var w = $(cmp.ele.parents()[0]).innerWidth();
                    if (cmp.oldWidth !== w) {
                        cmp.oldWidth = w;
                        handler(cmp.oldWidth, function () {
                            setTimeout(function () {
                                internalServcies.watchWidth(cmp, handler);
                            }, 300);
                        });
                    }
                    else {
                        setTimeout(function () {
                            internalServcies.watchWidth(cmp, handler);
                        }, 300);
                    }
                };
                internalServcies.watchEle = function (cmp, handler) {
                    if ($.contains($("body")[0], cmp.ele[0])) {
                        handler();
                    }
                    else {
                        setTimeout(function () { internalServcies.watchEle(cmp, handler); }, 10);
                    }
                };
                internalServcies.initGrid = function (cmp) {
                    $(cmp.ele[0]).find("#grid").css({
                        height: cmp.ele.height()
                    });
                    if (!cmp.isEditable) {
                        cmp.options.forceSyncScrolling = true;
                        cmp.options.enableCellNavigation = true;
                        cmp.options.sortable = false;
                    }
                    cmp.data = cmp.scope.$eval(cmp.attr.source) || [];
                    if (cmp.isTree) {
                        cmp.ele.addClass("sg-tree");
                    }
                    cmp.grid = new Slick.Grid($(cmp.ele.find("#grid")[0])[0], cmp.dataView, cmp.cols, cmp.options);
                    if (cmp.options.editable) {
                        cmp.grid.setSelectionModel(new Slick["CellSelectionModel"]());
                    }
                    console.log("create slick grid with options", cmp.options);
                    console.log("create slick grid with columns", cmp.cols);
                    cmp.getSelectedItems = function () {
                        var ret = [];
                        var keys = Object.keys(cmp._$$$selectedItems);
                        console.log(cmp);
                        for (var i = 0; i < keys.length; i++) {
                            ret.push(cmp._$$$selectedItems[keys[i]]);
                        }
                        return ret;
                    };
                    if (cmp.isEditable) {
                        debugger;
                        cmp.ele.addClass("sg-editor");
                    }
                    cmp.grid.onHeaderClick.subscribe(function (evt, args) {
                        console.log("grid.onHeaderClick");
                        if (args.column["mapField"]) {
                            var val = evt.target["checked"];
                            var dataRows = cmp.dataView.getItems();
                            var parser = $parse(args.column["mapField"]);
                            cmp._$$$selectedItems = {};
                            if (!cmp.attr.keyField) {
                                console.error('data-key-field was not found');
                            }
                            for (var i = 0; i < dataRows.length; i++) {
                                parser.assign(dataRows[i], val);
                                if (!val) {
                                    cmp._$$$selectedItems = {};
                                }
                                else {
                                    cmp._$$$selectedItems[cmp.scope.$eval(cmp.attr.keyField, dataRows[i])] = dataRows[i];
                                }
                            }
                            if (cmp.attr.selectedItems) {
                                $parse(cmp.attr.selectedItems).assign(cmp.scope, cmp.getSelectedItems());
                            }
                            cmp.scope.$applyAsync();
                            return;
                        }
                        if ($(evt.target).attr("ng-click")) {
                            var fn = cmp.scope.$eval($(evt.target).attr("ng-click"));
                            if (angular.isFunction(fn)) {
                                fn($(evt.target));
                            }
                            var data = internalServcies.buildData(cmp.scope.$eval(cmp.attr.source), cmp);
                            cmp.dataView.setItems(data);
                            cmp.data = data;
                            cmp.grid.render();
                            cmp.scope.$applyAsync();
                        }
                        if ($(evt.target).attr("ng-change")) {
                            var fn = cmp.scope.$eval($(evt.target).attr("ng-change"));
                            if (angular.isFunction(fn)) {
                                fn($(evt.target));
                            }
                            var data = internalServcies.buildData(cmp.scope.$eval(cmp.attr.source), cmp);
                            cmp.dataView.setItems(data);
                            cmp.data = data;
                            cmp.grid.render();
                            cmp.scope.$applyAsync();
                        }
                    });
                    cmp.grid.onClick.subscribe(function (evt, args) {
                        console.log("onClick");
                        if (args.grid.getColumns()[args.cell]["$editor"]) {
                            var editor = args.grid.getColumns()[args.cell]["$editor"];
                            if (editor.tagName === "INPUT" && editor.type === "checkbox") {
                                editor.doEdit(cmp, evt, args);
                            }
                            return;
                        }
                        if (args.grid.getColumns()[args.cell].id == "_checkbox_selector") {
                            var dataItem = cmp.dataView.getItem(args.row);
                            var mapField = args.grid.getColumns()[args.cell]["mapField"];
                            $parse(mapField).assign(dataItem, evt.target["checked"]);
                            if (evt.target["checked"]) {
                                if (!cmp.attr.keyField) {
                                    console.error('data-key-field was not found');
                                }
                                if (!cmp._$$$selectedItems) {
                                    cmp._$$$selectedItems = {};
                                }
                                cmp._$$$selectedItems[cmp.scope.$eval(cmp.attr.keyField, dataItem)] = dataItem;
                            }
                            var fn = cmp.scope.$eval(cmp.attr.onCheckItem);
                            if (angular.isFunction(fn)) {
                                fn(dataItem);
                            }
                            if (cmp.attr.selectedItems) {
                                $parse(cmp.attr.selectedItems).assign(cmp.scope, cmp.getSelectedItems());
                            }
                            cmp.scope.$applyAsync();
                            return;
                        }
                        else {
                            cmp.doActionSeletedItem(evt, args, true);
                        }
                    });
                    cmp.grid.onSort.subscribe(function (e, args) {
                        var id = cmp.grid["getUID"]();
                        var eleId = "";
                        if ($(e.target).hasClass("slick-header-column")) {
                            eleId = $(e.target).attr("id");
                        }
                        else {
                            eleId = $(e.target).parent().attr("id");
                        }
                        var cF = eleId.substring(id.length, eleId.length);
                        var cols = cmp.cols.filter(function (c, index) {
                            if (c.field === cF) {
                                return c;
                            }
                        });
                        cmp.sortCols = [];
                        for (var i = 0; i < args.sortCols.length; i++) {
                            cmp.sortCols.push({
                                name: args.sortCols[i].sortCol.field,
                                asc: args.sortCols[i].sortAsc === true
                            });
                        }
                        if (cmp.attr.onRequestData) {
                            var fn = cmp.scope.$eval(cmp.attr.onRequestData);
                            if (angular.isFunction(fn)) {
                                fn(cmp);
                            }
                        }
                        cmp.scope.$applyAsync();
                    });
                    cmp.grid.onDblClick.subscribe(function (e, data) {
                        if (cmp.attr.onDbClick) {
                            cmp.selectedItem = cmp.data[data["row"]];
                            var fnk = cmp.scope.$eval(cmp.attr.onDbClick);
                            if (angular.isFunction(fnk)) {
                                fnk(cmp);
                            }
                            return;
                        }
                    });
                    cmp.grid.onDblClick.subscribe(function (evt, data) {
                        if (cmp.attr.ngModel) {
                            $parse(cmp.attr.ngModel).assign(cmp.scope, cmp.data[data["row"]]);
                        }
                        cmp.selectedItem = cmp.data[data["row"]];
                        var fn = cmp.scope.$eval(cmp.attr.onEdit);
                        if (angular.isFunction(fn)) {
                            fn(cmp, cmp.selectedItem);
                        }
                    });
                    cmp.grid.onKeyDown.subscribe(function (evt, data) {
                        if (evt["keyCode"] === 32) {
                            if (cmp.grid.getColumns()[data.cell]["$editor"]) {
                                var editor = cmp.grid.getColumns()[data.cell]["$editor"];
                                if (editor.tagName === "INPUT" && editor.type === "checkbox") {
                                    editor.doEdit(cmp, evt, data);
                                    evt.stopImmediatePropagation();
                                    return;
                                }
                            }
                        }
                        if (cmp.attr.onKeyDown) {
                            cmp.selectedItem = cmp.data[data["row"]];
                            var fnk = cmp.scope.$eval(cmp.attr.onKeyDown);
                            if (angular.isFunction(fnk)) {
                                fnk(cmp.selectedItem, evt["keyCode"], evt);
                            }
                            return;
                        }
                        if (evt["keyCode"] === 116) {
                            var fn = cmp.scope.$eval(cmp.attr.onRequestData);
                            if (angular.isFunction(fn)) {
                                fn();
                            }
                            evt.stopImmediatePropagation();
                        }
                        if (evt["keyCode"] === 13) {
                            debugger;
                            if (cmp.attr.ngModel) {
                                $parse(cmp.attr.ngModel).assign(cmp.scope, cmp.data[data["row"]]);
                            }
                            cmp.selectedItem = cmp.data[data["row"]];
                            var fn = cmp.scope.$eval(cmp.attr.onSelectItem);
                            if (angular.isFunction(fn)) {
                                evt.stopPropagation();
                                fn(cmp, cmp.selectedItem);
                            }
                            var fn = cmp.scope.$eval(cmp.attr.onEdit);
                            if (angular.isFunction(fn)) {
                                fn(cmp, cmp.selectedItem);
                            }
                            cmp.scope.$applyAsync();
                            evt.stopImmediatePropagation();
                        }
                    });
                    cmp.grid.render();
                    if (cmp.pageSize) {
                        cmp.pager = new Slick["Controls"].Pager(cmp.dataView, cmp.grid, cmp.ele.find("#pager"));
                        cmp.dataView.getLength = function () { return 100000; };
                        cmp.grid.updateRowCount();
                        //cmp.dataView.setItems(cmp.data);
                        cmp.grid.invalidate();
                        cmp.grid.render();
                        cmp.grid.onScroll.subscribe(function (evt, args) {
                            if (!cmp.strRows) {
                                cmp.strRows = ",";
                            }
                            var vp = cmp.grid.getViewport();
                            for (var i = 0; i < vp.bottom; i++) {
                                if (cmp.strRows.indexOf("," + i + ",") == -1) {
                                    cmp.data.push({
                                        id: i + vp.top,
                                        Title: i + vp.top
                                    });
                                    cmp.strRows += (i + vp.top) + ",";
                                }
                            }
                            cmp.dataView.setItems(cmp.data);
                            cmp.grid.invalidate();
                            cmp.grid.render();
                        });
                        cmp.grid["scrollTo"](0);
                    }
                };
                internalServcies.turnVisibleItems = function (items, isVisible) {
                    var ret = [];
                    for (var i = 0; i < items.length; i++) {
                        items[i].$isHidden = !isVisible;
                        ret.push(items[i]);
                        if (items[i].$$$children) {
                            if (items[i].$$$isCollapse !== false) {
                                var children = internalServcies.turnVisibleItems(items[i].$$$children, false);
                                for (var j = 0; j < children.length; j++) {
                                    ret.push(children[j]);
                                }
                            }
                            else {
                                var children = internalServcies.turnVisibleItems(items[i].$$$children, isVisible);
                                for (var j = 0; j < children.length; j++) {
                                    ret.push(children[j]);
                                }
                            }
                        }
                    }
                    return ret;
                };
                internalServcies.buildData = function (data, cmp) {
                    var fx = $filter;
                    cmp.hashItems = {};
                    function buildDataTree(items) {
                        for (var i = 0; i < items.length; i++) {
                            var item = items[i];
                            cmp.hashItems[item[cmp.key]] = item;
                            if (!item.$$$hasProcessed) {
                                var keyVal = item[cmp.key];
                                item.$$$childrenIds = [];
                                if (!item.$$$indent) {
                                    item.$$$indent = 0;
                                }
                                var ret = cmp.$filter("filter")(items, function (x) {
                                    var pK = x[cmp.parentKey];
                                    if (pK === keyVal) {
                                        x.$$$parentId = pK;
                                        if (!item.$$$childrenIds) {
                                            item.$$$childrenIds = [];
                                        }
                                        item.$$$childrenIds.push(x[cmp.key]);
                                        if (!x.$$$indent) {
                                            x.$$$indent = item.$$$indent + 1;
                                        }
                                        else {
                                            //x.$$$indent++;
                                        }
                                        return true;
                                    }
                                });
                                item.$$$children = ret;
                            }
                        }
                        var retItems = $filter("filter")(items, function (x) {
                            if ((!x[cmp.parentKey]) || (x[cmp.parentKey] === null)) {
                                return true;
                            }
                        });
                        return retItems;
                    }
                    function buildPivotData(items, tData) {
                        for (var i = 0; i < items.length; i++) {
                            tData.push(items[i]);
                            if (items[i].$$$children && (items[i].$$$children.length > 0)) {
                                buildPivotData(items[i].$$$children, tData);
                            }
                        }
                    }
                    for (var i = 0; i < data.length; i++) {
                        if (!data[i].id) {
                            data[i].id = i;
                        }
                    }
                    if (data.length == 0)
                        return data;
                    if (cmp.isTree) {
                        var tData = buildDataTree(data);
                        var retTData = [];
                        buildPivotData(tData, retTData);
                        return retTData;
                    }
                    return data;
                };
                internalServcies.copy = function (data) {
                    var ret = eval(JSON.stringify(data));
                    return ret;
                };
                internalServcies.buildColumns = function (html, w, scope, isTree, cmp) {
                    var ret = [];
                    var cols = $("<div>" + html + "</div>");
                    var n = 0;
                    var d = 0;
                    var subScope = scope.$new(true);
                    var treeColumn = undefined;
                    cols.children().each(function (index, ele) {
                        if ($(ele).attr('frozen') === "true") {
                            cmp.options.frozenColumn = index;
                            cmp.options.forceFitColumns = false;
                            cmp.options.topPanelHeight = 80;
                            cmp.ignoreFixColumnWidth = true;
                        }
                        var tmp = $(ele).attr("data-template");
                        var cellType = $(ele).attr("data-cell-type");
                        var tmpHeader = $(ele).attr("data-template-header");
                        if (tmp) {
                            tmp = decodeURIComponent(tmp);
                        }
                        if (tmpHeader) {
                            tmpHeader = decodeURIComponent(tmpHeader);
                            tmpHeader = cmp.$interpolate(tmpHeader)(scope);
                        }
                        var col = new clsColumnConfig();
                        if (cellType === "sg-check-box-select") {
                            col.isCheckBoxSelectColumn = true;
                            var checkboxSelector = new Slick["CheckboxSelectColumn"]({
                                cssClass: "slick-cell-checkboxsel"
                            });
                            col.columnDefinition = checkboxSelector.getColumnDefinition();
                            col.columnDefinition.width = 40;
                            col.columnDefinition.mapField = $(ele).attr("data-map-field") || "$selected";
                            // u(e, t, o, i, n) { return n ? r[e] ? "<input type='checkbox' checked='checked'>" : "<input type='checkbox'>" : null }
                            col.columnDefinition.formatter = function (rowIndex, cellIndex, o, i, n) {
                                console.log("columnDefinition.formatter");
                                var r = cmp.dataView.getItem(rowIndex);
                                var col = cmp.grid.getColumns()[cellIndex];
                                var e = cmp.scope.$eval(col["mapField"], r) || false;
                                if (e) {
                                    return "<input type='checkbox' checked='checked' class='editor-checkbox'>";
                                }
                                else {
                                    return "<input type='checkbox'  class='editor-checkbox'>";
                                }
                            };
                            cmp.isCheckSelectGrid = true;
                        }
                        col.id = $(ele).attr("id");
                        col.name = tmpHeader || $(ele).attr("title");
                        col.field = $(ele).attr("field") || $(ele).attr("id");
                        /**
                         *  e.parent().attr("data-cell-tag", e[0].tagName);
                    e.parent().attr("data-cell-type", a.type);
                    e.parent().attr("data-cell-field", a.ngModel);
                         * */
                        if ($(ele).attr("data-cell-tag")) {
                            col.$editor = new clsEditorConfig();
                            col.$editor.ngModel = $(ele).attr("data-cell-field");
                            col.field = col.$editor.ngModel;
                            col.$editor.tagName = $(ele).attr("data-cell-tag");
                            col.$editor.type = $(ele).attr("data-cell-type");
                            internalServcies.makeEditor(col);
                            cmp.options.editable = true;
                            cmp.options.enableCellNavigation = true;
                            cmp.options.asyncEditorLoading = false;
                            cmp.options.autoEdit = false;
                            cmp.isEditable = true;
                        }
                        else {
                            col.width = undefined;
                            col.onFormat = $(ele).attr("on-format");
                            col.template = tmp || $(ele).attr("on-format");
                            col.templateheader = tmpHeader;
                            col.commandId = $(ele).attr("command-id");
                            col.sortable = true;
                            col.formatter = undefined;
                            var treeColumnAttr = $(ele).attr("tree-column");
                            if (treeColumnAttr) {
                                treeColumn = col;
                            }
                            col.sortable = angular.isDefined(col.id);
                            if (col.template) {
                                col.formatter = function (rowIndex, colIndex, value, columnDef, dataContext) {
                                    return cmp.onCellFormat(rowIndex, colIndex, col, value, columnDef, dataContext);
                                };
                            }
                        }
                        if ($(ele).attr("width")) {
                            col.width = Number($(ele).attr("width"));
                        }
                        ret.push(col);
                        if (!$(ele).attr("width")) {
                            n++;
                        }
                        else {
                            d += Number($(ele).attr("width"));
                        }
                    });
                    for (var i = 0; i < ret.length; i++) {
                        if (ret[i].width) {
                            ret[i].width = ret[i].width * 1;
                        }
                        else {
                            ret[i].width = ((w - d) / n);
                        }
                    }
                    if ((!treeColumn) && (isTree)) {
                        treeColumn = ret[0];
                    }
                    if (treeColumn) {
                        treeColumn.isTree = true;
                        treeColumn.formatter = function (rowIndex, colIndex, value, columnDef, dataContext) {
                            return cmp.onTreeFormat(rowIndex, colIndex, value, columnDef, dataContext);
                        };
                    }
                    var ret2 = [];
                    for (var i = 0; i < ret.length; i++) {
                        if (ret[i].columnDefinition) {
                            ret2.push(ret[i].columnDefinition);
                        }
                        else {
                            ret2.push(ret[i]);
                        }
                    }
                    return ret2;
                };
                internalServcies.makeEditor = function (col) {
                    if (col.$editor.type == "checkbox") {
                        col.editor = Slick.Editors.Checkbox;
                        col.formatter = function (row, col, value) {
                            if (value) {
                                return "<input type=\"checkbox\" checked=checked  value=\"true\" class=\"editor-checkbox\" hidefocus=\"\">";
                            }
                            else {
                                return "<input type=\"checkbox\" value=\"false\"  class=\"editor-checkbox\" hidefocus=\"\">";
                            }
                        };
                    }
                };
                internalServcies.treeFilter = function (item) {
                    if (item.$isHidden) {
                        return false;
                    }
                    else {
                        return true;
                    }
                };
                return internalServcies;
            }());
            var slickGridComponent = /** @class */ (function () {
                function slickGridComponent(s, e, a) {
                    this.$interpolate = $interpolate;
                    this.$filter = $filter;
                    this.scope = s;
                    this.ele = e;
                    this.attr = a;
                    this.id = a.id;
                    this.dataItemsScope = {};
                    this.htmlRowCache = {};
                    this.pageIndex = 0;
                    this.grid = undefined;
                    this.key = a.key;
                    this.parentKey = a.parentKey;
                    this.cols = [];
                    this.options = {
                        enableCellNavigation: true,
                        enableColumnReorder: true,
                        rowHeight: s.$eval(a.rowHeight),
                        multiColumnSort: true
                        //forceFitColumns:true
                    };
                    if (this.parentKey) {
                        this.isTree = true;
                    }
                    this.data = [];
                    this.dataView = new Slick.Data.DataView({ inlineFilters: true });
                }
                slickGridComponent.prototype.watchParentHeight = function (ele, onChange) {
                    var parentHeight = $(ele.parents()[0]).innerHeight();
                    if (this.parentHeight != parentHeight) {
                        this.parentHeight = parentHeight;
                        onChange(this.parentHeight);
                    }
                    var me = this;
                    setTimeout(function () {
                        me.watchParentHeight(ele, onChange);
                    }, 300);
                };
                slickGridComponent.prototype.doActionSeletedItem = function (evt, args, buClick) {
                    var cmp = this;
                    cmp.selectedItem = cmp.dataView.getItem(args.row);
                    if (cmp.attr.ngModel) {
                        $parse(cmp.attr.ngModel).assign(cmp.scope, cmp.selectedItem);
                    }
                    if (cmp.cols[args.cell].isTree) {
                        if ($(evt.target).attr("id") == "indicator") {
                            var item = cmp.dataView.getItem(args.row);
                            if (!item.$$$isCollapse) {
                                item.$$$isCollapse = true;
                                cmp.dataView.updateItem(item.id, item);
                                cmp.dataView.refresh();
                                if (item.$$$children) {
                                    var updateItems = internalServcies.turnVisibleItems(item.$$$children, false);
                                    cmp.dataView.updateItem(item.id, item);
                                    for (var i = 0; i < updateItems.length; i++) {
                                        cmp.dataView.updateItem(updateItems[i].id, updateItems[i]);
                                    }
                                }
                                cmp.grid.invalidate();
                                cmp.grid.render();
                            }
                            else {
                                item.$$$isCollapse = false;
                                cmp.dataView.updateItem(item.id, item);
                                if (item.$$$children) {
                                    var updateItems = internalServcies.turnVisibleItems(item.$$$children, true);
                                    cmp.dataView.updateItem(item.id, item);
                                    for (var i = 0; i < updateItems.length; i++) {
                                        cmp.dataView.updateItem(updateItems[i].id, updateItems[i]);
                                    }
                                }
                                cmp.grid.invalidate();
                                cmp.grid.render();
                            }
                            evt.stopImmediatePropagation();
                            return;
                        }
                    }
                    if ($(evt.target).attr("command")) {
                        evt.stopPropagation();
                        if (cmp.attr.onCommand) {
                            var fn = cmp.scope.$eval(cmp.attr.onCommand);
                            if (angular.isFunction(fn)) {
                                fn($(evt.target).attr("command"), cmp.data[args.row]);
                                cmp.grid.render();
                            }
                        }
                        cmp.scope.$applyAsync();
                    }
                    if ($(evt.target).attr("ng-click")) {
                        evt.stopPropagation();
                        var ss = cmp.scope.$new();
                        ss["dataItem"] = cmp.data[args.row];
                        var fn = ss.$eval($(evt.target).attr("ng-click"));
                        if (angular.isFunction(fn)) {
                            fn($(evt.target), cmp.data[args.row]);
                        }
                        var data = internalServcies.buildData(cmp.scope.$eval(cmp.attr.source), cmp);
                        cmp.dataView.setItems(data);
                        cmp.data = data;
                        cmp.grid.render();
                        cmp.scope.$applyAsync();
                        return;
                    }
                    var fn = cmp.scope.$eval(cmp.attr.onSelectItem);
                    if (angular.isFunction(fn)) {
                        evt.stopPropagation();
                        fn(cmp, cmp.selectedItem);
                        cmp.scope.$applyAsync();
                    }
                };
                slickGridComponent.prototype.onTreeFormat = function (rowIndex, colIndex, value, columnDef, dataContext) {
                    //if (value == null || value == undefined || dataContext === undefined) { return ""; }
                    value = value || "";
                    if (columnDef.template) {
                        var ss = this.scope.$new();
                        this.dataItemsScope[ss.$id] = ss;
                        ss["dataItem"] = dataContext;
                        value = $interpolate(columnDef.template)(ss);
                    }
                    if (dataContext["$$$children"].length > 0) {
                        //value = value.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
                        var spacer = "<a id='indicator' data-id=\"" + dataContext[this.key] + "\" style='cursor:pointer;float:left;height:100% !important; display:flex;align-items:center;height:1px;width:" + (15 * (dataContext["$$$indent"] * 1) + 12) + "px'>\n                                            <i class='bowtie-icon bowtie-chevron-right' id='indicator' style='float:right' ></i></a>";
                        return spacer + value;
                    }
                    else {
                        var spacer = "<a id='indicator'  data-id=\"" + dataContext[this.key] + "\" style='cursor:pointer;float:left;height:100% !important; display:flex;align-items:center;height:1px;width:" + (15 * (dataContext["$$$indent"] * 1) + 12) + "px'>\n                                           </a>";
                        return spacer + value;
                    }
                };
                slickGridComponent.prototype.onCellFormat = function (rowIndex, colIndex, ci, cd, colInfo, dataRow) {
                    console.log("onCellFormat");
                    if (this.data.length === 0) {
                        this.data = this.scope.$eval(this.attr.source);
                    }
                    if (this.cols[colIndex].onFormat) {
                        var fn = this.scope.$eval(this.cols[colIndex].onFormat);
                        return fn(dataRow[this._cols[colIndex].field], dataRow);
                    }
                    var ss = this.scope.$new();
                    this.dataItemsScope[ss.$id] = ss;
                    ss["dataItem"] = dataRow;
                    var ret = $interpolate(colInfo.template)(ss);
                    return ret;
                };
                slickGridComponent.prototype.search = function (txtSearch, colsSearch) {
                    var items = this.dataView.getItems();
                    var cols = colsSearch || [];
                    if (cols.length === 0) {
                        this.cols.forEach(function (c) {
                            if (c.id) {
                                cols.push(c.id);
                            }
                        });
                    }
                    if (angular.isUndefined(txtSearch) || (txtSearch === "")) {
                        for (var i = 0; i < items.length; i++) {
                            items[i].$isHidden = false;
                        }
                    }
                    else {
                        for (var i = 0; i < items.length; i++) {
                            var isEqual = false;
                            for (var col = 0; col < cols.length; col++) {
                                var val = items[i][cols[col]];
                                if (val.toString().toLowerCase().indexOf(txtSearch.toLowerCase()) > -1) {
                                    isEqual = true;
                                    break;
                                }
                            }
                            items[i].$isHidden = !isEqual;
                            var pID = items[i].$$$parentId;
                            if (isEqual) {
                                while (pID) {
                                    this.hashItems[pID].$isHidden = false;
                                    pID = this.hashItems[pID].$$$parentId;
                                }
                            }
                        }
                    }
                    this.dataView.refresh();
                    this.grid.invalidate();
                };
                slickGridComponent.prototype.findById = function (id) {
                    var items = this.dataView.getItems();
                    var me = this;
                    var fx = items.filter(function (x) {
                        if (x[me.key] === id) {
                            return x;
                        }
                    });
                    if (fx.length > 0) {
                        return fx[0];
                    }
                    else {
                        return undefined;
                    }
                };
                ;
                slickGridComponent.prototype.expandNodeById = function (id) {
                    var node = this.findById(id);
                    while (node && (node != null)) {
                        node.$isHidden = false;
                        node = this.findById(node[this.parentKey]);
                    }
                    this.dataView.refresh();
                    this.grid.invalidate();
                };
                slickGridComponent.prototype.addToParent = function (data, parentId) {
                    var cmp = this;
                    var parentItem = this.findById(parentId);
                    data[cmp.parentKey] = parentId;
                    cmp.data.push(data);
                    var odata = internalServcies.buildData(cmp.data, cmp);
                    cmp.dataView.setItems(odata);
                    cmp.grid.invalidate();
                };
                slickGridComponent.prototype.collapseAll = function (level) {
                    var cmp = this;
                    this.data.forEach(function (n) {
                        if (!level) {
                            if (n[cmp.parentKey] && (n[cmp.parentKey] !== null))
                                n.$isHidden = true;
                        }
                        else {
                            if (n[cmp.parentKey] && (n[cmp.parentKey] !== null) && (n.$$$indent === level))
                                n.$isHidden = true;
                        }
                    });
                    var odata = internalServcies.buildData(cmp.data, cmp);
                    cmp.dataView.setItems(odata);
                    cmp.grid.invalidate();
                };
                slickGridComponent.prototype.expandAll = function (level) {
                    var cmp = this;
                    this.data.forEach(function (n) {
                        if (!level) {
                            if (n[cmp.parentKey] && (n[cmp.parentKey] !== null))
                                n.$isHidden = false;
                        }
                        else {
                            if (n[cmp.parentKey] && (n[cmp.parentKey] !== null) && (n.$$$indent === level))
                                n.$isHidden = false;
                        }
                    });
                    var odata = internalServcies.buildData(cmp.data, cmp);
                    cmp.dataView.setItems(odata);
                    cmp.grid.invalidate();
                };
                return slickGridComponent;
            }());
            return {
                restrict: "ECA",
                transclude: true,
                template: "<div>\n                                <div id='grid' style=''></div>\n                                <div id='pager'></div>\n                                <div ng-transclude style='display;none'></div></div>",
                link: function (scope, ele, attr) {
                    var cmp = new slickGridComponent(scope, ele, attr);
                    var removeHeader = function () {
                        if (ele.find(".slick-header").length === 1) {
                            ele.find(".slick-header").remove();
                            removeHeader = undefined;
                            if (cmp.grid) {
                                //cmp.grid.invalidate();
                                //cmp.grid.getVie
                                $(ele[0]).find(".slick-viewport").css({
                                    height: $(ele.parents()[0]).height()
                                });
                            }
                        }
                        else {
                            setTimeout(removeHeader, 10);
                        }
                    };
                    if (attr.hideHeader) {
                        removeHeader();
                    }
                    if (attr.id) {
                        $components.assign(attr.id, scope, cmp);
                    }
                    if (cmp.isTree) {
                        cmp.dataView.setFilter(internalServcies.treeFilter);
                        removeHeader();
                        cmp.ele.addClass("sg-tree");
                    }
                    scope.$watch(function () {
                        if (ele.find("columns").length > 0) {
                            return ele.find("columns").html();
                        }
                    }, function (v, o) {
                        if (angular.isDefined(v)) {
                            cmp.cols = internalServcies.buildColumns(v, $(ele[0]).width() - 18, scope, cmp.isTree, cmp);
                            cmp._cols = internalServcies.copy(cmp.cols);
                            internalServcies.watchEle(cmp, function () {
                                internalServcies.initGrid(cmp);
                            });
                            cmp.oldWidth = $(ele[0]).width();
                            internalServcies.watchWidth(cmp, function (w, next) {
                                internalServcies.resetColumnWidth(cmp, w);
                                next();
                            });
                        }
                    });
                    scope.$watch(function () {
                        return JSON.stringify(scope.$eval(attr.source));
                    }, function (v, o) {
                        if (cmp.grid) {
                            if (v != o) {
                                cmp.grid.invalidate();
                            }
                        }
                    });
                    cmp.watchParentHeight(ele, function (parentHeight) {
                        ele.css({
                            height: parentHeight
                        });
                        $(ele.children()[0]).css({
                            height: parentHeight
                        });
                        ele.find("#grid").css({
                            height: parentHeight
                        });
                        if (cmp.grid) {
                            cmp.grid.resizeCanvas();
                        }
                    });
                    scope.$watch(attr.source, function (v, o) {
                        if (cmp.grid) {
                            cmp.data = v || [];
                            var data = internalServcies.buildData(cmp.data, cmp);
                            cmp.dataView.setItems(data);
                            cmp.data = data;
                            cmp.grid.invalidate();
                            ele.find(".slick-viewport").scrollTop(1);
                            cmp.grid.render();
                        }
                    });
                }
            };
        }
    ]);
});
Q.angularDefine(function (mdl) {
    mdl.directive('sgCell', [function () {
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
    mdl.directive('sgCheckBoxSelect', [function () {
            return {
                restrict: "ECA",
                scope: false,
                compile: function (element, attributes) {
                    var originHtml = element.html();
                    element.empty();
                    return {
                        pre: function (s, e, a, c, t) {
                            e.parent().attr("data-template", encodeURIComponent(originHtml));
                            e.parent().attr("data-cell-type", 'sg-check-box-select');
                            e.parent().attr("width", 30);
                            e.parent().attr("data-map-field", a.field);
                            e.remove();
                        }
                    };
                }
            };
        }]);
    mdl.directive('sgEditor', [function () {
            return {
                restrict: "ECA",
                scope: false,
                compile: function (element, attributes) {
                    var originHtml = element.html();
                    element.empty();
                    return {
                        pre: function (s, e, a, c, t) {
                            console.log('sgEditor');
                            //e.parent().attr("data-template", encodeURIComponent(originHtml));
                            e.parent().attr("data-cell-tag", e[0].tagName);
                            e.parent().attr("data-cell-type", a.type);
                            e.parent().attr("data-cell-field", a.ngModel);
                            e.remove();
                        }
                    };
                }
            };
        }]);
});
Q.angularDefine(function (mdl) {
    mdl.directive('sgRowTemplate', [function () {
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
});
Q.angularDefine(function (mdl) {
    mdl.directive('sgHeader', [function () {
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
//# sourceMappingURL=ui.slickgrid.js.map