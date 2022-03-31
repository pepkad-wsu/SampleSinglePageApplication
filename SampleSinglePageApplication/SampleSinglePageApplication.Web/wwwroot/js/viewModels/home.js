var HomeModel = /** @class */ (function () {
    function HomeModel() {
        var _this = this;
        this.MainModel = ko.observable(window.mainModel);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    /**
     * This is where you would get any data needed for the home page, etc.
     */
    HomeModel.prototype.HomePage = function () {
        // Run any code here for when the home page loads.
    };
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    HomeModel.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "":
                this.HomePage();
                break;
        }
    };
    return HomeModel;
}());
//# sourceMappingURL=home.js.map