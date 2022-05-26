var RecordsModel = /** @class */ (function () {
    function RecordsModel() {
        var _this = this;
        this.AllowDelete = ko.observable(false);
        this.Loading = ko.observable(false);
        this.MainModel = ko.observable(window.mainModel);
        this.Record = ko.observable(new record);
        this.Records = ko.observableArray([]);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    /**
    * Called when the URL view is "EditRecord" to load the record and show the edit record interface.
    */
    RecordsModel.prototype.EditRecord = function () {
        var _this = this;
        var recordId = this.MainModel().Id();
        this.AllowDelete(false);
        this.Record(new record);
        if (tsUtilities.HasValue(recordId)) {
            this.MainModel().Message_Loading();
            var success = function (data) {
                _this.Loading(false);
                _this.MainModel().Message_Hide();
                if (data != null) {
                    if (data.actionResponse.result) {
                        _this.Record().Load(data);
                    }
                    else {
                        _this.MainModel().Nav("Records");
                    }
                }
                else {
                    _this.MainModel().Nav("Records");
                }
            };
            this.Loading(true);
            tsUtilities.AjaxData(window.baseURL + "api/Data/GetRecord/" + recordId, null, success);
        }
    };
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
            case "editrecord":
                this.EditRecord();
                break;
            case "records":
                this.GetRecords();
                break;
        }
    };
    return RecordsModel;
}());
//# sourceMappingURL=records.js.map