/*
 * <pager  ng-model="pager.pageIndex" 
 *          total-items="pager.totalItems" 
 *          ng-change="{{event}}">
 *          </pager>
 */
angularDefine(function (mdl) {
    mdl.directive('pager', ["$parse", "$filter", "$components", function ($parse, $filter, $components) {
        function paint(cmp, ele) {
            
            if (!cmp.totalItems) return;
            ele.empty();
            var numOfPages = Math.floor(cmp.totalItems / cmp.pageSize);
            if (cmp.totalItems % cmp.pageSize > 0) {
                numOfPages = numOfPages + 1;
            }
            if (cmp.pageIndex - 1 >= 0) {
                var liFirst = $("<li data-page-index='" + (cmp.pageIndex - 1) + "' class='first-page'><a class='page-link'><</a></li>").appendTo(ele[0]);
                liFirst.on("click", function (evt) {
                    evt.stopPropagation();
                    var pageIndex = $(evt.currentTarget).attr("data-page-index") * 1;
                    cmp.pageIndex = pageIndex;
                    cmp.doChange();
                });
            }
            var firstPageIndex = cmp.pageIndex - Math.floor(numOfPages / 2);
            if (firstPageIndex < 0) {
                firstPageIndex += Math.floor(numOfPages / 2);
            }
            if (numOfPages > 1) {
                var liSelectPage = $("<li><a style='padding:0 !important'><input type='number' id='pageIndex' style='width:70px' class='form-control input-sm'/></a></li>").appendTo(ele[0]);
                liSelectPage.find("#pageIndex").val(cmp.pageIndex + 1);
                liSelectPage.find("#pageIndex").on("change", function (evt) {
                    evt.stopPropagation();
                    cmp.pageIndex = ($(evt.currentTarget).val() * 1 - 1);
                    cmp.doChange();
                });
            }
            if (cmp.pageIndex + 1 < numOfPages) {
                var liLast = $("<li data-page-index='" + (cmp.pageIndex + 1) + "' class='first-page'><a class='page-link'>></a></li>").appendTo(ele[0]);
                liLast.on("click", function (evt) {
                    evt.stopPropagation();
                    var pageIndex = $(evt.currentTarget).attr("data-page-index") * 1;
                    cmp.pageIndex = pageIndex;
                    cmp.doChange();
                });
            }
        }
        return {
            restict: "CEA",
            template: "<ul class=\"pagination\"></ul>",
            replace: true,
            link: function (s, e, a) {
                var cmp = {
                    pageSize: s.$eval(a.pageSize)||50,
                    pageIndex: s.$eval(a.ngModel) || 0,
                    totalItems: s.$eval(a.totalItems) || 0,
                    indicators: s.$eval(a.indicators) || 5,
                    doChange: function () {
                        $parse(a.ngModel).assign(s, cmp.pageIndex);
                        var fn = s.$eval(a.ngChange);
                        if (angular.isFunction(fn)) {
                            fn(cmp);

                        }
                        s.$applyAsync();
                    }
                };
                paint(cmp, $(e[0]));
                if (a.ngModel) {
                    $parse(a.ngModel).assign(s, cmp.pageIndex);
                    s.$applyAsync();
                }
                
                if (a.ngChange) {
                    var fn = s.$eval(a.ngChange);
                    if (angular.isFunction(fn)) {
                        cmp.doChange();
                    }
                }
                s.$watch(a.ngModel, function (n, o) {
                    if (angular.isUndefined(n)) return;
                    if (n !== o) {
                        cmp.pageIndex = n;
                        paint(cmp, $(e[0]));
                    }
                    
                });
                s.$watch(a.totalItems, function (v, o) {
                    if (angular.isUndefined(v)) return;
                    if (v === o) return;
                    cmp.totalItems = v;
                    paint(cmp, $(e[0]));
                });
                s.$watch(a.ngChange, function (v, o) {
                    if (angular.isFunction(v)) {
                        cmp.doChange();
                    }

                });
                if (a.id) {
                    $components.assign(a.id, s, cmp);
                    
                }

            }
        }
    }]);
});