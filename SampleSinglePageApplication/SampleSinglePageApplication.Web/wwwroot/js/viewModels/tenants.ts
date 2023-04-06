class TenantsModel {
    AllowDelete: KnockoutObservable<boolean> = ko.observable(false);
    Loading: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    Tenant: KnockoutObservable<tenant> = ko.observable(new tenant);
    Tenants: KnockoutObservableArray<tenant> = ko.observableArray([]);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });

        this.MainModel().SignalRUpdate.subscribe(() => {
            this.SignalrUpdate();
        });
    }

    AddExternalUserDataSource(type: string): void {
        let sortOrder: number = 0;

        if (this.Tenant().tenantSettings().externalUserDataSources() == null) {
            this.Tenant().tenantSettings().externalUserDataSources([]);
        } else {
            this.Tenant().tenantSettings().externalUserDataSources().forEach(function (item) {
                sortOrder++;
                if (item.sortOrder() > sortOrder) {
                    sortOrder = item.sortOrder() + 1;
                }
            });
        }

        let item: externalDataSource = new externalDataSource();
        item.sortOrder(sortOrder);
        item.type(type);
        item.active(true);

        if (type == 'csharp') {
            let code: string =
                "namespace CustomCode {\n" +
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
    }

    /**
     * Called when the URL view is set to "NewTenant" to prepare the Tenant object as a new tenant and sets the focus on the name field.
     */
    AddTenant(): void {
        this.AllowDelete(false);
        this.Tenant(new tenant);
        this.Tenant().tenantId(this.MainModel().GuidEmpty());
        this.Tenant().enabled(true);
        this.Tenant().tenantSettings(new tenantSettings);
        this.Tenant().tenantSettings().allowUsersToManageAvatars(true);
        this.Tenant().tenantSettings().allowUsersToManageBasicProfileInfo(true);
        this.Tenant().tenantSettings().loginOptions(["local"]);

        tsUtilities.DelayedFocus("new-tenant-name");
    }

    AuthOptionCustom = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.Tenant().tenantSettings().loginOptions() != null) {
            var hasOption = ko.utils.arrayFirst(this.Tenant().tenantSettings().loginOptions(), function (item) {
                return item == "custom";
            });
            output = hasOption != null;
        }

        return output;
    });

    AuthOptionFacebook = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.Tenant().tenantSettings().loginOptions() != null) {
            var hasOption = ko.utils.arrayFirst(this.Tenant().tenantSettings().loginOptions(), function (item) {
                return item == "facebook";
            });
            output = hasOption != null;
        }

        return output;
    });

    AuthOptionGoogle = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.Tenant().tenantSettings().loginOptions() != null) {
            var hasOption = ko.utils.arrayFirst(this.Tenant().tenantSettings().loginOptions(), function (item) {
                return item == "google";
            });
            output = hasOption != null;
        }

        return output;
    });

    AuthOptionMicrosoft = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.Tenant().tenantSettings().loginOptions() != null) {
            var hasOption = ko.utils.arrayFirst(this.Tenant().tenantSettings().loginOptions(), function (item) {
                return item == "microsoft";
            });
            output = hasOption != null;
        }

        return output;
    });

    AuthOptionOpenId = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.Tenant().tenantSettings().loginOptions() != null) {
            var hasOption = ko.utils.arrayFirst(this.Tenant().tenantSettings().loginOptions(), function (item) {
                return item == "openid";
            });
            output = hasOption != null;
        }

        return output;
    });

    /**
     * Called when the delete tenant confirmation button is clicked to navigate to the "DeletingTenant" page.
     */
    ConfirmDeleteTenant(): void {
        this.MainModel().Message_Hide();

        let confirmed: string = $("#confirm-delete-tenant").val();
        if (!tsUtilities.HasValue(confirmed) || confirmed != "CONFIRM") {
            this.MainModel().Message_Error("You must type 'CONFIRM' to continue.");
            return;
        }

        this.MainModel().Nav("DeletingTenant", this.MainModel().Id());
    }

    DeleteExternalUserDataSource(data: externalDataSource): void {
        this.Tenant().tenantSettings().externalUserDataSources.remove(data);
    }

    /**
     * Called to confirm deleting a tenant record.
     */
    DeleteTenant(): void {
        tsUtilities.DelayedFocus("confirm-delete-tenant");
    }

    /**
     * Shows a notification message while a tenant and all related data is being deleted.
     */
    DeletingTenant(): void {
        this.MainModel().Message_Error(this.MainModel().Language("DeletingTenantNotification"));

        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    window.location.href = window.baseURL + "/Tenants";
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to delete this tenant.");
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteTenant/" + this.MainModel().Id(), null, success);
    }

    /**
     * Called when the URL view is "EditTenant" to load the tenant record and show the edit tenant interface.
     */
    EditTenant(): void {
        this.MainModel().Message_Hide();

        let tenantId: string = this.MainModel().Id();

        this.AllowDelete(false);

        this.Tenant(new tenant);

        if (tsUtilities.HasValue(tenantId)) {
            let success: Function = (data: server.tenant) => {
                this.Loading(false);
                if (data != null) {
                    if (data.actionResponse.result) {
                        this.Tenant().Load(data);

                        if (this.Tenant().tenantId() != this.MainModel().Guid1() &&
                            this.Tenant().tenantId() != this.MainModel().Guid2()) {
                            this.AllowDelete(true);
                        }
                    } else {
                        this.MainModel().Nav("Tenants");
                    }
                } else {
                    this.MainModel().Nav("Tenants");
                }
            };

            this.Loading(true);
            tsUtilities.AjaxData(window.baseURL + "api/Data/GetTenant/" + tenantId, null, success);
        }
    }

    // Called when the URL view is "Tenants" to show the list of tenants.
    GetTenants(): void {
        let success: Function = (data: server.tenant[]) => {
            let tenants: tenant[] = [];
            if (data != null) {
                data.forEach(function (e) {
                    let item: tenant = new tenant();
                    item.Load(e);
                    tenants.push(item);
                });
            }
            this.Tenants(tenants);
            this.MainModel().Tenants(tenants);
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/GetTenants", null, success);
    }

    /**
     * Saved a tenant record.
     * @param newTenant {boolean} - Indicates if this is a new record or an existing record, as the interfaces are different so the focus fields have different IDs.
     */
    SaveTenant(newTenant: boolean): void {
        this.MainModel().Message_Hide();

        let errors: string[] = [];
        let focus: string = "";

        let labelPrefix: string = newTenant ? "new-" : "edit-";

        if (!tsUtilities.HasValue(this.Tenant().name())) {
            errors.push("Name is Required");
            if (focus == "") { focus = labelPrefix + "tenant-name"; }
        }
        if (!tsUtilities.HasValue(this.Tenant().tenantCode())) {
            errors.push("Tenant Code is Required");
            if (focus == "") { focus = labelPrefix + "tenant-tenantCode"; }
        }

        if (this.Tenant().tenantSettings().externalUserDataSources() != null && this.Tenant().tenantSettings().externalUserDataSources().length > 0) {
            let missingDataSourceInfo: boolean = false;

            this.Tenant().tenantSettings().externalUserDataSources().forEach(function (item) {
                if (missingDataSourceInfo == false) {
                    if (!tsUtilities.HasValue(item.name()) || !tsUtilities.HasValue(item.type())) {
                        missingDataSourceInfo = true;
                    } else {
                        if (item.type() == "sql" && !tsUtilities.HasValue(item.connectionString())) {
                            missingDataSourceInfo = true;
                        }
                        if (!tsUtilities.HasValue(item.source())) {
                            missingDataSourceInfo = true;
                        }
                    }
                }
            });

            if (missingDataSourceInfo) {
                errors.push("When configuring External User Data Sources you must complete all required fields.");
            }
        }

        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
            return;
        } else {
            this.MainModel().Message_Saving();

            let success: Function = (data: server.tenant) => {
                this.MainModel().Message_Hide();
                if (data != null) {
                    if (data.actionResponse.result) {
                        if (newTenant) {
                            this.MainModel().ReloadUser();
                        }
                        this.MainModel().Nav("Tenants");
                    } else {
                        this.MainModel().Message_Errors(data.actionResponse.messages);
                    }
                } else {
                    this.MainModel().Message_Error("An unknown error occurred attempting to save this tenant.");
                }
            };

            tsUtilities.AjaxData(window.baseURL + "api/Data/SaveTenant", ko.toJSON(this.Tenant), success);
        }
    }

    ShowEitSsoUrl = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.Tenant().tenantSettings().loginOptions() != null && this.Tenant().tenantSettings().loginOptions().length > 0) {
            output = this.Tenant().tenantSettings().loginOptions().indexOf("eitsso") > -1;
        }

        return output;
    });

    ShowLocalLoginSignup = ko.computed((): boolean => {
        let output: boolean = false;

        if (this.Tenant().tenantSettings().loginOptions() != null && this.Tenant().tenantSettings().loginOptions().length > 0) {
            output = this.Tenant().tenantSettings().loginOptions().indexOf("local") > -1;
        }

        return output;
    });

    /**
     * This model subscribes to the SignalR updates from the MainModel to update tenant data when tenants are changed.
     */
    SignalrUpdate(): void {
        //console.log("In Tenants, SignalR Update", JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate)));
        switch (this.MainModel().SignalRUpdate().updateTypeString().toLowerCase()) {
            case "setting":
                switch (this.MainModel().SignalRUpdate().message().toLowerCase()) {
                    case "tenantsaved":
                        // Update the item in the Tenants list.
                        let tenantId: string = this.MainModel().SignalRUpdate().tenantId();
                        let t: tenant = ko.utils.arrayFirst(this.Tenants(), function (item) {
                            return item.tenantId() == tenantId;
                        });
                        if (t != null) {
                            t.Load(JSON.parse(this.MainModel().SignalRUpdate().object()));
                        } else {
                            let newTenant: tenant = new tenant();
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
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged(): void {
        let allowed: boolean = this.MainModel().User().appAdmin();

        switch (this.MainModel().CurrentView()) {
            case "deletingtenant":
                if (allowed) {
                    this.DeletingTenant();
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;

            case "deletetenant":
                if (allowed) {
                    this.DeleteTenant();
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;

            case "edittenant":
                if (allowed) {
                    this.EditTenant();
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;

            case "newtenant":
                if (allowed) {
                    this.AddTenant();
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;

            case "tenants":
                if (allowed) {
                    this.GetTenants();
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
        }
    }
}