using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using Newtonsoft.Json;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.UI.WebControls;
using static ALTTPR_Ladder.Controllers.ScheduleController;
using ALTTPR_Ladder.Helpers;

namespace ALTTPR_Ladder.Controllers
{
    public class ScheduleController : Controller
    {
        public ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();

        public ActionResult Index(int w = 0, int m = 0, int y = 0)
        {
            Helpers.GlobalHelpers.BrowserLogging("Schedule", Request.UserHostAddress.ToString());

            DateTime startingdate = DateTime.Now;

            CalendarModel cm = new CalendarModel();

            //Switch month, m + y
            //Switch week, w + y
            if (y != 0)
            {
                if (m != 0)
                {
                    startingdate = new DateTime(y, m, 1);
                }
                else
                {
                    if (w == -1)
                    {
                        y = y - 1;

                        startingdate = new DateTime(y, 12, 31);

                        while(startingdate.DayOfWeek != DayOfWeek.Sunday)
                        {
                            startingdate = startingdate.AddDays(-1);
                        }
                    }
                    else if (w > GetWeeksInYear(y))
                    {
                        y = y + 1;

                        startingdate = new DateTime(y, 1, 1);
                    }
                    else
                    {
                        startingdate = new DateTime(y, 1, 1);

                        startingdate = startingdate.AddDays(7 * w);
                    }
                }
            }


            w = startingdate.DayOfYear / 7;
            y = startingdate.Year;
            m = startingdate.Month;

            cm.Week = w;
            cm.Month = m;
            cm.Year = y;

            //startingdate = startingdate.AddDays(7 * w);

            while (startingdate.DayOfWeek != DayOfWeek.Sunday)
            {
                startingdate = startingdate.AddDays(-1);
            }

            DateTime startingrange = startingdate.AddDays(-1);
            DateTime endingrange = startingdate.AddDays(8);

            cm.CalendarWeek = new List<CalendarWeek>();

            for (int i = 0; i < 7; i++)
            {
                CalendarWeek cw = new CalendarWeek();
                cw.Day = startingdate;

                cm.CalendarWeek.Add(cw);

                startingdate = startingdate.AddDays(1);
            }

            var racelist = dbContext.tb_races.Where(x => x.StartDateTime >= startingrange && x.StartDateTime <= endingrange).ToList();

            cm.CalendarRace = racelist.Select(x => new CalendarRace()
            {
                RaceGUID = x.RaceGUID,
                RaceName = "#" + x.RaceGUID.ToString().Substring(0, 8),
                FlagName = dbContext.tb_flags.Where(z => z.flag_id == x.flag_id).Select(z => z.FlagName).FirstOrDefault(),
                UTCStartTicks = GlobalHelpers.ConvertToUTC(x.StartDateTime),
                HasCompleted = x.HasCompleted,
                IsChampionship = x.ChampionshipRace ? " *" : ""
            }).OrderBy(x => x.UTCStartTicks).ToList();

            cm.RaceJSON = JsonConvert.SerializeObject(cm.CalendarRace);

            return View(cm);
        }

        public int GetWeeksInYear(int year)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            DateTime date1 = new DateTime(year, 12, 31);
            System.Globalization.Calendar cal = dfi.Calendar;
            return cal.GetWeekOfYear(date1, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }

        public ActionResult RaceDetails(string id)
        {
            Helpers.GlobalHelpers.BrowserLogging("Race Details", Request.UserHostAddress.ToString(), id.ToString());

            if (id.Length == 8)
            {
                id = (dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(id)).Select(x => x.RaceGUID).FirstOrDefault()).ToString();
            }

            Guid raceguid = Guid.Parse(id);

            DetailsModel dm = new DetailsModel();

            var currentrace = dbContext.tb_races.Where(x => x.RaceGUID == raceguid).FirstOrDefault();

