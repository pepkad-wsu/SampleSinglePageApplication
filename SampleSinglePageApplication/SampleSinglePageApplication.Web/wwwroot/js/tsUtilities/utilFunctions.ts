interface Window {
    //BaseUrl: string;
    //RootUrl: string;
    //Title: string;
    Token: string;
    //GuidEmpty: string;
    //viewModel: any;
    tsUtilsAjaxHeaderValues: any;
}

interface htmlEditorConfiguration {
    html?: string;
    onupdate?: Function;
    onmodechange?: Function;
    simpleView?: boolean;
    allowImageUploads?: boolean;
    hideSourceButton?: boolean;
    required?: boolean;
    placeholderText?: string;
}

declare namespace CKEDITOR {
    const config: any;
    const instances: any;
    const replace: any;
}

//#region GlobalFunctions
namespace tsUtilities {

    /**
     * Adds a given number of days to a date and returns the new date.
     * @param {Date} date - The date to modify.
     * @param {number} days - The number of days to add.
     * @returns {Date} - Returns a date with the specified number of days added (or subtracted if the days were negative.)
     */
    export function AddDays(date: Date, days: number): Date {
        let result: Date = new Date(date);
        result.setDate(date.getDate() + days);
        return result;
    }

    /**
     * Adds a given number of minutes to a date and returns the new date.
     * @param {Date} date - The date to modify.
     * @param {number} minutes - The number of minutes to add.
     * @returns {Date} - Returns a date with the specified number of minutes added (or subtracted if the minutes were negative.)
     */
    export function AddMinutes(date: Date, minutes: number): Date {
        let result: Date = new Date();
        result.setTime(date.getTime() + (minutes * 60000));
        return result;
    }

    /**
     * Makes an AJAX call to the specified endpoint. If PostData is provided a POST request will be made, otherwise, a GET request is made.
     * @param {string} Endpoint - The full URL endpoint.
     * @param {any} [PostData] - An optional data object to post to the endpoint.
     * @param {Function} [callbackHandler] - An optional function to handle the callback. Any data returned will be passed to the callback handler.
     * @param {Function} [errorHandler] - An optional function to handle the callback when errors are encountered.
     * @param {Function} [completeHandler] - An optional function to be called on complete, whether there is a success or failure.
     */
    export function AjaxData(Endpoint: string, PostData?: any, callbackHandler?: Function, errorHandler?: Function, completeHandler?: Function) {
        if (PostData != undefined && PostData != null) {
            $.ajax({
                url: Endpoint,
                type: 'POST',
                data: PostData,
                beforeSend: SetRequestHeaders,
                contentType: "application/json",
                success: function (data: any) {
                    if (callbackHandler != undefined && callbackHandler != null) {
                        callbackHandler(data);
                    }
                },
                error: function (xhr: JQueryXHR, textStatus: string, errorThrown: string) {
                    if (errorHandler != undefined && errorHandler != null) {
                        errorHandler(xhr, textStatus, errorThrown);
                    } else {
                        console.debug("jQuery AJAX Error", xhr);
                    }
                },
                complete: function (xhr: JQueryXHR, data: any) {
                    if (completeHandler != undefined && completeHandler != null) {
                        completeHandler(data);
                    }
                }
            });
        } else {
            $.ajax({
                url: Endpoint,
                type: 'GET',
                beforeSend: SetRequestHeaders,
                contentType: "application/json; charset=utf-8",
                success: function (data: any) {
                    if (callbackHandler != undefined && callbackHandler != null) {
                        callbackHandler(data);
                    }
                },
                error: function (xhr: JQueryXHR, textStatus: string, errorThrown: string) {
                    if (errorHandler != undefined && errorHandler != null) {
                        errorHandler(xhr, textStatus, errorThrown);
                    } else {
                        console.debug("jQuery AJAX Error", xhr);
                    }
                },
                complete: function (data: any) {
                    if (completeHandler != undefined && completeHandler != null) {
                        completeHandler(data);
                    }
                }
            });
        }
    }

    /**
     * Appends text to a CSV string.
     * @param {string} currentText - The current CSV text to append the new text to.
     * @param {string} appendText - The new text to be appended.
     * @returns {string} Returns the previous text, plus a comma, then the new text, or just the new text if there was no previous text.
     */
    export function AppendTextToCSV(currentText: string, appendText: string): string {
        let output: string = currentText;
        if (appendText != undefined && appendText != null && appendText != "") {
            if (output != undefined && output != null && output != "") {
                output += ",";
            }
            output += appendText;
        }
        return output;
    }

    /**
     * Converts a text input element into a jQuery autocomplete.
     * @param {string} element - The ID of the input element.
     * @param {string} Endpoint - The full URL to the lookup endpoint.
     * @param {Function} itemSelectedCallback - The function that will handle the response when an item is selected.
     */
    export function AutoComplete(element: string, Endpoint: string, itemSelectedCallback: Function): void {
        // Make a call to the AjaxData function to get the data from the endpoint and return the results as
        // an array of server.AjaxResults items
        let responseHandler: Function = (req: any, callbackHandler: Function) => {
            let search: tsutilsAjaxLookup = new tsutilsAjaxLookup();
            search.Search(req.term);

            AjaxData(Endpoint, search, (data: any) => {
                callbackHandler(data);
            });
        };

        // Setup a jQuery autocomplete for the specified element
        $("#" + element).autocomplete({
            source: responseHandler,
            minLength: 1,
            select: function (event, ui) {
                // When an items is selected clear the element value and pass the selected item
                // to the callback handler.
                event.preventDefault();
                $("#" + element).val("");
                itemSelectedCallback(ui)
            },
            focus: function (event, ui) {
                // When an item in the autocomplete list has focus update the element value
                // to an empty string
                event.preventDefault();
                $("#" + element).val("");
            }
        });
    }

    /**
     * Takes a boolean value and if the value is true then returns the HTML of an <i> element with a glypicons checkbox.
     * @param {boolean} value - The boolean value to evaluate.
     * @param {string} [iconTrue] - An optional icon class to use when the value is true instead of the default of "fas fa-check"
     * @param {string} [iconFalse] - An optional icon class to use when the value is false instead of the default of no icon.
     * @returns {string} - Returns either an empty string if false or a string with a icon in an <i> element if true.
     */
    export function BooleanToCheckbox(value: boolean, iconTrue?: string, iconFalse?: string): string {
        let output: string = "";
        if (!HasValue(iconTrue)) {
            iconTrue = "far fa-check-square";
        }
        if (!HasValue(iconFalse)) {
            iconFalse = "";
        }

        if (value != undefined && value != null && value == true) {
            output = iconTrue.indexOf("<") > -1 ? iconTrue : "<i class=\"" + iconTrue + "\"></i>";
        } else {
            output = iconFalse.indexOf("<") > -1 ? iconFalse : "<i class=\"" + iconFalse + "\"></i>";
        }

        return output;
    }

    /**
     * Converts a file length in bytes to a friendly display.
     * @param {number} bytes - The length of the content in bytes.
     * @param {string[]} [labels] - An optional array to use for the labels to override the defaults of ['b','k','m','g'].
     * @returns {string} - Returns a string formatting the bytes to a friendly view depending on file size.
     */
    export function BytesToFileSizeLabel(bytes: number, labels?: string[]): string {
        let output: string = "";
        if (bytes != undefined && bytes != null && bytes > 0) {
            if (labels == undefined || labels == null || labels.length < 4) {
                labels = ["bytes", "kb", "meg", "GB"];
            }

            if (bytes < 1024) {
                output = bytes.toFixed(0) + labels[0];
            } else if (bytes < (1024 * 1024)) {
                output = (bytes / 1024).toFixed(0) + labels[1];
            } else if (bytes < (1024 * 1024 * 1024)) {
                output = (bytes / 1024 / 1024).toFixed(1) + labels[2];
            } else if (bytes < (1024 * 1024 * 1024 * 1024)) {
                output = (bytes / 1024 / 1024 / 1024).toFixed(1) + labels[3];
            }
        }
        return output;
    }

    /**
     * Displays a confirmation message at the top of a Modal dialog with more controls over the buttons..
     * @param message {string} - The message to be displayed.
     * @param error {boolean} - Optional flag to indicate if the dialog should be styled as an error instead of an informational message.
     * @param {tsutilsDTO.tsutilsConfirmButton[]} buttons - An optional array of buttons to be shown on the dialog. If none are specified only a close button is shown.
     */
    export function Confirmation(message: string, error: boolean, buttons: tsutilsDTO.tsutilsConfirmButton[]): void {
        if (buttons == undefined || buttons == null) {
            return;
        }

        let msg: string =
            "<div class='tsutils-padbottom'>\n" + message + "\n</div>\n" +
            "<div class='btn-group btn-group-sm' role='group'>\n";

        // Generate any buttons
        buttons.forEach(function (button) {
            if (!HasValue(button.Id)) {
                button.Id = GenerateGuid();
            }

            let content: string = "";

            let icon: string = DefaultButtonIcon(button.Text, button.Icon);
            if (HasValue(icon)) {
                content += "<i class='" + icon + "'></i>";
            }
            if (HasValue(button.Text)) {
                if (content != "") { content += " "; }
                content += button.Text;
            }

            // Only include buttons that had either text or an icon, no empty buttons allowed
            if (HasValue(content)) {
                msg += "<button type='button' class='" + (HasValue(button.Class) ? button.Class : "btn btn-secondary") + "' id='" + button.Id + "'>" + content + "</button>\n";
            }
        });
        msg += "</div>";

        if (error == true) {
            ModalError(msg, false);
        } else {
            ModalMessage(msg, false);
        }

        // Add the button callback handlers
        buttons.forEach(function (button) {
            if (HasValue(button.Icon + button.Text)) {
                $("#" + button.Id).off('click');

                let closeDialog: boolean = true;
                if (button.ClosesConfirmation != undefined && button.ClosesConfirmation != null && button.ClosesConfirmation == false) {
                    closeDialog = false;
                }

                if (typeof button.CallbackHandler == "function") {
                    if (closeDialog) {
                        $("#" + button.Id).on('click', () => { ModalDialogClearMessages(); button.CallbackHandler(); });
                    } else {
                        $("#" + button.Id).on('click', () => { button.CallbackHandler() });
                    }
                } else if (closeDialog) {
                    $("#" + button.Id).on('click', () => { ModalDialogClearMessages(); });
                }
            }
        });
    }

