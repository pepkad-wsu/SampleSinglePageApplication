using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
            TestContext.Progress.WriteLine("Am I here?");
            string dropdownText = driver.FindElement(By.Id("navBarToggler")).FindElement(By.CssSelector("a[class='dropdown-item'][3]/i/span[1]")).Text;
            TestContext.Progress.WriteLine(dropdownText);
        }

        [TearDown]
        public void CloseBrowserOnPass()
        {
            //driver.Close();
        }
    }
}
