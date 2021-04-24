using CsvHelper.Configuration;

namespace _2021_draft_scorer
{

    public class ScoreCard
    {
        public int Ross;
        public int Jawad;
        public int Tilo;
        public int Jared;
        public int AJ;
        public string pick;

        public ScoreCard () {}
        public ScoreCard (string pick, int ross, int jawad, int tilo, int jared, int aj)
        {
            this.pick = pick;
            this.Ross = ross;
            this.Jawad = jawad;
            this.Tilo = tilo;
            this.Jared = jared;
            this.AJ = aj;
        }
    }
    public sealed class ScoreCardCsvMap : ClassMap<ScoreCard>
    {
        public ScoreCardCsvMap()
        {
            Map(m => m.pick).Name("Pick");
            Map(m => m.Ross).Name("Ross");
            Map(m => m.Jawad).Name("Jawad");
            Map(m => m.Tilo).Name("Tilo");
            Map(m => m.Jared).Name("Jared");
            Map(m => m.AJ).Name("AJ");
        }
    }
}