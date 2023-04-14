

using SampleSinglePageApplication.EFModels.EFModels;
using System;

namespace SampleSinglePageApplication // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static bool DEBUG = true;

        static async Task Main(string[] args)
        {
            //var daConnection = "Server=(local);Database=SSPA_Flex;Persist Security Info=True;Trusted_Connection=True;MultipleActiveResultSets=True";
            var SSPA_Flex_DaConnection = "Data Source=(local);Initial Catalog=sspa_main;Persist Security Info=True;User ID=sa;Password=saPassword;MultipleActiveResultSets=True;TrustServerCertificate=True";

            var da = new DataAccess(SSPA_Flex_DaConnection, "sqlserver");

            var tenants = await da.GetTenants();

            Console.WriteLine("success: " + tenants.Count());
        }
    }
}
