var UdfLabelsModel = /** @class */ (function () {
    function UdfLabelsModel() {
        var _this = this;
        this.MainModel = ko.observable(window.mainModel);
        this.ManageUdfLabelsShowHelp = ko.observable(false);
        this.UDFLabels = ko.observableArray([]);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
        var labels = [];
        if (this.MainModel().Tenant().udfLabels() != null) {
            this.MainModel().Tenant().udfLabels().forEach(function (e) {
                var item = new udfLabel();
                item.Load(JSON.parse(ko.toJSON(e)));
                labels.push(item);
            });
        }
        this.UDFLabels(labels);
    }
    UdfLabelsModel.prototype.GetUdfLabels = function () {
        var _this = this;
        var success = function (data) {
            var labels = [];
            if (data != null) {
                data.forEach(function (e) {
                    var item = new udfLabel();
                    item.Load(e);
                    labels.push(item);
                });
            }
            _this.UDFLabels(labels);
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUDFLabels/" + this.MainModel().TenantId(), null, success);
    };
    UdfLabelsModel.prototype.ManageUDFLabelsToggleHelp = function () {
        this.ManageUdfLabelsShowHelp(!this.ManageUdfLabelsShowHelp());
    };
    UdfLabelsModel.prototype.Save = function () {
        var _this = this;
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    _this.MainModel().Message(_this.MainModel().Language("Saved"), StyleType.Success, true, true);
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to save UDF Labels");
            }
        };
        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveUDFLabels/" + this.MainModel().TenantId(), ko.toJSON(this.UDFLabels), success);
    };
    UdfLabelsModel.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "udflabels":
                this.GetUdfLabels();
                break;
        }
    };
    return UdfLabelsModel;
}());
//# sourceMappingURL=udfLabels.js.map