using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using ALTTPR_Ladder.Helpers;
using System.Data;

namespace ALTTPR_Ladder.Controllers
{
    public class RacersController : Controller
    {
        public ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();

        // GET: Schedule
        public ActionResult Index(string id)
        {
            Helpers.GlobalHelpers.BrowserLogging("Racers", Request.UserHostAddress.ToString(), id.ToString());

            RacerModel rm = new RacerModel();

            Guid racerguid = Guid.Parse(id);

            var racer = dbContext.tb_racers.Where(x => x.RacerGUID == racerguid).FirstOrDefault();

            if (racer != null)
            {
                rm.RacerName = racer.RacerName;
                rm.RacerGUID = racer.RacerGUID;
                rm.old_racer_id = racer.old_racer_id;
                rm.StreamURL = racer.StreamURL;
                rm.Rankings = new List<RacerRanking>();

                List<int> flaglist = dbContext.tb_rankings_cache.Where(x => x.racer_id == racer.racer_id).Select(x => x.flag_id).Distinct().ToList();
                var totalraces = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.FinishTime != 0 && x.FinishTime != 99999 && x.FinishTime != 88888).ToList();

                foreach (int f in flaglist)
                {
                    var lastranking = dbContext.tb_rankings_cache.Where(x => x.racer_id == racer.racer_id && x.flag_id == f).OrderByDescending(x => x.LastUpdated).FirstOrDefault();

                    RacerRanking rr = new RacerRanking();

                    rr.FlagName = dbContext.tb_flags.Where(x => x.flag_id == lastranking.flag_id).Select(x => x.FlagName).FirstOrDefault();
                    rr.flag_id = dbContext.tb_flags.Where(x => x.flag_id == lastranking.flag_id).Select(x => x.flag_id).FirstOrDefault();
                    rr.Ranking = lastranking.Ranking;
                    rr.Rank = lastranking.Rank;
                    rr.TotalRaces = lastranking.TotalRaces;
                    rr.Firsts = lastranking.Firsts;
                    rr.Seconds = lastranking.Seconds;
                    rr.Thirds = lastranking.Thirds;
                    rr.Forfeits = lastranking.Forfeits;

                    var allmoderaceids = dbContext.tb_races.Where(x => x.flag_id == f).Select(x => x.race_id).ToList();

                    var allraces = totalraces.Where(x => allmoderaceids.Contains(x.race_id)).OrderBy(x => x.FinishTime).ToList();

                    rr.Fastest = (allraces.Count > 0) ? GlobalHelpers.ConvertToTime(allraces[0].FinishTime) : "0";
                    int fastestid = (allraces.Count > 0) ? allraces[0].race_id : 0;
                    rr.FastestGUID = (allraces.Count > 0) ? dbContext.tb_races.Where(x => x.race_id == fastestid).Select(x => x.RaceGUID).FirstOrDefault() : new Guid();
                    rr.Average = (allraces.Count > 0) ? GlobalHelpers.ConvertToTime(allraces.Sum(x => x.FinishTime) / allraces.Count) : "0";

                    rm.Rankings.Add(rr);
                }

                rm.Rankings = rm.Rankings.OrderBy(x => x.FlagName).ToList();

                RacerRanking rr2 = new RacerRanking();

                rr2.FlagName = "Total";
                rr2.flag_id = 0;
                rr2.Ranking = 0;
                rr2.Rank = 0;
                rr2.TotalRaces = rm.Rankings.Sum(x => x.TotalRaces);
                rr2.Firsts = rm.Rankings.Sum(x => x.Firsts);
                rr2.Seconds = rm.Rankings.Sum(x => x.Seconds);
                rr2.Thirds = rm.Rankings.Sum(x => x.Thirds);
                rr2.Forfeits = rm.Rankings.Sum(x => x.Forfeits);
                rr2.Fastest = "0";
                rr2.Average = "0";

                rm.Rankings.Add(rr2);
            }

            return View(rm);
        }

