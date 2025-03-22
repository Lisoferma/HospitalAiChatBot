using PuppeteerSharp;

namespace HospitalAiChatbot.Source.Models
{
    public class RzdMedicineSiteScraper : IHospitalSiteScraper
    {
        private readonly IBrowser _browser;


        public RzdMedicineSiteScraper(IBrowser browser)
        {
            _browser = browser;
        }


        public async Task<string> GetOpeningHoursAsync()
        {
            const string url = "https://chita.rzd-medicine.ru/clinics/chuz-dorozhnaja-klinicheskaja-bolnica-na-st-chita-2-oao-rzhd-poliklinika-1/shema-territorii";
            const string selector = "#__nuxt > div > div > div > div.wrapper > div.page.page-inner > div > div:nth-child(2) > div > div > div > div:nth-child(3) > div.SI_clinicInfo_block-text";

            return await GetTextFromSelector(url, selector);
        }


        public async Task<string> GetReceptionContactsAsync()
        {
            const string url = "https://chita.rzd-medicine.ru/clinics/chuz-dorozhnaja-klinicheskaja-bolnica-na-st-chita-2-oao-rzhd-poliklinika-1/shema-territorii";
            const string selector = "#__nuxt > div > div > div > div.wrapper > div.page.page-inner > div > div:nth-child(2) > div > div > div > div:nth-child(4) > div:nth-child(2) > span > a";

            return await GetTextFromSelector(url, selector);
        }


        private async Task<string> GetTextFromSelector(string url, string selector)
        {
            using var page = await _browser.NewPageAsync();

            await page.GoToAsync(url, WaitUntilNavigation.Load);
            await page.WaitForSelectorAsync(selector);

            string content = await page.EvaluateExpressionAsync<string>(
                $"document.querySelector('{selector}').innerText"
            );

            return content;
        }
    }
}
