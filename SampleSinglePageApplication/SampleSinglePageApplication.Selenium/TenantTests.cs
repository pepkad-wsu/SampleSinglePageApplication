using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Threading.Tasks;
using WebDriverManager.DriverConfigs.Impl;

namespace SampleSinglePageApplication.Selenium
{
    public class TenantTests
    {
        IWebDriver driver;
        const string _connectionString = "Data Source=(local);Initial Catalog=SampleSinglePageApplication;Persist Security Info=True;Trusted_Connection=True;MultipleActiveResultSets=True";

        [SetUp]
        public void StartBrowser()
        {
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver();

            /*
             * The driver not being able to find an element will cause the test to fail. However, the following line will allow
             * the driver to keep trying for 5 seconds to see if it can find the element. Time is saved if the element is found
             * before the implicit wait time is over.
             */

            driver.Manage().Timeouts().ImplicitWait = System.TimeSpan.FromSeconds(5);

            driver.Manage().Window.Maximize();
            driver.Url = "https://localhost:7118/";

            driver.FindElement(By.Id("local-username")).SendKeys("admin");
            driver.FindElement(By.Id("local-password")).SendKeys("admin");
            driver.FindElement(By.Id("login-button")).Click();

            // Locate to Tenants Page
            var navItems = driver.FindElements(By.ClassName("nav-item"));
            navItems[3].Click();
            navItems[3].FindElements(By.ClassName("dropdown-item"))[3].Click();
        }

        [Test]
        public async Task LocatorAddTenant()
        {
            bool pass = false;
            string tenantName = "Throwaway Tenant";
            string tenantCode = "Throwaway Tenant";

            // Locate to Add Tenant
            driver.FindElement(By.Id("add-tenant-btn")).Click();

            // Fill out Add Tenant form and click the save button.
            var newTenantForm = driver.FindElement(By.Id("newtenant-form"));
            var formControls = newTenantForm.FindElements(By.ClassName("form-control"));
            formControls[0].SendKeys(tenantName);
            formControls[1].SendKeys(tenantCode);

            var buttons = newTenantForm.FindElements(By.TagName("button"));
            buttons[1].Click();

            // This is a naive test, not sure if this is the best strategy.
            // Now lets iterate through the list of tenants and look for our new tenant.
            // But first add a short delay for knockout to update the page.
            Thread.Sleep(1000);
            var tenantsList = driver.FindElement(By.Id("tenants-list"));
            var tenantRows = tenantsList.FindElements(By.TagName("tr"));
            int newTenantIndex = -1;
            for (int i = 0; i < tenantRows.Count; i++)
            {
                var dataCells = tenantRows[i].FindElements(By.TagName("td"));
                TestContext.Progress.WriteLine("Name: " + dataCells[2].Text + ", Code: " + dataCells[3].Text);
                if (dataCells[2].Text == tenantName && dataCells[3].Text == tenantCode)
                {
                    pass = true;
                    newTenantIndex = i;
                    i = tenantRows.Count;
                }
            }

            if (!pass)
            {
                Assert.Fail();
                return;
            }

            // Now let's delete the new tenant we created.
            tenantRows[newTenantIndex].FindElement(By.TagName("button")).Click();
            driver.FindElement(By.Id("delete-tenant-button")).Click();
            driver.FindElement(By.Id("confirm-delete-tenant")).SendKeys("CONFIRM");
            driver.FindElement(By.Id("confirm-delete-tenant-button")).Click();

            //SampleSinglePageApplication.GlobalSettings.DatabaseConnection = _connectionString;
            //DataAccess da = new DataAccess(_connectionString);
            //await da.DeleteTenant(); // TODO
        }

        [Test]
        public async Task LocatorDeleteTenant()
        {
            string tenantName = "Throwaway Tenant Name";
            string tenantCode = "Throwaway Tenant Code";

            DataObjects.Tenant tenant = new DataObjects.Tenant
            {
                TenantId = System.Guid.NewGuid(),
                Name = tenantName,
                TenantCode = tenantCode
            };

            SampleSinglePageApplication.GlobalSettings.DatabaseConnection = _connectionString;
            DataAccess da = new DataAccess(_connectionString);
            await da.SaveTenant(tenant);
        }

        [TearDown]
        public void CloseBrowserOnPass()
        {
            //driver.Close();
        }
    }
}
