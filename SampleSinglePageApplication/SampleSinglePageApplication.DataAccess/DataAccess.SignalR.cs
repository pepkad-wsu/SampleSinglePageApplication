namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public async Task SignalRUpdate(DataObjects.SignalRUpdate update)
    {
        var baseURL = await AppSetting<string>("ApplicationURL");
        if (String.IsNullOrEmpty(baseURL)) {
            baseURL = String.Empty;
        }

        if (!baseURL.EndsWith("/")) { baseURL += "/"; }

        var client = new HttpClient();
        //client.BaseAddress = new Uri(baseURL);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        update.UpdateTypeString = SignalRUpdateTypeToString(update.UpdateType);
        if (update.Object != null) {
            update.Object = Utilities.SerializeObjectToJsonCamelCase(update.Object);
        }

        await client.PostAsync(baseURL + "api/Data/SignalRUpdate/",
            new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(update), System.Text.Encoding.UTF8, "application/json"));
    }

    public DataObjects.SignalRUpdateType SignalRUpdateTypeFromString(string updateType)
    {
        DataObjects.SignalRUpdateType output = DataObjects.SignalRUpdateType.Unknown;

        switch (updateType.ToUpper()) {
            case "SETTING":
                output = DataObjects.SignalRUpdateType.Setting;
                break;
            case "THIS":
                output = DataObjects.SignalRUpdateType.This;
                break;
            case "THAT":
                output = DataObjects.SignalRUpdateType.That;
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
            case DataObjects.SignalRUpdateType.This:
                output = "This";
                break;
            case DataObjects.SignalRUpdateType.That:
                output = "That";
                break;
        }

        return output;
    }


}