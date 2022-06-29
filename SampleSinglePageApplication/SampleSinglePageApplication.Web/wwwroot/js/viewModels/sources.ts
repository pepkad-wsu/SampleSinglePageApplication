class SourcesModel {
    Loading: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    Source: KnockoutObservable<source> = ko.observable(new source);
    Sources: KnockoutObservableArray<source> = ko.observableArray([]);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    EditSource(): void {

    }

    GetSources(): void {

    }

    /**
    * Called when the view changes in the MainModel to do any necessary work in this viewModel.
    */
    ViewChanged() {
        switch (this.MainModel().CurrentView()) {
            case "editsource":
                this.EditSource();
                break;
            case "sources":
                this.GetSources();
                break;
        }
    }
}