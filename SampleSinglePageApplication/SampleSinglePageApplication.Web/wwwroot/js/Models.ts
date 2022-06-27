interface Window {
    action: string;
    appName: string;
    baseURL: string;
    id: string;
    loggedIn: boolean;
    missingTenantCode: boolean;
    objDefaultLanguage: server.optionPair[];
    objExtra: string[];
    objLanguage: server.optionPair[];
    objTenant: server.tenant;
    objUser: server.user;
    objVersionInfo: server.versionInfo;
    tenantCode: string;
    tenantId: string;
    token: string;
    useTenantCodeInUrl: boolean;
}

declare function configureDatePickers(): void;
declare function insertAtCursor(field: string, value: string): void;
declare function RefreshChosenElements(): void;

interface JQuery {
    chosen(options?: any): JQuery;
    highcharts(options?: any): JQuery;
    mask(options?: any): JQuery;
    unmask(options?: any): JQuery;
}

interface JQueryStatic {
    colorbox: any;
}

function setRequestHeaders(xhr) {
    xhr.setRequestHeader('Token', window.token);
}

class actionResponseObject {
    actionResponse: KnockoutObservable<booleanResponse> = ko.observable(new booleanResponse);

    Load(data: server.actionResponseObject) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
        } else {
            this.actionResponse(new booleanResponse);
        }
    }
}

class ajaxLookup extends actionResponseObject {
    TenantId: KnockoutObservable<string> = ko.observable(null);
    Search: KnockoutObservable<string> = ko.observable(null);
    Parameters: KnockoutObservableArray<string> = ko.observableArray([]);
    Results: KnockoutObservableArray<ajaxResults> = ko.observableArray([]);

    Load(data: server.ajaxLookup) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.TenantId(data.tenantId);

            this.Search(data.search);
            this.Results([]);
            let parameters: string[] = [];
            if (data.parameters != null) {
                data.parameters.forEach(function (element) {
                    parameters.push(element);
                });
            }
            this.Parameters(parameters);
            if (data.results != null) {
                let results: ajaxResults[] = [];
                data.results.forEach(function (element) {
                    let item: ajaxResults = new ajaxResults();
                    item.Load(element);
                    results.push(item);
                });
                this.Results(results);
            }
        }
    }
}

class ajaxResults {
    label: KnockoutObservable<string> = ko.observable(null);
    value: KnockoutObservable<string> = ko.observable(null);
    email: KnockoutObservable<string> = ko.observable(null);
    username: KnockoutObservable<string> = ko.observable(null);
    extra1: KnockoutObservable<string> = ko.observable(null);
    extra2: KnockoutObservable<string> = ko.observable(null);
    extra3: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.ajaxResults) {
        if (data != null) {
            this.label(data.label);
            this.value(data.value);
            this.email(data.email);
            this.username(data.username);
            this.extra1(data.extra1);
            this.extra2(data.extra2);
            this.extra3(data.extra3);
        }
    }
}

class authenticate {
    username: KnockoutObservable<string> = ko.observable(null);
    password: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.authenticate) {
        if (data != null) {
            this.username(data.username);
            this.password(data.password);
        } else {
            this.username(null);
            this.password(null);
        }
    }
}

class booleanResponse {
    messages: KnockoutObservableArray<string> = ko.observableArray([]);
    result: KnockoutObservable<boolean> = ko.observable(false);

    Load(data: server.booleanResponse) {
        if (data != null) {
            this.messages(data.messages);
            this.result(data.result);
        } else {
            this.messages([]);
            this.result(false);
        }
    }
}

class department extends actionResponseObject {
    departmentId: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    departmentName: KnockoutObservable<string> = ko.observable(null);
    activeDirectoryNames: KnockoutObservable<string> = ko.observable(null);
    enabled: KnockoutObservable<boolean> = ko.observable(false);
    departmentGroupId: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.department) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.departmentId(data.departmentId);
            this.tenantId(data.tenantId);
            this.departmentName(data.departmentName);
            this.activeDirectoryNames(data.activeDirectoryNames);
            this.enabled(data.enabled);
            this.departmentGroupId(data.departmentGroupId);
        }
    }
}

class departmentGroup extends actionResponseObject {
    departmentGroupId: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    departmentGroupName: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.departmentGroup) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.departmentGroupId(data.departmentGroupId);
            this.tenantId(data.tenantId);
            this.departmentGroupName(data.departmentGroupName);
        }
    }
}

