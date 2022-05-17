using System;
using HelloWorld;
using HelloWorld.EFModels;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            HelloWorld.DataAccess da = new DataAccess("Server=localhost;Database=HelloWorld;Trusted_Connection=True;MultipleActiveResultSets=true;");

            da.EnsureCreated();

            var before = da.GetSources();

            //var newSource = new DataObjects.Source
            //{
            //    SourceCategory = "a",
            //    SourceId = Guid.Empty,
            //    SourceName = "name",
            //    SourceTemplate = "template",
            //    SourceType = DataObjects.SourceType.FirstType,
            //    //TenantId = new Guid("216672e3-2566-4cd5-aba8-76b568b4cf47"),
            //    TenantId = null,
            //};

            //var result = await da.SaveSource(newSource);
            //var aftersave = da.GetSources();

            //await da.DeleteSource(newSource.SourceId);

            //var afterdelete = da.GetSources();

            //if (!result.ActionResponse.Result)
            //{
            //    foreach (var messages in result.ActionResponse.Messages)
            //    {
            //        Console.WriteLine(messages);
            //    }
            //}

            //Console.WriteLine("Before Count: " + before.Count());
            //Console.WriteLine("After Save Count: " + aftersave.Count());
            //Console.WriteLine("After Delete Count: " + afterdelete.Count());

        }
    }
}