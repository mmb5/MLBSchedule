using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLBSchedule.Model;

namespace MLBSchedule.Service
{
    public class DataService
    {
        private List<Game> games = new List<Game>();
        private List<Division> divisions = new List<Division>();
        public List<Game> Games { get { return games; } }
        public List<Division> Divisions { get { return divisions; } }

        public DataService(List<Game> Games, List<Division> Divisions)
        {
            games = Games;
            divisions = Divisions;
        }

        public Game GetGame(DateTime Date, string Team, bool IsHome)
        {
            return games.Where(g => (g.Date == Date) && ((IsHome && (g.Home.Abbr == Team)) || (!IsHome && (g.Visitor.Abbr == Team)))).FirstOrDefault();
        }

        public Division GetDivision(string Team)
        {
            return divisions.Where(d => d.Teams.Contains(Team)).FirstOrDefault();
        }

        public IEnumerable<Game> GetGamesForDayAndSite(string Team, DayOfWeek Day, bool IsHome)
        {
            return games.Where(g => ((g.Visitor.Abbr.Equals(Team) && !IsHome) || (g.Home.Abbr.Equals(Team) && IsHome)) &&
                                    (g.Date.DayOfWeek == Day));
        }

        public int GetDatesForDayAndSite(string Team, DayOfWeek Day, bool IsHome)
        {
            return GetGamesForDayAndSite(Team, Day, IsHome).Select(d => d.Date).Distinct().Count();
        }

        public int GetGameCountForDayAndSite(string Team, DayOfWeek Day, bool IsHome)
        {
            return GetGamesForDayAndSite(Team, Day, IsHome).Select(d => d.Date).Count();
        }

        public int[] GetLongestStand(string Team, bool IsHome)
        {
            int longestByDay = 0;
            int longestByCount = 0;
            int offDay = 0;
            DateTime firstDate = games.Where(g => g.Home.Abbr.Equals(Team) || g.Visitor.Abbr.Equals(Team)).Min(m => m.Date);
            DateTime lastDate = games.Where(g => g.Home.Abbr.Equals(Team) || g.Visitor.Abbr.Equals(Team)).Max(m => m.Date);

            var today = firstDate;
            int standByDay = 0;
            int standByCount = 0;
            bool currentlyAtHome = false;
            while (today <= lastDate)
            {
                longestByDay = Math.Max(standByDay, longestByDay);
                longestByCount = Math.Max(standByCount, longestByCount);
                var game = games.Where(g => (g.Home.Abbr.Equals(Team) || g.Visitor.Abbr.Equals(Team)) && (g.Date == today));
                if (game.FirstOrDefault() != null)
                {
                    var atHome = game.FirstOrDefault().Home.Abbr.Equals(Team);
                    if ((atHome == currentlyAtHome) && (atHome == IsHome))
                    {
                        standByDay += offDay + 1;
                        standByCount += game.Count();
                    }
                    else
                    {
                        standByDay = 1;
                        standByCount = 1;
                    }
                    offDay = 0;
                    currentlyAtHome = atHome;
                }
                else
                {
                    offDay++;
                }
                today = today.AddDays(1);
            }
            longestByDay = Math.Max(standByDay, longestByDay);
            longestByCount = Math.Max(standByCount, longestByCount);

            int[] longest = new int[2];
            longest[0] = longestByDay;
            longest[1] = longestByCount;

            return longest;

        }

        public int GetSeriesCount(string Team)
        {
            DateTime firstDate = games.Where(g => g.Home.Abbr.Equals(Team) || g.Visitor.Abbr.Equals(Team)).Min(m => m.Date);
            DateTime lastDate = games.Where(g => g.Home.Abbr.Equals(Team) || g.Visitor.Abbr.Equals(Team)).Max(m => m.Date);

            var today = firstDate;
            var lastOpp = "X";
            int count = 0;
            while (today <= lastDate)
            {
                var game = games.Where(g => (g.Home.Abbr.Equals(Team) || g.Visitor.Abbr.Equals(Team)) && (g.Date == today)).FirstOrDefault();
                if (game != null)
                {
                    var opp = $"{game.Visitor.Abbr}@{game.Home.Abbr}";
                    if (opp != lastOpp)
                    {
                        count++;
                    }
                    lastOpp = opp;
                }
                today = today.AddDays(1);

            }

            return count;

        }

        public int GetDoubleheaders(string Team, bool IsHome)
        {
            return games.Where(g => ((IsHome && g.Home.Abbr.Equals(Team)) || (!IsHome && g.Visitor.Abbr.Equals(Team))) && (g.DHType == 2)).Count();
        }

        public int GetOffDays(string Team)
        {
            var teamGames = games.Where(g => g.Home.Abbr.Equals(Team) || g.Visitor.Abbr.Equals(Team));
            var daysInSeason = (teamGames.Max(m => m.Date) - teamGames.Min(m => m.Date)).Days + 1;
            var days = teamGames.Select(g => g.Date).Distinct().Count();
            return daysInSeason - days;
        }

        public string GetOppCounts(string Team)
        {
            StringBuilder html = new StringBuilder();
            List<string> sets = new List<string>();
            foreach (var division in divisions)
            {
                foreach (var opp in division.Teams.Where(t => !t.Equals(Team)))
                {
                    var road = games.Where(g => g.Home.Abbr.Equals(opp) && g.Visitor.Abbr.Equals(Team)).Count();
                    var home = games.Where(g => g.Visitor.Abbr.Equals(opp) && g.Home.Abbr.Equals(Team)).Count();
                    if (road + home > 0)
                    {
                        var oppDiv = divisions.Where(d => d.Teams.Contains(opp)).FirstOrDefault();
                        var oppIdx = oppDiv.Teams.IndexOf(opp);
                        sets.Add($"{oppDiv.TwoChar[oppIdx]}: {home}/{road}");
                    }
                }
            }

            for (int row = 0; row < 8; row++)
            {
                html.AppendLine("<tr>");
                for (int col = 0; col < 3; col++)
                {
                    html.Append("<td style=\"border-style: none\" width=\"33%\">");
                    if (row + (col * 8) < sets.Count)
                    {
                        html.Append($"{sets[row + (col * 8)]}</td>");
                    }
                    else
                    {
                        html.Append("&nbsp;</td>");
                    }
                }
                html.AppendLine("</tr>");
            }
            return html.ToString();

        }


    }
}
