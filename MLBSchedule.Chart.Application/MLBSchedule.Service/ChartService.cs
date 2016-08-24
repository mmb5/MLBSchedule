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
    }
}
