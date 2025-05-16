using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using static ALTTPR_Ladder.LadderAPIController;

namespace ALTTPR_Ladder.Controllers
{
    public class HomeController : Controller
    {
        public ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();

        public ActionResult Index()
        {
            Helpers.GlobalHelpers.BrowserLogging("Home", Request.UserHostAddress.ToString());

            IndexModel imodel = new IndexModel();
            imodel.CurrentRace = GetCurrentRace();
            imodel.Standings = GetStandings();
            imodel.NextRaces = GetNextRaces();

            return View(imodel);
        }

        public ActionResult Error()
        {
            Helpers.GlobalHelpers.BrowserLogging("Error", Request.UserHostAddress.ToString());

            return View();
        }

        public ActionResult SwitchStream(string id)
        {
            Helpers.GlobalHelpers.BrowserLogging("Stream", Request.UserHostAddress.ToString(), id);

            return View();
        }

        public ActionResult LiveStandings()
        {
            return View(GetCurrentRace());
        }

        public ActionResult LiveStandingsPopout(string id)
        {
            Helpers.GlobalHelpers.BrowserLogging("Standings Popout", Request.UserHostAddress.ToString(), id);

            return View(GetCurrentRace(id));
        }

        public ActionResult LoadStandings(string id)
        {
            CurrentRace cr = new CurrentRace();

            cr.RaceGUID = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(id)).Select(x => x.RaceGUID).FirstOrDefault();

            return View(cr);
        }

