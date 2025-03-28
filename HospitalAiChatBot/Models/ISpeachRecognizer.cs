namespace HospitalAiChatbot.Source.Models
{
    /// <summary>
    /// Предоставляет метод для распознавания речи из аудио.
    /// </summary>
    public interface ISpeachRecognizer
    {
        /// <summary>
        /// Распознать речь из аудио.
        /// </summary>
        /// <param name="audioData">Аудио файл в виде массива байт.</param>
        /// <returns>Распознанная речь в виде текста.</returns>
        string Recognize(byte[] audioData);
    }
}