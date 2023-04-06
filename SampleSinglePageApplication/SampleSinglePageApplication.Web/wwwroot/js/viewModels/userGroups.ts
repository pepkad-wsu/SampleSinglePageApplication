class UserGroupsModel {
    Confirm: KnockoutObservable<string> = ko.observable("");
    Group: KnockoutObservable<userGroup> = ko.observable(new userGroup);
    Groups: KnockoutObservableArray<userGroup> = ko.observableArray([]);
    Loading: KnockoutObservable<boolean> = ko.observable(false);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
    View: KnockoutObservable<string> = ko.observable("");

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    /**
    * Function placeholder for your Add method
    */
    Add(): void {
        this.Group(new userGroup);
        this.Group().tenantId(this.MainModel().TenantId());
        this.Group().groupId(this.MainModel().GuidEmpty());
        this.Group().enabled(true);

        this.View("add");
        tsUtilities.DelayedFocus("edit-group-name");
    }

    AddUserToGroup(userId: string): void {
        if (this.Group().users() == null) {
            this.Group().users([]);
        }

        if (this.Group().users().indexOf(userId) == -1) {
            this.Group().users.push(userId);
            this.SortUsers();
        }
    }

    AvailableUsers = ko.computed((): user[] => {
        let output: user[] = [];

        if (this.MainModel().Users() != null && this.MainModel().Users().length > 0) {
            let current: string[] = this.Group().users();
            if (current == null) {
                current = [];
            }

            output = ko.utils.arrayFilter(this.MainModel().Users(), function (item) {
                return current.indexOf(item.userId()) == -1;
            });

            let m: MainModel = this.MainModel();
            output = output.sort(function (l, r) {
                return m.UserDisplayName(l.userId()) > m.UserDisplayName(r.userId()) ? 1 : -1;
            });
        }

        return output;
    });

    Delete(): void {
        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.result) {
                    this.MainModel().Nav("UserGroups");
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to delete the user group.");
            }
        };

        this.MainModel().Message_Deleting();
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteUserGroup/" + this.Group().groupId(), null, success);
    }

    /**
    * Function placeholder for your Edit method
    */
    Edit(): void {
        let success: Function = (data: server.userGroup) => {
            //console.log("Group", data);
            if (data != null) {
                if (data.actionResponse.result) {
                    this.Group().Load(data);
                    this.SortUsers();
                    this.Loading(false);
                    tsUtilities.DelayedFocus("edit-group-name");
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to load the user group.");
            }
        };

        this.View("edit");
        this.Loading(true);
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUserGroup/" + this.MainModel().Id(), null, success);
    }

    /**
    * Function placeholder for your Load method that gets called when this view is loaded
    */
    Load(): void {
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

        let success: Function = (data: server.userGroup[]) => {
            DataLoader(data, this.Groups, userGroup);

            this.Loading(false);
        };

        this.Loading(true);
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetUserGroups", null, success);
    }

    RemoveUserFromGroup(userId: string): void {
        this.Group().users.remove(userId);
        this.SortUsers();
    }

    /**
    * Function placeholder for your Save method
    */
    Save(): void {
        if (!tsUtilities.HasValue(this.Group().name())) {
            tsUtilities.DelayedFocus("edit-group-name");
            return;
        }

        let success: Function = (data: server.userGroup) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.actionResponse.result) {
                    this.MainModel().Nav("UserGroups");
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to save the user group.");
            }
        };

        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveUserGroup", ko.toJSON(this.Group), success);
    }

    SortUsers(): void {
        if (this.Group().users() != null && this.Group().users().length > 0) {
            let m: MainModel = this.MainModel();

            this.Group().users.sort(function (l, r) {
                return m.UserDisplayName(l) > m.UserDisplayName(r) ? 1 : -1;
            });
        }
    }

    /**
    * Called when the view changes in the MainModel to do any necessary work in this viewModel.
    */
    ViewChanged() {
        this.Confirm("");
        this.Loading(false);
        this.View("");

        let allowAccess: boolean = this.MainModel().AdminUser() && !this.MainModel().BlockModuleUserGroups();

        switch (this.MainModel().CurrentView()) {
            case "usergroups":
                if (allowAccess) {
                    this.Load();
                } else {
                    this.MainModel().Nav("AccessDenied");
                }
                break;
        }
    }
}