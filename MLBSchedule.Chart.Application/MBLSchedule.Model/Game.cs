using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLBSchedule.Model
{
    public class GameTeam
    {
        public string Abbr;
        public string League;
        public int GameNumber;
    }
    public class Game
    {
        public DateTime Date;
        public int DHType;
        public GameTeam Visitor;
        public GameTeam Home;
        public string DayNight;
        public string Postponed;
        public DateTime Makeup;
    }


}
