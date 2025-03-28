using HtmlAgilityPack;
using System.Text;

namespace HospitalAiChatbot.Source.Models
{
    /// <summary>
    /// Скрапер предоставляющий методы для доступа к информации с сайта
    /// <see href="https://clinica.chitgma.ru/">клиники ЧГМА</see>.
    /// </summary>
    public class ChitgmaClinicScraper : IHospitalInformationProvider
    {
        /// <summary>
        /// URL-адрес на файл с ценами на медицинские услуги.  
        /// </summary>
        public const string PRICE_LIST_URL = "https://clinica.chitgma.ru/images/Preyskurant/2025/1DP.pdf";

        /// <summary>
        /// URL-адрес на файл с памятками для пациентов по подготовке к диагностическим исследованиям.
        /// </summary>
        public const string PREPARING_PROCEDURE_URL = "https://clinica.chitgma.ru/images/Preyskurant/2025/1DP.pdf";


        /// <inheritdoc/>
        public string GetOpeningHours()
        {
            const string url = "https://clinica.chitgma.ru/informatsiya-po-otdeleniyu-9";
            const string selector = "//*[@id=\"component2\"]/div[2]/div/p[position() >= 22 and position() <= 24]";

            HtmlWeb web = new();
            var htmlDoc = web.Load(url);

            var nodes = htmlDoc.DocumentNode.SelectNodes(selector);

            StringBuilder result = new();

            foreach (var node in nodes)
            {
                string text = node.InnerText;
                text = CleanNBSP(text);
                text = text.Trim();
                result.AppendLine(text);
            }

            return result.ToString();
        }


        /// <inheritdoc/>
        public string GetCallCenterContacts()
        {
            const string url = "https://clinica.chitgma.ru/contact";
            const string selector = "//*[@id=\"component2\"]/div[2]/div/p[24]/span/text()";

            HtmlWeb web = new();
            var htmlDoc = web.Load(url);

            string result = htmlDoc.DocumentNode.SelectSingleNode(selector).InnerText;
            result = CleanNBSP(result);
            result = result.Replace(":", "").Replace(".", "");
            result = result.Trim();

            return result;
        }


        /// <inheritdoc/>
        public async Task<byte[]> DownloadPriceListAsync()
        {
            return await FileDownloader.DownloadFileAsync(PRICE_LIST_URL);
        }


        /// <inheritdoc/>
        public async Task<byte[]> DownloadPreparingProcedureAsync()
        {
            return await FileDownloader.DownloadFileAsync(PREPARING_PROCEDURE_URL);
        }


        /// <summary>
        /// Очистить текст от HTML символа "&nbsp;" заменив их на одиночный пробел.
        /// </summary>
        /// <param name="text">Текст который нужно очистить.</param>
        /// <returns>Очищенный текст.</returns>
        private static string CleanNBSP(string text)
        {
            text = text.Replace("&nbsp;", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            return text;
        }
    }
}
