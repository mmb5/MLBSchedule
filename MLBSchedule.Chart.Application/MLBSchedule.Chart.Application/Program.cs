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
//            MakeChart(2016);
            MakeChart(1949);
        }

        static private void MakeChart(int year)
        {
            var fileService = new FileService();
            var schedule = fileService.ReadSchedule(@"C:\Personal\MLBSchedule\scheduleYYYY.txt".Replace("YYYY", year.ToString()));
            var division = fileService.ReadFormatFile(@"C:\Personal\MLBSchedule\formatYYYY.txt".Replace("YYYY", year.ToString()));

            var chartService = new ChartService(schedule, division);
            var chart = chartService.Get();

            using (StreamWriter sw = new StreamWriter(@"c:\personal\MLBSchedule\chartYYYY.html".Replace("YYYY", year.ToString())))
            {
                sw.WriteLine("<html><style>td { font-family: Arial; font-size: 9pt; border-spacing: 0px; padding: 0px; border-collapse: collapse; } tr { border-spacing: 0px; padding: 0px; border-collapse: collapse; } table { border-spacing: 0px; padding: 0px; border-collapse: collapse; }</style><body>");
                sw.WriteLine(chart);
                sw.WriteLine("</body></html>");
            }


        }
    }
}