class fileStorage extends actionResponseObject {
    fileId: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    itemId: KnockoutObservable<string> = ko.observable(null);
    fileName: KnockoutObservable<string> = ko.observable(null);
    extension: KnockoutObservable<string> = ko.observable(null);
    sourceFileId: KnockoutObservable<string> = ko.observable(null);
    bytes: KnockoutObservable<number> = ko.observable(0);
    value: KnockoutObservableArray<any> = ko.observableArray([]);
    uploadDate: KnockoutObservable<string> = ko.observable(null);
    userId: KnockoutObservable<string> = ko.observable(null);
    base64value: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.fileStorage) {
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
    }
}

class filter extends actionResponseObject {
    tenantId: KnockoutObservable<string> = ko.observable(null);
    loading: KnockoutObservable<boolean> = ko.observable(false);
    showFilters: KnockoutObservable<boolean> = ko.observable(false);
    executionTime: KnockoutObservable<number> = ko.observable(0.0);
    start: KnockoutObservable<Date> = ko.observable(null);
    end: KnockoutObservable<Date> = ko.observable(null);
    keyword: KnockoutObservable<string> = ko.observable(null);
    sort: KnockoutObservable<string> = ko.observable(null);
    sortOrder: KnockoutObservable<string> = ko.observable(null);
    recordsPerPage: KnockoutObservable<number> = ko.observable(0);
    pageCount: KnockoutObservable<number> = ko.observable(0);
    recordCount: KnockoutObservable<number> = ko.observable(0);
    page: KnockoutObservable<number> = ko.observable(0);
    tenants: KnockoutObservableArray<string> = ko.observableArray([]);
    columns: KnockoutObservableArray<filterColumn> = ko.observableArray([]);
    records: KnockoutObservableArray<any> = ko.observableArray([]);

    Load(data: server.filter) {
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

            let c: filterColumn[] = [];
            if (data.columns != null) {
                data.columns.forEach(function (e) {
                    let item: filterColumn = new filterColumn();
                    item.Load(e);
                    c.push(item);
                });
            }
            this.columns(c);
        } else {
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
    }
}

class filterColumn {
    align: KnockoutObservable<string> = ko.observable(null);
    label: KnockoutObservable<string> = ko.observable(null);
    tipText: KnockoutObservable<string> = ko.observable(null);
    dataElementName: KnockoutObservable<string> = ko.observable(null);
    dataType: KnockoutObservable<string> = ko.observable(null);
    sortable: KnockoutObservable<boolean> = ko.observable(false);

    Load(data: server.filterColumn) {
        if (data != null) {
            this.align(data.align);
            this.label(data.label);
            this.tipText(data.tipText);
            this.dataElementName(data.dataElementName);
            this.dataType(data.dataType);
            this.sortable(data.sortable);
        } else {
            this.align(null);
            this.label(null);
            this.tipText(null);
            this.dataElementName(null);
            this.dataType(null);
            this.sortable(false);
        }
    }
}

class filterUsers extends filter {
    filterDepartments: KnockoutObservableArray<string> = ko.observableArray([]);
    enabled: KnockoutObservable<string> = ko.observable(null);
    admin: KnockoutObservable<string> = ko.observable(null);
    udf01: KnockoutObservable<string> = ko.observable(null);
    udf02: KnockoutObservable<string> = ko.observable(null);
    udf03: KnockoutObservable<string> = ko.observable(null);
    udf04: KnockoutObservable<string> = ko.observable(null);
    udf05: KnockoutObservable<string> = ko.observable(null);
    udf06: KnockoutObservable<string> = ko.observable(null);
    udf07: KnockoutObservable<string> = ko.observable(null);
    udf08: KnockoutObservable<string> = ko.observable(null);
    udf09: KnockoutObservable<string> = ko.observable(null);
    udf10: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.filterUsers) {
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
            } else {
                this.records([]);
            }

            let c: filterColumn[] = [];
            if (data.columns != null) {
                data.columns.forEach(function (e) {
                    let item: filterColumn = new filterColumn();
                    item.Load(e);
                    c.push(item);
                });
            }
            this.columns(c);

            this.filterDepartments(data.filterDepartments);
            this.enabled(data.enabled);
            this.admin(data.admin);
            this.udf01(data.udf01);
            this.udf02(data.udf02);
            this.udf03(data.udf03);
            this.udf04(data.udf04);
            this.udf05(data.udf05);
            this.udf06(data.udf06);
            this.udf07(data.udf07);
            this.udf08(data.udf08);
            this.udf09(data.udf09);
            this.udf10(data.udf10);
        } else {
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
            this.udf01(null);
            this.udf02(null);
            this.udf03(null);
            this.udf04(null);
            this.udf05(null);
            this.udf06(null);
            this.udf07(null);
            this.udf08(null);
            this.udf09(null);
            this.udf10(null);
        }
    }
}

