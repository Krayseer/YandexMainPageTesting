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

        // Перевод времени с сервера на реальное время
        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        [SetUp]
        public void Setup()
        {   
            var options = new EdgeOptions();
            options.AddArgument("headless");
            driver = new EdgeDriver(options);
            driver.Navigate().GoToUrl("https://yandex.ru");
        }
        
        // Проверяем, можно ли ввести запрос и перейти на соответствующую страницу
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
            var textInSearchBarAfterFindQueryButton = driver.FindElement(By.XPath("//input")).GetAttribute("value");

            Assert.AreEqual(testText, textInSearchBarAfterFindQueryButton);
        }

        // Проверяем работает ли виртуальная клавиатура, вводим запрос на ней и переходим на следующую страницу
        [Test]
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
            wait.Until(EC.ElementToBeClickable(By.XPath("//button[contains(@class, 'i-bem button_js_inited')]"))).Click();
            Thread.Sleep(1000);
            var textInSearchBarAfterfindQueryButton = driver.FindElement(By.XPath("//input")).GetAttribute("value");

            Assert.AreEqual(testText, textInSearchBarAfterfindQueryButton);
        }

        // Проверяем, корректно ли отображается время и дата на главной странице
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

        // Проверяем, что можно перейти на страницу Яндекс.Маркет
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

        // Проверяем, что можно перейти на страницу Яндекс.Видео
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

        // Проверяем, что можно перейти на страницу Яндекс.Картинки
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

        // Проверяем, что можно перейти на страницу Яндекс.Новости
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

        // Проверяем, что можно перейти на страницу Яндекс.Карты
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

        // Проверяем, что можем авторизироваться на яндекс почте. Сначала вводим несуществующий логин, убеждаемся что не можем войти, далее 
        // вводим существующий логин и переходим на страницу где уже можно вводить пароль
        [Test]
        public void CheckAuthorizationYandexMail()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
            ClosePopUpWindowWhenItExists(wait);

            var incorrectLogin = "thisLoginDoesNotExist";
            var correctLogin = "qwerty";

            wait.Until(EC.ElementToBeClickable(By.XPath("//div[@class='desk-notif-card__login-new-item-title']"))).Click();

            bool CheckingCorrectnessOfElementSearch(string login ,string htmlBlock, string attributeName, string attributeValue)
            {
                driver.FindElement(By.XPath("//input[@name='login']")).SendKeys(login);
                Thread.Sleep(500);
                driver.FindElement(By.XPath("//button[@id='passp:sign-in']")).Click();
                Thread.Sleep(500);
                foreach (var element in driver.FindElements(By.XPath(htmlBlock)))
                    if (element.GetAttribute(attributeName) == attributeValue)
                        return true;
                return false;
            }

            bool wrongLoginWorkedCorrectly = CheckingCorrectnessOfElementSearch(incorrectLogin, "//div", "role", "alert");
            driver.Navigate().Refresh();
            bool correctLoginWorkedCorrectly = CheckingCorrectnessOfElementSearch(correctLogin, "//label", "data-t", "field:label-passwd");
            Assert.IsTrue(wrongLoginWorkedCorrectly && correctLoginWorkedCorrectly);
        }
        
        // Проверяем, можно ли закрыть рекламу. Если у рекламы не имеется значка для закрытия, обновляем страницу
        [Test]
        public void CheckAdvertisementsClosingSuccess()
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, 3));
            ClosePopUpWindowWhenItExists(wait);
            for(int i = 0; i < 20; i++)
            {
                try { wait.Until(EC.ElementToBeClickable(By.XPath("//div[@class='direct-close-block__close-icon']"))).Click(); }
                catch { }
                finally { driver.Navigate().Refresh(); }
            }
        }

        // Провряем актуальность данных о заболевших коронавирусом. Сначала берём значения с главной страницы Яндекса далее сверяем их
        // с данными с официального сайта "стопкоронавирус.рф"
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

        // Предварительно меняем значение времени на некорректное значение. Проверяем, можно ли определить проблему неисправности значения времени на главной странице.
        // Далее из базы берём значение времени и интегрируем на главную страницу.
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
            var responseFromServer = reader.ReadToEnd();
            var timeFromServer = double.Parse(responseFromServer[(responseFromServer.IndexOf(':') + 1)..(responseFromServer.IndexOf(',') - 3)]);

            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("document.querySelector('.datetime__time').textContent = 'Ошибка системы'");
            Thread.Sleep(2000);

            var expectedTime = UnixTimeStampToDateTime(timeFromServer).ToShortTimeString();
            var correctTimeValueOnSite = string.Format("document.querySelector('.datetime__time').textContent = '{0}'", expectedTime);
            js.ExecuteScript(correctTimeValueOnSite);

            Assert.AreEqual(expectedTime, driver.FindElement(By.XPath("//span[@class='datetime__time']")).Text);
        }

        [TearDown]
        public void TearDown() => driver.Quit();
    }
}