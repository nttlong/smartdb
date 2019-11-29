var dms_mdl = dms_mdl || angular.module("ui-dms", ["q-ui"]);
/*
 * <dms-tree key="{key of node}" 
 * text="{text field of data item}" 
 * parent-key="{parent key of node }" 
 * data-source="{Data source name}"
 * on-expand="doLoadChildren"
 * selected-data="{SelectedItem}"
 * ng-model="{DataModel}"
 * on-edit="{Function for edit}"
 * on-select="{Function for select}"
 * >
 * </dms-tree>
 * */
dms_mdl.directive("dmsTree", ["$parse", "$compile", "$components", "$interpolate", function ($parse, $compile, $components, $interpolate) {

    function buildTreeData(data, cacheData, text, id, parentId, rootId) {
        rootId = rootId || null;
        var ret = [];
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            if ((item[parentId] === rootId) && (!item.$$hasBuil)) {
                var pItem = {};
                pItem.id = item[id];
                cacheData["key_" + pItem.id.toString()] = pItem;
                pItem.parentId = rootId;
                pItem.text = item[text];
                item.$$hasBuilt = true;
                ret.push(pItem);
                pItem.children = buildTreeData(data, cacheData, text, id, parentId, pItem.id);
            }
        }
        return ret;
    };
    function paint(e, data, template, scope, $compile) {
        for (var i = 0; i < data.length; i++) {
            var ele = $("<div>" + template + "</div>");
            var subScope = scope.$new();
            subScope.dataItem = data[i];
            $compile(ele.contents())(subScope);
            var childrenEle = ele.find("#children");
            ele.find("#text").html(data[i].text);
            $(ele.children()[0]).attr("data-ui-id", data[i].id);
            ele.children().appendTo(e);
            if (data[i].children) {
                paint(childrenEle, data[i].children, template, scope, $compile);
            }
        }
        
    };
    function initExpaneEvent(e, onClickCallback) {

        $(e[0]).find("[data-ui-id]").click(function (event) {
            var tE = $(event.target);
            var ele = $($(event.target).parents("[data-ui-id]")[0]);
            var id = ele.attr("data-ui-id");
            if (tE.attr("id") === "editor") {
                onClickCallback("edit", id, tE);
                event.preventDefault();
                event.stopPropagation();
                return;
            }
            if ((tE.attr("id") === "selector") || (tE.attr("id") === "text")) {
                onClickCallback("select", id, tE);
                event.preventDefault();
                event.stopPropagation();
                return;
            }
            var nodeEle = $(ele.find("div.collapse")[0]);
            if (nodeEle.hasClass("show")) {
                nodeEle.removeClass("show");
                onClickCallback("hide", id, nodeEle);
                event.preventDefault();
                event.stopPropagation();
                return;
            }
            else {

                onClickCallback("show", id, nodeEle);
                event.preventDefault();
                event.stopPropagation();
                return;
            }

        });
    };
    return {
        restrict: "ACE",
        template: "<div style='width:100%'>" +
            "<div ng-transclude style='display:none' id='template'></div>"
            + "</div>",
        transclude: true,
        replace: true,
        link: function (s, e, a) {
            function actionUI(action, id, ele) {
                var dataItem = component.cacheData["key_" + id];
                if (a.ngModel) {
                    $parse(a.ngModel).assign(s, id);
                }
                if (a.selectedData) {
                    $parse(a.selectedData).assign(s, dataItem);
                }
                if (a.ngChange) {
                    fn = s.$eval(a.ngChange);
                    if (angular.isFunction(fn)) {
                        fn(dataItem);
                    }
                }
                if (action === "show") {
                    var sender = {
                        id: id,
                        dataItem: dataItem,
                        emit: function () {
                            ele.addClass("show");
                        }
                    };
                    if (a.onExpand) {
                        fn = s.$eval(a.onExpand);
                        if (angular.isFunction(fn)) {
                            fn(sender);
                        }
                        else {
                            ele.addClass("show");
                        }
                    }
                    else {
                        ele.addClass("show");
                    }
                }
                if (action === "edit") {
                    fn = s.$eval(a.onEdit);
                    if (angular.isFunction(fn)) {
                        fn(dataItem);
                    }
                }
                if (action === "select") {
                    fn = s.$eval(a.onSelect);
                    if (angular.isFunction(fn)) {
                        fn(dataItem);
                    }
                }
            };
            var fn = undefined;
            var templteHtml = decodeURIComponent(e.attr("data-node-template"));
            var component = {
                id: a.key,
                parentId: a.parentKey,
                text: a.text,
                data: [],
                cacheData: {},
                template: templteHtml,
                addChildNode: function (NodeId, data) {

                    var ChildNode = {
                        id: data[component.id],
                        text: data[component.text],
                        parentId: NodeId
                    };

                    if (!NodeId) {
                        component.source.push(data);
                        component.data = buildTreeData(component.source, component.cacheData, component.text, component.id, component.parentId);
                        $(e[0]).empty();
                        paint(e, component.data, component.template, s, $compile);
                        initExpaneEvent(e, actionUI);
                        return;
                    }
                    
                    var children = component.cacheData["key_" + NodeId].children;
                    if (angular.isUndefined(children)) {
                        children = [];
                    }
                    children.push(ChildNode);
                    var ele = $(e[0]).find('[data-ui-id=' + NodeId + "]");
                    var childrenEle = ele.find("#children");
                    childrenEle.empty();
                    
                    paint(childrenEle[0], children, component.template, s, $compile);
                    component.cacheData["key_" + ChildNode.id] = data;
                    s.$applyAsync();
                    initExpaneEvent(childrenEle[0], actionUI);
                    component.doExpandNode(NodeId);
                },
                doExpandNode: function (nodeId) {
                    var ele = $(e[0]).find('[data-ui-id=' + nodeId + "]");
                    //ele.find(".bowtie-icon.bowtie-chevron-right").trigger("click");
                    var nodeEle = $(ele.find("div.collapse")[0]);
                    if (!nodeEle.hasClass("show")) {
                        actionUI("show", nodeId, nodeEle);
                        event.preventDefault();
                        event.stopPropagation();
                        return;
                    }
                    
                },
                doDeleteNode: function (nodeId) {
                    var ele = $(e[0]).find('[data-ui-id=' + nodeId + "]");
                    ele.remove();
                    var ret = [];
                    for (var i = 0; i < component.source.length; i++) {
                        if (component.source[i][component.id] !== nodeId) {
                            ret.push(component.source[i]);
                        }
                    }
                    component.source = ret;
                    component.data = buildTreeData(component.source, component.cacheData, component.text, component.id, component.parentId);
                }
            };
            $components.assign(a.id, s, component);
            
            
            var initData = s.$eval(a.source);
            if (angular.isDefined(initData)) {
                component.data = buildTreeData(initData, component.cacheData, component.text, component.id, component.parentId);
                $(e[0]).empty();
                paint(e, component.data, component.template, s, $compile);
                initExpaneEvent(e, actionUI);
                s.$applyAsync();
                
            }
            s.$watch(a.source, function (n, o) {
                if (angular.isUndefined(n)) return;
                if (n === o) return;
                component.source = n;
                component.data = buildTreeData(n,component.cacheData, component.text, component.id, component.parentId);
                $(e[0]).empty();
                paint(e, component.data, component.template, s, $compile);
                initExpaneEvent(e, actionUI);
                
            });
        }
    };
}]);

dms_mdl.directive("dmsNodeTemplate", [function () {
    return {
        restrict: "ECA",
        scope: false,
        compile: function (element, attributes) {
            
            var originHtml = element.html();
            element.empty();
            return {
                pre: function (s, e, a, c, t) {
                    e.parent().parent().attr("data-node-template", encodeURIComponent(originHtml));
                    e.remove();

                },
                post: function (s, e, a, c, t) {

                }
            };
        }
    };
}]);