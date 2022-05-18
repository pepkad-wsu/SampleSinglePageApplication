var Tenant = /** @class */ (function () {
    function Tenant() {
        this.tenantId = ko.observable("");
        this.name = ko.observable("");
        this.tenantCode = ko.observable("");
        this.enabled = ko.observable(false);
    }
    Tenant.prototype.Load = function (item) {
        if (item != null) {
            this.tenantId(item.tenantId);
            this.name(item.name);
            this.tenantCode(item.tenantCode);
            this.enabled(item.enabled);
        }
    };
    return Tenant;
}());
var Source = /** @class */ (function () {
    function Source() {
        this.sourceId = ko.observable("");
        this.sourceName = ko.observable("");
        this.sourceType = ko.observable("");
        this.sourceCategory = ko.observable("");
        this.sourceTemplate = ko.observable("");
        this.tenantId = ko.observable("");
        this.tenant = ko.observable(null);
        this.tenant(new Tenant());
    }
    Source.prototype.Load = function (item) {
        if (item != null) {
            this.sourceId(item.sourceId);
            this.sourceName(item.sourceName);
            this.sourceType(item.sourceType);
            this.sourceCategory(item.sourceCategory);
            this.sourceTemplate(item.sourceTemplate);
            this.tenantId(item.tenantId);
            if (item.tenant != null) {
                var newTenant = new Tenant();
                newTenant.Load(item.tenant);
                this.tenant(newTenant);
            }
        }
    };
    return Source;
}());
var MainModel = /** @class */ (function () {
    function MainModel() {
        this.View = ko.observable("source-list");
        this.Loaded = ko.observable(false);
        this.Error = ko.observable(false);
        this.ErrorMessages = ko.observableArray([]);
        this.AllSources = ko.observableArray(null);
        this.Source = ko.observable(null);
        this.ListSources();
    }
    MainModel.prototype.EditSource = function (newView, sourceId) {
        this.View(newView);
        var exists = this.AllSources().find(function (value, index) { return value.sourceId() == sourceId; });
        if (exists != null) {
            this.Source(exists);
        }
        else {
            this.Source(null);
        }
    };
    MainModel.prototype.ListSources = function () {
        this.View("source-list");
        this.GetSources();
    };
    /// Reset State sets the page to be unloaded and clears the errors
    MainModel.prototype.ResetState = function () {
        this.Loaded(false);
        this.Error(false);
        this.ErrorMessages([]);
    };
    MainModel.prototype.GetSources = function () {
        var _this = this;
        this.ResetState();
        this.AllSources([]);
        this.Source(null);
        var success = function (data) {
            console.log("data", data);
            if (data != null) {
                if (data.sources != null && data.sources.length > 0) {
                    data.sources.forEach(function (item) {
                        var newSource = new Source();
                        newSource.Load(item);
                        _this.AllSources.push(newSource);
                    });
                }
            }
            _this.Error(false);
            _this.ErrorMessages([]);
        };
        var failure = function (data) {
            console.log("this gets called when the api call fails", data);
            _this.ErrorMessages.push('Get sources failed');
            _this.Error(true);
        };
        var always = function (data) {
            console.log("this gets called no matter what, if we succeed or if we fail");
            // since we have finished we can mark the page as loaded
            _this.Loaded(true);
            _this.View("source-list");
        };
        tsUtilities.AjaxData("api/Data/GetSources/", null, success, failure, always);
    };
    MainModel.prototype.SaveSource = function () {
        var _this = this;
        this.ResetState();
        var success = function (data) {
            console.log("data", data);
            if (data != null) {
                if (data.result) {
                }
            }
        };
        var failure = function (data) {
            console.log("failed to save", data);
            _this.ErrorMessages.push('Save source failed');
            _this.Error(true);
        };
        var always = function (data) {
            console.log("this gets called no matter what, if we succeed or if we fail");
            // since we have finished we can mark the page as loaded
            _this.Loaded(true);
            _this.View("source-list");
        };
        var data = {
            sourceCategory: this.Source().sourceCategory(),
            sourceId: this.Source().sourceId(),
            sourceName: this.Source().sourceName(),
            sourceTemplate: this.Source().sourceTemplate(),
            sourceType: this.Source().sourceType(),
            tenant: null,
            tenantId: this.Source().tenantId()
        };
        tsUtilities.AjaxData("api/Data/SaveSource/", ko.toJSON(data), success, failure, always);
    };
    return MainModel;
}());
//# sourceMappingURL=ViewModel.js.map