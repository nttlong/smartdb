
angularDefine(function (mdl) {
    mdl.directive('imageUpload', ["$parse", "$filter", "$components", function ($parse, $filter, $components) {
        return {
            restict: "CEA",
            template: "<div class='imageUpload'></div>",
            scope: {
                src: '='
            },
            link: function (s, e, a) {
                var image = $('<img src=""></img>');

                if (a.width)
                    image.css("width",a.width);

                if (a.height)
                    image.css("height", a.height);

                if (s.src) {
                    image[0].src = "";
                    image[0].src = s.src || "";
                }
                var uploadicon = $('<i class="imageaction imageupload fa fa-upload" aria-hidden="true"></i>');
                var deleteicon = $('<i class="imageaction imagedelete fa fa-trash" aria-hidden="true"></i>');
                $(e[0]).append(image);
                $(e[0]).append(uploadicon);
                $(e[0]).append(deleteicon);

                var file = document.createElement('input');
                file.type = "file";
                
                $(e[0]).append(file);
                $(file).hide();
                $(file).change(function () {
                    if (this.files.length == 0)
                        return;
                    var file = this.files[0];
                    if (file.size > 1024 * 1024) {
                        alert('max upload size is 1M');
                    }
                    else {
                        var formData = new FormData();
                        formData.append('imageData', file);

                        var reader = new FileReader();
                        reader.onloadend = function () {
                            s.src = reader.result;
                            s.$applyAsync();
                        };
                        reader.readAsDataURL(file);
                    }

                });

                $(e[0]).mouseover(function () {
                    $(e[0]).find(".imageaction").show();
                });
                $(e[0]).mouseout(function () {
                    $(e[0]).find(".imageaction").hide();
                });

                uploadicon.click(function () {
                    file.value = null;
                    $(file).trigger('click');
                });

                deleteicon.click(function () {
                    s.src = "";
                    s.$applyAsync();
                });

                s.$watch("src", function (n, o) {
                    image[0].src = n;
                });
            }
        }
    }]);

    mdl.directive('boolFormatter', function () {
        return {
            require: 'ngModel',
            link: function (scope, elem, attrs, ngModel) {
                ngModel.$parsers.push(function (value) {
                    return value===true?1:0;
                });
                ngModel.$formatters.push(function (value) {
                    return value===0?false:true;
                });
            }
        };
    });
    
    mdl.filter('dropdownTreeFilter', function () {
        function findbyId(code, items) {
            for (var i = 0; i < items.length; i++) {
                if (items[i].Code == code) {
                    return items[i];
                }
            }
            return null;
        }
        function findbyIdShow(code, items) {
            for (var i = 0; i < items.length; i++) {
                if (items[i].Code == code && items[i]._hidden ==false) {
                    return items[i];
                }
            }
            return null;
        }
        return function (items, props, keyvalue) {


            var out = [];
            
            if (angular.isArray(items)) {
                if (!props || props === "") {
                    items.forEach(function (item) {
                        item._hidden = false;
                    });
                }
                else {
                    items.forEach(function (item) {
                        item._hidden = true;
                        if (item.Name.toLowerCase().indexOf(props) >= 0) {
                            item._hidden = false;
                            if (item.Parent) {
                                var p = findbyId(item.Parent, items);
                                while (p) {
                                    p._hidden = false;

                                    if (p.Parent) {
                                        p = findbyId(p.Parent, items);
                                    } else {
                                        p = null;
                                    }
                                }
                            }
                        }
                    });
                }

                items.forEach(function (item) {
                    if (item._hidden == false) {
                        if (item.Parent != null) {
                            var parent = findbyId(item.Parent, items);
                            var cbParent = findbyIdShow(item.Parent, items);
                            if (parent != null && parent.isCollapsed == false && cbParent != null) {
                                item._hidden = false;
                            }
                            else {
                                item._hidden = true;
                            }
                        }
                    }
                });

                items.forEach(function (item) {
                    if (item._hidden == false) {
                        out.push(item);
                    }
                });
            }
            else {
                // Let the output be the input untouched
                out = items;
            }

            return out;
        };
    });

});