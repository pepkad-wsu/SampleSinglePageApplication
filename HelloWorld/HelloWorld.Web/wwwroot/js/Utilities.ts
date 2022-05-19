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
    recordModel: RecordModel;
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

