using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager.DriverConfigs.Impl;

namespace SampleSinglePageApplication.Selenium
{
    public class Tests
    {
        IWebDriver driver;

        [SetUp]
        public void StartBrowser()
        {
            TestContext.Progress.WriteLine("Setup method execution");

            // Methods -geturl, click-
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver();
        }

        [Test]
        public void Test1()
        {
            TestContext.Progress.WriteLine("This is Test 1");

            driver.Url = "https://localhost:7118/";
            //Assert.Pass();
        }


        [TearDown]
        public void CloseBrowser()
        {
            TestContext.Progress.WriteLine("Tear down method");
        }
    }
}