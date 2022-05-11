var UsersModel = /** @class */ (function () {
    function UsersModel() {
        var _this = this;
        this.AddToSelectedTeam = ko.observable("");
        this.ConfirmDelete = ko.observable("");
        this.Filter = ko.observable(new filterUsers);
        this.Loading = ko.observable(false);
        this.MainModel = ko.observable(window.mainModel);
        this.NewPassword = ko.observable("");
        this.ResettingUserPassword = ko.observable(false);
        this.User = ko.observable(new user);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
        this.MainModel().SignalRUpdate.subscribe(function () {
            _this.SignalrUpdate();
        });
        setTimeout("setupUserPhotoDropZone()", 0);
        setTimeout(function () { return _this.StartFilterMonitoring(); }, 1000);
    }
    /**
     * Method fires when the URL action is "NewUser"
     */
    UsersModel.prototype.AddUser = function () {
        this.User(new user);
        this.User().userId(this.MainModel().GuidEmpty());
        this.User().tenantId(this.MainModel().TenantId());
        this.MainModel().UDFFieldsRender("edit-user-udf-fields", "Users", JSON.parse(ko.toJSON(this.User)));
        tsUtilities.DelayedFocus("edit-user-firstName");
    };
    /**
     * Clears the values for the user search filter.
     */
    UsersModel.prototype.ClearFilter = function () {
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
        this.GetUsers();
    };
    /**
     * Deletes a user.
     */
    UsersModel.prototype.DeleteUser = function () {
        var _this = this;
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    _this.MainModel().Nav("Users");
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to delete the user.");
            }
        };
        this.MainModel().Message_Deleting();
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteUser/" + this.MainModel().Id(), null, success);
    };
    /**
     * Deletes a user profile photo.
     */
    UsersModel.prototype.DeleteUserPhoto = function () {
        var _this = this;
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    _this.User().photo(null);
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to delete the user photo.");
            }
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteUserPhoto/" + this.User().userId(), null, success);
    };
    /**
     * Method fires when the URL action is "EditUser"
     */
    UsersModel.prototype.EditUser = function () {
        var _this = this;
        this.MainModel().Message_Hide();
        var userId = this.MainModel().Id();
        this.User(new user);
        this.User().userId(null);
        setTimeout("resetUserPhotoDropZone()", 0);
        if (tsUtilities.HasValue(userId)) {
            var success = function (data) {
                if (data != null) {
                    if (data.actionResponse.result) {
                        _this.User().Load(data);
                        tsUtilities.DelayedFocus("edit-user-firstName");
                        _this.MainModel().UDFFieldsRender("edit-user-udf-fields", "Users", JSON.parse(ko.toJSON(_this.User)));
                        _this.Loading(false);
                    }
                    else {
                        _this.MainModel().Message_Errors(data.actionResponse.messages);
                    }
                }
                else {
                    _this.MainModel().Message_Error("An unknown error occurred attempting to load the user record.");
                }
            };
            this.Loading(true);
            tsUtilities.AjaxData(window.baseURL + "api/data/GetUser/" + userId, null, success);
        }
        else {
            this.MainModel().Message_Error("No valid UserId received.");
        }
    };
    /**
     * The callback method used by the paged recordset control to handle the action on the record.
     * @param record {server.user} - The object being passed is a JSON object, not an observable.
     */
    UsersModel.prototype.EditUserCallback = function (record) {
        if (record != undefined && record != null && tsUtilities.HasValue(record.userId)) {
            this.MainModel().Nav("EditUser", record.userId);
        }
    };
    /**
     * Called when the user filter changes to reload user records, unless the filter is changing because
     * records are being reloaded.
     */
    UsersModel.prototype.FilterChanged = function () {
        if (!this.Filter().loading()) {
            this.GetUsers();
        }
    };
    /**
     * Loads the saved filter that is stored in a cookie as a JSON object.
     */
    UsersModel.prototype.GetSavedFilter = function () {
        this.Filter(new filterUsers);
        this.Filter().tenantId(this.MainModel().TenantId());
        var savedFilter = tsUtilities.CookieRead("saved-filter-users");
        if (tsUtilities.HasValue(savedFilter)) {
            this.Filter().Load(JSON.parse(savedFilter));
            this.StartFilterMonitoring();
        }
        this.GetUsers();
    };
    /**
     * Called when the filter changes or when the page loads to get the users matching the current filter.
     */
    UsersModel.prototype.GetUsers = function () {
        var _this = this;
        // Load the filter
        this.Filter().loading(true);
        if (this.Filter().recordsPerPage() == null || this.Filter().recordsPerPage() == 0) {
            this.Filter().recordsPerPage(10);
        }
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.Filter().Load(data);
                    _this.RenderUserTable();
                    _this.Filter().loading(false);
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to load user records.");
            }
            _this.Filter().loading(false);
        };
        var postFilter = new filterUsers();
        postFilter.Load(JSON.parse(ko.toJSON(this.Filter)));
        postFilter.columns(null);
        postFilter.records(null);
        var jsonData = ko.toJSON(postFilter);
        tsUtilities.CookieWrite("saved-filter-users", jsonData);
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUsersFiltered/", jsonData, success);
    };
    /**
     * Event fires when the Dropzone photo upload has completed.
     * @param message {server.booleanResponse} - The response that comes back from the file upload endpoint.
     */
    UsersModel.prototype.PhotoUploadComplete = function (message) {
        var _this = this;
        this.MainModel().Message_Hide();
        var response = new booleanResponse();
        response.Load(JSON.parse(message));
        if (response.result()) {
            // Get the user, but don't reload the local User unless it is the current user. Just update the photo property.
            if (this.User().userId() == this.MainModel().User().userId()) {
                this.MainModel().ReloadUser();
            }
            var success = function (data) {
                if (data != null && data.actionResponse.result) {
                    _this.User().photo(data.photo);
                }
            };
            tsUtilities.AjaxData(window.baseURL + "api/Data/GetUser/" + this.User().userId(), null, success);
        }
        else {
            this.MainModel().Message_Errors(response.messages());
        }
    };
    /**
     * Method handles the callback from the paged recordset control when the page changes, the records per page changes, or when the sort order changes.
     * @param type {string} - The type of change event (count, page, or sort)
     * @param data {any} - The data passed back, which is a number for count and page and a field column id for the sort.
     */
    UsersModel.prototype.RecordsetCallbackHandler = function (type, data) {
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
    };
    /**
     * Called when the Refresh Filter button is clicked.
     */
    UsersModel.prototype.RefreshFilter = function () {
        this.SaveFilter();
        this.GetUsers();
    };
    /**
     * Renders the paged recordset view. This happens when the filter loads, but also gets called for certain SignalR events
     * to update users that might be in the current user list.
     */
    UsersModel.prototype.RenderUserTable = function () {
        var _this = this;
        // Load records in the pagedRecordset
        var f = new filter();
        f.Load(JSON.parse(ko.toJSON(this.Filter)));
        var records = JSON.parse(ko.toJSON(this.Filter().records));
        // Only show photos if this set of data includes at least one photo
        var photoBaseUrl = "";
        if (records != null && records.length > 0) {
            var havePhotos = ko.utils.arrayFirst(records, function (e) {
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
            recordsetCallbackHandler: function (type, data) { _this.RecordsetCallbackHandler(type, data); },
            actionHandlers: [
                {
                    callbackHandler: function (user) { _this.EditUserCallback(user); },
                    actionElement: "<button type='button' class='btn btn-sm btn-primary nowrap'>" + this.MainModel().IconAndText("Edit") + "</button>"
                }
            ],
            recordNavigation: "both",
            photoBaseUrl: photoBaseUrl,
            booleanIcon: this.MainModel().Icon("selected")
        });
    };
    /**
     * Shows the Reset User Password interface allowing Admin users to set a local login password for a given user.
     */
    UsersModel.prototype.ResetUserPassword = function () {
        this.NewPassword("");
        this.ResettingUserPassword(true);
        tsUtilities.DelayedFocus("reset-user-password");
    };
    /**
     * Makes the actual endoint call to perform the user password reset.
     */
    UsersModel.prototype.ResetUserPasswordUpdate = function () {
        var _this = this;
        if (!tsUtilities.HasValue(this.NewPassword())) {
            tsUtilities.DelayedFocus("reset-user-password");
            return;
        }
        var success = function (data) {
            _this.MainModel().Message_Hide();
            _this.ResettingUserPassword(false);
            if (data != null) {
                if (data.result) {
                    _this.MainModel().Message("{{PasswordReset}}", StyleType.Success, true, true);
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to reset the user's password.");
            }
        };
        this.MainModel().Message_Saving();
        var reset = new userPasswordReset();
        reset.userId(this.User().userId());
        reset.tenantId(this.User().tenantId());
        reset.newPassword(this.NewPassword());
        tsUtilities.AjaxData(window.baseURL + "api/Data/ResetUserPassword", ko.toJSON(reset), success);
    };
    /**
     * Saves the current filter as a JSON object in a cookie. The items that aren't needed are nulled out first
     * so that the column data and record data are not stored in the cookie.
     */
    UsersModel.prototype.SaveFilter = function () {
        var saveFilter = new filterUsers();
        saveFilter.Load(JSON.parse(ko.toJSON(this.Filter)));
        saveFilter.actionResponse(null);
        saveFilter.columns([]);
        saveFilter.records(null);
        tsUtilities.CookieWrite("saved-filter-users", ko.toJSON(saveFilter));
    };
    /**
     * Saves a user record for the user currently being added or edited.
     */
    UsersModel.prototype.SaveUser = function () {
        var _this = this;
        this.MainModel().Message_Hide();
        var errors = [];
        var focus = "";
        if (!tsUtilities.HasValue(this.User().firstName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("FirstName")));
            if (focus == "") {
                focus = "edit-user-firstName";
            }
        }
        if (!tsUtilities.HasValue(this.User().lastName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("LastName")));
            if (focus == "") {
                focus = "edit-user-lastName";
            }
        }
        if (!tsUtilities.HasValue(this.User().email())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("Email")));
            if (focus == "") {
                focus = "edit-user-email";
            }
        }
        if (!tsUtilities.HasValue(this.User().username())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("Username")));
            if (focus == "") {
                focus = "edit-user-username";
            }
        }
        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
            return;
        }
        this.MainModel().UDFFieldsGetValues("Users", this.User());
        var json = ko.toJSON(this.User);
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.MainModel().Nav("Users");
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to save this user.");
            }
        };
        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveUser", json, success);
    };
    /**
     * This model subscribes to SignalR updates from the MainModel so that users in the user filter list
     * can be removed or updated when their data changes or they are deleted.
     */
    UsersModel.prototype.SignalrUpdate = function () {
        //console.log("In Tenants, SignalR Update", JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate)));
        switch (this.MainModel().SignalRUpdate().updateTypeString().toLowerCase()) {
            case "setting":
                var userId_1 = this.MainModel().SignalRUpdate().itemId();
                switch (this.MainModel().SignalRUpdate().message().toLowerCase()) {
                    case "deleteduser":
                        var records_1 = [];
                        if (this.Filter().records() != null && this.Filter().records().length > 0) {
                            this.Filter().records().forEach(function (e) {
                                if (e["userId"] != userId_1) {
                                    records_1.push(e);
                                }
                            });
                        }
                        this.Filter().records(records_1);
                        this.RenderUserTable();
                        break;
                    case "saveduser":
                        var userData = this.MainModel().SignalRUpdate().object();
                        var index_1 = -1;
                        var indexItem_1 = -1;
                        if (this.Filter().records() != null && this.Filter().records().length > 0) {
                            this.Filter().records().forEach(function (e) {
                                index_1++;
                                if (e["userId"] == userId_1) {
                                    indexItem_1 = index_1;
                                }
                            });
                        }
                        if (indexItem_1 > -1) {
                            this.Filter().records()[indexItem_1] = JSON.parse(userData);
                            this.RenderUserTable();
                        }
                }
                break;
        }
    };
    /**
     * Starts observing changes to the filter elements to call FilterChanged when selections are changed.
     */
    UsersModel.prototype.StartFilterMonitoring = function () {
        var _this = this;
        // Subscribe to filter changed
        this.Filter().keyword.subscribe(function () { _this.FilterChanged(); });
        this.Filter().filterDepartments.subscribe(function () { _this.FilterChanged(); });
        this.Filter().enabled.subscribe(function () { _this.FilterChanged(); });
        this.Filter().admin.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf01.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf02.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf03.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf04.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf05.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf06.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf07.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf08.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf09.subscribe(function () { _this.FilterChanged(); });
        this.Filter().udf10.subscribe(function () { _this.FilterChanged(); });
    };
    /**
     * Called when the Show or Hide Filter buttons are clicked.
     */
    UsersModel.prototype.ToggleShowFilter = function () {
        if (this.Filter().showFilters()) {
            this.Filter().showFilters(false);
        }
        else {
            this.Filter().showFilters(true);
        }
        this.SaveFilter();
    };
    /**
     * Unlocks a user account that was locked due to too many failed login attempts.
     */
    UsersModel.prototype.UnlockUserAccount = function () {
        var _this = this;
        var success = function (data) {
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.GetUsers();
                    _this.MainModel().Nav("Users");
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to unlock the user account.");
            }
        };
        tsUtilities.AjaxData(window.baseURL + "api/Data/UnlockUserAccount/" + this.User().userId(), null, success);
    };
    /**
     * Handles changing the sort order and updating the filter.
     * @param dataElementName
     */
    UsersModel.prototype.UpdateSort = function (dataElementName) {
        var currentSort = this.Filter().sort();
        if (tsUtilities.HasValue(currentSort)) {
            currentSort = currentSort.toLowerCase();
        }
        else {
            currentSort = "";
        }
        var currentDirection = this.Filter().sortOrder();
        if (tsUtilities.HasValue(currentDirection)) {
            currentDirection = currentDirection.toUpperCase();
        }
        if (tsUtilities.HasValue(dataElementName)) {
            if (currentSort.toLowerCase() == dataElementName.toLowerCase()) {
                if (currentDirection == "ASC") {
                    this.Filter().sortOrder("DESC");
                }
                else {
                    this.Filter().sortOrder("ASC");
                }
            }
            else {
                this.Filter().sort(dataElementName);
                this.Filter().sortOrder("");
            }
            this.GetUsers();
        }
    };
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    UsersModel.prototype.ViewChanged = function () {
        this.Loading(false);
        switch (this.MainModel().CurrentView()) {
            case "edituser":
                this.AddToSelectedTeam("");
                this.EditUser();
                this.ResettingUserPassword(false);
                break;
            case "newuser":
                this.AddToSelectedTeam("");
                this.AddUser();
                this.ResettingUserPassword(false);
                break;
            case "users":
                this.AddToSelectedTeam("");
                this.GetSavedFilter();
                this.ResettingUserPassword(false);
                break;
        }
    };
    return UsersModel;
}());
//# sourceMappingURL=users.js.map