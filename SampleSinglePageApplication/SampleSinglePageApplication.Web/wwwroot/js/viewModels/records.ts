class RecordsModel {
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    GetRecords(): void {
        console.log("records page loaded");
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        switch (this.MainModel().CurrentView()) {
            case "records":
                this.GetRecords();
                break;
        }
    }
}