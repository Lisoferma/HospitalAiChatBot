namespace HospitalAiChatBot.Models;

/// <summary>
///     Предоставляет метод для распознавания речи из аудио.
/// </summary>
public interface ISpeachRecognizer
{
    /// <summary>
    ///     Распознать речь из WAV-файла.
    /// </summary>
    /// <param name="audioData">WAV-файл в виде массива байт.</param>
    /// <returns>Распознанная речь в виде текста. String.Empty если не удалось распознать.</returns>
    string RecognizeWav(byte[] audioData);


    /// <summary>
    ///     Распознать речь из OGG Stream.
    /// </summary>
    /// <param name="oggStream">Входной поток OGG (должен быть читаемым).</param>
    /// <returns>Распознанная речь в виде текста. String.Empty если не удалось распознать.</returns>
    string RecognizeOggStream(Stream oggStream);
}