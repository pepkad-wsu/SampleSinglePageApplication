using HelloWorld.EFModels;
using Microsoft.EntityFrameworkCore;

namespace HelloWorld;

public partial class DataAccess
{
    public async Task<DataObjects.Tenant> SaveTenant(DataObjects.Tenant tenant)
    {
        DataObjects.Tenant output = tenant;
        output.ActionResponse = new DataObjects.BooleanResponse();

        bool newRecord = false;
        var rec = await data.Tenants.FirstOrDefaultAsync(x => x.TenantId == tenant.TenantId);
        if (rec == null)
        {
            if (output.TenantId == Guid.Empty)
            {
                newRecord = true;
                output.TenantId = Guid.NewGuid();
                rec = new Tenant();
                rec.TenantId = output.TenantId;
            }
            else
            {
                output.ActionResponse.Messages.Add("Tenant '" + tenant.TenantId.ToString() + "' Not Found");
                return output;
            }
        }

        output.Name = MaxStringLength(output.Name, 200);
        output.TenantCode = MaxStringLength(output.TenantCode, 50);

        rec.Name = output.Name;
        rec.TenantCode = output.TenantCode;

        try
        {
            if (newRecord)
            {
                await data.Tenants.AddAsync(rec);
            }
            await data.SaveChangesAsync();
            output.ActionResponse.Result = true;
            output.ActionResponse.Messages.Add(newRecord ? "New Tenant Added" : "Tenant Updated");
        }
        catch (Exception ex)
        {
            output.ActionResponse.Messages.Add("Error Saving Tenant '" + output.TenantId.ToString() + "' - " + ex.Message);
        }


        return output;
    }
}