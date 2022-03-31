class DepartmentsModel {
    ConfirmDelete: KnockoutObservable<string> = ko.observable("");
    Department: KnockoutObservable<department> = ko.observable(new department);
    DepartmentGroup: KnockoutObservable<departmentGroup> = ko.observable(new departmentGroup);
    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);

    constructor() {
        this.MainModel().View.subscribe(() => {
            this.ViewChanged();
        });
    }

    /**
     * Deletes a department record.
     */
    DeleteDepartment(): void {
        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.result) {
                    this.MainModel().Nav("Departments");
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to delete the department.");
            }
        };

        this.MainModel().Message_Deleting();
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteDepartment/" + this.MainModel().Id(), null, success);
    }

    /**
     * Deletes a department group record.
     */
    DeleteDepartmentGroup(): void {
        let success: Function = (data: server.booleanResponse) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.result) {
                    this.MainModel().Nav("DepartmentGroups");
                } else {
                    this.MainModel().Message_Errors(data.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to delete the department group.");
            }
        };

        this.MainModel().Message_Deleting();
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteDepartmentGroup/" + this.MainModel().Id(), null, success);
    }

    /**
     * Called when the URL view is "EditDepartment" or "NewDepartment" to load the appropriate values into the Department object and show the edit interface.
     */
    Edit(): void {
        this.MainModel().Message_Hide();

        if (!tsUtilities.HasValue(this.MainModel().Id())) {
            this.Department(new department);
            this.Department().departmentId(this.MainModel().GuidEmpty());
            this.Department().tenantId(this.MainModel().TenantId());
            this.Department().enabled(true);
            tsUtilities.DelayedFocus("edit-department-departmentName");
            return;
        }

        let success: Function = (data: server.department) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.actionResponse.result) {
                    this.Department().Load(data);
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to load the department.");
            }
        };

        this.MainModel().Message_Loading();
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetDepartment/" + this.MainModel().Id(), null, success);
    }

    /**
     * Called when the URL view is "EditDepartmentGroup" or "NewDepartmentGroup" to load the appropriate data into the DepartmentGroup object to edit.
     */
    EditDepartmentGroup(): void {
        this.MainModel().Message_Hide();

        if (!tsUtilities.HasValue(this.MainModel().Id())) {
            this.DepartmentGroup(new departmentGroup);
            this.DepartmentGroup().departmentGroupId(this.MainModel().GuidEmpty());
            this.DepartmentGroup().tenantId(this.MainModel().TenantId());
            tsUtilities.DelayedFocus("edit-departmentgroup-departmentGroupName");
            return;
        }

        let success: Function = (data: server.departmentGroup) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.actionResponse.result) {
                    this.DepartmentGroup().Load(data);
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to load the department group.");
            }
        };

        this.MainModel().Message_Loading();
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetDepartmentGroup/" + this.MainModel().Id(), null, success);
    }

    /**
     * Called when the URL view is "Departments" to get the list of departments from the WebAPI endpoint.
     */
    GetDepartments(): void {
        let success: Function = (data: server.department[]) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                let d: department[] = [];
                if (data != null) {
                    data.forEach(function (e) {
                        let item: department = new department();
                        item.Load(e);
                        d.push(item);
                    });
                }
                this.MainModel().Tenant().departments(d);

            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to load the departments.");
            }
        };

        this.MainModel().Message_Loading();
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetDepartments/" + this.MainModel().TenantId(), null, success);
    }

    /**
     * Called when the URL view is "DepartmentGroups" to get the list of department groups from the WebAPI endpoint.
     */
    GetDepartmentGroups(): void {
        let success: Function = (data: server.departmentGroup[]) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                let d: departmentGroup[] = [];
                if (data != null) {
                    data.forEach(function (e) {
                        let item: departmentGroup = new departmentGroup();
                        item.Load(e);
                        d.push(item);
                    });
                }
                this.MainModel().Tenant().departmentGroups(d);

            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to load the department groups.");
            }
        };

        this.MainModel().Message_Loading();
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetDepartmentGroups/" + this.MainModel().TenantId(), null, success);
    }

    /**
     * Saves a department record via the WebAPI endpoint.
     */
    Save(): void {
        this.MainModel().Message_Hide();
        let errors: string[] = [];
        let focus: string = "";

        if (!tsUtilities.HasValue(this.Department().departmentName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("DepartmentName")));
            if (focus == "") { focus = "edit-department-departmentName"; }
        }

        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
            return;
        }


        let success: Function = (data: server.department) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.actionResponse.result) {
                    this.MainModel().Nav("Departments");
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to save the department.");
            }
        };

        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveDepartment", ko.toJSON(this.Department), success);
    }

    /**
     * Saves a department group record via the WebAPI endpoint.
     */
    SaveDepartmentGroup(): void {
        this.MainModel().Message_Hide();
        let errors: string[] = [];
        let focus: string = "";

        if (!tsUtilities.HasValue(this.DepartmentGroup().departmentGroupName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("DepartmentGroupName")));
            if (focus == "") { focus = "edit-departmentgroup-departmentGroupName"; }
        }

        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
            return;
        }

        let success: Function = (data: server.department) => {
            this.MainModel().Message_Hide();

            if (data != null) {
                if (data.actionResponse.result) {
                    this.MainModel().Nav("DepartmentGroups");
                } else {
                    this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            } else {
                this.MainModel().Message_Error("An unknown error occurred attempting to save the department group.");
            }
        };

        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveDepartmentGroup", ko.toJSON(this.DepartmentGroup), success);
    }

    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    ViewChanged(): void {
        switch (this.MainModel().CurrentView()) {
            case "departmentgroups":
                this.GetDepartmentGroups();
                break;

            case "departments":
                this.GetDepartments();
                break;

            case "editdepartment":
            case "newdepartment":
                this.Edit();
                break;

            case "editdepartmentgroup":
            case "newdepartmentgroup":
                this.EditDepartmentGroup();
                break;
        }
    }
}