using CsvHelper.Configuration;

namespace _2021_draft_scorer
{
    public class SchoolResult
    {
        public string schoolName;
        public int score;

        public SchoolResult () {}
        public SchoolResult (string schoolName, int score)
        {
            this.schoolName = schoolName;
            this.score = score;
        }
    }
    public sealed class SchoolResultCsvMap : ClassMap<SchoolResult>
    {
        public SchoolResultCsvMap()
        {
            Map(m => m.schoolName).Name("School");
            Map(m => m.score).Name("Score");
        }
    }
}