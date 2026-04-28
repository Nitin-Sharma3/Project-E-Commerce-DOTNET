namespace Reviews.MongoDBSettings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ReviewCollection { get; set; } = "Reviews";
        public string SummaryCollection { get; set; } = "ReviewSummaries";
        public string VoteCollection { get; set; } = "ReviewVotes";
        public string ReportCollection { get; set; } = "ReviewReports";
    }
}