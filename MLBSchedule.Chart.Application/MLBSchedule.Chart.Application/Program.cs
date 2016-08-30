using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MLBSchedule.Model;
using MLBSchedule.Service;

namespace MLBSchedule.Chart.Application
{
    class Program
    {
        static string dataPath;

        static void Main(string[] args)
        {
            dataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\data");
            //MakeSeasonCharts();
            MakeTeamSummaries();
            //Console.ReadLine();
        }

        static private void MakeSeasonCharts()
        {
            var fileService = new FileService();
            var formats = fileService.ReadFormatPointerFile(Path.Combine(dataPath, "formatall.txt"));
            foreach (var y in formats.Where(k => (k.Key >= 1888) && (k.Key <= 2016)))
            {
                MakeChart(GetScheduleFile(y.Key), GetFormatFile(y.Value), y.Key);
            }
        }

        static private void MakeChart(string ScheduleFile, string FormatFile, int Year)
        {
            var fileService = new FileService();
            var schedule = fileService.ReadSchedule(ScheduleFile);
            var division = fileService.ReadFormatFile(FormatFile);

            var dataService = new DataService(schedule, division);
            var chartService = new ChartService();
            var chart = chartService.Get(dataService);

            using (StreamWriter sw = new StreamWriter(Path.Combine(dataPath.Replace("data", "output"), "chartYYYY.html".Replace("YYYY", Year.ToString()))))
            {
                sw.WriteLine("<html><style>td { font-family: Arial; font-size: 9pt; border-spacing: 0px; padding: 0px; border-collapse: collapse; } tr { border-spacing: 0px; padding: 0px; border-collapse: collapse; } table { border-spacing: 0px; padding: 0px; border-collapse: collapse; }</style><body>");
                sw.WriteLine(chart);
                sw.WriteLine("</body></html>");
            }
        }

        static private void MakeTeamSummaries()
        {
            var fileService = new FileService();
            var summaryService = new SummaryService();
            var formats = fileService.ReadFormatPointerFile(Path.Combine(dataPath, "formatall.txt"));
            var teams = fileService.ReadTeamFile(Path.Combine(dataPath, "teams.txt"));
            var summaries = new List<TeamSummary>();

            foreach (var y in formats.Where(k => (k.Key >= 1877) && (k.Key <= 2016)))
            {
                var schedule = fileService.ReadSchedule(GetScheduleFile(y.Key));
                var division = fileService.ReadFormatFile(GetFormatFile(y.Value));
                var dataService = new DataService(schedule, division);
                Console.WriteLine(y.Key);
                summaries.AddRange(summaryService.Get(dataService, teams));
            }

            foreach (var t in summaries.Select(t => t.Team).Distinct())
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(dataPath.Replace("data", "output"), "summaryXXX.html".Replace("XXX", t.Codes[0]))))
                {
                    try
                    {
                        var set = summaries.Where(f => (f != null) && (f.Team.Franchise == t.Franchise)).OrderBy(y => y.Year);
                        if (set.Any())
                        {
                            sw.WriteLine("<html><style>h2 { font-family: Arial; font-size: 14pt; font-weight: bold; text-align: center} td { text-align: center; font-family: Arial; font-size: 9pt; border-spacing: 0px; padding: 0px; border-collapse: collapse; } tr { border-spacing: 0px; padding: 0px; border-collapse: collapse; } table { border-spacing: 0px; padding: 0px; border-collapse: collapse; }</style><body>");
                            sw.WriteLine(summaryService.Coalesce(t, set));
                            sw.WriteLine("</body></html>");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        static private string GetScheduleFile(int Season)
        {
            return Path.Combine(dataPath, "YYYYsked.txt".Replace("YYYY", Season.ToString()));
        }

        static private string GetFormatFile(string Format)
        {
            return Path.Combine(dataPath, Format);
        }
    }
}
