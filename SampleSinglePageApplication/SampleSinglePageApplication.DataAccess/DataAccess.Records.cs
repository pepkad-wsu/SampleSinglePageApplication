namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public async Task<List<DataObjects.Record>> GetRecords(Guid tenantId)
    {
        List<DataObjects.Record> output = new List<DataObjects.Record>();

        var recs = await data.Records.Where(x => x.TenantId == tenantId).ToListAsync();
        if (recs != null && recs.Any())
        {
            foreach (var rec in recs)
            {
                if (rec != null)
                {
                    output.Add(new DataObjects.Record
                    {
                        RecordId = Guid.NewGuid(),
                        RedcordName = "todo",
                        TenantId = tenantId,
                        RecordBoolean = true,
                        RecordNumber = 1,
                        RecordText = "Text for the record",
                        ActionResponse = GetNewActionResponse(true),
                        UserId = tenantId,
                        Username = "name"
                    });
                }
            }
        }

        return output;
    }
}