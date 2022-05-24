var RecordsModel = /** @class */ (function () {
    function RecordsModel() {
        var _this = this;
        this.Loading = ko.observable(false);
        this.MainModel = ko.observable(window.mainModel);
        this.Record = ko.observable(new record);
        this.Records = ko.observableArray([]);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    RecordsModel.prototype.GetRecords = function () {
        var _this = this;
        console.log("records page loaded");
        var success = function (data) {
            console.log("data records call: ", data);
            var records = [];
            if (data != null) {
                data.forEach(function (e) {
                    var item = new record();
                    item.Load(e);
                    records.push(item);
                });
            }
            _this.Records(records);
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetRecords", null, success);
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