namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public async Task<DataObjects.Record> GetRecord(Guid RecordId)
    {
        DataObjects.Record? output = new DataObjects.Record();

        var rec = await data.Records.FirstOrDefaultAsync(x => x.RecordId == RecordId);
        if (rec != null)
        {
            output = new DataObjects.Record
            {
                ActionResponse = GetNewActionResponse(true),
                RecordId = rec.RecordId,
                Name = rec.Name,
                TenantId = rec.TenantId,
                Boolean = rec.Boolean,
                Number = rec.Number,
                Text = rec.Text,
                UserId = rec.User.UserId,
                Username = rec.User.Username
            };
        }

        return output;
    }

    public async Task<List<DataObjects.Record>> GetRecords()
    {
        List<DataObjects.Record> output = new List<DataObjects.Record>();

        var recs = await data.Records.ToListAsync();
        if (recs != null && recs.Any())
        {
            foreach (var rec in recs)
            {
                if (rec != null)
                {
                    output.Add(new DataObjects.Record
                    {
                        ActionResponse = GetNewActionResponse(true),
                        RecordId = rec.RecordId,
                        Name = rec.Name,
                        TenantId = rec.TenantId,
                        Boolean = rec.Boolean,
                        Number = rec.Number,
                        Text = rec.Text,
                        UserId = rec.User.UserId,
                        Username = rec.User.Username
                    });
                }
            }
        }

        return output;
    }
}