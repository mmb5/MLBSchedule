using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLBSchedule.Model
{
    public class LeagueSummary
    {
        public string Code;
        public string League;
        public int Teams;
        public int Year;
        public int Games;
        public DateTime FirstDate;
        public DateTime LastDate;
        public Dictionary<DayOfWeek, double> CapacityDates = new Dictionary<DayOfWeek, double>();
        public Dictionary<DayOfWeek, double> CapacityGames = new Dictionary<DayOfWeek, double>();
        public int LongHomeDays;
        public int LongRoadDays;
        public int LongHomeGames;
        public int LongRoadGames;
        public int MedianHomeDays;
        public int MedianRoadDays;
        public int MedianHomeGames;
        public int MedianRoadGames;
        public int TotalDHs;
        public int MedianDHs;
        public int MedianSeries;
        public int MinOffDays;
        public int MedianOffDays;
        public int MaxOffDays;



    }


}
