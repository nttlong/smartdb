var dms_mdl = dms_mdl || angular.module("ui-dms", ["q-ui"]);
/*
 * <dms-v-box> 
 * <label>{{}}</label>
 * </dms-v-box >
 * */
dms_mdl.directive("dmsVBox", ["$parse", "$components", function ($parse, $components) {
    return {
        scope: false,
        restrict: "ACE",
        template: '<div class="clearfix mb_15">'+
        '<div id="container" ng-transclude style="display:none"></div>'+
        '</div>',
        replace: true,
        transclude: true,
        priority:200000,
        link: function (s, e, a) {
            var container = $($(e[0]).find("#container")[0]);
            var component = {
                caption: undefined
            };
            container.children().each(function (i, ele) {
                
                if ($(ele).prop("tagName") === "INPUT") {
                    $(ele).addClass("txt_ip");
                }
                if ($(ele).prop("tagName") !== "LABEL") {
                    $(ele).appendTo(e[0]);
                }
                
            });
            s.$watch(function () {
                return container.find("label").html();
            }, function (n, o) {
                
                if (component.caption) {
                    component.caption.remove();
                }
                if (component.isRequire) {
                    component.caption = $('<p class="lb">' + n + '&nbsp;(<span class="red">*</span>)</p>');
                }
                else {
                    component.caption = $('<p class="lb">' + n + '</p>');
                }
                
                component.caption.prependTo(e[0]);
                });
            a.$observe("require", function (v) {
                component.isRequire = v;
                if (component.caption) {
                    component.caption.html(component.caption.html() + '&nbsp;(<span class="red">*</span>)');
                    if (component.caption.html().indexOf('&nbsp;(<span class="red">*</span>)') > -1) {
                        var txt = component.caption.html();
                        txt = txt.substring(0, txt.length - '&nbsp;(<span class="red">*</span>)'.length);
                        component.caption.html(txt);
                    }
                }
                
            });
        }
    };
}]);