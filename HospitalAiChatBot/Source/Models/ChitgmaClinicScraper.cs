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
        /// <inheritdoc/>
        public static string GetOpeningHours()
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
        public static string GetCallCenterContacts()
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