            if (currentrace != null)
            {
                dm.RaceGUID = currentrace.RaceGUID;
                dm.HasStarted = currentrace.HasStarted;
                dm.IsFinished = currentrace.HasCompleted;
                dm.RaceName = "#" + currentrace.RaceGUID.ToString().Substring(0, 8);
                dm.UTCStartTime = GlobalHelpers.ConvertToUTC(currentrace.StartDateTime);
                dm.RaceMode = dbContext.tb_flags.Where(x => x.flag_id == currentrace.flag_id).Select(x => x.FlagName).FirstOrDefault();
                dm.RacerCount = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id).Count();
                dm.GrabBag = (currentrace.grabbag_id != null ? " [" + dbContext.tb_flags.Where(x => x.flag_id == currentrace.grabbag_id).Select(x => x.FlagName).FirstOrDefault() + "]" : "");
                dm.SeedURL = currentrace.SeedURL;
                dm.SpoilerHash = (dbContext.tb_flags.Where(x => x.flag_id == currentrace.flag_id).Select(x => x.IsSpoiler).FirstOrDefault() == true ? dbContext.tb_spoilers.Where(x => x.race_id == currentrace.race_id).Select(x => x.SpoilerHash).FirstOrDefault() : "");
                dm.IsMystery = currentrace.flag_id == 6 || currentrace.flag_id == 44;
                if (dm.IsMystery)
                {
                    if (currentrace.MysteryJSON != null)
                    {
                        string jsonString = currentrace.MysteryJSON.Replace(".", "");
                        dynamic mysteryObject = JsonConvert.DeserializeObject<dynamic>(jsonString);

                        dm.MysteryDetails = string.Format("World State: {0}\r\n", CamelCase(mysteryObject.mode.ToString()));

                        dm.MysteryDetails += "Goal: ";

                        switch (mysteryObject.goal.ToString())
                        {
                            case "ganon":
                                dm.MysteryDetails += "Ganon\r\n";
                                break;
                            case "fast_ganon":
                                dm.MysteryDetails += "Fast Ganon\r\n";
                                break;
                            case "dungeons":
                                dm.MysteryDetails += "All Dungeons\r\n";
                                break;
                            case "pedestal":
                                dm.MysteryDetails += "Pedestal\r\n";
                                break;
                            case "triforce-hunt":
                                dm.MysteryDetails += "Triforce Hunt\r\n";
                                break;
                        }

                        dm.MysteryDetails += "Dungeon Items: ";

                        switch (mysteryObject.dungeon_items.ToString())
                        {
                            case "standard":
                                dm.MysteryDetails += "Standard\r\n";
                                break;
                            case "mc":
                                dm.MysteryDetails += "MC Shuffle\r\n";
                                break;
                            case "mcs":
                                dm.MysteryDetails += "MCS Shuffle\r\n";
                                break;
                            case "full":
                                dm.MysteryDetails += "Keysanity\r\n";
                                break;
                        }

                        dm.MysteryDetails += string.Format("Swords: {0}\r\n", CamelCase(mysteryObject.weapons.ToString()));


                        dm.MysteryDetails += "Accessibility: ";

                        switch (mysteryObject.accessibility.ToString())
                        {
                            case "items":
                                dm.MysteryDetails += "100% Inventory\r\n";
                                break;
                            case "none":
                                dm.MysteryDetails += "Beatable\r\n";
                                break;
                        }

                        dm.MysteryDetails += string.Format("Tower Open: {0}\r\n", mysteryObject.crystals.tower.ToString());

                        dm.MysteryDetails += string.Format("Ganon Vulnerable {0}\r\n", mysteryObject.crystals.ganon.ToString());

                        dm.MysteryDetails += string.Format("Item Pool: {0}\r\n", CamelCase(mysteryObject.item.pool.ToString()));

                        bool IsCustomizer = (mysteryObject.entrances == null || mysteryObject.entrances == "none" ? true : false);

                        //Check for customizer or not
                        if (IsCustomizer)
                        {
                            dm.MysteryDetails += "Entrance Shuffle: None\r\n";
                        }
                        else
                        {
                            dm.MysteryDetails += string.Format("Entrance Shuffle: {0}\r\n", CamelCase(mysteryObject.entrances.ToString()));
                        }

                        if (!IsCustomizer)
                        {
                            dm.MysteryDetails += "OHKO: No\r\n";
                        }
                        else
                        {
                            if (mysteryObject.custom == null)
                            {
                                dm.MysteryDetails += "OHKO: No\r\n";
                            }
                            else
                            {
                                dm.MysteryDetails += string.Format("OHKO: {0}\r\n", mysteryObject.custom.romtimerMode.ToString() == "off" ? "No" : "Yes");
                            }

                        }

                        dm.MysteryDetails += string.Format("Boss Shuffle: {0}\r\n", CamelCase(mysteryObject.enemizer.boss_shuffle.ToString()));

                        dm.MysteryDetails += string.Format("Enemy Shuffle: {0}\r\n", CamelCase(mysteryObject.enemizer.enemy_shuffle.ToString()));

                        dm.MysteryDetails += string.Format("Enemy Health: {0}\r\n", CamelCase(mysteryObject.enemizer.enemy_health.ToString()));

                        dm.MysteryDetails += string.Format("Enemy Damage: {0}\r\n", CamelCase(mysteryObject.enemizer.enemy_damage.ToString()));

                        dm.MysteryDetails += string.Format("Hints: {0}\r\n", CamelCase(mysteryObject.hints.ToString()));

                        if (IsCustomizer && mysteryObject.eq != null)
                        {
                            dm.MysteryDetails += string.Format("Starting Boots: {0}\r\n", mysteryObject.eq.ToString().Contains("PegasusBoots") ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Starting Flute: {0}\r\n", mysteryObject.eq.ToString().Contains("Ocarina") ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Starting Hookshot: {0}\r\n", mysteryObject.eq.ToString().Contains("Hookshot") ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Starting Fire Rod: {0}\r\n", mysteryObject.eq.ToString().Contains("FireRod") ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Starting Ice Rod: {0}\r\n", mysteryObject.eq.ToString().Contains("IceRod") ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Starting Flippers: {0}\r\n", mysteryObject.eq.ToString().Contains("Flippers") ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Starting Mirror: {0}\r\n", mysteryObject.eq.ToString().Contains("Mirror") ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Wild Maps: {0}\r\n", mysteryObject.custom.regionwildMaps.ToString() == "True" ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Wild Compasses: {0}\r\n", mysteryObject.custom.regionwildCompasses.ToString() == "True" ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Wild Keys: {0}\r\n", mysteryObject.custom.regionwildKeys.ToString() == "True" ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Wild Big Keys: {0}\r\n", mysteryObject.custom.regionwildBigKeys.ToString() == "True" ? "Yes" : "No");
                            dm.MysteryDetails += string.Format("Retro Keys: {0}\r\n", mysteryObject.custom.romgenericKeys.ToString() == "True" ? "Yes" : "No");
                        }
                        else
                        {
                            dm.MysteryDetails += string.Format("Starting Boots: No\r\n");
                            dm.MysteryDetails += string.Format("Starting Flute: No\r\n");
                            dm.MysteryDetails += string.Format("Starting Hookshot: No\r\n");
                            dm.MysteryDetails += string.Format("Starting Fire Rod: No\r\n");
                            dm.MysteryDetails += string.Format("Starting Ice Rod: No\r\n");
                            dm.MysteryDetails += string.Format("Starting Flippers: No\r\n");
                            dm.MysteryDetails += string.Format("Starting Mirror: No\r\n");
                            dm.MysteryDetails += string.Format("Wild Maps: No\r\n");
                            dm.MysteryDetails += string.Format("Wild Compasses: No\r\n");
                            dm.MysteryDetails += string.Format("Wild Keys: No\r\n");
                            dm.MysteryDetails += string.Format("Wild Big Keys: No\r\n");
                            dm.MysteryDetails += string.Format("Retro Keys: No\r\n");
                        }
                    }
                    else
                    {
                        dm.MysteryDetails += "Mystery details are unavailable";
                    }
                }
                else
                {
                    dm.MysteryDetails = "";
                }

