declare module pagedRecordsetData {
    /**
     * Handles an action for a data row. You can have one or more action handlers.
     */
    interface actionHandler {
        /**
         * REQUIRED: The function that will be invoked.
         */
        callbackHandler: Function;
        /**
         * OPTIONAL: Any text or HTML element to use for the action handler. This can be empty if you are only
         * using a single handler. In that case the entire data row will invoke the callback handler. If you
         * have more than one action handler then each must include the actionElement as those items will be
         * rendered in the first columns of the table view.
         */
        actionElement?: string;
        /**
         * OPTIONAL: If you only want this handler to be visible if the data row contains a value for a given
         * field then enter that data element name here.
         */
        showIfElementHasValue?: string;
        /**
         * OPTIONAL: A column title option
         */
        columnTitle?: string;
        /**
         * OPTIONAL: If using a column title this is the optional alignment (left, center, or right)
         */
        columnTitleAlign?: string;
        /**
         * OPTIONAL: When clicking on the column title an optional sort field.
         */
        dataElementName?: string;
    }

    /**
     * The configuration for rendering the paged recordset table.
     */
    interface configuration {
        /**
         * REQUIRED: The unique ID of the element where the paged-recordset view will be rendered.
         */
        elementId: string;
        /**
         * REQUIRED: A Filter object that contains your data and record information.
         */
        data: pagedRecordsetData.filter;
        /**
         * REQUIRED: The function that will be invoked when for callbacks.
         * Will be passed the a parameter of type ("count", "page", "sort"; as well as the appropriate related object)
         */
        recordsetCallbackHandler: Function;
        /**
         * OPTIONAL: One or more action handlers (eg: edit, view, etc.). If only one actionHandler is specified and no actionElement is specified then the entire
         * table row will be used to invoke the action. Otherwise, these items will be rendered in the first columns of the data table.
         */
        actionHandlers?: pagedRecordsetData.actionHandler[];
        /**
         * OPTIONAL: An array of extra data values to show as full rows for a given record.
         */
        extraRowData?: pagedRecordsetData.FilterReportRowExtraData[];
        /**
         * OPTIONAL: An optional value (top, bottom, both) to indicate where the record navigation should be displayed. Defaults to "top".
         */
        recordNavigation?: string;
        /**
         * OPTIONAL: An optional URL to use as the base image URL for photo types.
         * The field value will be appended to the end unless a "{0}" item exists, in which case it will be place where that element exists.
         */
        photoBaseUrl?: string;
        /**
         * OPTIONAL: An optional icon to use for boolean items. If not specified then "true" will be shown for true boolean items.
         */
        booleanIcon?: string;
        /**
         * OPTIONAL: If true a column of checkboxes will be added to allow for record selection with a checkbox in the header row to toggle the state of all the checkboxes.
         */
        includeCheckboxes?: boolean;
        /**
         * OPTIONAL: A callback handler that will receive in array of numbers representing the indexes of the record rows that are checked.
         * This will be called every time the state of one or more checkboxes changes.
         */
        checkboxCallbackHandler?: Function;
        /**
         * OPTIONAL: Any CSS class that should be added to the row using the same index as the row.
         */
        rowClasses?: string[];
    }

    interface filter {
        tenantId: string;
        loading: boolean;
        showFilters: boolean;
        executionTime: number;
        start: Date;
        end: Date;
        keyword: string;
        sort: string;
        sortOrder: string;
        recordsPerPage: number;
        pageCount: number;
        recordCount: number;
        page: number;
        tenants: string[];
        columns: column[];
        records: any[];
    }

    interface column {
        align: string;
        label: string;
        tipText: string;
        dataElementName: string;
        dataType: string;
        sortable: boolean;
    }

    interface FilterReportRowExtraData {
        row: number;
        data: FilterReportRowExtraDataValues[];
    }

    interface FilterReportRowExtraDataValues {
        label: string;
        value: string;
    }
}

class pagedRecordsetFilterColumn {
    align: KnockoutObservable<string> = ko.observable("");
    label: KnockoutObservable<string> = ko.observable("");
    tipText: KnockoutObservable<string> = ko.observable("");
    dataElementName: KnockoutObservable<string> = ko.observable("");
    dataType: KnockoutObservable<string> = ko.observable("");
    sortable: KnockoutObservable<boolean> = ko.observable(false);

