namespace sampleSinglePageApplicationDataBindingAutos {
    export function ApplyKnockoutViewModelBindingsAuto() {
	window.departmentsModelAuto = new DepartmentsModelAuto();
	ko.applyBindings(window.departmentsModelAuto, document.getElementById('view-departments-auto'));

	window.departmentGroupsModelAuto = new DepartmentGroupsModelAuto();
	ko.applyBindings(window.departmentGroupsModelAuto, document.getElementById('view-departmentgroups-auto'));

	window.fileStoragesModelAuto = new FileStoragesModelAuto();
	ko.applyBindings(window.fileStoragesModelAuto, document.getElementById('view-filestorages-auto'));

	window.settingsModelAuto = new SettingsModelAuto();
	ko.applyBindings(window.settingsModelAuto, document.getElementById('view-settings-auto'));

	window.tenantsModelAuto = new TenantsModelAuto();
	ko.applyBindings(window.tenantsModelAuto, document.getElementById('view-tenants-auto'));

	window.usersModelAuto = new UsersModelAuto();
	ko.applyBindings(window.usersModelAuto, document.getElementById('view-users-auto'));

	window.userGroupsModelAuto = new UserGroupsModelAuto();
	ko.applyBindings(window.userGroupsModelAuto, document.getElementById('view-usergroups-auto'));

	}
}
