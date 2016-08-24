using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLBSchedule.Model
{
    public class Division
    {
        public string Abbreviation;
        public List<string> Teams = new List<string>();
        public List<string> Cities = new List<string>();
        public List<string> TwoChar = new List<string>();
        public string ColorMask;
    }
}
