var TenantsModel = /** @class */ (function () {
    function TenantsModel() {
        var _this = this;
        this.AllowDelete = ko.observable(false);
        this.Loading = ko.observable(false);
        this.MainModel = ko.observable(window.mainModel);
        this.Tenant = ko.observable(new tenant);
        this.Tenants = ko.observableArray([]);
        this.AuthOptionCustom = ko.computed(function () {
            var output = false;
            if (_this.Tenant().tenantSettings().loginOptions() != null) {
                var hasOption = ko.utils.arrayFirst(_this.Tenant().tenantSettings().loginOptions(), function (item) {
                    return item == "custom";
                });
                output = hasOption != null;
            }
            return output;
        });
        this.AuthOptionFacebook = ko.computed(function () {
            var output = false;
            if (_this.Tenant().tenantSettings().loginOptions() != null) {
                var hasOption = ko.utils.arrayFirst(_this.Tenant().tenantSettings().loginOptions(), function (item) {
                    return item == "facebook";
                });
                output = hasOption != null;
            }
            return output;
        });
        this.AuthOptionGoogle = ko.computed(function () {
            var output = false;
            if (_this.Tenant().tenantSettings().loginOptions() != null) {
                var hasOption = ko.utils.arrayFirst(_this.Tenant().tenantSettings().loginOptions(), function (item) {
                    return item == "google";
                });
                output = hasOption != null;
            }
            return output;
        });
        this.AuthOptionMicrosoft = ko.computed(function () {
            var output = false;
            if (_this.Tenant().tenantSettings().loginOptions() != null) {
                var hasOption = ko.utils.arrayFirst(_this.Tenant().tenantSettings().loginOptions(), function (item) {
                    return item == "microsoft";
                });
                output = hasOption != null;
            }
            return output;
        });
        this.AuthOptionOpenId = ko.computed(function () {
            var output = false;
            if (_this.Tenant().tenantSettings().loginOptions() != null) {
                var hasOption = ko.utils.arrayFirst(_this.Tenant().tenantSettings().loginOptions(), function (item) {
                    return item == "openid";
                });
                output = hasOption != null;
            }
            return output;
        });
        this.ShowEitSsoUrl = ko.computed(function () {
            var output = false;
            if (_this.Tenant().tenantSettings().loginOptions() != null && _this.Tenant().tenantSettings().loginOptions().length > 0) {
                output = _this.Tenant().tenantSettings().loginOptions().indexOf("eitsso") > -1;
            }
            return output;
        });
        this.ShowLocalLoginSignup = ko.computed(function () {
            var output = false;
            if (_this.Tenant().tenantSettings().loginOptions() != null && _this.Tenant().tenantSettings().loginOptions().length > 0) {
                output = _this.Tenant().tenantSettings().loginOptions().indexOf("local") > -1;
            }
            return output;
        });
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
        this.MainModel().SignalRUpdate.subscribe(function () {
            _this.SignalrUpdate();
        });
    }
    TenantsModel.prototype.AddExternalUserDataSource = function (type) {
        var sortOrder = 0;
        if (this.Tenant().tenantSettings().externalUserDataSources() == null) {
            this.Tenant().tenantSettings().externalUserDataSources([]);
        }
        else {
            this.Tenant().tenantSettings().externalUserDataSources().forEach(function (item) {
                sortOrder++;
                if (item.sortOrder() > sortOrder) {
                    sortOrder = item.sortOrder() + 1;
                }
            });
        }
        var item = new externalDataSource();
        item.sortOrder(sortOrder);
        item.type(type);
        item.active(true);
        if (type == 'csharp') {
            var code = "namespace CustomCode {\n" +
                "  using SampleSinglePageApplication;\n" +
                "  using SampleSinglePageApplication.EFModels.EFModels;\n" +
                "  using System;\n" +
                "  using System.Data;\n" +
                "  using System.Drawing;\n" +
                "  using System.Text;\n" +
                "  using System.Text.RegularExpressions;\n" +
                "  using System.Xml;\n" +
                "  using System.Xml.Serialization;\n" +
                "  using System.Net.Http.Headers;\n" +
                "  using JWT;\n" +
                "  using JWT.Algorithms;\n" +
                "  using JWT.Serializers;\n" +
                "  using Microsoft.EntityFrameworkCore;\n" +
                "  using System.Net;\n" +
                "  using System.Net.Mail;\n" +
                "  using System.Data.SqlClient;\n" +
                "  using Newtonsoft.Json;\n" +
                "  using Newtonsoft.Json.Converters;\n" +
                "  using System.Dynamic;\n" +
                "  using Microsoft.Graph;\n" +
                "  using Microsoft.Identity.Client;\n" +
                "  // Other using statements, etc.\n" +
                "\n" +
                "  public class CustomDynamicCode {\n" +
                "    public DataObjects.User? FindUser(string EmployeeId, string Username, string Email){\n" +
                "      DataObjects.User? output = null;\n" +
                "\n" +
                "      // Execute your code here to find a user and update the output object\n" +
                "\n" +
                "      return output;\n" +
                "    }\n" +
                "  }\n" +
                "}";
            item.source(code);
        }
        this.Tenant().tenantSettings().externalUserDataSources.push(item);
    };
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
    TenantsModel.prototype.DeleteExternalUserDataSource = function (data) {
        this.Tenant().tenantSettings().externalUserDataSources.remove(data);
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
                    window.location.href = window.baseURL + "/Tenants";
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
        this.MainModel().Message_Hide();
        var tenantId = this.MainModel().Id();
        this.AllowDelete(false);
        this.Tenant(new tenant);
        if (tsUtilities.HasValue(tenantId)) {
            var success = function (data) {
                _this.Loading(false);
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
            _this.MainModel().Tenants(tenants);
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
        if (this.Tenant().tenantSettings().externalUserDataSources() != null && this.Tenant().tenantSettings().externalUserDataSources().length > 0) {
            var missingDataSourceInfo_1 = false;
            this.Tenant().tenantSettings().externalUserDataSources().forEach(function (item) {
                if (missingDataSourceInfo_1 == false) {
                    if (!tsUtilities.HasValue(item.name()) || !tsUtilities.HasValue(item.type())) {
                        missingDataSourceInfo_1 = true;
                    }
                    else {
                        if (item.type() == "sql" && !tsUtilities.HasValue(item.connectionString())) {
                            missingDataSourceInfo_1 = true;
                        }
                        if (!tsUtilities.HasValue(item.source())) {
                            missingDataSourceInfo_1 = true;
                        }
                    }
                }
            });
            if (missingDataSourceInfo_1) {
                errors.push("When configuring External User Data Sources you must complete all required fields.");
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
                        if (newTenant) {
                            _this.MainModel().ReloadUser();
                        }
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
        var allowed = this.MainModel().User().appAdmin();
        switch (this.MainModel().CurrentView()) {
            case "deletingtenant":
                if (allowed) {
                    this.DeletingTenant();
                }
                else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
            case "deletetenant":
                if (allowed) {
                    this.DeleteTenant();
                }
                else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
            case "edittenant":
                if (allowed) {
                    this.EditTenant();
                }
                else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
            case "newtenant":
                if (allowed) {
                    this.AddTenant();
                }
                else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
            case "tenants":
                if (allowed) {
                    this.GetTenants();
                }
                else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
        }
    };
    return TenantsModel;
}());
//# sourceMappingURL=tenants.js.map