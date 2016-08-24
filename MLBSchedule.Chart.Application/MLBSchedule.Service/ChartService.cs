using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLBSchedule.Model;

namespace MLBSchedule.Service
{
    public class ChartService
    {
        private List<Game> games = new List<Game>();
        private List<Division> divisions = new List<Division>();

        public ChartService(List<Game> Games, List<Division> Divisions)
        {
            games = Games;
            divisions = Divisions;
        }

        public string Get()
        {
            StringBuilder html = new StringBuilder();

            //  Get the date bounds of the schedule
            var firstMonday = games.Min(g => g.Date);
            while (firstMonday.DayOfWeek != DayOfWeek.Monday)
            {
                firstMonday = firstMonday.AddDays(-1);
            }
            var lastSunday = games.Max(g => g.Date);
            while (lastSunday.DayOfWeek != DayOfWeek.Sunday)
            {
                lastSunday = lastSunday.AddDays(1);
            }

            foreach (var d in divisions)
            {
                html.AppendLine(GetDivision(d, firstMonday, lastSunday));
                html.AppendLine("<br />");
            }

            return html.ToString();

        }


        private string GetDivision(Division Division, DateTime FirstMonday, DateTime LastSunday)
        {
            StringBuilder html = new StringBuilder();

            //  Set the division row
            html.AppendLine("<table><tr><td>&nbsp;</td>");
            html.AppendLine($"<td style=\"text-align: center; {GetBorderStyle(true, true, true, true)}\" colspan={7 * Division.Teams.Count}>{Division.Abbreviation}</td>");
            html.AppendLine("</tr>");

            //  Set the team row
            html.AppendLine("<tr><td>&nbsp;</td>");
            foreach (var t in Division.Cities)
            {
                var idx = Division.Cities.IndexOf(t);
                html.AppendLine($"<td style=\"text-align: center; {GetBorderStyle(true, true, true, false)}\" colspan=7>{t} ({Division.TwoChar[idx]})</td>");
            }
            html.AppendLine("</tr>");

            //  Set the day-of-week row
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, true, true)}\">Week</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">Mon</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">Tue</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">Wed</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">Thu</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">Fri</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">Sat</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, true)}\">Sun</td>");
            }
            html.AppendLine("</tr>");

            //  Go through the individual weeks
            DateTime thisWeek = FirstMonday;
            while (thisWeek < LastSunday)
            {
                html.AppendLine(GetWeek(Division, thisWeek, thisWeek == FirstMonday, thisWeek.AddDays(7) > LastSunday));
                thisWeek = thisWeek.AddDays(7);
            }

            //  Get the summary info
            html.AppendLine(GetFooter(Division));

            //  Send it all back
            html.AppendLine("</table>");
            return html.ToString();

        }

        private string GetWeek(Division Division, DateTime Monday, bool IsFirst, bool IsLast)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, IsFirst, IsLast)}\">{Monday.ToString("MMM dd")}</td>");

            foreach (var t in Division.Teams)
            {
                var today = Monday;
                while (today < Monday.AddDays(7))
                {
                    var visitor = games.Where(g => (g.Date == today) && (g.Visitor.Abbr == t)).FirstOrDefault();
                    var home = games.Where(g => (g.Date == today) && (g.Home.Abbr == t)).FirstOrDefault();
                    var borderStyle = GetBorderStyle(today.DayOfWeek == DayOfWeek.Monday, today.DayOfWeek == DayOfWeek.Sunday, IsFirst, IsLast);
                    if (visitor != null)
                    {
                        html.Append(GetGame(visitor, false, borderStyle));
                    }
                    else if (home != null)
                    {
                        html.Append(GetGame(home, true, borderStyle));
                    }
                    else
                    {
                        html.Append($"<td style=\"{borderStyle};\">&nbsp;</td>");
                    }
                    today = today.AddDays(1);
                }
            }

            html.AppendLine("</tr>");
            return html.ToString();

        }

        private string GetFooter(Division Division)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, true, false)}\">Home Dates</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, false)}\">{GetDatesForDayAndSite(t, DayOfWeek.Monday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetDatesForDayAndSite(t, DayOfWeek.Tuesday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetDatesForDayAndSite(t, DayOfWeek.Wednesday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetDatesForDayAndSite(t, DayOfWeek.Thursday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetDatesForDayAndSite(t, DayOfWeek.Friday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetDatesForDayAndSite(t, DayOfWeek.Saturday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, false)}\">{GetDatesForDayAndSite(t, DayOfWeek.Sunday, true)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Road Dates</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{GetDatesForDayAndSite(t, DayOfWeek.Monday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetDatesForDayAndSite(t, DayOfWeek.Tuesday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetDatesForDayAndSite(t, DayOfWeek.Wednesday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetDatesForDayAndSite(t, DayOfWeek.Thursday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetDatesForDayAndSite(t, DayOfWeek.Friday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetDatesForDayAndSite(t, DayOfWeek.Saturday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, true)}\">{GetDatesForDayAndSite(t, DayOfWeek.Sunday, false)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, true, false)}\">Home Games</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, false)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Monday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Tuesday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Wednesday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Thursday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Friday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Saturday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, false)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Sunday, true)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Road Games</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Monday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Tuesday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Wednesday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Thursday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Friday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Saturday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, true)}\">{GetGameCountForDayAndSite(t, DayOfWeek.Sunday, false)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine(GetStandCounts(Division));

            return html.ToString();
        }

        private string GetGame(Game Game, bool IsHome, string BorderStyle)
        {
            string html = null;
            string color = IsHome ? GetOppColor(Game.Visitor.Abbr) : GetOppColor(Game.Home.Abbr);
            var division = IsHome ? divisions.Where(d => d.Teams.Contains(Game.Visitor.Abbr)).FirstOrDefault() :
                                    divisions.Where(d => d.Teams.Contains(Game.Home.Abbr)).FirstOrDefault();
            int idx = IsHome ? division.Teams.IndexOf(Game.Visitor.Abbr) :
                               division.Teams.IndexOf(Game.Home.Abbr);
            string suffix = GameSuffix(Game);

            if (IsHome)
            {
                html = $"<td style=\"background-color: {color}; {BorderStyle}\">{division.TwoChar[idx]}{suffix}</td>";
            }
            else
            {
                html = $"<td style=\"background-color: {color}; {BorderStyle}\">@{division.TwoChar[idx]}{suffix}</td>";
            }

            return html;
        }

        private string GetOppColor(string Team)
        {
            var opp = divisions.Where(d => d.Teams.Contains(Team)).FirstOrDefault();
            if (opp == null)
            {
                return "000000";
            }
            else
            {
                return opp.ColorMask;
            }
        }

        private string GetBorderStyle(bool StrongLeft, bool StrongRight, bool StrongTop, bool StrongBottom)
        {
            var top = StrongTop ? "solid" : "dotted";
            var right = StrongRight ? "solid" : "dotted";
            var bottom = StrongBottom ? "solid" : "dotted";
            var left = StrongLeft ? "solid" : "dotted";
            return $"border-style:{top} {right} {bottom} {left}; border-width: thin;";
        }

        private string GameSuffix(Game Game)
        {
            if (Game.DHType == 0)
            {
                return "";
            }
            else
            {
                return "+";
            }
        }

        private IEnumerable<Game> GetGamesForDayAndSite(string Team, DayOfWeek Day, bool IsHome)
        {
            return games.Where(g => ((g.Visitor.Abbr.Equals(Team) && !IsHome) || (g.Home.Abbr.Equals(Team) && IsHome)) &&
                                    (g.Date.DayOfWeek == Day));
        }

        private int GetDatesForDayAndSite(string Team, DayOfWeek Day, bool IsHome)
        {
            return GetGamesForDayAndSite(Team, Day, IsHome).Select(d => d.Date).Distinct().Count();
        }

        private int GetGameCountForDayAndSite(string Team, DayOfWeek Day, bool IsHome)
        {
            return GetGamesForDayAndSite(Team, Day, IsHome).Select(d => d.Date).Count();
        }

        private int[] GetLongestStand(string Team, bool IsHome)
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

        private int GetSeriesCount(string Team)
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

        private int GetDoubleheaders(string Team, bool IsHome)
        {
            return games.Where(g => ((IsHome && g.Home.Abbr.Equals(Team)) || (!IsHome && g.Visitor.Abbr.Equals(Team))) && (g.DHType == 2)).Count();
        }

        private int GetOffDays(string Team)
        {
            var teamGames = games.Where(g => g.Home.Abbr.Equals(Team) || g.Visitor.Abbr.Equals(Team));
            var daysInSeason = (teamGames.Max(m => m.Date) - teamGames.Min(m => m.Date)).Days + 1;
            var days = teamGames.Select(g => g.Date).Distinct().Count();
            return daysInSeason - days;
        }

        private string GetOppCounts(string Team)
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

        private string GetStandCounts(Division Division)
        {
            StringBuilder html = new StringBuilder();
            Dictionary<string, List<int>> counts = new Dictionary<string, List<int>>();
            foreach (var t in Division.Teams)
            {
                var home = GetLongestStand(t, true);
                var road = GetLongestStand(t, false);
                var count = new List<int>();
                count.Add(home[0]);
                count.Add(home[1]);
                count.Add(road[0]);
                count.Add(road[1]);
                counts.Add(t, count);
            }
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, true, false)}\">Long Home Days</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, true, false)}\">{counts[t][0]}</td>");
                html.Append($"<td style=\"{GetBorderStyle(true, true, true, true)} vertical-align: top;\" colspan=6 rowspan=8><table width=\"100%\">{GetOppCounts(t)}</table></td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Long Road Days</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{counts[t][2]}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, true, false)}\">Long Home Games</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, true, false)}\">{counts[t][1]}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Long Road Games</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{counts[t][3]}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, true, false)}\">Home DH</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, true, false)}\">{GetDoubleheaders(t, true)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Road DH</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{GetDoubleheaders(t, false)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, true, false)}\">Series</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, true, false)}\">{GetSeriesCount(t)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Off Days</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{GetOffDays(t)}</td>");
            }
            html.AppendLine("</tr>");

            return html.ToString();
        }


    }
}
