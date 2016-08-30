using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLBSchedule.Model
{
    public class Team
    {
        public string Franchise;
        public List<string> Codes = new List<string>();
    }

    public class TeamSummary
    {
        public Team Team;
        public string Code;
        public int Year;
        public DateTime FirstDate;
        public DateTime LastDate;
        public Dictionary<DayOfWeek, int> HomeDates = new Dictionary<DayOfWeek, int>();
        public Dictionary<DayOfWeek, int> RoadDates = new Dictionary<DayOfWeek, int>();
        public Dictionary<DayOfWeek, int> HomeGames = new Dictionary<DayOfWeek, int>();
        public Dictionary<DayOfWeek, int> RoadGames = new Dictionary<DayOfWeek, int>();
        public int LongHomeDays;
        public int LongRoadDays;
        public int LongHomeGames;
        public int LongRoadGames;
        public int HomeDHs;
        public int RoadDHs;
        public int Series;
        public int OffDays;

        public int TotalHomeGames { get { return TotalGames(true); } }
        public int TotalRoadGames { get { return TotalGames(false); } }

        private int TotalGames(bool IsHome)
        {
            int t = 0;
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                t += IsHome ? HomeGames[day] : RoadGames[day];
            }
            return t;
        }

    }


}
