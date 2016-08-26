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
        private DataService data;

        public ChartService()
        {
        }

        public string Get(DataService Data)
        {
            StringBuilder html = new StringBuilder();
            data = Data;

            //  Get the date bounds of the schedule
            var firstMonday = data.Games.Min(g => g.Date);
            while (firstMonday.DayOfWeek != DayOfWeek.Monday)
            {
                firstMonday = firstMonday.AddDays(-1);
            }
            var lastSunday = data.Games.Max(g => g.Date);
            while (lastSunday.DayOfWeek != DayOfWeek.Sunday)
            {
                lastSunday = lastSunday.AddDays(1);
            }

            foreach (var d in data.Divisions)
            {
                html.AppendLine(GetDivisionBlock(d, firstMonday, lastSunday));
                html.AppendLine("<br />");
            }

            return html.ToString();

        }


        private string GetDivisionBlock(Division Division, DateTime FirstMonday, DateTime LastSunday)
        {
            StringBuilder html = new StringBuilder();

            //  Set the division row
            var color = Division.ColorMask;
            html.AppendLine("<table><tr><td>&nbsp;</td>");
            html.AppendLine($"<td style=\"background-color: {color}; text-align: center; {GetBorderStyle(true, true, true, true)}\" colspan={7 * Division.Teams.Count}>{Division.Abbreviation}</td>");
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
                html.AppendLine(GetWeekBlock(Division, thisWeek, thisWeek == FirstMonday, thisWeek.AddDays(7) > LastSunday));
                thisWeek = thisWeek.AddDays(7);
            }

            //  Get the summary info
            html.AppendLine(GetFooter(Division));

            //  Send it all back
            html.AppendLine("</table>");
            return html.ToString();

        }

        private string GetWeekBlock(Division Division, DateTime Monday, bool IsFirst, bool IsLast)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, IsFirst, IsLast)}\">{Monday.ToString("MMM dd")}</td>");

            foreach (var t in Division.Teams)
            {
                var today = Monday;
                while (today < Monday.AddDays(7))
                {
                    var visitor = data.GetGame(today, t, false);
                    var home = data.GetGame(today, t, true);
                    var borderStyle = GetBorderStyle(today.DayOfWeek == DayOfWeek.Monday, today.DayOfWeek == DayOfWeek.Sunday, IsFirst, IsLast);
                    if (visitor != null)
                    {
                        html.Append(GetGameBlock(visitor, false, borderStyle));
                    }
                    else if (home != null)
                    {
                        html.Append(GetGameBlock(home, true, borderStyle));
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
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, false)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Monday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Tuesday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Wednesday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Thursday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Friday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Saturday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, false)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Sunday, true)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Road Dates</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Monday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Tuesday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Wednesday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Thursday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Friday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Saturday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, true)}\">{data.GetDatesForDayAndSite(t, DayOfWeek.Sunday, false)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, true, false)}\">Home Games</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, false)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Monday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Tuesday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Wednesday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Thursday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Friday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Saturday, true)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, false)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Sunday, true)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Road Games</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Monday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Tuesday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Wednesday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Thursday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Friday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, true)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Saturday, false)}</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, true)}\">{data.GetGameCountForDayAndSite(t, DayOfWeek.Sunday, false)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine(GetStandBlock(Division));

            return html.ToString();
        }

        private string GetGameBlock(Game Game, bool IsHome, string BorderStyle)
        {
            string html = null;
            string color = IsHome ? GetOppColor(Game.Visitor.Abbr) : GetOppColor(Game.Home.Abbr);
            var division = data.GetDivision(IsHome ? Game.Visitor.Abbr : Game.Home.Abbr);
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
            var opp = data.GetDivision(Team);
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

        private string GetStandBlock(Division Division)
        {
            StringBuilder html = new StringBuilder();
            Dictionary<string, List<int>> counts = new Dictionary<string, List<int>>();
            foreach (var t in Division.Teams)
            {
                var home = data.GetLongestStand(t, true);
                var road = data.GetLongestStand(t, false);
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
                html.Append($"<td style=\"{GetBorderStyle(true, true, true, true)} vertical-align: top;\" colspan=6 rowspan=8><table width=\"100%\">{data.GetOppCounts(t)}</table></td>");
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
                html.Append($"<td style=\"{GetBorderStyle(true, false, true, false)}\">{data.GetDoubleheaders(t, true)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Road DH</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{data.GetDoubleheaders(t, false)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, true, false)}\">Series</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, true, false)}\">{data.GetSeriesCount(t)}</td>");
            }
            html.AppendLine("</tr>");
            html.AppendLine($"<tr><td style=\"{GetBorderStyle(true, true, false, true)}\">Off Days</td>");
            foreach (var t in Division.Teams)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)}\">{data.GetOffDays(t)}</td>");
            }
            html.AppendLine("</tr>");

            return html.ToString();
        }


    }
}
