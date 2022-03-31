﻿class SettingsModel {
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    /**
     * This is where you would make your Web API call to get the settings for the given tenant.
     */
    GetSettings() {
        // Get any settings here.
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged() {
        switch (this.MainModel().CurrentView()) {
            case "settings":
                this.GetSettings();
                break;
        }
    }
}