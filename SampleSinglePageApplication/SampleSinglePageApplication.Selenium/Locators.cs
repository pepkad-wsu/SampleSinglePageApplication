﻿using NUnit.Framework;
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

            driver.Manage().Timeouts().ImplicitWait = System.TimeSpan.FromSeconds(1);

            driver.Manage().Window.Maximize();
            driver.Url = "https://localhost:7118/";
        }

        [Test]
        public void LocatorIdentification()
        {
            //.GetAttribute("id");
            //.Clear();
            //while (!driver.FindElement(By.Id("loaded")).Enabled)
            //{
            //    await Task.Delay(10);
            //}

            driver.FindElement(By.Id("local-username")).SendKeys("admin");
            driver.FindElement(By.Id("local-password")).SendKeys("admin");
            driver.FindElement(By.Id("login-button")).Click();

            Assert.AreEqual(driver.FindElements(By.Id("view-home")).Count, 1);
        }


        [TearDown]
        public void CloseBrowser()
        {

        }
    }
}