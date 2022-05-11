var pagedRecordsetFilterColumn = /** @class */ (function () {
    function pagedRecordsetFilterColumn() {
        this.align = ko.observable("");
        this.label = ko.observable("");
        this.tipText = ko.observable("");
        this.dataElementName = ko.observable("");
        this.dataType = ko.observable("");
        this.sortable = ko.observable(false);
    }
    pagedRecordsetFilterColumn.prototype.Load = function (data) {
        if (data != null) {
            this.align(data.align);
            this.label(data.label);
            this.tipText(data.tipText);
            this.dataElementName(data.dataElementName);
            this.dataType(data.dataType);
            this.sortable(data.sortable);
        }
        else {
            this.align("");
            this.label("");
            this.tipText("");
            this.dataElementName("");
            this.dataType("");
            this.sortable(false);
        }
    };
    return pagedRecordsetFilterColumn;
}());
var pagedRecordset;
(function (pagedRecordset) {
    /**
     * Renders a page-recordset view with record indicators, page selectors, the list of data
     * with an optional callback handler that can be used to perform an action against the data, such as editing a record.
     * If a callback handler is passed and no actionButtonInnerHTML is passed, then the entire row will be highlighted
     * on hover and the pointer will be a cursor indicating an action on the row of data and clicking the row will invoke
     * the callback handler. If the actionButtonInnerHTML is passed then a column will be added to the beginning of the table
     * with a button that invokes the callback handler. In both cases the callback handler will receive a single object which
     * is the record that relates to that row.
     * @param {pagedRecordsetData.configuration} configuration - REQUIRED: The configuration for how to render the recordset.
    */
    var config = null;
    var reloadingCheckboxes = false;
    function CheckItems(indexes) {
        reloadingCheckboxes = true;
        $(".check-record").prop("checked", false);
        if (indexes != null && indexes.length > 0) {
            indexes.forEach(function (i) {
                $(".form-check-input-" + i.toString()).prop("checked", true);
            });
        }
        reloadingCheckboxes = false;
    }
    pagedRecordset.CheckItems = CheckItems;
    function Render(configuration) {
        var output = "";
        var navigation = "";
        var recordTable = "";
        var useActionHandler = false; // configuration.actionCallbackHandler != undefined;
        var showActionButtons = 0;
        config = configuration;
        if (configuration.actionHandlers != null && configuration.actionHandlers.length > 0) {
            useActionHandler = true;
            configuration.actionHandlers.forEach(function (handler) {
                if (handler.actionElement != undefined && handler.actionElement != null && handler.actionElement != "") {
                    showActionButtons++;
                }
            });
        }
        if (!HasValue(configuration.data.sort)) {
            configuration.data.sort = "";
        }
        var columns = [];
        if (configuration.data != null && configuration.data.columns != null) {
            configuration.data.columns.forEach(function (e) {
                var item = new pagedRecordsetFilterColumn();
                item.Load(e);
                columns.push(item);
            });
        }
        if (configuration.data != null) {
            if (configuration.data.records != null && configuration.data.records.length > 0) {
                var PageCount = configuration.data.pageCount;
                var CurrentPage = configuration.data.page;
                var RecordsPerPage = configuration.data.recordsPerPage;
                var Records = configuration.data.records.length;
                var RecordCount = configuration.data.recordCount;
                // First, draw the record navigation table.
                var startCount = 1;
                if (CurrentPage > 1) {
                    startCount += (CurrentPage - 1) * RecordsPerPage;
                }
                navigation +=
                    "<div class='row'>\n" +
                        "  <div id='paged-recordset-left-POSITION' class='col col-sm-4 paged-recordset-left'>\n" +
                        "    Showing " + startCount;
                if (RecordCount > 1) {
                    navigation += " to " + (startCount + Records - 1).toString();
                }
                navigation += " of " + RecordCount.toString() + " item";
                if (RecordCount > 1) {
                    navigation += "s";
                }
                navigation +=
                    "  <select class='form-select fixed-75 records-per-page'>\n" +
                        "    <option value='1'" + (RecordsPerPage == 1 ? " SELECTED" : "") + ">1</option>\n" +
                        "    <option value='5'" + (RecordsPerPage == 5 ? " SELECTED" : "") + ">5</option>\n" +
                        "    <option value='10'" + (RecordsPerPage == 10 ? " SELECTED" : "") + ">10</option>\n" +
                        "    <option value='15'" + (RecordsPerPage == 15 ? " SELECTED" : "") + ">15</option>\n" +
                        "    <option value='20'" + (RecordsPerPage == 20 ? " SELECTED" : "") + ">20</option>\n" +
                        "    <option value='25'" + (RecordsPerPage == 25 ? " SELECTED" : "") + ">25</option>\n" +
                        "    <option value='50'" + (RecordsPerPage == 50 ? " SELECTED" : "") + ">50</option>\n" +
                        "    <option value='100'" + (RecordsPerPage == 100 ? " SELECTED" : "") + ">100</option>\n" +
                        "  </select>\n" +
                        "  </div>\n" +
                        "  <div id='paged-recordset-center-POSITION' class='col col-sm-4 paged-recordset-center'></div>\n" +
                        "  <div id='paged-recordset-right-POSITION' class='col col-sm-4 paged-recordset-right right'>\n";
                if (PageCount > 1) {
                    var StartPage = 1;
                    var EndPage = 1;
                    if (PageCount < 6) {
                        EndPage = PageCount;
                    }
                    else {
                        if (CurrentPage > 2) {
                            StartPage = CurrentPage - 2;
                            EndPage = CurrentPage + 2;
                        }
                        else {
                            EndPage = 5;
                        }
                        if (StartPage < 1) {
                            StartPage = 1;
                        }
                        if (EndPage > PageCount) {
                            var HowManyMore = EndPage - PageCount;
                            EndPage = EndPage - HowManyMore;
                            StartPage = StartPage - HowManyMore;
                            if (StartPage < 1) {
                                StartPage = 1;
                            }
                        }
                    }
                    navigation += "<div class='btn-group btn-group-sm' role='group'>\n";
                    // First, draw the First and Previous buttons
                    var Disabled = CurrentPage == 1 ? " disabled" : "";
                    navigation += "  <button class='btn btn-default btn-record-nav'" + Disabled + " goto-page='1' aria-label='Go to First Page of Records' title='Go to First Page of Records'>" +
                        "<i class='fas fa-backward'></i></button>\n" +
                        "  <button class='btn btn-default btn-record-nav'" + Disabled + " goto-page='" + (CurrentPage - 1).toString() + "' aria-label='Go to Previous Page of Records' title='Go to Previous Page of Records'>" +
                        "<i class='fas fa-fast-backward'></i></button>\n";
                    for (var x = StartPage; x <= EndPage; x++) {
                        Disabled = x == CurrentPage ? " disabled" : "";
                        navigation += "  <button class='btn btn-default btn-record-nav'" + Disabled + " goto-page='" + x.toString() + "' aria-label='Go to Page " + x.toString() + " of Records' title='Go to Page " + x.toString() + " of Records'>" +
                            x.toString() + "</button>\n";
                    }
                    // Finally, draw the Next and Last buttons
                    Disabled = CurrentPage == EndPage ? " disabled" : "";
                    navigation += "  <button class='btn btn-default btn-record-nav'" + Disabled + " goto-page='" + (CurrentPage + 1).toString() + "' aria-label='Go to Next Page of Records' title='Go to Next Page of Records'>" +
                        "<i class='fas fa-forward'></i></button>\n" +
                        "  <button class='btn btn-default btn-record-nav'" + Disabled + " goto-page='" + PageCount.toString() + "' aria=label='Go to Last Page of Records' title='Go to Last Page of Records'>" +
                        "<i class='fas fa-step-forward'></i></button>\n";
                    navigation += "</div>\n";
                }
                navigation +=
                    "  </div>\n" +
                        "</div>\n";
                recordTable =
                    "<div class='row padbottom'>\n" +
                        "  <div class='col col-sm-12'>\n" +
                        "    <table class='table paged-recordset-table'>\n" +
                        "      <thead>\n" +
                        "        <tr class='table-dark'>\n";
                if (showActionButtons > 0) {
                    //    for (let b: number = 0; b < showActionButtons; b++) {
                    //        recordTable += "          <th class='action-item'></th>\n";
                    //    }
                    configuration.actionHandlers.forEach(function (handler) {
                        if (handler.actionElement != undefined && handler.actionElement != null && handler.actionElement != "") {
                            recordTable += "          <th class='action-item" + (HasValue(handler.columnTitleAlign) ? " " + handler.columnTitleAlign : "") + "'>";
                            if (HasValue(handler.columnTitle)) {
                                if (HasValue(handler.dataElementName)) {
                                    recordTable += "<button type='button' class='btn btn-xs " +
                                        (configuration.data.sort.toLowerCase() == handler.dataElementName.toLowerCase() ? "btn-sortable btn-primary" : "btn-sortable btn-default") +
                                        " nowrap' sort-element='" + handler.dataElementName + "'>";
                                    if (configuration.data.sort.toLowerCase() == handler.dataElementName.toLowerCase()) {
                                        recordTable += "<i class='sort-indicator " + (configuration.data.sortOrder.toUpperCase() == "ASC"
                                            ? "fas fa-caret-up sort-arrow" : "fas fa-caret-down sort-arrow") + "'></i>";
                                    }
                                    recordTable += handler.columnTitle + "</button>";
                                }
                                else {
                                    recordTable += "<button type='button' class='btn btn-xs btn-default nowrap' DISABLED>" + handler.columnTitle + "</button>";
                                }
                            }
                            recordTable += "</th>\n";
                        }
                    });
                }
                if (configuration.includeCheckboxes) {
                    recordTable += "          <th class='action-item'><input type='checkbox' class='form-check-input check-all-records' /></th>\n";
                }
                var index_1 = 0;
                configuration.data.columns.forEach(function (c) {
                    recordTable +=
                        "          <th class='" + c.align + "'>\n";
                    if (c.sortable || c.label != "") {
                        recordTable +=
                            "            <button type='button' class='btn btn-xs " +
                                (c.sortable ? configuration.data.sort.toLowerCase() == c.dataElementName.toLowerCase() ? "btn-sortable btn-primary" : "btn-sortable btn-default" : "btn-default") +
                                " nowrap'" +
                                (c.sortable ? "" : " DISABLED") +
                                " sort-element='" + index_1.toString() + "'" + (HasValue(c.tipText) ? " title='" + c.tipText + "'" : "") + ">";
                        if (configuration.data.sort.toLowerCase() == c.dataElementName.toLowerCase()) {
                            recordTable += "<i class='sort-indicator " + (configuration.data.sortOrder.toUpperCase() == "ASC" ? "fas fa-caret-up sort-arrow" : "fas fa-caret-down sort-arrow") + "'></i>";
                        }
                        recordTable += c.label + "</button>";
                    }
                    recordTable += "          </th>\n";
                    index_1++;
                });
                recordTable +=
                    "        </tr>\n" +
                        "      </thead>\n" +
                        "      <tbody>\n";
                var hasExtraData_1 = configuration.extraRowData != undefined && configuration.extraRowData != null && configuration.extraRowData.length > 0;
                index_1 = -1;
                configuration.data.records.forEach(function (r) {
                    index_1++;
                    var extraData = [];
                    if (hasExtraData_1) {
                        extraData = configuration.extraRowData[index_1].data;
                    }
                    var rowHasExtraData = extraData != undefined && extraData != null && extraData.length > 0;
                    var extraRowClass = "";
                    if (configuration.rowClasses != null && configuration.rowClasses.length > (index_1 - 1)) {
                        var rClass = configuration.rowClasses[index_1];
                        if (HasValue(rClass)) {
                            extraRowClass = rClass;
                        }
                    }
                    var rowClass = rowHasExtraData ? "" : "sep-row";
                    if (HasValue(extraRowClass)) {
                        if (rowClass != "") {
                            rowClass += " ";
                        }
                        rowClass += extraRowClass;
                    }
                    if (useActionHandler) {
                        if (showActionButtons > 0) {
                            recordTable += "      <tr class='" + rowClass + "'>\n";
                            var buttonIndex_1 = -1;
                            configuration.actionHandlers.forEach(function (handler) {
                                if (handler.actionElement != undefined && handler.actionElement != null && handler.actionElement != "") {
                                    buttonIndex_1++;
                                    var show = true;
                                    if (HasValue(handler.showIfElementHasValue)) {
                                        var itemValue = r[handler.showIfElementHasValue];
                                        show = HasValue(itemValue);
                                    }
                                    if (show) {
                                        var actionElement = handler.actionElement;
                                        var iStart = actionElement.indexOf("{{");
                                        var iEnd = actionElement.indexOf("}}");
                                        if (iStart > -1 && iEnd > -1) {
                                            var itemName = actionElement.substring(iStart + 2, iEnd);
                                            if (HasValue(r[itemName])) {
                                                actionElement = actionElement.replace("{{" + itemName + "}}", r[itemName]);
                                            }
                                            else {
                                                actionElement = actionElement.replace("{{" + itemName + "}}", "view");
                                            }
                                        }
                                        recordTable +=
                                            "        <td class='no-pad-right action-item action-button' action-index='" + buttonIndex_1.toString() + "' action-id='" + index_1.toString() + "'>\n" +
                                                actionElement +
                                                "        </td>\n";
                                    }
                                    else {
                                        recordTable += "<td></td>";
                                    }
                                }
                            });
                        }
                        else {
                            recordTable +=
                                "        <tr class='action-row " + rowClass + "' action-id='" + index_1.toString() + "'>\n";
                        }
                    }
                    else {
                        recordTable += "        <tr class='" + rowClass + "'>\n";
                    }
                    if (configuration.includeCheckboxes) {
                        recordTable += "        <td><input type='checkbox' class='form-check-input form-check-input-" + index_1.toString() + " check-record' record-id='" + index_1.toString() + "' /></td>\n";
                    }
                    configuration.data.columns.forEach(function (c) {
                        var value = "";
                        switch (c.dataType) {
                            case "datetime":
                                value = FormatDateTime(r[c.dataElementName]);
                                break;
                            case "boolean":
                                if (r[c.dataElementName] == true) {
                                    if (configuration.booleanIcon != undefined && configuration.booleanIcon != null && configuration.booleanIcon != "") {
                                        value += configuration.booleanIcon;
                                    }
                                    else {
                                        value += "true";
                                    }
                                }
                                else {
                                    value += "";
                                }
                                break;
                            case "photo":
                                if (configuration.photoBaseUrl != undefined && configuration.photoBaseUrl != null && configuration.photoBaseUrl != "") {
                                    var photo = r[c.dataElementName];
                                    if (photo != undefined && photo != null && photo != "") {
                                        var url = configuration.photoBaseUrl;
                                        if (url.indexOf("{0}") > -1) {
                                            url = url.replace("{0}", photo);
                                        }
                                        else {
                                            url += photo;
                                        }
                                        value = "<img src=\"" + url + "\" class=\"paged-recordset-photo\" />";
                                    }
                                }
                                break;
                            case "listitem":
                                var itemName = window.mainModel.ListItemNameFromId(r[c.dataElementName]);
                                if (itemName != "") {
                                    value = itemName;
                                }
                                break;
                            default:
                                if (HasValue(r[c.dataElementName])) {
                                    value += r[c.dataElementName];
                                }
                                break;
                        }
                        var tdClass = c.align;
                        if (HasValue(tdClass)) {
                            tdClass += " ";
                        }
                        tdClass += c.dataElementName.toLowerCase();
                        recordTable += "          <td class='" + tdClass + "'>" + value + "</td>\n";
                    });
                    recordTable += "        </tr>\n";
                    if (rowHasExtraData) {
                        var counter_1 = 0;
                        extraData.forEach(function (row) {
                            counter_1++;
                            rowClass = counter_1 == extraData.length ? "sep-row" : "";
                            if (HasValue(extraRowClass)) {
                                if (rowClass != "") {
                                    rowClass += " ";
                                }
                                rowClass += extraRowClass;
                            }
                            recordTable += "        <tr" + (rowClass != "" ? " class='" + rowClass + "'" : "") + ">\n";
                            if (useActionHandler && showActionButtons > 0) {
                                configuration.actionHandlers.forEach(function (handler) {
                                    if (handler.actionElement != undefined && handler.actionElement != null && handler.actionElement != "") {
                                        recordTable += "          <td></td>\n";
                                    }
                                });
                            }
                            recordTable +=
                                "          <td class='extra-data-label'>" + row.label + "</td>\n" +
                                    "          <td colspan='" + (configuration.data.columns.length) + "'>" + row.value + "</td>\n" +
                                    "        </tr>\n";
                        });
                    }
                });
                recordTable +=
                    "      </tbody>\n" +
                        "    </table>\n" +
                        "  </div>\n" +
                        "</div>\n";
            }
            else {
                recordTable = "There are no records to display.";
            }
        }
        else {
            recordTable = "There are no records to display.";
        }
        if (!HasValue(configuration.recordNavigation)) {
            configuration.recordNavigation = "top";
        }
        switch (configuration.recordNavigation.toLowerCase()) {
            case "bottom":
                output = recordTable + navigation.replace(/-POSITION/g, '-bottom');
                break;
            case "both":
                output = "<div class='padtop-10 padbottom-10'>" + navigation.replace(/-POSITION/g, '-top') + "</div>" + recordTable + navigation.replace(/-POSITION/g, '-bottom');
                break;
            default:
                output = "<div class='padtop-10 padbottom-10'>" + navigation.replace(/-POSITION/g, '-top') + "</div>" + recordTable;
                break;
        }
        $("#" + configuration.elementId).html(output);
        $(".records-per-page").off("change");
        $(".records-per-page").on("change", function () {
            var v = $(this).val();
            if (HasValue(v)) {
                configuration.recordsetCallbackHandler("count", v);
            }
        });
        //$(".records-per-page").on("change", () => {
        //    let v: string = $("#records-per-page").val();
        //    callbackHandler("count", v);
        //});
        $(".btn-record-nav").off("click");
        $(".btn-record-nav").on("click", function (e) {
            var p = e.target.getAttribute("goto-page");
            if (p == undefined) {
                p = e.target.parentElement.getAttribute("goto-page");
            }
            if (HasValue(p)) {
                var v = parseInt(p);
                configuration.recordsetCallbackHandler("page", v);
            }
        });
        $(".btn-sortable").off("click");
        $(".btn-sortable").on("click", function (e) {
            var c = e.target.getAttribute("sort-element");
            if (c == undefined) {
                c = e.target.parentElement.getAttribute("sort-element");
            }
            if (HasValue(c)) {
                if (HasNumericalValue(c)) {
                    configuration.recordsetCallbackHandler("sort", configuration.data.columns[parseInt(c)].dataElementName);
                }
                else {
                    configuration.recordsetCallbackHandler("sort", c);
                }
            }
        });
        if (useActionHandler) {
            var actionElement = showActionButtons > 0 ? ".action-button" : ".action-row";
            $(actionElement).off("click");
            $(actionElement).on("click", function (e) {
                var actionIdElement = e.target.closest("[action-id]");
                var actionIndexElement = e.target.closest("[action-index]");
                if (actionIdElement != undefined && actionIdElement != null) {
                    var index = 0;
                    var actionId = actionIdElement.getAttribute("action-id");
                    if (actionIndexElement != undefined && actionIndexElement != null) {
                        var actionIndex = actionIndexElement.getAttribute("action-index");
                        if (HasNumericalValue(actionIndex)) {
                            index = parseInt(actionIndex);
                        }
                    }
                    //console.log("action-id", actionId, "index", index);
                    if (HasValue(actionId)) {
                        configuration.actionHandlers[index].callbackHandler(configuration.data.records[parseInt(actionId)]);
                    }
                }
            });
        }
        if (configuration.includeCheckboxes) {
            $(".check-all-records").off("click");
            $(".check-all-records").on("click", function () {
                $(".check-record").each(function () {
                    $(this).prop("checked", !$(this).prop("checked"));
                });
                CallCheckboxCallbackHandler();
            });
            $(".check-record").off("click");
            $(".check-record").on("click", function () {
                CallCheckboxCallbackHandler();
            });
        }
    }
    pagedRecordset.Render = Render;
    function CallCheckboxCallbackHandler() {
        if (!reloadingCheckboxes) {
            if (config.checkboxCallbackHandler != undefined && config.checkboxCallbackHandler != null && typeof config.checkboxCallbackHandler === 'function') {
                var output_1 = [];
                $(".check-record:checkbox:checked").each(function () {
                    var index = $(this).attr("record-id");
                    if (HasValue(index)) {
                        output_1.push(index);
                    }
                });
                config.checkboxCallbackHandler(output_1);
            }
        }
    }
    function FormatDateTime(date) {
        var output = "";
        if (date != undefined && date != null) {
            output = moment(date).format("M/D/YYYY h:mm a");
        }
        return output;
    }
    function HasNumericalValue(check) {
        var output = false;
        if (typeof check == 'string') {
            if (!HasValue(check)) {
                // An empty string was passed in, so return false. Otherwise, this will evaluate as true in the next check below.
                return false;
            }
        }
        if (check != undefined && check != null && check != "") {
            output = !isNaN(check);
        }
        return output;
    }
    function HasValue(check) {
        return check != undefined && check != null && check != "";
    }
})(pagedRecordset || (pagedRecordset = {}));
//# sourceMappingURL=pagedRecordset.js.map