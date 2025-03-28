using Vosk;

namespace HospitalAiChatBot.Models;

/// <summary>
///     Содержит медоты для распознавания речи из аудио с помощью нейронной сети.
/// </summary>
public class VoskSpeechRecognizer : ISpeachRecognizer
{
    /// <summary>
    ///     Модель для распознавания речи из аудио.
    /// </summary>
    private readonly VoskRecognizer _recognizer;

    /// <summary>
    ///     Частота дискретизации у аудио, которое будет распознаваться.
    /// </summary>
    public readonly float SampleRate;


    /// <summary>
    ///     Инициализировать модель для распознавания речи из аудио.
    /// </summary>
    /// <param name="modelPath">Путь к папке с файлами модели нейросети.</param>
    /// <param name="sampleRate">Частота дискретизации аудио, которое будет распознаваться.</param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public VoskSpeechRecognizer(string modelPath = @"Resources\vosk-model-small-ru-0.22", float sampleRate = 16000.0f)
    {
        if (!Directory.Exists(modelPath))
            throw new DirectoryNotFoundException($"Модель Vosk не найдена по пути: {modelPath}");

        SampleRate = sampleRate;
        Model model = new(modelPath);
        _recognizer = new VoskRecognizer(model, SampleRate);
    }


    /// <summary>
    ///     Распознать речь из WAV-файла
    ///     c частотой дискретизации <see cref="SampleRate" />, моно, 16-bit PCM.
    /// </summary>
    /// <param name="audioData">WAV-файл в виде массива байт.</param>
    /// <returns>Распознанная речь в виде текста.</returns>
    public string Recognize(byte[] audioData)
    {
        if (_recognizer.AcceptWaveform(audioData, audioData.Length))
            return _recognizer.Result();

        return _recognizer.PartialResult();
    }
}