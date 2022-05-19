
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