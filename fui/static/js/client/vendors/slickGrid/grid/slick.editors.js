/***
 * Contains basic SlickGrid editors.
 * @module Editors
 * @namespace Slick
 */

(function ($) {
    // register namespace
    $.extend(true, window, {
        "Slick": {
            "Editors": {
                "Text": TextEditor,
                "Integer": IntegerEditor,
                "Date": DateEditor,
                "YesNoSelect": YesNoSelectEditor,
                "Checkbox": CheckboxEditor,
                "PercentComplete": PercentCompleteEditor,
                "LongText": LongTextEditor,
                "Combobox": SelectEditor,
                "ObjectValue": GetObjectValue,
            }
        }
    });

    function TextEditor(args) {
        var $input;
        var defaultValue;
        var scope = this;

        this.init = function () {

            $input = $("<INPUT type=text class='editor-text textEditor' />")
                .appendTo(args.container)
                .bind("keydown.nav", function (e) {
                    if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                        e.stopImmediatePropagation();
                    }
                })
                .focus()
                .select();

            $input.blur(function () {
                if (Slick.GlobalEditorLock.isActive())
                    Slick.GlobalEditorLock.commitCurrentEdit();
            });
        };

        this.destroy = function () {
            $input.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.getValue = function () {
            return $input.val();
        };

        this.setValue = function (val) {
            $input.val(val);
        };

        this.loadValue = function (item) {
            if (args.column.field.indexOf('.') != -1) {
                var fields = args.column.field.split('.');
                var obj = item[fields[0]];
                if (obj) defaultValue = obj[fields[1]];
            }
            else
                defaultValue = item[args.column.field];

            $input.val(defaultValue);
            $input[0].defaultValue = defaultValue;
            $input.select();
        };

        this.serializeValue = function () {
            return $input.val();
        };

        this.applyValue = function (item, state) {
            if (defaultValue != state) {
                if (args.column.field.indexOf('.') != -1) {
                    var fields = args.column.field.split('.');
                    if (!item[fields[0]]) item[fields[0]] = {};
                    item[fields[0]][fields[1]] = state;
                }
                else
                    item[args.column.field] = state;

                if (!item.slickRowState)
                    item.slickRowState = 'update';
            }
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function () {
            if (args.column.validator) {
                var validationResults = args.column.validator($input.val());
                if (!validationResults.valid) {
                    return validationResults;
                }
            }

            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function IntegerEditor(args) {
        var $input;
        var defaultValue;
        var option = args.column.opt;

        var lockEdit = false;
        var item = args.item;

        if (item && item.fieldPermission) {
            if (!item.fieldPermission[capitalizeText(args.column.field)] || !item.fieldPermission[capitalizeText(args.column.field)].update)
                lockEdit = true;
        }

        if (lockEdit) return;

        this.init = function (a) {
            $input = $("<INPUT name='quantity min='0' class='editor-text numberEditor' />");

            $input.bind("keydown.nav", function (e) {
                if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                    e.stopImmediatePropagation();
                }
            });

            $input.appendTo(args.container);
            $input.focus().select();

            $input.keydown(function (e) {
                var value = $input.val();
                var minValue = option ? option.min : null;
                var maxValue = option ? option.max : null;

                if (e.keyCode == "38") {
                    value++;

                    if (maxValue == null || value <= maxValue)
                        $input.val(value);

                    e.stopPropagation();
                }
                else if (e.keyCode == "40") {
                    value--;

                    if (minValue == null || minValue <= value)
                        $input.val(value);

                    e.stopPropagation();
                }
            });

            $input.blur(function () {
                if (Slick.GlobalEditorLock.isActive())
                    Slick.GlobalEditorLock.commitCurrentEdit();
            });
        };

        this.destroy = function () {
            $input.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            if (args.column.field.indexOf('.') != -1) {
                var fields = args.column.field.split('.');
                var obj = item[fields[0]];
                if (obj) defaultValue = obj[fields[1]];
            }
            else
                defaultValue = item[args.column.field];

            $input.val(defaultValue);
            $input[0].defaultValue = defaultValue;
            $input.select();
        };

        this.serializeValue = function () {
            return parseInt($input.val(), 10) || 0;
        };

        this.applyValue = function (item, state) {
            if (defaultValue != state) {
                var minValue = option ? option.min : null;
                var maxValue = option ? option.max : null;

                if ((minValue != null && state < minValue) || (maxValue != null && maxValue < state)) return;

                if (args.column.field.indexOf('.') != -1) {
                    var fields = args.column.field.split('.');
                    if (!item[fields[0]]) item[fields[0]] = {};
                    item[fields[0]][fields[1]] = state;
                }
                else {
                    item[args.column.field] = state;
                }

                if (!item.slickRowState)
                    item.slickRowState = 'update';
            }
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function () {
            if (isNaN($input.val())) {
                return {
                    valid: false,
                    msg: "Please enter a valid integer"
                };
            }

            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function DateEditor(args) {
        var $input;
        var defaultValue;
        var calendarOpen = false;

        this.init = function () {
            var $dateEditor = $('<div class="input-group date"><INPUT type="text" class="editor-text dateEditor"/><div class="input-group-addon"><span class="bowtie-icon bowtie-calendar"></span></div></div>');
            $dateEditor.appendTo(args.container);
            $input = $(".dateEditor");

            var format = moment().locale(keyLang).localeData()._longDateFormat['l'];
            if (format) format = format.toLowerCase().replace('ddd', 'D').replace('mmm', 'M');
            else keyLang == "vn" ? format = "dd/mm/yyyy" : "mm/dd/yyyy";

            $input.datepicker({
                autoclose: true,
                todayHighlight: true,
                showOnFocus: true,
                format: format
            });

            $input.width($input.width() - 18);

            $(".slick-row .input-group .input-group-addon").click(function () {
                $input.focus();
            });

            $input.focus().select();

            //$input.datepicker().on("hide", function () {
                //if (Slick.GlobalEditorLock.isActive())
                //    Slick.GlobalEditorLock.commitCurrentEdit();
            //});
        };

        this.destroy = function () {
            $.datepicker.dpDiv.stop(true, true);
            $input.datepicker("hide");
            $input.datepicker("destroy");
            $input.remove();
        };

        this.show = function () {
            if (calendarOpen) {
                $.datepicker.dpDiv.stop(true, true).show();
            }
        };

        this.hide = function () {
            if (calendarOpen) {
                $.datepicker.dpDiv.stop(true, true).hide();
            }
        };

        this.position = function (position) {
            if (!calendarOpen) {
                return;
            }
            $.datepicker.dpDiv
                .css("top", position.top + 30)
                .css("left", position.left);
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            if (args.column.field.indexOf('.') != -1) {
                var fields = args.column.field.split('.');
                var obj = item[fields[0]];
                if (obj) defaultValue = obj[fields[1]];
            }
            else
                defaultValue = item[args.column.field] || '';

            $input.defaultValue = defaultValue;
            $input.datepicker('update', (moment(defaultValue).format("l")));

            $input.select();
        };

        this.serializeValue = function () {
            return $input.val();
        };

        this.applyValue = function (item, state) {
            var value = moment(state, 'l').format();
            if (defaultValue != value) {
                if (args.column.field.indexOf('.') != -1) {
                    var fields = args.column.field.split('.');
                    if (!item[fields[0]]) item[fields[0]] = {};
                    item[fields[0]][fields[1]] = value;
                }
                else
                    item[args.column.field] = value;

                if (!item.slickRowState)
                    item.slickRowState = 'update';
            }
        };

        this.isValueChanged = function () {
            var isChanged = (!($input.val() == "" && $input.defaultValue == null)) && ($input.val() != $input.defaultValue);
            return isChanged;
        };

        this.validate = function () {
            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function YesNoSelectEditor(args) {
        var $select;
        var defaultValue;
        var scope = this;

        this.init = function () {
            $select = $("<SELECT tabIndex='0' class='editor-yesno'><OPTION value='yes'>Yes</OPTION><OPTION value='no'>No</OPTION></SELECT>");
            $select.appendTo(args.container);
            $select.focus();
        };

        this.destroy = function () {
            $select.remove();
        };

        this.focus = function () {
            $select.focus();
        };

        this.loadValue = function (item) {
            $select.val((defaultValue = item[args.column.field]) ? "yes" : "no");
            $select.select();
        };

        this.serializeValue = function () {
            return ($select.val() == "yes");
        };

        this.applyValue = function (item, state) {
            if (item[args.column.field] != state) {
                item[args.column.field] = state;

                if (!item.slickRowState)
                    item.slickRowState = 'update';
            }
        };

        this.isValueChanged = function () {
            return ($select.val() != defaultValue);
        };

        this.validate = function () {
            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function CheckboxEditor(args) {
        var $input;
        var defaultValue;
        var scope = this;

        this.init = function () {
            $input = $("<INPUT type=checkbox value='true' class='editor-checkbox' hideFocus>");
            $input.appendTo(args.container);
            $input.focus();

            $input.change(function () {
                if (Slick.GlobalEditorLock.isActive())
                    Slick.GlobalEditorLock.commitCurrentEdit();
            });
        };

        this.destroy = function () {
            $input.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            defaultValue = !!item[args.column.field];
            if (args.column.field.indexOf('.') != -1) {
                var fields = args.column.field.split('.');
                var obj = item[fields[0]];
                if (obj) defaultValue = obj[fields[1]];
            }
            else
                defaultValue = item[args.column.field];

            if (defaultValue) {
                $input.prop('checked', true);
            } else {
                $input.prop('checked', false);
            }
        };

        this.serializeValue = function () {
            return $input.prop('checked');
        };

        this.applyValue = function (item, state) {
            if (defaultValue != state) {
                if (args.column.field.indexOf('.') != -1) {
                    var fields = args.column.field.split('.');
                    if (!item[fields[0]]) item[fields[0]] = {};
                    item[fields[0]][fields[1]] = state;
                }
                else
                    item[args.column.field] = state;

                if (!item.slickRowState)
                    item.slickRowState = 'update';
            }
        };

        this.isValueChanged = function () {
            return (this.serializeValue() !== defaultValue);
        };

        this.validate = function () {
            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function PercentCompleteEditor(args) {
        var $input, $picker;
        var defaultValue;
        var scope = this;

        this.init = function () {
            $input = $("<INPUT type=text class='editor-percentcomplete' />");
            $input.width($(args.container).innerWidth() - 25);
            $input.appendTo(args.container);

            $picker = $("<div class='editor-percentcomplete-picker' />").appendTo(args.container);
            $picker.append("<div class='editor-percentcomplete-helper'><div class='editor-percentcomplete-wrapper'><div class='editor-percentcomplete-slider' /><div class='editor-percentcomplete-buttons' /></div></div>");

            $picker.find(".editor-percentcomplete-buttons").append("<button val=0>Not started</button><br/><button val=50>In Progress</button><br/><button val=100>Complete</button>");

            $input.focus().select();

            $picker.find(".editor-percentcomplete-slider").slider({
                orientation: "vertical",
                range: "min",
                value: defaultValue,
                slide: function (event, ui) {
                    $input.val(ui.value)
                }
            });

            $picker.find(".editor-percentcomplete-buttons button").bind("click", function (e) {
                $input.val($(this).attr("val"));
                $picker.find(".editor-percentcomplete-slider").slider("value", $(this).attr("val"));
            })
        };

        this.destroy = function () {
            $input.remove();
            $picker.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            if (args.column.field.indexOf('.') != -1) {
                var fields = args.column.field.split('.');
                var obj = item[fields[0]];
                if (obj) defaultValue = obj[fields[1]];
            }
            else
                defaultValue = item[args.column.field];

            $input.val(defaultValue);
            $input.select();
        };

        this.serializeValue = function () {
            return parseInt($input.val(), 10) || 0;
        };

        this.applyValue = function (item, state) {
            if (defaultValue != state) {
                if (args.column.field.indexOf('.') != -1) {
                    var fields = args.column.field.split('.');
                    if (!item[fields[0]]) item[fields[0]] = {};
                    item[fields[0]][fields[1]] = state;
                }
                else
                    item[args.column.field] = state;

                if (!item.slickRowState)
                    item.slickRowState = 'update';
            }
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ((parseInt($input.val(), 10) || 0) != defaultValue);
        };

        this.validate = function () {
            if (isNaN(parseInt($input.val(), 10))) {
                return {
                    valid: false,
                    msg: "Please enter a valid positive number"
                };
            }

            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function LongTextEditor(args) {
        var $input, $wrapper;
        var defaultValue;
        var scope = this;

        this.init = function () {
            var $container = $("body");

            $wrapper = $("<DIV style='z-index:10000;position:absolute;background:white;padding:5px;border:3px solid gray; -moz-border-radius:10px; border-radius:10px;'/>")
                .appendTo($container);

            $input = $("<TEXTAREA hidefocus rows=5 style='backround:white;width:250px;height:80px;border:0;outline:0'>")
                .appendTo($wrapper);

            $("<DIV style='text-align:right'><BUTTON>Save</BUTTON><BUTTON>Cancel</BUTTON></DIV>")
                .appendTo($wrapper);

            $wrapper.find("button:first").bind("click", this.save);
            $wrapper.find("button:last").bind("click", this.cancel);
            $input.bind("keydown", this.handleKeyDown);

            scope.position(args.position);
            $input.focus().select();
        };

        this.handleKeyDown = function (e) {
            if (e.which == $.ui.keyCode.ENTER && e.ctrlKey) {
                scope.save();
            } else if (e.which == $.ui.keyCode.ESCAPE) {
                e.preventDefault();
                scope.cancel();
            } else if (e.which == $.ui.keyCode.TAB && e.shiftKey) {
                e.preventDefault();
                args.grid.navigatePrev();
            } else if (e.which == $.ui.keyCode.TAB) {
                e.preventDefault();
                args.grid.navigateNext();
            }
        };

        this.save = function () {
            args.commitChanges();
        };

        this.cancel = function () {
            $input.val(defaultValue);
            args.cancelChanges();
        };

        this.hide = function () {
            $wrapper.hide();
        };

        this.show = function () {
            $wrapper.show();
        };

        this.position = function (position) {
            $wrapper
                .css("top", position.top - 5)
                .css("left", position.left - 5)
        };

        this.destroy = function () {
            $wrapper.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            if (args.column.field.indexOf('.') != -1) {
                var fields = args.column.field.split('.');
                var obj = item[fields[0]];
                if (obj) defaultValue = obj[fields[1]];
            }
            else
                defaultValue = item[args.column.field];

            $input.val(defaultValue);
            $input.select();
        };

        this.serializeValue = function () {
            return $input.val();
        };

        this.applyValue = function (item, state) {
            if (defaultValue != state) {
                if (args.column.field.indexOf('.') != -1) {
                    var fields = args.column.field.split('.');
                    if (!item[fields[0]]) item[fields[0]] = {};
                    item[fields[0]][fields[1]] = state;
                }
                else
                    item[args.column.field] = state;

                if (!item.slickRowState)
                    item.slickRowState = 'update';
            }
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function () {
            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function SelectEditor(args) {
        var $input;
        var defaultValue;

        this.keyCaptureList = [Slick.keyCode.UP, Slick.keyCode.DOWN, Slick.keyCode.ENTER];
        this.init = function () {
            $input = $('<select class="gridSelectWrapper"></select>');
            $input.width(args.container.clientWidth + 3);
            $input.appendTo(args.container);
            $input.focus().select();

            if (args.column.comboboxSetting) {
                var currentId = null;
                if (args.item) currentId = args.item[args.column.field];

                var param = args.column.comboboxSetting.param;
                param.currentId = currentId;

                $.ajax({
                    url: args.column.comboboxSetting.url,
                    data: param,
                    type: "POST",
                    dataType: "json",
                    async: false,
                    contentType: "application/x-www-form-urlencoded; charset=utf-8",
                    success: function (response) {
                        args.column.dataSource = response.data;
                    }
                });

                $input.select2({
                    placeholder: '-',
                    allowClear: true,
                    dropdownAutoWidth: true,
                    templateResult: args.column.formatState,
                    data: args.column.dataSource,
                    ajax: {
                        delay: 250,
                        url: args.column.comboboxSetting.url,
                        type: "POST",
                        async: false,
                        dataType: 'json',
                        contentType: "application/x-www-form-urlencoded; charset=utf-8",
                        data: function (params) {
                            args.column.comboboxSetting.param.searchValue = params.term;
                            args.column.comboboxSetting.param.currentId = currentId;
                            return args.column.comboboxSetting.param;
                        },
                        processResults: function (response) {
                            if (!response.isError) {
                                args.column.dataSource = response.data;
                                if (args.column.comboboxSetting.addHeader)
                                    args.column.dataSource.splice(0, 0, { id: 'header', disabled: true });
                            }
                            else {
                                args.column.dataSource = [];
                                if (args.column.comboboxSetting.addHeader)
                                    args.column.dataSource.splice(0, 0, { id: 'header', disabled: true });
                            }

                            return {
                                results: args.column.dataSource
                            };
                        }
                    },
                });
            }
            else {
                var dataSource = null;
                if (args.column.dataSource && args.column.dataSource.length > 0) {
                    var item = args.column.dataSource[0];
                    if (typeof item.text == 'object')
                        dataSource = args.column.dataSource.map(function (x) { return { id: x.id, text: x.text[keyLang] } });
                    else
                        dataSource = args.column.dataSource;
                }

                function matchCustom(params, data) {
                    // If there are no search terms, return all of the data
                    if ($.trim(params.term) === '') {
                        return data;
                    }

                    if (data.id == 'header')
                        return data;

                    // Do not display the item if there is no 'text' property
                    if (typeof data.text === 'undefined') {
                        return null;
                    }

                    // `params.term` should be the term that is used for searching
                    // `data.text` is the text that is displayed for the data object

                    var match = false;

                    $.each(data, function (index, prop) {
                        if (index == 'element' || index == "disabled" || index == "selected") return true;

                        if (typeof prop != 'string') return true;

                        var propTemp = prop.toLowerCase();
                        var termTemp = params.term.toLowerCase();

                        if (propTemp.indexOf(termTemp) > -1) {
                            match = true;
                            return false;
                        }
                    });

                    if (match) return data;
                    //if (data.text.indexOf(params.term) > -1) {
                    //var modifiedData = $.extend({}, data, true);
                    //modifiedData.text += ' (matched)';

                    // You can return modified objects from here
                    // This includes matching the `children` how you want in nested data sets
                    //return modifiedData;
                    //}



                    // Return `null` if the term should not be displayed
                    return null;
                }

                $input.select2({
                    placeholder: '-',
                    allowClear: true,
                    dropdownAutoWidth: true,
                    data: dataSource,
                    matcher: matchCustom,
                    templateResult: args.column.formatState,
                });
            }

            $input.on("select2:close", function () {
                if (Slick.GlobalEditorLock.isActive())
                    Slick.GlobalEditorLock.commitCurrentEdit();
            });
        };

        this.destroy = function () {
            $input.select2('destroy');
            $input.remove();
        };

        this.show = function () {
        };

        this.hide = function () {
            $input.select2('results_hide');
        };

        this.position = function (position) {
        };

        this.focus = function () {
            $input.select2('input_focus');
        };

        this.loadValue = function (item) {
            if (args.column.field.indexOf('.') != -1) {
                var fields = args.column.field.split('.');
                var obj = item[fields[0]];
                if (obj) defaultValue = obj[fields[1]];
            }
            else
                defaultValue = item[args.column.field];

            $input.val(defaultValue);
            $input[0].defaultValue = defaultValue;
            $input.trigger("change.select2");
        };

        this.serializeValue = function () {
            return $input.val();
        };

        this.applyValue = function (item, state) {
            if (defaultValue != state) {
                if (args.column.field.indexOf('.') != -1) {
                    var fields = args.column.field.split('.');
                    if (!item[fields[0]]) item[fields[0]] = {};
                    item[fields[0]][fields[1]] = state;
                }
                else {
                    item[args.column.field] = state;
                    if (args.column.comboboxSetting && args.column.comboboxSetting.relateColumn) {
                        var data = $input.select2('data');
                        item[args.column.comboboxSetting.relateColumn] = data[0];
                    }
                }

                if (!item.slickRowState)
                    item.slickRowState = 'update';
            }
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function () {
            return {
                valid: true,
                msg: null
            };
        };

        this.formatter = function (row, cell, value, columnDef, dataContext) {
            return columnDef.dataSource[value] || '-';
        }

        this.populateSelect = function (select, dataSource, addBlank) {
            var newOption;
            if (addBlank) { select.appendChild(new Option('', '')); }
            $.each(dataSource, function (index, item) {
                newOption = new Option(item.text, item.id);
                select.appendChild(newOption);
            });
        };

        this.init();
    }

    function GetObjectValue(item, column) {
        if (!item) return null;

        var val = null;
        if (column.field.indexOf('.') != -1) {
            var fields = column.field.split('.');
            var obj = item[fields[0]];
            if (obj) val = obj[fields[1]];
        }
        else
            val = item[column.field];

        return val;
    }
})(jQuery);