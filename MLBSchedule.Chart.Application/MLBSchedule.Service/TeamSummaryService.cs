using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLBSchedule.Model;

namespace MLBSchedule.Service
{
    public class TeamSummaryService
    {
        private DataService data;

        public List<TeamSummary> Get(DataService Data, List<Team> Teams)
        {
            data = Data;
            var summaries = new List<TeamSummary>();

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

            foreach (var div in data.Divisions)
            {
                var group = data.GetGroupForCode(div.Abbreviation);
                foreach (var city in div.Teams)
                {
                    var games = data.GetGames(city);
                    var summary = new TeamSummary();
                    summary.Team = Teams.Where(t => t.Codes.Contains(city)).FirstOrDefault();
                    summary.Group = group.Code;
                    summary.League = group.LeagueCode;
                    summary.Year = firstMonday.Year;
                    summary.Code = city;
                    summary.FirstDate = games.Min(m => m.Date);
                    summary.LastDate = games.Max(m => m.Date);
                    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                    {
                        summary.HomeDates.Add(day, data.GetDatesForDayAndSite(city, day, true));
                        summary.HomeGames.Add(day, data.GetGameCountForDayAndSite(city, day, true));
                        summary.RoadDates.Add(day, data.GetDatesForDayAndSite(city, day, false));
                        summary.RoadGames.Add(day, data.GetGameCountForDayAndSite(city, day, false));
                    }
                    var stand = data.GetLongestStand(city, true);
                    summary.LongHomeDays = stand[0];
                    summary.LongHomeGames = stand[1];
                    stand = data.GetLongestStand(city, false);
                    summary.LongRoadDays = stand[0];
                    summary.LongRoadGames = stand[1];
                    summary.HomeDHs = data.GetDoubleheaders(city, true);
                    summary.RoadDHs = data.GetDoubleheaders(city, false);
                    summary.Series = data.GetSeriesCount(city);
                    summary.OffDays = data.GetOffDays(city);
                    summaries.Add(summary);
                }
            }

            return summaries;
        }

        public string Coalesce(Team Team, IEnumerable<TeamSummary> Summaries)
        {
            var html = new StringBuilder();
            var first = Summaries.Min(y => y.Year);
            var last = Summaries.Max(y => y.Year);

            html.AppendLine("<table>");
            html.AppendLine($"<tr><td colspan=43 style=\"font-size: 14pt; font-weight: bold; border-style: none\">Schedule Summary for {Team.Franchise}</td></tr>");
            html.AppendLine(GetHeader());
            for (int year = first; year <= last; year++)
            {
                var summary = Summaries.Where(y => y.Year == year).FirstOrDefault();
                if (summary != null)
                {
                    html.AppendLine(GetYear(summary, year == first, year == last));
                }
            }
            html.AppendLine("</table>");

            return html.ToString();
        }

        private string GetHeader()
        {
            var html = new StringBuilder();

            html.Append("<tr>");
            html.Append("<td colspan=9>&nbsp;</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, true, false)}\" colspan=14>Dates</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, true, false)}\" colspan=14>Games</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, true, false)}\" colspan=4>Longest Stand</td>");
            html.Append("<td colspan=2>&nbsp;</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td colspan=9>&nbsp;</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=7>Home</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=7>Road</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=7>Home</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=7>Road</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=2>Days</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=2>Games</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, true, false)}\" colspan=2>DHs</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, true, true)} min-width: 3em;\">Year</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 3em;\">Code</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 3em;\">League</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 3em;\">Group</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 3em;\">Games</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 5em;\">First</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 5em;\">Last</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 4em;\">Series</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, true, true)} min-width: 4em;\">Off Days</td>");

            for (int i = 0; i < 4; i++)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, false)} min-width: 2em;\">Mo</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">Tu</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">We</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">Th</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">Fr</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">Sa</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, false)} min-width: 2em;\">Su</td>");

            }
            for (int i = 0; i < 3; i++)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)} min-width: 3em;\">Home</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, true)} min-width: 3em;\">Road</td>");
            }
            html.Append("</tr>");

            return html.ToString();

        }

        private string GetYear(TeamSummary Summary, bool IsFirst, bool IsLast)
        {
            var html = new StringBuilder();
            var color = Summary.Year % 10 < 5 ? "white" : "whitesmoke";

            html.Append($"<tr style=\"background-color: {color}\">");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{Summary.Year}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.Code}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.League}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.Group}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.TotalHomeGames + Summary.TotalRoadGames}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.FirstDate.ToString("ddd").Substring(0, 2)} {Summary.FirstDate.ToString("MM/dd")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.LastDate.ToString("ddd").Substring(0, 2)} {Summary.LastDate.ToString("MM/dd")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.Series}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, IsFirst, IsLast)}\">{Summary.OffDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{Summary.HomeDates[DayOfWeek.Monday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeDates[DayOfWeek.Tuesday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeDates[DayOfWeek.Wednesday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeDates[DayOfWeek.Thursday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeDates[DayOfWeek.Friday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeDates[DayOfWeek.Saturday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, IsFirst, IsLast)}\">{Summary.HomeDates[DayOfWeek.Sunday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{Summary.RoadDates[DayOfWeek.Monday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadDates[DayOfWeek.Tuesday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadDates[DayOfWeek.Wednesday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadDates[DayOfWeek.Thursday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadDates[DayOfWeek.Friday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadDates[DayOfWeek.Saturday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, IsFirst, IsLast)}\">{Summary.RoadDates[DayOfWeek.Sunday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{Summary.HomeGames[DayOfWeek.Monday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeGames[DayOfWeek.Tuesday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeGames[DayOfWeek.Wednesday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeGames[DayOfWeek.Thursday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeGames[DayOfWeek.Friday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeGames[DayOfWeek.Saturday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, IsFirst, IsLast)}\">{Summary.HomeGames[DayOfWeek.Sunday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{Summary.RoadGames[DayOfWeek.Monday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadGames[DayOfWeek.Tuesday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadGames[DayOfWeek.Wednesday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadGames[DayOfWeek.Thursday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadGames[DayOfWeek.Friday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.RoadGames[DayOfWeek.Saturday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, IsFirst, IsLast)}\">{Summary.RoadGames[DayOfWeek.Sunday]}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{Summary.LongHomeDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.LongRoadDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.LongHomeGames}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.LongHomeDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.HomeDHs}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, IsFirst, IsLast)}\">{Summary.RoadDHs}</td>");

            html.Append("</tr>");
            return html.ToString();

        }


        private string GetBorderStyle(bool StrongLeft, bool StrongRight, bool StrongTop, bool StrongBottom)
        {
            var top = StrongTop ? "black" : "grey";
            var right = StrongRight ? "black" : "grey";
            var bottom = StrongBottom ? "black" : "grey";
            var left = StrongLeft ? "black" : "grey";
            return $"border-color:{top} {right} {bottom} {left}; border-width: thin; border-style: solid;";
        }

    }
}
