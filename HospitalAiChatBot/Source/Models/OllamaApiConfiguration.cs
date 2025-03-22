namespace HospitalAiChatbot.Source.Models
{
    public struct OllamaChatClientConfiguration
    {
        public Uri ApiUri;
        public string ModelName;
        public string? Suffix = null;
        public bool IsStreamResponce = true;

        public OllamaChatClientConfiguration(Uri apiUri, string modelName, string? suffix = null, bool isStreamResponce = true)
        {
            ApiUri = apiUri;
            ModelName = modelName;
            Suffix = suffix;
            IsStreamResponce = isStreamResponce;
        }
    }
}