namespace HospitalAiChatBot.Models;

/// <summary>
///     Предоставляет методы для доступа к информации о медицинском учреждении.
/// </summary>
public interface IHospitalInformationProvider
{
    /// <summary>
    ///     Получить расписание работы в виде форматированной строки.
    /// </summary>
    string GetOpeningHours();


    /// <summary>
    ///     Получить номер телефона колл-центра.
    /// </summary>
    string GetCallCenterContacts();


    /// <summary>
    ///     Скачать файл с ценами на медицинские услуги.
    /// </summary>
    /// <returns>Файл в виде масива байт.</returns>
    Task<byte[]> DownloadPriceListAsync();


    /// <summary>
    ///     Скачать файл с памятками для пациентов по подготовке к диагностическим исследованиям.
    /// </summary>
    /// <returns>Файл в виде масива байт.</returns>
    Task<byte[]> DownloadPreparingProcedureAsync();
}