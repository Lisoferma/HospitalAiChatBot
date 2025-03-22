namespace HospitalAiChatbot.Source.Models
{
    public interface IScenarioDeterminator
    {
        /// <summary>
        /// Determinate type of scenario by a query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns><see cref="Scenarios"/> - if scenario was determinated;</br>
        /// <see cref="null"/> - otherwise</returns>
        Scenarios? DeterminateScenario(string query);
    }
}