    Load(data: pagedRecordsetData.column) {
        if (data != null) {
            this.align(data.align);
            this.label(data.label);
            this.tipText(data.tipText);
            this.dataElementName(data.dataElementName);
            this.dataType(data.dataType);
            this.sortable(data.sortable);
        } else {
            this.align("");
            this.label("");
            this.tipText("");
            this.dataElementName("");
            this.dataType("");
            this.sortable(false);
        }
    }
}


namespace pagedRecordset {
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
    let config: pagedRecordsetData.configuration = null;
    let reloadingCheckboxes: boolean = false;

    export function CheckItems(indexes: number[]): void {
        reloadingCheckboxes = true;

        $(".check-record").prop("checked", false);

        if (indexes != null && indexes.length > 0) {
            indexes.forEach(function (i) {
                $(".form-check-input-" + i.toString()).prop("checked", true);
            });
        }

        reloadingCheckboxes = false;
    }

    export function Render(
        configuration: pagedRecordsetData.configuration): void {

        let output: string = "";
        let navigation: string = "";
        let recordTable: string = "";

        let useActionHandler: boolean = false; // configuration.actionCallbackHandler != undefined;
        let showActionButtons: number = 0;

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

        let columns: pagedRecordsetFilterColumn[] = [];
        if (configuration.data != null && configuration.data.columns != null) {
            configuration.data.columns.forEach(function (e) {
                let item: pagedRecordsetFilterColumn = new pagedRecordsetFilterColumn();
                item.Load(e);
                columns.push(item);
            });
        }

        if (configuration.data != null) {
            if (configuration.data.records != null && configuration.data.records.length > 0) {
                let PageCount: number = configuration.data.pageCount;
                let CurrentPage: number = configuration.data.page;
                let RecordsPerPage: number = configuration.data.recordsPerPage;
                let Records: number = configuration.data.records.length;
                let RecordCount: number = configuration.data.recordCount;

                // First, draw the record navigation table.
                let startCount: number = 1;
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
                    let StartPage: number = 1;
                    let EndPage: number = 1;
                    if (PageCount < 6) {
                        EndPage = PageCount;
                    } else {
                        if (CurrentPage > 2) {
                            StartPage = CurrentPage - 2;
                            EndPage = CurrentPage + 2;
                        } else {
                            EndPage = 5;
                        }

                        if (StartPage < 1) {
                            StartPage = 1;
                        }
                        if (EndPage > PageCount) {
                            let HowManyMore: number = EndPage - PageCount;
                            EndPage = EndPage - HowManyMore;
                            StartPage = StartPage - HowManyMore;
                            if (StartPage < 1) {
                                StartPage = 1;
                            }
                        }

                    }

                    navigation += "<div class='btn-group btn-group-sm' role='group'>\n";
                    // First, draw the First and Previous buttons
                    let Disabled: string = CurrentPage == 1 ? " disabled" : "";

                    navigation += "  <button class='btn btn-default btn-record-nav'" + Disabled + " goto-page='1' aria-label='Go to First Page of Records' title='Go to First Page of Records'>" +
                        "<i class='fas fa-backward'></i></button>\n" +
                        "  <button class='btn btn-default btn-record-nav'" + Disabled + " goto-page='" + (CurrentPage - 1).toString() + "' aria-label='Go to Previous Page of Records' title='Go to Previous Page of Records'>" +
                        "<i class='fas fa-fast-backward'></i></button>\n";

                    for (let x: number = StartPage; x <= EndPage; x++) {
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
                                } else {
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

                let index: number = 0;
                configuration.data.columns.forEach(function (c) {
                    recordTable +=
                        "          <th class='" + c.align + "'>\n";

                    if (c.sortable || c.label != "") {
                        recordTable +=
                            "            <button type='button' class='btn btn-xs " +
                            (c.sortable ? configuration.data.sort.toLowerCase() == c.dataElementName.toLowerCase() ? "btn-sortable btn-primary" : "btn-sortable btn-default" : "btn-default") +
                            " nowrap'" +
                            (c.sortable ? "" : " DISABLED") +
                            " sort-element='" + index.toString() + "'" + (HasValue(c.tipText) ? " title='" + c.tipText + "'" : "") + ">";

                        if (configuration.data.sort.toLowerCase() == c.dataElementName.toLowerCase()) {
                            recordTable += "<i class='sort-indicator " + (configuration.data.sortOrder.toUpperCase() == "ASC" ? "fas fa-caret-up sort-arrow" : "fas fa-caret-down sort-arrow") + "'></i>";
                        }
                        recordTable += c.label + "</button>";
                    }

                    recordTable += "          </th>\n";

                    index++;
                });

                recordTable +=
                    "        </tr>\n" +
                    "      </thead>\n" +
                    "      <tbody>\n";

                let hasExtraData: boolean = configuration.extraRowData != undefined && configuration.extraRowData != null && configuration.extraRowData.length > 0;

                index = -1;
                configuration.data.records.forEach(function (r) {
                    index++;
                    let extraData: pagedRecordsetData.FilterReportRowExtraDataValues[] = [];
                    if (hasExtraData) {
                        extraData = configuration.extraRowData[index].data;
                    }
                    let rowHasExtraData: boolean = extraData != undefined && extraData != null && extraData.length > 0;

                    let extraRowClass: string = "";
                    if (configuration.rowClasses != null && configuration.rowClasses.length > (index - 1)) {
                        let rClass: string = configuration.rowClasses[index];
                        if (HasValue(rClass)) {
                            extraRowClass = rClass;
                        }
                    }

                    let rowClass: string = rowHasExtraData ? "" : "sep-row";
                    if (HasValue(extraRowClass)) {
                        if (rowClass != "") { rowClass += " "; }
                        rowClass += extraRowClass;
                    }

                    if (useActionHandler) {

                        if (showActionButtons > 0) {
                            recordTable += "      <tr class='" + rowClass + "'>\n";
                            let buttonIndex: number = -1;

                            configuration.actionHandlers.forEach(function (handler) {
                                if (handler.actionElement != undefined && handler.actionElement != null && handler.actionElement != "") {
                                    buttonIndex++;

                                    let show: boolean = true;

                                    if (HasValue(handler.showIfElementHasValue)) {
                                        let itemValue: string = r[handler.showIfElementHasValue];
                                        show = HasValue(itemValue);
                                    }

                                    if (show) {
                                        let actionElement: string = handler.actionElement;
                                        let iStart: number = actionElement.indexOf("{{");
                                        let iEnd: number = actionElement.indexOf("}}");
                                        if (iStart > -1 && iEnd > -1) {
                                            let itemName: string = actionElement.substring(iStart + 2, iEnd);

                                            if (HasValue(r[itemName])) {
                                                actionElement = actionElement.replace("{{" + itemName + "}}", r[itemName]);
                                            } else {
                                                actionElement = actionElement.replace("{{" + itemName + "}}", "view");
                                            }
                                        }

                                        recordTable +=
                                            "        <td class='no-pad-right action-item action-button' action-index='" + buttonIndex.toString() + "' action-id='" + index.toString() + "'>\n" +
                                            actionElement +
                                            "        </td>\n";
                                    } else {
                                        recordTable += "<td></td>";
                                    }
                                }
                            });
                        } else {
                            recordTable +=
                                "        <tr class='action-row " + rowClass + "' action-id='" + index.toString() + "'>\n";
                        }
                    } else {
                        recordTable += "        <tr class='" + rowClass + "'>\n";
                    }

                    if (configuration.includeCheckboxes) {
                        recordTable += "        <td><input type='checkbox' class='form-check-input form-check-input-" + index.toString() + " check-record' record-id='" + index.toString() + "' /></td>\n";
                    }

                    configuration.data.columns.forEach(function (c) {
                        let value: string = "";

                        switch (c.dataType) {
                            case "datetime":
                                value = FormatDateTime(r[c.dataElementName]);
                                break;

                            case "boolean":
                                if (r[c.dataElementName] == true) {
                                    if (configuration.booleanIcon != undefined && configuration.booleanIcon != null && configuration.booleanIcon != "") {
                                        value += configuration.booleanIcon;
                                    } else {
                                        value += "true";
                                    }
                                } else {
                                    value += "";
                                }
                                break;

                            case "photo":
                                if (configuration.photoBaseUrl != undefined && configuration.photoBaseUrl != null && configuration.photoBaseUrl != "") {
                                    let photo: any = r[c.dataElementName];
                                    if (photo != undefined && photo != null && photo != "") {
                                        let url: string = configuration.photoBaseUrl;
                                        if (url.indexOf("{0}") > -1) {
                                            url = url.replace("{0}", photo);
                                        } else {
                                            url += photo;
                                        }
                                        value = "<img src=\"" + url + "\" class=\"paged-recordset-photo\" />";
                                    }
                                }
                                break;

                            case "listitem":
                                let itemName: string = window.mainModel.ListItemNameFromId(r[c.dataElementName]);
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
                        let tdClass: string = c.align;
                        if (HasValue(tdClass)) {
                            tdClass += " ";
                        }
                        tdClass += c.dataElementName.toLowerCase();

                        recordTable += "          <td class='" + tdClass + "'>" + value + "</td>\n";
                    });

                    recordTable += "        </tr>\n";

                    if (rowHasExtraData) {
                        let counter: number = 0;
                        extraData.forEach(function (row) {
                            counter++;
                            rowClass = counter == extraData.length ? "sep-row" : "";
                            if (HasValue(extraRowClass)) {
                                if (rowClass != "") { rowClass += " "; }
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
            } else {
                recordTable = "There are no records to display.";
            }
        } else {
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
            let v: string = $(this).val();
            if (HasValue(v)) {
                configuration.recordsetCallbackHandler("count", v);
            }
        });

        //$(".records-per-page").on("change", () => {
        //    let v: string = $("#records-per-page").val();
        //    callbackHandler("count", v);
        //});

        $(".btn-record-nav").off("click");
        $(".btn-record-nav").on("click", (e) => {
            let p: string = e.target.getAttribute("goto-page");
            if (p == undefined) {
                p = e.target.parentElement.getAttribute("goto-page");
            }
            if (HasValue(p)) {
                let v: number = parseInt(p);
                configuration.recordsetCallbackHandler("page", v);
            }
        });

        $(".btn-sortable").off("click");
        $(".btn-sortable").on("click", (e) => {
            let c: string = e.target.getAttribute("sort-element");
            if (c == undefined) {
                c = e.target.parentElement.getAttribute("sort-element");
            }
            if (HasValue(c)) {
                if (HasNumericalValue(c)) {
                    configuration.recordsetCallbackHandler("sort", configuration.data.columns[parseInt(c)].dataElementName);
                } else {
                    configuration.recordsetCallbackHandler("sort", c);
                }

            }
        });

        if (useActionHandler) {
            let actionElement: string = showActionButtons > 0 ? ".action-button" : ".action-row";

            $(actionElement).off("click");
            $(actionElement).on("click", (e) => {
                let actionIdElement: Element = e.target.closest("[action-id]");
                let actionIndexElement: Element = e.target.closest("[action-index]");

                if (actionIdElement != undefined && actionIdElement != null) {
                    let index: number = 0;
                    let actionId: string = actionIdElement.getAttribute("action-id");

                    if (actionIndexElement != undefined && actionIndexElement != null) {
                        let actionIndex: string = actionIndexElement.getAttribute("action-index");
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

    function CallCheckboxCallbackHandler() {
        if (!reloadingCheckboxes) {
            if (config.checkboxCallbackHandler != undefined && config.checkboxCallbackHandler != null && typeof config.checkboxCallbackHandler === 'function') {
                let output: string[] = [];

                $(".check-record:checkbox:checked").each(function () {
                    let index: string = $(this).attr("record-id");
                    if (HasValue(index)) {
                        output.push(index);
                    }
                });

                config.checkboxCallbackHandler(output);
            }
        }
    }

    function FormatDateTime(date: Date): string {
        let output: string = "";
        if (date != undefined && date != null) {
            output = moment(date).format("M/D/YYYY h:mm a")
        }
        return output;
    }

    function HasNumericalValue(check: any): boolean {
        let output: boolean = false;

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

    function HasValue(check: string): boolean {
        return check != undefined && check != null && check != "";
    }
}