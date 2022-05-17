var MainModel = /** @class */ (function () {
    function MainModel() {
        var _this = this;
        this.Loaded = ko.observable(false);
        this.AllSources = ko.observableArray([]);
        console.log("Before: ", this.Loaded());
        var getSourcesFinished = function (items) {
            if (items != null) {
                items.forEach(function (item) {
                    _this.AllSources.push(item);
                });
            }
        };
        tsUtilities.AjaxData("api/Data/GetSources/", null, getSourcesFinished);
    }
    MainModel.prototype.finishedLoading = function (finished) {
        this.Loaded(finished);
    };
    return MainModel;
}());
//# sourceMappingURL=ViewModel.js.map