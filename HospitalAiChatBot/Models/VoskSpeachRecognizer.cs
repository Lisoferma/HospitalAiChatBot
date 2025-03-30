using HospitalAiChatbot.Models.Services;
using System.Text.Json;
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


    /// <inheritdoc />
    public string RecognizeWav(byte[] audioData)
    {
        string jsonResult;

        if (_recognizer.AcceptWaveform(audioData, audioData.Length))
        {
            jsonResult = _recognizer.Result();
        }           
        else
        {
            jsonResult = _recognizer.PartialResult();
        }

        var jsonDoc = JsonDocument.Parse(jsonResult);
        string recognizedText = jsonDoc.RootElement.GetProperty("text").GetString() ?? String.Empty;

        return recognizedText;
    }


    /// <inheritdoc />
    public string RecognizeOggStream(Stream oggStream)
    {
        byte[] audioData = AudioFormatConverter.ConvertOggStreamToWavBytes(oggStream, (int)SampleRate);
        return RecognizeWav(audioData);
    }
}