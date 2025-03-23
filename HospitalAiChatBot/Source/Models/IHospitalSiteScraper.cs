namespace HospitalAiChatbot.Source.Models
{
    public interface IHospitalSiteScraper
    {
        Task<string> GetOpeningHoursAsync();
        Task<string> GetReceptionContactsAsync();
    }
}
