namespace HospitalAiChatBot.Models;

public enum ClientType
{
    TelegramBot,
    ChitgmaClinicSite
}

public class Question
{
    public ClientType FromClientType { get; set; }

    public required string Contacts { get; set; }

    public required string Text { get; set; }

    public bool IsAnswered { get; set; } = false;
}