var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
function setRequestHeaders(xhr) {
    xhr.setRequestHeader('Token', window.token);
}
var actionResponseObject = /** @class */ (function () {
    function actionResponseObject() {
        this.actionResponse = ko.observable(new booleanResponse);
    }
    actionResponseObject.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
        }
        else {
            this.actionResponse(new booleanResponse);
        }
    };
    return actionResponseObject;
}());
var ajaxLookup = /** @class */ (function (_super) {
    __extends(ajaxLookup, _super);
    function ajaxLookup() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.TenantId = ko.observable(null);
        _this.Search = ko.observable(null);
        _this.Parameters = ko.observableArray([]);
        _this.Results = ko.observableArray([]);
        return _this;
    }
    ajaxLookup.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.TenantId(data.tenantId);
            this.Search(data.search);
            this.Results([]);
            var parameters_1 = [];
            if (data.parameters != null) {
                data.parameters.forEach(function (element) {
                    parameters_1.push(element);
                });
            }
            this.Parameters(parameters_1);
            if (data.results != null) {
                var results_1 = [];
                data.results.forEach(function (element) {
                    var item = new ajaxResults();
                    item.Load(element);
                    results_1.push(item);
                });
                this.Results(results_1);
            }
        }
    };
    return ajaxLookup;
}(actionResponseObject));
var ajaxResults = /** @class */ (function () {
    function ajaxResults() {
        this.label = ko.observable(null);
        this.value = ko.observable(null);
        this.email = ko.observable(null);
        this.username = ko.observable(null);
        this.extra1 = ko.observable(null);
        this.extra2 = ko.observable(null);
        this.extra3 = ko.observable(null);
    }
    ajaxResults.prototype.Load = function (data) {
        if (data != null) {
            this.label(data.label);
            this.value(data.value);
            this.email(data.email);
            this.username(data.username);
            this.extra1(data.extra1);
            this.extra2(data.extra2);
            this.extra3(data.extra3);
        }
    };
    return ajaxResults;
}());
var authenticate = /** @class */ (function () {
    function authenticate() {
        this.username = ko.observable(null);
        this.password = ko.observable(null);
    }
    authenticate.prototype.Load = function (data) {
        if (data != null) {
            this.username(data.username);
            this.password(data.password);
        }
        else {
            this.username(null);
            this.password(null);
        }
    };
    return authenticate;
}());
var booleanResponse = /** @class */ (function () {
    function booleanResponse() {
        this.messages = ko.observableArray([]);
        this.result = ko.observable(false);
    }
    booleanResponse.prototype.Load = function (data) {
        if (data != null) {
            this.messages(data.messages);
            this.result(data.result);
        }
        else {
            this.messages([]);
            this.result(false);
        }
    };
    return booleanResponse;
}());
var department = /** @class */ (function (_super) {
    __extends(department, _super);
    function department() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.departmentId = ko.observable(null);
        _this.tenantId = ko.observable(null);
        _this.departmentName = ko.observable(null);
        _this.activeDirectoryNames = ko.observable(null);
        _this.enabled = ko.observable(false);
        _this.departmentGroupId = ko.observable(null);
        return _this;
    }
    department.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.departmentId(data.departmentId);
            this.tenantId(data.tenantId);
            this.departmentName(data.departmentName);
            this.activeDirectoryNames(data.activeDirectoryNames);
            this.enabled(data.enabled);
            this.departmentGroupId(data.departmentGroupId);
        }
    };
    return department;
}(actionResponseObject));
var departmentGroup = /** @class */ (function (_super) {
    __extends(departmentGroup, _super);
    function departmentGroup() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.departmentGroupId = ko.observable(null);
        _this.tenantId = ko.observable(null);
        _this.departmentGroupName = ko.observable(null);
        return _this;
    }
    departmentGroup.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.departmentGroupId(data.departmentGroupId);
            this.tenantId(data.tenantId);
            this.departmentGroupName(data.departmentGroupName);
        }
    };
    return departmentGroup;
}(actionResponseObject));
var fileStorage = /** @class */ (function (_super) {
    __extends(fileStorage, _super);
    function fileStorage() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.fileId = ko.observable(null);
        _this.tenantId = ko.observable(null);
        _this.itemId = ko.observable(null);
        _this.fileName = ko.observable(null);
        _this.extension = ko.observable(null);
        _this.sourceFileId = ko.observable(null);
        _this.bytes = ko.observable(0);
        _this.value = ko.observableArray([]);
        _this.uploadDate = ko.observable(null);
        _this.userId = ko.observable(null);
        _this.base64value = ko.observable(null);
        return _this;
    }
    fileStorage.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.fileId(data.fileId);
            this.tenantId(data.tenantId);
            this.itemId(data.itemId);
            this.fileName(data.fileName);
            this.extension(data.extension);
            this.sourceFileId(data.sourceFileId);
            this.bytes(data.bytes);
            this.value(data.value);
            this.uploadDate(data.uploadDate);
            this.userId(data.userId);
            this.base64value(data.base64value);
        }
    };
    return fileStorage;
}(actionResponseObject));
var filter = /** @class */ (function (_super) {
    __extends(filter, _super);
    function filter() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.tenantId = ko.observable(null);
        _this.loading = ko.observable(false);
        _this.showFilters = ko.observable(false);
        _this.executionTime = ko.observable(0.0);
        _this.start = ko.observable(null);
        _this.end = ko.observable(null);
        _this.keyword = ko.observable(null);
        _this.sort = ko.observable(null);
        _this.sortOrder = ko.observable(null);
        _this.recordsPerPage = ko.observable(0);
        _this.pageCount = ko.observable(0);
        _this.recordCount = ko.observable(0);
        _this.page = ko.observable(0);
        _this.tenants = ko.observableArray([]);
        _this.columns = ko.observableArray([]);
        _this.records = ko.observableArray([]);
        return _this;
    }
    filter.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.tenantId(data.tenantId);
            this.loading(data.loading);
            this.showFilters(data.showFilters);
            this.start(data.start);
            this.end(data.end);
            this.keyword(data.keyword);
            this.sort(data.sort);
            this.sortOrder(data.sortOrder);
            this.recordsPerPage(data.recordsPerPage);
            this.pageCount(data.pageCount);
            this.recordCount(data.recordCount);
            this.page(data.page);
            this.tenants(data.tenants);
            this.records(data.records);
            var c_1 = [];
            if (data.columns != null) {
                data.columns.forEach(function (e) {
                    var item = new filterColumn();
                    item.Load(e);
                    c_1.push(item);
                });
            }
            this.columns(c_1);
        }
        else {
            this.actionResponse(new booleanResponse);
            this.start(null);
            this.end(null);
            this.keyword(null);
            this.sort(null);
            this.sortOrder(null);
            this.recordsPerPage(0);
            this.pageCount(0);
            this.recordCount(0);
            this.page(0);
            this.tenants([]);
            this.records([]);
            this.columns([]);
        }
    };
    return filter;
}(actionResponseObject));
var filterColumn = /** @class */ (function () {
    function filterColumn() {
        this.align = ko.observable(null);
        this.label = ko.observable(null);
        this.tipText = ko.observable(null);
        this.dataElementName = ko.observable(null);
        this.dataType = ko.observable(null);
        this.sortable = ko.observable(false);
    }
    filterColumn.prototype.Load = function (data) {
        if (data != null) {
            this.align(data.align);
            this.label(data.label);
            this.tipText(data.tipText);
            this.dataElementName(data.dataElementName);
            this.dataType(data.dataType);
            this.sortable(data.sortable);
        }
        else {
            this.align(null);
            this.label(null);
            this.tipText(null);
            this.dataElementName(null);
            this.dataType(null);
            this.sortable(false);
        }
    };
    return filterColumn;
}());
var filterUsers = /** @class */ (function (_super) {
    __extends(filterUsers, _super);
    function filterUsers() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.filterDepartments = ko.observableArray([]);
        _this.enabled = ko.observable(null);
        _this.admin = ko.observable(null);
        return _this;
    }
    filterUsers.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.tenantId(data.tenantId);
            this.loading(data.loading);
            this.showFilters(data.showFilters);
            this.start(data.start);
            this.end(data.end);
            this.keyword(data.keyword);
            this.sort(data.sort);
            this.sortOrder(data.sortOrder);
            this.recordsPerPage(data.recordsPerPage);
            this.pageCount(data.pageCount);
            this.recordCount(data.recordCount);
            this.page(data.page);
            this.tenants(data.tenants);
            if (data.records != null) {
                this.records(data.records);
            }
            else {
                this.records([]);
            }
            var c_2 = [];
            if (data.columns != null) {
                data.columns.forEach(function (e) {
                    var item = new filterColumn();
                    item.Load(e);
                    c_2.push(item);
                });
            }
            this.columns(c_2);
            this.filterDepartments(data.filterDepartments);
            this.enabled(data.enabled);
            this.admin(data.admin);
        }
        else {
            this.actionResponse(new booleanResponse);
            this.tenantId(null);
            this.loading(null);
            this.showFilters(null);
            this.start(null);
            this.end(null);
            this.keyword(null);
            this.sort(null);
            this.sortOrder(null);
            this.recordsPerPage(0);
            this.pageCount(0);
            this.recordCount(0);
            this.page(0);
            this.tenants([]);
            this.records([]);
            this.columns([]);
            this.filterDepartments([]);
            this.enabled(null);
            this.admin(null);
        }
    };
    return filterUsers;
}(filter));
var listItem = /** @class */ (function (_super) {
    __extends(listItem, _super);
    function listItem() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.id = ko.observable(null);
        _this.tenantId = ko.observable(null);
        _this.type = ko.observable(null);
        _this.name = ko.observable(null);
        _this.sortOrder = ko.observable(0);
        _this.enabled = ko.observable(false);
        return _this;
    }
    listItem.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.id(data.id);
            this.tenantId(data.tenantId);
            this.type(data.type);
            this.name(data.name);
            this.sortOrder(data.sortOrder);
            this.enabled(data.enabled);
        }
    };
    return listItem;
}(actionResponseObject));
var optionPair = /** @class */ (function () {
    function optionPair() {
        this.id = ko.observable(null);
        this.value = ko.observable(null);
    }
    optionPair.prototype.Load = function (data) {
        if (data != null) {
            this.id(data.id);
            this.value(data.value);
        }
        else {
            this.id(null);
            this.value(null);
        }
    };
    return optionPair;
}());
var signalRUpdate = /** @class */ (function () {
    function signalRUpdate() {
        this.tenantId = ko.observable(null);
        this.requestId = ko.observable(null);
        this.itemId = ko.observable(null);
        this.userId = ko.observable(null);
        this.updateTypeString = ko.observable(null);
        this.message = ko.observable(null);
        this.object = ko.observable(null);
    }
    signalRUpdate.prototype.Load = function (data) {
        if (data != null) {
            this.tenantId(data.tenantId);
            this.requestId(data.requestId);
            this.itemId(data.itemId);
            this.userId(data.userId);
            this.updateTypeString(data.updateTypeString);
            this.message(data.message);
            this.object(data.object);
        }
        else {
            this.tenantId(null);
            this.requestId(null);
            this.itemId(null);
            this.userId(null);
            this.updateTypeString(null);
            this.message(null);
            this.object(null);
        }
    };
    return signalRUpdate;
}());
var setting = /** @class */ (function (_super) {
    __extends(setting, _super);
    function setting() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.settingId = ko.observable(0);
        _this.settingName = ko.observable(null);
        _this.settingType = ko.observable(null);
        _this.settingNotes = ko.observable(null);
        _this.settingText = ko.observable(null);
        _this.tenantId = ko.observable(null);
        _this.userId = ko.observable(null);
        return _this;
    }
    setting.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.settingId(data.settingId);
            this.settingName(data.settingName);
            this.settingType(data.settingType);
            this.settingNotes(data.settingNotes);
            this.settingText(data.settingText);
            this.tenantId(data.tenantId);
            this.userId(data.userId);
        }
        else {
            this.actionResponse(new booleanResponse);
            this.settingId(null);
            this.settingName(null);
            this.settingType(null);
            this.settingNotes(null);
            this.settingText(null);
            this.tenantId(null);
            this.userId(null);
        }
    };
    return setting;
}(actionResponseObject));
var simplePost = /** @class */ (function () {
    function simplePost() {
        this.singleItem = ko.observable(null);
        this.items = ko.observableArray([]);
    }
    simplePost.prototype.Load = function (data) {
        if (data != null) {
            this.singleItem(data.singleItem);
            this.items(data.items);
        }
    };
    return simplePost;
}());
var tenant = /** @class */ (function (_super) {
    __extends(tenant, _super);
    function tenant() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.tenantId = ko.observable(null);
        _this.name = ko.observable(null);
        _this.tenantCode = ko.observable(null);
        _this.enabled = ko.observable(false);
        _this.departments = ko.observableArray([]);
        _this.departmentGroups = ko.observableArray([]);
        _this.tenantSettings = ko.observable(new tenantSettings);
        _this.listItems = ko.observableArray([]);
        return _this;
    }
    tenant.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.tenantId(data.tenantId);
            this.name(data.name);
            this.tenantCode(data.tenantCode);
            this.enabled(data.enabled);
            this.tenantSettings().Load(data.tenantSettings);
            var d_1 = [];
            if (data.departments != null) {
                data.departments.forEach(function (e) {
                    var item = new department();
                    item.Load(e);
                    d_1.push(item);
                });
            }
            this.departments(d_1);
            var dG_1 = [];
            if (data.departmentGroups != null) {
                data.departmentGroups.forEach(function (e) {
                    var item = new departmentGroup();
                    item.Load(e);
                    dG_1.push(item);
                });
            }
            this.departmentGroups(dG_1);
            var li_1 = [];
            if (data.listItems != null) {
                data.listItems.forEach(function (e) {
                    var item = new listItem();
                    item.Load(e);
                    li_1.push(item);
                });
            }
            this.listItems(li_1);
        }
        else {
            this.actionResponse(new booleanResponse);
            this.tenantId(null);
            this.name(null);
            this.tenantCode(null);
            this.enabled(false);
            this.tenantSettings(new tenantSettings);
            this.departments([]);
            this.departmentGroups([]);
            this.listItems([]);
        }
    };
    return tenant;
}(actionResponseObject));
var tenantSettings = /** @class */ (function () {
    function tenantSettings() {
        this.allowUsersToManageAvatars = ko.observable(false);
        this.allowUsersToManageBasicProfileInfo = ko.observable(false);
        this.allowUsersToManageBasicProfileInfoElements = ko.observableArray([]);
        this.jasonWebTokenKey = ko.observable(null);
        this.loginOptions = ko.observableArray([]);
        this.workSchedule = ko.observable(new workSchedule);
        this.requirePreExistingAccountToLogIn = ko.observable(false);
    }
    tenantSettings.prototype.Load = function (data) {
        if (data != null) {
            this.allowUsersToManageAvatars(data.allowUsersToManageAvatars);
            this.allowUsersToManageBasicProfileInfo(data.allowUsersToManageBasicProfileInfo);
            this.allowUsersToManageBasicProfileInfoElements(data.allowUsersToManageBasicProfileInfoElements);
            this.jasonWebTokenKey(data.jasonWebTokenKey);
            this.loginOptions(data.loginOptions);
            this.workSchedule().Load(data.workSchedule);
            this.requirePreExistingAccountToLogIn(data.requirePreExistingAccountToLogIn);
        }
        else {
            this.allowUsersToManageAvatars(null);
            this.allowUsersToManageBasicProfileInfo(null);
            this.allowUsersToManageBasicProfileInfoElements([]);
            this.jasonWebTokenKey(null);
            this.loginOptions(null);
            this.workSchedule(new workSchedule);
            this.requirePreExistingAccountToLogIn(true);
        }
    };
    return tenantSettings;
}());
var user = /** @class */ (function (_super) {
    __extends(user, _super);
    function user() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.userId = ko.observable(null);
        _this.tenantId = ko.observable(null);
        _this.firstName = ko.observable(null);
        _this.lastName = ko.observable(null);
        _this.displayName = ko.observable(null);
        _this.email = ko.observable(null);
        _this.phone = ko.observable(null);
        _this.username = ko.observable(null);
        _this.employeeId = ko.observable(null);
        _this.departmentId = ko.observable(null);
        _this.departmentName = ko.observable(null);
        _this.title = ko.observable(null);
        _this.location = ko.observable(null);
        _this.enabled = ko.observable(false);
        _this.lastLogin = ko.observable(null);
        _this.admin = ko.observable(false);
        _this.appAdmin = ko.observable(false);
        _this.photo = ko.observable(null);
        _this.password = ko.observable(null);
        _this.hasLocalPassword = ko.observable(false);
        _this.authToken = ko.observable(null);
        _this.tenants = ko.observableArray([]);
        _this.userTenants = ko.observableArray([]);
        return _this;
    }
    user.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.userId(data.userId);
            this.tenantId(data.tenantId);
            this.firstName(data.firstName);
            this.lastName(data.lastName);
            this.displayName(data.displayName);
            this.email(data.email);
            this.phone(data.phone);
            this.username(data.username);
            this.employeeId(data.employeeId);
            this.departmentId(data.departmentId);
            this.departmentName(data.departmentName);
            this.title(data.title);
            this.location(data.location);
            this.enabled(data.enabled);
            this.lastLogin(data.lastLogin);
            this.admin(data.admin);
            this.appAdmin(data.appAdmin);
            this.photo(data.photo);
            this.password(data.password);
            this.hasLocalPassword(data.hasLocalPassword);
            this.authToken(data.authToken);
            var t_1 = [];
            if (data.tenants != null) {
                data.tenants.forEach(function (e) {
                    var item = new tenant();
                    item.Load(e);
                    t_1.push(item);
                });
            }
            this.tenants(t_1);
            var ut_1 = [];
            if (data.userTenants != null) {
                data.userTenants.forEach(function (e) {
                    var item = new userTenant();
                    item.Load(e);
                    ut_1.push(item);
                });
            }
            this.userTenants(ut_1);
        }
        else {
            this.actionResponse(new booleanResponse);
            this.userId(null);
            this.tenantId(null);
            this.firstName(null);
            this.lastName(null);
            this.displayName(null);
            this.email(null);
            this.phone(null);
            this.username(null);
            this.employeeId(null);
            this.departmentId(null);
            this.departmentName(null);
            this.title(null);
            this.location(null);
            this.enabled(false);
            this.lastLogin(null);
            this.admin(false);
            this.appAdmin(false);
            this.photo(null);
            this.password(null);
            this.hasLocalPassword(null);
            this.authToken(null);
            this.tenants([]);
            this.userTenants([]);
        }
    };
    return user;
}(actionResponseObject));
var userPasswordReset = /** @class */ (function (_super) {
    __extends(userPasswordReset, _super);
    function userPasswordReset() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.userId = ko.observable(null);
        _this.tenantId = ko.observable(null);
        _this.currentPassword = ko.observable(null);
        _this.newPassword = ko.observable(null);
        return _this;
    }
    userPasswordReset.prototype.Load = function (data) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.userId(data.userId);
            this.tenantId(data.tenantId);
            this.currentPassword(data.currentPassword);
            this.newPassword(data.newPassword);
        }
        else {
            this.actionResponse(new booleanResponse);
            this.userId(null);
            this.tenantId(null);
            this.currentPassword(null);
            this.newPassword(null);
        }
    };
    return userPasswordReset;
}(actionResponseObject));
var userTenant = /** @class */ (function () {
    function userTenant() {
        this.userId = ko.observable(null);
        this.tenantId = ko.observable(null);
        this.tenantCode = ko.observable(null);
        this.tenantName = ko.observable(null);
    }
    userTenant.prototype.Load = function (data) {
        if (data != null) {
            this.userId(data.userId);
            this.tenantId(data.tenantId);
            this.tenantCode(data.tenantCode);
            this.tenantName(data.tenantName);
        }
        else {
            this.userId(null);
            this.tenantId(null);
            this.tenantCode(null);
            this.tenantName(null);
        }
    };
    return userTenant;
}());
var workSchedule = /** @class */ (function () {
    function workSchedule() {
        this.sunday = ko.observable(false);
        this.sundayAllDay = ko.observable(false);
        this.sundayStart = ko.observable(null);
        this.sundayEnd = ko.observable(null);
        this.monday = ko.observable(false);
        this.mondayAllDay = ko.observable(false);
        this.mondayStart = ko.observable(null);
        this.mondayEnd = ko.observable(null);
        this.tuesday = ko.observable(false);
        this.tuesdayAllDay = ko.observable(false);
        this.tuesdayStart = ko.observable(null);
        this.tuesdayEnd = ko.observable(null);
        this.wednesday = ko.observable(false);
        this.wednesdayAllDay = ko.observable(false);
        this.wednesdayStart = ko.observable(null);
        this.wednesdayEnd = ko.observable(null);
        this.thursday = ko.observable(false);
        this.thursdayAllDay = ko.observable(false);
        this.thursdayStart = ko.observable(null);
        this.thursdayEnd = ko.observable(null);
        this.friday = ko.observable(false);
        this.fridayAllDay = ko.observable(false);
        this.fridayStart = ko.observable(null);
        this.fridayEnd = ko.observable(null);
        this.saturday = ko.observable(false);
        this.saturdayAllDay = ko.observable(false);
        this.saturdayStart = ko.observable(null);
        this.saturdayEnd = ko.observable(null);
    }
    workSchedule.prototype.Load = function (data) {
        if (data != null) {
            this.sunday(data.sunday);
            this.sundayAllDay(data.sundayAllDay);
            this.sundayStart(data.sundayStart);
            this.sundayEnd(data.sundayEnd);
            this.monday(data.monday);
            this.mondayAllDay(data.mondayAllDay);
            this.mondayStart(data.mondayStart);
            this.mondayEnd(data.mondayEnd);
            this.tuesday(data.tuesday);
            this.tuesdayAllDay(data.tuesdayAllDay);
            this.tuesdayStart(data.tuesdayStart);
            this.tuesdayEnd(data.tuesdayEnd);
            this.wednesday(data.wednesday);
            this.wednesdayAllDay(data.wednesdayAllDay);
            this.wednesdayStart(data.wednesdayStart);
            this.wednesdayEnd(data.wednesdayEnd);
            this.thursday(data.thursday);
            this.thursdayAllDay(data.thursdayAllDay);
            this.thursdayStart(data.thursdayStart);
            this.thursdayEnd(data.thursdayEnd);
            this.friday(data.friday);
            this.fridayAllDay(data.fridayAllDay);
            this.fridayStart(data.fridayStart);
            this.fridayEnd(data.fridayEnd);
            this.saturday(data.saturday);
            this.saturdayAllDay(data.saturdayAllDay);
            this.saturdayStart(data.saturdayStart);
            this.saturdayEnd(data.saturdayEnd);
        }
    };
    return workSchedule;
}());
//# sourceMappingURL=Models.js.map