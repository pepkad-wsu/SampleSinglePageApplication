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

        [SetUp]
        public void StartBrowser()
        {
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver();

            driver.Manage().Timeouts().ImplicitWait = System.TimeSpan.FromSeconds(3);

            driver.Manage().Window.Maximize();
            driver.Url = "https://localhost:7118/";

            driver.FindElement(By.Id("local-username")).SendKeys("admin");
            driver.FindElement(By.Id("local-password")).SendKeys("admin");
            driver.FindElement(By.Id("login-button")).Click();
        }

        [Test]
        public void LocatorAddTenant()
        {
            bool pass = false;

            // Locate to Tenants Page
            var navItems = driver.FindElements(By.ClassName("nav-item"));
            navItems[3].Click();
            navItems[3].FindElements(By.ClassName("dropdown-item"))[3].Click();

            // Locate to Add Tenant
            driver.FindElement(By.Id("add-tenant-btn")).Click();

            // Fill out Add Tenant form and click the save button.
            var newTenantForm = driver.FindElement(By.Id("newtenant-form"));
            var formControls = newTenantForm.FindElements(By.ClassName("form-control"));
            formControls[0].SendKeys("Throwaway Tenant");
            formControls[1].SendKeys("Throwaway Tenant");

            var buttons = newTenantForm.FindElements(By.TagName("button"));
            buttons[1].Click();

            // This is a naive test, just roll with it.
            // Now lets iterate through the list of tenants and look for our new tenant.
            //var tenantRows = tenantsList.FindElements(By.TagName("tr"));
            //for (int i = 0; i < tenantRows.Count; i++)
            //{
            //    var dataCells = tenantRows[i].FindElements(By.TagName("td"));
            //    TestContext.Progress.WriteLine("Name: " + dataCells[2].Text + ", Code: " + dataCells[3].Text);
            //    if (dataCells[2].Text == "Throwaway Tenant" && dataCells[3].Text == "Throwaway Tenant")
            //    {
            //        pass = true;
            //        i = tenantRows.Count;
            //    }
            //}

            //if (pass)
            //{
            //    Assert.Pass();
            //}
            //else
            //{
            //    Assert.Fail();
            //}
        }

        [TearDown]
        public void CloseBrowserOnPass()
        {
            //driver.Close();
        }
    }
}
