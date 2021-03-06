﻿/// <reference path="types/jqlite.d.ts" />
/// <reference path="types/angular.d.ts" />
/// <reference path="types/select2/index.d.ts" />
/// <reference path="types/q.d.ts.ts" />

Q.angularDefine(mdl => {
    mdl.directive("dp",
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
                return {
                    restrict: "ECA",
                    template: `<input size="16" type="text" value="">`,
                    replace: true,
                    link: (s, e, a) => {
                        var dp = $(e[0])['datepicker']({
                            change: function (e) {
                                $parse(a.field).assign(s, new Date(dp.value()));
                                s.$applyAsync();
                            }
                        });

                        s.$watch(a.field, (value, oldvalue) => {
                            if (value !== oldvalue) {
                                //alert(value);
                            }
                        })

                    }                                               
                }
            }])
});