class listItem extends actionResponseObject {
    id: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    type: KnockoutObservable<string> = ko.observable(null);
    name: KnockoutObservable<string> = ko.observable(null);
    sortOrder: KnockoutObservable<number> = ko.observable(0);
    enabled: KnockoutObservable<boolean> = ko.observable(false);

    Load(data: server.listItem) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.id(data.id);
            this.tenantId(data.tenantId);
            this.type(data.type);
            this.name(data.name);
            this.sortOrder(data.sortOrder);
            this.enabled(data.enabled);
        }
    }
}

class optionPair {
    id: KnockoutObservable<string> = ko.observable(null);
    value: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.optionPair) {
        if (data != null) {
            this.id(data.id);
            this.value(data.value);
        } else {
            this.id(null);
            this.value(null);
        }
    }
}

class signalRUpdate {
    recordId: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    requestId: KnockoutObservable<string> = ko.observable(null);
    itemId: KnockoutObservable<string> = ko.observable(null);
    userId: KnockoutObservable<string> = ko.observable(null);
    updateTypeString: KnockoutObservable<string> = ko.observable(null);
    message: KnockoutObservable<string> = ko.observable(null);
    object: KnockoutObservable<any> = ko.observable(null);

    Load(data: server.signalRUpdate) {
        if (data != null) {
            this.recordId(data.recordId);
            this.tenantId(data.tenantId);
            this.requestId(data.requestId);
            this.itemId(data.itemId);
            this.userId(data.userId);
            this.updateTypeString(data.updateTypeString);
            this.message(data.message);
            this.object(data.object);
        } else {
            this.recordId(null);
            this.tenantId(null);
            this.requestId(null);
            this.itemId(null);
            this.userId(null);
            this.updateTypeString(null);
            this.message(null);
            this.object(null);
        }
    }
}

class setting extends actionResponseObject {
    settingId: KnockoutObservable<number> = ko.observable(0);
    settingName: KnockoutObservable<string> = ko.observable(null);
    settingType: KnockoutObservable<string> = ko.observable(null);
    settingNotes: KnockoutObservable<string> = ko.observable(null);
    settingText: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    userId: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.setting) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.settingId(data.settingId);
            this.settingName(data.settingName);
            this.settingType(data.settingType);
            this.settingNotes(data.settingNotes);
            this.settingText(data.settingText);
            this.tenantId(data.tenantId);
            this.userId(data.userId);
        } else {
            this.actionResponse(new booleanResponse);
            this.settingId(null);
            this.settingName(null);
            this.settingType(null);
            this.settingNotes(null);
            this.settingText(null);
            this.tenantId(null);
            this.userId(null);
        }
    }
}

class simplePost {
    singleItem: KnockoutObservable<string> = ko.observable(null);
    items: KnockoutObservableArray<string> = ko.observableArray([]);

    Load(data: server.simplePost) {
        if (data != null) {
            this.singleItem(data.singleItem);
            this.items(data.items);
        }
    }
}

class record extends actionResponseObject {
    recordId: KnockoutObservable<string> = ko.observable("");
    name: KnockoutObservable<string> = ko.observable("");
    number: KnockoutObservable<number> = ko.observable(0);
    boolean: KnockoutObservable<boolean> = ko.observable(false);
    text: KnockoutObservable<string> = ko.observable("");
    tenantId: KnockoutObservable<string> = ko.observable("");
    userId: KnockoutObservable<string> = ko.observable("");

    Load(data: server.record) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.recordId(data.recordId);
            this.name(data.name);
            this.number(data.number);
            this.boolean(data.boolean);
            this.text(data.text);
            this.tenantId(data.tenantId);
            this.userId(data.userId);
        }
    }
}

