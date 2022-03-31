var DepartmentsModel = /** @class */ (function () {
    function DepartmentsModel() {
        var _this = this;
        this.ConfirmDelete = ko.observable("");
        this.Department = ko.observable(new department);
        this.DepartmentGroup = ko.observable(new departmentGroup);
        this.MainModel = ko.observable(window.mainModel);
        this.MainModel().View.subscribe(function () {
            _this.ViewChanged();
        });
    }
    /**
     * Deletes a department record.
     */
    DepartmentsModel.prototype.DeleteDepartment = function () {
        var _this = this;
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    _this.MainModel().Nav("Departments");
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to delete the department.");
            }
        };
        this.MainModel().Message_Deleting();
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteDepartment/" + this.MainModel().Id(), null, success);
    };
    /**
     * Deletes a department group record.
     */
    DepartmentsModel.prototype.DeleteDepartmentGroup = function () {
        var _this = this;
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.result) {
                    _this.MainModel().Nav("DepartmentGroups");
                }
                else {
                    _this.MainModel().Message_Errors(data.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to delete the department group.");
            }
        };
        this.MainModel().Message_Deleting();
        tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteDepartmentGroup/" + this.MainModel().Id(), null, success);
    };
    /**
     * Called when the URL view is "EditDepartment" or "NewDepartment" to load the appropriate values into the Department object and show the edit interface.
     */
    DepartmentsModel.prototype.Edit = function () {
        var _this = this;
        this.MainModel().Message_Hide();
        if (!tsUtilities.HasValue(this.MainModel().Id())) {
            this.Department(new department);
            this.Department().departmentId(this.MainModel().GuidEmpty());
            this.Department().tenantId(this.MainModel().TenantId());
            this.Department().enabled(true);
            tsUtilities.DelayedFocus("edit-department-departmentName");
            return;
        }
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.Department().Load(data);
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to load the department.");
            }
        };
        this.MainModel().Message_Loading();
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetDepartment/" + this.MainModel().Id(), null, success);
    };
    /**
     * Called when the URL view is "EditDepartmentGroup" or "NewDepartmentGroup" to load the appropriate data into the DepartmentGroup object to edit.
     */
    DepartmentsModel.prototype.EditDepartmentGroup = function () {
        var _this = this;
        this.MainModel().Message_Hide();
        if (!tsUtilities.HasValue(this.MainModel().Id())) {
            this.DepartmentGroup(new departmentGroup);
            this.DepartmentGroup().departmentGroupId(this.MainModel().GuidEmpty());
            this.DepartmentGroup().tenantId(this.MainModel().TenantId());
            tsUtilities.DelayedFocus("edit-departmentgroup-departmentGroupName");
            return;
        }
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.DepartmentGroup().Load(data);
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to load the department group.");
            }
        };
        this.MainModel().Message_Loading();
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetDepartmentGroup/" + this.MainModel().Id(), null, success);
    };
    /**
     * Called when the URL view is "Departments" to get the list of departments from the WebAPI endpoint.
     */
    DepartmentsModel.prototype.GetDepartments = function () {
        var _this = this;
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                var d_1 = [];
                if (data != null) {
                    data.forEach(function (e) {
                        var item = new department();
                        item.Load(e);
                        d_1.push(item);
                    });
                }
                _this.MainModel().Tenant().departments(d_1);
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to load the departments.");
            }
        };
        this.MainModel().Message_Loading();
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetDepartments/" + this.MainModel().TenantId(), null, success);
    };
    /**
     * Called when the URL view is "DepartmentGroups" to get the list of department groups from the WebAPI endpoint.
     */
    DepartmentsModel.prototype.GetDepartmentGroups = function () {
        var _this = this;
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                var d_2 = [];
                if (data != null) {
                    data.forEach(function (e) {
                        var item = new departmentGroup();
                        item.Load(e);
                        d_2.push(item);
                    });
                }
                _this.MainModel().Tenant().departmentGroups(d_2);
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to load the department groups.");
            }
        };
        this.MainModel().Message_Loading();
        tsUtilities.AjaxData(window.baseURL + "api/Data/GetDepartmentGroups/" + this.MainModel().TenantId(), null, success);
    };
    /**
     * Saves a department record via the WebAPI endpoint.
     */
    DepartmentsModel.prototype.Save = function () {
        var _this = this;
        this.MainModel().Message_Hide();
        var errors = [];
        var focus = "";
        if (!tsUtilities.HasValue(this.Department().departmentName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("DepartmentName")));
            if (focus == "") {
                focus = "edit-department-departmentName";
            }
        }
        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
            return;
        }
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.MainModel().Nav("Departments");
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to save the department.");
            }
        };
        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveDepartment", ko.toJSON(this.Department), success);
    };
    /**
     * Saves a department group record via the WebAPI endpoint.
     */
    DepartmentsModel.prototype.SaveDepartmentGroup = function () {
        var _this = this;
        this.MainModel().Message_Hide();
        var errors = [];
        var focus = "";
        if (!tsUtilities.HasValue(this.DepartmentGroup().departmentGroupName())) {
            errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("DepartmentGroupName")));
            if (focus == "") {
                focus = "edit-departmentgroup-departmentGroupName";
            }
        }
        if (errors.length > 0) {
            this.MainModel().Message_Errors(errors);
            tsUtilities.DelayedFocus(focus);
            return;
        }
        var success = function (data) {
            _this.MainModel().Message_Hide();
            if (data != null) {
                if (data.actionResponse.result) {
                    _this.MainModel().Nav("DepartmentGroups");
                }
                else {
                    _this.MainModel().Message_Errors(data.actionResponse.messages);
                }
            }
            else {
                _this.MainModel().Message_Error("An unknown error occurred attempting to save the department group.");
            }
        };
        this.MainModel().Message_Saving();
        tsUtilities.AjaxData(window.baseURL + "api/Data/SaveDepartmentGroup", ko.toJSON(this.DepartmentGroup), success);
    };
    /**
     * Called when the view changes in the MainModel to do any necessary work in this viewModel.
     */
    DepartmentsModel.prototype.ViewChanged = function () {
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
    };
    return DepartmentsModel;
}());
//# sourceMappingURL=departments.js.map