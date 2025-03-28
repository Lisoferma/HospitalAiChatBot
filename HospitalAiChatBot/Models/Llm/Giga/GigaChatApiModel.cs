// ReSharper disable InvalidXmlDocComment

namespace HospitalAiChatBot.Models.Llm.Giga;

/// <summary>
///     <para>Модели GigaChat.</para>
///     <see cref="https://developers.sber.ru/docs/ru/gigachat/models">Подробнее о моделях</see>
///     <remarks>
///         Описание моделей взято с
///         <see cref="https://developers.sber.ru/docs/ru/gigachat/models">сайта моделей GigaChat</see>
///     </remarks>
/// </summary>
public enum GigaChatApiModel
{
    /// <summary>
    ///     Легкая модель для простых задач, требующих максимальной скорости работы
    /// </summary>
    GigaChat,

    /// <summary>
    ///     Продвинутая модель для сложных задач, требующих креативности и лучшего следования инструкциям
    /// </summary>
    GigaChatPro,

    /// <summary>
    ///     Продвинутая модель для сложных задач, требующих высокого уровня креативности и качества работы
    /// </summary>
    GigaChatMax,

    /// <summary>
    ///     Быстрая и легкая модель для простых повседневных задач
    /// </summary>
    GigaChat2,

    /// <summary>
    ///     Усовершенствованная модель для ресурсоемких задач, обеспечивающая максимальную эффективность в обработке данных,
    ///     креативности и соблюдении инструкций.
    /// </summary>
    GigaChat2Pro,

    /// <summary>
    ///     Мощная модель для самых сложных и масштабных задач, требующих высочайшего уровня креативности и качества исполнения
    /// </summary>
    GigaChat2Max
}

public static class GigaChatModelExtensions
{
    /// <summary>
    ///     Преобразует значение модели в название модели, которое можно использовать в запросах к API GigaChat.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string ToApiModelName(this GigaChatApiModel model)
    {
        return model switch
        {
            GigaChatApiModel.GigaChat => "GigaChat",
            GigaChatApiModel.GigaChatPro => "GigaChat-Pro",
            GigaChatApiModel.GigaChatMax => "GigaChat-Max",
            GigaChatApiModel.GigaChat2 => "GigaChat-2",
            GigaChatApiModel.GigaChat2Pro => "GigaChat-2-Pro",
            GigaChatApiModel.GigaChat2Max => "GigaChat-2-Max",
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
        };
    }
}