Q.angularDefine(mdl => {
    mdl.directive("sb", ["$parse", ($parse: angular.IParseService) => {
        return {
            template: `<div style="width:100%;display:flex;flex-direction:row;padding:0" class="search-box input-group">
                       
                        <input type="text" placeholder="" style="flex:1;border:none !important" id="txtSearch"/>
                            <button style="padding:4px;font-size:14px;display:none" id="btnClear" class="btn">
                                <span class="fa fa-times"></span>
                                <i class="bowtie-icon bowtie-close"></i>
                            </button>
                            
                            <button style="padding:4px;font-size:14px" id="btnSearch" class="btn">
                               
                                <i class="bowtie-icon bowtie-search"></i>     
                            </button>
                        </div>`,
            replace: true,
            link: (s, e, a) => {
                var isManualChange = false;
                a.$observe<string>("placeholder", v => {
                    e.find("#txtSearch").attr("placeholder", v);
                });
                e.find("#txtSearch").bind("keyup", evt => {
                    if (evt.keyCode === 13) {
                        evt.preventDefault();
                        evt.stopImmediatePropagation();
                        evt.stopPropagation();
                        var val = undefined;
                        if (angular.isDefined(e.find("#txtSearch").val())) {
                            val = e.find("#txtSearch").val();
                        }
                        $parse(a.ngModel).assign(s, val);
                        var fn = s.$eval(a.ngChange);
                        if (angular.isFunction(fn)) {
                            fn(val);
                        }
                        return;
                    }
                    if (e.find("#txtSearch").val() !== "") {
                        e.find("#btnClear").show();
                    }
                    else {
                        e.find("#btnClear").hide();
                    }
                });
                e.find("#btnSearch").bind("click", evt => {
                    $parse(a.ngModel).assign(s, e.find("#txtSearch").val());
                    var fn = s.$eval(a.ngChange);
                    if (angular.isFunction(fn)) {
                        fn(e.find("#txtSearch").val());
                    }
                });
                e.find("#btnClear").bind("click", () => {
                    isManualChange = true;
                    e.find("#txtSearch").val("");
                    e.find("#btnClear").hide();
                    $parse(a.ngModel).assign(s, undefined);
                    console.log("clear search");
                    var fn = s.$eval(a.ngChange);
                    if (angular.isFunction(fn)) {
                        fn(undefined);
                    }
                    
                });
                s.$watch<string>(a.ngModel, (n, o) => {
                    if (isManualChange) return;
                    e.find("#txtSearch").val(n);
                    if (angular.isDefined(n)) {
                        e.find("#btnClear").show();
                    }
                    isManualChange = false;
                });
            }
        }

    }]);
});