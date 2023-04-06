var UserGroupsModel = /** @class */ (function () {
    function UserGroupsModel() {
        var _this = this;
        this.Confirm = ko.observable("");
        this.Group = ko.observable(new userGroup);
        this.Groups = ko.observableArray([]);
        this.Loading = ko.observable(false);
        this.MainModel = ko.observable(window.mainModel);
        this.View = ko.observable("");
        this.AvailableUsers = ko.computed(function () {
            var output = [];
            if (_this.MainModel().Users() != null && _this.MainModel().Users().length > 0) {
                var current_1 = _this.Group().users();
                if (current_1 == null) {
                    current_1 = [];
                }
                output = ko.utils.arrayFilter(_this.MainModel().Users(), function (item) {
                    return current_1.indexOf(item.userId()) == -1;
                });
                var m_1 = _this.MainModel();
                output = output.sort(function (l, r) {
                    return m_1.UserDisplayName(l.userId()) > m_1.UserDisplayName(r.userId()) ? 1 : -1;
                });
            }
            return output;
        });
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    /**
    * Function placeholder for your Add method
    */
    UserGroupsModel.prototype.Add = function () {
        this.Group(new userGroup);
        this.Group().tenantId(this.MainModel().TenantId());
        this.Group().groupId(this.MainModel().GuidEmpty());
        this.Group().enabled(true);
        this.View("add");
        tsUtilities.DelayedFocus("edit-group-name");
    };
    UserGroupsModel.prototype.AddUserToGroup = function (userId) {
        if (this.Group().users() == null) {
            this.Group().users([]);
        }
        if (this.Group().users().indexOf(userId) == -1) {
            this.Group().users.push(userId);
            this.SortUsers();
        }
    };
    UserGroupsModel.prototype.Delete = function () {
        var _this = this;
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    _this.MainModel().Nav("UserGroups");
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to delete the user group.");
            }
        };
        this.MainModel().Message_Deleting();
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteUserGroup/" + this.Group().groupId(), null, success);
    };
    /**
    * Function placeholder for your Edit method
    */
    UserGroupsModel.prototype.Edit = function () {
        var _this = this;
        var success = function (data) {
            //console.log("Group", data);
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.Group().Load(data);
                    _this.SortUsers();
                    _this.Loading(false);
                    tsUtilities.DelayedFocus("edit-group-name");
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to load the user group.");
            }
        };
        this.View("edit");
        this.Loading(true);
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUserGroup/" + this.MainModel().Id(), null, success);
    };
    /**
    * Function placeholder for your Load method that gets called when this view is loaded
    */
    UserGroupsModel.prototype.Load = function () {
        var _this = this;
        if (tsUtilities.HasValue(this.MainModel().Id())) {
            switch (this.MainModel().Id().toLowerCase()) {
                case "add":
                    this.Add();
                    break;
                default:
                    this.Edit();
                    break;
            }
            return;
        }
        var success = function (data) {
            DataLoader(data, _this.Groups, userGroup);
            _this.Loading(false);
        };
        this.Loading(true);
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUserGroups", null, success);
    };
    UserGroupsModel.prototype.RemoveUserFromGroup = function (userId) {
        this.Group().users.remove(userId);
        this.SortUsers();
    };
    /**
    * Function placeholder for your Save method
    */
    UserGroupsModel.prototype.Save = function () {
        var _this = this;
        if (!tsUtilities.HasValue(this.Group().name())) {
            tsUtilities.DelayedFocus("edit-group-name");
            return;
        }
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.MainModel().Nav("UserGroups");
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to save the user group.");
            }
        };
        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveUserGroup", ko.toJSON(this.Group), success);
    };
    UserGroupsModel.prototype.SortUsers = function () {
        if (this.Group().users() != null && this.Group().users().length > 0) {
            var m_2 = this.MainModel();
            this.Group().users.sort(function (l, r) {
                return m_2.UserDisplayName(l) > m_2.UserDisplayName(r) ? 1 : -1;
            });
        }
    };
    /**
    * Called when the view changes in the MainModel to do any necessary work in this viewModel.
    */
    UserGroupsModel.prototype.ViewChanged = function () {
        this.Confirm("");
        this.Loading(false);
        this.View("");
        var allowAccess = this.MainModel().AdminUser() && !this.MainModel().BlockModuleUserGroups();
        switch (this.MainModel().CurrentView()) {
            case "usergroups":
                if (allowAccess) {
                    this.Load();
                }
                else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
        }
    };
    return UserGroupsModel;
}());
//# sourceMappingURL=userGroups.js.map