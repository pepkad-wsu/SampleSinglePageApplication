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
                    });
                }
            }
        }

        return output;
    }

    public async Task<DataObjects.Record> SaveRecord(DataObjects.Record record)
    {
        DataObjects.Record output = record;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        var rec = await data.Records.FirstOrDefaultAsync(x => x.RecordId == record.RecordId);
        if (rec == null)
        {
            if (output.RecordId == Guid.Empty)
            {
                newRecord = true;
                output.RecordId = Guid.NewGuid();
                rec = new Record();
                rec.RecordId = output.RecordId;
            }
            else
            {
                output.ActionResponse.Messages.Add("Record '" + record.RecordId.ToString() + "' Not Found");
                return output;
            }
        }

        output.Name = MaxStringLength(output.Name, 200);
        //output.TenantCode = MaxStringLength(output.TenantCode, 50); CLEANUP

        rec.Name = output.Name;
        rec.Number = output.Number;
        rec.Text = output.Text;

        try
        {
            if (newRecord)
            {
                await data.Records.AddAsync(rec);
            }
            await data.SaveChangesAsync();
            output.ActionResponse.Result = true;
            output.ActionResponse.Messages.Add(newRecord ? "New Record Added" : "Record Updated");

            // CLEANUP
            //if (newRecord)
            //{
            //    SeedTestData_CreateDefaultTenantData(output.RecordId);
            //}
            //else
            //{
            //    SaveTenantSettings(output.TenantId, output.TenantSettings);
            //}

            //ClearTenantCache(output.TenantId);

            //await SignalRUpdate(new DataObjects.SignalRUpdate
            //{
            //    TenantId = output.TenantId,
            //    UpdateType = DataObjects.SignalRUpdateType.Setting,
            //    Message = "TenantSaved",
            //    Object = output
            //});
        }
        catch (Exception ex)
        {
            output.ActionResponse.Messages.Add("Error Saving Record '" + output.RecordId.ToString() + "' - " + ex.Message);
        }

        return output;
    }
}