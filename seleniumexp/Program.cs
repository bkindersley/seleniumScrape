using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
namespace SeleniumEsperiments
{
    class Program
    {
        static void Main(string[] args)
        {

            string panelText = "Change NSW Driver Licence, NSW Photo Card and vehicle registration contact details";

            IWebDriver driver = new ChromeDriver();
            WebDriverWait w = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(5));
            driver.Navigate().GoToUrl("https://my.service.nsw.gov.au/MyServiceNSW/index#/rms/changeDetails");
            //on page with radio buttons
            w.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.ClassName("panel-group")));
            IWebElement panelGroup = driver.FindElement(By.ClassName("panel-group"));
            IWebElement form = driver.FindElement(By.ClassName("form"));

            IList<IWebElement> panels = new List<IWebElement>();
            int i = 0;
            while (!string.Equals(panels.FirstOrDefault()?.Text, panelText) && i < 10)
            {
                panels = panelGroup.FindElements(By.ClassName("panel"));
                i++;
            }
            Debug.WriteLine(i);
            if (!string.Equals(panels.FirstOrDefault()?.Text, panelText))
            {
                Debug.WriteLine("panel not found");
                return;
            }

            IWebElement panel = panels.First();
            panel.Click();

            IWebElement submitBtn = form.FindElement(By.TagName("input"));
            submitBtn.Click();
            
            w.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("/html/body//p/button")));

            IList<IWebElement> buttons = driver.FindElements(By.XPath("/html/body//p/button"));

            IWebElement guestLoginBtn = null;

            foreach (IWebElement btn in buttons)
            {
                if (string.Equals(btn.Text, "Continue as guest"))
                {
                    guestLoginBtn = btn;
                    break;
                }
            }

            if (guestLoginBtn == null)
            {
                Debug.WriteLine("guest login not found");
                return;
            }

            guestLoginBtn.Click();

            w.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.ClassName("panel")));

            IWebElement textPanel = driver.FindElement(By.ClassName("panel"));

            w.Until(d => textPanel.FindElements(By.XPath(".//form")).Count() > 1);

            IList<IWebElement> forms = textPanel.FindElements(By.XPath(".//form"));

            IWebElement detailsForm = null;

            foreach (IWebElement frm in forms)
            {
                IList<IWebElement> ins = frm.FindElements(By.ClassName("form-control"));
                if (ins.Count() == 3)
                {
                    detailsForm = frm;
                    break;
                }             
            }

            if (detailsForm == null)
            {
                Debug.WriteLine("form not found");
            }

            IWebElement lName = detailsForm.FindElement(By.XPath(".//input[contains(@id, 'lastName')]"));
            IWebElement phIDNum = detailsForm.FindElement(By.XPath(".//input[contains(@id, 'photoIdNumber')]"));
            IWebElement dlNum = detailsForm.FindElement(By.XPath(".//input[contains(@id, 'cardNumber')]"));

            lName.SendKeys("Smith");
            phIDNum.SendKeys("11111111");
            dlNum.SendKeys("11111111");

            IWebElement nextBtn = detailsForm.FindElement(By.XPath(".//input[@value = 'Next']"));

            nextBtn.Click();

            IWebElement notFoundAlert = null;

            try
            {
                w.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.ClassName("alert-dismissible")));
                notFoundAlert = textPanel.FindElement(By.ClassName("alert-dismissible"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
            if (notFoundAlert == null)
            {
                if (string.Equals(driver.Url, "https://my.service.nsw.gov.au/MyServiceNSW/index#/rms/changeDetails/address"))
                {
                    Debug.WriteLine("found match?");

                }
            }
            else if (notFoundAlert.Text.Contains("do not match"))
            {
                Debug.WriteLine("No Match!");
            }
            else
            {
                Debug.WriteLine("Not the expected error!");
            }

            driver.Quit();
        }
    }
}