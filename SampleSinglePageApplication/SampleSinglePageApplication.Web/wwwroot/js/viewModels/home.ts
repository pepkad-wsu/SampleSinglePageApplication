class HomeModel {
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    /**
     * This is where you would get any data needed for the home page, etc.
     */
    HomePage() {
        // Run any code here for when the home page loads.
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        switch (this.MainModel().CurrentView()) {
            case "":
                this.HomePage();
                break;
        }
    }
}