var SourcesModel = /** @class */ (function () {
    function SourcesModel() {
        var _this = this;
        this.Loading = ko.observable(false);
        this.MainModel = ko.observable(window.mainModel);
        this.Source = ko.observable(new source);
        this.Sources = ko.observableArray([]);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    SourcesModel.prototype.EditSource = function () {
    };
    SourcesModel.prototype.GetSources = function () {
    };
    /**
    * Called when the view changes in the MainModel to do any necessary work in this viewModel.
    */
    SourcesModel.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "editsource":
                this.EditSource();
                break;
            case "sources":
                this.GetSources();
                break;
        }
    };
    return SourcesModel;
}());
//# sourceMappingURL=sources.js.map