        public ActionResult RaceStats(string r, int f)
        {
            var rs = new RacerStats();

            rs.FlagName = dbContext.tb_flags.Where(x => x.flag_id == f).Select(x => x.FlagName).FirstOrDefault();

            Guid racerguid = Guid.Parse(r);

            var racer = dbContext.tb_racers.Where(x => x.RacerGUID == racerguid).FirstOrDefault();

            var rankinglist = dbContext.tb_rankings.Where(x => x.flag_id == f && x.racer_id == racer.racer_id).OrderBy(x => x.LastUpdated).ToList();

            rs.RankingsHistory = rankinglist.Select(x => new RankingsHistory()
            {
                race_id = x.race_id,
                RaceName = x.race_id == 0 ? "" : dbContext.tb_races.Where(y => y.race_id == x.race_id).Select(y => y.RaceGUID.ToString().Substring(0, 8)).FirstOrDefault(),
                RaceGUID = x.race_id == 0 ? new Guid() : dbContext.tb_races.Where(y => y.race_id == x.race_id).Select(y => y.RaceGUID).FirstOrDefault(),
                TotalCount = dbContext.tb_entrants.Where(y => y.race_id == x.race_id).Count(),
                FinishTime = GlobalHelpers.ConvertToTime(dbContext.tb_entrants.Where(y => y.race_id == x.race_id && y.racer_id == x.racer_id).Select(y => y.FinishTime).FirstOrDefault()),
                Ranking = x.Ranking,
                Change = x.Change,
                Result = GlobalHelpers.ConvertToPlace(x.Result),
                UTCStartTime = x.race_id == -1 ? GlobalHelpers.ConvertToUTC(x.LastUpdated) :  GlobalHelpers.ConvertToUTC(dbContext.tb_races.Where(y => y.race_id == x.race_id).Select(y => y.StartDateTime).FirstOrDefault())
            }).ToList();

            return View(rs);
        }

        //[HttpPost]
        //public JsonResult RaceChart(string r, int f)
        //{


        //    List<object> iData = new List<object>();
        //    DataTable dt = new DataTable();
        //    dt.Columns.Add("Race Name", System.Type.GetType("System.String"));
        //    dt.Columns.Add("Rating", System.Type.GetType("System.Int32"));
        //    DataRow dr = dt.NewRow();
        //    dr["Employee"] = "Sam";
        //    dr["Rating"] = 123;
        //    dt.Rows.Add(dr);
        //    dr = dt.NewRow();
        //    dr["Employee"] = "Alex";
        //    dr["Rating"] = 456;
        //    dt.Rows.Add(dr);
        //    dr = dt.NewRow();
        //    dr["Employee"] = "Michael";
        //    dr["Rating"] = 587;
        //    dt.Rows.Add(dr);
        //    foreach (DataColumn dc in dt.Columns)
        //    {
        //        List<object> x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
        //        iData.Add(x);
        //    }
        //    // Source data returned as JSON
        //    return Json(iData, JsonRequestBehavior.AllowGet);
        //}

        // GET: Schedule
        public ActionResult Stream(string id)
        {
            Helpers.GlobalHelpers.BrowserLogging("Race Stream", Request.UserHostAddress.ToString(), id.ToString());

            Guid racerguid = Guid.Parse(id);

            var racer = dbContext.tb_racers.Where(x => x.RacerGUID == racerguid).FirstOrDefault();

            if (racer != null)
            {
                return Redirect(racer.StreamURL);
            }

            return View();
        }


        public class RacerModel
        {
            public string RacerName { get; set; }
            public Guid RacerGUID { get; set; }
            public int? old_racer_id {  get; set; }
            public string StreamURL { get; set; }
            public List<RacerRanking> Rankings { get; set; }
        }

        public class RacerRanking
        {
            public string FlagName { get; set; }
            public int flag_id { get; set; }
            public int Ranking { get; set; }
            public int Rank { get; set; }
            public int TotalRaces { get; set; }
            public int Firsts { get; set; }
            public int Seconds { get; set; }
            public int Thirds { get; set; }
            public int Forfeits { get; set; }
            public string Average { get; set; }
            public string Fastest { get; set; }
            public Guid? FastestGUID { get; set; }
            public List<RankingsHistory> History { get; set; }
        }

        public class RacerStats
        {
            public string FlagName { get; set; }
            public List<RankingsHistory> RankingsHistory { get; set; }
        }

        public class RankingsHistory
        {
            public int race_id { get; set; }
            public string RaceName { get; set; }
            public Guid RaceGUID { get; set; }
            public int TotalCount { get; set; }
            public string FinishTime { get; set; }
            public int Ranking { get; set; }
            public int Change { get; set; }
            public string Result { get; set; }
            public int UTCStartTime { get; set; }
        }
    }
}