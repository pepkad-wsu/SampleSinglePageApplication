namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteDepartment(Guid DepartmentId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var rec = await data.Departments.FirstOrDefaultAsync(x => x.DepartmentId == DepartmentId);
        if (rec == null) {
            output.Messages.Add("Error Deleting Department " + DepartmentId.ToString() + " - Record No Longer Exists");
        } else {
            // First, delete related data
            try {
                var recs = await data.Users.Where(x => x.DepartmentId == DepartmentId).ToListAsync();
                if(recs != null && recs.Any()) {
                    foreach(var record in recs) {
                        record.DepartmentId = null;
                    }
                    await data.SaveChangesAsync();
                }
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Related Department Records for Department " + DepartmentId.ToString() + " - " + ex.Message);
                return output;
            }

            Guid tenantId = GuidOrEmpty(rec.TenantId);

            data.Departments.Remove(rec);
            try {
                await data.SaveChangesAsync();
                output.Result = true;

                if (tenantId != Guid.Empty) {
                    ClearTenantCache(tenantId);
                }

                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = tenantId,
                    ItemId = DepartmentId.ToString(),
                    UpdateType = DataObjects.SignalRUpdateType.Setting,
                    Message = "DeletedDepartment"
                });
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Department " + DepartmentId.ToString() + " - " + ex.Message);
            }
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteDepartmentGroup(Guid DepartmentGroupId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var rec = await data.DepartmentGroups.FirstOrDefaultAsync(x => x.DepartmentGroupId == DepartmentGroupId);
        if (rec == null) {
            output.Messages.Add("Error Deleting DepartmentGroup " + DepartmentGroupId.ToString() + " - Record No Longer Exists");
        } else {
            // First, delete related data
            try {
                if (_inMemoryDatabase) {
                    var recs = await data.Departments.Where(x => x.DepartmentGroupId == DepartmentGroupId).ToListAsync();
                    if(recs != null) {
                        foreach(var r in recs) {
                            r.DepartmentGroupId = null;
                        }
                        await data.SaveChangesAsync();
                    }
                } else {
                    await data.Database.ExecuteSqlRawAsync("UPDATE Departments SET DepartmentGroupId=NULL WHERE DepartmentGroupId={0}", DepartmentGroupId);
                }
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Related Department Records for DepartmentGroup " + DepartmentGroupId.ToString() + " - " + ex.Message);
                return output;
            }

            Guid tenantId = GuidOrEmpty(rec.TenantId);

            data.DepartmentGroups.Remove(rec);
            try {
                await data.SaveChangesAsync();
                output.Result = true;

                if (tenantId != Guid.Empty) {
                    ClearTenantCache(tenantId);
                }

                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = tenantId,
                    ItemId = DepartmentGroupId.ToString(),
                    UpdateType = DataObjects.SignalRUpdateType.Setting,
                    Message = "DeletedDepartmentGroup"
                });
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting DepartmentGroup " + DepartmentGroupId.ToString() + " - " + ex.Message);
            }
        }

        return output;
    }

    public async Task<Guid> DepartmentIdFromActiveDirectoryName(Guid TenantId, string? Department)
    {
        Guid output = Guid.Empty;

        if (!String.IsNullOrEmpty(Department)) {
            var rec = await data.Departments.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.ActiveDirectoryNames != null && x.ActiveDirectoryNames.ToLower().Contains("{" + Department.ToLower() + "}"));
            if (rec != null) {
                output = rec.DepartmentId;
            }
        }

        return output;
    }

    public async Task<DataObjects.Department> GetDepartment(Guid DepartmentId)
    {
        DataObjects.Department output = new DataObjects.Department();

        var rec = await data.Departments.FirstOrDefaultAsync(x => x.DepartmentId == DepartmentId);
        if (rec == null) {
            output.ActionResponse.Messages.Add("Department " + DepartmentId.ToString() + " Not Found");
        } else {
            output.ActionResponse.Result = true;
            output.ActiveDirectoryNames = rec.ActiveDirectoryNames;
            output.DepartmentId = DepartmentId;
            output.TenantId = GuidOrEmpty(rec.TenantId);
            output.DepartmentName = rec.DepartmentName;
            output.Enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false;
            output.DepartmentGroupId = rec.DepartmentGroupId;
        }

        return output;
    }

    public async Task<DataObjects.DepartmentGroup> GetDepartmentGroup(Guid DepartmentGroupId)
    {
        DataObjects.DepartmentGroup output = new DataObjects.DepartmentGroup();

        var rec = await data.DepartmentGroups.FirstOrDefaultAsync(x => x.DepartmentGroupId == DepartmentGroupId);
        if(rec != null) {
            output = new DataObjects.DepartmentGroup { 
                ActionResponse = GetNewActionResponse(true),
                DepartmentGroupId = rec.DepartmentGroupId,
                DepartmentGroupName = rec.DepartmentGroupName,
                TenantId = GuidOrEmpty(rec.TenantId)
            };
        } else {
            output.ActionResponse = GetNewActionResponse(false, "Department Group '" + DepartmentGroupId.ToString() + "' Not Found");
        }

        return output;
    }

    public async Task<List<DataObjects.DepartmentGroup>> GetDepartmentGroups(Guid TenantId)
    {
        List<DataObjects.DepartmentGroup> output = new List<DataObjects.DepartmentGroup>();

        var recs = await data.DepartmentGroups.Where(x => x.TenantId == TenantId).OrderBy(x => x.DepartmentGroupName).ToListAsync();
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                output.Add(new DataObjects.DepartmentGroup {
                    ActionResponse = GetNewActionResponse(true),
                    DepartmentGroupId = rec.DepartmentGroupId,
                    TenantId = TenantId,
                    DepartmentGroupName = rec.DepartmentGroupName
                });
            }
        }

        return output;
    }

    public string GetDepartmentName(Guid TenantId, Guid DepartmentId)
    {
        string output = String.Empty;

        var rec = data.Departments.FirstOrDefault(x => x.TenantId == TenantId && x.DepartmentId == DepartmentId);
        if(rec != null && !String.IsNullOrEmpty(rec.DepartmentName)) {
            output = rec.DepartmentName;
        }

        return output;
    }

    public async Task<List<DataObjects.Department>> GetDepartments(Guid TenantId)
    {
        List<DataObjects.Department> output = new List<DataObjects.Department>();

        var recs = await data.Departments.Where(x => x.TenantId == TenantId).OrderBy(x => x.DepartmentName).ToListAsync();
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                output.Add(new DataObjects.Department {
                    ActionResponse = GetNewActionResponse(true),
                    ActiveDirectoryNames = rec.ActiveDirectoryNames,
                    DepartmentId = rec.DepartmentId,
                    TenantId = GuidOrEmpty(rec.TenantId),
                    DepartmentName = rec.DepartmentName,
                    Enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false,
                    DepartmentGroupId = rec.DepartmentGroupId
                });
            }
        }

        return output;
    }

    public async Task<DataObjects.Department> SaveDepartment(DataObjects.Department department)
    {
        department.ActionResponse = GetNewActionResponse();

        if (GuidOrEmpty(department.TenantId) != Guid.Empty) {
            bool newRecord = false;
            var rec = await data.Departments.FirstOrDefaultAsync(x => x.DepartmentId == department.DepartmentId);
            if (rec == null) {
                if (department.DepartmentId == Guid.Empty) {
                    rec = new Department();
                    department.DepartmentId = Guid.NewGuid();
                    rec.DepartmentId = department.DepartmentId;
                    newRecord = true;
                } else {
                    department.ActionResponse.Messages.Add("Error Saving Department " + department.DepartmentId.ToString() + " - Record No Longer Exists");
                    return department;
                }
            }

            rec.TenantId = GuidOrEmpty(department.TenantId);
            rec.DepartmentName = department.DepartmentName;
            rec.ActiveDirectoryNames = department.ActiveDirectoryNames;
            rec.Enabled = department.Enabled;
            rec.DepartmentGroupId = department.DepartmentGroupId;

            try {
                if (newRecord) {
                    data.Departments.Add(rec);
                }
                await data.SaveChangesAsync();
                department.ActionResponse.Result = true;
                ClearTenantCache(department.TenantId);

                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = department.TenantId,
                    ItemId = department.DepartmentId.ToString(),
                    UpdateType = DataObjects.SignalRUpdateType.Setting,
                    Message = "SavedDepartment",
                    Object = department
                });
            } catch (Exception ex) {
                department.ActionResponse.Messages.Add("Error Saving Department " + department.DepartmentId.ToString() + " - " + ex.Message);
            }
        } else {
            department.ActionResponse.Messages.Add("Invalid DepartmentId");
        }

        return department;
    }

    public async Task<DataObjects.DepartmentGroup> SaveDepartmentGroup(DataObjects.DepartmentGroup departmentGroup)
    {
        departmentGroup.ActionResponse = GetNewActionResponse();

        if (GuidOrEmpty(departmentGroup.TenantId) != Guid.Empty) {
            bool newRecord = false;
            var rec = await data.DepartmentGroups.FirstOrDefaultAsync(x => x.DepartmentGroupId == departmentGroup.DepartmentGroupId);
            if (rec == null) {
                if (departmentGroup.DepartmentGroupId == Guid.Empty) {
                    rec = new DepartmentGroup();
                    departmentGroup.DepartmentGroupId = Guid.NewGuid();
                    rec.DepartmentGroupId = departmentGroup.DepartmentGroupId;
                    rec.TenantId = departmentGroup.TenantId;
                    newRecord = true;
                } else {
                    departmentGroup.ActionResponse.Messages.Add("Error Saving DepartmentGroup " + departmentGroup.DepartmentGroupId.ToString() + " - Record No Longer Exists");
                    return departmentGroup;
                }
            }

            rec.DepartmentGroupName = departmentGroup.DepartmentGroupName;
            try {
                if (newRecord) {
                    data.DepartmentGroups.Add(rec);
                }
                await data.SaveChangesAsync();
                departmentGroup.ActionResponse.Result = true;
                ClearTenantCache(departmentGroup.TenantId);

                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = departmentGroup.TenantId,
                    ItemId = departmentGroup.DepartmentGroupId.ToString(),
                    UpdateType = DataObjects.SignalRUpdateType.Setting,
                    Message = "SavedDepartmentGroup",
                    Object = departmentGroup
                });
            } catch (Exception ex) {
                departmentGroup.ActionResponse.Messages.Add("Error Saving DepartmentGroup " + departmentGroup.DepartmentGroupId.ToString() + " - " + ex.Message);
            }
        } else {
            departmentGroup.ActionResponse.Messages.Add("Invalid DepartmentId");
        }

        return departmentGroup;
    }

    public async Task<List<DataObjects.Department>> SaveDepartments(List<DataObjects.Department> departments)
    {
        List<DataObjects.Department> output = new List<DataObjects.Department>();
        foreach (var department in departments) {
            var saved = await SaveDepartment(department);
            output.Add(saved);
        }
        return output;
    }


}