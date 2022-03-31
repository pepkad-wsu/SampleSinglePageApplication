class Page2Model {
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        switch (this.MainModel().CurrentView()) {
            case "page2":
                break;
        }
    }
}