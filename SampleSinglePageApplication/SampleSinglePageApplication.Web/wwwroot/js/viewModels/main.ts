interface Window {
    departmentsModel: DepartmentsModel;
    languageModel: LanguageModel;
    mainModel: MainModel;
    profileModel: ProfileModel;
    settingsModel: SettingsModel;
    tenantsModel: TenantsModel;
    udfLabelsModel: UdfLabelsModel;
    usersModel: UsersModel;
}
/**
 * enum types used for representing various types of interface styles
 */
enum StyleType {
    Danger,
    Dark,
    Default,
    Info,
    Light,
    Primary,
    Secondary,
    Success,
    Warning
}

/**
 * The main viewmodel shared among all pages.
 */
class MainModel {
    AccessDeniedMessage: KnockoutObservable<string> = ko.observable("");
    Action: KnockoutObservable<string> = ko.observable("");
    AppOnline: KnockoutObservable<boolean> = ko.observable(true);
    Culture: KnockoutObservable<string> = ko.observable(window.culture);
    Cultures: KnockoutObservableArray<string> = ko.observableArray([]);
    CultureCodes: KnockoutObservableArray<optionPair> = ko.observableArray([]);
    DefaultLanguageItems: KnockoutObservableArray<optionPair> = ko.observableArray([]);
    Extra: KnockoutObservableArray<string> = ko.observableArray([]);
    GuidEmpty: KnockoutObservable<string> = ko.observable("00000000-0000-0000-0000-000000000000");
    Guid1: KnockoutObservable<string> = ko.observable("00000000-0000-0000-0000-000000000001");
    Guid2: KnockoutObservable<string> = ko.observable("00000000-0000-0000-0000-000000000002");
    Id: KnockoutObservable<string> = ko.observable("");
    LanguageItems: KnockoutObservableArray<optionPair> = ko.observableArray([]);
    Loaded: KnockoutObservable<boolean> = ko.observable(false);
    LoggedIn: KnockoutObservable<boolean> = ko.observable(false);
    MessageText: KnockoutObservable<string> = ko.observable("");
    MessageClass: KnockoutObservable<string> = ko.observable("");
    MobileMenu: KnockoutObservable<boolean> = ko.observable(false);
    PreferredColorScheme: KnockoutObservable<string> = ko.observable(null);
    ShowTenantCodeFieldOnLoginForm: KnockoutObservable<boolean> = ko.observable(false);
    ShowTenantListingWhenMissingTenantCode: KnockoutObservable<boolean> = ko.observable(false);
    SignalRUpdate: KnockoutObservable<signalRUpdate> = ko.observable(new signalRUpdate);
    StickyMenus: KnockoutObservable<boolean> = ko.observable(false);
    Tenant: KnockoutObservable<tenant> = ko.observable(new tenant);
    TenantCode: KnockoutObservable<string> = ko.observable("");
    TenantId: KnockoutObservable<string> = ko.observable("");
    Tenants: KnockoutObservableArray<tenantList> = ko.observableArray([]);
    Theme: KnockoutObservable<string> = ko.observable("");
    Token: KnockoutObservable<string> = ko.observable("");
    User: KnockoutObservable<user> = ko.observable(new user);
    Users: KnockoutObservableArray<user> = ko.observableArray([]);
    UseTenantCodeInUrl: KnockoutObservable<boolean> = ko.observable(false);
    View: KnockoutObservable<string> = ko.observable("");
    VersionInfo: KnockoutObservable<versionInfo> = ko.observable(new versionInfo);
    WindowHeight: KnockoutObservable<number> = ko.observable(0);
    WindowWidth: KnockoutObservable<number> = ko.observable(0);

    messageTimer: TimerHandler;
    messageTimerHandle: number;
    messageTimerInterval: number;

    /**
     * The main viewmodel shared among all pages.
     */
    constructor() {
        tsUtilities.KnockoutCustomBindingHandlers();
        this.messageTimerInterval = 3000;
        this.Token(window.token);

        this.Culture.subscribe(() => this.CultureChanged());
        let savedCulture: string = tsUtilities.CookieRead(window.appName + "-culture");
        if (tsUtilities.HasValue(savedCulture)) {
            this.Culture(savedCulture);
        }

        this.ShowTenantCodeFieldOnLoginForm(window.showTenantCodeFieldOnLoginForm);
        this.ShowTenantListingWhenMissingTenantCode(window.showTenantListingWhenMissingTenantCode);
        this.UseTenantCodeInUrl(window.useTenantCodeInUrl);

        if (window.loggedIn != undefined && window.loggedIn != null && window.loggedIn == true) {
            this.LoggedIn(true);
        }

        DataLoader(window.objCultureCodes, this.CultureCodes, optionPair);

        if (window.objCultures != null && window.objCultures.length > 0) {
            this.Cultures(window.objCultures);
        }

        this.LoadLanguage(window.objLanguage, window.objDefaultLanguage);

        if (window.objExtra != undefined && window.objExtra != null) {
            this.Extra(window.objExtra);
        }

        if (window.objTenant != undefined && window.objTenant != null) {
            this.Tenant().Load(window.objTenant);
        }

        DataLoader(window.objTenantList, this.Tenants, tenantList);

        if (window.objUser != undefined && window.objUser != null) {
            this.User().Load(window.objUser);
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

        this.UpdateWindowSettings();
        $(window).off("resize");
        $(window).on("resize", () => {
            this.UpdateWindowSettings();
        });

        window.onpopstate = () => { this.PopStateChanged(); }

        let stickyMenus: string = window.localStorage.getItem(window.appName + "-sticky-menus");
        if (!tsUtilities.HasValue(stickyMenus)) {
            stickyMenus = "0";
        }
        this.StickyMenus(stickyMenus == "1");

        let colorScheme: string = getComputedStyle(document.body, ':after').content;
        if (tsUtilities.HasValue(colorScheme)) {
            this.PreferredColorScheme(colorScheme.indexOf("dark") > -1 ? "dark" : "light");
        }

        this.CheckLoad();

        this.CheckForUpdates();

        let theme: string = window.localStorage.getItem(window.appName + "-theme");
        if (!tsUtilities.HasValue(theme)) {
            theme = "";
        }
        this.Theme(theme);
        this.Theme.subscribe(function (v) {
            window.localStorage.setItem(window.appName + "-theme", v);
        });

        setTimeout(() => this.ThemeWatcher(), 1000);

        this.GetTenantUsers();
    }

    AccessDenied(message?: string): void {
        console.log("Access Denied", message);
        this.AccessDeniedMessage("");

        if (message != undefined && message != null && message != "") {
            this.AccessDeniedMessage(message);
        }

        this.Nav("AccessDenied");
    }

    AdminUser = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.User() != null) {
            if (this.User().admin() || this.User().appAdmin()) {
                output = true;
            }
        }