    /**
     * Displays a confirmation message at the top of a Modal dialog.
     * @param message {string} - The message to be displayed.
     * @param okButtonText {string} - Optional text for the OK button. If set to "hidden" the button will not be shown.
     * @param okButtonIcon {string} - Optional class to add to the OK button to show in an I element as an icon (i.e. "fas fa-check"). Set to "auto" to attempt to select an icon automatically based on the button text.
     * @param okButtonClass {string} - Optional button class to be used on the OK button (i.e. "btn btn-success").
     * @param okHandler {Function} - Optional callback function to be called when the OK button is clicked.
     * @param cancelButtonText {string} - Optional text for the Cancel button. If set to "hidden" the button will not be shown.
     * @param cancelButtonIcon {string} - Optional class to add to the Cancel button to show in an I element as an icon (i.e. "fas fa-check"). Set to "auto" to attempt to select an icon automatically based on the button text.
     * @param cancelButtonClass {string} - Optional button class to be used on the Cancel button (i.e. "btn btn-default").
     * @param cancelHandler {Function} - Optional callback function to be called when the Cancel button is clicked.
     * @param error {boolean} - Optional flag to indicate if the dialog should be styled as an error instead of an informational message.
     */
    export function ConfirmationMessage(message: string,
        okButtonText?: string, okButtonIcon?: string, okButtonClass?: string, okHandler?: Function,
        cancelButtonText?: string, cancelButtonIcon?: string, cancelButtonClass?: string, cancelHandler?: Function,
        error?: boolean) {

        if (error == undefined || error == null) { error = false; }

        if (!HasValue(okButtonText)) { okButtonText = "OK"; }
        if (!HasValue(okButtonClass)) { okButtonClass = error == true ? "btn btn-danger" : "btn btn-success"; }
        if (!HasValue(okButtonIcon)) {
            okButtonIcon = "fas fa-check";
        } else {
            okButtonIcon = DefaultButtonIcon(okButtonText, okButtonIcon);
        }

        if (!HasValue(cancelButtonText)) { cancelButtonText = "Cancel"; }
        if (!HasValue(cancelButtonClass)) { cancelButtonClass = "btn btn-secondary btn-default"; }
        if (!HasValue(cancelButtonIcon)) {
            cancelButtonIcon = "fas fa-times";
        } else {
            cancelButtonIcon = DefaultButtonIcon(cancelButtonText, cancelButtonIcon);
        }

        let msg: string =
            "<div class='tsutils-padbottom'>\n" + message + "\n</div>\n" +
            "<div class='btn-group btn-group-sm' role='group'>\n";

        if (okButtonText.toLowerCase() != "hidden") {
            msg +=
                "  <button type='button' class='" + okButtonClass + "' id='tsutils-confirmation-message-ok'>\n" +
                "    <i class='" + okButtonIcon + "'></i> " + okButtonText + "\n" +
                "  </button>\n";
        }
        if (cancelButtonText.toLowerCase() != "hidden") {
            msg +=
                "  <button type='button' class='" + cancelButtonClass + "' id='tsutils-confirmation-message-cancel'>\n" +
                "    <i class='" + cancelButtonIcon + "'></i> " + cancelButtonText + "\n" +
                "  </button>\n";
        }

        msg += "</div>";

        ModalDialogClearMessages();
        if (error == true) {
            ModalError(msg, false);
        } else {
            ModalMessage(msg, false);
        }

        $("#tsutils-confirmation-message-ok").off('click');
        $("#tsutils-confirmation-message-ok").click(() => ModalDialogClearMessages());
        if (okHandler != undefined && okHandler != null) {
            $("#tsutils-confirmation-message-ok").click(() => okHandler());
        }

        $("#tsutils-confirmation-message-cancel").off('click');
        $("#tsutils-confirmation-message-cancel").click(() => ModalDialogClearMessages());
        if (cancelHandler != undefined && cancelHandler != null) {
            $("#tsutils-confirmation-message-cancel").click(() => cancelHandler());
        }
    }

    /**
     * Reads a client cookie and returns the value as a string.
     * @param {string} name - The name of the cookie to read.
     * @returns {string} Returns the value stored in the cookie, or an empty string if there was no value.
     */
    export function CookieRead(name: string): string {
        let nameEQ: string = name + "=";
        let cookies: string[] = document.cookie.split(';');
        for (let i: number = 0; i < cookies.length; i++) {
            let cookie: string = cookies[i];
            while (cookie.charAt(0) == ' ') {
                cookie = cookie.substring(1, cookie.length);
            }
            if (cookie.indexOf(nameEQ) == 0) {
                return cookie.substring(nameEQ.length, cookie.length);
            }
        }
        return "";
    }

    /**
     * Writes a cookie to the client's browser.
     * @param {string} name - The name of the cookie.
     * @param {string} value - The value to store in the cookie.
     * @param {number} [days=30] An optional number of days until the cookie expires. If left empty this defaults to 30 days.
     * @param {string} domain An options cookie domain
     */
    export function CookieWrite(name: string, value: string, days?: number, domain?: string) {
        if (days == undefined || days == null) { days = 30; }
        let date: Date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        let expires: string = "; expires=" + date.toUTCString();
        let setDomain: string = "";

        if (HasValue(domain)) {
            setDomain = "; domain=" + domain;
        }

        document.cookie = name + "=" + value + expires + setDomain + "; path=/";
    }

    /**
    * Function used internally to create the tsutilsModalDialog if it has not been added to the page.
    */
    function CreateModalDialogIfMissing(): void {
        // Make sure the modal dialog element exists
        let exists: boolean = $("#tsutilsModalDialog").length > 0;
        if (!exists) {
            // Missing the modal dialog, so create it
            let dialog: string =
                '<div class="modal-dialog" id="tsutilsModalDialogBox">' +
                '    <!-- Modal content-->' +
                '    <div class="modal-content">' +
                '        <div class="modal-header">' +
                '            <h4 class="modal-title" id="tsutilsModalDialogTitle" style="width:100%;"></h4>' +
                '        </div>' +
                '        <div class="modal-body">' +
                '            <div id="tsutilsModalDialogMessage" class="alert alert-success" style="display:none;"></div>' +
                '            <div id="tsutilsModalDialogError" class="alert alert-danger" style="display:none;"></div>' +
                '            <div id="tsutilsModalDialogBody"></div>' +
                '        </div>' +
                '        <div class="modal-footer" id="tsutilsModalDialogFooter">' +
                '            <button type="button" class="btn btn-success" id="tsutilsModalDialogOkButton">' +
                '                <i class="" id="tsutilsModalDialogOkButtonIcon"></i>' +
                '                <span id="tsutilsModalDialogOkButtonText">OK</span>' +
                '            </button>' +
                '            <span id="tsutilsModalExtraButtons" style="display:none;"></span>' +
                '            <button type="button" class="btn btn-secondary btn-default" id="tsutilsModalDialogCancelButton">' +
                '                <i class="fas fa-times"></i>' +
                '                Cancel' +
                '            </button>' +
                '        </div>' +
                '    </div>' +
                '</div>';

            let dialogContent: HTMLDivElement = document.createElement("div");
            dialogContent.id = "tsutilsModalDialog";
            dialogContent.className = "modal";
            dialogContent.setAttribute("role", "dialog");
            dialogContent.setAttribute("data-backdrop", "static");
            dialogContent.setAttribute("data-keyboard", "false");
            document.body.appendChild(dialogContent);
            document.getElementById("tsutilsModalDialog").innerHTML = dialog;

            SetupScrolling();
        }
    }

    export function DaysBetween(date1: Date, date2: Date): number {
        let output: number = 0;

        if (date1 != null && date2 != null) {
            let oneDay: number = 1000 * 60 * 60 * 24;

            // Always just check for the day, not the hour.
            let d1: Date = new Date(date1);
            let d2: Date = new Date(date2);

            let compareDate1: Date = new Date(d1.getFullYear(), d1.getMonth(), d1.getDate());
            let compareDate2: Date = new Date(d2.getFullYear(), d2.getMonth(), d2.getDate());

            output = Math.ceil((compareDate1.getTime() - compareDate2.getTime()) / oneDay);
        }

        return output;
    }