        private CurrentRace GetCurrentRace(string id = "")
        {
            DateTime signuptime = DateTime.Now.AddMinutes(30);

            var currentrace = dbContext.tb_races.Where(x => x.StartDateTime < signuptime && x.HasCompleted == false).OrderBy(x => x.StartDateTime).FirstOrDefault();

            if (id != "")
            {
                Guid guid = Guid.Parse(id);
                currentrace = dbContext.tb_races.Where(x => x.RaceGUID == guid).FirstOrDefault();
            }

            if (currentrace != null)
            {
                CurrentRace cr = new CurrentRace();

                cr.RaceName = "#" + currentrace.RaceGUID.ToString().Substring(0, 8);
                cr.RaceMode = dbContext.tb_flags.Where(x => x.flag_id == currentrace.flag_id).Select(x => x.FlagName).FirstOrDefault();
                cr.RaceGUID = currentrace.RaceGUID;
                cr.RacerCount = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id).Count();
                cr.CurrentlyRacing = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id && x.FinishTime == 0).Count();
                cr.TimerTicks = currentrace.RaceStartTime == null ? 0 : (int)(DateTime.Now - (DateTime)currentrace.RaceStartTime).TotalSeconds;
                cr.StartDate = currentrace.flag_id == 14 && currentrace.RaceStartTime != null ? ((DateTime)currentrace.RaceStartTime).AddMinutes(15) : currentrace.RaceStartTime;
                cr.UTCStartTicks = currentrace.RaceStartTime == null || currentrace.HasCompleted ? 0 : Helpers.GlobalHelpers.ConvertToUTC((DateTime)currentrace.RaceStartTime);
                var entrantlist = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id).ToList();

                cr.CurrentRaceStandings = entrantlist.Select(x => new CurrentRaceStandings()
                {
                    RacerName = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerName).FirstOrDefault(),
                    RacerGUID = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerGUID).FirstOrDefault(),
                    FinishTime = "",
                    FinishSeconds = dbContext.tb_entrants.Where(y => y.racer_id == x.racer_id && x.race_id == y.race_id).Select(y => y.FinishTime).FirstOrDefault(),
                    FinishPlace = 0,
                    Ranking = dbContext.tb_rankings.Where(y => y.racer_id == x.racer_id && y.flag_id == currentrace.flag_id).OrderByDescending(y => y.LastUpdated).Select(y => y.Ranking).FirstOrDefault(),
                    RacerStream = ParseRacerStream(dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.StreamURL).FirstOrDefault()),
                    Change = currentrace.HasCompleted ? dbContext.tb_rankings.Where(y => y.racer_id == x.racer_id && y.race_id == currentrace.race_id).Select(y => y.Change).FirstOrDefault() : 0
                }).OrderBy(x => x.FinishSeconds).ThenByDescending(x => x.Ranking).ToList();

                for (int i = 0; i < cr.CurrentRaceStandings.Count; i++)
                {
                    if (cr.CurrentRaceStandings[i].FinishSeconds == 0)
                    {
                        cr.CurrentRaceStandings[i].FinishPlace = 999;
                        cr.CurrentRaceStandings[i].FinishTime = "RACING";
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

                cr.CurrentRaceStandings = cr.CurrentRaceStandings.OrderBy(x => x.FinishPlace).ThenByDescending(x => x.Ranking).ToList();

                //cr.ActiveStream = cr.CurrentRaceStandings.Where(x => x.FinishPlace == 999).OrderBy(x => x.FinishPlace).Select(x => x.RacerStream).FirstOrDefault();

                return cr;
            }

            return null;
        }

        private List<TopStandings> GetStandings()
        {
            var flaglist = dbContext.tb_rankings_cache.Select(x => x.flag_id).Distinct().ToList();

            flaglist.Sort();

            List<TopStandings> tslist = new List<TopStandings>();

            foreach (int f in flaglist)
            {
                TopStandings ts = new TopStandings();

                ts.flag_id = f;
                ts.RaceMode = dbContext.tb_flags.Where(x => x.flag_id == f).Select(x => x.FlagName).FirstOrDefault();
                ts.RacerStandings = dbContext.tb_rankings_cache.Where(x => x.flag_id == f).OrderBy(x => x.Rank).Select(y => new RacerStandings()
                {
                    Racer = dbContext.tb_racers.Where(z => z.racer_id == y.racer_id).Select(z => z.RacerName).FirstOrDefault(),
                    Rank = y.Rank,
                    Ranking = y.Ranking,
                    RacerGUID = dbContext.tb_racers.Where(z => z.racer_id == y.racer_id).Select(z => z.RacerGUID).FirstOrDefault(),
                    Firsts = y.Firsts,
                    Seconds = y.Seconds,
                    Thirds = y.Thirds
                }).Take(10).ToList();

                tslist.Add(ts);
            }

            tslist = tslist.OrderBy(x => x.RaceMode).ToList();

            return tslist;
        }

        private List<NextRaces> GetNextRaces()
        {
            var racelist = dbContext.tb_races.Where(x => x.HasCompleted == false && x.HasBeenProcessed == false).OrderBy(x => x.StartDateTime).Take(12).ToList();

            return racelist.Select(x => new NextRaces()
            {
                FlagName = dbContext.tb_flags.Where(y => y.flag_id == x.flag_id).Select(y => y.FlagName).FirstOrDefault(),
                flag_id = x.flag_id,
                RaceGUID = x.RaceGUID,
                StartTime = x.StartDateTime,
                UTCTime = Helpers.GlobalHelpers.ConvertToUTC(x.StartDateTime),
                Status = (x.HasStarted == true ? "IN PROGRESS" : (x.StartDateTime <= DateTime.Now.AddMinutes(10) ? "PRERACE" : (x.StartDateTime <= DateTime.Now.AddMinutes(30) ? "SIGNUPS" : "SCHEDULED")))
            }).OrderBy(x => x.StartTime).ToList();
        }

        private string ParseRacerStream(string streamurl)
        {
            return streamurl != null ? streamurl.Substring(streamurl.LastIndexOf('/') + 1) : "";
        }



        //public List<NextFourRaces> GetNextFourRaces()
        //{
        //    List<NextFourRaces> nextfour = new List<NextFourRaces>();

        //    List<tb_races> racelist = dbContext.tb_races.Where(x => x.HasCompleted == false).OrderBy(x => x.StartDateTime).Take(10).ToList();

        //    foreach (tb_races r in racelist)
        //    {
        //        NextFourRaces nfr = new NextFourRaces();

        //        nfr.race_id = r.race_id;
        //        nfr.StartTime = r.StartDateTime;
        //        nfr.UTCTime = r.StartDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mmZ");
        //        nfr.Flagset = dbContext.tb_flags.Where(x => x.flag_id == r.flag_id).Select(x => x.FlagName).FirstOrDefault();
        //        if (r.HasStarted == true)
        //        {
        //            nfr.Status = "In Progress";
        //        }
        //        else
        //        {
        //            if (dbContext.tb_pairings.Where(x => x.race_id == r.race_id).Count() > 0)
        //            {
        //                nfr.Status = "Pre-Race";
        //            }
        //            else if (dbContext.tb_entrants.Where(x => x.race_id == r.race_id).Count() > 0)
        //            {
        //                nfr.Status = "Signups";
        //            }
        //            else
        //            {
        //                nfr.Status = "Scheduled";
        //            }
        //        }

        //        nextfour.Add(nfr);
        //    }

        //    return nextfour;
        //}

        //public List<TopTenStandings> GetTopTenStandings(int flag_id = 0)
        //{
        //    List<TopTenStandings> topten = new List<TopTenStandings>();

        //    DateTime nowTime = DateTime.Now;

        //    List<vw_rankings_cache> rankinglist = dbContext.vw_rankings_cache.Where(x => x.flag_id == flag_id).OrderBy(x => x.Rank).Take(10).ToList();

        //    foreach (vw_rankings_cache r in rankinglist)
        //    {
        //        TopTenStandings standing = new TopTenStandings();

        //        standing.RacerGUID = r.RacerGUID;
        //        standing.Racer = r.RacerName;
        //        standing.Ranking = r.Ranking;
        //        standing.Rank = r.Rank;

        //        topten.Add(standing);
        //    }

        //    return topten;
        //}
    }

    public class IndexModel
    {
        public CurrentRace CurrentRace { get; set; }
        public List<TopStandings> Standings { get; set; }
        public List<NextRaces> NextRaces { get; set; }
    }

    public class CurrentRace
    {
        public string RaceName { get; set; }
        public string RaceMode { get; set; }
        public Guid RaceGUID { get; set; }
        public int RacerCount { get; set; }
        public int CurrentlyRacing { get; set; }
        public int TimerTicks { get; set; }
        public int UTCStartTicks { get; set; }
        public DateTime? StartDate { get; set; }
        public string ActiveStream { get; set; }
        public List<CurrentRaceStandings> CurrentRaceStandings { get; set; }
    }

    public class CurrentRaceStandings
    {
        public string RacerName { get; set; }
        public Guid RacerGUID { get; set; }
        public string FinishTime { get; set; }
        public int FinishSeconds { get; set; }
        public int FinishPlace { get; set; }
        public string RacerStream { get; set; }
        public int Ranking { get; set; }
        public int Change { get; set; }
    }

    public class TopStandings
    {
        public string RaceMode { get; set; }
        public int flag_id { get; set; }
        public List<RacerStandings> RacerStandings { get; set; }
    }

    public class RacerStandings
    {
        public string Racer { get; set; }
        public int Rank { get; set; }
        public int Ranking { get; set; }
        public int racer_id { get; set; }
        public Guid RacerGUID { get; set; }
        public int Firsts { get; set; }
        public int Seconds { get; set; }
        public int Thirds { get; set; }

    }


    public class NextRaces
    {
        public string FlagName { get; set; }
        public int flag_id { get; set; }
        public DateTime StartTime { get; set; }
        public int UTCTime { get; set; }
        //Scheduled
        //Signups
        //Pre-Race
        //Active
        public string Status { get; set; }
        public Guid RaceGUID { get; set; }
    }
}