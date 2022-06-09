using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using EC = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace YandexMainPageTesting
{
    public class TestClass
    {
        private IWebDriver driver;

        // Закрыть всплывающее окно, если оно существует, которое предлагает установить яндекс браузер
        private static void ClosePopUpWindowWhenItExists(WebDriverWait wait)
        {
            try { wait.Until(EC.ElementToBeClickable(By.XPath("//div[@class='modal__close']"))).Click(); }
            catch { }
        }

        // Проверить, перешёл ли пользователь, при нажатии кнопки, на соответственную страницу
        private static bool CheckIfUserWentToPageWhenClickOnButton(IWebDriver driver, string expectedUrlAfterMovingToNewPage)
        {
            return driver.Url.Contains(expectedUrlAfterMovingToNewPage);
        }
        
        // Перейти на следующую вкладку
        private static void SwitchToAnotherTab(IWebDriver driver)
        {
            driver.SwitchTo().Window(driver.WindowHandles[1]);
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        [SetUp]
        public void Setup()
        {   
            var options = new EdgeOptions();
            options.AddArgument("headless");
            driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://yandex.ru");
        }
        
        [Test]
        public void CanUseSearchBar()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);
            var testText = "Some text";
            var searchBar = wait.Until(EC.ElementToBeClickable(By.XPath("//input[contains(@class, 'mini-suggest__input')]")));
            var findQueryButton = wait.Until(EC.ElementToBeClickable(By.XPath("//button[contains(@class, 'i-bem button_js_inited')]")));
            searchBar.SendKeys(testText);
            findQueryButton.Click();
            Thread.Sleep(1000);
            var textInSearchBarAfterfindQueryButton = driver.FindElement(By.XPath("//input")).GetAttribute("value");

            Assert.AreEqual(testText, textInSearchBarAfterfindQueryButton);
        }

        [Test] // добавить другие случаи, включая пробел и пустой ввод
        public void CheckingFunctionalityOfVirtualKeyboard()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);

            wait.Until(EC.ElementToBeClickable(By.XPath("//i[contains(@class, 'keyboard-loader__icon')]"))).Click();
            string? testText = null;
            var commands = new List<string>();
            var letters = new List<string> { "т", "е", "с", "т", "и", "р", "о", "в", "а", "н", "и", "е" };
            letters.ForEach(x =>
            {
                commands.Add("//span[text()='" + x + "']");
                testText += x;
            });
            commands.ForEach(x => wait.Until(EC.ElementToBeClickable(By.XPath(x))).Click());
            var findQueryButton = wait.Until(EC.ElementToBeClickable(By.XPath("//button[contains(@class, 'i-bem button_js_inited')]")));
            findQueryButton.Click();
            Thread.Sleep(1000);
            var textInSearchBarAfterfindQueryButton = driver.FindElement(By.XPath("//input")).GetAttribute("value");

            Assert.AreEqual(testText, textInSearchBarAfterfindQueryButton);
        }

        [Test]
        public void CheckIfDateAndTimeIsCorrect()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);

            var hours = wait.Until(EC.ElementToBeClickable(By.XPath("//span[@class='datetime__hour']"))).Text;
            var minutes = wait.Until(EC.ElementToBeClickable(By.XPath("//span[@class='datetime__min']"))).Text;
            var actualTime = hours + ":" + minutes;

            var day = wait.Until(EC.ElementToBeClickable(By.XPath("//span[@class='datetime__day']"))).Text;
            var month = wait.Until(EC.ElementToBeClickable(By.XPath("//span[@class='datetime__month']"))).Text;
            var actualDate = day + " " + month.Replace(',', ' ').Trim();

            var expectedTime = DateTime.Now.ToShortTimeString();
            expectedTime = expectedTime.Length == 4 ? "0" + expectedTime : expectedTime;
            var expectedDate = DateTime.Now.ToString("dd MMMM");
            expectedDate = expectedDate[0] == '0' ? expectedDate[1..] : expectedDate;

            Assert.AreEqual(expectedTime, actualTime);
            Assert.AreEqual(expectedDate, actualDate);
        }

        [Test]
        public void CheckTransitionToMarketPageOnButtonClick()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);
            wait.Until(EC.ElementToBeClickable(By.XPath("//div[contains(@class, 'services-new__icon services-new__icon_market')]"))).Click();
            var expectedUrlAfterMovingToNewPage = "https://market.yandex.ru";
            SwitchToAnotherTab(driver);

            Assert.IsTrue(CheckIfUserWentToPageWhenClickOnButton(driver, expectedUrlAfterMovingToNewPage));
        }

        [Test]
        public void CheckTransitionToVideoPageOnButtonClick()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);
            wait.Until(EC.ElementToBeClickable(By.XPath("//div[contains(@class, 'services-new__icon services-new__icon_video')]"))).Click();
            var expectedUrlAfterMovingToNewPage = "https://yandex.ru/video";
            SwitchToAnotherTab(driver);

            Assert.IsTrue(CheckIfUserWentToPageWhenClickOnButton(driver, expectedUrlAfterMovingToNewPage));
        }

        [Test]
        public void CheckTransitionToImagesPageOnButtonClick()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);
            wait.Until(EC.ElementToBeClickable(By.XPath("//div[contains(@class, 'services-new__icon services-new__icon_images')]"))).Click();
            var expectedUrlAfterMovingToNewPage = "https://yandex.ru/images";
            SwitchToAnotherTab(driver);

            Assert.IsTrue(CheckIfUserWentToPageWhenClickOnButton(driver, expectedUrlAfterMovingToNewPage));
        }
        
        [Test]
        public void CheckTransitionToNewsPageOnButtonClick()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);
            wait.Until(EC.ElementToBeClickable(By.XPath("//div[contains(@class, 'services-new__icon services-new__icon_news')]"))).Click();
            var expectedUrlAfterMovingToNewPage = "https://yandex.ru/news";
            SwitchToAnotherTab(driver);

            Assert.IsTrue(CheckIfUserWentToPageWhenClickOnButton(driver, expectedUrlAfterMovingToNewPage));
        }
        
        [Test]
        public void CheckTransitionToMapsPageOnButtonClick()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);
            wait.Until(EC.ElementToBeClickable(By.XPath("//div[contains(@class, 'services-new__icon services-new__icon_maps')]"))).Click();
            var expectedUrlAfterMovingToNewPage = "https://yandex.ru/maps";
            SwitchToAnotherTab(driver);

            Assert.IsTrue(CheckIfUserWentToPageWhenClickOnButton(driver, expectedUrlAfterMovingToNewPage));
        }
        
        [Test]
        public void CanLogIntoNonExistentYandexMailAccount()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);

            var testLogin = "thisLoginDoesNotExist";
            wait.Until(EC.ElementToBeClickable(By.XPath("//div[@class='desk-notif-card__login-new-item-title']"))).Click();
            var authorization = driver.FindElement(By.XPath("//input[@name='login']"));
            authorization.SendKeys(testLogin);
            Thread.Sleep(1000);
            driver.FindElement(By.XPath("//button[@id='passp:sign-in']")).Click();
            Thread.Sleep(1000);
            bool isFindAttributeError = false;
            foreach (var element in driver.FindElements(By.XPath("//div")))
                if (element.GetAttribute("role") == "alert")
                {
                    isFindAttributeError = true;
                    break;
                }
            Assert.IsTrue(isFindAttributeError);
        }
        
        [Test] // доделать
        public void CheckAdvertisementsClosingSuccess()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);

            for(var i = 0; i < 10; i++)
            {
                try
                {
                    wait.Until(EC.ElementToBeClickable(By.XPath("//button[@class='direct-close-block__close-button']"))).Click();
                    wait.Until(EC.ElementToBeClickable(By.XPath("//span[@data-id='21-4']"))).Click();
                    wait.Until(EC.ElementToBeClickable(By.XPath("//span[@data-id='3-2']"))).Click();
                    driver.Navigate().Refresh();
                }
                catch
                {
                    try
                    {
                        wait.Until(EC.ElementToBeClickable(By.XPath("//span[@data-id='3-2']"))).Click();
                    }
                    catch
                    {
                        driver.Navigate().Refresh();
                    }
                }
            }
        }

        [Test]
        public void CheckCorrectnessOfValuesNumberOfSick()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);

            var categorySelectionPoint = driver.FindElements(By.XPath("//div[@class='rotate-items__dot rotate-items__dot_type_anchor']"));
            var actualValues = driver.FindElements(By.XPath("//span[@class='desk-notif-card__covid-histogram-desc-count ']"));
            var actualValuesList = new List<string>();
            for (int i = 0; i < categorySelectionPoint.Count - 1; i++)
            {
                categorySelectionPoint[i].Click();
                actualValuesList.Add(actualValues[i].Text.Replace("+", ""));
                Thread.Sleep(500);
            }

            driver.Navigate().GoToUrl("https://xn--80aesfpebagmfblc0a.xn--p1ai/?");
            var expectedValuesFromOfficialSite = driver.FindElements(By.XPath("//div[@class='cv-countdown__item-value _accent']"));
            var expectedValuesList = new List<string>();
            foreach (var value in expectedValuesFromOfficialSite) 
                expectedValuesList.Add(value.Text.Replace(" ", ""));
            expectedValuesList.Reverse();

            Assert.AreEqual(expectedValuesList, actualValuesList);
        }
        
        [Test]
        public void FixTimeDisplayErrorOnWebsite()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);
            var queryUrl = "https://yandex.ru/time/sync.json?geo=213%2C202%2C10393%2C10636%2C10145&lang=ru&ncrnd=0.7915699997502998";
            var request = WebRequest.Create(queryUrl);
            var response = request.GetResponse();
            using Stream dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("document.querySelector('.datetime__time').textContent = 'Ошибка системы'");
            Thread.Sleep(2000);

            var expectedTime = UnixTimeStampToDateTime(double.Parse(responseFromServer[8..18])).ToShortTimeString();
            var correctTimeValueOnSite = string.Format("document.querySelector('.datetime__time').textContent = '{0}'", expectedTime);
            js.ExecuteScript(correctTimeValueOnSite);
        }

        [TearDown]
        public void TearDown() => driver.Quit();
    }
}