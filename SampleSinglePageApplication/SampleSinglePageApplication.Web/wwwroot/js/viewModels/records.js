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
        this.MainModel().SignalRUpdate.subscribe(function () {
            _this.SignalrUpdate();
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
    RecordsModel.prototype.SaveRecord = function (newRecord) {
        var _this = this;
        this.MainModel().Message_Hide();
        var errors = [];
        var focus = "";
        var labelPrefix = newRecord ? "new-" : "edit-";
        if (!tsUtilities.HasValue(this.Record().name())) {
            errors.push("Name is Required");
            if (focus == "") {
                focus = labelPrefix + "record-name";
            }
        }
        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
            return;
        }
        else {
            this.MainModel().Message_Saving();
            var success = function (data) {
                _this.MainModel().Message_Hide();
                if (data != null) {
                    if (data.actionResponse.result) {
                        _this.MainModel().Nav("Records");
                    }
                    else {
                        _this.MainModel().Message_Errors(data.actionResponse.messages);
                    }
                }
                else {
                    _this.MainModel().Message_Error("An unknown error occurred attempting to save this record.");
                }
            };
            var newData = {
                actionResponse: {
                    messages: this.Record().actionResponse().messages(),
                    result: this.Record().actionResponse().result()
                },
                recordId: this.Record().recordId(),
                name: this.Record().name(),
                number: this.Record().number(),
                boolean: this.Record().boolean(),
                text: this.Record().text(),
                tenantId: this.Record().tenantId(),
                userId: this.Record().userId()
            };
            console.log(newData);
            //tsUtilities.AjaxData(window.baseURL + "api/Data/SaveRecord", ko.toJSON(this.Record), success);
            tsUtilities.AjaxData(window.baseURL + "api/Data/SaveRecord", ko.toJSON(newData), success);
        }
    };
    /**
     * This model subscribes to the SignalR updates from the MainModel to update record data when records are changed.
     */
    RecordsModel.prototype.SignalrUpdate = function () {
        //console.log("In Records, SignalR Update", JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate)));
        switch (this.MainModel().SignalRUpdate().updateTypeString().toLowerCase()) {
            case "setting":
                switch (this.MainModel().SignalRUpdate().message().toLowerCase()) {
                    case "recordsaved":
                        // Update the item in the Records list.
                        var recordId_1 = this.MainModel().SignalRUpdate().recordId();
                        var t = ko.utils.arrayFirst(this.Records(), function (item) {
                            return item.recordId() == recordId_1;
                        });
                        if (t != null) {
                            t.Load(JSON.parse(this.MainModel().SignalRUpdate().object()));
                        }
                        else {
                            var newRecord = new record();
                            newRecord.Load(JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate().object)));
                            this.Records.push(newRecord);
                            this.Records().sort(function (l, r) {
                                return l.name() > r.name() ? 1 : -1;
                            });
                        }
                        break;
                }
                break;
        }
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