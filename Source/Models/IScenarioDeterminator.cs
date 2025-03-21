namespace HospitalAIChatbot.Source.Models
{
    public interface IScenarioDeterminator
    {
        Scenarios DeterminateScenario(string query);
    }
}