class source extends actionResponseObject {
    sourceId: KnockoutObservable<string> = ko.observable("");
    name: KnockoutObservable<string> = ko.observable("");
    number: KnockoutObservable<number> = ko.observable(0);
    boolean: KnockoutObservable<boolean> = ko.observable(false);
    type: KnockoutObservable<string> = ko.observable("");
    date: KnockoutObservable<string> = ko.observable("");
    tenantId: KnockoutObservable<string> = ko.observable("");
    userId: KnockoutObservable<string> = ko.observable("");
}

class tenant extends actionResponseObject {
    tenantId: KnockoutObservable<string> = ko.observable(null);
    name: KnockoutObservable<string> = ko.observable(null);
    tenantCode: KnockoutObservable<string> = ko.observable(null);
    enabled: KnockoutObservable<boolean> = ko.observable(false);
    departments: KnockoutObservableArray<department> = ko.observableArray([]);
    departmentGroups: KnockoutObservableArray<departmentGroup> = ko.observableArray([]);
    tenantSettings: KnockoutObservable<tenantSettings> = ko.observable(new tenantSettings);
    listItems: KnockoutObservableArray<listItem> = ko.observableArray([]);
    udfLabels: KnockoutObservableArray<udfLabel> = ko.observableArray([]);

    Load(data: server.tenant) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.tenantId(data.tenantId);
            this.name(data.name);
            this.tenantCode(data.tenantCode);
            this.enabled(data.enabled);
            this.tenantSettings().Load(data.tenantSettings);

            let d: department[] = [];
            if (data.departments != null) {
                data.departments.forEach(function (e) {
                    let item: department = new department();
                    item.Load(e);
                    d.push(item);
                });
            }
            this.departments(d);

            let dG: departmentGroup[] = [];
            if (data.departmentGroups != null) {
                data.departmentGroups.forEach(function (e) {
                    let item: departmentGroup = new departmentGroup();
                    item.Load(e);
                    dG.push(item);
                });
            }
            this.departmentGroups(dG);

            let li: listItem[] = [];
            if (data.listItems != null) {
                data.listItems.forEach(function (e) {
                    let item: listItem = new listItem();
                    item.Load(e);
                    li.push(item);
                });
            }
            this.listItems(li);

            let u: udfLabel[] = [];
            if (data.udfLabels != null) {
                data.udfLabels.forEach(function (e) {
                    let item: udfLabel = new udfLabel();
                    item.Load(e);
                    u.push(item);
                });
            }
            this.udfLabels(u);
        } else {
            this.actionResponse(new booleanResponse);
            this.tenantId(null);
            this.name(null);
            this.tenantCode(null);
            this.enabled(false);
            this.tenantSettings(new tenantSettings);
            this.departments([]);
            this.departmentGroups([]);
            this.listItems([]);
            this.udfLabels([]);
        }
    }
}

class tenantSettings {
    allowUsersToManageAvatars: KnockoutObservable<boolean> = ko.observable(false);
    allowUsersToManageBasicProfileInfo: KnockoutObservable<boolean> = ko.observable(false);
    allowUsersToManageBasicProfileInfoElements: KnockoutObservableArray<string> = ko.observableArray([]);
    cookieDomain: KnockoutObservable<string> = ko.observable(null);
    eitSsoUrl: KnockoutObservable<string> = ko.observable(null);
    jasonWebTokenKey: KnockoutObservable<string> = ko.observable(null);
    loginOptions: KnockoutObservableArray<string> = ko.observableArray([]);
    workSchedule: KnockoutObservable<workSchedule> = ko.observable(new workSchedule);
    requirePreExistingAccountToLogIn: KnockoutObservable<boolean> = ko.observable(false);

    Load(data: server.tenantSettings) {
        if (data != null) {
            this.allowUsersToManageAvatars(data.allowUsersToManageAvatars);
            this.allowUsersToManageBasicProfileInfo(data.allowUsersToManageBasicProfileInfo);
            this.allowUsersToManageBasicProfileInfoElements(data.allowUsersToManageBasicProfileInfoElements);
            this.cookieDomain(data.cookieDomain);
            this.eitSsoUrl(data.eitSsoUrl);
            this.jasonWebTokenKey(data.jasonWebTokenKey);
            this.loginOptions(data.loginOptions);
            this.workSchedule().Load(data.workSchedule);
            this.requirePreExistingAccountToLogIn(data.requirePreExistingAccountToLogIn);
        } else {
            this.allowUsersToManageAvatars(null);
            this.allowUsersToManageBasicProfileInfo(null);
            this.allowUsersToManageBasicProfileInfoElements([]);
            this.cookieDomain(null);
            this.eitSsoUrl(null);
            this.jasonWebTokenKey(null);
            this.loginOptions(null);
            this.workSchedule(new workSchedule);
            this.requirePreExistingAccountToLogIn(true);
        }
    }
}

