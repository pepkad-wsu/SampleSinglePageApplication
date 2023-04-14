interface Window {
    action: string;
    appName: string;
    baseURL: string;
    culture: string;
    id: string;
    GuidEmpty: string;
    loggedIn: boolean;
    missingTenantCode: boolean;
    models: string[];
    objCultureCodes: server.optionPair[];
    objCultures: string[];
    objDefaultLanguage: server.optionPair[];
    objExtra: string[];
    objLanguage: server.language;
    objTenant: server.tenant;
    objTenantList: server.tenant[];
    objUser: server.user;
    objVersionInfo: server.versionInfo;
    showTenantCodeFieldOnLoginForm: boolean;
    showTenantListingWhenMissingTenantCode: boolean;
    tenantCode: string;
    tenantId: string;
    token: string;
    useAuthProviderCustom: boolean;
    useAuthProviderFacebook: boolean;
    useAuthProviderGoogle: boolean;
    useAuthProviderMicrosoftAccount: boolean;
    useAuthProviderOpenId: boolean;
    useTenantCodeInUrl: boolean;

    mainModel: MainModel;
    udfLabelsModel: UdfLabelsModel;
    usersModel: UsersModel;
    departmentsModel: DepartmentsModel;
    languageModel: LanguageModel;
    profileModel: ProfileModel;
    settingsModel: SettingsModel;
    tenantsModel: TenantsModel;


    // we set these in the _Layout file 
    _guid0: string;
    _guid1: string;
    _guid2: string;
    _guid3: string;
    _guid4: string;
    _guid5: string;
    _guid6: string;
    _guid7: string;
    _guid8: string;
    _guid9: string;
}

/**
 * @param {any[]} data - The DTO data object that contains an array of data objects to load.
 * @param {KnockoutObservableArray} - loadInto - The KnockoutObservableArray to load with the DTO dataType.
 * @param {class} - dataType - The class type of the KnockoutObservableArray object.
 */
function DataLoader(data: any[], loadInto: KnockoutObservableArray<any>, dataType: any): void {
    let d = [];
    if (data != null && data.length > 0) {
        data.forEach(function (e) {
            var item = new dataType();
            item.Load(e);
            d.push(item);
        });
    }
    loadInto(d);
}

declare function configureDatePickers(): void;
declare function insertAtCursor(field: string, value: string): void;
declare function RefreshChosenElements(): void;


namespace autoUtility {
    
    export function ConvertStringToBoolean(value: boolean | string): boolean {//SampleSinglePageApplication.DataObjects+SettingType
        let output: boolean = undefined;

        if (tsUtilities.HasValue("" + value)) {
            switch (value) {
                case "True":
                case "true":
                case true:
                    output = true;
                    break;
                case "False":
                case "false":
                case false:
                    output = false;
                    break;
                default:
                    break;
            }
        }

        return output;
    }

    export function ConvertBooleanToString(value: boolean | string): string {//SampleSinglePageApplication.DataObjects+SettingType
        let output: string = "";

        if (tsUtilities.HasValue("" + value)) {
            switch (value) {
                case "True":
                case "true":
                case true:
                    output = "true";
                    break;
                case "False":
                case "false":
                case false:
                    output = "false";
                    break;
                default:
                    break;
            }
        }

        return output;
    }
}

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