    /**
     * Function used internally to try and find a button icon for buttons with the class set to "auto"
     * @param {string} ButtonText - The text of the button
     * @param {string} CurrentIcon - The current icon value
     */
    function DefaultButtonIcon(ButtonText?: string, CurrentIcon?: string): string {
        let output: string = "";
        let auto: boolean = false;

        if (HasValue(CurrentIcon)) {
            if (CurrentIcon.toLowerCase() == "auto") {
                auto = true;
            }
        }

        if (HasValue(ButtonText) && auto) {
            switch (ButtonText.toUpperCase()) {
                case "ADD COMMENT":
                case "CREATE COMMENT":
                case "NEW COMMENT":
                    output = "fas fa-comment-medical";
                    break;

                case "ADD USER":
                case "CREATE USER":
                case "NEW USER":
                    output = "fas fa-user-plus";
                    break;

                case "ADDRESS":
                    output = "fas fa-address-card";
                    break;

                case "ADD EVENT":
                case "CREATE EVENT":
                case "NEW EVENT":
                    output = "far fa-calendar-plus";
                    break;

                case "ALERT":
                case "DANGER":
                case "WARNING":
                    output = "fas fa-exclamation-triangle";
                    break;

                case "BACK":
                    output = "fas fa-arrow-circle-left";
                    break;

                case "CALL":
                    output = "fas fa-phone";
                    break;

                case "CANCEL":
                case "CLOSE":
                    output = "fas fa-times";
                    break;

                case "CHECK":
                    output = "far fa-check-square";
                    break;

                case "CLOCK":
                case "TIME":
                    output = "fas fa-clock";
                    break;

                case "CODE":
                case "HTML":
                case "SOURCE":
                    output = "fas fa-code";
                    break;

                case "COMMENT":
                    output = "fas fa-comment";
                    break;

                case "COMMENTS":
                    output = "fas fa-comments";
                    break;

                case "COPY":
                case "DUPLICATE":
                    output = "fas fa-copy";
                    break;

                case "DATA":
                case "DATABASE":
                    output = "fas fa-database";
                    break;

                case "DELETE":
                    output = "fas fa-trash";
                    break;

                case "EDIT":
                    output = "far fa-edit";
                    break;

                case "EMAIL":
                case "SEND":
                    output = "far fa-envelope";
                    break;

                case "EXPORT":
                    output = "fas fa-file-export";
                    break;

                case "FIRST":
                    output = "fas fa-step-backward";
                    break;

                case "FORWARD":
                    output = "fas fa-arrow-circle-right";
                    break;

                case "HELP":
                    output = "fas fa-question-circle";
                    break;

                case "HIDE":
                    output = "far fa-eye-slash";
                    break;

                case "HOME":
                    output = "fas fa-home";
                    break;

                case "IMAGE":
                    output = "far fa-image";
                    break;

                case "IMAGES":
                    output = "far fa-images";
                    break;

                case "IMPORT":
                    output = "fas fa-file-import";
                    break;

                case "LAST":
                    output = "fas fa-step-forward";
                    break;

                case "LIST":
                case "LISTS":
                    output = "far fa-list-alt";
                    break;

                case "LOGIN":
                case "LOG IN":
                    output = "fas fa-sign-in-alt";
                    break;

                case "LOGOUT":
                case "LOG OUT":
                    output = "fas fa-sign-out-alt";
                    break;

                case "MENU":
                    output = "fas fa-bars";
                    break;

                case "MOVE":
                    output = "fas fa-arrows-alt";
                    break;

                case "NEXT":
                    output = "fas fa-angle-double-right";
                    break;

                case "OK":
                case "OKAY":
                case "YES":
                    output = "fas fa-check";
                    break;

                case "PICTURE":
                case "PHOTO":
                    output = "fas fa-camera";
                    break;

                case "PREVIOUS":
                    output = "fas fa-angle-double-left";
                    break;

                case "PRINT":
                    output = "fas fa-print";
                    break;

                case "REDO":
                case "REFRESH":
                case "RELOAD":
                    output = "fas fa-sync-alt";
                    break;

                case "REPORT":
                case "STATS":
                case "STATISTICS":
                    output = "fas fa-chart-pie";
                    break;

                case "SAVE":
                    output = "far fa-save";
                    break;

                case "SETTINGS":
                    output = "fas fa-cog";
                    break;

                case "SHOW":
                    output = "far fa-eye";
                    break;

                case "STUDENT":
                case "STUDENTS":
                    output = "fas fa-user-graduate";
                    break;

                case "SUBMIT":
                    output = "fas fa-file-export";
                    break;

                case "TAG":
                    output = "fas fa-tag";
                    break;

                case "TAGS":
                    output = "fas fa-tags";
                    break;

                case "TOGGLE":
                    output = "fas fa-toggle-off";
                    break;

                case "UNCHECK":
                    output = "far fa-square";
                    break;

                case "USERS":
                    output = "fas fa-users";
                    break;
            }

            if (!HasValue(output)) {
                // Try keyword matches
                let buttonText: string = ButtonText.toLowerCase();

                if (buttonText.indexOf("delete ") == 0 || buttonText.indexOf(" delete ") > -1) {
                    output = "fas fa-trash";
                } else if (buttonText.indexOf("edit ") == 0 || buttonText.indexOf(" edit ") > -1) {
                    output = "fas fa-edit";
                } else if (buttonText.indexOf("save ") == 0 || buttonText.indexOf(" save ") > -1) {
                    output = "far fa-save";
                } else if (buttonText.indexOf("add ") == 0 || buttonText.indexOf(" add ") > -1) {
                    output = "fas fa-plus-circle";
                } else if (buttonText.indexOf("yes ") == 0 || buttonText.indexOf(" yes ") > -1) {
                    output = "fas fa-check";
                } else if (buttonText.indexOf("no ") == 0 || buttonText.indexOf(" no ") > -1) {
                    output = "fas fa-times";
                } else if (buttonText.indexOf("print ") == 0 || buttonText.indexOf(" print ") > -1) {
                    output = "fas fa-print";
                } else if (buttonText.indexOf("submit ") == 0 || buttonText.indexOf(" submit ") > -1) {
                    output = "fas fa-file-export";
                } else if (buttonText.indexOf("help ") == 0 || buttonText.indexOf(" help ") > -1) {
                    output = "fas fa-question-circle";
                } else if (buttonText.indexOf("users ") == 0 || buttonText.indexOf(" users ") > -1) {
                    output = "fas fa-users";
                } else if (buttonText.indexOf("uncheck ") == 0 || buttonText.indexOf(" uncheck ") > -1) {
                    output = "far fa-square";
                } else if (buttonText.indexOf("check ") == 0 || buttonText.indexOf(" check ") > -1) {
                    output = "far fa-check-square";
                } else if (buttonText.indexOf("toggle ") == 0 || buttonText.indexOf(" toggle ") > -1) {
                    output = "fas fa-toggle-off";
                } else if (buttonText.indexOf("hide ") == 0 || buttonText.indexOf(" hide ") > -1) {
                    output = "far fa-eye-slash";
                } else if (buttonText.indexOf("show ") == 0 || buttonText.indexOf(" show ") > -1) {
                    output = "far fa-eye";
                } else if (buttonText.indexOf("list ") == 0 || buttonText.indexOf("lists ") == 0 || buttonText.indexOf(" list ") > -1 || buttonText.indexOf(" lists ") > -1) {
                    output = "far fa-list-alt";
                } else if (buttonText.indexOf("image ") == 0 || buttonText.indexOf(" image ") > -1) {
                    output = "far fa-image";
                } else if (buttonText.indexOf("images ") == 0 || buttonText.indexOf(" images ") > -1) {
                    output = "far fa-images";
                } else if (buttonText.indexOf("tag ") == 0 || buttonText.indexOf(" tag ") > -1) {
                    output = "fas fa-tag";
                } else if (buttonText.indexOf("tags ") == 0 || buttonText.indexOf(" tags ") > -1) {
                    output = "fas fa-tags";
                } else if (buttonText.indexOf("toggle ") == 0 || buttonText.indexOf(" toggle ") > -1) {
                    output = "fas fa-toggle-off";
                } else if (buttonText.indexOf("student ") == 0 || buttonText.indexOf("students ") == 0 || buttonText.indexOf(" student ") > -1 || buttonText.indexOf(" students ") > -1) {
                    output = "fas fa-user-graduate";
                }
            }

        } else if (HasValue(CurrentIcon)) {
            output += CurrentIcon;
        }

        return output;
    }

    /**
     * Sets the focus to an element as soon as it's visible. Some things like Modal dialogs have a delay in showing, so this allows the focus to be set as soon as the item becomes visible.
     * @param {string} element - The ID of the HTML element.
     * @param {number} [safety] Leave empty, this is used internally to make sure only a few attempts are made to show the item.
     */
    export function DelayedFocus(element: string, safety?: number): void {
        if (safety == undefined || safety == null) { safety = 0; }
        if (safety > 20) { return; }
        safety++;

        if ($("#" + element).is(":visible") == true) {
            $("#" + element).focus();
        } else {
            setTimeout(() => DelayedFocus(element, safety), 20);
        }
    }

    /**
     * Sets the focus to an input element and selects the content as soon as it's visible. Some things like Modal dialogs have a delay in showing, so this allows the focus to be set as soon as the item becomes visible.
     * @param {string} element - The ID of the HTML element.
     * @param {number} [safety] Leave empty, this is used internally to make sure only a few attempts are made to show the item.
     */
    export function DelayedSelect(element: string, safety?: number): void {
        if (safety == undefined || safety == null) { safety = 0; }
        if (safety > 20) { return; }
        safety++;

        if ($("#" + element).is(":visible") == true) {
            $("#" + element).select();
        } else {
            setTimeout(() => DelayedSelect(element, safety), 20);
        }
    }

