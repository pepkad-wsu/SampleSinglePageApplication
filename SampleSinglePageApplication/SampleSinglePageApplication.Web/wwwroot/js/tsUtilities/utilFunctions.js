//#region GlobalFunctions
var tsUtilities;
(function (tsUtilities) {
    /**
     * Adds a given number of days to a date and returns the new date.
     * @param {Date} date - The date to modify.
     * @param {number} days - The number of days to add.
     * @returns {Date} - Returns a date with the specified number of days added (or subtracted if the days were negative.)
     */
    function AddDays(date, days) {
        var result = new Date(date);
        result.setDate(date.getDate() + days);
        return result;
    }
    tsUtilities.AddDays = AddDays;
    /**
     * Adds a given number of minutes to a date and returns the new date.
     * @param {Date} date - The date to modify.
     * @param {number} minutes - The number of minutes to add.
     * @returns {Date} - Returns a date with the specified number of minutes added (or subtracted if the minutes were negative.)
     */
    function AddMinutes(date, minutes) {
        var result = new Date();
        result.setTime(date.getTime() + (minutes * 60000));
        return result;
    }
    tsUtilities.AddMinutes = AddMinutes;
    /**
     * Makes an AJAX call to the specified endpoint. If PostData is provided a POST request will be made, otherwise, a GET request is made.
     * @param {string} Endpoint - The full URL endpoint.
     * @param {any} [PostData] - An optional data object to post to the endpoint.
     * @param {Function} [callbackHandler] - An optional function to handle the callback. Any data returned will be passed to the callback handler.
     * @param {Function} [errorHandler] - An optional function to handle the callback when errors are encountered.
     * @param {Function} [completeHandler] - An optional function to be called on complete, whether there is a success or failure.
     */
    function AjaxData(Endpoint, PostData, callbackHandler, errorHandler, completeHandler) {
        if (PostData != undefined && PostData != null) {
            $.ajax({
                url: Endpoint,
                type: 'POST',
                data: PostData,
                beforeSend: SetRequestHeaders,
                contentType: "application/json",
                success: function (data) {
                    if (callbackHandler != undefined && callbackHandler != null) {
                        callbackHandler(data);
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    if (errorHandler != undefined && errorHandler != null) {
                        errorHandler(xhr, textStatus, errorThrown);
                    }
                    else {
                        console.debug("jQuery AJAX Error", xhr);
                    }
                },
                complete: function (xhr, data) {
                    if (completeHandler != undefined && completeHandler != null) {
                        completeHandler(data);
                    }
                }
            });
        }
        else {
            $.ajax({
                url: Endpoint,
                type: 'GET',
                beforeSend: SetRequestHeaders,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (callbackHandler != undefined && callbackHandler != null) {
                        callbackHandler(data);
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    if (errorHandler != undefined && errorHandler != null) {
                        errorHandler(xhr, textStatus, errorThrown);
                    }
                    else {
                        console.debug("jQuery AJAX Error", xhr);
                    }
                },
                complete: function (data) {
                    if (completeHandler != undefined && completeHandler != null) {
                        completeHandler(data);
                    }
                }
            });
        }
    }
    tsUtilities.AjaxData = AjaxData;
    /**
     * Appends text to a CSV string.
     * @param {string} currentText - The current CSV text to append the new text to.
     * @param {string} appendText - The new text to be appended.
     * @returns {string} Returns the previous text, plus a comma, then the new text, or just the new text if there was no previous text.
     */
    function AppendTextToCSV(currentText, appendText) {
        var output = currentText;
        if (appendText != undefined && appendText != null && appendText != "") {
            if (output != undefined && output != null && output != "") {
                output += ",";
            }
            output += appendText;
        }
        return output;
    }
    tsUtilities.AppendTextToCSV = AppendTextToCSV;
    /**
     * Converts a text input element into a jQuery autocomplete.
     * @param {string} element - The ID of the input element.
     * @param {string} Endpoint - The full URL to the lookup endpoint.
     * @param {Function} itemSelectedCallback - The function that will handle the response when an item is selected.
     */
    function AutoComplete(element, Endpoint, itemSelectedCallback) {
        // Make a call to the AjaxData function to get the data from the endpoint and return the results as
        // an array of server.AjaxResults items
        var responseHandler = function (req, callbackHandler) {
            var search = new tsutilsAjaxLookup();
            search.Search(req.term);
            AjaxData(Endpoint, search, function (data) {
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
                itemSelectedCallback(ui);
            },
            focus: function (event, ui) {
                // When an item in the autocomplete list has focus update the element value
                // to an empty string
                event.preventDefault();
                $("#" + element).val("");
            }
        });
    }
    tsUtilities.AutoComplete = AutoComplete;
    /**
     * Takes a boolean value and if the value is true then returns the HTML of an <i> element with a glypicons checkbox.
     * @param {boolean} value - The boolean value to evaluate.
     * @param {string} [iconTrue] - An optional icon class to use when the value is true instead of the default of "fas fa-check"
     * @param {string} [iconFalse] - An optional icon class to use when the value is false instead of the default of no icon.
     * @returns {string} - Returns either an empty string if false or a string with a icon in an <i> element if true.
     */
    function BooleanToCheckbox(value, iconTrue, iconFalse) {
        var output = "";
        if (!HasValue(iconTrue)) {
            iconTrue = "far fa-check-square";
        }
        if (!HasValue(iconFalse)) {
            iconFalse = "";
        }
        if (value != undefined && value != null && value == true) {
            output = iconTrue.indexOf("<") > -1 ? iconTrue : "<i class=\"" + iconTrue + "\"></i>";
        }
        else {
            output = iconFalse.indexOf("<") > -1 ? iconFalse : "<i class=\"" + iconFalse + "\"></i>";
        }
        return output;
    }
    tsUtilities.BooleanToCheckbox = BooleanToCheckbox;
    /**
     * Converts a file length in bytes to a friendly display.
     * @param {number} bytes - The length of the content in bytes.
     * @param {string[]} [labels] - An optional array to use for the labels to override the defaults of ['b','k','m','g'].
     * @returns {string} - Returns a string formatting the bytes to a friendly view depending on file size.
     */
    function BytesToFileSizeLabel(bytes, labels) {
        var output = "";
        if (bytes != undefined && bytes != null && bytes > 0) {
            if (labels == undefined || labels == null || labels.length < 4) {
                labels = ["bytes", "kb", "meg", "GB"];
            }
            if (bytes < 1024) {
                output = bytes.toFixed(0) + labels[0];
            }
            else if (bytes < (1024 * 1024)) {
                output = (bytes / 1024).toFixed(0) + labels[1];
            }
            else if (bytes < (1024 * 1024 * 1024)) {
                output = (bytes / 1024 / 1024).toFixed(1) + labels[2];
            }
            else if (bytes < (1024 * 1024 * 1024 * 1024)) {
                output = (bytes / 1024 / 1024 / 1024).toFixed(1) + labels[3];
            }
        }
        return output;
    }
    tsUtilities.BytesToFileSizeLabel = BytesToFileSizeLabel;
    /**
     * Displays a confirmation message at the top of a Modal dialog with more controls over the buttons..
     * @param message {string} - The message to be displayed.
     * @param error {boolean} - Optional flag to indicate if the dialog should be styled as an error instead of an informational message.
     * @param {tsutilsDTO.tsutilsConfirmButton[]} buttons - An optional array of buttons to be shown on the dialog. If none are specified only a close button is shown.
     */
    function Confirmation(message, error, buttons) {
        if (buttons == undefined || buttons == null) {
            return;
        }
        var msg = "<div class='tsutils-padbottom'>\n" + message + "\n</div>\n" +
            "<div class='btn-group btn-group-sm' role='group'>\n";
        // Generate any buttons
        buttons.forEach(function (button) {
            if (!HasValue(button.Id)) {
                button.Id = GenerateGuid();
            }
            var content = "";
            var icon = DefaultButtonIcon(button.Text, button.Icon);
            if (HasValue(icon)) {
                content += "<i class='" + icon + "'></i>";
            }
            if (HasValue(button.Text)) {
                if (content != "") {
                    content += " ";
                }
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
        }
        else {
            ModalMessage(msg, false);
        }
        // Add the button callback handlers
        buttons.forEach(function (button) {
            if (HasValue(button.Icon + button.Text)) {
                $("#" + button.Id).off('click');
                var closeDialog = true;
                if (button.ClosesConfirmation != undefined && button.ClosesConfirmation != null && button.ClosesConfirmation == false) {
                    closeDialog = false;
                }
                if (typeof button.CallbackHandler == "function") {
                    if (closeDialog) {
                        $("#" + button.Id).on('click', function () { ModalDialogClearMessages(); button.CallbackHandler(); });
                    }
                    else {
                        $("#" + button.Id).on('click', function () { button.CallbackHandler(); });
                    }
                }
                else if (closeDialog) {
                    $("#" + button.Id).on('click', function () { ModalDialogClearMessages(); });
                }
            }
        });
    }
    tsUtilities.Confirmation = Confirmation;
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
    function ConfirmationMessage(message, okButtonText, okButtonIcon, okButtonClass, okHandler, cancelButtonText, cancelButtonIcon, cancelButtonClass, cancelHandler, error) {
        if (error == undefined || error == null) {
            error = false;
        }
        if (!HasValue(okButtonText)) {
            okButtonText = "OK";
        }
        if (!HasValue(okButtonClass)) {
            okButtonClass = error == true ? "btn btn-danger" : "btn btn-success";
        }
        if (!HasValue(okButtonIcon)) {
            okButtonIcon = "fas fa-check";
        }
        else {
            okButtonIcon = DefaultButtonIcon(okButtonText, okButtonIcon);
        }
        if (!HasValue(cancelButtonText)) {
            cancelButtonText = "Cancel";
        }
        if (!HasValue(cancelButtonClass)) {
            cancelButtonClass = "btn btn-secondary btn-default";
        }
        if (!HasValue(cancelButtonIcon)) {
            cancelButtonIcon = "fas fa-times";
        }
        else {
            cancelButtonIcon = DefaultButtonIcon(cancelButtonText, cancelButtonIcon);
        }
        var msg = "<div class='tsutils-padbottom'>\n" + message + "\n</div>\n" +
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
        }
        else {
            ModalMessage(msg, false);
        }
        $("#tsutils-confirmation-message-ok").off('click');
        $("#tsutils-confirmation-message-ok").click(function () { return ModalDialogClearMessages(); });
        if (okHandler != undefined && okHandler != null) {
            $("#tsutils-confirmation-message-ok").click(function () { return okHandler(); });
        }
        $("#tsutils-confirmation-message-cancel").off('click');
        $("#tsutils-confirmation-message-cancel").click(function () { return ModalDialogClearMessages(); });
        if (cancelHandler != undefined && cancelHandler != null) {
            $("#tsutils-confirmation-message-cancel").click(function () { return cancelHandler(); });
        }
    }
    tsUtilities.ConfirmationMessage = ConfirmationMessage;
    /**
     * Reads a client cookie and returns the value as a string.
     * @param {string} name - The name of the cookie to read.
     * @returns {string} Returns the value stored in the cookie, or an empty string if there was no value.
     */
    function CookieRead(name) {
        var nameEQ = name + "=";
        var cookies = document.cookie.split(';');
        for (var i = 0; i < cookies.length; i++) {
            var cookie = cookies[i];
            while (cookie.charAt(0) == ' ') {
                cookie = cookie.substring(1, cookie.length);
            }
            if (cookie.indexOf(nameEQ) == 0) {
                return cookie.substring(nameEQ.length, cookie.length);
            }
        }
        return "";
    }
    tsUtilities.CookieRead = CookieRead;
    /**
     * Writes a cookie to the client's browser.
     * @param {string} name - The name of the cookie.
     * @param {string} value - The value to store in the cookie.
     * @param {number} [days=30] An optional number of days until the cookie expires. If left empty this defaults to 30 days.
     * @param {string} domain An options cookie domain
     */
    function CookieWrite(name, value, days, domain) {
        if (days == undefined || days == null) {
            days = 30;
        }
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "; expires=" + date.toUTCString();
        var setDomain = "";
        if (HasValue(domain)) {
            setDomain = "; domain=" + domain;
        }
        document.cookie = name + "=" + value + expires + setDomain + "; path=/";
    }
    tsUtilities.CookieWrite = CookieWrite;
    /**
    * Function used internally to create the tsutilsModalDialog if it has not been added to the page.
    */
    function CreateModalDialogIfMissing() {
        // Make sure the modal dialog element exists
        var exists = $("#tsutilsModalDialog").length > 0;
        if (!exists) {
            // Missing the modal dialog, so create it
            var dialog = '<div class="modal-dialog" id="tsutilsModalDialogBox">' +
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
            var dialogContent = document.createElement("div");
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
    function DaysBetween(date1, date2) {
        var output = 0;
        if (date1 != null && date2 != null) {
            var oneDay = 1000 * 60 * 60 * 24;
            // Always just check for the day, not the hour.
            var d1 = new Date(date1);
            var d2 = new Date(date2);
            var compareDate1 = new Date(d1.getFullYear(), d1.getMonth(), d1.getDate());
            var compareDate2 = new Date(d2.getFullYear(), d2.getMonth(), d2.getDate());
            output = Math.ceil((compareDate1.getTime() - compareDate2.getTime()) / oneDay);
        }
        return output;
    }
    tsUtilities.DaysBetween = DaysBetween;
    /**
     * Function used internally to try and find a button icon for buttons with the class set to "auto"
     * @param {string} ButtonText - The text of the button
     * @param {string} CurrentIcon - The current icon value
     */
    function DefaultButtonIcon(ButtonText, CurrentIcon) {
        var output = "";
        var auto = false;
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
                var buttonText = ButtonText.toLowerCase();
                if (buttonText.indexOf("delete ") == 0 || buttonText.indexOf(" delete ") > -1) {
                    output = "fas fa-trash";
                }
                else if (buttonText.indexOf("edit ") == 0 || buttonText.indexOf(" edit ") > -1) {
                    output = "fas fa-edit";
                }
                else if (buttonText.indexOf("save ") == 0 || buttonText.indexOf(" save ") > -1) {
                    output = "far fa-save";
                }
                else if (buttonText.indexOf("add ") == 0 || buttonText.indexOf(" add ") > -1) {
                    output = "fas fa-plus-circle";
                }
                else if (buttonText.indexOf("yes ") == 0 || buttonText.indexOf(" yes ") > -1) {
                    output = "fas fa-check";
                }
                else if (buttonText.indexOf("no ") == 0 || buttonText.indexOf(" no ") > -1) {
                    output = "fas fa-times";
                }
                else if (buttonText.indexOf("print ") == 0 || buttonText.indexOf(" print ") > -1) {
                    output = "fas fa-print";
                }
                else if (buttonText.indexOf("submit ") == 0 || buttonText.indexOf(" submit ") > -1) {
                    output = "fas fa-file-export";
                }
                else if (buttonText.indexOf("help ") == 0 || buttonText.indexOf(" help ") > -1) {
                    output = "fas fa-question-circle";
                }
                else if (buttonText.indexOf("users ") == 0 || buttonText.indexOf(" users ") > -1) {
                    output = "fas fa-users";
                }
                else if (buttonText.indexOf("uncheck ") == 0 || buttonText.indexOf(" uncheck ") > -1) {
                    output = "far fa-square";
                }
                else if (buttonText.indexOf("check ") == 0 || buttonText.indexOf(" check ") > -1) {
                    output = "far fa-check-square";
                }
                else if (buttonText.indexOf("toggle ") == 0 || buttonText.indexOf(" toggle ") > -1) {
                    output = "fas fa-toggle-off";
                }
                else if (buttonText.indexOf("hide ") == 0 || buttonText.indexOf(" hide ") > -1) {
                    output = "far fa-eye-slash";
                }
                else if (buttonText.indexOf("show ") == 0 || buttonText.indexOf(" show ") > -1) {
                    output = "far fa-eye";
                }
                else if (buttonText.indexOf("list ") == 0 || buttonText.indexOf("lists ") == 0 || buttonText.indexOf(" list ") > -1 || buttonText.indexOf(" lists ") > -1) {
                    output = "far fa-list-alt";
                }
                else if (buttonText.indexOf("image ") == 0 || buttonText.indexOf(" image ") > -1) {
                    output = "far fa-image";
                }
                else if (buttonText.indexOf("images ") == 0 || buttonText.indexOf(" images ") > -1) {
                    output = "far fa-images";
                }
                else if (buttonText.indexOf("tag ") == 0 || buttonText.indexOf(" tag ") > -1) {
                    output = "fas fa-tag";
                }
                else if (buttonText.indexOf("tags ") == 0 || buttonText.indexOf(" tags ") > -1) {
                    output = "fas fa-tags";
                }
                else if (buttonText.indexOf("toggle ") == 0 || buttonText.indexOf(" toggle ") > -1) {
                    output = "fas fa-toggle-off";
                }
                else if (buttonText.indexOf("student ") == 0 || buttonText.indexOf("students ") == 0 || buttonText.indexOf(" student ") > -1 || buttonText.indexOf(" students ") > -1) {
                    output = "fas fa-user-graduate";
                }
            }
        }
        else if (HasValue(CurrentIcon)) {
            output += CurrentIcon;
        }
        return output;
    }
    /**
     * Sets the focus to an element as soon as it's visible. Some things like Modal dialogs have a delay in showing, so this allows the focus to be set as soon as the item becomes visible.
     * @param {string} element - The ID of the HTML element.
     * @param {number} [safety] Leave empty, this is used internally to make sure only a few attempts are made to show the item.
     */
    function DelayedFocus(element, safety) {
        if (safety == undefined || safety == null) {
            safety = 0;
        }
        if (safety > 20) {
            return;
        }
        safety++;
        if ($("#" + element).is(":visible") == true) {
            $("#" + element).focus();
        }
        else {
            setTimeout(function () { return DelayedFocus(element, safety); }, 20);
        }
    }
    tsUtilities.DelayedFocus = DelayedFocus;
    /**
     * Sets the focus to an input element and selects the content as soon as it's visible. Some things like Modal dialogs have a delay in showing, so this allows the focus to be set as soon as the item becomes visible.
     * @param {string} element - The ID of the HTML element.
     * @param {number} [safety] Leave empty, this is used internally to make sure only a few attempts are made to show the item.
     */
    function DelayedSelect(element, safety) {
        if (safety == undefined || safety == null) {
            safety = 0;
        }
        if (safety > 20) {
            return;
        }
        safety++;
        if ($("#" + element).is(":visible") == true) {
            $("#" + element).select();
        }
        else {
            setTimeout(function () { return DelayedSelect(element, safety); }, 20);
        }
    }
    tsUtilities.DelayedSelect = DelayedSelect;
    /**
     * Converts an array of strings into an error message.
     * @param {string[]} errors - The array of strings containing one or more error messages.
     * @param {string} noticeText - Optional text to use to show before the errors. If not specified then "<strong>Please correct the following error{s}</strong>:" is used. The {s} in the string will be removed for a single result and converted to an "s" for multiple result.
     * @returns {string} Returns the message formatted as an error message.
     */
    function ErrorMessage(errors, noticeText) {
        var output = "";
        if (errors != undefined && errors != null && errors.length > 0) {
            if (!HasValue(noticeText)) {
                noticeText = "<strong>Please correct the following error{s}</strong>:";
            }
            if (errors.length == 1) {
                noticeText = noticeText.replace("{s}", "");
                output = "<div class='tsutils-padbottom'>" + noticeText + "</div>\n" +
                    "<div>" + errors[0] + "</div>";
            }
            else {
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
    tsUtilities.ErrorMessage = ErrorMessage;
    /**
     * Finds the text between two different strings. Useful for finding text between tags in XML, etc.
     * @param {string} searchIn - The string to be searched.
     * @param {string} start - The text to find for the beginning of the search.
     * @param {string} end - The text that marks the end of the search.
     * @returns {string} Returns the text found between the start and end.
     */
    function FindTextBetween(searchIn, start, end) {
        var output = "";
        if (searchIn != undefined && searchIn != null && searchIn != "" && start != undefined && start != null && start != "" && end != undefined && end != null && end != "") {
            var s = searchIn.toLowerCase().indexOf(start.toLowerCase());
            if (s > -1) {
                var e = searchIn.toLowerCase().indexOf(end.toLowerCase(), s + 1);
                if (e > -1) {
                    output = searchIn.substr(s, e - s + 1);
                }
            }
        }
        return output;
    }
    tsUtilities.FindTextBetween = FindTextBetween;
    /**
     * Formats a number as currency with a dollar sign and two decimal places.
     * @param {number} amount - The number to be formatted as currency.
     * @param {string} [currencySymbol] - Optionally pass in the currency symbol (defaults to $.)
     * @returns {string} Returns the number formatted as currency.
     */
    function FormatCurrency(amount, currencySymbol) {
        if (currencySymbol == undefined || currencySymbol == null || currencySymbol == "") {
            currencySymbol = "$";
        }
        var output = currencySymbol + (0).toFixed(2);
        if (amount != undefined && amount != null && !isNaN(amount)) {
            output = currencySymbol + amount.toFixed(2);
        }
        return output;
    }
    tsUtilities.FormatCurrency = FormatCurrency;
    /**
     * Formats a date in the "M/D/YYYY" format.
     * @param {Date} date - The date to be formatted.
     * @returns {string} Returns the date formatted as "M/D/YYYY"
     */
    function FormatDate(date) {
        var output = "";
        if (date != undefined && date != null) {
            output = moment(date).format("M/D/YYYY");
        }
        return output;
    }
    tsUtilities.FormatDate = FormatDate;
    /**
     * Formats a date in the "M/D/YYYY h:mm a" format.
     * @param date (Date: The date to be formatted.)
     * @returns {string} Returns the date formatted as "M/D/YYYY h:mm a"
     */
    function FormatDateTime(date) {
        var output = "";
        if (date != undefined && date != null) {
            output = moment(date).format("M/D/YYYY h:mm a");
        }
        return output;
    }
    tsUtilities.FormatDateTime = FormatDateTime;
    /**
     * Formats a date in the "M/D/YYYY h:mm:ss a" format.
     * @param date (Date: The date to be formatted.)
     * @returns {string} Returns the date formatted as "M/D/YYYY h:mm:ss a"
     */
    function FormatDateTimeLong(date) {
        var output = "";
        if (date != undefined && date != null) {
            output = moment(date).format("M/D/YYYY h:mm:ss a");
        }
        return output;
    }
    tsUtilities.FormatDateTimeLong = FormatDateTimeLong;
    /**
     * Formats a date in the "h:mm a" format.
     * @param date (Date: The date to be formatted.)
     * @returns {string} Returns the date formatted as "h:mm a"
    */
    function FormatTime(date) {
        var output = "";
        if (date != undefined && date != null) {
            output = moment(date).format("h:mm a");
        }
        return output;
    }
    tsUtilities.FormatTime = FormatTime;
    /**
     * Formats a date in the "h:mm:ss a" format.
     * @param {Date} date - The date to be formatted.
     * @returns {string} - Returns the date formatted as "h:mm:ss a"
     */
    function FormatTimeLong(date) {
        var output = "";
        if (date != undefined && date != null) {
            output = moment(date).format("h:mm:ss a");
        }
        return output;
    }
    tsUtilities.FormatTimeLong = FormatTimeLong;
    /**
     * Generates a new GUID.
     * @returns {string} Returns a string containing a new GUID.
     */
    function GenerateGuid() {
        var d = new Date().getTime();
        var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
        return uuid;
    }
    tsUtilities.GenerateGuid = GenerateGuid;
    /**
     * Gets the extension from a file name (does not include the leading period.)
     * @param {string} file - The filename from which you wish to get the extension.
     * @returns {string} Returns the extension with no leading period.
     */
    function GetExtension(file) {
        var output = "";
        var re = /(?:\.([^.]+))?$/;
        var ext = "." + re.exec(file)[1];
        if (ext != ".") {
            ext = ext.replace(".", "");
            output = ext;
        }
        return output;
    }
    tsUtilities.GetExtension = GetExtension;
    /**
     * Returns an empty GUID (00000000-0000-0000-0000-000000000000).
     * @returns {string} Returns an empty GUID (00000000-0000-0000-0000-000000000000).
     */
    function GuidEmpty() {
        return "00000000-0000-0000-0000-000000000000";
    }
    tsUtilities.GuidEmpty = GuidEmpty;
    /**
     * Tests to see if the number passed in contains a value.
     * @param {any} check - The number to check for a value.
     * @returns {boolean} Returns true if the item passed in contained a value.
     */
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
    tsUtilities.HasNumericalValue = HasNumericalValue;
    /**
     * Determines if the string passed in contains a value and is not "undefined", null, or an empty string.
     * @param {string} check - The string to check for a value.
     * @returns {boolean} Returns true if the item passed in is not "undefined", not null, and not an empty string.
     */
    function HasValue(check) {
        return check != undefined && check != null && check != "";
    }
    tsUtilities.HasValue = HasValue;
    function HtmlEditor(element, config) {
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
        if (!tsUtilities.HasValue(config.html)) {
            config.html = "";
        }
        if (!tsUtilities.HasValue(config.placeholderText)) {
            config.placeholderText = "HTML Editor";
        }
        if (config.hideSourceButton == undefined || config.hideSourceButton == null) {
            config.hideSourceButton = false;
        }
        if (config.required == undefined || config.required == null) {
            config.required = false;
        }
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
        }
        else {
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
                        }
                        else {
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
    tsUtilities.HtmlEditor = HtmlEditor;
    function HtmlEditorDestroy(element) {
        if ($("#" + element).length == 0) {
            return;
        }
        if (CKEDITOR.instances[element] != undefined) {
            CKEDITOR.instances[element].destroy();
        }
    }
    tsUtilities.HtmlEditorDestroy = HtmlEditorDestroy;
    function HtmlEditorExists(element) {
        var output = false;
        if (CKEDITOR.instances[element] != undefined) {
            output = true;
        }
        return output;
    }
    tsUtilities.HtmlEditorExists = HtmlEditorExists;
    function HtmlEditorFocus(element, safety) {
        if (safety == undefined || safety == null) {
            safety = 1;
        }
        if (safety > 50) {
            return;
        }
        safety++;
        if ($("#" + element).nextUntil("div.cke").andSelf().last().next().is(":visible")) {
            if (CKEDITOR.instances[element] != undefined) {
                setTimeout(function () { return CKEDITOR.instances[element].focus(); }, 100);
            }
            else {
                setTimeout(function () { return HtmlEditorFocus(element, safety); }, 20);
            }
        }
        else {
            setTimeout(function () { return HtmlEditorFocus(element, safety); }, 20);
        }
    }
    tsUtilities.HtmlEditorFocus = HtmlEditorFocus;
    function HtmlEditorInsertText(element, text, asHtml) {
        if (CKEDITOR.instances[element] != undefined) {
            if (asHtml != undefined && asHtml != null && asHtml == true) {
                CKEDITOR.instances[element].insertHtml(text);
            }
            else {
                CKEDITOR.instances[element].insertText(text);
            }
        }
    }
    tsUtilities.HtmlEditorInsertText = HtmlEditorInsertText;
    function HtmlEditorUpdate(element, html) {
        if (CKEDITOR.instances[element] != undefined) {
            CKEDITOR.instances[element].setData(html);
        }
    }
    tsUtilities.HtmlEditorUpdate = HtmlEditorUpdate;
    /**
     * Sets up Knockout Custom Binding Handlers to support HTML binding, date (datePicker), and dateTime (dateTimePicker)
     */
    function KnockoutCustomBindingHandlers() {
        eval("moment.tz.guess();");
        ko.bindingHandlers.dateTime = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                ko.utils.registerEventHandler(element, 'change', function () {
                    var value = valueAccessor();
                    if (element != undefined && element != null && element.value != undefined && element.value != null && element.value.length > 0) {
                        // To make sure the date includes the correct Timezone information for posting to an API controller
                        // it needs to be converted to a correct full date instead of just the M/D/YYY h:mm a format.
                        var correctDate = new Date(moment(element.value).format("M/D/YYYY h:mm a"));
                        value(correctDate);
                    }
                    else {
                        value(null);
                    }
                });
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                var value = valueAccessor();
                var valueUnwrapped = ko.utils.unwrapObservable(value);
                var output = '';
                if (valueUnwrapped != undefined && valueUnwrapped != null && element != undefined && element != null && element.value != undefined && element.value != null) {
                    output = moment(valueUnwrapped).format("M/D/YYYY h:mm a");
                }
                if ($(element).is('input') === true) {
                    $(element).val(output);
                }
                else {
                    $(element).text(output);
                }
            }
        };
        ko.bindingHandlers.date = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                ko.utils.registerEventHandler(element, 'change', function () {
                    var value = valueAccessor();
                    if (element != undefined && element != null && element.value != undefined && element.value != null && element.value.length > 0) {
                        // Converts the date string back to a date object.
                        var correctDate = new Date(moment(element.value).format("M/D/YYYY"));
                        value(correctDate);
                    }
                    else {
                        value(null);
                    }
                });
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                var value = valueAccessor();
                var valueUnwrapped = ko.utils.unwrapObservable(value);
                var output = '';
                if (valueUnwrapped != undefined && valueUnwrapped != null && element != undefined && element != null && element.value != undefined && element.value != null) {
                    //output = FormatDate(valueUnwrapped);
                    output = moment(valueUnwrapped).format("M/D/YYYY");
                }
                if ($(element).is('input') === true) {
                    $(element).val(output);
                }
                else {
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
        };
        ko.bindingHandlers.selectize = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
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
                };
                if (allBindingsAccessor.has('options')) {
                    var passed_options = allBindingsAccessor.get('options');
                    for (var attr_name in passed_options) {
                        options[attr_name] = passed_options[attr_name];
                    }
                }
                var $select = $(element).selectize(options)[0].selectize;
                if (typeof allBindingsAccessor.get('value') == 'function') {
                    $select.addItem(allBindingsAccessor.get('value')());
                    allBindingsAccessor.get('value').subscribe(function (new_val) {
                        $select.addItem(new_val);
                    });
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
                                    if (itemId != null)
                                        $select.removeOption(itemId);
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
                            var id = i[optionsValue];
                        else
                            var id = i[optionsValue];
                        return id == allBindingsAccessor.get('value')();
                    })[0];
                    if (selected_obj) {
                        allBindingsAccessor.get('object')(selected_obj);
                    }
                }
            }
        };
    }
    tsUtilities.KnockoutCustomBindingHandlers = KnockoutCustomBindingHandlers;
    function LastDayOfMonth(date) {
        var startOfMonth = new Date(date.getFullYear(), date.getMonth(), 1);
        var nextMonth = AddDays(startOfMonth, 35);
        var firstOfNextMonth = new Date(nextMonth.getFullYear(), nextMonth.getMonth(), 1);
        var output = AddDays(firstOfNextMonth, -1);
        return output;
    }
    tsUtilities.LastDayOfMonth = LastDayOfMonth;
    /**
     * Converts an array of strings to a message. If a single string is in the array only the text is returned. If multiple items are in the array an <ul> element is returned with <li> elements for each text item.
     * @param {string[]} messages - An array of strings.
     * @returns {string} Returns a single string if the array only contained one string, or a <ul> with <li> elements for each string.
     */
    function MessagesToString(messages) {
        var output = "";
        if (messages != undefined && messages != null && messages.length > 0) {
            if (messages.length == 1) {
                output = messages[0];
            }
            else {
                output = "<ul>\n";
                messages.forEach(function (msg) {
                    output += "  <li>" + msg + "</li>\n";
                });
                output += "</ul>\n";
            }
        }
        return output;
    }
    tsUtilities.MessagesToString = MessagesToString;
    function MinutesToDaysHoursAndMinutes(minutes) {
        var output = "none";
        if (minutes > 0) {
            var years = 0;
            var days = 0;
            var hours = 0;
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
                if (output != "") {
                    output += ", ";
                }
                output += days.toString() + " day" + (days > 1 ? "s" : "");
            }
            if (hours > 0) {
                if (output != "") {
                    output += ", ";
                }
                output += hours.toString() + " hour" + (hours > 1 ? "s" : "");
            }
            if (minutes > 0) {
                if (output != "") {
                    output += ", ";
                }
                output += minutes.toString() + " minute" + (minutes > 1 ? "s" : "");
            }
        }
        return output;
    }
    tsUtilities.MinutesToDaysHoursAndMinutes = MinutesToDaysHoursAndMinutes;
    function MinutesToHoursAndMinutes(minutes) {
        var output = "none";
        if (minutes > 0) {
            if (minutes > 59) {
                var hours = Math.floor(minutes / 60);
                minutes = minutes - (60 * hours);
                output = hours.toString() + " hour" + (hours > 1 ? "s" : "");
                if (minutes > 0) {
                    output += ", " + minutes.toString() + " minute" + (minutes > 1 ? "s" : "");
                }
            }
            else {
                output = minutes.toString() + " minute" + (minutes > 1 ? "s" : "");
            }
        }
        return output;
    }
    tsUtilities.MinutesToHoursAndMinutes = MinutesToHoursAndMinutes;
    /**
     * Another Modal Dialog option with more controls over the buttons.
     * @param {string} message - The message to show.
     * @param {string} [title] - An optional title for the dialog (defaults to "Message".)
     * @param {string} [width=""] - Optional string to set the width of the modal in percentage, px, etc. to override the bootstrap default of "max-width:500px;".
     * @param {tsutilsDTO.tsutilsDialogButton[]} buttons - An optional array of buttons to be shown on the dialog. If none are specified only a close button is shown.
     */
    function Modal(message, title, width, buttons) {
        if (!HasValue(title)) {
            title = "Message";
        }
        if (!HasValue(width)) {
            width = "";
        }
        CreateModalDialogIfMissing();
        $("#tsutilsModalExtraButtons").html("");
        ResetScrolling("tsutilsModalDialog");
        // Generate any buttons
        var buttonsTop = "";
        var buttonsTopFixed = "";
        var buttonsBottom = "";
        if (buttons != undefined && buttons != null) {
            var buttonGroups_1 = [];
            var buildButton_1 = function (buttonClass, buttonId, content) {
                return "<button type='button' class='" + (HasValue(buttonClass) ? buttonClass : "btn btn-secondary") + "' id='" + buttonId + "'>" + content + "</button>\n";
            };
            // Make sure buttons all have a defined position
            buttons.forEach(function (button) {
                if (!HasValue(button.Position)) {
                    button.Position = "bottom";
                }
                if (!HasValue(button.Id)) {
                    button.Id = GenerateGuid();
                }
            });
            buttons.forEach(function (button) {
                var content = "";
                var icon = DefaultButtonIcon(button.Text, button.Icon);
                if (HasValue(icon)) {
                    content += "<i class='" + icon + "'></i>";
                }
                if (HasValue(button.Text)) {
                    if (content != "") {
                        content += " ";
                    }
                    content += button.Text;
                }
                var position = button.Position;
                var buttonGroup = button.GroupName;
                if (HasValue(content)) {
                    var b = buildButton_1(button.Class, button.Id, content);
                    if (HasValue(buttonGroup)) {
                        // In a group, so process if this group hasn't already been processed
                        b = "";
                        if (buttonGroups_1.indexOf(buttonGroup + "_" + position) == -1) {
                            buttonGroups_1.push(buttonGroup + "_" + position);
                            // Find all buttons in this same grouping for the same position.
                            for (var x = 0; x < buttons.length; x++) {
                                var checkButton = buttons[x];
                                if (checkButton.GroupName == buttonGroup && checkButton.Position == position) {
                                    content = "";
                                    icon = DefaultButtonIcon(checkButton.Text, checkButton.Icon);
                                    if (HasValue(icon)) {
                                        content += "<i class='" + icon + "'></i>";
                                    }
                                    if (HasValue(checkButton.Text)) {
                                        if (content != "") {
                                            content += " ";
                                        }
                                        content += checkButton.Text;
                                    }
                                    b += buildButton_1(checkButton.Class, checkButton.Id, content);
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
        }
        else {
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
        }
        else {
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
                    var closeDialog = true;
                    if (button.ClosesDialog != undefined && button.ClosesDialog != null && button.ClosesDialog == false) {
                        closeDialog = false;
                    }
                    if (typeof button.CallbackHandler == "function") {
                        if (closeDialog) {
                            $("#" + button.Id).on('click', function () { ModalDialogHide(); button.CallbackHandler(); });
                        }
                        else {
                            $("#" + button.Id).on('click', function () { button.CallbackHandler(); });
                        }
                    }
                    else if (closeDialog) {
                        $("#" + button.Id).on('click', function () { ModalDialogHide(); });
                    }
                }
            });
        }
    }
    tsUtilities.Modal = Modal;
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
    function ModalDialog(message, title, okButtonText, okButtonIcon, okHandler, cancelHandler, width, closeOnOk) {
        if (title == undefined || title == null || title == "") {
            title = "Message";
        }
        if (!HasValue(okButtonText)) {
            okButtonText = "";
        }
        if (!HasValue(okButtonIcon)) {
            okButtonIcon = "fas fa-check";
        }
        else {
            okButtonIcon = DefaultButtonIcon(okButtonText, okButtonIcon);
        }
        if (!HasValue(width)) {
            width = "";
        }
        if (closeOnOk == undefined || closeOnOk == null) {
            closeOnOk = true;
        }
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
            $("#tsutilsModalDialogOkButton").addClass("btn btn-danger");
        }
        else {
            $("#tsutilsModalDialogOkButton").removeClass();
            $("#tsutilsModalDialogOkButton").addClass("btn btn-success");
        }
        if (width == "") {
            $("#tsutilsModalDialogBox").css("max-width", "");
            $("#tsutilsModalDialogBox").css("width", "");
        }
        else {
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
        }
        else if (okButtonText == "hidden") {
            $("#tsutilsModalDialogOkButton").hide();
            $("#tsutilsModalDialogCancelButton").hide();
        }
        else {
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
                $("#tsutilsModalDialogOkButton").on('click', function () { return ModalDialogHide(); });
                $("#tsutilsModalDialogOkButton").click(function () { return okHandler(); });
            }
            else {
                $("#tsutilsModalDialogOkButton").click(function () { return okHandler(); });
            }
        }
        else {
            $("#tsutilsModalDialogOkButton").on('click', function () { return ModalDialogHide(); });
        }
        // setup click event for the cancel button
        $("#tsutilsModalDialogCancelButton").off('click');
        if (cancelHandler != undefined && cancelHandler != null) {
            $("#tsutilsModalDialogCancelButton").click(ModalDialogHide);
            $("#tsutilsModalDialogCancelButton").click(function () { return cancelHandler(); });
        }
        else {
            $("#tsutilsModalDialogCancelButton").click(ModalDialogHide);
        }
    }
    tsUtilities.ModalDialog = ModalDialog;
    /**
     * Clears any messages showing at the top of the Modal dialog box.
     */
    function ModalDialogClearMessages() {
        $("#tsutilsModalDialogError").html("");
        $("#tsutilsModalDialogError").hide();
        $("#tsutilsModalDialogMessage").html("");
        $("#tsutilsModalDialogMessage").hide();
    }
    tsUtilities.ModalDialogClearMessages = ModalDialogClearMessages;
    /**
     * Hides the Modal dialog box.
     */
    function ModalDialogHide() {
        // Hide the dialog box, clear any previous message, and hide any previously-shown "tsutils-modal-section" elements.
        $("#tsutilsModalDialog").modal("hide");
        $("#tsutilsModalDialogBody").html("");
        $(".tsutils-modal-section").hide();
    }
    tsUtilities.ModalDialogHide = ModalDialogHide;
    /**
     * Hides the Modal dialog box after a specified number of seconds.
     * @param {number} [secs=3] - The number of seconds until the dialog hides (defaults to 3.)
     * @param {boolean} [showCountdownTimer=false] - Option to show a countdown timer in the title area (defaults to true.)
     */
    function ModalDialogHideOnTimer(secs, showCountdownTimer) {
        if (secs == undefined || secs == null || secs < 1) {
            secs = 3;
        }
        if (showCountdownTimer == undefined || showCountdownTimer == null) {
            showCountdownTimer = true;
        }
        if (showCountdownTimer == true) {
            ModalCountdownTimer($("#tsutilsModalDialogTitle").html(), secs, new Date());
        }
        setTimeout(function () { return ModalDialogHide(); }, secs * 1000);
    }
    tsUtilities.ModalDialogHideOnTimer = ModalDialogHideOnTimer;
    /**
     * A function used internally to show a countdown timer when a modal dialog is being hidden after X number of seconds.
     * @param {string} title - The title of the dialog box at the time the ModalDialogHideOnTimer function was called.
     * @param {number} seconds - The number of seconds that the ModalDialogHideOnTimer function was called with.
     * @param {Date} started - The Date that the ModalDialogHideOnTimer function first called this countdown timer function.
     */
    function ModalCountdownTimer(title, seconds, started) {
        if (ModalDialogIsOpen() == true) {
            // Make sure it's the same dialog title we had before
            var currentTitle = $("#tsutilsModalDialogTitle").html();
            if (currentTitle.indexOf(title) > -1) {
                var date = new Date();
                var totalSeconds = Math.floor((date.getTime() - started.getTime()) / 1000);
                var remaining = seconds - totalSeconds;
                if (remaining > 0) {
                    $("#tsutilsModalDialogTitle").html("<div class='tsutils-closing-countdown'>" + (remaining).toString() + "</div>" + title);
                    setTimeout(function () { return ModalCountdownTimer(title, seconds, started); }, 1000);
                }
            }
        }
    }
    tsUtilities.ModalCountdownTimer = ModalCountdownTimer;
    /**
     * Indicates if the Modal dialog box is open.
     * @returns {boolean} Returns true if the dialog is currently open.
     */
    function ModalDialogIsOpen() {
        return $("#tsutilsModalDialog").is(":visible");
    }
    tsUtilities.ModalDialogIsOpen = ModalDialogIsOpen;
    /**
     * Shows an error message at the top of the Modal dialog box.
     * @param {string} message - The error message to show.
     * @param {boolean} [includeCloseButtons] - Option to include a button to dismiss the error message (defaults to false.)
     */
    function ModalError(message, includeCloseButtons) {
        ModalDialogClearMessages();
        if (includeCloseButtons != undefined && includeCloseButtons != null && includeCloseButtons == true) {
            message = "<div class='tsutils-close-button' id='tsutils-modal-message-close-button'>" +
                "<button class='btn btn-danger btn-sm'><i class='fas fa-times'></i></button></div>" + message;
        }
        $("#tsutilsModalDialogError").html(message);
        $("#tsutils-modal-message-close-button").off('click');
        $("#tsutils-modal-message-close-button").on('click', function () { return ModalDialogClearMessages(); });
        $("#tsutilsModalDialogError").show();
        ScrollToTopOfPage("tsutilsModalDialog");
    }
    tsUtilities.ModalError = ModalError;
    /**
     * Shows an informational message at the top of the Modal dialog box.
     * @param {string} message - The error message to show.
     * @param {boolean} [includeCloseButtons] - Option to include a button to dismiss the error message (defaults to false.)
     */
    function ModalMessage(message, includeCloseButtons) {
        ModalDialogClearMessages();
        if (includeCloseButtons != undefined && includeCloseButtons != null && includeCloseButtons == true) {
            message = "<div class='tsutils-close-button' id='tsutils-modal-message-close-button'>" +
                "<button class='btn btn-success btn-sm'><i class='fas fa-times'></i></button></div>" + message;
        }
        $("#tsutilsModalDialogMessage").html(message);
        $("#tsutils-modal-message-close-button").off('click');
        $("#tsutils-modal-message-close-button").on('click', function () { return ModalDialogClearMessages(); });
        $("#tsutilsModalDialogMessage").show();
        ScrollToTopOfPage("tsutilsModalDialog");
    }
    tsUtilities.ModalMessage = ModalMessage;
    /**
     * Shows a named modal section and hides all other sections with the "tsutils-modal-section" class. The dialog will be shown without any of the default buttons, so include your own buttons.
     * @param {string} sectionName - The ID of the section element
     * @param {string} [title="Message"] - An optional Modal Dialog title (defaults to "Message")
     * @param {string} [width=""] - Optional string to set the width of the modal in percentage, px, etc. to override the bootstrap default of "max-width:500px;".
     * @param {tsutilsDTO.tsutilsDialogButton[]} buttons - An optional array of buttons to be shown on the dialog. If none are specified only a close button is shown.
     */
    function ModalPageSection(sectionName, title, width, buttons) {
        $(".tsutils-modal-section").hide();
        // Make sure the requested element exists
        var exists = $("#" + sectionName).length > 0;
        if (exists) {
            CreateModalDialogIfMissing();
            // Make sure the requested element is inside of the Modal. If not, move it there.
            if ($("#" + sectionName).parents("#tsutilsModalDialog").length == 0) {
                $("#" + sectionName).appendTo("#tsutilsModalDialog div.modal-body");
            }
            $("#" + sectionName).show();
            //ModalDialog("", title, "hidden", "", null, null, width);
            Modal("", title, width, buttons);
        }
        else {
            Modal("<div class='alert alert-danger'>The Element &ldquo;" + sectionName + "&rdquo; Does Not Exists</div>", "Error", "", [
                { Text: "OK", ClosesDialog: true, Icon: "auto", Class: "btn btn-danger" }
            ]);
        }
    }
    tsUtilities.ModalPageSection = ModalPageSection;
    /**
     * Finds the text inside of a simple XML tag. Intended only for a very simple XML string.
     * @param {string} xml - The XML to search.
     * @param {string} tag - The name of the tag to search for, not including any < or > characters.
     * @returns {string} Returns the text between the start and end tags in the XML.
     */
    function ParseXML(xml, tag) {
        if (xml == undefined || xml == null || xml == "" || tag == undefined || tag == null || tag == "") {
            return "";
        }
        var xmlData = xml.toUpperCase();
        var xmlTag = tag.toUpperCase();
        var start = xmlData.indexOf("<" + xmlTag + ">");
        if (start == -1) {
            return "";
        }
        start = xmlData.indexOf(">", start + 1);
        if (start == -1) {
            return "";
        }
        var end = xmlData.indexOf("</" + xmlTag + ">");
        if (end == -1) {
            return "";
        }
        return xml.substring(start + 1, end);
    }
    tsUtilities.ParseXML = ParseXML;
    /**
     * Replaces any spaces in a string with &nbsp; elements.
     * @param {string} text - The text for which you wish to replace spaces.
     * @returns {string} Returns the string with any spaces replaced with &nbsp; characters.
     */
    function ReplaceSpaces(text) {
        var output = "";
        if (text != undefined && text != null && text != "") {
            output = text.replace(/ /g, "&nbsp;");
        }
        return output;
    }
    tsUtilities.ReplaceSpaces = ReplaceSpaces;
    /**
     * For elements with the "fixed-positon" class resets scrolling back to the top of the element.
     * @param {string} element - The ID of the element.
     */
    function ResetScrolling(element) {
        $("#" + element + " .tsutils-fixed-position").stop().animate({ "top": "0px" }, "fast");
    }
    tsUtilities.ResetScrolling = ResetScrolling;
    /**
     * Rounds a number
     * @param num
     * @param decimals
     */
    function Round(num, decimals) {
        return Math.round(num * Math.pow(10, decimals)) / Math.pow(10, decimals);
    }
    tsUtilities.Round = Round;
    /**
     * Scrolls to the top of an HTML element.
     * @param {string} element - The ID of the HTML element.
     */
    function ScrollToTopOfPage(element) {
        $("#" + element).animate({ scrollTop: 0 }, "fast");
    }
    tsUtilities.ScrollToTopOfPage = ScrollToTopOfPage;
    /**
     * Sets any header values to pass along when making ajax calls
     * @param {tsutilsDTO.tsutilsKeyPairValues[]} headerValues - an array of tsutilsDTO.tsutilsKeyPairValues key/pair values
     */
    function SetAjaxHeaderValues(headerValues) {
        if (headerValues != undefined && headerValues != null) {
            window.tsUtilsAjaxHeaderValues = headerValues;
        }
        else {
            window.tsUtilsAjaxHeaderValues = null;
        }
    }
    tsUtilities.SetAjaxHeaderValues = SetAjaxHeaderValues;
    /**
     * Sets the security token in the header before making a remote server call.
     * @param xhr (string: The JSON Web Token.)
     */
    function SetRequestHeaders(xhr) {
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
            var values = window.tsUtilsAjaxHeaderValues;
            if (values != undefined && values != null) {
                values.forEach(function (value) {
                    xhr.setRequestHeader(value.key, value.value);
                });
            }
        }
    }
    tsUtilities.SetRequestHeaders = SetRequestHeaders;
    /**
     * Configures scrolling for elements marked with the class tsutils-fixed-position.
     */
    function SetupScrolling() {
        // Setup scrolling for fixed positioned items on the document
        $(document).off('scroll');
        $(document).scroll(function () {
            var scrollTop = $(document).scrollTop();
            if (scrollTop < 80) {
                scrollTop = 0;
            }
            else {
                scrollTop = scrollTop - 80;
            }
            $(".tsutils-fixed-position:visible").stop().animate({ "top": (scrollTop) + "px" }, "fast");
        });
        // Setup scrolling for fixed positioned items in modals
        $("div.modal").on('scroll', function () {
            var scrollTop = $(this).scrollTop();
            if (scrollTop < 80) {
                scrollTop = 0;
            }
            else {
                scrollTop = scrollTop - 80;
            }
            $(this).find(".tsutils-fixed-position:visible").stop().animate({ "top": (scrollTop) + "px" }, "fast");
        });
    }
    tsUtilities.SetupScrolling = SetupScrolling;
    /**
     * Returns a string value. However, if the item is null or undefined then an empty string is returned instead of null.
     * @param {string} text - The text to validate.
     * @returns {string} - Returns only valid text values.
     */
    function StringValue(text) {
        var output = "";
        if (HasValue(text)) {
            output = text;
        }
        return output;
    }
    tsUtilities.StringValue = StringValue;
    /**
     * Converts each word in a string of text to have the first letter capitalized, except for special cases which are always lowercase unless they are the first or last word.
     * @param {string} text - The text to convert to title case.
     * @returns {string} - Returns the text in title case.
     */
    function TitleCase(text) {
        var specialCases = /^(a|an|and|as|at|but|by|en|for|from|if|in|into|near|nor|of|on|onto|or|the|to|with|vs?\.?|via)$/i;
        return text.replace(/([^\W_]+[^\s-]*) */g, function (e, n, r, i) {
            return r > 0 && r + n.length !== i.length && n.search(specialCases) > -1 && i.charAt(r - 2) !== ":" && i.charAt(r - 1).search(/[^\s-]/) < 0 ? e.toLowerCase() : n.substr(1).search(/[A-Z]|\../) > -1 ? e : e.charAt(0).toUpperCase() + e.substr(1);
        });
    }
    tsUtilities.TitleCase = TitleCase;
    /**
     * Gets the overall size of the browser window as an array of numbers in the format of [width, height].
     * @returns {number[]} Returns an array of numbers in the format of [width, height].
     */
    function WindowSize() {
        var de = document.documentElement;
        var width = window.innerWidth || self.innerWidth || (de && de.clientWidth) || document.body.clientWidth;
        var height = window.innerHeight || self.innerHeight || (de && de.clientHeight) || document.body.clientHeight;
        return [width, height];
    }
    tsUtilities.WindowSize = WindowSize;
})(tsUtilities || (tsUtilities = {}));
//#endregion
//# sourceMappingURL=utilFunctions.js.map