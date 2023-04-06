using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SampleSinglePageApplication;
using WebDriverManager.DriverConfigs.Impl;
using static SampleSinglePageApplication.DataObjects;

namespace TouchPoints.Selenium
{

    public class TestAdminTenant
    {
        public static class TestVariables
        {
            public static string TENANT_NAME = "AddTenantTest1_NAME";
            public static string TENANT_CODE = "AddTenantTest1_CODE".ToLower(); // needs to be in lower apparently
            public static string CONFIRM_DELETE_TEXT = "CONFIRM";
            public static string LOCAL_USERNAME = "admin";
            public static string LOCAL_PASSWORD = "admin";
        }

        IWebDriver driver;
        IWebElement viewDiv;

        string url = "https://localhost:7118/";

        string _connectionString = "Data Source=(local);Initial Catalog=SampleSinglePageApplication;Persist Security Info=True;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";
        string _databaseType = "SQLServer";

        DataAccess da;

        bool testPassed;

        bool ignoreDatabase = true;

        [SetUp]
        public void Setup()
        {
            // sspa database
            da = new DataAccess(_connectionString, _databaseType);

            if (url.Contains("localhost")) {
                ignoreDatabase = false;
            }

            // web driver stuff
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver();

            // wait 10 seconds for the page to load. If you don't do this knockout
            // isn't finished making the page by the time we start acting 
            driver.Manage().Timeouts().ImplicitWait = System.TimeSpan.FromSeconds(10);

            driver.Manage().Window.Maximize();
            driver.Url = url;

            IWebElement viewLoginDiv = driver.FindElement(By.Id("view-login"));
            
            if (url.Contains("dev.em.wsu.edu")) {
                // if you are running locally the default settings don't have sso enabled,
                // if you are going to dev then we need to pick the local login
                viewLoginDiv.FindElement(By.Id("local-login-button")).Click();
            }
            
            // enter the username
            viewLoginDiv.FindElement(By.Id("local-username")).SendKeys($"{TestVariables.LOCAL_USERNAME}");
            // enter the password
            viewLoginDiv.FindElement(By.Id("local-password")).SendKeys($"{TestVariables.LOCAL_PASSWORD}");
            // submit the login
            viewLoginDiv.FindElement(By.Id("login-button")).Click();

            // Locate to Tenants Page
            var navItems = driver.FindElements(By.ClassName("nav-item"));
            navItems[1].Click();
            navItems[1].FindElements(By.ClassName("dropdown-item"))[4].Click();

            viewDiv = GetViewDiv();

            testPassed = false;
        }

        public IWebElement GetViewDiv()
        {
            IWebElement output = driver.FindElement(By.Id("view-tenants"));

            return output;
        }


        // local helper function for getting the tenant and returning null if it doesn't exist
        private async Task<DataObjects.Tenant?> GetTestTenant()
        {
            DataObjects.Tenant? output = null;
            DataObjects.Tenant? existing = await da.GetTenantFromCode($"{TestVariables.TENANT_CODE}");
            if (existing != null && existing.ActionResponse != null && existing.ActionResponse.Result) {
                output = existing;
            }
            return output;
        }


        // local helper function for getting the tenant and returning null if it doesn't exist
        private async Task DeleteTestTenant()
        {
            DataObjects.Tenant? existing = await GetTestTenant();
            if (existing != null) {
                await da.DeleteTenant(existing.TenantId);
            }
        }


        /// <summary>
        /// Get the tenant we are trying to test with.  If it does not exist then null is returned. 
        /// </summary>
        /// <returns>Null or Existing DataObject.Tenant</returns>
        public async Task<DataObjects.Tenant?> CreateTestTenant()
        {
            DataObjects.Tenant? output = null;

            var existing = await GetTestTenant();

            if (existing == null) {
                // if it doesn't exist but should, go add it
                var testTenant = new DataObjects.Tenant {
                    Name = $"{TestVariables.TENANT_NAME}",
                    TenantCode = $"{TestVariables.TENANT_CODE}",
                    Enabled = true,
                    TenantSettings = new TenantSettings {
                        AllowUsersToManageAvatars = true,
                        AllowUsersToManageBasicProfileInfo = true,
                        LoginOptions = new List<string> {
                            "local"
                        }
                    }
                };

                await da.SaveTenant(testTenant);

                output = await GetTestTenant();
            }

            return output;
        }

