namespace HospitalAiChatBot.Models.ScenarioDeterminant;

public interface IScenarioDeterminant
{
    // ReSharper disable once GrammarMistakeInComment
    /// <summary>
    ///     Сопоставляет/Выводит запрошенный сценарий со словесным запросом
    /// </summary>
    /// <param name="query">Словесный запрос</param>
    /// <returns>
    ///     <c>
    ///         <see cref="RequestedScenario">Сценарий</see>
    ///     </c>
    ///     - если сценарий был сопоставлен,
    ///     <c>null</c> - иначе
    /// </returns>
    RequestedScenario? DeterminateScenario(string query);
}