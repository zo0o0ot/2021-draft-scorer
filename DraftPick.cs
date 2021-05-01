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
                    Round 3 = picks 65-105
                    Round 4 = picks 106-144
                    Round 5 = picks 145-184
                    Round 6 = picks 185-228
                    Round 7 = picks 229-259
                */
                if(intpick >= 1 && intpick <= 32)
                {
                    return 1;
                } else if (intpick >= 33 && intpick <= 64)
                {
                    return 2;
                } else if (intpick >= 65 && intpick <=105)
                {
                    return 3;
                } else if (intpick >= 106 && intpick <= 144)
                {
                    return 4;
                } else if (intpick >= 145 && intpick <= 184)
                {
                    return 5;
                } else if (intpick >= 185 && intpick <= 228)
                {
                    return 6;
                } else if (intpick >= 229 && intpick <= 259)
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
            //This is frustrating, I'm just entering all trades here.
            else if (pick == "3" || pick == "6" || pick == "10" || pick == "11" 
                    || pick == "12" || pick == "14" || pick == "20" || pick == "23" 
                    || pick == "25" || pick == "31" || pick == "35" || pick == "36" 
                    || pick == "38" || pick == "39" || pick == "40" || pick == "42"
                    || pick == "43" || pick == "45" || pick == "46" || pick == "48" 
                    || pick == "50" || pick == "52" || pick == "58" || pick == "59" 
                    || pick == "66" || pick == "70" || pick == "71" || pick == "73"
                    || pick == "74" || pick == "76" || pick == "79" || pick == "83" 
                    || pick == "84" || pick == "86" || pick == "89" || pick == "90"
                    || pick == "91" || pick == "94" || pick == "98" || pick == "101" 
                    || pick == "105" || pick == "110" || pick == "112" || pick == "114"
                    || pick == "120" || pick == "121" || pick == "122" || pick == "123" 
                    || pick == "125" || pick == "130" || pick == "134" || pick == "136"
                    || pick == "138" || pick == "143" || pick == "151" || pick == "154"
                    || pick == "156" || pick == "158" || pick == "161" || pick == "162"
                    || pick == "164" || pick == "167" || pick == "168" || pick == "169"
                    || pick == "170" || pick == "172" || pick == "185" || pick == "188"
                    || pick == "191" || pick == "192" || pick == "195" || pick == "201" 
                    || pick == "202" || pick == "203" || pick == "204" || pick == "207"
                    || pick == "208" || pick == "212" || pick == "215" || pick == "216"
                    || pick == "219" || pick == "223" || pick == "226" || pick == "229" 
                    || pick == "230" || pick == "231" || pick == "232" || pick == "233"
                    || pick == "235" || pick == "236" || pick == "239" || pick == "240"
                    || pick == "244" || pick == "245" || pick == "247" || pick == "249" 
                    || pick == "251" || pick == "253" || pick == "254" || pick == "257"
                    || pick == "258" 
                    || pick == "85" || pick == "92" || pick == "135"   // Packers-Titans Trade at 85
                    || pick == "88" || pick == "117" || pick == "121"  // 49ers-Rams Trade at 88
                    || pick == "109" || pick == "126" || pick == "166" // Titans-Panthers Trade at 109, also inlcudes 232 (previously traded)
                    || pick == "113" || pick == "153"  //Lions-Browns trade at 113, includes 257 (previously traded) and 2022 4th rounder
                    || pick == "209" || pick == "126" || pick == "166" //Jaguars-Rams trade at 121, includes 121, 130, 170, and 249 (previously traded)
                    || pick == "129" || pick == "137" || pick == "217" //Bucs-Sehawks trade at 129
                    || pick == "200" //Raiders-Jets trade at 143, includes 143 and 162 (previously traded)
                    || pick == "160" || pick == "210" // Cards-Ravens trade at 136 (previously traded)
                    )
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