class udfLabel extends actionResponseObject {
    id: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    module: KnockoutObservable<string> = ko.observable(null);
    udf: KnockoutObservable<string> = ko.observable(null);
    label: KnockoutObservable<string> = ko.observable(null);
    showColumn: KnockoutObservable<boolean> = ko.observable(false);
    showInFilter: KnockoutObservable<boolean> = ko.observable(false);
    includeInSearch: KnockoutObservable<boolean> = ko.observable(false);
    filterOptions: KnockoutObservableArray<string> = ko.observableArray([]);

    Load(data: server.udfLabel) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.id(data.id);
            this.tenantId(data.tenantId);
            this.module(data.module);
            this.udf(data.udf);
            this.label(data.label);
            this.showColumn(data.showColumn);
            this.showInFilter(data.showInFilter);
            this.includeInSearch(data.includeInSearch);
            this.filterOptions(data.filterOptions);
        }
    }
}

class user extends actionResponseObject {
    userId: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    firstName: KnockoutObservable<string> = ko.observable(null);
    lastName: KnockoutObservable<string> = ko.observable(null);
    displayName: KnockoutObservable<string> = ko.observable(null);
    email: KnockoutObservable<string> = ko.observable(null);
    phone: KnockoutObservable<string> = ko.observable(null);
    username: KnockoutObservable<string> = ko.observable(null);
    employeeId: KnockoutObservable<string> = ko.observable(null);
    departmentId: KnockoutObservable<string> = ko.observable(null);
    departmentName: KnockoutObservable<string> = ko.observable(null);
    title: KnockoutObservable<string> = ko.observable(null);
    location: KnockoutObservable<string> = ko.observable(null);
    enabled: KnockoutObservable<boolean> = ko.observable(false);
    lastLogin: KnockoutObservable<string> = ko.observable(null);
    admin: KnockoutObservable<boolean> = ko.observable(false);
    appAdmin: KnockoutObservable<boolean> = ko.observable(false);
    photo: KnockoutObservable<string> = ko.observable(null);
    password: KnockoutObservable<string> = ko.observable(null);
    preventPasswordChange: KnockoutObservable<boolean> = ko.observable(false);
    hasLocalPassword: KnockoutObservable<boolean> = ko.observable(false);
    authToken: KnockoutObservable<string> = ko.observable(null);
    lastLockoutDate: KnockoutObservable<Date> = ko.observable(null);
    tenants: KnockoutObservableArray<tenant> = ko.observableArray([]);
    userTenants: KnockoutObservableArray<userTenant> = ko.observableArray([]);
    udf01: KnockoutObservable<string> = ko.observable(null);
    udf02: KnockoutObservable<string> = ko.observable(null);
    udf03: KnockoutObservable<string> = ko.observable(null);
    udf04: KnockoutObservable<string> = ko.observable(null);
    udf05: KnockoutObservable<string> = ko.observable(null);
    udf06: KnockoutObservable<string> = ko.observable(null);
    udf07: KnockoutObservable<string> = ko.observable(null);
    udf08: KnockoutObservable<string> = ko.observable(null);
    udf09: KnockoutObservable<string> = ko.observable(null);
    udf10: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.user) {
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
            this.preventPasswordChange(data.preventPasswordChange);
            this.hasLocalPassword(data.hasLocalPassword);
            this.authToken(data.authToken);
            this.lastLockoutDate(data.lastLockoutDate);

            let t: tenant[] = [];
            if (data.tenants != null) {
                data.tenants.forEach(function (e) {
                    let item: tenant = new tenant();
                    item.Load(e);
                    t.push(item);
                });
            }
            this.tenants(t);

            let ut: userTenant[] = [];
            if (data.userTenants != null) {
                data.userTenants.forEach(function (e) {
                    let item: userTenant = new userTenant();
                    item.Load(e);
                    ut.push(item);
                });
            }
            this.userTenants(ut);

            this.udf01(data.udf01);
            this.udf02(data.udf02);
            this.udf03(data.udf03);
            this.udf04(data.udf04);
            this.udf05(data.udf05);
            this.udf06(data.udf06);
            this.udf07(data.udf07);
            this.udf08(data.udf08);
            this.udf09(data.udf09);
            this.udf10(data.udf10);
        } else {
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
            this.preventPasswordChange(false);
            this.hasLocalPassword(null);
            this.authToken(null);
            this.lastLockoutDate(null);
            this.tenants([]);
            this.userTenants([]);
            this.udf01(null);
            this.udf02(null);
            this.udf03(null);
            this.udf04(null);
            this.udf05(null);
            this.udf06(null);
            this.udf07(null);
            this.udf08(null);
            this.udf09(null);
            this.udf10(null);
        }
    }
}

