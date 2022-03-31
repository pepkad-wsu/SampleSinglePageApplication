namespace SampleSinglePageApplication;
public partial class DataAccess
{
    private void SeedTestData()
    {
        // Make sure the initial admin and default tenants are created.
        var adminTenant = data.Tenants.FirstOrDefault(x => x.TenantId == _guid1);
        if (adminTenant == null) {
            data.Tenants.Add(new Tenant {
                TenantId = _guid1,
                TenantCode = "Admin",
                Enabled = true,
                Name = "Admin"
            });
            data.SaveChanges();
            SeedTestData_CreateDefaultTenantData(_guid1);
        }

        var defaultTenant = data.Tenants.FirstOrDefault(x => x.TenantId == _guid2);
        if (defaultTenant == null) {
            data.Tenants.Add(new Tenant {
                TenantId = _guid2,
                TenantCode = "Tenant1",
                Enabled = true,
                Name = "Tenant1"
            });
            data.SaveChanges();
            SeedTestData_CreateDefaultTenantData(_guid2);
        }

        bool newRecord = false;
        string adminPassword = String.Empty;

        // Make sure there is an Admin record in the admin tenant
        var adminUser = data.Users.FirstOrDefault(x => x.TenantId == _guid1 && x.UserId == _guid1);
        if (adminUser == null) {
            adminUser = new User {
                UserId = _guid1,
                TenantId = _guid1
            };
            newRecord = true;
        }
        adminUser.FirstName = "Admin";
        adminUser.LastName = "User";
        adminUser.Email = "admin@local";
        adminUser.Username = "admin";
        adminUser.EmployeeId = "app.admin";
        adminUser.Enabled = true;
        adminUser.Admin = true;
        if (String.IsNullOrEmpty(adminUser.Password)) {
            adminPassword = "admin";
            adminUser.Password = Encrypt(adminPassword);
        } else {
            adminPassword = Decrypt(adminUser.Password);
        }
        if (newRecord) {
            data.Users.Add(adminUser);
        }
        data.SaveChanges();

        // Next, make sure that an admin user exists in each tenant account using the same password that is set on the admin tenant account.
        var tenants = data.Tenants.Where(x => x.TenantId != _guid1);
        if (tenants != null && tenants.Any()) {
            foreach (var tenant in tenants) {
                newRecord = false;
                var tenantAdmin = data.Users.FirstOrDefault(x => x.TenantId == tenant.TenantId && x.Username == "admin");
                if (tenantAdmin == null) {
                    tenantAdmin = new EFModels.EFModels.User {
                        TenantId = tenant.TenantId,
                        UserId = tenant.TenantId // Set the admin user id in each tenant to the tenant id
                    };
                    newRecord = true;
                }
                tenantAdmin.FirstName = "Admin";
                tenantAdmin.LastName = "User";
                tenantAdmin.Email = "admin@local";
                tenantAdmin.Username = "admin";
                tenantAdmin.EmployeeId = "app.admin";
                tenantAdmin.Enabled = true;
                tenantAdmin.Admin = true;
                tenantAdmin.Password = Encrypt(adminPassword);

                if (newRecord) {
                    data.Users.Add(tenantAdmin);
                }
            }
        }
        data.SaveChanges();
    }

    // Called when a new tenant record is created to add defaults
    private void SeedTestData_CreateDefaultTenantData(Guid TenantId)
    {
        var tenantSettings = GetTenantSettings(TenantId);
        tenantSettings.LoginOptions = new List<string> { "local", "eitSSO" };
        tenantSettings.JasonWebTokenKey = TenantId.ToString().Replace("-", "");
        tenantSettings.AllowUsersToManageAvatars = true;
        tenantSettings.AllowUsersToManageBasicProfileInfo = true;
        tenantSettings.AllowUsersToManageBasicProfileInfoElements = new List<string> { "name", "email", "phone", "employeeid", "title", "department", "location" };
        tenantSettings.RequirePreExistingAccountToLogIn = TenantId == _guid1 ? true : false;

        SaveTenantSettings(TenantId, tenantSettings);

        // For the main test account add some default data
        if(TenantId == _guid2) {
            // Make sure there is at least one department group
            Guid departmentGroupId = Guid.Empty;
            var deptGroups = data.DepartmentGroups.Where(x => x.TenantId == TenantId);
            if(deptGroups != null && deptGroups.Any()) {
                departmentGroupId = deptGroups.First().DepartmentGroupId;
            } else {
                departmentGroupId = Guid.NewGuid();
                data.DepartmentGroups.Add(new DepartmentGroup { 
                    DepartmentGroupId = departmentGroupId,
                    DepartmentGroupName = "EM Area Wide",
                    TenantId = TenantId
                });
                data.SaveChanges();
            }

            // Make sure there is at least one department
            Guid departmentId = Guid.Empty;
            var depts = data.Departments.Where(x => x.TenantId == TenantId);
            if(depts != null && depts.Any()) {
                departmentId = depts.First().DepartmentId;
            } else {
                departmentId = Guid.NewGuid();
                data.Departments.Add(new Department { 
                    DepartmentId = departmentId,
                    TenantId = TenantId,
                    DepartmentName = "Enrollment IT",
                    DepartmentGroupId = departmentGroupId,
                    ActiveDirectoryNames = "{ENROLLMENT INFO TECHNOLOGY}{ENROLLMENT IT}",
                    Enabled = true
                });
                data.SaveChanges();
            }

            var testUser = data.Users.FirstOrDefault(x => x.TenantId == TenantId && x.Username == "test");
            if(testUser == null) {
                // Add a couple test users
                data.Users.Add(new User {
                    UserId = Guid.NewGuid(),
                    TenantId = TenantId,
                    FirstName = "Test",
                    LastName = "User",
                    Email = "testuser@local",
                    Username = "test",
                    Password = Encrypt("test"),
                    Enabled = true,
                    EmployeeId = "000000001",
                    Phone = "509-555-1212",
                    Location = "Works from Home",
                    Title = "A Test Admin User",
                    DepartmentId = departmentId != Guid.Empty ? departmentId : null,
                    Admin = true,
                    LastLogin = DateTime.Now.AddDays(-1)
                });

                data.Users.Add(new User {
                    UserId = Guid.NewGuid(),
                    TenantId = TenantId,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@local",
                    Username = "john.doe",
                    Password = Encrypt("test"),
                    Enabled = true,
                    EmployeeId = "000000002",
                    Phone = "208-555-1212",
                    Location = "Works from Home",
                    Title = "A Regular User That's Enabled",
                    DepartmentId = departmentId != Guid.Empty ? departmentId : null,
                    LastLogin = DateTime.Now.AddDays(-2)
                });

                data.Users.Add(new User {
                    UserId = Guid.NewGuid(),
                    TenantId = TenantId,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@local",
                    Username = "jane.doe",
                    Password = Encrypt("test"),
                    Enabled = false,
                    EmployeeId = "000000003",
                    Phone = "916-555-1212",
                    Location = "Works from Home",
                    Title = "A Regular User That's Disabled",
                    DepartmentId = departmentId != Guid.Empty ? departmentId : null,
                    LastLogin = DateTime.Now.AddDays(-3)
                });

                data.SaveChanges();
            }
        }
    }
}