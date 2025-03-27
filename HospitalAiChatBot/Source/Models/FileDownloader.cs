namespace HospitalAiChatbot.Source.Models
{
    /// <summary>
    /// Содержит метод для скачивания файлов по URL.
    /// </summary>
    public static class FileDownloader
    {
        /// <summary>
        /// Скачать файл по URL-адресу.
        /// </summary>
        /// <param name="url">URL-адрес на файл который нужно скачать.</param>
        /// <returns>Скачанный файл в виде массива байт.</returns>
        public static async Task<byte[]> DownloadFileAsync(string url)
        {
            using HttpClient httpClient = new();

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

            return fileBytes;
        }
    }
}