        return output;
    });

    AjaxUserAutocomplete(element: string, callbackHandler: Function, localResultsOnly?: boolean): void {
        if (localResultsOnly == undefined || localResultsOnly == null) {
            localResultsOnly = false;
        }

        $("#" + element).autocomplete({
            source: localResultsOnly ? this.AjaxUserSearchLocalOnly : this.AjaxUserSearch,
            minLength: 3,
            select: (event, ui) => {
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
            } else {
                return $("<li>")
                    .append("<a>" + item.label + "</a>")
                    .appendTo(ul);
            }
        };
    }

    AjaxUserSearch(req: any, callbackHandler: any) {
        let lookup: ajaxLookup = new ajaxLookup();
        lookup.Search(req.term);
        lookup.TenantId(window.tenantId);

        $.ajax({
            url: window.baseURL + "api/Data/AjaxUserSearch/",
            type: 'POST',
            data: ko.toJSON(lookup),
            beforeSend: setRequestHeaders,
            contentType: "application/json; charset=utf-8",
            success: (data: server.ajaxLookup) => {
                let results: ajaxResults[] = [];
                if (data != null) {
                    if (data.results != null) {
                        data.results.forEach(function (element) {
                            let item: ajaxResults = new ajaxResults();
                            item.Load(element);
                            results.push(item);
                        });
                    }
                }
                callbackHandler(JSON.parse(ko.toJSON(results)));
            }
        });
    }

    AjaxUserSearchLocalOnly(req: any, callbackHandler: any) {
        let lookup: ajaxLookup = new ajaxLookup();
        lookup.Search(req.term);
        lookup.TenantId(window.tenantId);

        $.ajax({
            url: window.baseURL + "api/Data/AjaxUserSearchLocalOnly/",
            type: 'POST',
            data: ko.toJSON(lookup),
            beforeSend: setRequestHeaders,
            contentType: "application/json; charset=utf-8",
            success: (data: server.ajaxLookup) => {
                let results: ajaxResults[] = [];
                if (data != null) {
                    if (data.results != null) {
                        data.results.forEach(function (element) {
                            let item: ajaxResults = new ajaxResults();
                            item.Load(element);
                            results.push(item);
                        });
                    }
                }
                callbackHandler(JSON.parse(ko.toJSON(results)));
            }
        });
    }

    /**
     * Determines if the image file is an allowed image file type.
     * @param file {string} - The file name to validate.
     */
    AllowedFileTypeImage(file: string): boolean {
        let output: boolean = false;
        this.Message_Hide();

        let ext: string = tsUtilities.GetExtension(file);
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
    }

    AvailableCultures = ko.computed((): optionPair[] => {
        let output: optionPair[] = [];

        if (this.CultureCodes() != null && this.CultureCodes().length > 0) {
            let cc: string[] = this.Cultures();

            output = ko.utils.arrayFilter(this.CultureCodes(), function (item) {
                return cc.indexOf(item.id()) == -1;
            });
        }

        return output;
    });

    BlockedModules = ko.computed((): string[] => {
        let output: string[] = [];

        if (this.Tenant() != null && this.Tenant().tenantSettings() != null && this.Tenant().tenantSettings().moduleHideElements() != null) {
            output = this.Tenant().tenantSettings().moduleHideElements();
        }

        return output;
    });

    BlockModuleDepartments = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.BlockedModules().length > 0) {
            output = this.BlockedModules().indexOf("departments") > -1;
        }

        return output;
    });

    BlockModuleEmployeeId = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.BlockedModules().length > 0) {
            output = this.BlockedModules().indexOf("employeeid") > -1;
        }

        return output;
    });

    BlockModuleLocation = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.BlockedModules().length > 0) {
            output = this.BlockedModules().indexOf("location") > -1;
        }

        return output;
    });

    BlockModuleUDF = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.BlockedModules().length > 0) {
            output = this.BlockedModules().indexOf("udf") > -1;
        }

        return output;
    });

    BlockModuleUserGroups = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.BlockedModules().length > 0) {
            output = this.BlockedModules().indexOf("usergroups") > -1;
        }

        return output;
    });

    /**
     * Observable that applies the necessary styling to the body element for theme support.
     */
    BodyClass = ko.computed((): void => {
        let output: string = "";

        if (this.PreferredColorScheme() != null) {
            switch (this.Theme()) {
                case "dark":
                    output = "dark-theme";
                    break;

                case "light":
                    output = "light-theme";
                    break;

                default:
                    // Auto
                    output = this.PreferredColorScheme() + "-theme";
                    this.ThemeWatcher();
                    break;
            }
        }

        $("#body-element").removeClass();
        $("#body-element").addClass(output);
    });

    /**
     * Used to render an icon from a boolean value.
     * @param {boolean} value - The value to check.
     * @param {string} icon - OPTIONAL: an icon to use if the boolean value is true. Defaults to teh Selected icon.
     */
    BooleanToIcon(value: boolean, icon?: string): string {
        let output: string = "";

        if (value != undefined && value != null && value == true) {
            if (icon != undefined && icon != null && icon != "") {
                output = this.Icon(icon);
                if (output == "") {
                    output = icon;
                }
            } else {
                output = this.Icon("Selected");
            }
        }

        return output;
    }

    /**
     * Checks the VersionInfo from the server to see if the server has restarted or been updated.
     */
    CheckForUpdates(): void {
        let success: Function = (data: server.versionInfo) => {
            let reload: boolean = false;

            if (data != null) {
                if (tsUtilities.HasValue(data.version)) {
                    this.AppOnline(true);
                }

                if (this.VersionInfo().released() != data.released || this.VersionInfo().runningSince() != data.runningSince || this.VersionInfo().version() != data.version) {
                    reload = true;
                }
            }

            if (reload) {
                this.Nav("ServerUpdated");
            } else {
                setTimeout(() => this.CheckForUpdates(), 10000);
            }
        };

        let failure: Function = () => {
            this.AppOnline(false);
            setTimeout(() => this.CheckForUpdates(), 2000);
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/GetVersionInfo", null, success, failure);
    }

    /**
     * Make sure all required models have been loaded before performing the first navigation.
     */
    CheckLoad(): void {
        // Get the models from the window object. Since this model is loaded first, the
        // object might not contain any values initially. If that is the case, set a small
        // delay to check again.
        let models: string[] = [];
        if (window.models != undefined && window.models != null && window.models.length > 0) {
            models = window.models;
        } else {
            setTimeout(() => this.CheckLoad(), 500);
        }

        let missingModels: string[] = [];

        models.forEach(function (model) {
            if (eval("typeof " + model) === 'undefined') {
                missingModels.push(model);
            }
        });

        if (missingModels.length > 0) {
            console.log("missingModels", missingModels);
            setTimeout(() => this.CheckLoad(), 200);
        } else {
            // Everything is loaded, but give a small delay to allow time for the other
            // models to bind to the change event for the View property in this model.
            this.Loaded(true);
            setTimeout(() => this.Nav(this.Action(), this.Id()), 500);
        }
    }

    CsvToListOfString(csv: string): string[] {
        let output: string[] = [];

        if (tsUtilities.HasValue(csv)) {
            let items: string[] = csv.split(',');
            if (items != null && items.length > 0) {
                items.forEach(function (e) {
                    let value: string = $.trim(e);
                    if (tsUtilities.HasValue(value)) {
                        output.push(value);
                    }
                });
            }
        }

        return output;
    }

    CultureChanged(): void {
        let success: Function = (data: server.language) => {
            if (data != null) {
                tsUtilities.CookieWrite(window.appName + "-culture", this.Culture());
                this.LoadLanguage(data, null);
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/GetLanguage/" + this.Culture(), null, success);
    }

    CurrentCultures = ko.computed((): optionPair[] => {
        let output: optionPair[] = [];

        if (this.Cultures() != null && this.Cultures().length > 0) {
            for (let x: number = 0; x < this.Cultures().length; x++) {
                let code: string = this.Cultures()[x];
                if (tsUtilities.HasValue(code)) {
                    let name: string = this.LanguageName(code);
                    if (tsUtilities.HasValue(name)) {
                        let item: optionPair = new optionPair();
                        item.id(code);
                        item.value(name);
                        output.push(item);
                    }
                }
            }

            if (output.length > 0) {
                output = output.sort(function (l, r) {
                    return l.value() > r.value() ? 1 : -1;
                });
            }
        }

        return output;
    });

    /**
     * The current page URL
     */
    CurrentUrl(): string {
        let url: string = window.baseURL;

        if (window.useTenantCodeInUrl && this.Tenant() != null && tsUtilities.HasValue(this.Tenant().tenantCode())) {
            url += this.Tenant().tenantCode() + "/";
        }

        if (tsUtilities.HasValue(this.Action())) {
            url += this.Action() + "/";

            if (tsUtilities.HasValue(this.Id())) {
                url += this.Id();
            }
        }

        return url;
    }

    /**
     * An observable that always returns the current view as lowercase. Primarily used in the _Layout page
     * to highlight active menu elements.
     */
    CurrentView = ko.computed((): string => {
        let output: string = "";

        if (tsUtilities.HasValue(this.View())) {
            output = this.View().toLowerCase();
        }

        return output;
    });

    /**
     * Returns true if the current view is part of the Admin menu. This includes not only the main links, but other sub-URLs (eg: EditUser for the User sub-menu item)
     */
    CurrentViewAdmin = ko.computed((): boolean => {
        let output: boolean = false;

        let view: string = this.CurrentView();
        if (tsUtilities.HasValue(view)) {
            switch (view.toLowerCase()) {
                // <BEGIN ADMIN MENU ITEMS>
                // <END ADMIN MENU ITEMS>
                case "appsettings":
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

    /**
     * Gets the ticks from a date.
     * @param {Date} date - The date.
     * @returns {number} - Returns the ticekts from the date using the .getTime method.
     */
    DateTicks(date: Date): number {
        let output: number = 0;

        if (date != undefined && date != null) {
            let d: Date = new Date(date);
            output = d.getTime();
        }

        return output;
    }

    /**
     * Gets the name of a department group from the Id. Used in the departments view table to show the groups when the Id is the only data element available.
     * @param departmentGroupId {string} - The unique Id of the department group.
     */
    DepartmentGroupNameFromId(departmentGroupId: string): string {
        let output: string = "";

        if (this.Loaded()) {
            if (this.Tenant().departmentGroups() != null && this.Tenant().departmentGroups().length > 0) {
                let group: departmentGroup = ko.utils.arrayFirst(this.Tenant().departmentGroups(), function (item) {
                    return item.departmentGroupId() == departmentGroupId;
                });
                if (group != null) {
                    output = group.departmentGroupName();
                }
            }
        }

        return output;
    }

    GetTenantUsers(): void {
        let success: Function = (data: server.user[]) => {
            DataLoader(data, this.Users, user);
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUsers/" + this.TenantId(), null, success);
    }

    HtmlEditorFocus(element: string): void {
        if (tsUtilities.HtmlEditorExists(element)) {
            tsUtilities.HtmlEditorFocus(element);
        } else {
            $("#" + element).focus();
        }
    }

    /**
     * I wanted a simpler way to manage my interface icons, so that if I decided to change an icon for an action (eg: save) I can
     * simply change it in one place and all the calls to get the icon returns the icon. Currently I'm using FontAwesome icons.
     * However, this just returns HTML, so these could be images, external links to images or SVG definitions, etc.
     * @param module {string} - The parameter indicating what icon this is. These match language item names for convenience when using the IconAndText option.
     */
    Icon(module: string): string {
        let output: string = "";

        if (tsUtilities.HasValue(module)) {
            switch (module.toLowerCase()) {
                // <BEGIN ICON ITEMS>
                // <END ICON ITEMS>
                case "accessdenied":
                    output = '<i class="fa-regular fa-shield-exclamation"></i>';
                    break;
                case "addmodule":
                    output = '<i class="fa-solid fa-code"></i>';
                    break;
                case "addnewuser":
                    output = '<i class="fas fa-user-plus"></i>';
                    break;
                case "add":
                case "addnewdepartmentgroup":
                case "addlanguage":
                case "addnewusergroup":
                case "addtenant":
                    output = '<i class="fas fa-plus-square"></i>';
                    break;
                case "admin":
                    output = '<i class="fa-solid fa-sliders"></i>';
                    break;
                case "allitems":
                case "showfilter":
                    output = '<i class="fa-solid fa-filter-circle-xmark"></i>';
                    break;
                case "appsettings":
                    output = '<i class="fa-solid fa-shield-keyhole"></i>';
                    break;
                case "back":
                case "backtologin":
                    output = '<i class="fas fa-arrow-left"></i>';
                    break;
                case "cancel":
                    output = '<i class="fas fa-times"></i>';
                    break;
                case "changepassword":
                case "passwordchanged":
                case "useradmin":
                    output = '<i class="fa-solid fa-lock"></i>';
                    break;
                case "clear":
                    output = '<i class="fas fa-broom"></i>';
                    break;
                case "close":
                case "closedialog":
                    output = '<i class="fas fa-times"></i>';
                    break;
                case "code":
                case "html":
                    output = '<i class="fa-solid fa-code"></i>';
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
                case "forgotpassword":
                    output = '<i class="fas fa-unlock"></i>';
                    break;
                case "hidefilter":
                case "modifieditems":
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
                case "loginwithcustom":
                    output = '<i class="fa-solid fa-user-lock"></i>';
                    break;
                case "loginwitheitsso":
                    output = '<i class="okta-logo"></i>';
                    break;
                case "loginwithfacebook":
                    output = '<i class="fa-brands fa-facebook"></i>';
                    break;
                case "loginwithgoogle":
                    output = '<i class="fa-brands fa-google"></i>';
                    break;
                case "loginwithmicrosoftaccount":
                    output = '<i class="fa-brands fa-microsoft"></i>';
                    break;
                case "loginwithopenid":
                    output = '<i class="fa-brands fa-openid"></i>';
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
                case "photo":
                    output = '<i class="fa-regular fa-image"></i>';
                    break;
                case "samplepage":
                    output = '<i class="fa-regular fa-file-lines"></i>';
                    break;
                case "recordstableiconadmin":
                    output = '<i class="fa-solid fa-user-lock"></i>';
                    break;
                case "recordstableiconenabled":
                    output = '<i class="fa-solid fa-user-check"></i>';
                    break;
                case "refresh":
                    output = '<i class="fas fa-sync-alt"></i>';
                    break;
                case "reset":
                    output = '<i class="fa-solid fa-rotate-left"></i>';
                    break;
                case "resetpassword":
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
                case "userenabled":
                    output = '<i class="fa-solid fa-circle-check"></i>';
                    break;
                case "usergroups":
                    output = '<i class="fa-solid fa-people-group"></i>';
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
                case "signup":
                    output = '<i class="fa-solid fa-signature"></i>';
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
                    output = '<i class="fa-solid fa-user"></i>';
                    break;
                case "validateconfirmationcode":
                    output = '<i class="fa-solid fa-shield"></i>';
                    break;
            }
        }

        if (output != "") {
            output = "<span class='icon-element'>" + output + "</span>";
        }

        return output;
    }

    /**
     * Gets the icon and the language for a given item name.
     * @param module{string} - The parameter indicating what icon and language item this is.
     */
    IconAndText(module: string): string {
        let output: string = this.Icon(module) + "<span class='icon-text'>" + this.Language(module) + "</span>";
        return output;
    }

    /**
     * Inserts text into a field at the current cursor position.
     * @param {string} field - The id of the input element.
     * @param {string} value - The value to insert.
     */
    InsertAtCursor(field: string, value: string): void {
        insertAtCursor(field, value);
    }

    InvalidTenantCodeMessage = ko.computed(() => {
        let msg: string = "<h1>" + this.Language("InvalidTenantCode") + "</h1><p>" + this.Language("InvalidTenantCodeMessage") + "</p>";

        if (this.ShowTenantListingWhenMissingTenantCode()) {
            msg += "<div class='mb-2'>" + this.Language("SelectTenant") + "</div>";

            this.Tenants().forEach(function (tenant) {
                msg += "<div><a href='" + window.baseURL + tenant.tenantCode() + "'>" + tenant.name() + "</a></div>";
            });
        }

        $("#invalid-tenant-code-message").html(msg);
    });

    IsGuid(value: string): boolean {
        let output: boolean = false;

        if (tsUtilities.HasValue(value)) {
            if (value.indexOf("-") > -1) {
                let parts: string[] = value.split("-");
                if (parts.length == 5) {
                    if (parts[0].length == 8 && parts[1].length == 4 && parts[2].length == 4 && parts[3].length == 4 && parts[4].length == 12) {
                        output = true;
                    }
                }
            }
        }

        return output;
    }

    /**
     * Gets the language text for a given language item.
     * @param item {string} - The unique id of the language item.
     * @param defaultValue {string} - An optional default value to use if the language item is not found.
     * @param removeSpaces {string} - An option to replace spaces with &nbsp; characters. This is used when showing the item where you don't want it to wrap.
     * @param replaceValues {string} - For language items that use replacement properties (eg: {0}) this is an array of strings that relate to the index of the item values.
     */
    Language(item: string, defaultValue?: string, removeSpaces?: boolean, replaceValues?: string[]): string {
        let output: string = item.toUpperCase();

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
            let index: number = 0;
            replaceValues.forEach(function (item) {
                output = output.replace("{" + index.toString() + "}", item);
                index++;
            });
        }

        if (removeSpaces != undefined && removeSpaces != null && removeSpaces == true) {
            output = tsUtilities.ReplaceSpaces(output);
        }

        output = output.replace(/(?:\r\n|\r|\n)/g, '<br>');

        return output;
    }

    LanguageName(code: string): string {
        let output: string = "";

        if (tsUtilities.HasValue(code)) {
            if (this.CultureCodes() != null && this.CultureCodes().length > 0) {
                let cc: optionPair = ko.utils.arrayFirst(this.CultureCodes(), function (item) {
                    return item.id().toLowerCase() == code.toLowerCase();
                });
                if (cc != null) {
                    output = cc.value();
                }
            }
        }

        return output;
    }

    /**
     * Gets the language text for a given language item and marked it as a required field.
     * @param item {string} - The unique id of the language item.
     * @param defaultValue {string} - An optional default value to use if the language item is not found.
     * @param removeSpaces {string} - An option to replace spaces with &nbsp; characters. This is used when showing the item where you don't want it to wrap.
     * @param replaceValues {string} - For language items that use replacement properties (eg: {0}) this is an array of strings that relate to the index of the item values.
     */
    LanguageRequired(item: string, defaultValue?: string, removeSpaces?: boolean, replaceValues?: string[]): string {
        let output: string = this.Language(item, defaultValue, removeSpaces, replaceValues);

        return "<span class='required'>* " + output + "</span>";
    }

    LinesInText(text: string): number {
        let output: number = 1;

        if (tsUtilities.HasValue(text)) {
            output = text.split(/\r\n|\r|\n/).length;
        }

        if (output < 3) {
            output = 3;
        }

        return output;
    }

    ListItemNameFromId(itemId: string): string {
        let output: string = "";

        if (this.Loaded()) {
            let item: listItem = ko.utils.arrayFirst(this.Tenant().listItems(), function (item) {
                return item.id() == itemId;
            });
            if (item != null) {
                output = item.name();
            }
        }

        return output;
    }

    ListOfStringsToCSV(list: string[]): string {
        let output: string = "";

        if (list != null && list.length > 0) {
            list.forEach(function (e) {
                let item: string = $.trim(e);
                if (tsUtilities.HasValue(item)) {
                    if (output != "") { output += ", "; }
                    output += item;
                }
            });
        }

        return output;
    }

    /**
     * Called when the page loads to load the language and default language. Also called when SignalR recieves language updates.
     * @param language {server.language} - An array of optionPair values for the tenant language.
     * @param defaults {server.optionPair[]} - An array of optionPair values for the default language.
     */
    LoadLanguage(language: server.language, defaults: server.optionPair[]) {
        if (language != undefined && language != null && language.phrases != null && language.phrases.length > 0) {
            let options: optionPair[] = [];
            language.phrases.forEach(function (e) {
                let item: optionPair = new optionPair();
                item.Load(e);
                options.push(item);
            });
            this.LanguageItems(options);
        }

        if (defaults != undefined && defaults != null) {
            let defaultOptions: optionPair[] = [];
            defaults.forEach(function (e) {
                let item: optionPair = new optionPair();
                item.Load(e);
                defaultOptions.push(item);
            })
            this.DefaultLanguageItems(defaultOptions);
        }
    }

    /**
     * Called when a user clicks the logout button. Since a redirect happens, the interface elements are hidden first to avoid confusion about the redirect delay.
     */
    Logout(): void {
        // Hide the interface
        $("#main-model").hide();
        $("#page-view-model-area").hide();

        // Remove the local token cookies
        tsUtilities.CookieWrite("Token-" + this.TenantId(), "");

        if (this.User() != null && this.User().userTenants() != null && this.User().userTenants().length > 0) {
            this.User().userTenants().forEach(function (tenant) {
                tsUtilities.CookieWrite("Token-" + tenant.tenantId(), "");
            });
        }

        // Redirect to the local logout page
        let returnUrl: string = window.baseURL;
        if (window.useTenantCodeInUrl) {
            returnUrl += this.TenantCode();
        }

        let url: string = window.baseURL + "Logout/" + this.TenantId() + "?redirect=" + encodeURI(returnUrl);
        window.location.href = url;
    }

    /**
     * Updates the status message shown at the top of the page.
     * @param {string} message - The message to show.
     * @param {StyleType} styleType - An optional style type.
     * @param {boolean} autoHide - Option to auto-hide the message after an interval (defaults to true)
     * @param {boolean} fixedPosition - Optionally show as a fixed position dialog.
     * @param {boolean} small - Optionally indicate the message should be a small message on the right.
     */
    Message(message: string, styleType?: StyleType, autoHide?: boolean, fixedPosition?: boolean, replaceLineBreaksWithHtmlBreaks?: boolean, small?: boolean): void {
        clearTimeout(this.messageTimerHandle);

        if (autoHide == undefined || autoHide == null) {
            autoHide = true;
        }

        if (autoHide == true) {
            this.messageTimerHandle = setTimeout(() => this.Message_Hide(), this.messageTimerInterval);
        }

        if (fixedPosition == undefined || fixedPosition == null) {
            fixedPosition = false;
        }

        if (small == undefined || small == null) {
            small = false;
        }

        let fixed: string = "";
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
            let tag: string = message.replace("{{", "");
            tag = tag.replace("}}", "");
            message = this.Language(tag);
        }

        this.MessageText(message);
    }


    /**
     * Shows a default message when items are being deleted.
     * @param message {string} - An optional message to show. If not passed a default message is shown.
     */
    Message_Deleting(message?: string): void {
        if (!tsUtilities.HasValue(message)) {
            message = this.Language("DeletingWait", "Deleting, PLease Wait...");
        }
        this.Message(message, StyleType.Danger, false, true, true);
    }

    /**
     * Shows a simple error message.
     * @param {string} error - The error message to show.
     */
    Message_Error(error: string): void {
        this.Message(error, StyleType.Danger, false, true, true);
    }

    /**
     * Shows an error message with one or more error messages.
     * @param {string[]} errors - The array of errors.
     * @param {string} intro - An optional intro message. 
     * If not specified defaults to either "The following error has occurred" or 
     * "The following errors have occurred" depending on the error count.
     */
    Message_Errors(errors: string[], intro?: string): void {
        let message: string = "";

        if (errors != undefined && errors != null && errors.length > 0) {
            if (tsUtilities.HasValue(intro)) {
                message += intro;
            } else {
                if (errors.length == 1) {
                    message += "<div class='padbottom'>The following error has occurred:</div>\n";
                } else {
                    message += "<div class='padbottom'>The following errors have occurred:</div>\n";
                }
            }

            if (errors.length == 1) {
                message += errors[0];
            } else {
                message += "<ul>\n";

                errors.forEach(function (msg) {
                    message += "  <li>" + msg + "</li>\n";
                });

                message += "</ul>\n";
            }
        }

        this.Message(message, StyleType.Danger, false, true, false);
    }

    /**
     * Hides the status message, either when the timer expires or the user clicks the close icon.
     */
    Message_Hide() {
        clearTimeout(this.messageTimerHandle);
        this.MessageText("");
    }

    /**
     * Shows a loading message.
     * @param {string} message - Optional loading message. Defaults to "Loading, Please Wait..."
     */
    Message_Loading(message?: string): void {
        if (!tsUtilities.HasValue(message)) {
            message = this.Language("LoadingWait", "Loading, PLease Wait...");
        }
        this.Message(message, StyleType.Dark, false, true, true);
    }

    /**
     * Shows a message indicated the record was saved.
     * @param {string} message - Optionally pass the message. Defaults to "SavedAt" and the long time.
     */
    Message_Saved(message?: string): void {
        if (!tsUtilities.HasValue(message)) {
            message = this.Language("SavedAt", "Saved at") + " " + tsUtilities.FormatTimeLong(new Date());
        }
        this.Message(message, StyleType.Success, true, true, true, true);
    }

    /**
     * Shows a saving message.
     * @param {string} message - Optional saving message. Defaults to "Saving, Please Wait..."
     */
    Message_Saving(message?: string): void {
        if (!tsUtilities.HasValue(message)) {
            message = this.Language("SavingWait", "Saving, Please Wait...");
        }
        this.Message(message, StyleType.Dark, false, true, true);
    }

    /**
     * Used when saving records and showing error messages about missing required elements to simplify those calls.
     * @param fieldName {string} - The name of the element. Should be retrieved from Language(elementName) instead of hard-coding.
     */
    MissingRequiredField(fieldName: string): string {
        let output: string = this.Language("RequiredMissing", "", false, [fieldName]);
        return output;
    }

    /**
     * Handles the Single-Page Application navigation.
     * @param action {string} - The optional action (eg: "Users", "Settings", Tenants", etc.)
     * @param id {string} - An optional Id value. Only used in conjunction with action, so if action is not given but an Id is the Id will be ignored.
     * @param fromPop {boolean} - Set to true when this is called from the window.onpopstate function that handles SPA navigation without reloading the browser.
     */
    Nav(action?: string, id?: string, fromPop?: boolean): void {
        //console.log("Nav", action, id, fromPop);

        if (action == null || action == undefined || action == "undefined" || action == "") {
            action = "";
        }

        if (!tsUtilities.HasValue(action)) {
            if (!this.LoggedIn()) {
                if (window.useTenantCodeInUrl && !tsUtilities.HasValue(window.tenantCode)) {
                    action = "MissingTenantCode";
                } else {
                    action = "Login";
                }
            }
        }

        // If we have a requested URL cookie then clear that cookie and redirect to that page.
        let requestedUrl: string = tsUtilities.CookieRead("requested-url");

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

        let url: string = window.baseURL;
        if (window.useTenantCodeInUrl && this.Tenant() != null && tsUtilities.HasValue(this.Tenant().tenantCode())) {
            url += this.Tenant().tenantCode() + "/";
        }

        if (tsUtilities.HasValue(action)) {
            url += action + "/";

            if (tsUtilities.HasValue(id)) {
                url += id;
            }
        }

        let navChanged: boolean = false;
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
        let allowAnonymousAccess: boolean = false;

        if (!this.LoggedIn()) {
            switch (action.toLowerCase()) {
                case "accessdenied":
                case "login":
                case "missingtenantcode":
                case "serverupdated":

                case "other pages":
                case "that can be":
                case "accessed when":
                case "not logged in":
                    allowAnonymousAccess = true;
                    break;

                default:
                    tsUtilities.CookieWrite("requested-url", url);

                    if (window.useTenantCodeInUrl) {
                        if (tsUtilities.HasValue(this.TenantCode())) {
                            window.location.href = window.baseURL + this.TenantCode() + "/Login";
                        }
                    } else {
                        window.location.href = window.baseURL + "Login";
                    }
                    return;
            }
        }

        // Make sure the user has access to the requested resourse
        let accessDenied: boolean = false;
        if (!allowAnonymousAccess) {
            // This is where you would test various actions to make sure the user has access.
            switch (action.toLowerCase()) {
                case "appsettings":
                case "tenants":
                    accessDenied = !this.User().appAdmin();
                    break;

                case "changepassword":
                    accessDenied = this.User().preventPasswordChange();
                    break;

                case "departments":
                    accessDenied = !this.User().admin() && !this.BlockModuleDepartments();
                    break;

                case "language":
                case "settings":
                case "udflabels":
                case "users":
                    accessDenied = !this.User().admin();
                    break;

                case "usergroups":
                    accessDenied = !this.User().admin() && !this.BlockModuleUserGroups();
                    break;
            }
        }

        if (accessDenied == true) {
            action = "AccessDenied";
            if (window.useTenantCodeInUrl) {
                url = window.baseURL + this.TenantCode() + "/AccessDenied";
            } else {
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
    }

    /**
     * Generates a URL that can be bound to the a element to create links.
     * @param action {string} - The optional action (eg: "Users", "Settings", Tenants", etc.)
     * @param id {string} - An optional Id value. Only used in conjunction with action, so if action is not given but an Id is the Id will be ignored.
     */
    NavLink(action?: string, id?: string): string {
        let output: string = window.baseURL;
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
    }

    /**
     * Formats the select list option items.
     * @param option - The option item.
     * @param {SimpleObject} item - The item from the collection.
     */
    OptionListFormat(option, item: optionPair): void {
        if (item != undefined && item != null) {
            if (!tsUtilities.HasValue(item.value())) {
                ko.applyBindingsToNode(option, { disable: true }, item);
            }

            option.text = item.id().replace(/ /g, '\u00A0');
        }
    }

    Parse(item: any): string {
        let output: string = "";

        if (item != null) {
            output = JSON.parse(ko.toJSON(item));
        }

        return output;
    }

    /**
     * The method that gets called when the window.onpopstate event fires so navigation can be updated.
     */
    PopStateChanged(): void {
        let path: string = document.location.href;

        // Remove the base URL from the path so only the part we are concerned with remains.
        if (tsUtilities.HasValue(path)) {
            if (path.toLowerCase().indexOf(window.baseURL.toLowerCase()) > -1) {
                path = path.substring(window.baseURL.length);
            }
            if (path.indexOf("/") == 0) {
                path = path.substr(1);
            }
        }
        
        let tenantId: string = "";
        let action: string = "";
        let id: string = "";

        if (tsUtilities.HasValue(path)) {
            let parts: string[] = path.split("/");
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
            let url: string = window.baseURL + tenantId + "/";
            if (tsUtilities.HasValue(action)) {
                url += action + "/";

                if (tsUtilities.HasValue(id)) {
                    url += id;
                }
            }
            window.location.href = url;
        } else {
            this.Nav(action, id, true);
        }
    }

    /**
     * Gets a value from the querystring
     * @param {string} name - The key for the value to get.
     * @returns {string} - The value or an emtpy string.
     */
    QuerystringValue(name: string): string {
        let output: string = "";

        name = name.replace(/[\[\]]/g, '\\$&');
        var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'), results = regex.exec(window.location.href);
        if (results) {
            if (results[2]) {
                output = decodeURIComponent(results[2].replace(/\+/g, ' '));
            }
        }
        
        return output;
    }

    /**
     * Reloads the User object for the current user.
     */
    ReloadUser(): void {
        let success: Function = (data: server.user) => {
            if (data != null && data.actionResponse.result) {
                this.User().Load(data);
                this.User.notifySubscribers();
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUser/" + this.User().userId(), null, success);
    }

    /**
     * Renders a file storage item as either a thumbnail preview or an icon.
     * @param {fileStorage} file - The fileStorage item.
     * @returns {string} - A file icon.
     */
    RenderFileImageOrIcon(file: fileStorage): string {
        let output: string = "";

        if (file != null && tsUtilities.HasValue(file.extension())) {
            let ext: string = file.extension().replace(".", "").toLowerCase();
            switch (ext) {
                case "png":
                case "jpg":
                case "jpeg":
                case "gif":

                    if (file.value() != null && file.value().length > 0) {
                        output = "data:image/" + ext + "; base64," + file.value();
                    } else if (tsUtilities.HasValue(file.base64value())) {
                        output = file.base64value();
                    } else {
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
    }

    /**
     * Renders a list of files to the specified element.
     * @param {string} element - The id of the html element where the file list will be rendered.
     * @param {fileStorage[]} files - The collection of fileStorage items.
     * @param {Function} deleteCallbackHandler - An optional callback handler for deleting files.
     */
    RenderFiles(element: string, files: fileStorage[], deleteCallbackHandler?: Function) {
        let output: string = "";
        let allowDelete: boolean = this.User().admin();
        let deleteIcon: string = "<i class='fas fa-trash'></i>";
        let deleteText: string = this.Language("Delete");

        if (files != null && files.length > 0) {
            //output += "<div class='row'><div class='col-12'>\n";
            output += "<div class='file-container'>\n";
            for (let x: number = 0; x < files.length; x++) {
                let file: fileStorage = files[x];

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
                    "  </div>\n";;
            }

            output += "</div>\n";
        }

        $("#" + element).html(output);

        if (files != null && files.length > 0) {
            files.forEach(function (file) {
                $("#delete-file-" + file.fileId()).off("click");
                $("#delete-file-" + file.fileId()).on("click", function () {
                    let id: string = $(this).attr("file-id");
                    if (tsUtilities.HasValue(id)) {
                        deleteCallbackHandler(id);
                    }
                });

                $("#view-file-" + file.fileId()).off("click");
                $("#view-file-" + file.fileId()).on("click", function () {
                    let id: string = $(this).attr("file-id");
                    if (tsUtilities.HasValue(id)) {
                        window.mainModel.ViewFile(id);
                    }
                });
            });
        }
    }

    /**
     * Called from the javascript code on the _Layout page when a SignalR message is received from the server.
     * Some models subscribe to the event that fires when the this.SignalRUpdate() object gets updated here with the data received from the server.
     * @param update
     */
    SignalRUpdateHandler(update: any): void {
        //console.log("SignalR Object", update);

        this.SignalRUpdate().Load(JSON.parse(ko.toJSON(update)));
        this.SignalRUpdate.notifySubscribers();

        let updateType: string = this.SignalRUpdate().updateTypeString();
        let message: string = this.SignalRUpdate().message();
        let itemId: string = this.SignalRUpdate().itemId();

        switch (updateType.toLowerCase()) {
            case "setting":
                switch (message.toLowerCase()) {
                    case "deleteddepartment":
                        let dept: department = ko.utils.arrayFirst(this.Tenant().departments(), function (item) {
                            return item.departmentId() == update.itemId;
                        });
                        if (dept != null) {
                            this.Tenant().departments.remove(dept);
                        }
                        break;

                    case "deleteddepartmentgroup":
                        let dgItem: departmentGroup = ko.utils.arrayFirst(this.Tenant().departmentGroups(), function (item) {
                            return item.departmentGroupId() == update.itemId;
                        });
                        if (dgItem != null) {
                            this.Tenant().departmentGroups.remove(dgItem);
                        }

                        break;

                    case "language":
                        // Only load if the culture matches the current user's culture
                        let l: language = new language();
                        l.Load(JSON.parse(update.object));
                        if (l != undefined && l != null && tsUtilities.HasValue(l.culture())) {
                            if (navigator != null && tsUtilities.HasValue(navigator.language)) {
                                if (l.culture().toLowerCase() == navigator.language.toLowerCase()) {
                                    this.LoadLanguage(JSON.parse(update.object), null);
                                }
                            }
                        }
                        break;

                    case "saveddepartment":
                        let savedDept: department = new department();
                        savedDept.Load(JSON.parse(update.object));

                        let existingDept: department = null;
                        if (this.Tenant().departments() != null && this.Tenant().departments().length > 0) {
                            existingDept = ko.utils.arrayFirst(this.Tenant().departments(), function (item) {
                                return item.departmentId() == savedDept.departmentId();
                            });
                        }
                        if (existingDept != null) {
                            existingDept.Load(JSON.parse(update.object));
                        } else {
                            this.Tenant().departments.push(savedDept);
                            this.Tenant().departments.notifySubscribers();
                        }

                        break;

                    case "saveddepartmentgroup":
                        let dg: departmentGroup = new departmentGroup();
                        dg.Load(JSON.parse(update.object));

                        let existingdg: departmentGroup = ko.utils.arrayFirst(this.Tenant().departmentGroups(), function (item) {
                            return item.departmentGroupId() == dg.departmentGroupId();
                        });
                        if (existingdg != null) {
                            existingdg.departmentGroupName(dg.departmentGroupName());
                        } else {
                            this.Tenant().departmentGroups.push(dg);
                            this.Tenant().departmentGroups.notifySubscribers();
                        }

                        break;

                    case "savedudflabels":
                        let labels: server.udfLabel[] = update.object;
                        if (labels != null && labels.length > 0) {
                            let l: udfLabel[] = [];

                            labels.forEach(function (e) {
                                let item: udfLabel = new udfLabel();
                                item.Load(e);
                                l.push(item);
                            });

                            this.Tenant().udfLabels(l);
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

                    case "usetenantcodeinurl":
                        let useTenantCodeInUrl = itemId == "1";
                        if (window.useTenantCodeInUrl != useTenantCodeInUrl) {
                            window.location.href = window.baseURL;
                        }
                        break;
                }

                break;
        }
    }

    Spacer(width: number): string {
        let output: string = '<img src="' + window.baseURL + 'images/spacer.png" width="' + width.toString() + '" height="1" />';
        return output;
    }

    /**
     * Renders the sticky icon at the top of fixed menu areas.
     */
    StickyIcon(): string {
        let output: string = "";

        if (this.StickyMenus()) {
            output = this.Icon("StickyMenus");
        } else {
            output = this.Icon("ScrollingMenus");
        }

        return output;
    }

    /**
     * When a user selects a different tenant account from the menu this performs the actual page reload.
     * @param tenantCode {string} - The new tenant code.
     */
    SwitchTenant(tenantCode: string): void {
        if (tsUtilities.HasValue(tenantCode) && tenantCode.toLowerCase() != window.tenantCode.toLowerCase()) {
            if (window.useTenantCodeInUrl) {
                window.location.href = window.baseURL + tenantCode;
            } else {
                tsUtilities.CookieWrite(window.appName + "-tenant-code", tenantCode);
                window.location.href = window.baseURL;
            }
        }
    }

    /**
     * Called when a tenant is saved. If the tenant code changes the browser needs to be refreshed.
     * @param object {any} - The message coming from SignalR that contains the updated tenant object.
     */
    TenantSaved(object: any): void {
        let t: tenant = new tenant();
        t.Load(JSON.parse(object));

        let tenantId: string = t.tenantId();
        if (t.tenantId() == window.tenantId) {
            if (t.tenantCode() != window.tenantCode) {
                // If this is the current tenant and the tenant code has changed we need to redirect back to the root to reload.
                window.location.href = window.baseURL;
            } else {
                // Reload the current Tenant object
                this.Tenant().Load(JSON.parse(object));

                // Update items on the user object.
                let userTenant: userTenant = ko.utils.arrayFirst(this.User().userTenants(), function (item) {
                    return item.tenantId() == tenantId;
                });
                if (userTenant != null) {
                    userTenant.tenantName(t.name());
                }

                let userT: tenant = ko.utils.arrayFirst(this.User().tenants(), function (item) {
                    return item.tenantId() == tenantId;
                });
                if (userT != null) {
                    userT.name(t.name());
                }
            }
        }
    }

    ThemeWatcher(): void {
        let colorScheme: string = getComputedStyle(document.body, ':after').content;

        if (tsUtilities.HasValue(colorScheme)) {
            this.PreferredColorScheme(colorScheme.indexOf("dark") > -1 ? "dark" : "light");
        }

        // If we are in auto-mode then constantly watch for system changes.
        // Some OS's will automatically move from light mode to dark mode later in the day.
        // This will allow the page to automatically switch between modes when the
        // system changes.
        if (this.Theme() == "") {
            setTimeout(() => this.ThemeWatcher(), 1000);
        }
    }

    /**
     * Toggles the sticky state of the top fixed menus
     */
    ToggleStickyMenus(): void {
        if (this.StickyMenus()) {
            this.StickyMenus(false);
            window.localStorage.setItem(window.appName + "-sticky-menus", "0");
        } else {
            this.StickyMenus(true);
            window.localStorage.setItem(window.appName + "-sticky-menus", "1");
        }
    }

    /**
     * Gets the value for a given UDF
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    UDFFieldGetValue(module: string, item: number): string {
        let output: string = "";

        if (this.Loaded() == true && module != undefined && module != null && module != "") {
            let m: string = module.toLowerCase();
            let id: string = m + "-udf-" + item.toString();
            let type: string = this.UDFFieldType(module, item);

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
    }

    /**
     * If this UDF has options (eg: text separated by pipe characters) then those options are returned.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    UDFFieldOptions(module: string, item: number): string[] {
        let output: string[] = [];

        if (this.Loaded() == true) {
            let values: string = "";

            let udf: string = "UDF" + (item < 10 ? "0" : "") + item.toString();

            let match: udfLabel = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf;
            });
            if (match != null && match.label() != null && match.label() != "") {
                values = match.label();
            }
            if (values.indexOf("|") > -1) {
                let items: string[] = values.split("|");
                if (items != null && items.length > 2) {
                    values = items[2].trim();
                    if (values == undefined || values == null) {
                        values = "";
                    }
                }
                if (values != "") {
                    let items: string[] = values.split(",");
                    if (items != null && items.length > 0) {
                        items.forEach(function (e) {
                            output.push(e.trim());
                        });
                    }
                }
            }
        }

        return output;
    }

    /**
     * Renders the display-only version of a UDF
     * @param element {string} - The id of the HTML element where the fields should be rendered.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param dataObject {any} - The object that contains the UDF fields (eg: a User object).
     */
    UDFFieldsDisplay(element: string, module: string, dataObject: any): void {
        let output: string = "";

        if (this.Loaded() == true) {
            let m: string = module.toLowerCase();
            let totalItems: number = m == "assets" ? 20 : 10;

            for (let x: number = 1; x <= totalItems; x++) {
                if (this.UDFShowField(module, x)) {
                    let label: string = this.UDFLabel(module, x);
                    let options: string[] = [];

                    let udfField: string = "udf" + (x < 10 ? "0" : "") + x.toString();
                    let value: string = dataObject[udfField];

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
        } else {
            $("#" + element).html("");
        }
    }

    /**
     * Renders the input version of a UDF
     * @param element {string} - The id of the HTML element where the fields should be rendered.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param dataObject {any} - The object that contains the UDF fields (eg: a User object).
     */
    UDFFieldsRender(element: string, module: string, dataObject: any): void {
        let output: string = "";

        if (this.Loaded() == true) {
            let m: string = module.toLowerCase();
            let totalItems: number = m == "assets" ? 20 : 10;

            for (let x: number = 1; x <= totalItems; x++) {
                if (this.UDFShowField(module, x)) {
                    let label: string = this.UDFLabel(module, x);
                    let id: string = m + "-udf-" + x.toString();
                    let options: string[] = [];

                    if (tsUtilities.HasValue(label)) {
                        output +=
                            "<div class='row padtop'>\n" +
                            "  <div class='col-sm-12'>\n" +
                            "    <label for='" + id + "'>" + label + "</label>\n";

                        let type: string = this.UDFFieldType(module, x);
                        switch (type) {
                            case "input":
                                output +=
                                    "    <input type='text' class='form-control' id='" + id + "'>";
                                break;

                            case "select":
                                options = this.UDFFieldOptions(module, x);
                                output += "    <select class='form-control' id='" + id + "'>\n";

                                if (options != null && options.length > 0) {
                                    output += "      <option value=\"\">Select a Value</option>\n";
                                    options.forEach(function (item) {
                                        output += "      <option value=\"" + item + "\">" + item + "</option>\n";
                                    });
                                }

                                output += "    </select>\n";
                                break;

                            case "radio":
                                options = this.UDFFieldOptions(module, x);

                                if (options != null && options.length > 0) {
                                    let o: number = 0;
                                    options.forEach(function (item) {
                                        o++;
                                        output +=
                                            "    <div>\n" +
                                            "      <input type='radio' name='" + id + "' id='" + id + "-" + o.toString() + "' value=\"" + item + "\">\n" +
                                            "      <label for='" + id + "-" + o.toString() + "' class='unbold'>" + item + "</label>\n" +
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
            }
            $("#" + element).html(output);
            this.UDFFieldsSetValues(module, dataObject);
        } else {
            $("#" + element).html("");
        }
    }

    /**
     * Gets the values from the UDF items and stores them back in the object.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param observableObject {any} - The object that contains the UDF fields (eg: a User object).
     */
    UDFFieldsGetValues(module: string, observableObject: any): void {
        let m: string = module.toLowerCase();
        let totalItems: number = m == "assets" ? 20 : 10;

        for (let x: number = 1; x <= totalItems; x++) {
            let label: string = this.UDFLabel(module, x);

            if (tsUtilities.HasValue(label)) {
                let id: string = m + "-udf-" + x.toString();
                let type: string = this.UDFFieldType(module, x);
                let value: string = "";

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
    }

    /**
     * Sets the values for the rendered UDF items.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param dataObject {any} - The object that contains the UDF fields (eg: a User object).
     */
    UDFFieldsSetValues(module: string, dataObject: any): void {
        let m: string = module.toLowerCase();
        let totalItems: number = m == "assets" ? 20 : 10;

        for (let x: number = 1; x <= totalItems; x++) {
            let label: string = this.UDFLabel(module, x);

            if (tsUtilities.HasValue(label)) {
                let udfField: string = "udf" + (x < 10 ? "0" : "") + x.toString();
                let id: string = m + "-udf-" + x.toString();
                let type: string = this.UDFFieldType(module, x);
                let value: string = dataObject[udfField];


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
    }

    /**
     * Determines the type of input to render for the given UDF.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    UDFFieldType(module: string, item: number): string {
        let output: string = "input";

        if (this.Loaded() == true) {
            let label: string = "";
            let udf: string = "UDF" + (item < 10 ? "0" : "") + item.toString();

            let match: udfLabel = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf;
            });
            if (match != null && match.label() != null && match.label() != "") {
                label = match.label();
            }
            if (label.indexOf("|") > -1) {
                let items: string[] = label.split("|");
                if (items != null && items.length > 1) {
                    output = items[1].trim();
                    if (output == undefined || output == null || output == "") {
                        output = "input";
                    }
                }
            }
        }

        return output.toLowerCase();
    }

    /**
     * Gets the filter options for the given UDF.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    UDFFilterOptions(module: string, item: number): string[] {
        let output: string[] = [];

        if (this.Loaded() == true) {
            let udf: string = "UDF" + (item < 10 ? "0" : "") + item.toString();

            let match: udfLabel = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf;
            });
            if (match != null && match.filterOptions() != null && match.filterOptions().length > 0) {
                match.filterOptions().forEach(function (item) {
                    output.push(item);
                });
            }
        }

        return output;
    }

    /**
     * Gets the label for the given UDF.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    UDFLabel(module: string, item: number): string {
        let output: string = "";

        if (this.Loaded() == true) {
            let udf: string = "UDF" + (item < 10 ? "0" : "") + item.toString();

            let match: udfLabel = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf;
            });
            if (match != null && match.label() != null && match.label() != "") {
                output = match.label();
            }
            if (output.indexOf("|") > -1) {
                output = output.substr(0, output.indexOf("|"));
            }
        }

        return output;
    }

    /**
     * Determines if the given UDF should be shown in a column.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    UDFShowColumn(module: string, item: number): boolean {
        let output: boolean = false;

        if (this.Loaded() == true) {
            let udf: string = "UDF" + (item < 10 ? "0" : "") + item.toString();

            let match: udfLabel = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf;
            });

            if (match != null && match.label() != null && match.label() != "" && match.showColumn() == true) {
                output = true;
            }
        }

        return output;
    }

    /**
     * Determines if the given UDF should be shown in the interface when editing or viewing an item.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    UDFShowField(module: string, item: number): boolean {
        let output: boolean = false;

        if (this.Loaded() == true) {
            let label: string = this.UDFLabel(module, item);
            if (tsUtilities.HasValue(label)) {
                output = true;
            }
        }

        return output;
    }

    /**
     * Determines if the given UDF should be shown in the filter options.
     * @param module {string} - The name of the UDF module (eg: users)
     * @param item {number} - The index of the UDF item.
     */
    UDFShowInFilter(module: string, item: number): boolean {
        let output: boolean = false;

        if (this.Loaded() == true) {
            let udf: string = "UDF" + (item < 10 ? "0" : "") + item.toString();

            let match: udfLabel = ko.utils.arrayFirst(this.Tenant().udfLabels(), function (item) {
                return item.module().toLowerCase() == module.toLowerCase() && item.udf() == udf;
            });

            if (match != null && match.label() != null && match.label() != "" && match.showInFilter() == true) {
                output = true;
            }
        }

        return output;
    }

    UpdatePagedRecordsetColumnIcons(data: filter): filter {
        let output: filter = data;

        if (output.columns() != null && output.columns().length > 0) {
            for (let x: number = 0; x < output.columns().length; x++) {
                let column: filterColumn = output.columns()[x];
                if (tsUtilities.HasValue(column.booleanIcon())) {
                    let booleanIcon: string = this.Icon(column.booleanIcon());
                    if (tsUtilities.HasValue(booleanIcon)) {
                        column.booleanIcon(booleanIcon);
                    }
                }

                if (tsUtilities.HasValue(column.label()) && column.label().toLowerCase().indexOf("icon:") > -1) {
                    let label: string = column.label().toLowerCase().replace("icon:", "");
                    let icon: string = this.Icon(label);
                    if (tsUtilities.HasValue(icon)) {
                        column.label(icon);
                        if (!tsUtilities.HasValue(column.booleanIcon())) {
                            column.booleanIcon(icon);
                        }
                    } else {
                        column.label(column.label().substring(5));
                    }
                }

            }
        }

        return output;
    }

    /**
     * Called on load and when the window resizes to keep track of whether the mobile menu is visible or not, as well as the window width and height.
     */
    UpdateWindowSettings(): void {
        this.MobileMenu($("#menu-bar-toggler").is(":visible"));
        this.WindowHeight($(window).height());
        this.WindowWidth($(window).width());
    }

    UserDisplayName(userId: string, includeEmail?: boolean): string {
        let output: string = "";

        if (includeEmail == undefined || includeEmail == null) {
            includeEmail = false;
        }

        if (this.Users() != null && this.Users().length > 0) {
            let u: user = ko.utils.arrayFirst(this.Users(), function (item) {
                return item.userId() == userId;
            });
            if (u != null) {
                if (tsUtilities.HasValue(u.firstName())) {
                    output += u.firstName();
                    if (tsUtilities.HasValue(u.lastName())) {
                        output += " " + u.lastName();
                    }
                }
                if (includeEmail && tsUtilities.HasValue(u.email())) {
                    if (output != "") { output += " "; }
                    output += "[" + u.email() + "]";
                }
            }
        }

        return output;
    }

    /**
     * Views a file. If the file is an image it will be viewed in place with the colorbox plugin.
     * @param {string} fileId - The fileId of the file to view.
     */
    ViewFile(fileId: string): void {
        let success: Function = (data: server.fileStorage) => {
            if (data != null) {
                if (data.actionResponse.result) {
                    let ext: string = "";
                    if (tsUtilities.HasValue(data.extension)) {
                        ext = data.extension.replace(".", "").toLowerCase();
                    }
                    switch (ext) {
                        case "png":
                        case "jpg":
                        case "jpeg":
                        case "gif":
                            this.ViewImage(window.baseURL + "File/View/" + fileId);
                            break;

                        case "txt":
                            $.colorbox({
                                href: window.baseURL + "File/View/" + fileId,
                                title: data.fileName,
                                maxWidth: "95%",
                                maxHeight: "95%",
                                width: "95%",
                                height: "75%",
                                scalePhotos: true,
                                photo: false,
                                transition: "none",
                                className: "colorbox-text"
                            });
                            break;
                        default:
                            window.open(window.baseURL + "File/View/" + fileId);
                            break;
                    }

                } else {
                    this.Message_Errors(data.actionResponse.messages);
                }
            } else {
                console.log("Error loading file", fileId);
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/GetFileStorage/" + fileId, null, success);
    }

    /**
     * Views an image using the colorbox plugin.
     * @param {string} url - The url to the image.
     */
    ViewImage(url: string): void {
        $.colorbox({
            href: url,
            photo: true,
            maxWidth: "95%",
            transition: "none"
        });
    }
}