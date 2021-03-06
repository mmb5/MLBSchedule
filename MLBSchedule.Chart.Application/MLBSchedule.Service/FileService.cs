﻿using System;
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

        public Dictionary<int, string> ReadFormatPointerFile(string Filename)
        {
            var dict = new Dictionary<int, string>();
            using (StreamReader sr = new StreamReader(Filename))
            {
                while (!sr.EndOfStream)
                {
                    var input = sr.ReadLine().Split(',');
                    dict.Add(Convert.ToInt32(input[0]), input[1]);
                }
            }
            return dict;

        }

        public List<Team> ReadTeamFile(string Filename)
        {
            var list = new List<Team>();
            using (StreamReader sr = new StreamReader(Filename))
            {
                while (!sr.EndOfStream)
                {
                    var input = sr.ReadLine().Split(',');
                    var team = new Team();
                    team.Franchise = input[0];
                    team.Codes = input[1].Split('|').ToList();
                    list.Add(team);
                }
            }
            return list;
        }

        public List<Group> ReadGroupFile(string Filename)
        {
            var list = new List<Group>();
            using (StreamReader sr = new StreamReader(Filename))
            {
                while (!sr.EndOfStream)
                {
                    var input = sr.ReadLine().Split(',');
                    var group = new Group();
                    group.Code = input[0];
                    group.Name = input[1];
                    group.LeagueCode = input[2];
                    list.Add(group);
                }
            }
            return list;
        }

        public List<League> ReadLeagueFile(string Filename)
        {
            var list = new List<League>();
            using (StreamReader sr = new StreamReader(Filename))
            {
                while (!sr.EndOfStream)
                {
                    var input = sr.ReadLine().Split(',');
                    var league = new League();
                    league.Code = input[0];
                    league.Name = input[1];
                    list.Add(league);
                }
            }
            return list;
        }

        private Game ParseGame(string Input)
        {
            var input = Input.Replace("\"", "");
            var fields = input.Split(',');
            if ((fields.Count() == 12) || (fields.Count() == 10))
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
                if (fields.Count() >= 11)
                {
                    game.Postponed = fields[10];
                    ConvertDate(fields[11], out game.Makeup);
                }
                return game;
            }
            else
            {
                throw new InvalidDataException($"Schedule record does not contain 10 or 12 fields: {Input}");
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
                throw new InvalidDataException($"Division record does not contain 5 fields: {Input}");
            }
        }

        private void ConvertDate(string Input, out DateTime Date)
        {
            Date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(Input))
            {
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
}
