namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public async Task<List<DataObjects.Record>> GetRecords(Guid tenantId)
    {
        List<DataObjects.Record> output = new List<DataObjects.Record>();

        output.Add(new DataObjects.Record
        {
            RecordId = Guid.NewGuid(),
            RedcordName = "todo",
            TenantId = tenantId
        });

        return output;
    }
}