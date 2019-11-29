var dms_mdl = dms_mdl || angular.module("ui-dms", ["q-ui"]);
/*
 * <dms-select 
 * key="{key field name}"
 * text="{text field name}"
 * on-open="{event}"
 * selected-data="{binding selected item}"
 * multi='true'
 * >
 * </dms-select>
 * */
dms_mdl.directive("dmsSelect", ["$parse", function ($parse) {
    return {
        restrict: "ACE",
        template: '<div style="width:100%"></div>',
        replace: true,
        transclude1:true,
        link: function (s, e, a) {
            function builData(data,cacheData, key, text) {
                var ret = [];
                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    var pItem = {
                        id: item[key],
                        text:item[text]
                    };
                    cacheData["key_" + pItem.id] = item;
                    ret.push(pItem);
                }
                return ret;
            }
            
            
            //$(e[0]).select2();
            var fn = undefined;
           
            var component = {
                value: undefined,
                keyField: a.key,
                text:a.text,
                data: [],
                cacheData: {},
                oldSource: undefined,
                placeholder: undefined,
                cmp: undefined
                
            };
            if (a.multi) {
                component.cmp = $("<select style='width:100%' ui-id='select' multiple='multiple'></select>");
            }
            else {
                component.cmp = $("<select style='width:100%' ui-id='select'></select>");
            }
            
            
            component.cmp.appendTo(e[0]);
            $($(e[0]).find("[ui-id='select']")[0]).select2({});
            var isManualChange = false;
            function paint(e, options) {
                component.cmp.empty();
                var x=component.cmp.select2(options);
                component.cmp.val(component.value);
                component.cmp.trigger('change');
                if (!component.$hasPaint) {
                    x.on("change", function (evt) {

                        isManualChange = true;
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
                        setTimeout(function () { isManualChange = false; }, 100);

                    });
                    component.$hasPaint = true;
                }
            }
            if (a.id) {
                $parse("$components." + a.id).assign(s, component);
            }
            var initData = component.oldSource= s.$eval(a.source);
            if (angular.isDefined(initData)) {
                component.data = builData(initData, component.cacheData, component.keyField, component.text);
                paint(e, {
                    data: component.data,
                    placeholder: component.placeholder
                }, opOpen);
            }
            component.value = s.$eval(a.ngModel);
            s.$watch(a.source, function (n, o) {
                if (angular.isUndefined(n)) return;
                if ((n !== component.oldSource)||(n.length !== component.oldSource.length)) {
                    component.oldSource = n;
                    component.data = builData(n, component.cacheData, component.keyField, component.text);
                    paint(e, {
                        data: component.data,
                        placeholder: component.placeholder
                    });
                }
            });
            s.$watch(a.ngModel, function (o, v) {
                if (isManualChange) return;
                if (angular.isUndefined(component.cmp)) return;
                component.value = o;
                if (!component.cmp) return;
                component.cmp.val(o);
                component.cmp.trigger('change');
            });
            s.$watch(function () {
                var v = s.$eval(a.source);
                if (angular.isArray(v)) {
                    return v.length;
                }
            }, function (n, o) {
                if (angular.isUndefined(n)) return;
                if (n !== o) {
                    var v = s.$eval(a.source);
                    
                    component.data = builData(v, component.cacheData, component.keyField, component.text);
                    paint(e, {
                        data: component.data,
                        placeholder: component.placeholder
                    });
                }
            });
            a.$observe("placeholder", function (v) {
                component.placeholder = v;
                paint(e, {
                    data: component.data,
                    placeholder: component.placeholder
                });

            });
           
        }
    };
}]);