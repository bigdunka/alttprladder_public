using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Configuration;
using System.Web;
using System.Runtime.CompilerServices;
using System.Collections;
using ALTTPR_Ladder.Controllers;
using ALTTPR_Ladder.Helpers;

namespace ALTTPR_Ladder
{
    public class PublicAPIController : ApiController
    {
        public ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();

        private void LogRequest(string apirequest)
        {
            string ip = HttpContext.Current.Request.UserHostAddress;

            tb_api_requests req = new tb_api_requests();

            req.IPAddress = ip;
            req.Request = apirequest;
            req.RequestDateTime = DateTime.Now;

            dbContext.tb_api_requests.Add(req);

            dbContext.SaveChanges();
        }

        // api/v1/PublicAPI/
        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage Ping()
        {
            LogRequest("Ping");

            return Request.CreateResponse(HttpStatusCode.OK, "Public API - Pong! - " + DateTime.Now.ToString());
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetActiveRacers()
        {
            LogRequest("GetActiveRacers");

            List<tb_racers> racerlist = dbContext.tb_racers.Where(x => x.IsActive == true).ToList();

            List<PublicRacers> publicracers = new List<PublicRacers>();

            foreach (tb_racers r in racerlist)
            {
                PublicRacers pr = new PublicRacers();

                pr.RacerGUID = r.RacerGUID;
                pr.RacerName = r.RacerName;
                pr.DiscordName = r.RacerLogin;

                publicracers.Add(pr);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(publicracers), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetFlags()
        {
            LogRequest("GetFlags");

            List<tb_flags> flaglist = dbContext.tb_flags.ToList();

            List<PublicFlags> publicflaglist = flaglist.Select(x => new PublicFlags()
            {
                flag_id = x.flag_id,
                FlagName = x.FlagName,
                HoursToComplete = x.HoursToComplete
            }).OrderBy(x => x.flag_id).ToList();

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(publicflaglist), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetRankings(int flag_id = 0)
        {
            LogRequest("GetRankings");

            List<int> flaglist = dbContext.tb_rankings_cache.Select(x => x.flag_id).Distinct().ToList();

            if (flag_id != 0)
            {
                flaglist = new List<int>();

                flaglist.Add(flag_id);
            }

            List<PublicRankingModel> rankinglist = new List<PublicRankingModel>();

            foreach (int flag in flaglist)
            {
                PublicRankingModel rankmodel = new PublicRankingModel();

                rankmodel.FlagName = dbContext.tb_flags.Where(x => x.flag_id == flag).Select(x => x.FlagName).FirstOrDefault();
                rankmodel.flag_id = flag;
                rankmodel.LastUpdated = dbContext.tb_rankings_cache.Where(x => x.flag_id == flag).OrderByDescending(x => x.LastUpdated).Select(x => x.LastUpdated).FirstOrDefault();

                var rankingscache = dbContext.tb_rankings_cache.Where(x => x.flag_id == flag).OrderBy(x => x.Rank).ThenBy(x => x.Ranking).ToList();

                rankmodel.Rankings = rankingscache.Select(x => new PublicRankingObject()
                {
                    RacerGUID = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerGUID).FirstOrDefault(),
                    RacerName = dbContext.tb_racers.Where(y => y.racer_id == x.racer_id).Select(y => y.RacerName).FirstOrDefault(),
                    Ranking = x.Ranking,
                    Rank = x.Rank,
                    Firsts = x.Firsts,
                    Seconds = x.Seconds,
                    Thirds = x.Thirds,
                    Forfeits = x.Forfeits
                }).OrderBy(x => x.Rank).ThenBy(x => x.Ranking).ToList();

                rankinglist.Add(rankmodel);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(rankinglist), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetRacerRankings(string RacerGUID, int flag_id = 0)
        {
            LogRequest("GetRacerRankings");

            List<int> flaglist = dbContext.tb_rankings_cache.Select(x => x.flag_id).Distinct().ToList();

            if (flag_id != 0)
            {
                flaglist = new List<int>();

                flaglist.Add(flag_id);
            }

            List<PublicRacerRanking> rankinglist = new List<PublicRacerRanking>();

            Guid racerguid = Guid.Parse(RacerGUID);

            var racer = dbContext.tb_racers.Where(x => x.RacerGUID == racerguid).FirstOrDefault();

            var moderankinglist = dbContext.tb_rankings_cache.Where(x => x.racer_id == racer.racer_id).ToList();

            foreach (int flag in flaglist)
            {
                PublicRacerRanking prr = new PublicRacerRanking();

                prr.FlagName = dbContext.tb_flags.Where(x => x.flag_id == flag).Select(x => x.FlagName).FirstOrDefault();
                prr.Ranking = moderankinglist.Where(x => x.flag_id == flag).Select(x => x.Ranking).FirstOrDefault();
                prr.Rank = moderankinglist.Where(x => x.flag_id == flag).Select(x => x.Rank).FirstOrDefault();
                prr.Firsts = moderankinglist.Where(x => x.flag_id == flag).Select(x => x.Firsts).FirstOrDefault();
                prr.Seconds = moderankinglist.Where(x => x.flag_id == flag).Select(x => x.Seconds).FirstOrDefault();
                prr.Thirds = moderankinglist.Where(x => x.flag_id == flag).Select(x => x.Thirds).FirstOrDefault();
                prr.Forfeits = moderankinglist.Where(x => x.flag_id == flag).Select(x => x.Forfeits).FirstOrDefault();
                prr.LastUpdated = moderankinglist.Where(x => x.flag_id == flag).Select(x => x.LastUpdated).FirstOrDefault();

                rankinglist.Add(prr);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(rankinglist), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetRacerHistory(string RacerGUID, int flag_id = 0)
        {
            LogRequest("GetRacerHistory");

            List<int> flaglist = dbContext.tb_rankings_cache.Select(x => x.flag_id).Distinct().ToList();

            if (flag_id != 0)
            {
                flaglist = new List<int>();

                flaglist.Add(flag_id);
            }

            List<PublicRacerHistory> matchlist = new List<PublicRacerHistory>();

            Guid racerguid = Guid.Parse(RacerGUID);

            var racer = dbContext.tb_racers.Where(x => x.RacerGUID == racerguid).FirstOrDefault();

            foreach (int flag in flaglist)
            {
                var racerresultslist = dbContext.tb_rankings.Where(x => x.racer_id == racer.racer_id && x.flag_id == flag && x.race_id != 0).ToList();

                PublicRacerHistory prh = new PublicRacerHistory();

                prh.FlagName = dbContext.tb_flags.Where(x => x.flag_id == flag).Select(x => x.FlagName).FirstOrDefault();

                prh.Results = racerresultslist.Select(x => new PublicRacerResult()
                {
                    RaceName = "#" + dbContext.tb_races.Where(y => y.race_id == x.race_id).Select(y => y.RaceGUID.ToString().Substring(0, 8)).FirstOrDefault(),
                    RaceGUID = dbContext.tb_races.Where(y => y.race_id == x.race_id).Select(y => y.RaceGUID).FirstOrDefault(),
                    StartDateTime = dbContext.tb_races.Where(y => y.race_id == x.race_id).Select(y => y.StartDateTime).FirstOrDefault(),
                    FinishTime = dbContext.tb_entrants.Where(y => y.race_id == x.race_id && y.racer_id == x.racer_id).Select(y => y.FinishTime).FirstOrDefault(),
                    Result = GlobalHelpers.ConvertToPlace(x.Result),
                    RacerCount = dbContext.tb_entrants.Where(y => y.race_id == x.race_id).Count(),
                    Ranking = x.Ranking,
                    GainLoss = x.Change
                }).ToList();

                prh.Results = prh.Results.OrderBy(x => x.StartDateTime).ToList();

                matchlist.Add(prh);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(matchlist), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetRacerDetails(string RacerGUID)
        {
            LogRequest("GetRacerDetails");

            PublicRacerDetails prd = new PublicRacerDetails();

            Guid racerguid = Guid.Parse(RacerGUID);

            tb_racers racer = dbContext.tb_racers.Where(x => x.RacerGUID == racerguid && x.IsActive == true).FirstOrDefault();

            if (racer != null)
            {
                prd.RacerName = racer.RacerName;
                prd.RacerTag = racer.RacerLogin;
                prd.RacerStream = racer.StreamURL;
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(prd), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage IsCurrentlyRacing(string RacerGUID)
        {
            LogRequest("IsCurrentlyRacing");

            Guid racerguid = Guid.Parse(RacerGUID);

            tb_racers racer = dbContext.tb_racers.Where(x => x.RacerGUID == racerguid && x.IsActive == true).FirstOrDefault();

            tb_entrants e = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.FinishTime == 0).FirstOrDefault();

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent((e == null ? "false" : "true"), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetCurrentRaceTime()
        {
            LogRequest("GetCurrentRaceTime");

            List<PublicRaceReturn> prrlist = new List<PublicRaceReturn>();

            //DateTime startingdt = DateTime.Now.AddMinutes(-30);

            //var activeraces = dbContext.tb_races.Where(x => x.HasCompleted == false && x.StartDateTime >= startingdt).ToList();

            //foreach (var race in activeraces)
            //{
            //    PublicRaceReturn prr = new PublicRaceReturn();

            //    int datediff = Helpers.GlobalHelpers.ConvertToUTC(DateTime.Now) - Helpers.GlobalHelpers.ConvertToUTC(race.StartDateTime);

            //    prr.RaceGUID = race.RaceGUID;
            //    prr.RaceName = "#" + race.RaceGUID.ToString().Substring(0, 8);
            //    prr.RaceStatus = (datediff < -600 ? "SIGNUPS" : (datediff < 0 ? "PRERACE" : "ACTIVE"));
            //    prr.TotalRacers = dbContext.tb_entrants.Where(x => x.race_id == race.race_id).Count();
            //    prr.ActiveRacers = dbContext.tb_entrants.Where(x => x.race_id == race.race_id && x.FinishTime == 0).Count();
            //    prr.RaceSeconds = datediff;
            //    prr.RaceTimer = ConvertToTime(datediff);

            //    prrlist.Add(prr);
            //}

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(prrlist), Encoding.UTF8, "application/json");
            return response;
        }

        private string ConvertToTime(int CurrentTime)
        {
            bool IsNegative = CurrentTime < 0;

            CurrentTime = Math.Abs(CurrentTime);

            string h;
            string m;
            string s;

            h = (CurrentTime / 3600).ToString();
            m = ((CurrentTime % 3600) / 60).ToString();
            s = (CurrentTime % 60).ToString();

            return string.Format("{0}{1}:{2}:{3}", (IsNegative ? "-" : ""), h, (m.Length == 2 ? m : "0" + m), (s.Length == 2 ? s : "0" + s));
        }




        //[AcceptVerbs("GET", "POST")]
        //[HttpGet]
        //public HttpResponseMessage GetRacerStandings(int racer_id, int? season_id, int? flag_id)
        //{
        //    LogRequest("GetRacerStandings");

        //    vw_rankings_cache ranking = dbContext.vw_rankings_cache.Where(x => x.racer_id == racer_id && x.season_id == season_id && x.flag_id == flag_id).FirstOrDefault();

        //    string jsonreturn = "";

        //    if (ranking != null)
        //    {
        //        PublicRacerRecord racerrecord = new PublicRacerRecord();

        //        racerrecord.RacerName = ranking.Racer;
        //        racerrecord.Season = (season_id == 0 || season_id == null ? "Lifetime" : dbContext.tb_seasons.Where(x => x.season_id == season_id).Select(x => x.SeasonName).FirstOrDefault());
        //        racerrecord.Mode = (flag_id == 0 || flag_id == null ? "Global" : dbContext.tb_flags.Where(x => x.flag_id == flag_id).Select(x => x.FlagName).FirstOrDefault());
        //        racerrecord.Rating = ranking.Ranking;
        //        racerrecord.Rank = ranking.Rank;
        //        racerrecord.Change = ranking.Change;
        //        racerrecord.Wins = ranking.Wins;
        //        racerrecord.Losses = ranking.Losses;
        //        racerrecord.Ties = ranking.Ties;

        //        jsonreturn = JsonConvert.SerializeObject(racerrecord);
        //    }

        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    response.Content = new StringContent(jsonreturn, Encoding.UTF8, "application/json");
        //    return response;
        //}



        //[AcceptVerbs("GET", "POST")]
        //[HttpGet]
        //public HttpResponseMessage GetSchedule(int season_id)
        //{
        //    LogRequest("GetSchedule");

        //    List<tb_races> racelist = new List<tb_races>();

        //    if (season_id == 0)
        //    {
        //        racelist = dbContext.tb_races.OrderBy(x => x.StartDateTime).ToList();
        //    }
        //    else
        //    {
        //        racelist = dbContext.tb_races.Where(x => x.season_id == season_id).OrderBy(x => x.StartDateTime).ToList();
        //    }

        //    List<PublicSchedule> publicschedulelist = racelist.Select(x => new PublicSchedule()
        //    {
        //        race_id = x.race_id,
        //        Season = dbContext.tb_seasons.Where(y => y.season_id == x.season_id).Select(y => y.SeasonName).FirstOrDefault(),
        //        Mode = dbContext.tb_flags.Where(y => y.flag_id == x.flag_id).Select(y => y.FlagName).FirstOrDefault(),
        //        StartTime = x.StartDateTime.ToString("M/d/yy h tt"),
        //        RaceName = x.RaceChannel,
        //        HasCompleted = x.HasCompleted,
        //        ParticipantCount = dbContext.tb_entrants.Where(y => y.race_id == x.race_id).Count()
        //    }).OrderBy(x => x.race_id).ToList();

        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    response.Content = new StringContent(JsonConvert.SerializeObject(publicschedulelist), Encoding.UTF8, "application/json");
        //    return response;
        //}



        //[AcceptVerbs("GET", "POST")]
        //[HttpGet]
        //public HttpResponseMessage GetRacerStats(int racer_id)
        //{
        //    LogRequest("GetRacerStats");

        //    List<PublicStatsReturn> statlist = dbContext.Database.SqlQuery<PublicStatsReturn>("sp_GetRaceStats " + racer_id).ToList();

        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    response.Content = new StringContent(JsonConvert.SerializeObject(statlist), Encoding.UTF8, "application/json");
        //    return response;
        //}



        //[AcceptVerbs("GET", "POST")]
        //[HttpGet]
        //public HttpResponseMessage GetRaceDetails(int race_id)
        //{
        //    LogRequest("GetRaceDetails");

        //    List<tb_pairings> plist = dbContext.tb_pairings.Where(x => x.race_id == race_id && x.Racer1Time != null && x.Racer2Time != null).ToList();

        //    int flag_id = dbContext.tb_races.Where(x => x.race_id == race_id).Select(x => x.flag_id).FirstOrDefault();

        //    List<PublicRaceReturn> racelist = plist.Select(x => new PublicRaceReturn()
        //    {
        //        flag_id = flag_id,
        //        FlagName = dbContext.tb_flags.Where(y => y.flag_id == flag_id).Select(y => y.FlagName).FirstOrDefault(),
        //        racer1_id = x.racer1_id,
        //        Racer1Name = dbContext.tb_racers.Where(y => y.racer_id == x.racer1_id).Select(y => y.RacerName).FirstOrDefault(),
        //        Racer1Time = (int)x.Racer1Time,
        //        racer2_id = x.racer2_id,
        //        Racer2Name = dbContext.tb_racers.Where(y => y.racer_id == x.racer2_id).Select(y => y.RacerName).FirstOrDefault(),
        //        Racer2Time = (int)x.Racer2Time,
        //        grabbag_id = (x.grabbag_id != null ? x.grabbag_id : null)
        //    }).ToList();

        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    response.Content = new StringContent(JsonConvert.SerializeObject(racelist), Encoding.UTF8, "application/json");
        //    return response;
        //}



        //[AcceptVerbs("GET", "POST")]
        //[HttpGet]
        //public HttpResponseMessage GetRacerRaceHistory(string discordid, string startdt = "", string enddt = "")
        //{
        //    LogRequest("GetRacerRaceHistory");

        //    PublicRacerRaceHistory prrh = new PublicRacerRaceHistory();

        //    tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == discordid).FirstOrDefault();

        //    if (racer != null)
        //    {
        //        if (startdt == null || startdt == "") 
        //        {
        //            startdt = "01011900";
        //        }

        //        if (enddt == null || enddt == "")
        //        {
        //            enddt = "01012100";
        //        }

        //        DateTime sdt = DateTime.Parse(string.Format("{0}{1}/{2}{3}/{4}{5}{6}{7}", startdt[0], startdt[1], startdt[2], startdt[3], startdt[4], startdt[5], startdt[6], startdt[7]));
        //        DateTime edt = DateTime.Parse(string.Format("{0}{1}/{2}{3}/{4}{5}{6}{7}", enddt[0], enddt[1], enddt[2], enddt[3], enddt[4], enddt[5], enddt[6], enddt[7]));

        //        List<int> raceids = dbContext.tb_races.Where(x => x.StartDateTime >= sdt && x.StartDateTime <= edt).Select(x => x.race_id).ToList();

        //        List<tb_pairings> pairings = dbContext.tb_pairings.Where(x => (x.racer1_id == racer.racer_id || x.racer2_id == racer.racer_id) && raceids.Contains(x.race_id)).ToList();

        //        prrh.TotalCount = pairings.Count();
        //        prrh.RacerName = racer.RacerName;
        //    }

        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    response.Content = new StringContent(JsonConvert.SerializeObject(prrh), Encoding.UTF8, "application/json");
        //    return response;
        //}
    }
}
