using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HospitalAiChatBot.Models;

public enum ClientType
{
    TelegramBot,
    ChitgmaClinicSite
}

public class Question
{
    public string? Id { get; set; }

    public required ClientType FromClientType { get; set; }

    public required string Contacts { get; set; }

    public required string Text { get; set; }
}