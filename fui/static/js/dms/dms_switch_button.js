var dms_mdl = dms_mdl || angular.module("ui-dms", ["q-ui"]);
/*
 * <dms-switch>
 * </dms-switch>
 * */
dms_mdl.directive("dmsSwitch", ["$parse", function ($parse) {
    return {
        restrict: "ACE",
        template: '<div class="switch">'+
            '<input type="checkbox">'+
            '<p class="slider mb-0">'+
            '</p>'+
            '</div>',
        replace: true,
        link: function (s, e, a) {
            var fn = undefined;
            var component = {
                value: undefined    
            };
            if (a.id) {
                $parse("$components." + a.id).assign(s, component);
            }
           
            component.value = s.$eval(a.ngModel);
            if (component.value === true) {
                $(e[0]).addClass("change");
            }
            else {
                $(e[0]).removeClass("change");
            }
            s.$watch(a.ngModel, function (n, o) {
                component.value = n === true;
                if (component.value === true) {
                    $(e[0]).addClass("change");
                }
                else {
                    $(e[0]).removeClass("change");
                }
            });
            $(e[0]).on("click", function () {
                if ($(e[0]).hasClass("change")) {
                    component.value = false;
                    $(e[0]).removeClass("change");
                }
                else {
                    component.value = true;
                    $(e[0]).addClass("change");
                }
                $parse(a.ngModel).assign(s, component.value);
                var fn = s.$eval(a.ngChange);
                if (angular.isFunction(fn)) {
                    fn(component.value);
                }
                s.$applyAsync();
            });
        }
    };
}]);