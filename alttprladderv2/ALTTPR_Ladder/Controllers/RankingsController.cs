using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using static ALTTPR_Ladder.LadderAPIController;

namespace ALTTPR_Ladder.Controllers
{
    public class RankingsController : Controller
    {
        public ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();

        // GET: Rankings
        public ActionResult Index(int id = 0)
        {
            Helpers.GlobalHelpers.BrowserLogging("Rankings", Request.UserHostAddress.ToString(), id.ToString());

            RankingModel rankmodel = new RankingModel();

            List<int> flaglist = dbContext.tb_rankings_cache.Select(x => x.flag_id).Distinct().ToList();

            rankmodel.Rankings = new List<RankingObject>();
            rankmodel.Modes = new List<RankingModes>();

            foreach (int f in flaglist)
            {
                RankingModes mode = new RankingModes();
                mode.flag_id = f;
                mode.FlagName = dbContext.tb_flags.Where(x => x.flag_id == f).Select(x => x.FlagName).FirstOrDefault();
                mode.LastUpdated = dbContext.tb_rankings_cache.Where(x => x.flag_id == f).OrderByDescending(x => x.LastUpdated).Select(x => x.LastUpdated).FirstOrDefault();

                rankmodel.Modes.Add(mode);
            }

            if (rankmodel.Modes.Count > 0)
            {
                rankmodel.Modes = rankmodel.Modes.OrderBy(x => x.FlagName).ToList();

                if (id == 0)
                {
                    id = rankmodel.Modes[0].flag_id;
                }
            }

            rankmodel.ActiveMode = id;

            var cachedrankings = dbContext.tb_rankings_cache.Where(x => x.flag_id == id).OrderBy(x => x.Rank).ToList();

            rankmodel.Rankings = cachedrankings.Select(x => new RankingObject()
            {
                RacerGUID = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerGUID).FirstOrDefault(),
                RacerName = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerName).FirstOrDefault(),
                Ranking = x.Ranking,
                Rank = x.Rank,
                GainLoss = x.GainLoss,
                TotalRaces = x.TotalRaces,
                Firsts = x.Firsts,
                Seconds = x.Seconds,
                Thirds = x.Thirds,
                Forfeits = x.Forfeits
            }).ToList();

            if (cachedrankings.Count > 0)
            {
                rankmodel.LastUpdated = cachedrankings[0].LastUpdated;
            }

            return View(rankmodel);
        }
        public class RankingModel
        {
            public List<RankingObject> Rankings { get; set; }
            public List<RankingModes> Modes { get; set; }
            public int ActiveMode { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        public class RankingModes
        {
            public int flag_id { get; set; }
            public string FlagName { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        public class RankingObject
        {
            public Guid RacerGUID { get; set; }
            public string RacerName { get; set; }
            public int Ranking { get; set; }
            public int Rank { get; set; }
            public int GainLoss { get; set; }
            public int TotalRaces { get; set; }
            public int Firsts { get; set; }
            public int Seconds { get; set; }
            public int Thirds { get; set; }
            public int Forfeits { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}