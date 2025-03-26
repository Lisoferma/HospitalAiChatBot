namespace HospitalAiChatbot.Source.Models
{
    /// <summary>
    /// Предоставляет методы для доступа к информации о медицинском учреждении.
    /// </summary>
    public interface IHospitalInformationProvider
    {
        /// <summary>
        /// Получить расписание работы в виде форматированной строки.
        /// </summary>
        static abstract string GetOpeningHours();


        /// <summary>
        /// Получить номер телефона колл-центра.
        /// </summary>
        static abstract string GetCallCenterContacts();
    }
}
