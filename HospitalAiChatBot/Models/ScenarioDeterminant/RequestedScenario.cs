namespace HospitalAiChatBot.Models.ScenarioDeterminant;

/// <summary>
///     Запрошенный сценарии пользователя
/// </summary>
public enum RequestedScenario
{
    GetWorkTimeAndContacts,
    GetDoctorWorkTime,
    GetExaminationPrepareInfo,
    GetSamplesPreparingTimeInfo,
    MakeAppointment,
    Feedback,
    DefferedAnswer,
    PromiseToCall,
    CommunicationWithOperator
}