    /**
     * Converts an array of strings into an error message.
     * @param {string[]} errors - The array of strings containing one or more error messages.
     * @param {string} noticeText - Optional text to use to show before the errors. If not specified then "<strong>Please correct the following error{s}</strong>:" is used. The {s} in the string will be removed for a single result and converted to an "s" for multiple result.
     * @returns {string} Returns the message formatted as an error message.
     */
    export function ErrorMessage(errors: string[], noticeText?: string): string {
        let output: string = "";
        if (errors != undefined && errors != null && errors.length > 0) {

            if (!HasValue(noticeText)) {
                noticeText = "<strong>Please correct the following error{s}</strong>:";
            }

            if (errors.length == 1) {
                noticeText = noticeText.replace("{s}", "");
                output = "<div class='tsutils-padbottom'>" + noticeText + "</div>\n" +
                    "<div>" + errors[0] + "</div>";
            } else {
                noticeText = noticeText.replace("{s}", "s");
                output = "<div class='tsutils-padbottom'>" + noticeText + "</div>\n<ul>\n";
                errors.forEach(function (msg) {
                    output += "  <li>" + msg + "</li>\n";
                });
                output += "</ul>";
            }
        }
        return output;
    }

    /**
     * Finds the text between two different strings. Useful for finding text between tags in XML, etc.
     * @param {string} searchIn - The string to be searched.
     * @param {string} start - The text to find for the beginning of the search.
     * @param {string} end - The text that marks the end of the search.
     * @returns {string} Returns the text found between the start and end.
     */
    export function FindTextBetween(searchIn: string, start: string, end: string): string {
        let output: string = "";
        if (searchIn != undefined && searchIn != null && searchIn != "" && start != undefined && start != null && start != "" && end != undefined && end != null && end != "") {
            let s: number = searchIn.toLowerCase().indexOf(start.toLowerCase());
            if (s > -1) {
                let e: number = searchIn.toLowerCase().indexOf(end.toLowerCase(), s + 1);
                if (e > -1) {
                    output = searchIn.substr(s, e - s + 1);
                }
            }
        }
        return output;
    }

    /**
     * Formats a number as currency with a dollar sign and two decimal places.
     * @param {number} amount - The number to be formatted as currency.
     * @param {string} [currencySymbol] - Optionally pass in the currency symbol (defaults to $.)
     * @returns {string} Returns the number formatted as currency.
     */
    export function FormatCurrency(amount: number, currencySymbol?: string): string {
        if (currencySymbol == undefined || currencySymbol == null || currencySymbol == "") {
            currencySymbol = "$";
        }
        let output: string = currencySymbol + (0).toFixed(2);
        if (amount != undefined && amount != null && !isNaN(amount)) {
            output = currencySymbol + amount.toFixed(2);
        }
        return output;
    }

    /**
     * Formats a date in the "M/D/YYYY" format.
     * @param {Date} date - The date to be formatted.
     * @returns {string} Returns the date formatted as "M/D/YYYY"
     */
    export function FormatDate(date: Date): string {
        let output: string = "";
        if (date != undefined && date != null) {
            output = moment(date).format("M/D/YYYY");
        }
        return output;
    }

    /**
     * Formats a date in the "M/D/YYYY h:mm a" format.
     * @param date (Date: The date to be formatted.)
     * @returns {string} Returns the date formatted as "M/D/YYYY h:mm a"
     */
    export function FormatDateTime(date: Date): string {
        let output: string = "";
        if (date != undefined && date != null) {
            output = moment(date).format("M/D/YYYY h:mm a")
        }
        return output;
    }

    /**
     * Formats a date in the "M/D/YYYY h:mm:ss a" format.
     * @param date (Date: The date to be formatted.)
     * @returns {string} Returns the date formatted as "M/D/YYYY h:mm:ss a"
     */
    export function FormatDateTimeLong(date: Date): string {
        let output: string = "";
        if (date != undefined && date != null) {
            output = moment(date).format("M/D/YYYY h:mm:ss a")
        }
        return output;
    }

    /**
     * Formats a date in the "h:mm a" format.
     * @param date (Date: The date to be formatted.)
     * @returns {string} Returns the date formatted as "h:mm a"
    */
    export function FormatTime(date: Date): string {
        let output: string = "";
        if (date != undefined && date != null) {
            output = moment(date).format("h:mm a");
        }
        return output;
    }

    /**
     * Formats a date in the "h:mm:ss a" format.
     * @param {Date} date - The date to be formatted.
     * @returns {string} - Returns the date formatted as "h:mm:ss a"
     */
    export function FormatTimeLong(date: Date): string {
        let output: string = "";
        if (date != undefined && date != null) {
            output = moment(date).format("h:mm:ss a");
        }
        return output;
    }

    /**
     * Generates a new GUID.
     * @returns {string} Returns a string containing a new GUID.
     */
    export function GenerateGuid(): string {
        let d: number = new Date().getTime();
        let uuid: string = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            let r: number = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
        return uuid;
    }

    /**
     * Gets the extension from a file name (does not include the leading period.)
     * @param {string} file - The filename from which you wish to get the extension.
     * @returns {string} Returns the extension with no leading period.
     */
    export function GetExtension(file: string): string {
        let output: string = "";
        let re: RegExp = /(?:\.([^.]+))?$/;
        let ext: string = "." + re.exec(file)[1];
        if (ext != ".") {
            ext = ext.replace(".", "");
            output = ext;
        }
        return output;
    }

    /**
     * Returns an empty GUID (00000000-0000-0000-0000-000000000000).
     * @returns {string} Returns an empty GUID (00000000-0000-0000-0000-000000000000).
     */
    export function GuidEmpty(): string {
        return "00000000-0000-0000-0000-000000000000";
    }

