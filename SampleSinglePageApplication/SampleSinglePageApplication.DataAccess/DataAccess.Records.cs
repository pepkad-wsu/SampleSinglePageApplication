namespace SampleSinglePageApplication;
public partial class DataAccess
{
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
                        Name = rec.RedcordName,
                        TenantId = rec.TenantId,
                        Boolean = rec.RecordBoolean,
                        Number = rec.RecordNumber,
                        Text = rec.RecordText,
                        UserId = rec.User.UserId,
                        Username = rec.User.Username
                    });
                }
            }
        }

        return output;
    }
}