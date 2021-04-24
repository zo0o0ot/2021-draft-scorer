using System;
using System.Collections.Generic;
using CsvHelper.Configuration;
using CsvHelper;
using System.IO;
using System.Linq;

namespace _2021_draft_scorer
{
    public class DraftPick
    {
        public int round;
        public string teamCity;
        public string pickNumber;
        public string playerName;
        public string school;
        public string position;
        public string reachValue;
        public int leagifyPoints;
        public string state;
        public bool pickTraded;
        public bool actualPick;


        public DraftPick(){}
        public DraftPick(string pick, string team, string name, string school, string pos, string relativeVal)
        {
            this.pickNumber = pick;
            this.teamCity = team;
            this.round = convertPickToRound(pick);
            this.playerName = name;
            this.school = school;
            this.position = pos;
            this.reachValue = relativeVal;
            this.pickTraded = hasPickBeenTraded(team, pick);
            this.state = getState(school);

            if(this.pickTraded)
            {
                this.leagifyPoints = 10 + convertPickToPoints(pick, this.round);
            }
            else
            {
                this.leagifyPoints = convertPickToPoints(pick, this.round);
            }


        }
        public static int convertPickToRound(string pick)
        {
            // Compensatory picks added on 2/1/20
            int intpick = 0;
            var canParse = int.TryParse(pick, out intpick);
            if (canParse)
            {
                /* 
                    Pick numbers without comp picks:
                    Picks 1-32 : Round 1
                    Picks 33-64: Round 2
                    Picks 65-96: Round 3
                    Picks 97-128: Round 4
                    Picks 129-159: Round 5
                    Picks 160-191: Round 6
                    Picks 192-223: Round 7

                    Pick numbers with comp picks:
                    Round 1 = picks 1-32
                    Round 2 = picks 33-64 
                    Round 3 = picks 65-106
                    Round 4 = picks 107-146
                    Round 5 = picks 147-179
                    Round 6 = picks 180-214
                    Round 7 = picks 215-255
                */
                if(intpick >= 1 && intpick <= 32)
                {
                    return 1;
                } else if (intpick >= 33 && intpick <= 64)
                {
                    return 2;
                } else if (intpick >= 65 && intpick <=106)
                {
                    return 3;
                } else if (intpick >= 107 && intpick <= 146)
                {
                    return 4;
                } else if (intpick >= 147 && intpick <= 179)
                {
                    return 5;
                } else if (intpick >= 180 && intpick <= 214)
                {
                    return 6;
                } else if (intpick >= 215 && intpick <= 255)
                {
                    return 7;
                }
                return 0;
            }
            else
            {
                return 0;
            }
            
        }

        public static bool hasPickBeenTraded(string teamText, string pick)
        {
            if (teamText.Contains("Trade"))
            {
                return true;
            }
            else if (pick == "59" || pick == "60" || pick == "69" || pick == "74" 
                    || pick == "75" || pick == "88" || pick == "98" || pick == "100" 
                    || pick == "105" || pick == "109" || pick == "121" || pick == "126" 
                    || pick == "130" || pick == "134" || pick == "139" || pick == "141"
                    || pick == "146" || pick == "148" || pick == "153" || pick == "159" 
                    || pick == "164" || pick == "169" || pick == "182" || pick == "196" 
                    || pick == "203" || pick == "207" || pick == "210" || pick == "212"
                    || pick == "213" || pick == "219" || pick == "233" || pick == "240" 
                    || pick == "245" || pick == "248" || pick == "250" || pick == "251")
            {
                // bad data on site. Not listed as part of a trade even though it is.
                return true;
            }
            else
            {
                return false;
            }
            //return teamText.Contains("Trade");
        }

        public static int convertPickToPoints(string pick, int round)
        {
            int intpick = 0;
            var canParse = int.TryParse(pick, out intpick);
            if (canParse)
            {
                /* 
                    Top Pick: 40 Points
                    Picks 2-10: 35 Points
                    Picks 11-20: 30 Points
                    Picks 21-32: 25 Points
                    Picks 33-48: 20 Points
                    Picks 49-64: 15 Points
                    Round 3: 10 Points
                    Round 4: 8 Points
                    Round 5: 7 Points
                    Round 6: 6 Points
                    Round 7: 5 Points
                */
                if(intpick == 1)
                {
                    return 40;
                }
                else if (intpick >= 2 && intpick <= 10)
                {
                    return 35;
                }
                else if (intpick >= 11 && intpick <= 20)
                {
                    return 30;
                }
                else if (intpick >= 21 && intpick <= 32)
                {
                    return 25;
                }
                else if (intpick >= 33 && intpick <= 48)
                {
                    return 20;
                }
                else if (intpick >= 49 && intpick <= 64)
                {
                    return 15;
                } 
                else if (round == 3)
                {
                    return 10;
                } 
                else if (round == 4)
                {
                    return 8;
                } 
                else if (round == 5)
                {
                    return 7;
                } 
                else if (round == 6)
                {
                    return 6;
                } 
                else if (round == 7)
                {
                    return 5;
                }
            }
            return 0;
        }
        public static string getState(string school)
        {
            // Get Schools and the States where they are located.
            List<School> schoolsAndConferences;
            using (var reader = new StreamReader($"info{Path.DirectorySeparatorChar}SchoolStatesAndConferences.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<SchoolCsvMap>();
                schoolsAndConferences = csv.GetRecords<School>().ToList();
            }
            var stateResult = from s in schoolsAndConferences
                                 where s.schoolName == school
                                 select s.state;

            var srfd = stateResult.FirstOrDefault();
            string sr = "";

            if (srfd != null)
            {
                sr = srfd.ToString();
            }
            else
            {
                Console.WriteLine("Error matching school!");
            }
            

            if(sr.Length > 0)
            {
                return sr;
            }
            else
            {
                return "";
            }
            //return stateResult.FirstOrDefault().ToString();
        }
    }
    public sealed class DraftPickCsvMap : ClassMap<DraftPick>
    {
        public DraftPickCsvMap()
        {
            //Pick,Round,Player,School,Position,Team,ReachValue,Points,PickTraded
            Map(m => m.pickNumber).Name("Pick");
            Map(m => m.round).Name("Round");
            Map(m => m.playerName).Name("Player");
            Map(m => m.school).Name("School");
            Map(m => m.position).Name("Position");
            Map(m => m.teamCity).Name("Team");
            Map(m => m.reachValue).Name($"ReachValue");
            Map(m => m.leagifyPoints).Name("Points");
            Map(m => m.state).Name("State");
            Map(m => m.pickTraded).Name("PickTraded");
        }
    }
}