using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLBSchedule.Model;
using MLBSchedule.Service;

namespace MLBSchedule.Chart.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            GetSummaries();
        }

        static private void GetSummaries()
        {
            var fileService = new FileService();
            var formats = fileService.ReadFormatPointerFile(@"C:\Personal\MLBSchedule\data\formatall.txt");
            foreach (var y in formats.Where(k => (k.Key >= 1889) && (k.Key <= 1890)))
            {
                MakeChart(@"C:\Personal\MLBSchedule\data\YYYYsked.txt".Replace("YYYY", y.Key.ToString()),
                          @"C:\Personal\MLBSchedule\data\FFILE".Replace("FFILE", y.Value.ToString()), y.Key);

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

            using (StreamWriter sw = new StreamWriter(@"c:\personal\MLBSchedule\data\chartYYYY.html".Replace("YYYY", Year.ToString())))
            {
                sw.WriteLine("<html><style>td { font-family: Arial; font-size: 9pt; border-spacing: 0px; padding: 0px; border-collapse: collapse; } tr { border-spacing: 0px; padding: 0px; border-collapse: collapse; } table { border-spacing: 0px; padding: 0px; border-collapse: collapse; }</style><body>");
                sw.WriteLine(chart);
                sw.WriteLine("</body></html>");
            }


        }
    }
}
