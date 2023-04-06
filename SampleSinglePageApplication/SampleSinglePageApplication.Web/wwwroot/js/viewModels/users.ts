class UsersModel {
    AddToSelectedTeam: KnockoutObservable<string> = ko.observable("");
    ConfirmDelete: KnockoutObservable<string> = ko.observable("");
    Filter: KnockoutObservable<filterUsers> = ko.observable(new filterUsers);
    Loading: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    NewPassword: KnockoutObservable<string> = ko.observable("");
    ResettingUserPassword: KnockoutObservable<boolean> = ko.observable(false);
    User: KnockoutObservable<user> = ko.observable(new user);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });

        this.MainModel().SignalRUpdate.subscribe(() => {
            this.SignalrUpdate();
        });

        setTimeout("setupUserPhotoDropZone()", 0);

        setTimeout(() => this.StartFilterMonitoring(), 1000);
    }

    /**
     * Method fires when the URL action is "NewUser"
     */
    AddUser(): void {
        this.User(new user);
        this.User().userId(this.MainModel().GuidEmpty());
        this.User().tenantId(this.MainModel().TenantId());

        this.MainModel().UDFFieldsRender("edit-user-udf-fields", "Users", JSON.parse(ko.toJSON(this.User)));

        tsUtilities.DelayedFocus("edit-user-firstName");
    }

    /**
     * Clears the values for the user search filter.
     */
    ClearFilter(): void {
        this.Filter().keyword(null);
        this.Filter().filterDepartments([]);
        this.Filter().enabled(null);
        this.Filter().admin(null);
        this.Filter().udf01(null);
        this.Filter().udf02(null);
        this.Filter().udf03(null);
        this.Filter().udf04(null);
        this.Filter().udf05(null);
        this.Filter().udf06(null);
        this.Filter().udf07(null);
        this.Filter().udf08(null);
        this.Filter().udf09(null);
        this.Filter().udf10(null);
        this.Filter().page(1);
        this.Filter().sort(null);
        this.Filter().sortOrder(null);
        this.GetUsers();
    }

    /**
     * Deletes a user.
     */
    DeleteUser(): void {
        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    this.MainModel().Nav("Users");
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to delete the user.");
            }
        };

        this.MainModel().Message_Deleting();
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteUser/" + this.MainModel().Id(), null, success);
    }

    /**
     * Deletes a user profile photo.
     */
    DeleteUserPhoto(): void {
        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.result) {
                    this.User().photo(null);
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to delete the user photo.");
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteUserPhoto/" + this.User().userId(), null, success);
    }

    /**
     * Method fires when the URL action is "EditUser"
     */
    EditUser(): void {
        this.MainModel().Message_Hide();
        let userId: string = this.MainModel().Id();
        this.User(new user);
        this.User().userId(null);

        setTimeout("resetUserPhotoDropZone()", 0);

        if (tsUtilities.HasValue(userId)) {
            let success: Function = (data: server.user) => {
                if (data != null) {
                    if (data.actionResponse.result) {
                        this.User().Load(data);
                        tsUtilities.DelayedFocus("edit-user-firstName");

                        this.MainModel().UDFFieldsRender("edit-user-udf-fields", "Users", JSON.parse(ko.toJSON(this.User)));

                        this.Loading(false);
                    } else {
                        this.MainModel().Message_Errors(data.actionResponse.messages);
                    }
                } else {
                    this.MainModel().Message_Error("An unknown error occurred attempting to load the user record.");
                }
            };

            this.Loading(true);
            tsUtilities.AjaxData(window.baseURL + "api/data/GetUser/" + userId, null, success);
        } else {
            this.MainModel().Message_Error("No valid UserId received.");
        }

    }

    /**
     * The callback method used by the paged recordset control to handle the action on the record.
     * @param record {server.user} - The object being passed is a JSON object, not an observable.
     */
    EditUserCallback(record: server.user): void {
        if (record != undefined && record != null && tsUtilities.HasValue(record.userId)) {
            this.MainModel().Nav("EditUser", record.userId);
        }
    }

    /**
     * Called when the user filter changes to reload user records, unless the filter is changing because
     * records are being reloaded.
     */
    FilterChanged(): void {
        if (!this.Filter().loading()) {
            this.GetUsers();
        }
    }

    /**
     * Loads the saved filter that is stored in a cookie as a JSON object.
     */
    GetSavedFilter(): void {
        this.Filter(new filterUsers);
        this.Filter().tenantId(this.MainModel().TenantId());

        let savedFilter: string = tsUtilities.CookieRead("saved-filter-users");
        if (tsUtilities.HasValue(savedFilter)) {
            this.Filter().Load(JSON.parse(savedFilter));
            this.StartFilterMonitoring();
        }

        this.GetUsers();
    }

    /**
     * Called when the filter changes or when the page loads to get the users matching the current filter.
     */
    GetUsers(): void {
        // Load the filter
        this.Filter().loading(true);
        if (this.Filter().recordsPerPage() == null || this.Filter().recordsPerPage() == 0) {
            this.Filter().recordsPerPage(10);
        }
        this.Filter().tenantId(this.MainModel().TenantId());

        let success: Function = (data: server.filterUsers) => {
            this.MainModel().Message_Hide();
            if (data != null) {
                if (data.actionResponse.result) {
                    this.Filter().Load(data);

                    this.RenderUserTable();

                    this.Filter().loading(false);
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to load user records.");
            }
            this.Filter().loading(false);
        };

        let postFilter: filterUsers = new filterUsers();
        postFilter.Load(JSON.parse(ko.toJSON(this.Filter)));
        postFilter.tenantId(this.MainModel().TenantId());
        postFilter.columns(null);
        postFilter.records(null);
        postFilter.cultureCode(this.MainModel().Culture());

        let jsonData: string = ko.toJSON(postFilter);
        tsUtilities.CookieWrite("saved-filter-users", jsonData);
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUsersFiltered/", jsonData, success);
    }

    /**
     * Event fires when the Dropzone photo upload has completed.
     * @param message {server.booleanResponse} - The response that comes back from the file upload endpoint.
     */
    PhotoUploadComplete(message: any): void {
        this.MainModel().Message_Hide();

        let response: booleanResponse = new booleanResponse();
        response.Load(JSON.parse(message));

        if (response.result()) {
            // Get the user, but don't reload the local User unless it is the current user. Just update the photo property.
            if (this.User().userId() == this.MainModel().User().userId()) {
                this.MainModel().ReloadUser();
            }

            let success: Function = (data: server.user) => {
                if (data != null && data.actionResponse.result) {
                    this.User().photo(data.photo);
                }
            };

            tsUtilities.AjaxData(window.baseURL + "api/Data/GetUser/" + this.User().userId(), null, success);
        } else {
            this.MainModel().Message_Errors(response.messages());
        }
    }
    /**
     * Method handles the callback from the paged recordset control when the page changes, the records per page changes, or when the sort order changes.
     * @param type {string} - The type of change event (count, page, or sort)
     * @param data {any} - The data passed back, which is a number for count and page and a field column id for the sort.
     */
    RecordsetCallbackHandler(type: string, data: any): void {
        console.log("RecordsetCallbackHandler", type, data);
        switch (type) {
            case "count":
                window.usersModel.Filter().recordsPerPage(data);
                window.usersModel.GetUsers();
                break;

            case "page":
                window.usersModel.Filter().page(data);
                window.usersModel.GetUsers();
                break;

            case "sort":
                window.usersModel.UpdateSort(data);
                break;
        }
    }

    /**
     * Called when the Refresh Filter button is clicked.
     */
    RefreshFilter(): void {
        this.SaveFilter();
        this.GetUsers();
    }

    /**
     * Renders the paged recordset view. This happens when the filter loads, but also gets called for certain SignalR events
     * to update users that might be in the current user list.
     */
    RenderUserTable(): void {
        // Load records in the pagedRecordset
        let f: filter = new filter();
        f.Load(JSON.parse(ko.toJSON(this.Filter)));
        f = this.MainModel().UpdatePagedRecordsetColumnIcons(f);

        let records: any = JSON.parse(ko.toJSON(this.Filter().records));

        // Only show photos if this set of data includes at least one photo
        let photoBaseUrl: string = "";

        if (records != null && records.length > 0) {
            let havePhotos: any = ko.utils.arrayFirst(records, function (e) {
                var photo = e["photo"];
                return photo != undefined && photo != null && photo != "";
            });
            photoBaseUrl = havePhotos != null
                ? window.baseURL + "File/View/"
                : "";
        }

        pagedRecordset.Render({
            elementId: "user-records",
            data: JSON.parse(ko.toJSON(f)),
            recordsetCallbackHandler: (type: string, data: any) => { this.RecordsetCallbackHandler(type, data); },
            actionHandlers: [
                {
                    callbackHandler: (user: server.user) => { this.EditUserCallback(user); },
                    actionElement: "<button type='button' class='btn btn-sm btn-primary nowrap'>" + this.MainModel().IconAndText("Edit") + "</button>"
                }
            ],
            recordNavigation: "both",
            photoBaseUrl: photoBaseUrl,
            booleanIcon: this.MainModel().Icon("selected")
        });
    }

    /**
     * Shows the Reset User Password interface allowing Admin users to set a local login password for a given user.
     */
    ResetUserPassword(): void {
        this.NewPassword("");
        this.ResettingUserPassword(true);
        tsUtilities.DelayedFocus("reset-user-password");
    }

    /**
     * Makes the actual endoint call to perform the user password reset.
     */
    ResetUserPasswordUpdate(): void {
        if (!tsUtilities.HasValue(this.NewPassword())) {
            tsUtilities.DelayedFocus("reset-user-password");
            return;
        }

        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();
            this.ResettingUserPassword(false);
            if (data != null) {
                if (data.result) {
                    this.MainModel().Message("{{PasswordReset}}", StyleType.Success, true, true);
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to reset the user's password.");
            }
        };

        this.MainModel().Message_Saving();

        let reset: userPasswordReset = new userPasswordReset();
        reset.userId(this.User().userId());
        reset.tenantId(this.User().tenantId());
        reset.newPassword(this.NewPassword());

        tsUtilities.AjaxData(window.baseURL + "api/Data/ResetUserPassword", ko.toJSON(reset), success);
    }

    /**
     * Saves the current filter as a JSON object in a cookie. The items that aren't needed are nulled out first
     * so that the column data and record data are not stored in the cookie.
     */
    SaveFilter(): void {
        let saveFilter: filterUsers = new filterUsers();
        saveFilter.Load(JSON.parse(ko.toJSON(this.Filter)));
        saveFilter.actionResponse(null);
        saveFilter.columns([]);
        saveFilter.records(null);

        tsUtilities.CookieWrite("saved-filter-users", ko.toJSON(saveFilter));
    }

    /**
     * Saves a user record for the user currently being added or edited.
     */
    SaveUser(): void {
        this.MainModel().Message_Hide();
        let errors: string[] = [];
        let focus: string = "";

        if (!tsUtilities.HasValue(this.User().firstName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("FirstName")));
            if (focus == "") { focus = "edit-user-firstName"; }
        }
        if (!tsUtilities.HasValue(this.User().lastName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("LastName")));
            if (focus == "") { focus = "edit-user-lastName"; }
        }
        if (!tsUtilities.HasValue(this.User().email())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("Email")));
            if (focus == "") { focus = "edit-user-email"; }
        }
        if (!tsUtilities.HasValue(this.User().username())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("Username")));
            if (focus == "") { focus = "edit-user-username"; }
        }

        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
            return;
        }

        this.MainModel().UDFFieldsGetValues("Users", this.User());
        let json: string = ko.toJSON(this.User);

        let success: Function = (data: server.user) => {
            this.MainModel().Message_Hide();
            if (data != null) {
                if (data.actionResponse.result) {
                    this.MainModel().Nav("Users");
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to save this user.");
            }
        };

        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveUser", json, success);
    }

    /**
     * This model subscribes to SignalR updates from the MainModel so that users in the user filter list
     * can be removed or updated when their data changes or they are deleted.
     */
    SignalrUpdate(): void {
        //console.log("In Tenants, SignalR Update", JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate)));
        switch (this.MainModel().SignalRUpdate().updateTypeString().toLowerCase()) {
            case "setting":
                let userId: string = this.MainModel().SignalRUpdate().itemId();

                switch (this.MainModel().SignalRUpdate().message().toLowerCase()) {
                    case "deleteduser":
                        let records: any[] = [];
                        if (this.Filter().records() != null && this.Filter().records().length > 0) {
                            this.Filter().records().forEach(function (e) {
                                if (e["userId"] != userId) {
                                    records.push(e);
                                }
                            });
                        }
                        this.Filter().records(records);
                        this.RenderUserTable();

                        break;

                    case "saveduser":
                        let userData: any = this.MainModel().SignalRUpdate().object();

                        let index: number = -1;
                        let indexItem: number = -1;
                        if (this.Filter().records() != null && this.Filter().records().length > 0) {
                            this.Filter().records().forEach(function (e) {
                                index++;
                                if (e["userId"] == userId) {
                                    indexItem = index;
                                }
                            });
                        }

                        if (indexItem > -1) {
                            this.Filter().records()[indexItem] = JSON.parse(userData);
                            this.RenderUserTable();
                        }
                }

                break;
        }
    }

    /**
     * Starts observing changes to the filter elements to call FilterChanged when selections are changed.
     */
    StartFilterMonitoring(): void {
        // Subscribe to filter changed
        this.Filter().keyword.subscribe(() => { this.FilterChanged(); });
        this.Filter().filterDepartments.subscribe(() => { this.FilterChanged(); });
        this.Filter().enabled.subscribe(() => { this.FilterChanged(); });
        this.Filter().admin.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf01.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf02.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf03.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf04.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf05.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf06.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf07.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf08.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf09.subscribe(() => { this.FilterChanged(); });
        this.Filter().udf10.subscribe(() => { this.FilterChanged(); });
    }

    /**
     * Called when the Show or Hide Filter buttons are clicked.
     */
    ToggleShowFilter(): void {
        if (this.Filter().showFilters()) {
            this.Filter().showFilters(false);
        } else {
            this.Filter().showFilters(true);
        }
        this.SaveFilter();
    }

    /**
     * Unlocks a user account that was locked due to too many failed login attempts.
     */
    UnlockUserAccount(): void {
        let success: Function = (data: server.user) => {
            if (data != null) {
                if (data.actionResponse.result) {
                    this.GetUsers();
                    this.MainModel().Nav("Users");
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to unlock the user account.");
            }
        };

        tsUtilities.AjaxData(window.baseURL + "api/Data/UnlockUserAccount/" + this.User().userId(), null, success);
    }

    /**
     * Handles changing the sort order and updating the filter.
     * @param dataElementName
     */
    UpdateSort(dataElementName: string): void {
        let currentSort: string = this.Filter().sort();
        if (tsUtilities.HasValue(currentSort)) {
            currentSort = currentSort.toLowerCase();
        } else {
            currentSort = "";
        }

        let currentDirection: string = this.Filter().sortOrder();
        if (tsUtilities.HasValue(currentDirection)) {
            currentDirection = currentDirection.toUpperCase();
        }

        if (tsUtilities.HasValue(dataElementName)) {
            if (currentSort.toLowerCase() == dataElementName.toLowerCase()) {
                if (currentDirection == "ASC") {
                    this.Filter().sortOrder("DESC");
                } else {
                    this.Filter().sortOrder("ASC");
                }
            } else {
                this.Filter().sort(dataElementName);
                this.Filter().sortOrder("");
            }
            this.GetUsers();
        }
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged(): void {
        this.Loading(false);

        let allowAccess: boolean = this.MainModel().AdminUser();

        switch (this.MainModel().CurrentView()) {
            case "edituser":
                if (allowAccess) {
                    this.AddToSelectedTeam("");
                    this.EditUser();
                    this.ResettingUserPassword(false);
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;

            case "newuser":
                if (allowAccess) {
                    this.AddToSelectedTeam("");
                    this.AddUser();
                    this.ResettingUserPassword(false);
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;

            case "users":
                if (allowAccess) {
                    this.AddToSelectedTeam("");
                    this.GetSavedFilter();
                    this.ResettingUserPassword(false);
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
        }
    }
}