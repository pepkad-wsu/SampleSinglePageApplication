var RecordsModel = /** @class */ (function () {
    function RecordsModel() {
        var _this = this;
        this.MainModel = ko.observable(window.mainModel);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    RecordsModel.prototype.GetRecords = function () {
        console.log("records page loaded");
    };
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    RecordsModel.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "records":
                this.GetRecords();
                break;
        }
    };
    return RecordsModel;
}());
//# sourceMappingURL=records.js.map