class userPasswordReset extends actionResponseObject {
    userId: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    currentPassword: KnockoutObservable<string> = ko.observable(null);
    newPassword: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.userPasswordReset) {
        if (data != null) {
            this.actionResponse().Load(data.actionResponse);
            this.userId(data.userId);
            this.tenantId(data.tenantId);
            this.currentPassword(data.currentPassword);
            this.newPassword(data.newPassword);
        } else {
            this.actionResponse(new booleanResponse);
            this.userId(null);
            this.tenantId(null);
            this.currentPassword(null);
            this.newPassword(null);
        }
    }
}

class userTenant {
    userId: KnockoutObservable<string> = ko.observable(null);
    tenantId: KnockoutObservable<string> = ko.observable(null);
    tenantCode: KnockoutObservable<string> = ko.observable(null);
    tenantName: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.userTenant) {
        if (data != null) {
            this.userId(data.userId);
            this.tenantId(data.tenantId);
            this.tenantCode(data.tenantCode);
            this.tenantName(data.tenantName);
        } else {
            this.userId(null);
            this.tenantId(null);
            this.tenantCode(null);
            this.tenantName(null);
        }
    }
}

class versionInfo {
    released: KnockoutObservable<string> = ko.observable(null);
    runningSince: KnockoutObservable<number> = ko.observable(0);
    version: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.versionInfo) {
        if (data != null) {
            this.released(data.released);
            this.runningSince(data.runningSince);
            this.version(data.version);
        } else {
            this.released(null);
            this.runningSince(0);
            this.version(null);
        }
    }
}

class workSchedule {
    sunday: KnockoutObservable<boolean> = ko.observable(false);
    sundayAllDay: KnockoutObservable<boolean> = ko.observable(false);
    sundayStart: KnockoutObservable<string> = ko.observable(null);
    sundayEnd: KnockoutObservable<string> = ko.observable(null);

    monday: KnockoutObservable<boolean> = ko.observable(false);
    mondayAllDay: KnockoutObservable<boolean> = ko.observable(false);
    mondayStart: KnockoutObservable<string> = ko.observable(null);
    mondayEnd: KnockoutObservable<string> = ko.observable(null);

    tuesday: KnockoutObservable<boolean> = ko.observable(false);
    tuesdayAllDay: KnockoutObservable<boolean> = ko.observable(false);
    tuesdayStart: KnockoutObservable<string> = ko.observable(null);
    tuesdayEnd: KnockoutObservable<string> = ko.observable(null);

    wednesday: KnockoutObservable<boolean> = ko.observable(false);
    wednesdayAllDay: KnockoutObservable<boolean> = ko.observable(false);
    wednesdayStart: KnockoutObservable<string> = ko.observable(null);
    wednesdayEnd: KnockoutObservable<string> = ko.observable(null);

    thursday: KnockoutObservable<boolean> = ko.observable(false);
    thursdayAllDay: KnockoutObservable<boolean> = ko.observable(false);
    thursdayStart: KnockoutObservable<string> = ko.observable(null);
    thursdayEnd: KnockoutObservable<string> = ko.observable(null);

    friday: KnockoutObservable<boolean> = ko.observable(false);
    fridayAllDay: KnockoutObservable<boolean> = ko.observable(false);
    fridayStart: KnockoutObservable<string> = ko.observable(null);
    fridayEnd: KnockoutObservable<string> = ko.observable(null);

    saturday: KnockoutObservable<boolean> = ko.observable(false);
    saturdayAllDay: KnockoutObservable<boolean> = ko.observable(false);
    saturdayStart: KnockoutObservable<string> = ko.observable(null);
    saturdayEnd: KnockoutObservable<string> = ko.observable(null);

    Load(data: server.workSchedule) {
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
    }
}