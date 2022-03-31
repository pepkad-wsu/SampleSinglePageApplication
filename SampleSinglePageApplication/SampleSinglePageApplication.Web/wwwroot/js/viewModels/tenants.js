var TenantsModel = /** @class */ (function () {
    function TenantsModel() {
        var _this = this;
        this.AllowDelete = ko.observable(false);
        this.AllowedFileTypes = ko.observable("");
        this.Loading = ko.observable(false);
        this.MainModel = ko.observable(window.mainModel);
        this.Tenant = ko.observable(new tenant);
        this.Tenants = ko.observableArray([]);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
        this.MainModel().SignalRUpdate.subscribe(function () {
            _this.SignalrUpdate();
        });
    }
    /**
     * Called when the URL view is set to "NewTenant" to prepare the Tenant object as a new tenant and sets the focus on the name field.
     */
    TenantsModel.prototype.AddTenant = function () {
        this.AllowDelete(false);
        this.Tenant(new tenant);
        this.Tenant().tenantId(this.MainModel().GuidEmpty());
        this.Tenant().enabled(true);
        this.Tenant().tenantSettings(new tenantSettings);
        this.Tenant().tenantSettings().allowUsersToManageAvatars(true);
        this.Tenant().tenantSettings().allowUsersToManageBasicProfileInfo(true);
        this.Tenant().tenantSettings().loginOptions(["local"]);
        tsUtilities.DelayedFocus("new-tenant-name");
    };
    /**
     * Called when the delete tenant confirmation button is clicked to navigate to the "DeletingTenant" page.
     */
    TenantsModel.prototype.ConfirmDeleteTenant = function () {
        this.MainModel().Message_Hide();
        var confirmed = $("#confirm-delete-tenant").val();
        if (!tsUtilities.HasValue(confirmed) || confirmed != "CONFIRM") {
            this.MainModel().Message_Error("You must type 'CONFIRM' to continue.");
            return;
        }
        this.MainModel().Nav("DeletingTenant", this.MainModel().Id());
    };
    /**
     * Called to confirm deleting a tenant record.
     */
    TenantsModel.prototype.DeleteTenant = function () {
        tsUtilities.DelayedFocus("confirm-delete-tenant");
    };
    /**
     * Shows a notification message while a tenant and all related data is being deleted.
     */
    TenantsModel.prototype.DeletingTenant = function () {
        var _this = this;
        this.MainModel().Message_Error(this.MainModel().Language("DeletingTenantNotification"));
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    window.location.href = window.baseURL + "Admin/Tenants";
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to delete this tenant.");
            }
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteTenant/" + this.MainModel().Id(), null, success);
    };
    /**
     * Called when the URL view is "EditTenant" to load the tenant record and show the edit tenant interface.
     */
    TenantsModel.prototype.EditTenant = function () {
        var _this = this;
        var tenantId = this.MainModel().Id();
        this.AllowedFileTypes("");
        this.AllowDelete(false);
        this.Tenant(new tenant);
        if (tsUtilities.HasValue(tenantId)) {
            this.MainModel().Message_Loading();
            var success = function (data) {
                _this.Loading(false);
                _this.MainModel().Message_Hide();
                if (data != null) {
                    if (data.actionResponse.result) {
                        _this.Tenant().Load(data);
                        if (_this.Tenant().tenantId() != _this.MainModel().Guid1() &&
                            _this.Tenant().tenantId() != _this.MainModel().Guid2()) {
                            _this.AllowDelete(true);
                        }
                    }
                    else {
                        _this.MainModel().Nav("Tenants");
                    }
                }
                else {
                    _this.MainModel().Nav("Tenants");
                }
            };
            this.Loading(true);
            tsUtilities.AjaxData(window.baseURL + "api/Data/GetTenant/" + tenantId, null, success);
        }
    };
    // Called when the URL view is "Tenants" to show the list of tenants.
    TenantsModel.prototype.GetTenants = function () {
        var _this = this;
        var success = function (data) {
            var tenants = [];
            if (data != null) {
                data.forEach(function (e) {
                    var item = new tenant();
                    item.Load(e);
                    tenants.push(item);
                });
            }
            _this.Tenants(tenants);
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetTenants", null, success);
    };
    /**
     * Saved a tenant record.
     * @param newTenant {boolean} - Indicates if this is a new record or an existing record, as the interfaces are different so the focus fields have different IDs.
     */
    TenantsModel.prototype.SaveTenant = function (newTenant) {
        var _this = this;
        this.MainModel().Message_Hide();
        var errors = [];
        var focus = "";
        var labelPrefix = newTenant ? "new-" : "edit-";
        if (!tsUtilities.HasValue(this.Tenant().name())) {
            errors.push("Name is Required");
            if (focus == "") {
                focus = labelPrefix + "tenant-name";
            }
        }
        if (!tsUtilities.HasValue(this.Tenant().tenantCode())) {
            errors.push("Tenant Code is Required");
            if (focus == "") {
                focus = labelPrefix + "tenant-tenantCode";
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
                        _this.MainModel().Nav("Tenants");
                    }
                    else {
                        _this.MainModel().Message_Errors(data.actionResponse.messages);
                    }
                }
                else {
                    _this.MainModel().Message_Error("An unknown error occurred attempting to save this tenant.");
                }
            };
            tsUtilities.AjaxData(window.baseURL + "api/Data/SaveTenant", ko.toJSON(this.Tenant), success);
        }
    };
    /**
     * This model subscribes to the SignalR updates from the MainModel to update tenant data when tenants are changed.
     */
    TenantsModel.prototype.SignalrUpdate = function () {
        //console.log("In Tenants, SignalR Update", JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate)));
        switch (this.MainModel().SignalRUpdate().updateTypeString().toLowerCase()) {
            case "setting":
                switch (this.MainModel().SignalRUpdate().message().toLowerCase()) {
                    case "tenantsaved":
                        // Update the item in the Tenants list.
                        var tenantId_1 = this.MainModel().SignalRUpdate().tenantId();
                        var t = ko.utils.arrayFirst(this.Tenants(), function (item) {
                            return item.tenantId() == tenantId_1;
                        });
                        if (t != null) {
                            t.Load(JSON.parse(this.MainModel().SignalRUpdate().object()));
                        }
                        else {
                            var newTenant = new tenant();
                            newTenant.Load(JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate().object)));
                            this.Tenants.push(newTenant);
                            this.Tenants().sort(function (l, r) {
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
    TenantsModel.prototype.ViewChanged = function () {
        switch (this.MainModel().CurrentView()) {
            case "deletingtenant":
                this.DeletingTenant();
                break;
            case "deletetenant":
                this.DeleteTenant();
                break;
            case "edittenant":
                this.EditTenant();
                break;
            case "newtenant":
                this.AddTenant();
                break;
            case "tenants":
                this.GetTenants();
                break;
        }
    };
    return TenantsModel;
}());
//# sourceMappingURL=tenants.js.map