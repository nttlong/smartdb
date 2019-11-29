angularDefine(function (mdl) {
    mdl.directive("hbTreeview", ["$parse", "$compile", function ($parse, $compile) {
        return {
            restrict: "ACE",
            template: '<div style="width:100%"><div id="tree" class="hummingbird-treeview" style="height: 230px; overflow-y: scroll;" ng-transclude></div></div>',
            //replace: true,
            transclude: true,
            link: function (s, e, a) {
                //var x = $('<ul class="hummingbird-base"><li><i class="fa fa-plus"></i><label><input id = "xnode-0" data-id="custom-0" type = "checkbox"/> node - 0</label></li></ul>');
                //var ul = $('<ul><li><i class="fa fa-plus"></i><label><input id = "xnode-1" data-id="custom-1" type = "checkbox"/> node - 2</label></li></ul>');
                //ul.appendTo(x.find("li")[0]);
                //x.appendTo($(e[0]).find('#tree')[0]);

                $(e[0]).find('#tree').hummingbird("expandAll");
            }
        };
    }]);
});
