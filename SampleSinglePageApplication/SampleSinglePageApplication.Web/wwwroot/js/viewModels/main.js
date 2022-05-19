/**
 * enum types used for representing various types of interface styles
 */
var StyleType;
(function (StyleType) {
    StyleType[StyleType["Danger"] = 0] = "Danger";
    StyleType[StyleType["Dark"] = 1] = "Dark";
    StyleType[StyleType["Default"] = 2] = "Default";
    StyleType[StyleType["Info"] = 3] = "Info";
    StyleType[StyleType["Light"] = 4] = "Light";
    StyleType[StyleType["Primary"] = 5] = "Primary";
    StyleType[StyleType["Secondary"] = 6] = "Secondary";
    StyleType[StyleType["Success"] = 7] = "Success";
    StyleType[StyleType["Warning"] = 8] = "Warning";
})(StyleType || (StyleType = {}));
/**
 * The main viewmodel shared among all pages.
 */
var MainModel = /** @class */ (function () {
    /**
     * The main viewmodel shared among all pages.
     */
    function MainModel() {
        var _this = this;
        this.AccessDeniedMessage = ko.observable("");
        this.Action = ko.observable("");
        this.DefaultLanguageItems = ko.observableArray([]);
        this.Extra = ko.observableArray([]);
        this.GuidEmpty = ko.observable("00000000-0000-0000-0000-000000000000");
        this.Guid1 = ko.observable("00000000-0000-0000-0000-000000000001");
        this.Guid2 = ko.observable("00000000-0000-0000-0000-000000000002");
        this.Id = ko.observable("");
        this.LanguageItems = ko.observableArray([]);
        this.Loaded = ko.observable(false);
        this.LoggedIn = ko.observable(false);
        //MainLoader: KnockoutObservable<mainLoader> = ko.observable(new mainLoader);
        this.MessageText = ko.observable("");
        this.MessageClass = ko.observable("");
        this.MobileMenu = ko.observable(false);
        this.SignalRUpdate = ko.observable(new signalRUpdate);
        this.StickyMenus = ko.observable(false);
        this.Tenant = ko.observable(new tenant);
        this.TenantCode = ko.observable("");
        this.TenantId = ko.observable("");
        this.Theme = ko.observable("");
        this.Token = ko.observable("");
        this.User = ko.observable(new user);
        this.View = ko.observable("");
        this.VersionInfo = ko.observable(new versionInfo);
        this.WindowHeight = ko.observable(0);
        this.WindowWidth = ko.observable(0);
        /**
         * Observable that applies the necessary styling to the body element for theme support.
         */
        this.BodyClass = ko.computed(function () {
            var output = "";
            switch (_this.Theme()) {
                case "dark":
                    output = "dark-theme";
                    break;
                case "medium":
                    output = "medium-theme";
                    break;
                default:
                    output = "light-theme";
                    break;
            }
            $("#body-element").removeClass();
            $("#body-element").addClass(output);
        });
        /**
         * An observable that always returns the current view as lowercase. Primarily used in the _Layout page
         * to highlight active menu elements.
         */
        this.CurrentView = ko.computed(function () {
            var output = "";
            if (tsUtilities.HasValue(_this.View())) {
                output = _this.View().toLowerCase();
            }
            return output;
        });
        /**
         * Returns true if the current view is part of the Admin menu. This includes not only the main links, but other sub-URLs (eg: EditUser for the User sub-menu item)
         */
        this.CurrentViewAdmin = ko.computed(function () {
            var output = false;
            var view = _this.CurrentView();
            if (tsUtilities.HasValue(view)) {
                switch (view.toLowerCase()) {
                    case "departmentgroups":
                    case "departments":
                    case "editdepartment":
                    case "editdepartmentgroup":
                    case "edittenant":
                    case "edituser":
                    case "language":
                    case "newdepartment":
                    case "newdepartmentgroup":
                    case "newtenant":
                    case "newuser":
                    case "settings":
                    case "tenants":
                    case "udflabels":
                    case "users":
                        output = true;
                        break;
                }
            }
            return output;
        });
        tsUtilities.KnockoutCustomBindingHandlers();
        this.messageTimerInterval = 3000;
        this.Token(window.token);
        if (window.loggedIn != undefined && window.loggedIn != null && window.loggedIn == true) {
            this.LoggedIn(true);
        }
        this.LoadLanguage(window.objLanguage, window.objDefaultLanguage);
        if (window.objExtra != null) {
            this.Extra(window.objExtra);
        }
        if (window.objTenant != undefined && window.objTenant != null) {
            this.Tenant().Load(window.objTenant);
            window.objTenant = null;
        }
        if (window.objUser != undefined && window.objUser != null) {
            this.User().Load(window.objUser);
            window.objUser = null;
        }
        if (window.objVersionInfo != undefined && window.objVersionInfo != null) {
            this.VersionInfo().Load(window.objVersionInfo);
        }
        if (tsUtilities.HasValue(window.action)) {
            this.Action(window.action);
        }
        if (tsUtilities.HasValue(window.id)) {
            this.Id(window.id);
        }
        if (tsUtilities.HasValue(window.tenantCode)) {
            this.TenantCode(window.tenantCode);
        }
        if (tsUtilities.HasValue(window.tenantId)) {
            this.TenantId(window.tenantId);
        }
        var theme = tsUtilities.CookieRead("theme");
        if (!tsUtilities.HasValue(theme)) {
            theme = "";
        }
        this.Theme(theme);
        this.Theme.subscribe(function (v) {
            tsUtilities.CookieWrite("theme", v);
        });
        this.CheckLoad();
        this.UpdateWindowSettings();
        $(window).off("resize");
        $(window).on("resize", function () {
            _this.UpdateWindowSettings();
        });
        var stickyMenus = tsUtilities.CookieRead("sticky-menus");
        if (!tsUtilities.HasValue(stickyMenus)) {
            stickyMenus = "0";
        }
        this.StickyMenus(stickyMenus == "1");
        window.onpopstate = function () { _this.PopStateChanged(); };
        this.CheckForUpdates();
    }
    MainModel.prototype.AccessDenied = function (message) {
        console.log("Access Denied", message);
        this.AccessDeniedMessage("");
        if (message != undefined && message != null && message != "") {
            this.AccessDeniedMessage(message);
        }
        this.Nav("AccessDenied");
    };
    MainModel.prototype.AjaxUserAutocomplete = function (element, callbackHandler) {
        $("#" + element).autocomplete({
            source: this.AjaxUserSearch,
            minLength: 3,
            select: function (event, ui) {
                event.preventDefault();
                $("#" + element).val("");
                callbackHandler(ui.item);
            },
            focus: function (event, ui) {
                event.preventDefault();
            }
        }).data("ui-autocomplete")._renderItem = function (ul, item) {
            //Add the .ui-state-disabled class and don't wrap in <a> if value is empty
            if (item.value == '' || item.value == window.mainModel.GuidEmpty()) {
                return $('<li class="ui-state-disabled">' + item.label + '</li>').appendTo(ul);
            }
            else {
                return $("<li>")
                    .append("<a>" + item.label + "</a>")
                    .appendTo(ul);
            }
        };
    };
    MainModel.prototype.AjaxUserSearch = function (req, callbackHandler) {
        var lookup = new ajaxLookup();
        lookup.Search(req.term);
        lookup.TenantId(window.tenantId);
        $.ajax({
            url: window.baseURL + "api/Data/AjaxUserSearch/",
            type: 'POST',
            data: ko.toJSON(lookup),
            beforeSend: setRequestHeaders,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var results = [];
                if (data != null) {
                    if (data.results != null) {
                        data.results.forEach(function (element) {
                            var item = new ajaxResults();
                            item.Load(element);
                            results.push(item);
                        });
                    }
                }
                callbackHandler(JSON.parse(ko.toJSON(results)));
            }
        });
    };
    /**
     * Determines if the image file is an allowed image file type.
     * @param file {string} - The file name to validate.
     */
    MainModel.prototype.AllowedFileTypeImage = function (file) {
        var output = false;
        this.Message_Hide();
        var ext = tsUtilities.GetExtension(file);
        if (tsUtilities.HasValue(ext)) {
            switch (ext.toLowerCase()) {
                case "gif":
                case "jpg":
                case "png":
                    output = true;
                    break;
            }
        }
        if (!output) {
            this.Message_Error(this.Language("InvalidImageFileType"));
        }
        return output;
    };
    /**
     * Used to render an icon from a boolean value.
     * @param {boolean} value - The value to check.
     * @param {string} icon - OPTIONAL: an icon to use if the boolean value is true. Defaults to teh Selected icon.
     */
    MainModel.prototype.BooleanToIcon = function (value, icon) {
        var output = "";
        if (value != undefined && value != null && value == true) {
            if (icon != undefined && icon != null && icon != "") {
                output = this.Icon(icon);
                if (output == "") {
                    output = icon;
                }
            }
            else {
                output = this.Icon("Selected");
            }
        }
        return output;
    };
    /**
     * Make sure all required models have been loaded before performing the first navigation.
     */
    MainModel.prototype.CheckLoad = function () {
        var _this = this;
        var models = [];
        models.push("LoginModel");
        if (this.LoggedIn()) {
            models.push("HomeModel");
            models.push("ProfileModel");
            models.push("Page1Model");
            models.push("Page2Model");
            models.push("RecordsModel");
            // Depending on the user type various models will be loaded (other than this MainModel).
            if (this.User().admin()) {
                models.push("LanguageModel");
                models.push("SettingsModel");
                models.push("UdfLabelsModel");
                models.push("UsersModel");
                if (this.User().appAdmin()) {
                    models.push("TenantsModel");
                }
            }
        }
        var missingModels = [];
        models.forEach(function (model) {
            if (eval("typeof " + model) === 'undefined') {
                missingModels.push(model);
            }
        });
        if (missingModels.length > 0) {
            setTimeout(function () { return _this.CheckLoad(); }, 200);
        }
        else {
            // Everything is loaded, but give a small delay to allow time for the other
            // models to bind to the change event for the View property in this model.
            this.Loaded(true);
            setTimeout(function () { return _this.Nav(_this.Action(), _this.Id()); }, 500);
        }
    };
    /**
     * Checks the VersionInfo from the server to see if the server has restarted or been updated.
     */
    MainModel.prototype.CheckForUpdates = function () {
        var _this = this;
        var success = function (data) {
            var reload = false;
            if (data != null) {
                if (_this.VersionInfo().released() != data.released || _this.VersionInfo().runningSince() != data.runningSince || _this.VersionInfo().version() != data.version) {
                    reload = true;
                }
            }
            if (reload) {
                _this.Nav("ServerUpdated");
            }
            else {
                setTimeout(function () { return _this.CheckForUpdates(); }, 60000);
            }
        };
        var failure = function () {
            setTimeout(function () { return _this.CheckForUpdates(); }, 60000);
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetVersionInfo", null, success, failure);
    };
    /**
     * The current page URL
     */
    MainModel.prototype.CurrentUrl = function () {
        var url = window.baseURL;
        if (window.useTenantCodeInUrl) {
            url += this.Tenant().tenantCode() + "/";
        }
        if (tsUtilities.HasValue(this.Action())) {
            url += this.Action() + "/";
            if (tsUtilities.HasValue(this.Id())) {
                url += this.Id();
            }
        }
        return url;
    };
    /**
     * Gets the ticks from a date.
     * @param {Date} date - The date.
     * @returns {number} - Returns the ticekts from the date using the .getTime method.
     */
    MainModel.prototype.DateTicks = function (date) {
        var output = 0;
        if (date != undefined && date != null) {
            var d = new Date(date);
            output = d.getTime();
        }
        return output;
    };
    /**
     * Gets the name of a department group from the Id. Used in the departments view table to show the groups when the Id is the only data element available.
     * @param departmentGroupId {string} - The unique Id of the department group.
     */
    MainModel.prototype.DepartmentGroupNameFromId = function (departmentGroupId) {
        var output = "";
        if (this.Loaded()) {
            if (this.Tenant().departmentGroups() != null && this.Tenant().departmentGroups().length > 0) {
                var group = ko.utils.arrayFirst(this.Tenant().departmentGroups(), function (item) {
                    return item.departmentGroupId() == departmentGroupId;
                });
                if (group != null) {
                    output = group.departmentGroupName();
                }
            }
        }
        return output;
    };
    /**
     * I wanted a simpler way to manage my interface icons, so that if I decided to change an icon for an action (eg: save) I can
     * simply change it in one place and all the calls to get the icon returns the icon. Currently I'm using FontAwesome icons.
     * However, this just returns HTML, so these could be images, external links to images or SVG definitions, etc.
     * @param module {string} - The parameter indicating what icon this is. These match language item names for convenience when using the IconAndText option.
     */
    MainModel.prototype.Icon = function (module) {
        var output = "";
        if (tsUtilities.HasValue(module)) {
            switch (module.toLowerCase()) {
                case "accessdenied":
                    output = '<i class="fa-regular fa-shield-exclamation"></i>';
                    break;
                case "addnewuser":
                    output = '<i class="fas fa-user-plus"></i>';
                    break;
                case "addnewdepartmentgroup":
                case "addtenant":
                    output = '<i class="fas fa-plus-square"></i>';
                    break;
                case "admin":
                    output = '<i class="fa-solid fa-sliders"></i>';
                    break;
                case "back":
                    output = '<i class="fas fa-arrow-left"></i>';
                    break;
                case "cancel":
                    output = '<i class="fas fa-times"></i>';
                    break;
                case "changepassword":
                case "passwordchanged":
                    output = '<i class="fa-solid fa-lock"></i>';
                    break;
                case "clear":
                    output = '<i class="fas fa-broom"></i>';
                    break;
                case "closedialog":
                    output = '<i class="fas fa-times"></i>';
                    break;
                case "delete":
                case "deletetenant":
                case "deleteavatar":
                case "confirmdelete":
                case "confirmdeletetenant":
                    output = '<i class="fas fa-trash"></i>';
                    break;
                case "departments":
                case "addnewdepartment":
                case "editdepartment":
                    output = '<i class="fa-solid fa-list-tree"></i>';
                    break;
                case "departmentgroups":
                    output = '<i class="fa-solid fa-screen-users"></i>';
                    break;
                case "edit":
                    output = '<i class="fas fa-edit"></i>';
                    break;
                case "edittenant":
                    output = '<i class="fas fa-users-cog"></i>';
                    break;
                case "hidefilter":
                    output = '<i class="fa-solid fa-filter"></i>';
                    break;
                case "hidehelp":
                    output = '<i class="fas fa-arrow-up"></i>';
                    break;
                case "homepage":
                    output = '<i class="fa-solid fa-house"></i>';
                    break;
                case "language":
                    output = '<i class="fas fa-language"></i>';
                    break;
                case "login":
                case "logintitle":
                case "loginwithlocalaccount":
                    output = '<i class="fas fa-sign-in-alt"></i>';
                    break;
                case "loginwitheitsso":
                    output = '<i class="okta-logo"></i>';
                    break;
                case "logout":
                    output = '<i class="fas fa-sign-out-alt"></i>';
                    break;
                case "manageavatar":
                    output = '<i class="fas fa-user-circle"></i>';
                    break;
                case "manageprofile":
                case "manageprofileinfo":
                    output = '<i class="fa-light fa-id-badge"></i>';
                    break;
                case "newtenant":
                    output = '<i class="fas fa-users-cog"></i>';
                    break;
                case "page1":
                    output = '<i class="fa-regular fa-face-smile"></i>';
                    break;
                case "page2":
                    output = '<i class="fa-regular fa-face-laugh"></i>';
                    break;
                case "refresh":
                    output = '<i class="fas fa-sync-alt"></i>';
                    break;
                case "records":
                    output = '<i class="fa-solid fa-alien-8bit" ></i>';
                    break;
                case "resetuserpassword":
                case "updatepassword":
                    output = '<i class="fas fa-lock"></i>';
                    break;
                case "save":
                    output = '<i class="fas fa-save"></i>';
                    break;
                case "scrollingmenus":
                    output = '<i class="fa-solid fa-thumbtack scrolling"></i>';
                    break;
                case "settings":
                    output = '<i class="fa-solid fa-sliders"></i>';
                    break;
                case "selected":
                    output = '<i class="fa-solid fa-circle-check"></i>';
                    break;
                case "serverupdated":
                    output = '<i class="fa-solid fa-server"></i>';
                    break;
                case "showfilter":
                    output = '<i class="fa-solid fa-filter-circle-xmark"></i>';
                    break;
                case "showhelp":
                    output = '<i class="fas fa-arrow-down"></i>';
                    break;
                case "stickymenus":
                    output = '<i class="fa-solid fa-thumbtack sticky"></i>';
                    break;
                case "tenants":
                    output = '<i class="fas fa-users-cog"></i>';
                    break;
                case "udflabels":
                    output = '<i class="fa-brands fa-firstdraft"></i>';
                    break;
                case "uploadfile":
                    output = '<i class="fas fa-cloud-upload-alt"></i>';
                    break;
                case "usermenuicon":
                    output = '<i class="fa-solid fa-circle-user"></i>';
                    break;
                case "users":
                    output = '<i class="fas fa-users"></i>';
                    break;
            }
        }
        if (output != "") {
            output = "<span class='icon-element'>" + output + "</span>";
        }
        return output;
    };
    /**
     * Gets the icon and the language for a given item name.
     * @param module{string} - The parameter indicating what icon and language item this is.
     */
    MainModel.prototype.IconAndText = function (module) {
        var output = this.Icon(module) + "<span class='icon-text'>" + this.Language(module) + "</span>";
        return output;
    };
    /**
     * Inserts text into a field at the current cursor position.
     * @param {string} field - The id of the input element.
     * @param {string} value - The value to insert.
     */
    MainModel.prototype.InsertAtCursor = function (field, value) {
        insertAtCursor(field, value);
    };
    /**
     * Gets the language text for a given language item.
     * @param item {string} - The unique id of the language item.
     * @param defaultValue {string} - An optional default value to use if the language item is not found.
     * @param removeSpaces {string} - An option to replace spaces with &nbsp; characters. This is used when showing the item where you don't want it to wrap.
     * @param replaceValues {string} - For language items that use replacement properties (eg: {0}) this is an array of strings that relate to the index of the item values.
     */
    MainModel.prototype.Language = function (item, defaultValue, removeSpaces, replaceValues) {
        var output = item.toUpperCase();
        if (tsUtilities.HasValue(defaultValue)) {
            output = defaultValue;
        }
        if (tsUtilities.HasValue(item)) {
            if (this.LanguageItems() != null && this.LanguageItems().length > 0) {
                var lang = ko.utils.arrayFirst(this.LanguageItems(), function (e) {
                    return e.id().toLowerCase() == item.toLowerCase();
                });
                if (lang != null) {
                    output = lang.value();
                }
            }
        }
        if (replaceValues != undefined && replaceValues != null && replaceValues.length > 0) {
            var index_1 = 0;
            replaceValues.forEach(function (item) {
                output = output.replace("{" + index_1.toString() + "}", item);
                index_1++;
            });
        }
        if (removeSpaces != undefined && removeSpaces != null && removeSpaces == true) {
            output = tsUtilities.ReplaceSpaces(output);
        }
        output = output.replace(/(?:\r\n|\r|\n)/g, '<br>');
        return output;
    };
    /**
     * Gets the language text for a given language item and marked it as a required field.
     * @param item {string} - The unique id of the language item.
     * @param defaultValue {string} - An optional default value to use if the language item is not found.
     * @param removeSpaces {string} - An option to replace spaces with &nbsp; characters. This is used when showing the item where you don't want it to wrap.
     * @param replaceValues {string} - For language items that use replacement properties (eg: {0}) this is an array of strings that relate to the index of the item values.
     */
    MainModel.prototype.LanguageRequired = function (item, defaultValue, removeSpaces, replaceValues) {
        var output = this.Language(item, defaultValue, removeSpaces, replaceValues);
        return "<span class='required'>* " + output + "</span>";
    };
    MainModel.prototype.ListItemNameFromId = function (itemId) {
        var output = "";
        if (this.Loaded()) {
            var item = ko.utils.arrayFirst(this.Tenant().listItems(), function (item) {
                return item.id() == itemId;
            });
            if (item != null) {
                output = item.name();
            }
        }
        return output;
    };
    /**
     * Called when the page loads to load the language and default language. Also called when SignalR recieves language updates.
     * @param language {server.optionPair[]} - An array of optionPair values for the tenant language.
     * @param defaults {server.optionPair[]} - An array of optionPair values for the default language.
     */
    MainModel.prototype.LoadLanguage = function (language, defaults) {
        if (language != undefined && language != null) {
            var options_1 = [];
            language.forEach(function (e) {
                var item = new optionPair();
                item.Load(e);
                options_1.push(item);
            });
            this.LanguageItems(options_1);
        }
        if (defaults != undefined && defaults != null) {
            var defaultOptions_1 = [];
            defaults.forEach(function (e) {
                var item = new optionPair();
                item.Load(e);
                defaultOptions_1.push(item);
            });
            this.DefaultLanguageItems(defaultOptions_1);
        }
    };
    /**
     * Called when a user clicks the logout button. Since a redirect happens, the interface elements are hidden first to avoid confusion about the redirect delay.
     */
    MainModel.prototype.Logout = function () {
        // Hide the interface
        $("#main-model").hide();
        $("#page-view-model-area").hide();
        // Redirect to the local logout page
        var returnUrl = window.baseURL;
        if (window.useTenantCodeInUrl) {
            returnUrl += this.TenantCode();
        }
        var url = window.baseURL + "Logout/" + this.TenantId() + "?redirect=" + encodeURI(returnUrl);
        window.location.href = url;
    };
    /**
     * Updates the status message shown at the top of the page.
     * @param {string} message - The message to show.
     * @param {StyleType} styleType - An optional style type.
     * @param {boolean} autoHide - Option to auto-hide the message after an interval (defaults to true)
     * @param {boolean} fixedPosition - Optionally show as a fixed position dialog.
     * @param {boolean} small - Optionally indicate the message should be a small message on the right.
     */
    MainModel.prototype.Message = function (message, styleType, autoHide, fixedPosition, replaceLineBreaksWithHtmlBreaks, small) {
        var _this = this;
        clearTimeout(this.messageTimerHandle);
        if (autoHide == undefined || autoHide == null) {
            autoHide = true;
        }
        if (autoHide == true) {
            this.messageTimerHandle = setTimeout(function () { return _this.Message_Hide(); }, this.messageTimerInterval);
        }
        if (fixedPosition == undefined || fixedPosition == null) {
            fixedPosition = false;
        }
        if (small == undefined || small == null) {
            small = false;
        }
        var fixed = "";
        if (fixedPosition) {
            fixed = " fixed-message";
        }
        if (small) {
            fixed += " small";
        }
        if (styleType == undefined || styleType == null) {
            styleType = StyleType.Default;
        }
        switch (styleType) {
            case StyleType.Danger:
                this.MessageClass("alert alert-danger" + fixed);
                break;
            case StyleType.Dark:
                this.MessageClass("alert alert-dark" + fixed);
                break;
            case StyleType.Default:
            case StyleType.Primary:
                this.MessageClass("alert alert-primary" + fixed);
                break;
            case StyleType.Info:
                this.MessageClass("alert alert-info" + fixed);
                break;
            case StyleType.Light:
                this.MessageClass("alert alert-light" + fixed);
                break;
            case StyleType.Secondary:
                this.MessageClass("alert alert-secondary" + fixed);
                break;
            case StyleType.Success:
                this.MessageClass("alert alert-success" + fixed);
                break;
            case StyleType.Warning:
                this.MessageClass("alert alert-warning" + fixed);
                break;
        }
        if (replaceLineBreaksWithHtmlBreaks != undefined && replaceLineBreaksWithHtmlBreaks != null && replaceLineBreaksWithHtmlBreaks == true) {
            message = message.replace(/(?:\r\n|\r|\n)/g, '<br>');
        }
        if (message.indexOf("{{") > -1 && message.indexOf("}}") > -1) {
            // A language tag was passed.
            var tag = message.replace("{{", "");
            tag = tag.replace("}}", "");
            message = this.Language(tag);
        }
        this.MessageText(message);
    };
    /**
     * Shows a default message when items are being deleted.
     * @param message {string} - An optional message to show. If not passed a default message is shown.
     */
    MainModel.prototype.Message_Deleting = function (message) {
        if (!tsUtilities.HasValue(message)) {
            message = this.Language("DeletingWait", "Deleting, PLease Wait...");
        }
        this.Message(message, StyleType.Danger, false, true, true);
    };
    /**
     * Shows a simple error message.
     * @param {string} error - The error message to show.
     */
    MainModel.prototype.Message_Error = function (error) {
        this.Message(error, StyleType.Danger, false, true, true);
    };
    /**
     * Shows an error message with one or more error messages.
     * @param {string[]} errors - The array of errors.
     * @param {string} intro - An optional intro message.
     * If not specified defaults to either "The following error has occurred" or
     * "The following errors have occurred" depending on the error count.
     */
    MainModel.prototype.Message_Errors = function (errors, intro) {
        var message = "";
        if (errors != undefined && errors != null && errors.length > 0) {
            if (tsUtilities.HasValue(intro)) {
                message += intro;
            }
            else {
                if (errors.length == 1) {
                    message += "<div class='padbottom'>The following error has occurred:</div>\n";
                }
                else {
                    message += "<div class='padbottom'>The following errors have occurred:</div>\n";
                }
            }
            if (errors.length == 1) {
                message += errors[0];
            }
            else {
                message += "<ul>\n";
                errors.forEach(function (msg) {
                    message += "  <li>" + msg + "</li>\n";
                });
                message += "</ul>\n";
            }
        }
        this.Message(message, StyleType.Danger, false, true, false);
    };
    /**
     * Hides the status message, either when the timer expires or the user clicks the close icon.
     */
    MainModel.prototype.Message_Hide = function () {
        clearTimeout(this.messageTimerHandle);
        this.MessageText("");
    };
    /**
     * Shows a loading message.
     * @param {string} message - Optional loading message. Defaults to "Loading, Please Wait..."
     */
    MainModel.prototype.Message_Loading = function (message) {
        if (!tsUtilities.HasValue(message)) {
            message = this.Language("LoadingWait", "Loading, PLease Wait...");
        }
        this.Message(message, StyleType.Dark, false, true, true);
    };
    /**
     * Shows a message indicated the record was saved.
     * @param {string} message - Optionally pass the message. Defaults to "SavedAt" and the long time.
     */
    MainModel.prototype.Message_Saved = function (message) {
        if (!tsUtilities.HasValue(message)) {
            message = this.Language("SavedAt", "Saved at") + " " + tsUtilities.FormatTimeLong(new Date());
        }
        this.Message(message, StyleType.Success, true, true, true, true);
    };
    /**
     * Shows a saving message.
     * @param {string} message - Optional saving message. Defaults to "Saving, Please Wait..."
     */
    MainModel.prototype.Message_Saving = function (message) {
        if (!tsUtilities.HasValue(message)) {
            message = this.Language("SavingWait", "Saving, Please Wait...");
        }
        this.Message(message, StyleType.Dark, false, true, true);
    };
    /**
     * Used when saving records and showing error messages about missing required elements to simplify those calls.
     * @param fieldName {string} - The name of the element. Should be retrieved from Language(elementName) instead of hard-coding.
     */
    MainModel.prototype.MissingRequiredField = function (fieldName) {
        var output = this.Language("RequiredMissing", "", false, [fieldName]);
        return output;
    };
    /**
     * Handles the Single-Page Application navigation.
     * @param action {string} - The optional action (eg: "Users", "Settings", Tenants", etc.)
     * @param id {string} - An optional Id value. Only used in conjunction with action, so if action is not given but an Id is the Id will be ignored.
     * @param fromPop {boolean} - Set to true when this is called from the window.onpopstate function that handles SPA navigation without reloading the browser.
     */
    MainModel.prototype.Nav = function (action, id, fromPop) {
        //console.log("Nav", action, id, fromPop);
        if (action == null || action == undefined || action == "undefined" || action == "") {
            action = "";
        }
        if (!tsUtilities.HasValue(action)) {
            action = this.LoggedIn() ? "" : "Login";
        }
        // If we have a requested URL cookie then clear that cookie and redirect to that page.
        var requestedUrl = tsUtilities.CookieRead("requested-url");
        if (tsUtilities.HasValue(requestedUrl) && action.toLowerCase() != "login" && requestedUrl.toLowerCase().indexOf("null") == -1) {
            tsUtilities.CookieWrite("requested-url", "");
            window.location.href = requestedUrl;
            return;
        }
        this.Message_Hide();
        if (fromPop == undefined || fromPop == null) {
            fromPop = false;
        }
        if (id == undefined || id == null) {
            id = "";
        }
        var url = window.baseURL;
        if (window.useTenantCodeInUrl) {
            url += this.Tenant().tenantCode() + "/";
        }
        if (tsUtilities.HasValue(action)) {
            url += action + "/";
            if (tsUtilities.HasValue(id)) {
                url += id;
            }
        }
        var navChanged = false;
        if (this.Action().toLowerCase() != action.toLowerCase() || this.Id().toLowerCase() != id.toLowerCase()) {
            navChanged = true;
        }
        if (navChanged && this.Action().toLowerCase() == action.toLowerCase()) {
            // If the action didn't change, but the id did, then we need to toggle the action temporarily
            // to a value that isn't actually bound anywhere to cause the view to change so models looking
            // for this change can reload as needed.
            this.View("A Fake Action to Trigger a Change");
        }
        if (!navChanged) {
            // Refresh the view.
            this.View("A Fake Action to Trigger a Change");
        }
        // At this point, if the user is not logged in and we are not on a valid page for non-logged-in users,
        // save the current URL in a cookie and redirect to the login page.
        var allowAnonymousAccess = false;
        if (!this.LoggedIn()) {
            switch (action.toLowerCase()) {
                case "accessdenied":
                case "login":
                case "otherpages":
                case "thatcanbe":
                case "accessedwhen":
                case "notloggedin":
                case "serverupdated":
                    allowAnonymousAccess = true;
                    break;
                default:
                    tsUtilities.CookieWrite("requested-url", url);
                    if (window.useTenantCodeInUrl) {
                        window.location.href = window.baseURL + this.TenantCode() + "/Login";
                    }
                    else {
                        window.location.href = window.baseURL + "Login";
                    }
                    return;
            }
        }
        // Make sure the user has access to the requested resourse
        var accessDenied = false;
        if (!allowAnonymousAccess) {
            // This is where you would test various actions to make sure the user has access.
            switch (action.toLowerCase()) {
                case "changepassword":
                    if (this.User().preventPasswordChange()) {
                        accessDenied = true;
                    }
                    break;
                case "udflabels":
                case "users":
                    if (!this.User().admin()) {
                        accessDenied = true;
                    }
                    break;
            }
        }
        if (accessDenied == true) {
            action = "AccessDenied";
            if (window.useTenantCodeInUrl) {
                url = window.baseURL + this.TenantCode() + "/AccessDenied";
            }
            else {
                url = window.baseURL + "AccessDenied";
            }
            navChanged = true;
        }
        this.Action(action);
        this.Id(id);
        this.View(action);
        $("#main-model").show();
        $("#page-view-model-area").show();
        if (navChanged && !fromPop) {
            window.history.pushState(null, null, url);
        }
    };
    /**
     * Generates a URL that can be bound to the a element to create links.
     * @param action {string} - The optional action (eg: "Users", "Settings", Tenants", etc.)
     * @param id {string} - An optional Id value. Only used in conjunction with action, so if action is not given but an Id is the Id will be ignored.
     */
    MainModel.prototype.NavLink = function (action, id) {
        var output = window.baseURL;
        if (window.useTenantCodeInUrl) {
            output += this.Tenant().tenantCode() + "/";
        }
        if (tsUtilities.HasValue(action)) {
            output += action;
            if (tsUtilities.HasValue(id)) {
                output += "/" + id;
            }
        }
        return output;
    };
    /**
     * Formats the select list option items.
     * @param option - The option item.
     * @param {SimpleObject} item - The item from the collection.
     */
    MainModel.prototype.OptionListFormat = function (option, item) {
        if (item != undefined && item != null) {
            if (!tsUtilities.HasValue(item.value())) {
                ko.applyBindingsToNode(option, { disable: true }, item);
            }
            option.text = item.id().replace(/ /g, '\u00A0');
        }
    };
    /**
     * The method that gets called when the window.onpopstate event fires so navigation can be updated.
     */
    MainModel.prototype.PopStateChanged = function () {
        var path = document.location.href;
        // Remove the base URL from the path so only the part we are concerned with remains.
        if (tsUtilities.HasValue(path)) {
            if (path.toLowerCase().indexOf(window.baseURL.toLowerCase()) > -1) {
                path = path.substring(window.baseURL.length);
            }
            if (path.indexOf("/") == 0) {
                path = path.substr(1);
            }
        }
        var tenantId = "";
        var action = "";
        var id = "";
        if (tsUtilities.HasValue(path)) {
            var parts = path.split("/");
            if (parts != undefined && parts != null && parts.length > 0) {
                tenantId = parts[0];
                if (parts.length > 0) {
                    action = parts[1];
                    if (parts.length > 1) {
                        id = parts[2];
                    }
                }
            }
        }
        if (tenantId.toLowerCase() != this.Tenant().tenantCode().toLowerCase()) {
            // Need a full redirect
            var url = window.baseURL + tenantId + "/";
            if (tsUtilities.HasValue(action)) {
                url += action + "/";
                if (tsUtilities.HasValue(id)) {
                    url += id;
                }
            }
            window.location.href = url;
        }
        else {
            this.Nav(action, id, true);
        }
    };
    /**
     * Reloads the User object for the current user.
     */
    MainModel.prototype.ReloadUser = function () {
        var _this = this;
        var success = function (data) {
            if (data != null && data.actionResponse.result) {
                _this.User().Load(data);
                _this.User.notifySubscribers();
            }
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUser/" + this.User().userId(), null, success);
    };
    /**
     * Renders a file storage item as either a thumbnail preview or an icon.
     * @param {fileStorage} file - The fileStorage item.
     * @returns {string} - A file icon.
     */
    MainModel.prototype.RenderFileImageOrIcon = function (file) {
        var output = "";
        if (file != null && tsUtilities.HasValue(file.extension())) {
            var ext = file.extension().replace(".", "").toLowerCase();
            switch (ext) {
                case "png":
                case "jpg":
                case "jpeg":
                case "gif":
                    if (file.value() != null && file.value().length > 0) {
                        output = "data:image/" + ext + "; base64," + file.value();
                    }
                    else if (tsUtilities.HasValue(file.base64value())) {
                        output = file.base64value();
                    }
                    else {
                        output = window.baseURL + "file/view/" + file.fileId();
                    }
                    break;
                case "csv":
                case "doc":
                case "docx":
                case "eml":
                case "eps":
                case "mp3":
                case "pdf":
                case "ppt":
                case "pptx":
                case "tif":
                case "tiff":
                case "txt":
                case "wav":
                case "xls":
                case "xlsx":
                case "zip":
                    output = window.baseURL + "images/file_" + ext + ".png";
                    break;
                default:
                    output = window.baseURL + "images/file_unknown.png";
                    break;
            }
        }
        return output;
    };
    /**
     * Renders a list of files to the specified element.
     * @param {string} element - The id of the html element where the file list will be rendered.
     * @param {fileStorage[]} files - The collection of fileStorage items.
     * @param {Function} deleteCallbackHandler - An optional callback handler for deleting files.
     */
    MainModel.prototype.RenderFiles = function (element, files, deleteCallbackHandler) {
        var output = "";
        var allowDelete = this.User().admin();
        var deleteIcon = "<i class='fas fa-trash'></i>";
        var deleteText = this.Language("Delete");
        if (files != null && files.length > 0) {
            //output += "<div class='row'><div class='col-12'>\n";
            output += "<div class='file-container'>\n";
            for (var x = 0; x < files.length; x++) {
                var file = files[x];
                output += "  <div class='item-file'>\n";
                if (allowDelete && deleteCallbackHandler != undefined && deleteCallbackHandler != null) {
                    output +=
                        "    <div class='item-file-delete-button'>\n" +
                            "      <button type='button' class='btn btn-xs btn-danger show-on-hover' file-id='" + file.fileId() + "' id='delete-file-" + file.fileId() + "'>\n" +
                            "        " + deleteIcon + "\n" +
                            "        <span class='icon-text show-on-hover'>" + deleteText + "</span>\n" +
                            "      </button>\n" +
                            "    </div>\n";
                }
                output +=
                    "    <div class='center'>\n" +
                        "      <div class='item-file-image'>\n" +
                        "        <img src='" + this.RenderFileImageOrIcon(file) + "' class='pointer' file-id='" + file.fileId() + "' id='view-file-" + file.fileId() + "'>\n" +
                        "      </div>\n" +
                        "      <div class='item-file-label'>\n" +
                        "        <div class='file-name'>" + file.fileName() + "</div>\n" +
                        "        <div class='file-size'>" + tsUtilities.BytesToFileSizeLabel(file.bytes()) + "</div>\n" +
                        "      </div>\n" +
                        "    </div>\n" +
                        "  </div>\n";
                ;
            }
            output += "</div>\n";
        }
        $("#" + element).html(output);
        if (files != null && files.length > 0) {
            files.forEach(function (file) {
                $("#delete-file-" + file.fileId()).off("click");
                $("#delete-file-" + file.fileId()).on("click", function () {
                    var id = $(this).attr("file-id");
                    if (tsUtilities.HasValue(id)) {
                        deleteCallbackHandler(id);
                    }
                });
                $("#view-file-" + file.fileId()).off("click");
                $("#view-file-" + file.fileId()).on("click", function () {
                    var id = $(this).attr("file-id");
                    if (tsUtilities.HasValue(id)) {
                        window.mainModel.ViewFile(id);
                    }
                });
            });
        }
    };
    /**
     * Called from the javascript code on the _Layout page when a SignalR message is received from the server.
     * Some models subscribe to the event that fires when the this.SignalRUpdate() object gets updated here with the data received from the server.
     * @param update
     */
    MainModel.prototype.SignalRUpdateHandler = function (update) {
        //console.log("SignalR Object", update);
        this.SignalRUpdate().Load(JSON.parse(ko.toJSON(update)));
        this.SignalRUpdate.notifySubscribers();
        var updateType = this.SignalRUpdate().updateTypeString();
        var message = this.SignalRUpdate().message();
        switch (updateType.toLowerCase()) {
            case "setting":
                switch (message.toLowerCase()) {
                    case "deleteddepartment":
                        var dept = ko.utils.arrayFirst(this.Tenant().departments(), function (item) {
                            return item.departmentId() == update.itemId;
                        });
                        if (dept != null) {
                            this.Tenant().departments.remove(dept);
                        }
                        break;
                    case "deleteddepartmentgroup":
                        var dgItem = ko.utils.arrayFirst(this.Tenant().departmentGroups(), function (item) {
                            return item.departmentGroupId() == update.itemId;
                        });
                        if (dgItem != null) {
                            this.Tenant().departmentGroups.remove(dgItem);
                        }
                        break;
                    case "language":
                        this.LoadLanguage(JSON.parse(update.object), null);
                        break;
                    case "saveddepartment":
                        var savedDept_1 = new department();
                        savedDept_1.Load(JSON.parse(update.object));
                        var existingDept = null;
                        if (this.Tenant().departments() != null && this.Tenant().departments().length > 0) {
                            existingDept = ko.utils.arrayFirst(this.Tenant().departments(), function (item) {
                                return item.departmentId() == savedDept_1.departmentId();
                            });
                        }
                        if (existingDept != null) {
                            existingDept.Load(JSON.parse(update.object));
                        }
                        else {
                            this.Tenant().departments.push(savedDept_1);
                            this.Tenant().departments.notifySubscribers();
                        }
                        break;
                    case "saveddepartmentgroup":
                        var dg_1 = new departmentGroup();
                        dg_1.Load(JSON.parse(update.object));
                        var existingdg = ko.utils.arrayFirst(this.Tenant().departmentGroups(), function (item) {
                            return item.departmentGroupId() == dg_1.departmentGroupId();
                        });
                        if (existingdg != null) {
                            existingdg.departmentGroupName(dg_1.departmentGroupName());
                        }
                        else {
                            this.Tenant().departmentGroups.push(dg_1);
                            this.Tenant().departmentGroups.notifySubscribers();
                        }
                        break;
                    case "savedudflabels":
                        var labels = update.object;
                        if (labels != null && labels.length > 0) {
                            var l_1 = [];
                            labels.forEach(function (e) {
                                var item = new udfLabel();
                                item.Load(e);
                                l_1.push(item);
                            });
                            this.Tenant().udfLabels(l_1);
                        }
                        break;
                    case "saveduser":
                        if (this.User().userId() == update.itemId) {
                            // This is the current user, so reload the user object
                            this.User().Load(JSON.parse(update.object));
                        }
                        break;
                    case "tenantsaved":
                        this.TenantSaved(update.object);
                        break;
                }
                break;
        }
    };
    /**
     * Renders the sticky icon at the top of fixed menu areas.
     */
    MainModel.prototype.StickyIcon = function () {
        var output = "";
        if (this.StickyMenus()) {
            output = this.Icon("StickyMenus");
        }
        else {
            output = this.Icon("ScrollingMenus");
        }
        return output;
    };
    /**
     * When a user selects a different tenant account from the menu this performs the actual page reload.
     * @param tenantCode {string} - The new tenant code.
     */
    MainModel.prototype.SwitchTenant = function (tenantCode) {
        if (tsUtilities.HasValue(tenantCode) && tenantCode.toLowerCase() != window.tenantCode.toLowerCase()) {
            if (window.useTenantCodeInUrl) {
                window.location.href = window.baseURL + tenantCode;
            }
            else {
                tsUtilities.CookieWrite(window.appName + "-tenant-code", tenantCode);
                window.location.href = window.baseURL;
            }
        }
    };
    /**
     * Called when a tenant is saved. If the tenant code changes the browser needs to be refreshed.
     * @param object {any} - The message coming from SignalR that contains the updated tenant object.
     */
    MainModel.prototype.TenantSaved = function (object) {
        var t = new tenant();
        t.Load(JSON.parse(object));
        var tenantId = t.tenantId();
        if (t.tenantId() == window.tenantId) {
            if (t.tenantCode() != window.tenantCode) {
                // If this is the current tenant and the tenant code has changed we need to redirect back to the root to reload.
                window.location.href = window.baseURL;
            }
            else {
                // Reload the current Tenant object
                this.Tenant().Load(JSON.parse(object));
                // Update items on the user object.
                var userTenant_1 = ko.utils.arrayFirst(this.User().userTenants(), function (item) {
                    return item.tenantId() == tenantId;
                });
                if (userTenant_1 != null) {
                    userTenant_1.tenantName(t.name());
                }
                var userT = ko.utils.arrayFirst(this.User().tenants(), function (item) {
                    return item.tenantId() == tenantId;
                });
                if (userT != null) {
                    userT.name(t.name());
                }
            }
        }
    };
    /**
     * Toggles the sticky state of the top fixed menus
     */
    MainModel.prototype.ToggleStickyMenus = function () {
        if (this.StickyMenus()) {
            this.StickyMenus(false);
            tsUtilities.CookieWrite("sticky-menus", "0");
        }
        else {
            this.StickyMenus(true);
            tsUtilities.CookieWrite("sticky-menus", "1");
        }
    };
    /**
     * Gets the value for a given UDF
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    MainModel.prototype.UDFFieldGetValue = function (module, item) {
        var output = "";
        if (this.Loaded() == true && module != undefined && module != null && module != "") {
            var m = module.toLowerCase();
            var id = m + "-udf-" + item.toString();
            var type = this.UDFFieldType(module, item);
            switch (type) {
                case "input":
                case "select":
                    output = $("#" + id).val();
                    break;
                case "radio":
                    output = $('input[name="' + id + '"]:checked').val();
                    break;
            }
        }
        return output;
    };
    /**
     * If this UDF has options (eg: text separated by pipe characters) then those options are returned.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    MainModel.prototype.UDFFieldOptions = function (module, item) {
        var output = [];
        if (this.Loaded() == true) {
            var values = "";
            var udf_1 = "UDF" + (item < 10 ? "0" : "") + item.toString();
            var match = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf_1;
            });
            if (match != null && match.label() != null && match.label() != "") {
                values = match.label();
            }
            if (values.indexOf("|") > -1) {
                var items = values.split("|");
                if (items != null && items.length > 2) {
                    values = items[2].trim();
                    if (values == undefined || values == null) {
                        values = "";
                    }
                }
                if (values != "") {
                    var items_1 = values.split(",");
                    if (items_1 != null && items_1.length > 0) {
                        items_1.forEach(function (e) {
                            output.push(e.trim());
                        });
                    }
                }
            }
        }
        return output;
    };
    /**
     * Renders the display-only version of a UDF
     * @param element {string} - The id of the HTML element where the fields should be rendered.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param dataObject {any} - The object that contains the UDF fields (eg: a User object).
     */
    MainModel.prototype.UDFFieldsDisplay = function (element, module, dataObject) {
        var output = "";
        if (this.Loaded() == true) {
            var m = module.toLowerCase();
            var totalItems = m == "assets" ? 20 : 10;
            for (var x = 1; x <= totalItems; x++) {
                if (this.UDFShowField(module, x)) {
                    var label = this.UDFLabel(module, x);
                    var options = [];
                    var udfField = "udf" + (x < 10 ? "0" : "") + x.toString();
                    var value = dataObject[udfField];
                    if (tsUtilities.HasValue(label)) {
                        output +=
                            "<div class='row padtop'>\n" +
                                "  <div class='col-sm-12'>\n" +
                                "    <strong>" + label + "</strong>\n" +
                                "    <div>" + value + "</div>\n" +
                                "  </div>\n" +
                                "</div>\n";
                    }
                }
            }
            $("#" + element).html(output);
            this.UDFFieldsSetValues(module, dataObject);
        }
        else {
            $("#" + element).html("");
        }
    };
    /**
     * Renders the input version of a UDF
     * @param element {string} - The id of the HTML element where the fields should be rendered.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param dataObject {any} - The object that contains the UDF fields (eg: a User object).
     */
    MainModel.prototype.UDFFieldsRender = function (element, module, dataObject) {
        //console.log("UDFFieldsRender", element, module, dataObject);
        var output = "";
        if (this.Loaded() == true) {
            var m = module.toLowerCase();
            var totalItems = m == "assets" ? 20 : 10;
            var _loop_1 = function (x) {
                if (this_1.UDFShowField(module, x)) {
                    var label = this_1.UDFLabel(module, x);
                    var id_1 = m + "-udf-" + x.toString();
                    var options = [];
                    if (tsUtilities.HasValue(label)) {
                        output +=
                            "<div class='row padtop'>\n" +
                                "  <div class='col-sm-12'>\n" +
                                "    <label for='" + id_1 + "'>" + label + "</label>\n";
                        var type = this_1.UDFFieldType(module, x);
                        switch (type) {
                            case "input":
                                output +=
                                    "    <input type='text' class='form-control' id='" + id_1 + "'>";
                                break;
                            case "select":
                                options = this_1.UDFFieldOptions(module, x);
                                output += "    <select class='form-control' id='" + id_1 + "'>\n";
                                if (options != null && options.length > 0) {
                                    output += "      <option value=\"\">Select a Value</option>\n";
                                    options.forEach(function (item) {
                                        output += "      <option value=\"" + item + "\">" + item + "</option>\n";
                                    });
                                }
                                output += "    </select>\n";
                                break;
                            case "radio":
                                options = this_1.UDFFieldOptions(module, x);
                                if (options != null && options.length > 0) {
                                    var o_1 = 0;
                                    options.forEach(function (item) {
                                        o_1++;
                                        output +=
                                            "    <div>\n" +
                                                "      <input type='radio' name='" + id_1 + "' id='" + id_1 + "-" + o_1.toString() + "' value=\"" + item + "\">\n" +
                                                "      <label for='" + id_1 + "-" + o_1.toString() + "' class='unbold'>" + item + "</label>\n" +
                                                "    </div>\n";
                                    });
                                }
                                break;
                        }
                        output +=
                            "  </div>\n" +
                                "</div>\n";
                    }
                }
            };
            var this_1 = this;
            for (var x = 1; x <= totalItems; x++) {
                _loop_1(x);
            }
            $("#" + element).html(output);
            this.UDFFieldsSetValues(module, dataObject);
        }
        else {
            $("#" + element).html("");
        }
    };
    /**
     * Gets the values from the UDF items and stores them back in the object.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param observableObject {any} - The object that contains the UDF fields (eg: a User object).
     */
    MainModel.prototype.UDFFieldsGetValues = function (module, observableObject) {
        var m = module.toLowerCase();
        var totalItems = m == "assets" ? 20 : 10;
        for (var x = 1; x <= totalItems; x++) {
            var label = this.UDFLabel(module, x);
            if (tsUtilities.HasValue(label)) {
                var id = m + "-udf-" + x.toString();
                var type = this.UDFFieldType(module, x);
                var value = "";
                switch (type) {
                    case "input":
                    case "select":
                        value = $("#" + id).val();
                        break;
                    case "radio":
                        value = $('input[name="' + id + '"]:checked').val();
                        break;
                }
                switch (x) {
                    case 1:
                        observableObject.udf01(value);
                        break;
                    case 2:
                        observableObject.udf02(value);
                        break;
                    case 3:
                        observableObject.udf03(value);
                        break;
                    case 4:
                        observableObject.udf04(value);
                        break;
                    case 5:
                        observableObject.udf05(value);
                        break;
                    case 6:
                        observableObject.udf06(value);
                        break;
                    case 7:
                        observableObject.udf07(value);
                        break;
                    case 8:
                        observableObject.udf08(value);
                        break;
                    case 9:
                        observableObject.udf09(value);
                        break;
                    case 10:
                        observableObject.udf10(value);
                        break;
                    case 11:
                        observableObject.udf11(value);
                        break;
                    case 12:
                        observableObject.udf12(value);
                        break;
                    case 13:
                        observableObject.udf13(value);
                        break;
                    case 14:
                        observableObject.udf14(value);
                        break;
                    case 15:
                        observableObject.udf15(value);
                        break;
                    case 16:
                        observableObject.udf16(value);
                        break;
                    case 17:
                        observableObject.udf17(value);
                        break;
                    case 18:
                        observableObject.udf18(value);
                        break;
                    case 19:
                        observableObject.udf19(value);
                        break;
                    case 20:
                        observableObject.udf20(value);
                        break;
                }
            }
        }
    };
    /**
     * Sets the values for the rendered UDF items.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param dataObject {any} - The object that contains the UDF fields (eg: a User object).
     */
    MainModel.prototype.UDFFieldsSetValues = function (module, dataObject) {
        var m = module.toLowerCase();
        var totalItems = m == "assets" ? 20 : 10;
        for (var x = 1; x <= totalItems; x++) {
            var label = this.UDFLabel(module, x);
            if (tsUtilities.HasValue(label)) {
                var udfField = "udf" + (x < 10 ? "0" : "") + x.toString();
                var id = m + "-udf-" + x.toString();
                var type = this.UDFFieldType(module, x);
                var value = dataObject[udfField];
                switch (type) {
                    case "input":
                    case "select":
                        $("#" + id).val(value);
                        break;
                    case "radio":
                        $('input[name="' + id + '"]').val([value]);
                        break;
                }
            }
        }
    };
    /**
     * Determines the type of input to render for the given UDF.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    MainModel.prototype.UDFFieldType = function (module, item) {
        var output = "input";
        if (this.Loaded() == true) {
            var label = "";
            var udf_2 = "UDF" + (item < 10 ? "0" : "") + item.toString();
            var match = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf_2;
            });
            if (match != null && match.label() != null && match.label() != "") {
                label = match.label();
            }
            if (label.indexOf("|") > -1) {
                var items = label.split("|");
                if (items != null && items.length > 1) {
                    output = items[1].trim();
                    if (output == undefined || output == null || output == "") {
                        output = "input";
                    }
                }
            }
        }
        return output.toLowerCase();
    };
    /**
     * Gets the filter options for the given UDF.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    MainModel.prototype.UDFFilterOptions = function (module, item) {
        var output = [];
        if (this.Loaded() == true) {
            var udf_3 = "UDF" + (item < 10 ? "0" : "") + item.toString();
            var match = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf_3;
            });
            if (match != null && match.filterOptions() != null && match.filterOptions().length > 0) {
                match.filterOptions().forEach(function (item) {
                    output.push(item);
                });
            }
        }
        return output;
    };
    /**
     * Gets the label for the given UDF.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    MainModel.prototype.UDFLabel = function (module, item) {
        var output = "";
        if (this.Loaded() == true) {
            var udf_4 = "UDF" + (item < 10 ? "0" : "") + item.toString();
            var match = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf_4;
            });
            if (match != null && match.label() != null && match.label() != "") {
                output = match.label();
            }
            if (output.indexOf("|") > -1) {
                output = output.substr(0, output.indexOf("|"));
            }
        }
        return output;
    };
    /**
     * Determines if the given UDF should be shown in a column.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    MainModel.prototype.UDFShowColumn = function (module, item) {
        var output = false;
        if (this.Loaded() == true) {
            var udf_5 = "UDF" + (item < 10 ? "0" : "") + item.toString();
            var match = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf_5;
            });
            if (match != null && match.label() != null && match.label() != "" && match.showColumn() == true) {
                output = true;
            }
        }
        return output;
    };
    /**
     * Determines if the given UDF should be shown in the interface when editing or viewing an item.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    MainModel.prototype.UDFShowField = function (module, item) {
        var output = false;
        if (this.Loaded() == true) {
            var label = this.UDFLabel(module, item);
            if (tsUtilities.HasValue(label)) {
                output = true;
            }
        }
        return output;
    };
    /**
     * Determines if the given UDF should be shown in the filter options.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    MainModel.prototype.UDFShowInFilter = function (module, item) {
        var output = false;
        if (this.Loaded() == true) {
            var udf_6 = "UDF" + (item < 10 ? "0" : "") + item.toString();
            var match = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf_6;
            });
            if (match != null && match.label() != null && match.label() != "" && match.showInFilter() == true) {
                output = true;
            }
        }
        return output;
    };
    /**
     * Called on load and when the window resizes to keep track of whether the mobile menu is visible or not, as well as the window width and height.
     */
    MainModel.prototype.UpdateWindowSettings = function () {
        this.MobileMenu($("#menu-bar-toggler").is(":visible"));
        this.WindowHeight($(window).height());
        this.WindowWidth($(window).width());
    };
    /**
     * Views a file. If the file is an image it will be viewed in place with the colorbox plugin.
     * @param {string} fileId - The fileId of the file to view.
     */
    MainModel.prototype.ViewFile = function (fileId) {
        var _this = this;
        var success = function (data) {
            if (data != null) {
                if (data.actionResponse.result) {
                    //console.log("View File", data);
                    var ext = "";
                    if (tsUtilities.HasValue(data.extension)) {
                        ext = data.extension.replace(".", "").toLowerCase();
                    }
                    switch (ext) {
                        case "png":
                        case "jpg":
                        case "jpeg":
                        case "gif":
                            _this.ViewImage(window.baseURL + "File/View/" + fileId);
                            break;
                        default:
                            break;
                    }
                }
                else {
                    _this.Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                console.log("Error loading file", fileId);
            }
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetFileStorage/" + fileId, null, success);
    };
    /**
     * Views an image using the colorbox plugin.
     * @param {string} url - The url to the image.
     */
    MainModel.prototype.ViewImage = function (url) {
        $.colorbox({
            href: url,
            photo: true,
            maxWidth: "95%"
        });
    };
    return MainModel;
}());
//# sourceMappingURL=main.js.map