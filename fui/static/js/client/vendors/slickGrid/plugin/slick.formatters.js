/***
 * Contains basic SlickGrid formatters.
 * 
 * NOTE:  These are merely examples.  You will most likely need to implement something more
 *        robust/extensible/localizable/etc. for your use!
 * 
 * @module Formatters
 * @namespace Slick
 */

(function ($) {
    // register namespace
    $.extend(true, window, {
        "Slick": {
            "Formatters": {
                "PercentComplete": PercentCompleteFormatter,
                "PercentCompleteBar": PercentCompleteBarFormatter,
                "YesNo": YesNoFormatter,
                "Checkmark": CheckmarkFormatter,
                "TextBox": TextBoxFormatter,
                "Date": DateFormatter,
                "DateTime": DateTimeFormatter,
                "Number": NumberFormatter,
                "Money": MoneyFormatter,
                "Boolen": BoolenFormatter,
                "CheckBox": CheckBoxFormatter,
                "Combobox": ComboboxFormatter,
                "SumTotal": SumTotalFormatter,
                "AvgTotal": AvgTotalFormatter
            }
        }
    });

    function ComboboxFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission && columnDef.field) {
            var field = columnDef.field;
            if (field.indexOf(".") != -1)
                field = field.split('.')[0];

            field = field.substring(0, 1).toUpperCase() + field.substring(1, field.length);
            if (!dataContext.fieldPermission[field]) return "###";
        }

        var source = columnDef.dataSource;
        if (!source) return '-';

        var item = source.find(x => x.id == value);
        if (!item) return '-';

        var text = '';
        if (typeof item.text == 'object')
            text = item.text[keyLang];
        else
            text = item.text;

        if (item.class && item.icon)
            return '<span class="' + item.class + '"><i class="' + item.icon + '"bowtie-icon bowtie-square"></i></span>&nbsp; ' + text;
        else
            return text;
    }

    function PercentCompleteFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission) {
            if (!dataContext.fieldPermission[columnDef.field]) return "###";
        }

        if (value == null || value === "") {
            return "-";
        } else if (value < 50) {
            return "<span style='color:red;font-weight:bold;'>" + value + "%</span>";
        } else {
            return "<span style='color:green'>" + value + "%</span>";
        }
    }

    function PercentCompleteBarFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission) {
            if (!dataContext.fieldPermission[columnDef.field]) return "###";
        }

        if (value == null || value === "") {
            return "";
        }

        var color;

        if (value < 30) {
            color = "red";
        } else if (value < 70) {
            color = "silver";
        } else {
            color = "green";
        }

        return "<span class='percent-complete-bar' style='background:" + color + ";width:" + value + "%'></span>";
    }

    function YesNoFormatter(row, cell, value, columnDef, dataContext) {
        return value ? "Yes" : "No";
    }

    function TextBoxFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission && columnDef.field) {
            var field = columnDef.field;
            if (field.indexOf(".") != -1)
                field = field.split('.')[0];

            field = field.substring(0, 1).toUpperCase() + field.substring(1, field.length);
            if (!dataContext.fieldPermission[field]) return "###";
        }

        return value;
    }

    function CheckmarkFormatter(row, cell, value, columnDef, dataContext) {
        return value ? "<img src='../images/tick.png'>" : "";
    }

    function DateFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission && columnDef.field) {
            var field = columnDef.field;
            if (field.indexOf(".") != -1)
                field = field.split('.')[0];

            field = field.substring(0, 1).toUpperCase() + field.substring(1, field.length);
            if (!dataContext.fieldPermission[field]) return "###";
        }
        if (value) {
            return '<div style="width:100%;text-align:center">' + moment(value).format("l") + '</div>';
        }
    }

    function DateTimeFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission && columnDef.field) {
            var field = columnDef.field;
            if (field.indexOf(".") != -1)
                field = field.split('.')[0];

            field = field.substring(0, 1).toUpperCase() + field.substring(1, field.length);
            if (!dataContext.fieldPermission[field]) return "###";
        }

        if (value)
            return '<div style="width:100%;text-align:center">' + moment(value).format("lll") + '</div>';
    }

    function NumberFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission && columnDef.field) {
            var field = columnDef.field;
            if (field.indexOf(".") != -1)
                field = field.split('.')[0];

            field = field.substring(0, 1).toUpperCase() + field.substring(1, field.length);
            if (!dataContext.fieldPermission[field]) return "###";
        }

        if (value)
            return '<div style="width:100%;text-align:right">' + accounting.formatNumber(value) + '</div>';
        else
            return '<div style="width:100%;text-align:right">' + accounting.formatNumber(0) + '</div>';
    }

    function MoneyFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission && columnDef.field) {
            var field = columnDef.field;
            if (field.indexOf(".") != -1)
                field = field.split('.')[0];

            field = field.substring(0, 1).toUpperCase() + field.substring(1, field.length);
            if (!dataContext.fieldPermission[field]) return "###";
        }

        if (value)
            return '<div style="width:100%;text-align:right">' + accounting.formatMoney(value) + '</div>';
        else
            return '<div style="width:100%;text-align:right">' + accounting.formatMoney(0) + '</div>';
    }

    function BoolenFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission && columnDef.field) {
            var field = columnDef.field;
            if (field.indexOf(".") != -1)
                field = field.split('.')[0];

            field = field.substring(0, 1).toUpperCase() + field.substring(1, field.length);
            if (!dataContext.fieldPermission[field]) return "###";
        }

        var className = '';
        if (value) className = 'change';

        return '<div class="switch ' + className + '" style="margin-top: 5px !important;width:80%">' +
            '<input type="checkbox" value="' + value + '">' +
            '<p class="slider mb-0"><span class="text text_release">True</span><span class="text text_accept">False</span></p>' +
            '</div >';
    }

    function CheckBoxFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.fieldPermission && columnDef.field) {
            var field = columnDef.field;
            if (field.indexOf(".") != -1)
                field = field.split('.')[0];

            field = field.substring(0, 1).toUpperCase() + field.substring(1, field.length);
            if (!dataContext.fieldPermission[field]) return "###";
        }

        var icon = value == true ? "bowtie-icon bowtie-checkbox" : "bowtie-icon bowtie-checkbox-empty";
        return '<div style="text-align:-webkit-center"> ' +
            '<i class="' + icon + ' check-' + columnDef.field + '" style="cursor:pointer"></i >' +
            '</div> ';
    }

    function SumTotalFormatter(totals, columnDef) {
        var val = totals.sum && totals.sum[columnDef.field];
        if (val != null) {
            return "Total: " + ((Math.round(parseFloat(val) * 100) / 100));
        }
        return "";
    }

    function AvgTotalFormatter(totals, columnDef) {
        var val = totals.avg && totals.avg[columnDef.field];
        if (val != null) {
            return "Avg: " +  accounting.formatNumber(val);
        }
        return "";
    }

})(jQuery);