        [Test]
        public async Task AddTenantTest1()
        {
            // debugging local try to clear out the database
            if (url.Contains("localhost")) {
                bool existsBeforeDelete = (await GetTestTenant() != null);
                if (existsBeforeDelete) {
                    // remove the tenant if it exists
                    await DeleteTestTenant();
                    // if we deleted something we need to refresh after deleting
                    await Task.Delay(3000);
                    driver.Navigate().Refresh();
                    await Task.Delay(3000);
                    viewDiv = GetViewDiv();
                }
            }

            DataObjects.Tenant? existing = await GetTestTenant();

            if ( existing == null || ignoreDatabase) {                
                // this test shouldn't be ran if the tenant already exists in the db

                // click the add new tenant button
                viewDiv.FindElement(By.Id("add-new-tenant-button")).Click();
                await Task.Delay(3000);
                // add a name
                viewDiv.FindElement(By.Id("new-tenant-name")).SendKeys($"{TestVariables.TENANT_NAME}");

                // and a code
                viewDiv.FindElement(By.Id("new-tenant-tenantCode")).SendKeys($"{TestVariables.TENANT_CODE}");
                await Task.Delay(3000);
                // and now save it
                viewDiv.FindElement(By.Id("save-new-tenant-button")).Click();

                await Task.Delay(3000);

                existing = await GetTestTenant();
                Assert.IsNotNull(existing, "if the object is null we didn't succesffully add the thing");

                // pass the test if we successfully add the record from the db. 
                if (existing != null) {
                    testPassed = true;
                    if (url.Contains("localhost")) {
                        // if we are targeting localhost then we should try to delete the record
                        try {
                            await Task.Delay(3000);
                            // ok so its there, lets go delete it
                            await DeleteTestTenant();
                            Assert.IsNull(await GetTestTenant(), "after adding we try to delete it, this wasn't deleted.");
                            await Task.Delay(3000);
                        } catch (Exception ex){
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            } else {
                throw new Exception("Tenant already exists, add tenant test would fail if ran.  Ensure the tenant does not exist before this test is ran as it should be deleted at the end of the test. ");
            }

            Assert.IsTrue(testPassed, "the test failed for some reason");
        }

        [Test]
        public async Task DeleteTenantTest1()
        {
            // create the tenant if it does not exist
            bool missingBeforeCreate = (await GetTestTenant() == null);
            if (url.Contains("localhost")) {
                // if we are targeting localhost then we should try to create it
                if (missingBeforeCreate) {
                    await CreateTestTenant();
                    // if we added something we need to refresh after adding
                    await Task.Delay(2000);
                    driver.Navigate().Refresh();
                    await Task.Delay(2000);
                }
            }

            DataObjects.Tenant? existing = await GetTestTenant();            

            if (existing != null) {
                viewDiv = GetViewDiv();

                IWebElement tenantsTable = viewDiv.FindElement(By.Id("tenant-records"));
                List<IWebElement> rows = tenantsTable.FindElements(By.TagName("tr")).ToList(); // Finds the first element by default.
                
                // grab the table headers
                var headers = rows.First().FindElements(By.TagName("th")).Select(o => o.Text).ToList();

                // grab the rows and look for a particular customer code
                foreach(var row in rows.Skip(1)) {
                    var txt = row.Text;

                    var rowtext = row.GetAttribute("innerHtml");
                    Console.WriteLine(rowtext); 
                    var tds = row.FindElements(By.TagName("td")).Select(o => new { Text = o.Text, td = o }).ToList();

                    IWebElement? editButton = null;
                    IWebElement? tenantId = null;
                    IWebElement? code = null;

                    int i = 0;
                    var codeText = "";
                    var tenantIdText = "";

                    foreach (var header in headers) {
                        try {
                            if (string.IsNullOrWhiteSpace(header)) {                                
                                editButton = tds[i].td;
                            }else if (header == "TenantId") {
                                tenantId = tds[i].td;
                                tenantIdText = tds[i].Text;
                            }else if(header == "Code") {
                                code = tds[i].td;
                                codeText = tds[i].Text;
                            }
                    
                            i++;
                        }catch(Exception ex) {
                            Console.WriteLine($"ex: {ex.Message}");
                        }
                    }
                                        
                    if(codeText == TestVariables.TENANT_CODE) {
                        // ok we found the test tenant, lets click the button
                        // 
                        var button = editButton.FindElement(By.TagName("button"));
                        button.Click();
                        break;
                        // we don't need to finish the loop once we find it
                    }
                }

                await Task.Delay(2000);
                // ok we changed page, lets refresh the view div
                viewDiv = GetViewDiv();

                viewDiv.FindElement(By.Id("delete-tenant-button")).Click();
                await Task.Delay(2000);                
                // ok we changed page, lets refresh the view div
                viewDiv = GetViewDiv();
                
                // enter "CONFIRM" into box
                var allButtons = viewDiv.FindElements(By.TagName("button"));
                // now lets just find the button by its text
                foreach(var button in allButtons) {
                    var buttonText = button.Text;
                    if(buttonText == "Confirm Delete Tenant") {
                        // add a name
                        viewDiv.FindElement(By.Id("confirm-delete-tenant")).SendKeys($"{TestVariables.CONFIRM_DELETE_TEXT}");

                        // we found it!
                        await Task.Delay(2000);
                        button.Click();
                        break;
                    }
                }
                
                await Task.Delay(2000);
                existing = await GetTestTenant();

                // pass the test if we successfully remove the record from the db. 
                if (existing == null) {
                    testPassed = true;
                } else {
                    // not null, should be null try to go delete it
                    if (url.Contains("localhost")) {
                        // if we are targeting localhost then we should try to delete the record
                        await DeleteTestTenant();
                    }
                }

            } else {
                throw new Exception("Tenant already exists, add tenant test would fail if ran.  Ensure the tenant does not exist before this test is ran as it should be deleted at the end of the test. ");
            }

            Assert.IsTrue(testPassed);
        }

        [TearDown]
        public void CloseBrowserOnPass()
        {
            da.Dispose();
            // always close the browser
            if (testPassed)
            {
              driver.Close();
            }
        }
    }
}