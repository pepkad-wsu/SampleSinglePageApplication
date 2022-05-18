using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThrowawayDb;

namespace HelloWorld.UnitTests
{
    [TestClass]
    public class DataAccessSourcesTests
    {
        // https://khalidabuhakmeh.com/dotnet-database-integration-tests
        // https://blog.theodo.com/2017/05/good-practices-for-testing-database-interactions/

        [TestMethod]
        public async Task SaveSource_SourcePersistence()
        {
            //List<DataObjects.Source> after = new List<DataObjects.Source>();

            HelloWorld.DataAccess da = new DataAccess("Data Source=(local);Initial Catalog=HelloWorld;Persist Security Info=True;Trusted_Connection=True;MultipleActiveResultSets=True");

            var newSource = new DataObjects.Source
            {
                SourceCategory = "a",
                SourceId = Guid.Empty,
                SourceName = "name",
                SourceTemplate = "template",
                SourceType = DataObjects.SourceType.FirstType,
                //TenantId = new Guid("216672e3-2566-4cd5-aba8-76b568b4cf47"),
                TenantId = null,
            };

            await da.SaveSource(newSource);
            var after = await da.GetSources();
            int count = after.Sources.Count;

            Assert.AreEqual(1, count);
        }
    }
}