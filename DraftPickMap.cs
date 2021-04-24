  
using CsvHelper;
using CsvHelper.Configuration;

namespace _2021_draft_scorer
{
    public sealed class DraftPickMap : ClassMap<DraftPick>
    {
        public DraftPickMap()
        {
            //AutoMap();
            // public int round;
            // public string teamCity;
            // public string pickNumber;
            // public string playerName;
            // public string school;
            // public string position;
            // public string reachValue;
            // public int leagifyPoints;
            Map(m => m.pickNumber).Index(0).Name("Pick");
            Map(m => m.round).Index(1).Name("Round");
            Map(m => m.playerName).Index(2).Name("Player");
            Map(m => m.school).Index(3).Name("School");
            Map(m => m.position).Index(4).Name("Position");
            Map(m => m.teamCity).Index(5).Name("Team");
            Map(m => m.reachValue).Index(6).Name("ReachValue");
            Map(m => m.leagifyPoints).Index(7).Name("Points");
            Map(m => m.state).Index(8).Name("State");
            Map(m => m.pickTraded).Index(9).Name("PickTraded");
            Map(m => m.actualPick).Index(10).Name("ActualPick");
        }
    }
}