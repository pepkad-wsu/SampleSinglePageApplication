namespace SampleSinglePageApplication;
public class BioDemoClient
{
    private string _username;
    private string _password;
    private string _endpoint;

    public BioDemoClient(string username, string password, string endpoint)
    {
        _username = username;
        _password = password;
        _endpoint = endpoint;

        if (String.IsNullOrEmpty(_username)) {
            throw new NullReferenceException("Username is Required");
        }
        if (String.IsNullOrEmpty(_password)) {
            throw new NullReferenceException("Password is Required");
        }
        if (String.IsNullOrEmpty(_endpoint)) {
            throw new NullReferenceException("Endpoint is Required");
        }
    }

    public BioDemoUserResponse GetBioDemoByWsuId(int wsuId)
    {
        BioDemoUserResponse output = new BioDemoUserResponse();

        return output;
    }

    public class BioDemoUserResponse
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? NID { get; set; }
        public int WsuId { get; set; }
        public string? OfficeEmail { get; set; }
        public string? PreferredEmail { get; set; }
        public string? PreferredPhone { get; set; }
    }
}