var SettingsModel = /** @class */ (function () {
    function SettingsModel() {
        var _this = this;
        this.MainModel = ko.observable(window.mainModel);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    /**
     * This is where you would make your Web API call to get the settings for the given tenant.
     */
    SettingsModel.prototype.GetSettings = function () {
        // Get any settings here.
    };
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    SettingsModel.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "settings":
                this.GetSettings();
                break;
        }
    };
    return SettingsModel;
}());
//# sourceMappingURL=settings.js.map