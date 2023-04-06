namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public async Task SignalRUpdate(DataObjects.SignalRUpdate update)
    {
        var baseURL = ApplicationURL;
        if (String.IsNullOrEmpty(baseURL)) {
            baseURL = String.Empty;
        }

        if (!baseURL.EndsWith("/")) { baseURL += "/"; }

        HttpClient client = Utilities.GetHttpClient(baseURL);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        update.UpdateTypeString = SignalRUpdateTypeToString(update.UpdateType);
        if (update.Object != null) {
            update.Object = Utilities.SerializeObjectToJsonCamelCase(update.Object);
        }

        string updateData = Newtonsoft.Json.JsonConvert.SerializeObject(update);

        await client.PostAsync(baseURL + "api/Data/SignalRUpdate/",
            new StringContent(updateData, System.Text.Encoding.UTF8, "application/json"));
    }

    public DataObjects.SignalRUpdateType SignalRUpdateTypeFromString(string updateType)
    {
        DataObjects.SignalRUpdateType output = DataObjects.SignalRUpdateType.Unknown;

        switch (updateType.ToUpper()) {
            case "SETTING":
                output = DataObjects.SignalRUpdateType.Setting;
                break;
        }

        return output;
    }

    public string SignalRUpdateTypeToString(DataObjects.SignalRUpdateType updateType)
    {
        string output = String.Empty;

        switch (updateType) {
            case DataObjects.SignalRUpdateType.Setting:
                output = "Setting";
                break;
        }

        return output;
    }
}