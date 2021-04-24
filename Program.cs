using System;
using CsvHelper;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace _2021_draft_scorer
{
    class Program
    {
        static void Main(string[] args)
        {
            var webGet = new HtmlWeb();
            webGet.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            var document1 = webGet.Load("https://www.drafttek.com/2021-NFL-Mock-Draft/2021-NFL-Mock-Draft-Round-1.asp");
            var document2 = webGet.Load("https://www.drafttek.com/2021-NFL-Mock-Draft/2021-NFL-Mock-Draft-Round-1b.asp");
            var document3 = webGet.Load("https://www.drafttek.com/2021-NFL-Mock-Draft/2021-NFL-Mock-Draft-Round-2.asp");
            var document4 = webGet.Load("https://www.drafttek.com/2021-NFL-Mock-Draft/2021-NFL-Mock-Draft-Round-3.asp");
            var document5 = webGet.Load("https://www.drafttek.com/2021-NFL-Mock-Draft/2021-NFL-Mock-Draft-Round-4.asp");
            var document6 = webGet.Load("https://www.drafttek.com/2021-NFL-Mock-Draft/2021-NFL-Mock-Draft-Round-6.asp");

            //Console.WriteLine(document1.ParsedText);
            //#content > table:nth-child(9)
            //html body div#outer div#wrapper2 div#content table
            ///html/body/div[3]/div[3]/div[1]/table[1]

            List<DraftPick> list1 = getDraft(document1);
            List<DraftPick> list2 = getDraft(document2);
            List<DraftPick> list3 = getDraft(document3);
            List<DraftPick> list4 = getDraft(document4);
            List<DraftPick> list5 = getDraft(document5);
            List<DraftPick> list6 = getDraft(document6);

            //This is the file name we are going to write.
            var csvFileName = $"draft{Path.DirectorySeparatorChar}nflDraft.csv";

            Console.WriteLine("Creating csv...");

            //Write projects to csv with date.
            using (var writer = new StreamWriter(csvFileName))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.RegisterClassMap<DraftPickMap>();
                csv.WriteRecords(list1);
                csv.WriteRecords(list2);
                csv.WriteRecords(list3);
                csv.WriteRecords(list4);
                csv.WriteRecords(list5);
                csv.WriteRecords(list6);
            }

            CheckForMismatches(list1);
            CheckForMismatches(list2);
            CheckForMismatches(list3);
            CheckForMismatches(list4);
            CheckForMismatches(list5);
            CheckForMismatches(list6);

            ScorePicks(list1, list2, list3, list4, list5, list6);
            //CheckForMismatches($"mocks{Path.DirectorySeparatorChar}2020-01-11-mock.csv");
            //CheckForMismatches($"mocks{Path.DirectorySeparatorChar}2020-01-15-mock.csv");
            


            // Document data is of type HtmlAgilityPack.HtmlDocument - need to parse it to find info.
            // I'm pretty sure I'm looking for tables with this attribute: background-image: linear-gradient(to bottom right, #0b3661, #5783ad);

            
            Console.WriteLine("Behold, the draft!");
        }

        public static List<DraftPick> getDraft(HtmlAgilityPack.HtmlDocument doc)
        {
            List<DraftPick> mdpList = new List<DraftPick>();
            // This is still messy from debugging the different values.  It should be optimized.
            var dn = doc.DocumentNode;
            var dns = dn.SelectNodes("/html/body/div/div/div/table");
            Console.WriteLine(dns.Count);
            if (dns.Count > 1)
            {
                var attr = dns[1].Attributes;
                var attrs = attr.ToArray();
                var style = attr.FirstOrDefault().Value;
                var ss = style.ToString();
                bool hasStyle = ss.IndexOf("background-image: linear-gradient", StringComparison.OrdinalIgnoreCase) >= 0;
                foreach(var node in dns)
                {
                    var nodeStyle = node.Attributes.FirstOrDefault().Value.ToString();
                    bool hasTheStyle = node.Attributes.FirstOrDefault().Value.ToString().IndexOf("background-image: linear-gradient", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (hasTheStyle)
                    {
                        var tr = node.SelectSingleNode("tr");
                        DraftPick DraftPick = createDraftEntry(tr);
                        //Separate mock picks from actual picks
                        DraftPick.actualPick = node.Attributes.FirstOrDefault().Value.ToString().IndexOf("darkslategray", StringComparison.OrdinalIgnoreCase) >= 0;
                        mdpList.Add(DraftPick);
                    }
                }
                var hasGradient = dns[1].Attributes.Contains("background-image");
            }
            return mdpList;
        }
        public static DraftPick createDraftEntry(HtmlNode tableRow)
        {
            var childNodes = tableRow.ChildNodes;
            var node1 = childNodes[1].InnerText; //pick number?
            string pickNumber = node1.Replace("\r","")
                                    .Replace("\n","")
                                    .Replace("\t","")
                                    .Replace(" ","");
            var node3 = childNodes[3]; //team (and team image)?
            var teamCity = node3.ChildNodes[0].InnerText
                                    .Replace("\r","")
                                    .Replace("\n","")
                                    .Replace("\t","")
                                    .TrimEnd();
            var node5 = childNodes[5]; //Has Child Nodes - Player, School, Position, Reach/Value
            string playerName = node5.ChildNodes[1].InnerText
                                    .Replace("\r","")
                                    .Replace("\n","")
                                    .Replace("\t","")
                                    .TrimEnd();
            string playerSchoolBeforeChecking = node5.ChildNodes[3].InnerText
                                    .Replace("\r","")
                                    .Replace("\n","")
                                    .Replace("\t","")
                                    .TrimEnd(); // this may have a space afterwards.
            string playerSchool = checkSchool(playerSchoolBeforeChecking);
            string playerPosition = node5.ChildNodes[5].InnerText
                                    .Replace("\r","")
                                    .Replace("\n","")
                                    .Replace("\t","")
                                    .Replace(" ","");
            string reachValue = node5.ChildNodes[9].InnerText
                                    .Replace("\r","")
                                    .Replace("\n","")
                                    .Replace("\t","")
                                    .Replace(" ","");
            
            
            DraftPick dp = new DraftPick(pickNumber, teamCity, playerName, playerSchool, playerPosition, reachValue);
            Console.WriteLine(dp.round);
            Console.WriteLine(dp.leagifyPoints);
            Console.WriteLine(dp.pickNumber);
            Console.WriteLine(dp.teamCity);
            Console.WriteLine(dp.playerName);
            Console.WriteLine(dp.school);
            Console.WriteLine(dp.position);
            Console.WriteLine(dp.reachValue);
            Console.WriteLine(dp.state);
            return dp;
        }

        public static string checkSchool(string school)
        {
            switch(school)
            {
                case "Miami":
                    return "Miami (FL)";
                case "Mississippi":
                    return "Ole Miss";
                case "Central Florida":
                    return "UCF";
                case "MTSU":
                    return "Middle Tennessee";
                case "Eastern Carolina":
                    return "East Carolina";
                case "Pittsburgh":
                    return "Pitt";
                case "FIU":
                    return "Florida International";
                case "Florida St":
                    return "Florida State";
                case "Penn St":
                    return "Penn State";
                case "Minneosta":
                    return "Minnesota";
                case "Mississippi St.":
                    return "Mississippi State";
                case "Mississippi St":
                    return "Mississippi State";
                case "Oklahoma St":
                    return "Oklahoma State";
                case "Boise St":
                    return "Boise State";
                case "Lenoir-Rhyne":
                    return "Lenoir–Rhyne";
                case "NCState":
                    return "NC State";
                case "W Michigan":
                    return "Western Michigan";
                case "UL Lafayette":
                    return "Louisiana-Lafayette";
                case "Cal":
                    return "California";
                case "S. Illinois":
                    return "Southern Illinois";
                case "UConn":
                    return "Connecticut";
                case "LA Tech":
                    return "Louisiana Tech";
                case "Louisiana":
                    return "Louisiana-Lafayette";
                case "San Diego St":
                    return "San Diego State";
                case "South Carolina St":
                    return "South Carolina State";
                case "Wake Forrest":
                    return "Wake Forest";
                case "NM State":
                    return "New Mexico State";
                case "New Mexico St":
                    return "New Mexico State";
                case "Southern Cal":
                    return "USC";
                case "Mempis":
                    return "Memphis";
                case "0":
                    return "Georgia"; //Random Jake Fromm thing.
                case "Southeast Missouri St":
                    return "Southeast Missouri State";
                case "Utah St":
                    return "Utah State";
                default:
                    return school;
            }
        }

        private static void CheckForMismatches(List<DraftPick> listOfPicks)
        {
            //File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", "Checking for mismatches in " + csvFileName + "....." + Environment.NewLine);

            Console.WriteLine("Checking for mismatches....");
            // Read in data from a different project.
            List<School> schoolsAndConferences;
            using (var reader = new StreamReader($"info{Path.DirectorySeparatorChar}SchoolStatesAndConferences.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<SchoolCsvMap>();
                schoolsAndConferences = csv.GetRecords<School>().ToList();
            }

            List<DraftPick> ranks = listOfPicks;

            var schoolMismatches = from r in ranks
                                    join school in schoolsAndConferences on r.school equals school.schoolName into mm
                                    from school in mm.DefaultIfEmpty()
                                    where school is null
                                    select new {
                                        rank = r.pickNumber,
                                        name = r.playerName,
                                        college = r.school
                                    }
                                    ;

            bool noMismatches = true;

            if (schoolMismatches.Count() > 0)
            {
                //File.WriteAllText($"logs{Path.DirectorySeparatorChar}Mismatches.log", "");
            }

            foreach (var s in schoolMismatches){
                noMismatches = false;
                //File.AppendAllText($"logs{Path.DirectorySeparatorChar}Mismatches.log", $"{s.rank}, {s.name}, {s.college}" + Environment.NewLine);
                Console.WriteLine($"{s.rank}, {s.name}, {s.college}");
            }

            if (noMismatches)
            {
                //File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", "No mismatches in " + csvFileName + "....." + Environment.NewLine);
                Console.WriteLine("No mismatches in " + listOfPicks.ToString() + ".....");
            }
            else
            {
                //File.AppendAllText($"logs{Path.DirectorySeparatorChar}Status.log", schoolMismatches.Count() + " mismatches in " + csvFileName + ".....Check Mismatches.log." + Environment.NewLine);
                Console.WriteLine(schoolMismatches.Count() + " mismatches in " + listOfPicks.ToString() + ".....");
            }
        }
        private static void ScorePicks(List<DraftPick> list1, List<DraftPick> list2, List<DraftPick> list3, List<DraftPick> list4, List<DraftPick> list5, List<DraftPick> list6)
        {
            Dictionary<string, int> scores = new Dictionary<string, int>()
            {
                {"Ross",0},
                {"Jawad",0},
                {"Tilo",0},
                {"Jared",0},
                {"AJ",0}
            };
            

            Dictionary<string, string> fantasyTeams = new Dictionary<string, string>()
            {
                { "Wisconsin",	"AJ"},
                { "Minnesota", "AJ"},
                { "Auburn", "AJ"},
                { "Kentucky", "AJ"},
                { "Washington", "AJ"},
                { "Miami (FL)",	"AJ"},
                { "Texas", "AJ"},
                { "Oklahoma", "AJ"},
                { "Oklahoma State", "AJ"},
                { "North Dakota State",	"AJ"},
                { "Michigan", "Jawad"},
                { "Penn State",	"Jawad"},
                { "LSU", "Jared"},
                { "Ole Miss",	"Jawad"},
                { "Pitt", "Jawad"},
                { "TCU", "Jawad"},
                { "Notre Dame", "Jawad"},
                { "BYU", "Jawad"},
                { "Memphis",	"Jawad"},
                { "Indiana", "Ross"},
                { "Illinois", "Ross"},
                { "Alabama", "Ross"},
                { "Tennessee", "Ross"},
                { "USC", "Ross"},
                { "Florida State", "Ross"},
                { "West Virginia", "Ross"},
                { "North Carolina", "Ross"},
                { "Duke", "Ross"},
                { "Tulsa", "Ross"},
                { "Iowa", "Tilo"},
                { "Ohio State", "Tilo"},
                { "Florida", "Tilo"},
                { "Mississippi State", "Tilo"},
                { "Oregon", "Tilo"},
                { "NC State", "Tilo"},
                { "Kansas State", "Tilo"},
                { "Michigan State", "Tilo"},
                { "Louisville", "Tilo"},
                { "Cincinnati", "Tilo"},
                { "Northwestern", "Jared"},
                { "Purdue", "Jared"},
                { "Georgia", "Jared"},
                { "South Carolina", "Jared"},
                { "Stanford", "Jared"},
                { "Clemson", "Jared"},
                { "Texas Tech", "Jared"},
                { "Syracuse", "Jared"},
                { "Missouri", "Jared"},
                { "UCF", "Jared"}
            };

            List<ScoreCard> results = new List<ScoreCard>();

            var listOfLists = new System.Collections.ArrayList();
            listOfLists.Add(list1);
            listOfLists.Add(list2);
            listOfLists.Add(list3);
            listOfLists.Add(list4);
            listOfLists.Add(list5);
            listOfLists.Add(list6);

            foreach (List<DraftPick> list in listOfLists)
            {
                foreach (DraftPick dp in list)
                {
                    if (dp.actualPick) //change this to true to get mock draft picks.
                    {
                        try
                        {
                            if (dp.playerName == "Jalen Hurts") // this can probably be removed.
                            {
                                //Jalen Hurts counts for Oklahoma, not Alabama.
                                int originalScore = scores["Ross"];
                                scores["Ross"] = originalScore + dp.leagifyPoints;
                            }
                            else
                            {
                                string luckyDude = fantasyTeams[dp.school];
                                int originalScore = scores[luckyDude];
                                scores[luckyDude] = originalScore + dp.leagifyPoints;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Nobody picked this school:" + dp.school);
                        }
                        
                        //int ross, int jawad, int tilo, int jared, int aj
                        ScoreCard newScore = new ScoreCard(dp.pickNumber ,scores["Ross"], scores["Jawad"], scores["Tilo"], scores["Jared"], scores["AJ"]);
                        Console.WriteLine("Ross score: " + scores["Ross"].ToString());
                        results.Add(newScore);
                    }
                    
                }
            }

            // No prop bets this year, probably. 
            // AJ: 40
            // Ross: 35
            // Tilo: 25
            // Jawad: 20
            // Jared: Forgot to play
            int originalAJScore = scores["AJ"];
            int originalRossScore = scores["Ross"];
            int originalTiloScore = scores["Tilo"];
            int originalJawadScore = scores["Jawad"];
            int originalJaredScore = scores["Jared"];
            //int ross, int jawad, int tilo, int jared, int aj
            //ScoreCard scoreWithPropBets = new ScoreCard("WithPropBets",scores["Ross"]+35, scores["Jawad"]+20, scores["Tilo"]+25, scores["Jared"], scores["AJ"]+40);
            Console.WriteLine("Ross score: " + scores["Ross"].ToString());
            //results.Add(scoreWithPropBets);


            var csvFileName = $"draft{Path.DirectorySeparatorChar}leagifyResults.csv";

            Console.WriteLine("Creating csv...");

            //Write projects to csv with date.
            using (var writer = new StreamWriter(csvFileName))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.RegisterClassMap<ScoreCardCsvMap>();
                csv.WriteRecords(results);
            }
        }
    }
}
