namespace GHLearning.EasyFusionCache.Infrastructure;
public class MongoOptions
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "easy_fusion_cache";
    public string AnnouncementCollection { get; set; } = "announcements";
}
