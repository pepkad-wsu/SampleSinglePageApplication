using HelloWorld.EFModels;
using Microsoft.EntityFrameworkCore;

namespace HelloWorld;

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteSource (Guid SourceId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        try
        {
            var rec = data.Sources.FirstOrDefault(x => x.SourceId == SourceId);
            if(rec != null)
            {
                data.Sources.Remove(rec);
                await data.SaveChangesAsync();
                output.Result = true;
            }
        }
        catch (Exception ex)
        {
            output.Messages.Add("An error occurred attempting to delete the source '" + SourceId.ToString() + "' - " + ex.Message);
        }

        return output;
    }


    public async Task<DataObjects.Source> GetSource(Guid sourceId, Guid? tenantId = null)
    {
        DataObjects.Source output = new DataObjects.Source();

        DataObjects.GetSourcesResult result = await GetSources(tenantId, new List<Guid> { sourceId });

        if (result != null && result.ActionResponse.Result)
        {
            var source = result.Sources.SingleOrDefault();
            if (source != null) { 
                output = source; 
                output.ActionResponse.Result = true;
            }
            else
            {
                output.ActionResponse.Messages.Add("Lookup successful, single or default failed.");
            }
        }
        else
        {
            output.ActionResponse.Messages.Add("Lookup failed.");
        }

        return output;
    }

    public async Task<DataObjects.GetSourcesResult> GetSources(Guid? tenantId = null, List<Guid> SourceIds = null)
    {
        DataObjects.GetSourcesResult output = new DataObjects.GetSourcesResult();

        // get the sql queuery for sources
        IQueryable<EFModels.Source> sourceList = data.Sources.Include(o => o.Tenant);

        // First filter the query before executing
        if (tenantId != null)
        {
            sourceList = sourceList.Where(x => x.TenantId == tenantId);
        }
        // filter by source ids also
        if (SourceIds != null)
        {
            sourceList = sourceList.Where(x => SourceIds.Contains(x.SourceId));
        }


        // execute the sql statement and map it into data objects.
        output.Sources = (await sourceList.ToListAsync()).Select(o => new DataObjects.Source
        {
            SourceCategory = o.SourceCategory,
            SourceId = o.SourceId,
            TenantId = o.TenantId,
            SourceName = o.SourceName,
            SourceTemplate = o.SourceTemplate,
            SourceType = SourceTypeFromString("" + o.SourceType),
        }).ToList();

        // make a list of the tenant ids we need
        var tenantIds = output.Sources.Select(o => o.TenantId).Distinct().ToList();

        // make a sql query filtering on only the tenants we need
        var tenants = await data.Tenants.Where(o => tenantIds.Any(id => id == o.TenantId)).ToListAsync();

        foreach(var source in output.Sources)
        {
            var newTenant = tenants.Select(o => new DataObjects.Tenant
            {
                TenantId = o.TenantId,
                Enabled = o.Enabled,
                Name = o.Name,
                TenantCode = o.TenantCode
            }).SingleOrDefault(o => o.TenantId == source.TenantId);
            source.Tenant = newTenant;
        }


        return output;
    }

    public async Task<DataObjects.Source> SaveSource(DataObjects.Source source)
    {
        DataObjects.Source output = source;
        output.ActionResponse = new DataObjects.BooleanResponse();

        bool newRecord = false;
        var rec = await data.Sources.FirstOrDefaultAsync(x => x.SourceId == source.SourceId);
        if (rec == null)
        {
            if (output.SourceId == Guid.Empty)
            {
                newRecord = true;
                output.SourceId = Guid.NewGuid();
                rec = new Source();
                rec.SourceId = output.SourceId;
            }
            else
            {
                output.ActionResponse.Messages.Add("Source '" + source.SourceId.ToString() + "' Not Found");
                return output;
            }
        }

        output.SourceCategory = MaxStringLength(output.SourceCategory, 200);
        output.SourceName = MaxStringLength(output.SourceName, 200);

        rec.SourceName = output.SourceName;
        rec.SourceCategory = output.SourceCategory;
        rec.SourceType = SourceTypeToString(output.SourceType);
        rec.SourceTemplate = output.SourceTemplate;
        rec.TenantId = output.TenantId;

        try
        {
            if (newRecord)
            {
                await data.Sources.AddAsync(rec);
            }
            await data.SaveChangesAsync();
            output.ActionResponse.Result = true;
            output.ActionResponse.Messages.Add(newRecord ? "New Source Added" : "Source Updated");
        }
        catch (Exception ex)
        {
            output.ActionResponse.Messages.Add("Error Saving Source '" + output.SourceId.ToString() + "' - " + ex.Message);
        }


        return output;
    }

    private DataObjects.SourceType SourceTypeFromString(string knownCallerType)
    {
        DataObjects.SourceType output = DataObjects.SourceType.FirstType;

        if (!String.IsNullOrWhiteSpace(knownCallerType))
        {
            switch (knownCallerType.ToUpper())
            {
                case "FIRSTTYPE":
                    output = DataObjects.SourceType.FirstType;
                    break;

                case "SECONDTYPE":
                    output = DataObjects.SourceType.SecondType;
                    break;

                case "THIRDTYPE":
                    output = DataObjects.SourceType.ThirdType;
                    break;
            }
        }

        return output;
    }

    private string SourceTypeToString(DataObjects.SourceType knownCallerType)
    {
        string output = String.Empty;

        switch (knownCallerType)
        {
            case DataObjects.SourceType.FirstType:
                output = "FirstType";
                break;

            case DataObjects.SourceType.SecondType:
                output = "SecondType";
                break;

            case DataObjects.SourceType.ThirdType:
                output = "ThirdType";
                break;
        }

        return output;
    }
}