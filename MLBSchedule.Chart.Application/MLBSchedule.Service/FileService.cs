using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLBSchedule.Model;

namespace MLBSchedule.Service
{
    public class FileService
    {
        public List<Game> ReadSchedule(string Filename)
        {
            var games = new List<Game>();
            using (StreamReader sr = new StreamReader(Filename))
            {
                while (!sr.EndOfStream)
                {
                    var input = sr.ReadLine();
                    games.Add(ParseGame(input));
                }
            }
            return games;
        }

        public List<Division> ReadFormatFile(string Filename)
        {
            var divisions = new List<Division>();
            using (StreamReader sr = new StreamReader(Filename))
            {
                while (!sr.EndOfStream)
                {
                    var input = sr.ReadLine();
                    divisions.Add(ParseDivision(input));
                }
            }
            return divisions;
        }

        private Game ParseGame(string Input)
        {
            var input = Input.Replace("\"", "");
            var fields = input.Split(',');
            if (fields.Count() == 12)
            {
                var game = new Game();
                var visitor = new GameTeam();
                var home = new GameTeam();
                game.Visitor = visitor;
                game.Home = home;
                ConvertDate(fields[0], out game.Date);
                game.DHType = Convert.ToInt32(fields[1]);
                visitor.Abbr = fields[3];
                visitor.League = fields[4];
                visitor.GameNumber = Convert.ToInt32(fields[5]);
                home.Abbr = fields[6];
                home.League = fields[7];
                home.GameNumber = Convert.ToInt32(fields[8]);
                game.DayNight = fields[9];
                game.Postponed = fields[10];
                ConvertDate(fields[11], out game.Makeup);
                return game;
            }
            else
            {
                throw new InvalidDataException($"Schedule record does not contain 12 fields: {Input}");
            }
        }

        private Division ParseDivision(string Input)
        {
            var input = Input.Replace("\"", "");
            var fields = input.Split(',');
            if (fields.Count() == 5)
            {
                var division = new Division();
                division.Abbreviation = fields[0];
                division.Teams = fields[1].Split('|').ToList();
                division.Cities = fields[2].Split('|').ToList();
                division.TwoChar = fields[3].Split('|').ToList();
                division.ColorMask = fields[4];
                return division;
            }
            else
            {
                throw new InvalidDataException($"Division record does not contain 3 fields: {Input}");
            }
        }

        private void ConvertDate(string Input, out DateTime Date)
        {
            Date = DateTime.MinValue;
            try
            {
                Date = new DateTime(Convert.ToInt32(Input.Substring(0, 4)), Convert.ToInt32(Input.Substring(4, 2)), Convert.ToInt32(Input.Substring(6, 2)));
            }
            catch
            {
            }
        }
    }
}
