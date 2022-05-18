interface Window {
    action: string;
    appName: string;
    baseURL: string;
    id: string;
    loggedIn: boolean;
    missingTenantCode: boolean;
    tenantCode: string;
    tenantId: string;
    token: string;
    useTenantCodeInUrl: boolean;
    mainModel: MainModel;
}

class Tenant{
    tenantId: KnockoutObservable<string> = ko.observable("");
    name: KnockoutObservable<string> = ko.observable("");
    tenantCode: KnockoutObservable<string> = ko.observable("");
    enabled: KnockoutObservable<boolean> = ko.observable(false);

    Load(item: server.Tenant) {
        if (item != null) {
            this.tenantId(item.tenantId);
            this.name(item.name);
            this.tenantCode(item.tenantCode);
            this.enabled(item.enabled);
        }
    }
}

class Source{
    sourceId: KnockoutObservable<string> = ko.observable("");
    sourceName: KnockoutObservable<string> = ko.observable("");
    sourceType: KnockoutObservable<string> = ko.observable("");
    sourceCategory: KnockoutObservable<string> = ko.observable("");
    sourceTemplate: KnockoutObservable<string> = ko.observable("");
    tenantId: KnockoutObservable<string> = ko.observable("");
    tenant: KnockoutObservable<Tenant> = ko.observable(null);

    constructor() {
        this.tenant(new Tenant());
    }

    Load(item: server.Source) {
        if (item != null) {
            this.sourceId(item.sourceId);
            this.sourceName(item.sourceName);
            this.sourceType(item.sourceType);
            this.sourceCategory(item.sourceCategory);
            this.sourceTemplate(item.sourceTemplate);
            this.tenantId(item.tenantId);

            if (item.tenant != null) {
                let newTenant = new Tenant();
                newTenant.Load(item.tenant);
                this.tenant(newTenant);
            }
        }
    }
}

declare module server {
    interface GetSourcesResult {
        actionResponse: BooleanResponse;
        sources: Source[];
    }
    interface BooleanResponse {
        messages: string[];
        result: boolean;
    }
    interface Tenant {
        tenantId: string;
        name: string;
        tenantCode: string;
        enabled: boolean;
    }
    interface Source {
        sourceId: string;
        sourceName: string;
        sourceType: string;
        sourceCategory: string;
        sourceTemplate: string;
        tenantId: string;
        tenant: Tenant;
    }
}

class MainModel {
    View: KnockoutObservable<string> = ko.observable("source-list");

    Loaded: KnockoutObservable<boolean> = ko.observable(false);

    Error: KnockoutObservable<boolean> = ko.observable(false);
    ErrorMessages: KnockoutObservableArray<string> = ko.observableArray([]);

    AllSources: KnockoutObservableArray<Source> = ko.observableArray(null);
    Source: KnockoutObservable<Source> = ko.observable(null);

    constructor() {
        this.ListSources();
    }

    EditSource(newView: string, sourceId: string): void {
        this.View(newView);
        let exists: Source = this.AllSources().find((value: Source, index: number) => { return value.sourceId() == sourceId; });
        if (exists != null) {
            this.Source(exists)
        } else {
            this.Source(null);
        }
    }

    ListSources() {
        this.View("source-list");
        this.GetSources();
    }

    /// Reset State sets the page to be unloaded and clears the errors
    private ResetState(): void {
        this.Loaded(false);
        this.Error(false);
        this.ErrorMessages([]);
    }

    GetSources(): void {
        this.ResetState();

        this.AllSources([]);
        this.Source(null);

        let success = (data: server.GetSourcesResult) => {
            console.log("data", data);
            if (data != null) {
                if (data.sources != null && data.sources.length > 0) {
                    data.sources.forEach((item: server.Source) => {
                        let newSource = new Source();
                        newSource.Load(item);
                        this.AllSources.push(newSource);
                    });
                }
            }

            this.Error(false);
            this.ErrorMessages([]);
        }
        let failure = (data: any) => {
            console.log("this gets called when the api call fails", data);
            this.ErrorMessages.push('Get sources failed');
            this.Error(true);

        }
        let always = (data: any) => {
            console.log("this gets called no matter what, if we succeed or if we fail");
            // since we have finished we can mark the page as loaded
            this.Loaded(true);
            this.View("source-list");
        }
        tsUtilities.AjaxData("api/Data/GetSources/", null, success,failure,always);
    }

    SaveSource(): void {
        this.ResetState();

        let success = (data: server.BooleanResponse) => {
            console.log("data", data);
            if (data != null) {
                if (data.result) {

                }
            }
        }
        let failure = (data: any) => {
            console.log("failed to save", data);
            this.ErrorMessages.push('Save source failed');
            this.Error(true);

        }
        let always = (data: any) => {
            console.log("this gets called no matter what, if we succeed or if we fail");
            // since we have finished we can mark the page as loaded
            this.Loaded(true);
            this.View("source-list");
        }

        let data: server.Source = {
            sourceCategory: this.Source().sourceCategory(),
            sourceId: this.Source().sourceId(),
            sourceName: this.Source().sourceName(),
            sourceTemplate: this.Source().sourceTemplate(),
            sourceType: this.Source().sourceType(),
            tenant: null,
            tenantId: this.Source().tenantId()
        };

        tsUtilities.AjaxData("api/Data/SaveSource/", ko.toJSON(data), success, failure, always);
    }
}