    /**
     * Tests to see if the number passed in contains a value.
     * @param {any} check - The number to check for a value.
     * @returns {boolean} Returns true if the item passed in contained a value.
     */
    export function HasNumericalValue(check: any): boolean {
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

    /**
     * Determines if the string passed in contains a value and is not "undefined", null, or an empty string.
     * @param {string} check - The string to check for a value.
     * @returns {boolean} Returns true if the item passed in is not "undefined", not null, and not an empty string.
     */
    export function HasValue(check: string): boolean {
        return check != undefined && check != null && check != "";
    }

    export function HtmlEditor(element: string, config?: htmlEditorConfiguration): void {
        // Make sure the element actually exists
        if ($("#" + element).length == 0) {
            console.log("Element '" + element + "' not found, unable to create HTML editor.");
            return;
        }

        if (config == undefined || config == null) {
            config = {
                html: "",
                onupdate: null,
                onmodechange: null,
                simpleView: false,
                allowImageUploads: false,
                hideSourceButton: false,
                required: false,
                placeholderText: "HTML Editor"
            };
        }

        if (!tsUtilities.HasValue(config.html)) { config.html = ""; }
        if (!tsUtilities.HasValue(config.placeholderText)) { config.placeholderText = "HTML Editor"; }
        if (config.hideSourceButton == undefined || config.hideSourceButton == null) { config.hideSourceButton = false; }
        if (config.required == undefined || config.required == null) { config.required = false; }

        var allowUploads = config.allowImageUploads != undefined && config.allowImageUploads != null && config.allowImageUploads == true;

        if (CKEDITOR.instances[element] != undefined) {
            CKEDITOR.instances[element].destroy();
        }

        CKEDITOR.config.htmlEncodeOutput = false;
        CKEDITOR.config.entities = false;
        CKEDITOR.config.forcePasteAsPlainText = false;
        CKEDITOR.config.dataIndentationChars = '  ';
        CKEDITOR.config.height = '50px';
        CKEDITOR.config.resize_minHeight = 180;
        CKEDITOR.config.autoGrow_minHeight = 20;
        CKEDITOR.config.autoGrow_onStartup = true;
        CKEDITOR.config.editorplaceholder = config.placeholderText;
        CKEDITOR.config.baseFloatZIndex = 9999999;

        CKEDITOR.config.font_names =
            'Arial/Arial, Helvetica, sans-serif;' +
            'Brush Script MT/Brush Script MT, cursive;' +
            'Courier New/Courier New, monospace;' +
            'Garamond/Garamond, serif;' +
            'Georgia/Georgia, serif;' +
            'Helvetica/Helvetica, sans-serfi;' +
            'Tahoma/Tahoma/, sans-serif;' +
            'Times New Roman/Times New Roman, Times, serif;' +
            'Trebuchet MS/Trebuchet MS, sans-serif;' +
            'Verdana/Verdana, sans-serif;';

        var fontSizes = ['10', '11', '12', '14', '16', '18', '20', '22', '24', '26', '28', '36', '48', '72'];
        var fontSizeList = "";
        for (var x = 0; x < fontSizes.length; x++) {
            fontSizeList += fontSizes[x] + "/" + fontSizes[x] + "px;";
        }

        CKEDITOR.config.fontSize_sizes = fontSizeList;

        var extraRemove = "";
        var disallowedContent = 'form; input; textarea; radio;';

        if (!allowUploads) {
            disallowedContent += 'img; ';
            extraRemove = ",Image";
        }

        if (config.hideSourceButton) {
            extraRemove += ",Source";
        }

        if (config.simpleView != undefined && config.simpleView != null && config.simpleView == true) {
            // The simple view
            CKEDITOR.config.toolbarGroups = [
                { name: 'document', groups: ['mode', 'document', 'doctools'] },
                { name: 'clipboard', groups: ['clipboard', 'undo'] },
                { name: 'editing', groups: ['find', 'selection', 'spellchecker', 'editing'] },
                { name: 'forms', groups: ['forms'] },
                { name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
                { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi', 'paragraph'] },
                { name: 'links', groups: ['links'] },
                { name: 'insert', groups: ['insert'] },
                { name: 'styles', groups: ['styles'] },
                { name: 'colors', groups: ['colors'] },
                { name: 'tools', groups: ['tools'] },
                { name: 'others', groups: ['others'] },
                { name: 'about', groups: ['about'] }
            ];
            CKEDITOR.config.removeButtons = 'Save,NewPage,ExportPdf,Preview,Print,Templates,Find,Replace,Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,Language,Smiley,SpecialChar,PageBreak,Iframe,Styles,Format,About,Cut,Copy,Paste,PasteText,PasteFromWord,Undo,Redo,SelectAll,Scayt,Source,NumberedList,BulletedList,CopyFormatting,RemoveFormat,Strike,Subscript,Superscript,Blockquote,CreateDiv,BidiLtr,BidiRtl,Link,Unlink,Anchor,Table,HorizontalRule,Font,FontSize,Maximize' + extraRemove;
        } else {
            CKEDITOR.config.toolbarGroups = [
                { name: 'document', groups: ['mode', 'document', 'doctools'] },
                { name: 'clipboard', groups: ['clipboard', 'undo'] },
                { name: 'editing', groups: ['find', 'selection', 'spellchecker', 'editing'] },
                { name: 'forms', groups: ['forms'] },
                '/',
                { name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
                { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi', 'paragraph'] },
                { name: 'links', groups: ['links'] },
                { name: 'insert', groups: ['insert'] },
                '/',
                { name: 'styles', groups: ['styles'] },
                { name: 'colors', groups: ['colors'] },
                { name: 'tools', groups: ['tools'] },
                { name: 'others', groups: ['others'] },
                { name: 'about', groups: ['about'] }
            ];
            CKEDITOR.config.removeButtons = 'Save,NewPage,ExportPdf,Preview,Print,Templates,Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,Language,BidiRtl,BidiLtr,Anchor,PageBreak,Iframe,Maximize,About,Styles,uploadButton' + extraRemove;
        }

        CKEDITOR.config.format_tags = 'p;h1;h2;h3;h4;pre;address;div';
        CKEDITOR.config.autoParagraph = false;
        CKEDITOR.config.disallowedContent = disallowedContent;

        CKEDITOR.config.contentsCss = window.baseURL + "css/htmlEditor.css";

        $("#" + element).val(config.html);

        var editor = CKEDITOR.replace(element);

        if (editor !== undefined && editor !== null) {
            editor.on("instanceReady", function (evt) {
                evt.editor.commands.save.disable();
                //console.log( editor.filter.allowedContent );
                $("#cke_" + element).removeClass("missing-required");
                if (config.required == true && config.html == "") {
                    $("#cke_" + element).addClass("missing-required");
                }
            });
            editor.on("change", function (evt) {
                if (config.onupdate != undefined && config.onupdate != null) {
                    var html = evt.editor.getData();
                    config.onupdate(html);

                    if (config.required == true) {
                        if (html == null || html == "") {
                            $("#cke_" + element).addClass("missing-required");
                        } else {
                            $("#cke_" + element).removeClass("missing-required");
                        }
                    }
                }
            });

            editor.on("mode", function () {
                if (config.onmodechange != undefined && config.onmodechange != null) {
                    config.onmodechange(this.mode);
                }
            });
        }
    }

    export function HtmlEditorDestroy(element: string): void {
        if ($("#" + element).length == 0) {
            return;
        }

        if (CKEDITOR.instances[element] != undefined) {
            CKEDITOR.instances[element].destroy();
        }
    }

    export function HtmlEditorExists(element: string): boolean {
        let output: boolean = false;

        if (CKEDITOR.instances[element] != undefined) {
            output = true;
        }

        return output;
    }

    export function HtmlEditorFocus(element: string, safety?: number): void {
        if (safety == undefined || safety == null) { safety = 1; }
        if (safety > 50) { return; }
        safety++;

        if ($("#" + element).nextUntil("div.cke").andSelf().last().next().is(":visible")) {
            if (CKEDITOR.instances[element] != undefined) {
                setTimeout(() => CKEDITOR.instances[element].focus(), 100);
            } else {
                setTimeout(() => HtmlEditorFocus(element, safety), 20);
            }

        } else {
            setTimeout(() => HtmlEditorFocus(element, safety), 20);
        }
    }

    export function HtmlEditorInsertText(element: string, text: string, asHtml?: boolean): void {
        if (CKEDITOR.instances[element] != undefined) {
            if (asHtml != undefined && asHtml != null && asHtml == true) {
                CKEDITOR.instances[element].insertHtml(text);
            } else {
                CKEDITOR.instances[element].insertText(text);
            }
        }
    }

    export function HtmlEditorUpdate(element: string, html: string): void {
        if (CKEDITOR.instances[element] != undefined) {
            CKEDITOR.instances[element].setData(html);
        }
    }

    /**
     * Sets up Knockout Custom Binding Handlers to support HTML binding, date (datePicker), and dateTime (dateTimePicker)
     */
    export function KnockoutCustomBindingHandlers(): void {
        eval("moment.tz.guess();");

        ko.bindingHandlers.dateTime = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                ko.utils.registerEventHandler(element, 'change', function () {
                    let value: any = valueAccessor();
                    if (element != undefined && element != null && element.value != undefined && element.value != null && element.value.length > 0) {
                        // To make sure the date includes the correct Timezone information for posting to an API controller
                        // it needs to be converted to a correct full date instead of just the M/D/YYY h:mm a format.
                        let correctDate: Date = new Date(moment(element.value).format("M/D/YYYY h:mm a"));
                        value(correctDate);
                    } else {
                        value(null);
                    }
                });
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                let value: any = valueAccessor();
                let valueUnwrapped: any = ko.utils.unwrapObservable(value);
                let output: string = '';
                if (valueUnwrapped != undefined && valueUnwrapped != null && element != undefined && element != null && element.value != undefined && element.value != null) {
                    output = moment(valueUnwrapped).format("M/D/YYYY h:mm a");
                }
                if ($(element).is('input') === true) {
                    $(element).val(output);
                } else {
                    $(element).text(output);
                }
            }
        };

        ko.bindingHandlers.date = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                ko.utils.registerEventHandler(element, 'change', function () {
                    let value: any = valueAccessor();
                    if (element != undefined && element != null && element.value != undefined && element.value != null && element.value.length > 0) {
                        // Converts the date string back to a date object.
                        let correctDate: Date = new Date(moment(element.value).format("M/D/YYYY"));
                        value(correctDate);
                    } else {
                        value(null);
                    }
                });
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                let value: any = valueAccessor();
                let valueUnwrapped: any = ko.utils.unwrapObservable(value);
                let output: string = '';
                if (valueUnwrapped != undefined && valueUnwrapped != null && element != undefined && element != null && element.value != undefined && element.value != null) {
                    //output = FormatDate(valueUnwrapped);
                    output = moment(valueUnwrapped).format("M/D/YYYY");
                }

                if ($(element).is('input') === true) {
                    $(element).val(output);
                } else {
                    $(element).text(output);
                }
            }
        };

        var inject_binding = function (allBindings, key, value) {
            //https://github.com/knockout/knockout/pull/932#issuecomment-26547528
            return {
                has: function (bindingKey) {
                    return (bindingKey == key) || allBindings.has(bindingKey);
                },
                get: function (bindingKey) {
                    var binding = allBindings.get(bindingKey);
                    if (bindingKey == key) {
                        binding = binding ? [].concat(binding, value) : value;
                    }
                    return binding;
                }
            };
        }

        ko.bindingHandlers.selectize = {
            init: function (element, valueAccessor, allBindingsAccessor: any, viewModel: any, bindingContext) {
                if (!allBindingsAccessor.has('optionsText'))
                    allBindingsAccessor = inject_binding(allBindingsAccessor, 'optionsText', 'name');
                if (!allBindingsAccessor.has('optionsValue'))
                    allBindingsAccessor = inject_binding(allBindingsAccessor, 'optionsValue', 'id');
                if (typeof allBindingsAccessor.get('optionsCaption') == 'undefined')
                    allBindingsAccessor = inject_binding(allBindingsAccessor, 'optionsCaption', 'Choose...');

                ko.bindingHandlers.options.update(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);

                var options = {
                    valueField: allBindingsAccessor.get('optionsValue'),
                    labelField: allBindingsAccessor.get('optionsText'),
                    searchField: allBindingsAccessor.get('optionsText')
                }

                if (allBindingsAccessor.has('options')) {
                    var passed_options = allBindingsAccessor.get('options')
                    for (var attr_name in passed_options) {
                        options[attr_name] = passed_options[attr_name];
                    }
                }

                var $select = $(element).selectize(options)[0].selectize;

                if (typeof allBindingsAccessor.get('value') == 'function') {
                    $select.addItem(allBindingsAccessor.get('value')());
                    allBindingsAccessor.get('value').subscribe(function (new_val) {
                        $select.addItem(new_val);
                    })
                }

                if (typeof allBindingsAccessor.get('selectedOptions') == 'function') {
                    allBindingsAccessor.get('selectedOptions').subscribe(function (new_val) {
                        // Removing items which are not in new value
                        var values = $select.getValue();
                        var items_to_remove = [];
                        for (var k in values) {
                            if (new_val.indexOf(values[k]) == -1) {
                                items_to_remove.push(values[k]);
                            }
                        }

                        for (var k in items_to_remove) {
                            $select.removeItem(items_to_remove[k]);
                        }

                        for (var k in new_val) {
                            $select.addItem(new_val[k]);
                        }

                    });
                    var selected = allBindingsAccessor.get('selectedOptions')();
                    for (var k in selected) {
                        $select.addItem(selected[k]);
                    }
                }

                if (typeof valueAccessor().subscribe == 'function') {
                    valueAccessor().subscribe(function (changes) {
                        // To avoid having duplicate keys, all delete operations will go first
                        var addedItems = new Array();
                        changes.forEach(function (change) {
                            switch (change.status) {
                                case 'added':
                                    addedItems.push(change.value);
                                    break;
                                case 'deleted':
                                    var itemId = change.value[options.valueField];
                                    if (itemId != null) $select.removeOption(itemId);
                            }
                        });
                        addedItems.forEach(function (item) {
                            $select.addOption(item);
                        });

                    }, null, "arrayChange");
                }

            },
            update: function (element, valueAccessor, allBindingsAccessor) {

                if (allBindingsAccessor.has('object')) {
                    var optionsValue = allBindingsAccessor.get('optionsValue') || 'id';
                    var value_accessor = valueAccessor();
                    var selected_obj = $.grep(value_accessor(), function (i) {
                        if (typeof i[optionsValue] == 'function')
                            var id = i[optionsValue]
                        else
                            var id = i[optionsValue]
                        return id == allBindingsAccessor.get('value')();
                    })[0];

                    if (selected_obj) {
                        allBindingsAccessor.get('object')(selected_obj);
                    }
                }
            }
        }
    }

    export function LastDayOfMonth(date: Date): Date {
        let startOfMonth: Date = new Date(date.getFullYear(), date.getMonth(), 1);
        let nextMonth: Date = AddDays(startOfMonth, 35);
        let firstOfNextMonth: Date = new Date(nextMonth.getFullYear(), nextMonth.getMonth(), 1);
        let output: Date = AddDays(firstOfNextMonth, -1);
        return output;
    }

    /**
     * Converts an array of strings to a message. If a single string is in the array only the text is returned. If multiple items are in the array an <ul> element is returned with <li> elements for each text item.
     * @param {string[]} messages - An array of strings.
     * @returns {string} Returns a single string if the array only contained one string, or a <ul> with <li> elements for each string.
     */
    export function MessagesToString(messages: string[]): string {
        let output: string = "";
        if (messages != undefined && messages != null && messages.length > 0) {
            if (messages.length == 1) {
                output = messages[0];
            } else {
                output = "<ul>\n";
                messages.forEach(function (msg) {
                    output += "  <li>" + msg + "</li>\n";
                });
                output += "</ul>\n";
            }
        }
        return output;
    }

    export function MinutesToDaysHoursAndMinutes(minutes: number): string {
        let output: string = "none";

        if (minutes > 0) {
            let years: number = 0;
            let days: number = 0;
            let hours: number = 0;

            if (minutes > 525600) {
                years = Math.floor(minutes / 525600);
                minutes = minutes - (525600 * years);
            }

            if (minutes > 1440) {
                days = Math.floor(minutes / 1440);
                minutes = minutes - (1440 * days);
            }

            if (minutes > 59) {
                hours = Math.floor(minutes / 60);
                minutes = minutes - (60 * hours);
            }

            output = "";
            if (years > 0) {
                output += years.toString() + " year" + (years > 1 ? "s" : "");
            }
            if (days > 0) {
                if (output != "") { output += ", "; }
                output += days.toString() + " day" + (days > 1 ? "s" : "");
            }
            if (hours > 0) {
                if (output != "") { output += ", "; }
                output += hours.toString() + " hour" + (hours > 1 ? "s" : "");
            }
            if (minutes > 0) {
                if (output != "") { output += ", "; }
                output += minutes.toString() + " minute" + (minutes > 1 ? "s" : "");
            }
        }

        return output;
    }

    export function MinutesToHoursAndMinutes(minutes: number): string {
        let output: string = "none";
        if (minutes > 0) {
            if (minutes > 59) {
                let hours: number = Math.floor(minutes / 60);
                minutes = minutes - (60 * hours);
                output = hours.toString() + " hour" + (hours > 1 ? "s" : "");
                if (minutes > 0) {
                    output += ", " + minutes.toString() + " minute" + (minutes > 1 ? "s" : "");
                }
            } else {
                output = minutes.toString() + " minute" + (minutes > 1 ? "s" : "");
            }
        }
        return output;
    }

    /**
     * Another Modal Dialog option with more controls over the buttons.
     * @param {string} message - The message to show.
     * @param {string} [title] - An optional title for the dialog (defaults to "Message".)
     * @param {string} [width=""] - Optional string to set the width of the modal in percentage, px, etc. to override the bootstrap default of "max-width:500px;".
     * @param {tsutilsDTO.tsutilsDialogButton[]} buttons - An optional array of buttons to be shown on the dialog. If none are specified only a close button is shown.
     */
    export function Modal(message: string, title?: string, width?: string, buttons?: tsutilsDTO.tsutilsDialogButton[]): void {
        if (!HasValue(title)) { title = "Message"; }
        if (!HasValue(width)) { width = ""; }

        CreateModalDialogIfMissing();

        $("#tsutilsModalExtraButtons").html("");
        ResetScrolling("tsutilsModalDialog");

        // Generate any buttons
        let buttonsTop: string = "";
        let buttonsTopFixed: string = "";
        let buttonsBottom: string = "";

        if (buttons != undefined && buttons != null) {
            let buttonGroups: string[] = [];

            let buildButton: Function = (buttonClass: string, buttonId: string, content: string): string => {
                return "<button type='button' class='" + (HasValue(buttonClass) ? buttonClass : "btn btn-secondary") + "' id='" + buttonId + "'>" + content + "</button>\n"
            };

            // Make sure buttons all have a defined position
            buttons.forEach(function (button) {
                if (!HasValue(button.Position)) { button.Position = "bottom"; }
                if (!HasValue(button.Id)) { button.Id = GenerateGuid(); }
            });

            buttons.forEach(function (button) {
                let content: string = "";

                let icon: string = DefaultButtonIcon(button.Text, button.Icon);
                if (HasValue(icon)) {
                    content += "<i class='" + icon + "'></i>";
                }
                if (HasValue(button.Text)) {
                    if (content != "") { content += " "; }
                    content += button.Text;
                }

                let position: string = button.Position;
                let buttonGroup: string = button.GroupName;

                if (HasValue(content)) {
                    let b: string = buildButton(button.Class, button.Id, content);

                    if (HasValue(buttonGroup)) {
                        // In a group, so process if this group hasn't already been processed
                        b = "";

                        if (buttonGroups.indexOf(buttonGroup + "_" + position) == -1) {
                            buttonGroups.push(buttonGroup + "_" + position);

                            // Find all buttons in this same grouping for the same position.
                            for (let x: number = 0; x < buttons.length; x++) {
                                let checkButton: tsutilsDTO.tsutilsDialogButton = buttons[x];
                                if (checkButton.GroupName == buttonGroup && checkButton.Position == position) {
                                    content = "";

                                    icon = DefaultButtonIcon(checkButton.Text, checkButton.Icon);
                                    if (HasValue(icon)) {
                                        content += "<i class='" + icon + "'></i>";
                                    }

                                    if (HasValue(checkButton.Text)) {
                                        if (content != "") { content += " "; }
                                        content += checkButton.Text;
                                    }

                                    b += buildButton(checkButton.Class, checkButton.Id, content);
                                }
                            }
                        }

                        if (b != "") {
                            b = "<div class='btn-group' role='group'>" + b + "</div>\n";
                        }
                    }

                    switch (position.toUpperCase()) {
                        case "TOP":
                            buttonsTop += b;
                            break;

                        case "TOP-FIXED":
                            buttonsTopFixed += b;
                            break;

                        case "BOTTOM":
                            buttonsBottom += b;
                            break;
                    }
                }
            });
        }


        // Add the buttons based on their locations
        if (HasValue(buttonsTopFixed)) {
            message =
                "<div class='tsutils-padbottom tsutils-fixed-position'>\n" +
                buttonsTopFixed +
                "</div>\n" +
                message;
        }
        if (HasValue(buttonsTop)) {
            message =
                "<div class='tsutils-padbottom'>\n" +
                buttonsTop +
                "</div>\n" +
                message;
        }
        if (HasValue(buttonsBottom)) {
            $("#tsutilsModalExtraButtons").html(buttonsBottom);
            $("#tsutilsModalExtraButtons").show();
            $("#tsutilsModalDialogFooter").show();
        } else {
            // Hide the regular dialog footer
            $("#tsutilsModalDialogFooter").hide();
            $("#tsutilsModalExtraButtons").hide();
            $("#tsutilsModalExtraButtons").html("");
        }

        ModalDialogClearMessages();
        $("#tsutilsModalDialogTitle").html(title);
        $("#tsutilsModalDialogBody").html(message);

        if (width == "") {
            $("#tsutilsModalDialogBox").css("max-width", "");
            $("#tsutilsModalDialogBox").css("width", "");
        } else {
            $("#tsutilsModalDialogBox").css("max-width", width);
            $("#tsutilsModalDialogBox").css("width", width);
        }

        // Hide the built-in buttons
        $("#tsutilsModalDialogOkButton").hide();
        $("#tsutilsModalDialogCancelButton").hide();

        // Prevent the ESC key from closing the dialog.
        $("#tsutilsModalDialog").modal({ backdrop: 'static' });
        $("#tsutilsModalDialog").modal({ keyboard: false });

        // If the dialog is not currently visible then show it
        if (ModalDialogIsOpen() == false) {
            $("#tsutilsModalDialog").modal("show");
        }

        // Add the button callback handlers
        if (buttons != undefined && buttons != null) {
            buttons.forEach(function (button) {
                if (HasValue(button.Icon + button.Text)) {
                    $("#" + button.Id).off('click');

                    let closeDialog: boolean = true;
                    if (button.ClosesDialog != undefined && button.ClosesDialog != null && button.ClosesDialog == false) {
                        closeDialog = false;
                    }

                    if (typeof button.CallbackHandler == "function") {
                        if (closeDialog) {
                            $("#" + button.Id).on('click', () => { ModalDialogHide(); button.CallbackHandler(); });
                        } else {
                            $("#" + button.Id).on('click', () => { button.CallbackHandler() });
                        }
                    } else if (closeDialog) {
                        $("#" + button.Id).on('click', () => { ModalDialogHide(); });
                    }
                }
            });
        }
    }

    /**
     * Displays a Bootstrap modal dialog using the modal layout in the included _utilSampleModalDialog.cshtml file.
     * @param {string} message - The message to show.
     * @param {string} [title] - An optional title for the dialog (defaults to "Message".)
     * @param {string} [okButtonText] - Optional text for the OK button. If empty, the button will say "OK" and no Cancel button will display.
     * @param {string} [okButtonIcon] - A class to add to the button to show in an I element as an icon (i.e. "fas fa-check"). Set to "auto" to attempt to select an icon automatically based on the button text.
     * @param {Function} [okHandler] - An optional callback handler to be called when the OK button is clicked.
     * @param {Function} [cancelHandler] - An optional callback handler to be called when the Cancel button is clicked.
     * @param {string} [width=""] - Optional string to set the width of the modal in percentage, px, etc. to override the bootstrap default of "max-width:500px;".
     * @param {boolean} [closeOnOk=true] - Optional boolean to indicate if the Modal should close when the OK button is clicked (defaults to true).
     */
    export function ModalDialog(message: string, title?: string, okButtonText?: string, okButtonIcon?: string, okHandler?: Function, cancelHandler?: Function, width?: string, closeOnOk?: boolean): void {
        if (title == undefined || title == null || title == "") { title = "Message"; }

        if (!HasValue(okButtonText)) { okButtonText = ""; }
        if (!HasValue(okButtonIcon)) {
            okButtonIcon = "fas fa-check";
        } else {
            okButtonIcon = DefaultButtonIcon(okButtonText, okButtonIcon);
        }

        if (!HasValue(width)) { width = ""; }

        if (closeOnOk == undefined || closeOnOk == null) { closeOnOk = true; }

        CreateModalDialogIfMissing();

        ResetScrolling("tsutilsModalDialog");
        $("#tsutilsModalExtraButtons").html("");

        ModalDialogClearMessages();
        $("#tsutilsModalDialogTitle").html(title);
        $("#tsutilsModalDialogBody").html(message);

        $("#tsutilsModalDialogFooter").show();

        // If the OK button has the trash icon then set the button to the danger class, otherwise, use the success class
        if (okButtonIcon.toLowerCase().indexOf("trash") > -1) {
            $("#tsutilsModalDialogOkButton").removeClass();
            $("#tsutilsModalDialogOkButton").addClass("btn btn-danger")
        } else {
            $("#tsutilsModalDialogOkButton").removeClass();
            $("#tsutilsModalDialogOkButton").addClass("btn btn-success")
        }

        if (width == "") {
            $("#tsutilsModalDialogBox").css("max-width", "");
            $("#tsutilsModalDialogBox").css("width", "");
        } else {
            $("#tsutilsModalDialogBox").css("max-width", width);
            $("#tsutilsModalDialogBox").css("width", width);
        }

        $("#tsutilsModalDialogCancelButton").show();

        if (okButtonText == undefined || okButtonText == null || okButtonText == "") {
            $("#tsutilsModalDialogOkButtonText").html("OK");
            $("#tsutilsModalDialogOkButtonIcon").removeClass();
            $("#tsutilsModalDialogOkButtonIcon").addClass(okButtonIcon);
            $("#tsutilsModalDialogOkButton").show();
            $("#tsutilsModalDialogCancelButton").hide();
        } else if (okButtonText == "hidden") {
            $("#tsutilsModalDialogOkButton").hide();
            $("#tsutilsModalDialogCancelButton").hide();
        } else {
            $("#tsutilsModalDialogOkButtonIcon").removeClass();
            $("#tsutilsModalDialogOkButtonIcon").addClass(okButtonIcon);
            $("#tsutilsModalDialogOkButtonText").html(okButtonText);
            $("#tsutilsModalDialogOkButton").show();
        }

        // Prevent the ESC key from closing the dialog.
        $("#tsutilsModalDialog").modal({ backdrop: 'static' });
        $("#tsutilsModalDialog").modal({ keyboard: false });

        // If the dialog is not currently visible then show it
        if (ModalDialogIsOpen() == false) {
            $("#tsutilsModalDialog").modal("show");
        }

        // setup click event for the OK button
        $("#tsutilsModalDialogOkButton").off('click');
        if (okHandler != undefined && okHandler != null) {
            if (closeOnOk == true) {
                $("#tsutilsModalDialogOkButton").on('click', () => ModalDialogHide());
                $("#tsutilsModalDialogOkButton").click(() => okHandler());
            } else {
                $("#tsutilsModalDialogOkButton").click(() => okHandler());
            }
        } else {
            $("#tsutilsModalDialogOkButton").on('click', () => ModalDialogHide());
        }

        // setup click event for the cancel button
        $("#tsutilsModalDialogCancelButton").off('click');
        if (cancelHandler != undefined && cancelHandler != null) {
            $("#tsutilsModalDialogCancelButton").click(ModalDialogHide);
            $("#tsutilsModalDialogCancelButton").click(() => cancelHandler());
        } else {
            $("#tsutilsModalDialogCancelButton").click(ModalDialogHide);
        }
    }

    /**
     * Clears any messages showing at the top of the Modal dialog box.
     */
    export function ModalDialogClearMessages(): void {
        $("#tsutilsModalDialogError").html("");
        $("#tsutilsModalDialogError").hide();
        $("#tsutilsModalDialogMessage").html("");
        $("#tsutilsModalDialogMessage").hide();
    }

    /**
     * Hides the Modal dialog box.
     */
    export function ModalDialogHide(): void {
        // Hide the dialog box, clear any previous message, and hide any previously-shown "tsutils-modal-section" elements.
        $("#tsutilsModalDialog").modal("hide");
        $("#tsutilsModalDialogBody").html("");
        $(".tsutils-modal-section").hide();
    }

    /**
     * Hides the Modal dialog box after a specified number of seconds.
     * @param {number} [secs=3] - The number of seconds until the dialog hides (defaults to 3.)
     * @param {boolean} [showCountdownTimer=false] - Option to show a countdown timer in the title area (defaults to true.)
     */
    export function ModalDialogHideOnTimer(secs?: number, showCountdownTimer?: boolean): void {
        if (secs == undefined || secs == null || secs < 1) { secs = 3; }
        if (showCountdownTimer == undefined || showCountdownTimer == null) { showCountdownTimer = true; }

        if (showCountdownTimer == true) {
            ModalCountdownTimer($("#tsutilsModalDialogTitle").html(), secs, new Date());
        }

        setTimeout(() => ModalDialogHide(), secs * 1000);
    }

    /**
     * A function used internally to show a countdown timer when a modal dialog is being hidden after X number of seconds.
     * @param {string} title - The title of the dialog box at the time the ModalDialogHideOnTimer function was called.
     * @param {number} seconds - The number of seconds that the ModalDialogHideOnTimer function was called with.
     * @param {Date} started - The Date that the ModalDialogHideOnTimer function first called this countdown timer function.
     */
    export function ModalCountdownTimer(title: string, seconds: number, started: Date): void {
        if (ModalDialogIsOpen() == true) {
            // Make sure it's the same dialog title we had before
            let currentTitle: string = $("#tsutilsModalDialogTitle").html();
            if (currentTitle.indexOf(title) > -1) {
                let date: Date = new Date();
                let totalSeconds: number = Math.floor((date.getTime() - started.getTime()) / 1000);
                let remaining: number = seconds - totalSeconds;
                if (remaining > 0) {
                    $("#tsutilsModalDialogTitle").html("<div class='tsutils-closing-countdown'>" + (remaining).toString() + "</div>" + title);
                    setTimeout(() => ModalCountdownTimer(title, seconds, started), 1000);
                }
            }
        }
    }

    /**
     * Indicates if the Modal dialog box is open.
     * @returns {boolean} Returns true if the dialog is currently open.
     */
    export function ModalDialogIsOpen(): boolean {
        return $("#tsutilsModalDialog").is(":visible");
    }

    /**
     * Shows an error message at the top of the Modal dialog box.
     * @param {string} message - The error message to show.
     * @param {boolean} [includeCloseButtons] - Option to include a button to dismiss the error message (defaults to false.)
     */
    export function ModalError(message: string, includeCloseButtons?: boolean): void {
        ModalDialogClearMessages();

        if (includeCloseButtons != undefined && includeCloseButtons != null && includeCloseButtons == true) {
            message = "<div class='tsutils-close-button' id='tsutils-modal-message-close-button'>" +
                "<button class='btn btn-danger btn-sm'><i class='fas fa-times'></i></button></div>" + message;
        }

        $("#tsutilsModalDialogError").html(message);

        $("#tsutils-modal-message-close-button").off('click');
        $("#tsutils-modal-message-close-button").on('click', () => ModalDialogClearMessages());

        $("#tsutilsModalDialogError").show();

        ScrollToTopOfPage("tsutilsModalDialog");
    }

    /**
     * Shows an informational message at the top of the Modal dialog box.
     * @param {string} message - The error message to show.
     * @param {boolean} [includeCloseButtons] - Option to include a button to dismiss the error message (defaults to false.)
     */
    export function ModalMessage(message: string, includeCloseButtons?: boolean): void {
        ModalDialogClearMessages();

        if (includeCloseButtons != undefined && includeCloseButtons != null && includeCloseButtons == true) {
            message = "<div class='tsutils-close-button' id='tsutils-modal-message-close-button'>" +
                "<button class='btn btn-success btn-sm'><i class='fas fa-times'></i></button></div>" + message;
        }

        $("#tsutilsModalDialogMessage").html(message);

        $("#tsutils-modal-message-close-button").off('click');
        $("#tsutils-modal-message-close-button").on('click', () => ModalDialogClearMessages());

        $("#tsutilsModalDialogMessage").show();

        ScrollToTopOfPage("tsutilsModalDialog");
    }

    /**
     * Shows a named modal section and hides all other sections with the "tsutils-modal-section" class. The dialog will be shown without any of the default buttons, so include your own buttons.
     * @param {string} sectionName - The ID of the section element
     * @param {string} [title="Message"] - An optional Modal Dialog title (defaults to "Message")
     * @param {string} [width=""] - Optional string to set the width of the modal in percentage, px, etc. to override the bootstrap default of "max-width:500px;".
     * @param {tsutilsDTO.tsutilsDialogButton[]} buttons - An optional array of buttons to be shown on the dialog. If none are specified only a close button is shown.
     */
    export function ModalPageSection(sectionName: string, title?: string, width?: string, buttons?: tsutilsDTO.tsutilsDialogButton[]) {
        $(".tsutils-modal-section").hide();

        // Make sure the requested element exists
        let exists: boolean = $("#" + sectionName).length > 0;

        if (exists) {
            CreateModalDialogIfMissing();

            // Make sure the requested element is inside of the Modal. If not, move it there.
            if ($("#" + sectionName).parents("#tsutilsModalDialog").length == 0) {

                $("#" + sectionName).appendTo("#tsutilsModalDialog div.modal-body");
            }

            $("#" + sectionName).show();

            //ModalDialog("", title, "hidden", "", null, null, width);
            Modal("", title, width, buttons);

        } else {
            Modal("<div class='alert alert-danger'>The Element &ldquo;" + sectionName + "&rdquo; Does Not Exists</div>", "Error", "", [
                { Text: "OK", ClosesDialog: true, Icon: "auto", Class: "btn btn-danger" }
            ]);
        }
    }

    /**
     * Finds the text inside of a simple XML tag. Intended only for a very simple XML string.
     * @param {string} xml - The XML to search.
     * @param {string} tag - The name of the tag to search for, not including any < or > characters.
     * @returns {string} Returns the text between the start and end tags in the XML.
     */
    export function ParseXML(xml: string, tag: string): string {
        if (xml == undefined || xml == null || xml == "" || tag == undefined || tag == null || tag == "") { return ""; }
        let xmlData: string = xml.toUpperCase();
        let xmlTag: string = tag.toUpperCase();

        let start: number = xmlData.indexOf("<" + xmlTag + ">");
        if (start == -1) { return ""; }

        start = xmlData.indexOf(">", start + 1);
        if (start == -1) { return ""; }

        let end: number = xmlData.indexOf("</" + xmlTag + ">");
        if (end == -1) { return ""; }

        return xml.substring(start + 1, end);
    }

    /**
     * Replaces any spaces in a string with &nbsp; elements.
     * @param {string} text - The text for which you wish to replace spaces.
     * @returns {string} Returns the string with any spaces replaced with &nbsp; characters.
     */
    export function ReplaceSpaces(text: string): string {
        let output: string = "";
        if (text != undefined && text != null && text != "") {
            output = text.replace(/ /g, "&nbsp;");
        }
        return output;
    }

    /**
     * For elements with the "fixed-positon" class resets scrolling back to the top of the element.
     * @param {string} element - The ID of the element.
     */
    export function ResetScrolling(element: string): void {
        $("#" + element + " .tsutils-fixed-position").stop().animate({ "top": "0px" }, "fast");
    }

    /**
     * Rounds a number
     * @param num
     * @param decimals
     */
    export function Round(num: number, decimals: number): number {
        return Math.round(num * Math.pow(10, decimals)) / Math.pow(10, decimals);
    }


    /**
     * Scrolls to the top of an HTML element.
     * @param {string} element - The ID of the HTML element.
     */
    export function ScrollToTopOfPage(element): void {
        $("#" + element).animate({ scrollTop: 0 }, "fast");
    }

    /**
     * Sets any header values to pass along when making ajax calls
     * @param {tsutilsDTO.tsutilsKeyPairValues[]} headerValues - an array of tsutilsDTO.tsutilsKeyPairValues key/pair values
     */
    export function SetAjaxHeaderValues(headerValues?: tsutilsDTO.tsutilsKeyPairValues[]): void {
        if (headerValues != undefined && headerValues != null) {
            window.tsUtilsAjaxHeaderValues = headerValues;
        } else {
            window.tsUtilsAjaxHeaderValues = null;
        }
    }

    /**
     * Sets the security token in the header before making a remote server call.
     * @param xhr (string: The JSON Web Token.)
     */
    export function SetRequestHeaders(xhr) {
        // Always write out a Token as I use that value extensively
        if (HasValue(window.token)) {
            xhr.setRequestHeader('Token', window.token);
        }
        if (HasValue(window.tenantId)) {
            xhr.setRequestHeader('TenantId', window.tenantId);
        }
        if (HasValue(window.tenantCode)) {
            xhr.setRequestHeader('TenantCode', window.tenantCode);
        }

        // Write out any other values that were passed to the SetAjaxHeaderValues function
        if (window.tsUtilsAjaxHeaderValues != undefined && window.tsUtilsAjaxHeaderValues != null) {
            let values: tsutilsDTO.tsutilsKeyPairValues[] = window.tsUtilsAjaxHeaderValues;
            if (values != undefined && values != null) {
                values.forEach(function (value) {
                    xhr.setRequestHeader(value.key, value.value);
                });
            }
        }
    }

    /**
     * Configures scrolling for elements marked with the class tsutils-fixed-position.
     */
    export function SetupScrolling() {
        // Setup scrolling for fixed positioned items on the document
        $(document).off('scroll');
        $(document).scroll(() => {
            let scrollTop: number = $(document).scrollTop();
            if (scrollTop < 80) {
                scrollTop = 0;
            } else {
                scrollTop = scrollTop - 80;
            }
            $(".tsutils-fixed-position:visible").stop().animate({ "top": (scrollTop) + "px" }, "fast");
        });

        // Setup scrolling for fixed positioned items in modals
        $("div.modal").on('scroll', function () {
            let scrollTop: number = $(this).scrollTop();
            if (scrollTop < 80) {
                scrollTop = 0;
            } else {
                scrollTop = scrollTop - 80;
            }
            $(this).find(".tsutils-fixed-position:visible").stop().animate({ "top": (scrollTop) + "px" }, "fast");
        });
    }

    /**
     * Returns a string value. However, if the item is null or undefined then an empty string is returned instead of null.
     * @param {string} text - The text to validate.
     * @returns {string} - Returns only valid text values.
     */
    export function StringValue(text: string): string {
        let output: string = "";

        if (HasValue(text)) {
            output = text;
        }

        return output;
    }

    /**
     * Converts each word in a string of text to have the first letter capitalized, except for special cases which are always lowercase unless they are the first or last word.
     * @param {string} text - The text to convert to title case.
     * @returns {string} - Returns the text in title case.
     */
    export function TitleCase(text: string): string {
        var specialCases = /^(a|an|and|as|at|but|by|en|for|from|if|in|into|near|nor|of|on|onto|or|the|to|with|vs?\.?|via)$/i;
        return text.replace(/([^\W_]+[^\s-]*) */g, function (e, n, r, i) {
            return r > 0 && r + n.length !== i.length && n.search(specialCases) > -1 && i.charAt(r - 2) !== ":" && i.charAt(r - 1).search(/[^\s-]/) < 0 ? e.toLowerCase() : n.substr(1).search(/[A-Z]|\../) > -1 ? e : e.charAt(0).toUpperCase() + e.substr(1)
        }
        );
    }

    /**
     * Gets the overall size of the browser window as an array of numbers in the format of [width, height].
     * @returns {number[]} Returns an array of numbers in the format of [width, height].
     */
    export function WindowSize(): number[] {
        let de: any = document.documentElement;
        let width: number = window.innerWidth || self.innerWidth || (de && de.clientWidth) || document.body.clientWidth;
        let height: number = window.innerHeight || self.innerHeight || (de && de.clientHeight) || document.body.clientHeight;
        return [width, height];
    }
}
//#endregion