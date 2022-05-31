using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading.Tasks;
using WebDriverManager.DriverConfigs.Impl;

namespace SampleSinglePageApplication.Selenium
{
    public class Locators
    {
        // Xpath, Css, id, classname, name, tagname

        IWebDriver driver;

        [SetUp]
        public void StartBrowser()
        {
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver();

            //driver.Manage().Timeouts().ImplicitWait = System.TimeSpan.FromSeconds(0.25);

            driver.Manage().Window.Maximize();
            driver.Url = "https://localhost:7118/";
        }

        [Test]
        public async Task LocatorIdentification()
        {
            
            while (!driver.FindElement(By.Id("loaded")).Enabled)
            {
                TestContext.Progress.WriteLine("Not working yet");
                await Task.Delay(10);
            }

            driver.FindElement(By.Id("local-username")).SendKeys("admin");
            driver.FindElement(By.Id("local-password")).SendKeys("admin");
            //driver.FindElement(By.Id("local-username")).Clear(); // clear the text in the textbox.

            // css selector & xpath
            //  tagname[attribute = 'value']
        }


        [TearDown]
        public void CloseBrowser()
        {

        }
    }
}
