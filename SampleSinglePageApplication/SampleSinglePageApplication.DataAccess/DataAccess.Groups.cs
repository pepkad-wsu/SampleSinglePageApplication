namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> AddUserToGroup(Guid UserId, Guid GroupId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (UserId == Guid.Empty || GroupId == Guid.Empty) {
            output.Messages.Add("You must pass a valid UserId and GroupId");
            return output;
        }

        var group = await GetUserGroup(GroupId);
        if (group.TenantId == Guid.Empty) {
            output.Messages.Add("Unable to find TenantId for Group '" + GroupId.ToString() + "'");
            return output;
        }

        var rec = await data.UserInGroups.FirstOrDefaultAsync(x => x.UserId == UserId && x.GroupId == GroupId && x.TenantId == group.TenantId);
        if (rec != null) {
            output.Messages.Add("User is Already in the Selected Group");
        } else {
            try {
                await data.UserInGroups.AddAsync(new UserInGroup {
                    UserInGroupId = Guid.NewGuid(),
                    UserId = UserId,
                    TenantId = group.TenantId,
                    GroupId = GroupId
                });

                await data.SaveChangesAsync();
                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Adding User '" + UserId.ToString() + "' to Group '" + GroupId.ToString() + "' - " + ex.Message);
            }
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteUserGroup(Guid GroupId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var rec = await data.UserGroups.FirstOrDefaultAsync(x => x.GroupId == GroupId);
        if (rec != null) {
            try {
                data.UserInGroups.RemoveRange(data.UserInGroups.Where(x => x.GroupId == GroupId));
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Related Data for User Group '" + GroupId.ToString() + "' - " + ex.Message);
                return output;
            }

            try {
                data.UserGroups.Remove(rec);
                await data.SaveChangesAsync();
                output.Result = true;
                output.Messages.Add("User Group Deleted");
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting User Group '" + GroupId.ToString() + "' - " + ex.Message);
            }
        } else {
            output.Messages.Add("User Group '" + GroupId.ToString() + "' No Longer Exists");
        }

        return output;
    }

    public async Task<DataObjects.UserGroup> GetUserGroup(Guid GroupId, bool IncludeUsers = false)
    {
        DataObjects.UserGroup output = new DataObjects.UserGroup();

        var rec = await data.UserGroups
            .Include(x => x.UserInGroups)
            .FirstOrDefaultAsync(x => x.GroupId == GroupId);
        if (rec != null) {
            var settings = DeserializeObject<DataObjects.UserGroupSettings>(rec.Settings);
            if(settings == null) {
                settings = new DataObjects.UserGroupSettings();
            }

            output = new DataObjects.UserGroup {
                ActionResponse = GetNewActionResponse(true),
                GroupId = rec.GroupId,
                TenantId = rec.TenantId,
                Name = rec.Name,
                Enabled = rec.Enabled,
                Settings = settings,
                Users = IncludeUsers && rec.UserInGroups != null && rec.UserInGroups.Any()
                    ? rec.UserInGroups.Select(x => x.UserId).ToList()
                    : null
            };
        } else {
            output.ActionResponse.Messages.Add("Group '" + GroupId.ToString() + "' Not Found");
        }

        return output;
    }

    public async Task<List<DataObjects.UserGroup>> GetUserGroups(Guid TenantId, bool IncludeUsers = false)
    {
        List<DataObjects.UserGroup>? output = new List<DataObjects.UserGroup>();

        var recs = await data.UserGroups.Where(x => x.TenantId == TenantId)
            .Include(x => x.UserInGroups)
            .OrderBy(x => x.Name).ToListAsync();

        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                var settings = DeserializeObject<DataObjects.UserGroupSettings>(rec.Settings);
                if (settings == null) {
                    settings = new DataObjects.UserGroupSettings();
                }

                output.Add(new DataObjects.UserGroup {
                    ActionResponse = GetNewActionResponse(true),
                    GroupId = rec.GroupId,
                    TenantId = rec.TenantId,
                    Name = rec.Name,
                    Enabled = rec.Enabled,
                    Settings = settings,
                    Users = IncludeUsers && rec.UserInGroups != null && rec.UserInGroups.Any()
                    ? rec.UserInGroups.Select(x => x.UserId).ToList()
                    : null
                });
            }
        } else {
            // There are no groups yet for this customer, so add a new group
            var newGroup = new UserGroup {
                GroupId = Guid.NewGuid(),
                TenantId = TenantId,
                Name = "All Users (auto-created)",
                Enabled = true
            };

            await data.UserGroups.AddAsync(newGroup);
            await data.SaveChangesAsync();

            List<Guid>? groupUsers = null;

            // Now, add all users for this tenant into this new group
            var users = await GetUsers(TenantId);
            if (users != null && users.Any()) {
                if (IncludeUsers) {
                    groupUsers = new List<Guid>();
                }

                foreach (var user in users) {
                    if (groupUsers != null) {
                        groupUsers.Add(user.UserId);
                    }

                    await AddUserToGroup(user.UserId, newGroup.GroupId);
                }
            }

            output = new List<DataObjects.UserGroup>();
            output.Add(new DataObjects.UserGroup {
                ActionResponse = GetNewActionResponse(true),
                GroupId = newGroup.GroupId,
                TenantId = newGroup.TenantId,
                Name = newGroup.Name,
                Enabled = newGroup.Enabled,
                Users = groupUsers
            });
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> RemoveUserFromGroup(Guid UserId, Guid GroupId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if(UserId == Guid.Empty || GroupId == Guid.Empty) {
            output.Messages.Add("You must pass a valid UserId and GroupId");
            return output;
        }

        var group = await GetUserGroup(GroupId);
        if(group == null || !group.ActionResponse.Result || group.TenantId == Guid.Empty) {
            output.Messages.Add("Unable to find TenantId for Group '" + GroupId.ToString() + "'");
            return output;
        }

        var rec = await data.UserInGroups.FirstOrDefaultAsync(x => x.UserId == UserId && x.GroupId == GroupId && x.TenantId == group.TenantId);
        if(rec != null) {
            try {
                data.UserInGroups.Remove(rec);
                await data.SaveChangesAsync();
                output.Result = true;
            }catch(Exception ex) {
                output.Messages.Add("Error Removing User '" + UserId.ToString() + "' from Group '" + GroupId.ToString() + "' - " + ex.Message);
            }
        } else {
            output.Messages.Add("User Was Not in the Selected Group");
        }

        return output;
    }

    public async Task<DataObjects.UserGroup> SaveUserGroup(DataObjects.UserGroup Group)
    {
        DataObjects.UserGroup output = Group;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        var rec = await data.UserGroups.FirstOrDefaultAsync(x => x.GroupId == output.GroupId);
        if(rec == null) {
            if(output.GroupId == Guid.Empty) {
                newRecord = true;
                output.GroupId = Guid.NewGuid();
                rec = new UserGroup { 
                    GroupId = output.GroupId,
                    TenantId = output.TenantId
                };
            } else {
                output.ActionResponse.Messages.Add("Group '" + output.GroupId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        try {
            if (String.IsNullOrWhiteSpace(output.Name)) {
                output.Name = "Users";
            }

            rec.Name = output.Name;
            rec.Enabled = output.Enabled;
            rec.Settings = SerializeObject(output.Settings);

            if (newRecord) {
                await data.UserGroups.AddAsync(rec);
            }
            await data.SaveChangesAsync();
            output.ActionResponse.Result = true;
            output.ActionResponse.Messages.Add(newRecord ? "Group Created" : "Group Updated");

            // Save any users for this group.
            // First, find all users to keep.
            List<Guid> usersInGroup = new List<Guid>();
            if(output.Users != null && output.Users.Any()) {
                usersInGroup = output.Users;
            }

            // Remove any records that should no longer be in the group.
            data.UserInGroups.RemoveRange(data.UserInGroups.Where(x => x.GroupId == output.GroupId && !usersInGroup.Contains(x.UserId)));
            await data.SaveChangesAsync();

            // Make sure all users have a valid record.
            foreach(var userId in usersInGroup) {
                var userInGroup = await data.UserInGroups.FirstOrDefaultAsync(x => x.GroupId == output.GroupId && x.UserId == userId);
                if(userInGroup == null) {
                    await data.UserInGroups.AddAsync(new UserInGroup { 
                        UserInGroupId = Guid.NewGuid(),
                        GroupId = output.GroupId,
                        UserId = userId,
                        TenantId = output.TenantId
                    });
                    await data.SaveChangesAsync();
                }
            }

        }catch(Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Group '" + output.GroupId.ToString() + "' - " + ex.Message);
        }

        return output;
    }
}