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

class MainModel {
    Loaded: KnockoutObservable<boolean> = ko.observable(false);
    AllSources: KnockoutObservableArray<any> = ko.observableArray([]);

    constructor() {
        console.log("Before: ", this.Loaded());

        let getSourcesFinished = (items: any[]) => {
            if (items != null) {
                items.forEach(item => {
                    this.AllSources.push(item);
                });
            }
        }
        tsUtilities.AjaxData("api/Data/GetSources/", null, getSourcesFinished);
    }

    finishedLoading(finished: boolean): void {
        this.Loaded(finished);
    }
}