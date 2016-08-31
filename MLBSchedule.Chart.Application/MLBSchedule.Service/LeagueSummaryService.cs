using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLBSchedule.Model;

namespace MLBSchedule.Service
{
    public class LeagueSummaryService
    {
        public List<LeagueSummary> Get(List<TeamSummary> TeamSummaries)
        {
            var summaries = new List<LeagueSummary>();

            foreach (var league in TeamSummaries.Select(s => s.League).Distinct())
            {
                foreach (var year in TeamSummaries.Where(w => w.League == league).Select(s => s.Year).OrderBy(o => o).Distinct())
                {
                    var set = TeamSummaries.Where(q => (q.League == league) && (q.Year == year));
                    var summary = new LeagueSummary();
                    summary.Year = year;
                    summary.Code = league;
                    summary.Teams = set.Count();
                    summary.FirstDate = set.Min(g => g.FirstDate);
                    summary.LastDate = set.Max(g => g.LastDate);
                    summary.Games = Median(set.Select(g => g.TotalHomeGames + g.TotalRoadGames));
                    summary.MedianSeries = Median(set.Select(s => s.Series));
                    summary.MinOffDays = set.Min(s => s.OffDays);
                    summary.MedianOffDays = Median(set.Select(s => s.OffDays));
                    summary.MaxOffDays = set.Max(s => s.OffDays);
                    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                    {
                        var denom = DayInSet(summary.FirstDate, summary.LastDate, day) * summary.Teams;
                        summary.CapacityDates.Add(day, (double)set.Sum(s => s.HomeDates[day] + s.RoadDates[day]) / denom);
                        summary.CapacityGames.Add(day, (double)set.Sum(s => s.HomeGames[day] + s.RoadGames[day]) / denom);
                    }
                    summary.LongHomeDays = set.Max(s => s.LongHomeDays);
                    summary.LongHomeGames = set.Max(s => s.LongHomeGames);
                    summary.LongRoadDays = set.Max(s => s.LongRoadDays);
                    summary.LongRoadGames = set.Max(s => s.LongHomeGames);
                    summary.MedianHomeDays = Median(set.Select(s => s.LongHomeDays));
                    summary.MedianHomeGames = Median(set.Select(s => s.LongHomeGames));
                    summary.MedianRoadDays = Median(set.Select(s => s.LongRoadDays));
                    summary.MedianRoadGames = Median(set.Select(s => s.LongRoadGames));
                    summary.TotalDHs = set.Sum(s => s.HomeDHs + s.RoadDHs);
                    summary.MedianDHs = Median(set.Select(s => s.HomeDHs + s.RoadDHs));
                    summaries.Add(summary);
                }
            }
            return summaries;

        }


        public string Coalesce(League League, IEnumerable<LeagueSummary> Summaries)
        {
            var html = new StringBuilder();
            var first = Summaries.Min(y => y.Year);
            var last = Summaries.Max(y => y.Year);

            html.AppendLine("<table>");
            html.AppendLine($"<tr><td colspan=33 style=\"font-size: 14pt; font-weight: bold; border-style: none\">Schedule Summary for {League.Name}</td></tr>");
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
            html.Append($"<td style=\"{GetBorderStyle(true, true, true, false)}\" colspan=14>Capacity</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, true, false)}\" colspan=4>Max Longest Stand</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, true, false)}\" colspan=4>Med Longest Stand</td>");
            html.Append("<td colspan=2>&nbsp;</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td colspan=6>&nbsp;</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=3>Off Days</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=7>Days</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=7>Games</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=2>Days</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=2>Games</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=2>Days</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, false, false)}\" colspan=2>Games</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, true, false)}\" colspan=2>DHs</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, true, true)} min-width: 3em;\">Year</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 3em;\">Teams</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 3em;\">Games</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 5em;\">First</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 5em;\">Last</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, true, true)} min-width: 4em;\">Series</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, true, true)} min-width: 4em;\">Min</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, true, true)} min-width: 4em;\">Med</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, true, true)} min-width: 4em;\">Max</td>");

            for (int i = 0; i < 2; i++)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, false)} min-width: 2em;\">Mo</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">Tu</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">We</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">Th</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">Fr</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, false, false, false)} min-width: 2em;\">Sa</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, false)} min-width: 2em;\">Su</td>");

            }
            for (int i = 0; i < 4; i++)
            {
                html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)} min-width: 3em;\">Home</td>");
                html.Append($"<td style=\"{GetBorderStyle(false, true, false, true)} min-width: 3em;\">Road</td>");
            }

            html.Append($"<td style=\"{GetBorderStyle(true, false, false, true)} min-width: 3em;\">Total</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, false, true)} min-width: 3em;\">Med</td>");

            html.Append("</tr>");

            return html.ToString();

        }


        private string GetYear(LeagueSummary Summary, bool IsFirst, bool IsLast)
        {
            var html = new StringBuilder();
            var color = Summary.Year % 10 < 5 ? "white" : "whitesmoke";

            html.Append($"<tr style=\"background-color: {color}\">");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{Summary.Year}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.Teams}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.Games}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.FirstDate.ToString("ddd").Substring(0, 2)} {Summary.FirstDate.ToString("MM/dd")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.LastDate.ToString("ddd").Substring(0, 2)} {Summary.LastDate.ToString("MM/dd")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.MedianSeries}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.MinOffDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, IsFirst, IsLast)}\">{Summary.MedianOffDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.MaxOffDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{(Summary.CapacityDates[DayOfWeek.Monday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityDates[DayOfWeek.Tuesday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityDates[DayOfWeek.Wednesday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityDates[DayOfWeek.Thursday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityDates[DayOfWeek.Friday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityDates[DayOfWeek.Saturday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, IsFirst, IsLast)}\">{(Summary.CapacityDates[DayOfWeek.Sunday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{(Summary.CapacityGames[DayOfWeek.Monday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityGames[DayOfWeek.Tuesday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityGames[DayOfWeek.Wednesday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityGames[DayOfWeek.Thursday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityGames[DayOfWeek.Friday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{(Summary.CapacityGames[DayOfWeek.Saturday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, true, IsFirst, IsLast)}\">{(Summary.CapacityGames[DayOfWeek.Sunday] * 100).ToString("0")}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{Summary.LongHomeGames}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.LongRoadDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.LongHomeGames}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.LongHomeDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(true, false, IsFirst, IsLast)}\">{Summary.MedianHomeGames}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.MedianRoadDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.MedianHomeGames}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.MedianHomeDays}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, false, IsFirst, IsLast)}\">{Summary.TotalDHs}</td>");
            html.Append($"<td style=\"{GetBorderStyle(false, true, IsFirst, IsLast)}\">{Summary.MedianDHs}</td>");

            html.Append("</tr>");
            return html.ToString();

        }


        private int Median(IEnumerable<int> Values)
        {
            if (Values.Count() == 0)
            {
                return 0;
            }
            else if (Values.Count() == 1)
            {
                return Values.First();
            }
            else
            {
                var list = Values.OrderBy(o => o).ToList();
                var count = Values.Count();
                if (count % 2 == 0)
                {
                    var left = list[count / 2];
                    var right = list[(count / 2) + 1];
                    return (left + right) / 2;
                }
                else
                {
                    return list[count / 2];
                }
            }
        }

        private int DayInSet(DateTime FirstDate, DateTime LastDate, DayOfWeek Day)
        {
            var count = 0;
            var today = FirstDate;
            while (today <= LastDate)
            {
                if (today.DayOfWeek == Day)
                {
                    count++;
                }
                today = today.AddDays(1);
            }
            return count;
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