                dm.Standings = GetRaceStandings(currentrace.race_id);
            }

            return View(dm);
        }

        private List<Standings> GetRaceStandings(int race_id)
        {
            List<Standings> srlist = new List<Standings>();

            var currentrace = dbContext.tb_races.Where(x => x.race_id == race_id).FirstOrDefault();

            if (currentrace != null)
            {
                CurrentRace cr = new CurrentRace();

                cr.RaceName = "#" + currentrace.RaceGUID.ToString().Substring(0, 8);
                cr.RaceMode = dbContext.tb_flags.Where(x => x.flag_id == currentrace.flag_id).Select(x => x.FlagName).FirstOrDefault();
                cr.RaceGUID = currentrace.RaceGUID;
                cr.RacerCount = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id).Count();
                cr.CurrentlyRacing = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id && x.FinishTime == 0).Count();
                cr.TimerTicks = currentrace.RaceStartTime == null ? 0 : (int)(DateTime.Now - (DateTime)currentrace.RaceStartTime).TotalSeconds;

                var entrantlist = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id).ToList();

                cr.CurrentRaceStandings = entrantlist.Select(x => new CurrentRaceStandings()
                {
                    RacerName = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerName).FirstOrDefault(),
                    RacerGUID = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerGUID).FirstOrDefault(),
                    FinishTime = "",
                    FinishSeconds = dbContext.tb_entrants.Where(y => y.racer_id == x.racer_id && x.race_id == y.race_id).Select(y => y.FinishTime).FirstOrDefault(),
                    FinishPlace = 0,
                    Ranking = dbContext.tb_rankings.Where(y => y.racer_id == x.racer_id && y.flag_id == currentrace.flag_id).OrderByDescending(y => y.LastUpdated).Select(y => y.Ranking).FirstOrDefault()
                }).OrderBy(x => x.FinishSeconds).ThenByDescending(x => x.Ranking).ToList();

                for (int i = 0; i < cr.CurrentRaceStandings.Count; i++)
                {
                    if (cr.CurrentRaceStandings[i].FinishSeconds == 0)
                    {
                        cr.CurrentRaceStandings[i].FinishPlace = 999;
                        cr.CurrentRaceStandings[i].FinishTime = cr.CurrentRaceStandings[i].Ranking.ToString();
                    }
                    else
                    {
                        cr.CurrentRaceStandings[i].FinishPlace = cr.CurrentRaceStandings.Where(x => x.FinishSeconds != 0 && x.FinishSeconds < cr.CurrentRaceStandings[i].FinishSeconds).Count() + 1;
                        cr.CurrentRaceStandings[i].FinishTime = Helpers.GlobalHelpers.ConvertToTime(cr.CurrentRaceStandings[i].FinishSeconds);
                    }

                    if (cr.CurrentRaceStandings[i].Ranking == 0)
                    {
                        cr.CurrentRaceStandings[i].Ranking = 1000;
                    }
                }

                cr.ActiveStream = cr.CurrentRaceStandings.Where(x => x.FinishPlace == 999).OrderByDescending(x => x.FinishTime).Select(x => x.RacerStream).FirstOrDefault();

                if (!currentrace.HasCompleted)
                {
                    foreach (CurrentRaceStandings crs in cr.CurrentRaceStandings)
                    {
                        Standings s = new Standings();

                        s.RacerName = crs.RacerName;
                        s.RacerGUID = crs.RacerGUID;
                        s.Ranking = crs.Ranking;
                        if (crs.FinishSeconds > 0)
                        {
                            s.FinishTime = GlobalHelpers.ConvertToTime(crs.FinishSeconds);
                            s.Result = crs.FinishPlace;
                            s.FinishPlace = crs.FinishSeconds < 88888 ? GlobalHelpers.ConvertToPlace(crs.FinishPlace) : crs.FinishSeconds == 99999 ? "FF" : "DQ";
                        }
                        else
                        {
                            s.Result = 999;
                            s.FinishTime = "RACING";
                            s.FinishPlace = "";
                        }

                        srlist.Add(s);
                    }

                    return srlist.OrderBy(x => x.Result).ToList();
                }

                var rankingslist = dbContext.tb_rankings.Where(x => x.race_id == race_id).ToList();

                return rankingslist.Select(x => new Standings()
                {
                    RacerName = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerName).FirstOrDefault(),
                    RacerGUID = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerGUID).FirstOrDefault(),
                    FinishTime = GlobalHelpers.ConvertToTime(dbContext.tb_entrants.Where(y => y.racer_id == x.racer_id && x.race_id == y.race_id).Select(y => y.FinishTime).FirstOrDefault()),
                    Result = x.Result,
                    FinishPlace = GlobalHelpers.ConvertToPlace(x.Result),
                    Ranking = x.Ranking == 0 ? 1000 : x.Ranking,
                    Change = x.Change,
                    Comment = dbContext.tb_entrants.Where(y => y.racer_id == x.racer_id && y.race_id == x.race_id).Select(y => y.Comments).FirstOrDefault()
                }).OrderBy(x => x.Result).ToList();
            }

            return null;
        }

        private string CamelCase(string input)
        {
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public class DetailsModel
        {
            public Guid RaceGUID { get; set; }
            public bool IsFinished { get; set; }
            public bool HasStarted { get; set; }
            public string RaceName { get; set; }
            public int UTCStartTime { get; set; }
            public string RaceMode { get; set; }
            public int RacerCount { get; set; }
            public string SeedURL { get; set; }
            public string SpoilerHash { get; set; }
            public string GrabBag { get; set; }
            public bool IsMystery { get; set; }
            public string MysteryDetails { get; set; }
            public List<Standings> Standings { get; set; }
        }

        public class Standings
        {
            public int Result { get; set; }
            public Guid RacerGUID { get; set; }
            public string RacerName {  get; set; }
            public string FinishTime { get; set; }
            public string FinishPlace { get; set; }
            public int Ranking { get; set; }
            public int Change { get; set; }
            public string Comment { get; set; }
        }

        public class CalendarModel
        {
            public int Week { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
            public List<CalendarWeek> CalendarWeek { get; set; }
            public List<CalendarRace> CalendarRace { get; set; }
            public string RaceJSON { get; set; }

        }

        public class CalendarRace
        {
            public string RaceName { get; set; }
            public string FlagName { get; set; }
            public Guid RaceGUID { get; set; }
            public int UTCStartTicks { get; set; }
            public bool HasCompleted { get; set; }
            public string IsChampionship { get; set; }

        }

        public class CalendarWeek
        {
            public DateTime Day { get; set; }
        }
    }

    //public class CalendarModel
    //{
    //    public string CurrentMonthYear { get; set; }
    //    public int CurrentMonthInt { get; set; }
    //    public int CurrentYearInt { get; set; }
    //    public string PrevMonthYear { get; set; }
    //    public string PrevMonth { get; set; }
    //    public string NextMonthYear { get; set; }
    //    public string NextMonth { get; set; }
    //    public List<CalendarDay> CalendarDays { get; set; }

    //}

    //public class CalendarDay
    //{
    //    public bool IsVisible { get; set; }
    //    public bool IsToday { get; set; }
    //    public int Day { get; set; }
    //    public List<CalendarModes> Races { get; set; }
    //}

    //public class CalendarModes
    //{
    //    public string RaceText { get; set; }
    //    public string BackgroundClass { get; set; }
    //    public int race_id { get; set; }
    //}

    //public class ScheduleModel
    //{
    //    public List<ScheduleObject> Schedule { get; set; }
    //    public DateTime CurrentDay { get; set; }
    //}

    //public class ScheduleObject
    //{
    //    public DateTime StartTime { get; set; }
    //    public string Flagset { get; set; }
    //    public string RaceName { get; set; }
    //    public bool HasBeenCompleted { get; set; }
    //    public int race_id { get; set; }

    //}

    //public class DetailsModel
    //{
    //    public List<DetailsObject> Details { get; set; }
    //    public string Flagset { get; set; }
    //    public string RaceChannel { get; set; }
    //    public DateTime StartDateTime { get; set; }
    //    public bool IsMystery { get; set; }
    //    public bool IsSpoiler { get; set; }
    //    public bool IsGrabBag { get; set; }

    //}

    //public class DetailsObject
    //{
    //    public string Result { get; set; }
    //    public string Racer1Name { get; set; }
    //    public string Racer1Time { get; set; }
    //    public int racer1_id { get; set; }
    //    public string Racer2Name { get; set; }
    //    public string Racer2Time { get; set; }
    //    public int racer2_id { get; set; }
    //    public string Seed { get; set; }
    //    public string MysteryDetails { get; set; }
    //    public string Racer1Comment { get; set; }
    //    public string Racer2Comment { get; set; }
    //    public string Spoiler { get; set; }
    //    public string Multistream { get; set; }
    //    public string GrabBagMode { get; set; }
    //}

    //public class SeedDetailsObject
    //{
    //    public string SeedHash { get; set; }
    //    public string Result { get; set; }
    //    public string RaceDetails { get; set; }

    //}

}