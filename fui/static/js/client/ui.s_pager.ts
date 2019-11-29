Q.angularDefine(mdl => {
    mdl.directive("sPager", ["$parse", ($parse: angular.IParseService) => {
        return {
            restrict: "ECA",
            replace: true,
            template:`<ul class="pagination  pull-right" style="position:relative !important">
                    </ul>`,
            link: (s, e, a: angular.IAttributes & {
                totalItems: any,
                pageSize: any,
                pageIndex: any
            }) => {
                class clsPager {
                    pageIndex: number;
                    scope: angular.IScope;
                    attr: angular.IAttributes & { totalItems: any; pageSize: any; pageIndex: any; };
                    paint(): any {
                        if (!this.totalItems) {
                            this.ele.hide();
                            return;
                        }
                        this.ele.show();
                        this.ele.empty();
                        if (this.totalItems < this.pageSize) return;
                        var numOfPages = Math.floor(this.totalItems / this.pageSize);
                        if (this.totalItems % this.pageSize > 0) {
                            numOfPages++;
                        }
                        var numOfGroup = Math.floor(numOfPages / this.quickPage);
                        if (numOfPages % this.quickPage > 0) {
                            numOfPages++;
                        }
                        var w = Math.floor((this.pageIndex + 1) / this.quickPage);
                        var startPage = w * this.quickPage;
                        var endPage = (w + 1) * this.quickPage;
                        var renderPages = endPage;
                        if (renderPages > numOfPages) {
                            renderPages = numOfPages;
                        }
                        var me = this;
                        
                        if (me.pageIndex * this.pageSize >= this.totalItems - 1) {
                            startPage = me.pageIndex - this.quickPage;
                            endPage = me.pageIndex;
                            
                            var startPageIndex = (w - 1) * this.quickPage;
                            var elePage = $(`<li class="page-item" data-value="${startPageIndex}"><a class="page-link" href="javascript:void(0);" data-value="${startPageIndex}">...</a></li>`);
                            elePage.appendTo(me.ele[0]);
                            elePage.bind("click", evt => {
                                $parse(me.attr.pageIndex).assign(me.scope, Number($(evt.target).attr("data-value")));
                                $parse(me.attr.ngModel).assign(me.scope, Number($(evt.target).attr("data-value")));
                                if (me.attr.ngChange) {
                                    me.scope.$eval(me.attr.ngChange);
                                }
                                me.scope.$applyAsync();
                            });
                        }
                        else if (me.pageIndex >= this.quickPage) {
                            var startPageIndex = (w - 1) * this.quickPage;
                            var elePage = $(`<li class="page-item" data-value="${startPageIndex}"><a class="page-link" href="javascript:void(0);" data-value="${startPageIndex}">...</a></li>`);
                            elePage.appendTo(me.ele[0]);
                            elePage.bind("click", evt => {
                                if (angular.isDefined(me.attr.pageIndex)) {
                                    $parse(me.attr.pageIndex).assign(me.scope, Number($(evt.target).attr("data-value")));
                                }
                                $parse(me.attr.ngModel).assign(me.scope, Number($(evt.target).attr("data-value")));
                                if (me.attr.ngChange) {
                                    me.scope.$eval(me.attr.ngChange);
                                }
                                me.scope.$applyAsync();
                            });
                        }
                        for (var i = startPage; i < renderPages; i++) {
                            if (i !== me.pageIndex) {
                                var elePage = $(`<li class="page-item" data-value="${i}"><a class="page-link" href="javascript:void(0);" data-value="${i}">${i + 1}</a></li>`);
                                elePage.appendTo(me.ele[0]);
                                elePage.bind("click", evt => {
                                    if (angular.isDefined(me.attr.pageIndex)) {
                                        $parse(me.attr.pageIndex).assign(me.scope, Number($(evt.target).attr("data-value")));
                                    }
                                    $parse(me.attr.ngModel).assign(me.scope, Number($(evt.target).attr("data-value")));
                                    if (me.attr.ngChange) {
                                        me.scope.$eval(me.attr.ngChange);
                                    }
                                    me.scope.$applyAsync();
                                });
                            }
                            else {
                                var elePage = $(`<li class="page-item" data-value="${i}"><a class="page-link" href="javascript:void(0);" data-value="${i}"><b>${i + 1}</b></a></li>`);
                                elePage.appendTo(me.ele[0]);
                                
                            }
                        }
                        if (renderPages < numOfPages) {
                            var elePage = $(`<li class="page-item" data-value="${i}"><a class="page-link" href="javascript:void(0);" data-value="${i}">...</a></li>`);
                            elePage.appendTo(this.ele[0]);
                            elePage.bind("click", evt => {
                                $parse(me.attr.pageIndex).assign(me.scope, Number($(evt.target).attr("data-value")));
                                $parse(me.attr.ngModel).assign(me.scope, Number($(evt.target).attr("data-value")));
                                if (me.attr.ngChange) {
                                    me.scope.$eval(me.attr.ngChange);
                                }
                                me.scope.$applyAsync();
                            });
                        }
                    }
                    totalItems: number;
                    pageSize: number;
                    quickPage: number;
                    ele: JQLite;

                }
                var cmp = new clsPager();
                cmp.ele = e;
                cmp.scope = s;
                cmp.quickPage = Number(a.quickPage);
                cmp.totalItems = Number(s.$eval(a.totalItems) || 0);
                cmp.pageSize = Number(s.$eval(a.pageSize) || 50);
                cmp.pageIndex = (Number(s.$eval(a.pageIndex) || Number(s.$eval(a.ngModel))))||0;
                
                cmp.paint();

                cmp.attr = a;
                s.$watch(a.totalItems, (n: number, o: number) => {
                    
                    cmp.totalItems = n;
                    cmp.paint();
                });
                s.$watch(a.pageSize, (n: number, o: number) => {
                    
                    cmp.pageSize = n;
                    cmp.paint();
                });
                s.$watch(a.ngModel, (n: number, o: number) => {
                    cmp.pageIndex = n;
                    cmp.paint();
                });
               
            }
        }

    }])
});