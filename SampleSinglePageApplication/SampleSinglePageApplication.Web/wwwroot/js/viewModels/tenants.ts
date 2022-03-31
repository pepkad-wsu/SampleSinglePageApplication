class TenantsModel {
    AllowDelete: KnockoutObservable<boolean> = ko.observable(false);
    AllowedFileTypes: KnockoutObservable<string> = ko.observable("");
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
                    window.location.href = window.baseURL + "Admin/Tenants";
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
        let tenantId: string = this.MainModel().Id();

        this.AllowedFileTypes("");
        this.AllowDelete(false);

        this.Tenant(new tenant);

        if (tsUtilities.HasValue(tenantId)) {
            this.MainModel().Message_Loading();

            let success: Function = (data: server.tenant) => {
                this.Loading(false);
                this.MainModel().Message_Hide();
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
    }
}