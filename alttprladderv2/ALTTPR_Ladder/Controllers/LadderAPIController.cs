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
using System.Security.Cryptography;
using System.Security.Policy;
using System.Xml.Linq;
using System.Diagnostics;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using ALTTPR_Ladder.Helpers;
using Microsoft.Ajax.Utilities;
using System.Web;
using static ALTTPR_Ladder.LadderAPIController;
using System.Web.UI.WebControls;

namespace ALTTPR_Ladder
{
    public class LadderAPIController : ApiController
    {
        public ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();
        public int ErrorAttempts;

        private static bool ValidateHash(string hash)
        {
            return (hash == ConfigurationManager.AppSettings["ValidationHash"] ? true : false);
        }

        // api/v1/LadderAPI/
        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage Ping(string hash)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            //CancelRace(hash, "84cc268e");

            //ProcessDecay(5);

            //CacheStandings(5);

            //CacheStandingsNEW();

            //int i = 0;
            //for (i = 0; i < 1000; i++)
            //{
            //    GenerateMysterySeed();
            //}

            //var x = GenerateSeed(hash, 1);

            //var response = Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(JsonConvert.SerializeObject(GenerateSeed(1, "")), Encoding.UTF8, "application/json");
            //return response;

            return Request.CreateResponse(HttpStatusCode.OK, "Private API - Pong! - " + DateTime.Now.ToString());
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage RegisterRacer(string hash, string RacerName, string RacerLogin, string DiscordID)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            RegisterReturn rr = new RegisterReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID).FirstOrDefault();

            //If the racer is new, create a new user
            if (racer == null)
            {
                racer = new tb_racers();

                racer.RacerGUID = Guid.NewGuid();
                racer.RacerName = RacerName;
                racer.RacerLogin = RacerLogin.Replace("--", "#");
                racer.DiscordID = DiscordID;
                racer.IsActive = true;

                dbContext.tb_racers.Add(racer);

                dbContext.SaveChanges();

                rr.valid = true;
            }
            else if (racer.IsActive == false)
            {
                racer.RacerName = RacerName;
                racer.RacerLogin = RacerLogin.Replace("--", "#");
                racer.IsActive = true;

                dbContext.SaveChanges();

                rr.valid = true;
            }
            else
            {
                rr.valid = false;
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(rr), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage UnregisterRacer(string hash, string RacerName, string RacerLogin, string DiscordID)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            RegisterReturn rr = new RegisterReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID).FirstOrDefault();

            //If the racer doesn't exist, or already unregistered, return a false
            if (racer == null || racer.IsActive == false)
            {
                rr.valid = false;
            }
            else
            {
                racer.RacerName = RacerName;
                racer.RacerLogin = RacerLogin.Replace("--", "#");
                racer.IsActive = false;

                dbContext.SaveChanges();

                rr.valid = true;
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(rr), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage UpdateRacerName(string hash, string RacerName, string RacerLogin, string DiscordID)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            RegisterReturn rr = new RegisterReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID).FirstOrDefault();

            //If the racer doesn't exist, return a false
            if (racer == null)
            {
                rr.valid = false;
            }
            else
            {
                racer.RacerName = RacerName;
                racer.RacerLogin = RacerLogin.Replace("--", "#");

                dbContext.SaveChanges();

                rr.valid = true;
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(rr), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetNextRaces(string hash, int NumberOfRaces)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            List<tb_races> racelist = dbContext.tb_races.Where(x => x.HasCompleted == false).OrderBy(x => x.StartDateTime).Take(NumberOfRaces).ToList();

            ScheduleReturn sr = new ScheduleReturn();

            sr.ScheduleDetails = racelist.Select(x => new ScheduleDetails()
            {
                RaceName = x.RaceGUID.ToString().Substring(0, 8),
                ModeName = dbContext.tb_flags.Where(y => y.flag_id == x.flag_id).Select(y => y.FlagName).FirstOrDefault(),
                RaceStatus = GetRaceStatus(x.race_id),
                UTCStart = GetRaceStart(x.race_id),
                UTCTimerTicks = GetTimerTicks(x.race_id),
                Schedule = x.Schedule,
                CurrentlySignedUp = GetSignedUp(x.race_id),
                ActivelyRacing = GetActiveRacers(x.race_id),
                SeedURL = GetSeedURL(x.race_id),
                SpoilerHash = GetSpoilerHash(x.race_id),
                Hours = dbContext.tb_flags.Where(y => y.flag_id == x.flag_id).Select(y => y.HoursToComplete).FirstOrDefault(),
                IsChampionshipRace = x.ChampionshipRace
            }).ToList();

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(sr), Encoding.UTF8, "application/json");
            return response;
        }

        private int GetRaceStatus(int race_id)
        {
            //0: Scheduled
            //1: Signups
            //2: Closed
            //3: Racing
            //4: Ended
            var race = dbContext.tb_races.Where(x => x.race_id == race_id).FirstOrDefault();

            if (race.StartDateTime.AddMinutes(-30) >= DateTime.Now)
            {
                return 0;
            }
            else if (race.StartDateTime.AddMinutes(-30) < DateTime.Now && race.StartDateTime.AddMinutes(-10) >= DateTime.Now)
            {
                return 1;
            }
            else if (race.StartDateTime.AddMinutes(-10) < DateTime.Now && race.StartDateTime >= DateTime.Now)
            {
                return 2;
            }
            else if (race.HasStarted && !race.HasCompleted)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        private int GetRaceStart(int race_id)
        {
            var race = dbContext.tb_races.Where(x => x.race_id == race_id).FirstOrDefault();

            return Helpers.GlobalHelpers.ConvertToUTC(race.StartDateTime);

            //if (race.StartDateTime.AddMinutes(-30) >= DateTime.Now)
            //{
            //    return ConvertToUTC(race.StartDateTime.AddMinutes(-30));
            //}
            //else if (race.StartDateTime.AddMinutes(-30) < DateTime.Now && race.StartDateTime.AddMinutes(-10) >= DateTime.Now)
            //{
            //    return ConvertToUTC(race.StartDateTime.AddMinutes(-10));
            //}
            //else
            //{
            //    return ConvertToUTC(race.StartDateTime);
            //}
        }

        private int GetTimerTicks(int race_id)
        {
            var race = dbContext.tb_races.Where(x => x.race_id == race_id).FirstOrDefault();

            if (race.StartDateTime.AddMinutes(-30) >= DateTime.Now)
            {
                return Helpers.GlobalHelpers.ConvertToUTC(race.StartDateTime.AddMinutes(-30)) - Helpers.GlobalHelpers.ConvertToUTC(DateTime.Now);
            }
            else if (race.StartDateTime.AddMinutes(-30) < DateTime.Now && race.StartDateTime.AddMinutes(-10) >= DateTime.Now)
            {
                return Helpers.GlobalHelpers.ConvertToUTC(race.StartDateTime.AddMinutes(-10)) - Helpers.GlobalHelpers.ConvertToUTC(DateTime.Now);
            }
            else if (race.StartDateTime.AddMinutes(-10) < DateTime.Now && race.StartDateTime >= DateTime.Now)
            {
                return Helpers.GlobalHelpers.ConvertToUTC(race.StartDateTime) - Helpers.GlobalHelpers.ConvertToUTC(DateTime.Now);
            }
            else if (race.HasStarted && !race.HasCompleted)
            {
                return Helpers.GlobalHelpers.ConvertToUTC(((DateTime)race.RaceStartTime).AddHours(dbContext.tb_flags.Where(x => x.flag_id == race.flag_id).Select(x => x.HoursToComplete).FirstOrDefault())) - Helpers.GlobalHelpers.ConvertToUTC(DateTime.Now);
            }
            else if (race.HasCompleted)
            {
                return 300;
            }

            return 0;
        }

        private int GetSignedUp(int race_id)
        {
            return dbContext.tb_entrants.Where(x => x.race_id == race_id).Count();
        }

        private int GetActiveRacers(int race_id)
        {
            return dbContext.tb_entrants.Where(x => x.race_id == race_id && x.FinishTime == 0).Count();
        }

        private string GetSeedURL(int race_id)
        {
            var seeddetails = dbContext.tb_seeds.Where(x => x.race_id == race_id).FirstOrDefault();

            if (seeddetails != null)
            {
                return "https://alttprladder.com/seeds/getseed/" + seeddetails.SeedHash;
            }

            return null;
        }

        private string GetSpoilerHash(int race_id)
        {
            var spoilerdetails = dbContext.tb_spoilers.Where(x => x.race_id == race_id).FirstOrDefault();

            if (spoilerdetails != null)
            {
                return spoilerdetails.SpoilerHash;
            }

            return "";
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage JoinRace(string hash, string RacerName, string RacerLogin, string DiscordID, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            JoinReturn jr = new JoinReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID && x.IsActive == true).FirstOrDefault();

            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            if (racer == null)
            {
                jr.valid = false;
                jr.Reason = "Racer Does Not Exist";
            }
            else
            {
                if (racer.StreamURL == null || racer.StreamURL == "")
                {
                    jr.valid = false;
                    jr.Reason = "No Stream";
                }
                else
                {
                    racer.RacerName = RacerName;
                    racer.RacerLogin = RacerLogin.Replace("--", "#");

                    dbContext.SaveChanges();

                    if (currentrace == null)
                    {
                        jr.valid = false;
                        jr.Reason = "Race Does Not Exist";
                    }
                    else
                    {
                        tb_entrants entrant = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.race_id == currentrace.race_id).FirstOrDefault();

                        if (entrant != null)
                        {
                            jr.valid = false;
                            jr.Reason = "Racer Already Entered";
                        }
                        else
                        {
                            tb_races otheractiverace = dbContext.tb_races.Where(x => x.HasStarted == true && x.HasCompleted == false && x.RaceGUID.ToString().StartsWith(RaceName) == false).FirstOrDefault();

                            if (otheractiverace != null)
                            {
                                tb_entrants otherentrant = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.race_id == otheractiverace.race_id && x.FinishTime == 0).FirstOrDefault();

                                if (otherentrant != null)
                                {
                                    jr.valid = false;
                                    jr.Reason = "Racer In Another Race";
                                }
                                else
                                {
                                    jr.valid = true;

                                    entrant = new tb_entrants();

                                    entrant.racer_id = racer.racer_id;
                                    entrant.race_id = currentrace.race_id;
                                    entrant.FinishTime = 0;
                                    entrant.DateTimeEntered = DateTime.Now;
                                    entrant.Comments = null;

                                    dbContext.tb_entrants.Add(entrant);
                                }
                            }
                            else
                            {
                                jr.valid = true;

                                entrant = new tb_entrants();

                                entrant.racer_id = racer.racer_id;
                                entrant.race_id = currentrace.race_id;
                                entrant.FinishTime = 0;
                                entrant.DateTimeEntered = DateTime.Now;
                                entrant.Comments = null;

                                dbContext.tb_entrants.Add(entrant);
                            }

                            dbContext.SaveChanges();
                        }
                    }
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (jr.valid == false)
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(jr), Encoding.UTF8, "application/json");
            }
            else
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(GetRaceStandings(currentrace)), Encoding.UTF8, "application/json");
            }

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage LeaveRace(string hash, string RacerName, string RacerLogin, string DiscordID, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            JoinReturn jr = new JoinReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID && x.IsActive == true).FirstOrDefault();

            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            if (racer == null)
            {
                jr.valid = false;
                jr.Reason = "Racer Does Not Exist";
            }
            else
            {
                racer.RacerName = RacerName;
                racer.RacerLogin = RacerLogin.Replace("--", "#");

                dbContext.SaveChanges();

                if (currentrace == null)
                {
                    jr.valid = false;
                    jr.Reason = "Race Does Not Exist";
                }
                else
                {
                    tb_entrants entrant = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.race_id == currentrace.race_id).FirstOrDefault();

                    if (entrant == null)
                    {
                        jr.valid = false;
                        jr.Reason = "Racer Has Not Joined";
                    }
                    else
                    {
                        jr.valid = true;

                        dbContext.tb_entrants.Remove(entrant);

                        dbContext.SaveChanges();
                    }
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (jr.valid == false)
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(jr), Encoding.UTF8, "application/json");
            }
            else
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(GetRaceStandings(currentrace)), Encoding.UTF8, "application/json");
            }

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage RemoveRacer(string hash, string DiscordID, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            JoinReturn jr = new JoinReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID).FirstOrDefault();

            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            if (racer == null)
            {
                jr.valid = false;
                jr.Reason = "Racer Does Not Exist";
            }
            else
            {
                if (currentrace == null)
                {
                    jr.valid = false;
                    jr.Reason = "Race Does Not Exist";
                }
                else
                {
                    tb_entrants entrant = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.race_id == currentrace.race_id).FirstOrDefault();

                    if (entrant == null)
                    {
                        jr.valid = false;
                        jr.Reason = "Racer Has Not Joined";
                    }
                    else
                    {
                        jr.valid = true;

                        dbContext.tb_entrants.Remove(entrant);

                        dbContext.SaveChanges();
                    }
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (jr.valid == false)
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(jr), Encoding.UTF8, "application/json");
            }
            else
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(GetRaceStandings(currentrace)), Encoding.UTF8, "application/json");
            }

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage DQRacer(string hash, string RacerLogin)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            JoinReturn jr = new JoinReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.RacerLogin == RacerLogin).FirstOrDefault();

            tb_races currentrace = new tb_races();

            if (racer == null)
            {
                jr.valid = false;
                jr.Reason = "Racer Does Not Exist";
            }
            else
            {
                tb_entrants entrant = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.FinishTime == 0).FirstOrDefault();

                if (entrant == null)
                {
                    jr.valid = false;
                    jr.Reason = "Racer Has Not Joined";
                }
                else
                {
                    jr.valid = true;

                    entrant.FinishTime = 99999;
                    entrant.Comments = "Administrator DQ";

                    dbContext.SaveChanges();

                    currentrace = dbContext.tb_races.Where(x => x.race_id == entrant.race_id).FirstOrDefault();
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (jr.valid == false)
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(jr), Encoding.UTF8, "application/json");
            }
            else
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(GetRaceStandings(currentrace)), Encoding.UTF8, "application/json");
            }

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetRaceSignupMessage(string hash, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            JoinReturn jr = new JoinReturn();

            jr.valid = true;

            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            if (currentrace == null)
            {
                jr.valid = false;
                jr.Reason = "Race Does Not Exist";
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (jr.valid == false)
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(jr), Encoding.UTF8, "application/json");
            }
            else
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(FinishRaceStandings(currentrace, "")), Encoding.UTF8, "application/json");
            }

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage CloseSignups(string hash, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            SeedReturn sr = new SeedReturn();

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (currentrace != null)
            {
                int entrantcount = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id).Count();

                if (entrantcount > 1)
                {
                    sr = GenerateSeed(0, RaceName);
                }
                else
                {
                    sr.valid = false;

                    currentrace.HasCompleted = true;
                    currentrace.HasBeenProcessed = true;

                    dbContext.SaveChanges();
                }
            }

            response.Content = new StringContent(JsonConvert.SerializeObject(sr), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage StartRace(string hash, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            SeedReturn sr = new SeedReturn();

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (currentrace != null)
            {
                currentrace.HasStarted = true;
                currentrace.RaceStartTime = DateTime.Now;

                dbContext.SaveChanges();
            }

            sr.valid = true;

            response.Content = new StringContent(JsonConvert.SerializeObject(sr), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage RacerDone(string hash, string DiscordID, string RaceName, string FinishTime)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            JoinReturn jr = new JoinReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID).FirstOrDefault();

            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            if (racer == null)
            {
                jr.valid = false;
                jr.Reason = "Racer Does Not Exist";
            }
            else
            {
                if (currentrace == null)
                {
                    jr.valid = false;
                    jr.Reason = "Race Does Not Exist";
                }
                else
                {
                    tb_entrants entrant = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.race_id == currentrace.race_id && x.FinishTime == 0).FirstOrDefault();

                    if (entrant == null)
                    {
                        jr.valid = false;
                        jr.Reason = "Racer Already Finished";
                    }
                    else
                    {
                        jr.valid = true;

                        entrant.FinishTime = (FinishTime == "99999" ? 99999 : GetFinishTime(entrant));//  int.Parse(FinishTime);
                        entrant.DateTimeFinished = DateTime.Now;

                        FinishTime = entrant.FinishTime.ToString();

                        dbContext.SaveChanges();
                    }
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (jr.valid == false)
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(jr), Encoding.UTF8, "application/json");
            }
            else
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(FinishRaceStandings(currentrace, FinishTime)), Encoding.UTF8, "application/json");
            }

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage UndoFinish(string hash, string DiscordID, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            JoinReturn jr = new JoinReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID).FirstOrDefault();

            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName) && x.HasCompleted == false).FirstOrDefault();

            if (racer == null)
            {
                jr.valid = false;
                jr.Reason = "Racer Does Not Exist";
            }
            else
            {
                if (currentrace == null)
                {
                    jr.valid = false;
                    jr.Reason = "Race Does Not Exist";
                }
                else
                {
                    tb_entrants entrant = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.race_id == currentrace.race_id && x.FinishTime != 0).FirstOrDefault();

                    if (entrant == null)
                    {
                        jr.valid = false;
                        jr.Reason = "Racer Already Finished";
                    }
                    else
                    {
                        if (((DateTime)entrant.DateTimeFinished).AddSeconds(10) >= DateTime.Now)
                        {

                            jr.valid = true;

                            entrant.FinishTime = 0;
                            entrant.DateTimeFinished = null;

                            dbContext.SaveChanges();
                        }
                        else
                        {
                            jr.valid = false;
                            jr.Reason = "Time Elapsed";
                        }
                    }
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (jr.valid == false)
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(jr), Encoding.UTF8, "application/json");
            }
            else
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(FinishRaceStandings(currentrace, "0")), Encoding.UTF8, "application/json");
            }

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage CompleteRace(string hash, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            tb_races race = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            if (race != null)
            {
                race.HasCompleted = true;

                dbContext.SaveChanges();

                ReduceDecay(race.race_id);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StringContent(JsonConvert.SerializeObject(true), Encoding.UTF8, "application/json");

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage AutoFF(string hash, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            tb_races race = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            ForfeitReturn fr = new ForfeitReturn();

            fr.DiscordID = new List<string>();
            fr.valid = false;

            if (race != null)
            {
                List<tb_entrants> elist = dbContext.tb_entrants.Where(x => x.race_id == race.race_id && x.FinishTime == 0).ToList();

                foreach (tb_entrants e in elist)
                {
                    fr.DiscordID.Add(dbContext.tb_racers.Where(x => x.racer_id == e.racer_id).Select(x => x.DiscordID).FirstOrDefault());

                    e.DateTimeFinished = DateTime.Now;
                    e.FinishTime = 99999;
                }

                fr.valid = true;

                dbContext.SaveChanges();
            }

            CalculateRace(hash);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(fr), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage CancelRace(string hash, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            tb_races race = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            if (race != null)
            {
                race.HasStarted = true;
                race.HasCompleted = true;
                race.HasBeenProcessed = true;

                dbContext.SaveChanges();

                ReduceDecay(race.race_id);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StringContent(JsonConvert.SerializeObject(true), Encoding.UTF8, "application/json");

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetRaceDetails(string hash, string RaceName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();
            response.Content = new StringContent(JsonConvert.SerializeObject(GetRaceStandings(currentrace)), Encoding.UTF8, "application/json");

            return response;
        }

        private RaceStandingsReturn GetRaceStandings(tb_races currentrace)
        {
            RaceStandingsReturn rsr = new RaceStandingsReturn();
            rsr.RacerStandingList = new List<RacerStanding>();

            rsr.RaceGUID = currentrace.RaceGUID.ToString();
            rsr.RaceName = "#" + currentrace.RaceGUID.ToString().Substring(0, 8);
            rsr.FlagName = dbContext.tb_flags.Where(x => x.flag_id == currentrace.flag_id).Select(x => x.FlagName).FirstOrDefault();

            List<tb_entrants> entrantlist = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id).ToList();

            foreach (tb_entrants e in entrantlist)
            {
                string racername = dbContext.tb_racers.Where(x => x.racer_id == e.racer_id).Select(x => x.RacerName).FirstOrDefault();

                int? currentranking = dbContext.tb_rankings.Where(x => x.racer_id == e.racer_id && x.flag_id == currentrace.flag_id).OrderByDescending(x => x.LastUpdated).Select(x => x.Ranking).FirstOrDefault();

                if (currentranking == null || currentranking == 0)
                {
                    currentranking = 1000;
                }

                rsr.RacerStandingList.Add(new RacerStanding(racername, (int)currentranking, e.FinishTime, GlobalHelpers.ConvertToTime(e.FinishTime), 0, ""));
            }

            rsr.RacerCount = entrantlist.Count;
            rsr.CurrentlyRacing = entrantlist.Where(x => x.FinishTime == 0).Count();
            rsr.valid = true;

            rsr.RacerStandingList = rsr.RacerStandingList.OrderBy(x => x.RaceTime).ThenByDescending(x => x.Ranking).ToList();

            int standing = 0;
            int time = 0;

            for (int i = 0; i < rsr.RacerStandingList.Count(); i++)
            {
                if (rsr.RacerStandingList[i].FinishTime != time)
                {
                    time = rsr.RacerStandingList[i].FinishTime;
                    standing = i + 1;
                    if (rsr.RacerStandingList[i].FinishTime != 0)
                    {
                        rsr.RacerStandingList[i].StandingFinish = GlobalHelpers.ConvertToPlace(standing);
                    }
                }

                rsr.RacerStandingList[i].Standing = standing;
            }

            return rsr;
        }

        private FinishRaceReturn FinishRaceStandings(tb_races currentrace, string FinishTime)
        {
            FinishRaceReturn frr = new FinishRaceReturn();
            frr.RacerStandingList = new List<RacerStanding>();

            frr.RaceGUID = currentrace.RaceGUID.ToString();
            frr.RaceName = "#" + currentrace.RaceGUID.ToString().Substring(0, 8);
            frr.FlagName = dbContext.tb_flags.Where(x => x.flag_id == currentrace.flag_id).Select(x => x.FlagName).FirstOrDefault();

            List<tb_entrants> entrantlist = dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id).ToList();

            foreach (tb_entrants e in entrantlist)
            {
                string racername = dbContext.tb_racers.Where(x => x.racer_id == e.racer_id).Select(x => x.RacerName).FirstOrDefault();

                int? currentranking = dbContext.tb_rankings.Where(x => x.racer_id == e.racer_id && x.flag_id == currentrace.flag_id).OrderByDescending(x => x.LastUpdated).Select(x => x.Ranking).FirstOrDefault();

                if (currentranking == null || currentranking == 0)
                {
                    currentranking = 1000;
                }

                frr.RacerStandingList.Add(new RacerStanding(racername, (int)currentranking, e.FinishTime, GlobalHelpers.ConvertToTime(e.FinishTime), 0, ""));
            }

            frr.RacerCount = entrantlist.Count;
            frr.CurrentlyRacing = entrantlist.Where(x => x.FinishTime == 0).Count();
            frr.valid = true;

            frr.RacerStandingList = frr.RacerStandingList.OrderBy(x => x.RaceTime).ThenByDescending(x => x.Ranking).ToList();

            int standing = 1;
            int time = 0;

            for (int i = 0; i < frr.RacerStandingList.Count(); i++)
            {
                //If the racer has finished, check against the current standing
                if (frr.RacerStandingList[i].FinishTime > 0 && frr.RacerStandingList[i].FinishTime < 88888)
                {
                    //If it is not a tie, increment and assign. Otherwise, just assign
                    if (time != frr.RacerStandingList[i].FinishTime)
                    {
                        standing = i + 1;
                        time = frr.RacerStandingList[i].FinishTime;
                    }

                    frr.RacerStandingList[i].StandingFinish = GlobalHelpers.ConvertToPlace(standing);
                }
                //If Finish Time is a FF/DQ, set standing to the max count
                else if (frr.RacerStandingList[i].FinishTime == 99999)
                {
                    frr.RacerStandingList[i].StandingFinish = "FF";
                }
                else if (frr.RacerStandingList[i].FinishTime == 88888)
                {
                    frr.RacerStandingList[i].StandingFinish = "DQ";
                }
                // If Finish Time is 0, racer is still racing, and just increment the standing
                else if (frr.RacerStandingList[i].FinishTime == 0)
                {
                    frr.RacerStandingList[i].StandingFinish = (i + 1).ToString();
                }
            }

            if (FinishTime != "")
            {
                int finishseconds = int.Parse(FinishTime);

                frr.Finish = GlobalHelpers.ConvertToPlace(dbContext.tb_entrants.Where(x => x.race_id == currentrace.race_id && x.FinishTime < finishseconds && x.FinishTime != 0).Count() + 1);
                frr.FinishTime = GlobalHelpers.ConvertToTime(finishseconds);
            }
            else
            {
                frr.Finish = "";
                frr.FinishTime = "";
            }

            return frr;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage AddComment(string hash, string DiscordID, string RaceName, string Comments)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            JoinReturn jr = new JoinReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID).FirstOrDefault();

            tb_races currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

            if (racer == null)
            {
                jr.valid = false;
                jr.Reason = "Racer Does Not Exist";
            }
            else
            {
                if (currentrace == null)
                {
                    jr.valid = false;
                    jr.Reason = "Race Does Not Exist";
                }
                else
                {
                    tb_entrants entrant = dbContext.tb_entrants.Where(x => x.racer_id == racer.racer_id && x.race_id == currentrace.race_id && x.FinishTime != 0).FirstOrDefault();

                    if (entrant == null)
                    {
                        jr.valid = false;
                        jr.Reason = "Racer Not Finished";
                    }
                    else
                    {
                        jr.valid = true;

                        entrant.Comments = Comments;

                        dbContext.SaveChanges();
                    }
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(jr), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage CalculateRace(string hash)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            bool success = CalculateAllRaces();

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(success), Encoding.UTF8, "application/json");
            return response;
        }

        private void CalculateRankings()
        {
            var racelist = dbContext.tb_races.Where(x => x.HasCompleted == true && x.HasBeenProcessed == false).ToList();

            foreach (var race in racelist)
            {
                var entrantlist = dbContext.tb_entrants.Where(x => x.race_id == race.race_id).ToList();

                foreach (var e in entrantlist)
                {
                    if (dbContext.tb_rankings.Where(x => x.racer_id == e.racer_id && x.flag_id == race.flag_id).Count() == 0)
                    {
                        tb_rankings r = new tb_rankings();

                        r.racer_id = e.racer_id;
                        r.flag_id = race.flag_id;
                        r.race_id = 0;
                        r.Ranking = 1000;
                        r.Change = 0;
                        r.Result = 0;
                        r.LastUpdated = DateTime.Now;

                        dbContext.tb_rankings.Add(r);
                    }
                }

                dbContext.SaveChanges();

                List<RankingsObject> rankingobjectlist = entrantlist.Select(x => new RankingsObject()
                {
                    racer_id = x.racer_id,
                    FinishTime = x.FinishTime,
                    StartingRating = dbContext.tb_rankings.Where(y => y.flag_id == race.flag_id && y.racer_id == x.racer_id).OrderByDescending(y => y.LastUpdated).Select(y => y.Ranking).FirstOrDefault(),
                    Remainder = 0,
                    Wager = (int)(dbContext.tb_rankings.Where(y => y.flag_id == race.flag_id && y.racer_id == x.racer_id).OrderByDescending(y => y.LastUpdated).Select(y => y.Ranking).FirstOrDefault() * .025)
                }).OrderBy(y => y.FinishTime).ToList();

                int totalracers = entrantlist.Count();
                int totalrating = rankingobjectlist.Select(x => x.StartingRating).Sum();
                int wagerremaining = rankingobjectlist.Select(x => x.Wager).Sum();
                decimal gainperc = dbContext.tb_gainpercentages.Where(x => x.RacerCount == totalracers).Select(x => x.GainPerc).FirstOrDefault();

                if (totalracers > 100)
                {
                    gainperc = dbContext.tb_gainpercentages.Where(x => x.RacerCount == 100).Select(x => x.GainPerc).FirstOrDefault();
                }

                int lastfinishtime = 0;
                int lastrank = 0;
                int i = 1;
                bool checkforties = false;

                //Assign Points
                foreach (var ro in rankingobjectlist)
                {
                    if (ro.FinishTime != lastfinishtime)
                    {
                        lastfinishtime = ro.FinishTime;
                        lastrank = i;
                    }
                    else
                    {
                        checkforties = true;
                    }

                    ro.Result = lastrank;

                    ro.ResultPerc = ((100 / totalracers) * (ro.Result - 1)) + 1;

                    ro.Gain = (int)Math.Round((wagerremaining * gainperc));

                    if (ro.Gain < ro.Wager + (ro.Wager * 0.05) && ro.ResultPerc < 20)
                    {
                        ro.Gain = (int)Math.Round((ro.Wager + (ro.Wager * 0.05)));
                    }

                    wagerremaining = wagerremaining - ro.Gain;

                    i++;
                }

                int resultcheck = 0;

                //If there are any ties, redistribute the points for the ties
                if (checkforties)
                {
                    int gainsum = 0;

                    foreach (var ro in rankingobjectlist)
                    {

                        if (rankingobjectlist.Where(x => x.Result == ro.Result).Count() > 1)
                        {
                            if (resultcheck == 0)
                            {
                                resultcheck = ro.Result;
                                gainsum = rankingobjectlist.Where(x => x.Result == ro.Result).Select(x => x.Gain).Sum() / rankingobjectlist.Where(x => x.Result == ro.Result).Count();
                            }
                        }
                        else
                        {
                            resultcheck = 0;
                        }

                        if (ro.Result == resultcheck)
                        {
                            ro.Gain = gainsum;
                        }
                    }
                }

                lastrank = 1;
                int currentmax = 1;

                //Distribute remainder points
                while (wagerremaining > 0)
                {
                    foreach (var ro in rankingobjectlist.Where(x => x.Result == lastrank))
                    {
                        ro.Remainder++;
                        wagerremaining--;
                    }

                    lastrank++;
                    if (lastrank > currentmax)
                    {
                        lastrank = 1;
                        currentmax++;

                        if (currentmax > rankingobjectlist.Count())
                        {
                            currentmax = 1;
                        }
                    }
                }

                //Set Placement Percentage Bonus
                foreach (var ro in rankingobjectlist)
                {
                    ro.PlacementPerc = ro.FinishTime >= 88888 ? 0 : (101 - ro.ResultPerc) / 3;
                    ro.PlacementBonus = (int)Math.Round((totalracers * ro.PlacementPerc) / 100d, 0);

                    ro.Reward = ro.FinishTime >= 88888 ? 0 : ro.Gain + ro.Remainder + ro.PlacementBonus;
                    ro.NewRating = ro.StartingRating - ro.Wager + ro.Reward;
                    ro.GainLoss = ro.NewRating - ro.StartingRating;
                }

                dbContext.tb_rankings.AddRange(rankingobjectlist.Select(x => new tb_rankings()
                {
                    racer_id = x.racer_id,
                    flag_id = race.flag_id,
                    race_id = race.race_id,
                    Ranking = x.NewRating,
                    Change = x.GainLoss,
                    Result = x.FinishTime >= 88888 ? 999 : x.Result,
                    LastUpdated = DateTime.Now
                }).ToList());

                race.HasBeenProcessed = true;

                dbContext.SaveChanges();

                ProcessDecay(race.flag_id);

                CacheStandings(race.flag_id);
            }
        }

        private void CacheStandings(int flag_id)
        {
            dbContext.sp_CacheRankings(flag_id);
        }

        private void ReduceDecay(int race_id)
        {
            var currentrace = dbContext.tb_races.Where(x => x.race_id == race_id).FirstOrDefault();

            if (currentrace != null)
            {
                var entrants = dbContext.tb_entrants.Where(x => x.race_id == race_id).Select(x => x.racer_id).ToList();

                var decaylist = dbContext.tb_decay.Where(x => x.flag_id == currentrace.flag_id && entrants.Contains(x.racer_id)).ToList();

                foreach (var d in decaylist)
                {
                    if (d.DecayAccrued <= 5)
                    {
                        dbContext.tb_decay.Remove(d);
                    }
                    else
                    {
                        d.LastUpdated = DateTime.Now;
                        d.DecayAccrued = d.DecayAccrued - 5;
                    }

                    dbContext.SaveChanges();
                }
            }
        }

        private void ProcessDecay(int flag_id)
        {
            int decaydays = 90;
            int activedays = 30;
            int numberofraces = 10;
            int decaycutdays = 7;
            int decaylimit = 5;
            int decaycap = 30;

            DateTime beforetime = DateTime.Now.AddDays(decaydays * -1);
            DateTime activetime = DateTime.Now.AddDays(activedays * -1);
            DateTime decaycutoff = DateTime.Now.AddDays(decaycutdays * -1);
            DateTime yearago = DateTime.Now.AddYears(-1);

            //Check to see if there are any races over 90 days old, will skip calculation otherwise
            bool isvalid = dbContext.tb_races.Where(x => x.flag_id == flag_id && x.StartDateTime <= beforetime).Count() > 0;

            //If there are races over 90 days old and there have been more than 10 races in the past 30 days, otherwise skip
            if (isvalid && dbContext.tb_races.Where(x => x.flag_id == flag_id && x.StartDateTime >= activetime && x.HasCompleted == true).Count() >= numberofraces)
            {
                //Get the unique race IDs for all completed races with the flag_id
                var raceidlist = dbContext.tb_races.Where(x => x.flag_id == flag_id && x.HasCompleted == true).Select(x => x.race_id).ToList();

                //Get the unique racer IDs for all entered racers for the checked racers
                var uniqueracerids = dbContext.tb_entrants.Where(x => raceidlist.Contains(x.race_id)).Select(x => x.racer_id).Distinct().ToList();

                //Loop through each user
                foreach (var uid in uniqueracerids)
                {
                    //Retrieve all entry records for the user that it in the race list
                    var uniqueentrants = dbContext.tb_entrants.Where(x => x.racer_id == uid && raceidlist.Contains(x.race_id)).ToList();

                    //If there are any records with entries after the before time cutoff, we will skip
                    if (uniqueentrants.Where(x => x.DateTimeEntered > beforetime).Count() == 0)
                    {
                        //Check for the decay record for the user if it exists
                        var decayrecord = dbContext.tb_decay.Where(x => x.racer_id == uid && x.flag_id == flag_id).FirstOrDefault();

                        var decaymaxcap = decaylimit * dbContext.tb_entrants.Where(x => x.racer_id == uid && raceidlist.Contains(x.race_id) && x.DateTimeEntered >= yearago).Count();

                        decaymaxcap = decaymaxcap > decaycap ? decaycap : decaymaxcap;

                        //If it does not exist, or has not been generated in past 7 days and is less than 30, we apply decay, otherwise skip
                        if (decayrecord == null || (decayrecord.LastUpdated < decaycutoff && decayrecord.DecayAccrued < decaymaxcap))
                        {
                            var lastrankings = dbContext.tb_rankings.Where(x => x.racer_id == uid && x.flag_id == flag_id && x.race_id > 0).OrderByDescending(x => x.LastUpdated).FirstOrDefault();

                            if (lastrankings != null)
                            {
                                int lastranking = lastrankings.Ranking;

                                lastrankings = dbContext.tb_rankings.Where(x => x.racer_id == uid && x.flag_id == flag_id && x.race_id != 0).OrderByDescending(x => x.LastUpdated).FirstOrDefault();

                                int decaymod = decaylimit;

                                if (decayrecord == null)
                                {
                                    decayrecord = new tb_decay();

                                    decayrecord.LastUpdated = DateTime.Now;
                                    decayrecord.racer_id = uid;
                                    decayrecord.flag_id = flag_id;
                                    decayrecord.DecayAccrued = decaylimit;

                                    dbContext.tb_decay.Add(decayrecord);
                                }
                                else
                                {
                                    if (decayrecord.DecayAccrued + decaymod > decaymaxcap)
                                    {
                                        decaymod = decaymaxcap - decayrecord.DecayAccrued;
                                    }

                                    decayrecord.LastUpdated = DateTime.Now;
                                    decayrecord.DecayAccrued = decayrecord.DecayAccrued + decaymod;
                                }

                                var decayrankings = new tb_rankings();

                                decayrankings.racer_id = uid;
                                decayrankings.flag_id = flag_id;
                                decayrankings.race_id = -1;
                                decayrankings.Ranking = lastrankings.Ranking - (int)Math.Round((lastranking * (decaymod * .01)));
                                decayrankings.Change = decayrankings.Ranking - lastrankings.Ranking;
                                decayrankings.Result = -1;
                                decayrankings.LastUpdated = DateTime.Now;

                                dbContext.tb_rankings.Add(decayrankings);

                                dbContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }


        private bool CalculateAllRaces()
        {
            int loopchecks = 0;

            var currentprocess = dbContext.tb_processes.Where(x => x.ProcessEnd == null).FirstOrDefault();

            DateTime delay = DateTime.Now.AddSeconds(3);

            while (currentprocess != null)
            {
                if (delay < DateTime.Now)
                {
                    currentprocess = dbContext.tb_processes.Where(x => x.ProcessEnd == null).FirstOrDefault();

                    if (currentprocess != null)
                    {
                        delay = DateTime.Now.AddSeconds(3);
                        loopchecks++;
                        if (loopchecks > 3)
                        {
                            return false;
                        }
                    }
                }
            }

            currentprocess = new tb_processes();

            currentprocess.ProcessStart = DateTime.Now;

            dbContext.tb_processes.Add(currentprocess);

            dbContext.SaveChanges();

            CalculateRankings();

            currentprocess.ProcessEnd = DateTime.Now;

            dbContext.SaveChanges();

            return true;
        }





        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetCurrentSchedule(string hash)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(JsonConvert.SerializeObject(srlist), Encoding.UTF8, "application/json");
            return response;
        }


        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage SetStream(string hash, string DiscordID, string source)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            RegisterReturn rr = new RegisterReturn();

            tb_racers racer = dbContext.tb_racers.Where(x => x.DiscordID == DiscordID).FirstOrDefault();

            if (racer == null)
            {
                rr.valid = false;
            }
            else
            {
                racer.StreamURL = source;
                dbContext.SaveChanges();

                rr.valid = true;
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(rr), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetStream(string hash, string RacerName)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            RacerName = RacerName.Replace("--", "#");

            List<tb_racers> racerlist = dbContext.tb_racers.Where(x => (x.RacerLogin.Contains(RacerName) || x.RacerName.Contains(RacerName)) && x.IsActive == true && x.StreamURL != null).OrderBy(x => x.RacerName).ToList();

            StreamReturn sr = new StreamReturn();
            sr.StreamList = new List<StreamDetails>();

            foreach (tb_racers r in racerlist)
            {
                sr.StreamList.Add(new StreamDetails(r.StreamURL, r.RacerName));
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(sr), Encoding.UTF8, "application/json");
            return response;
        }


        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage BotSeed(string hash, int flag_id)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            SeedReturn sr = GenerateSeed(flag_id, "");

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(sr), Encoding.UTF8, "application/json");
            return response;
        }

        //[AcceptVerbs("GET", "POST")]
        //[HttpGet]
        //public HttpResponseMessage IsRaceOver(string hash)
        //{
        //    if (!ValidateHash(hash))
        //    {
        //        return Request.CreateResponse(HttpStatusCode.Unauthorized);
        //    }

        //    tb_races race = dbContext.tb_races.Where(x => x.HasCompleted == false).OrderBy(x => x.StartDateTime).FirstOrDefault();

        //    if (race != null)
        //    {
        //        int pairingscount = dbContext.tb_pairings.Where(x => x.race_id == race.race_id && (x.Racer1Time == null || x.Racer2Time == null)).Count();

        //        if (pairingscount == 0)
        //        {
        //            race.HasCompleted = true;

        //            dbContext.SaveChanges();
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, pairingscount + "|#" + race.RaceChannel);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.BadRequest, "RACER DOES NOT EXIST");
        //}


        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage SetSeasonSchedule(string hash, int races, string startdt, bool mystery = false)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            //tb_seasons season = dbContext.tb_seasons.Where(x => x.season_id == id).FirstOrDefault();

            int racecount = 0;
            int maxracecount = 12;

            List<tb_schedule> schedulelist = dbContext.tb_schedule.ToList();

            DateTime startDT = DateTime.Parse(startdt);
            DateTime currentDT = startDT;

            bool alternatemystery = false;

            for (int i = 0; i < races; i++)
            {
                var schedule1list = schedulelist.Where(x => x.ScheduleGroup == 0).OrderBy(x => x.SeedOrder).ToList();

                foreach (var s in schedule1list)
                {
                    tb_races r = new tb_races();

                    r.RaceGUID = Guid.NewGuid();
                    if (mystery)
                    {
                        if (alternatemystery)
                        {
                            if (s.flag_id == 6)
                            {
                                r.flag_id = 49;
                            }
                            else if (s.flag_id == 49)
                            {
                                r.flag_id = 6;
                            }
                            else
                            {
                                r.flag_id = s.flag_id;
                            }
                        }
                        else
                        {
                            r.flag_id = s.flag_id;
                        }

                        alternatemystery = !alternatemystery;
                    }
                    else
                    {
                        r.flag_id = s.flag_id;
                    }

                    r.Schedule = 0;
                    r.StartDateTime = currentDT;
                    r.HasStarted = false;
                    r.HasCompleted = false;
                    r.HasBeenProcessed = false;
                    r.ChampionshipRace = false;

                    dbContext.tb_races.Add(r);

                    racecount++;

                    if (racecount > maxracecount)
                    {
                        racecount = 0;
                        currentDT = currentDT.AddHours(dbContext.tb_flags.Where(x => x.flag_id == s.flag_id).Select(x => x.HoursToComplete).FirstOrDefault() + 2);
                    }
                    else
                    {
                        currentDT = currentDT.AddHours(dbContext.tb_flags.Where(x => x.flag_id == s.flag_id).Select(x => x.HoursToComplete).FirstOrDefault() + 1);
                    }

                    
                }
            }

            dbContext.SaveChanges();

            currentDT = startDT.AddHours(2);

            for (int i = 0; i < races; i++)
            {
                var schedule1list = schedulelist.Where(x => x.ScheduleGroup == 1).OrderBy(x => x.SeedOrder).ToList();

                foreach (var s in schedule1list)
                {
                    while (dbContext.tb_races.Where(x => x.StartDateTime == currentDT).Count() > 0)
                    {
                        currentDT = currentDT.AddHours(1);
                    }

                    tb_races r = new tb_races();

                    r.RaceGUID = Guid.NewGuid();
                    r.flag_id = s.flag_id;
                    r.Schedule = 1;
                    r.StartDateTime = currentDT;
                    r.HasStarted = false;
                    r.HasCompleted = false;
                    r.HasBeenProcessed = false;

                    dbContext.tb_races.Add(r);

                    currentDT = currentDT.AddHours(dbContext.tb_flags.Where(x => x.flag_id == s.flag_id).Select(x => x.HoursToComplete).FirstOrDefault() + 1);
                }
            }

            dbContext.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }


        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetInvitationalSchedule(string hash)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            int invid = 8;

            int week = dbContext.tb_invitationals.Where(x => x.invitational_id == invid).Select(x => x.CurrentWeek).FirstOrDefault();

            List<tb_invitational_pairings> pairingslist = dbContext.tb_invitational_pairings.Where(x => x.Week == week && x.invitational_id == invid).ToList();

            List<InvitationalSchedule> schedulelist = new List<InvitationalSchedule>();

            string groupname = "";

            foreach (var pairing in pairingslist)
            {
                if (pairing.GroupName != groupname)
                {
                    InvitationalSchedule gs = new InvitationalSchedule();

                    gs.GroupName = pairing.GroupName;

                    schedulelist.Add(gs);

                    groupname = pairing.GroupName;
                }

                InvitationalSchedule s = new InvitationalSchedule();

                int p1id = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant1_id).Select(x => x.racer_id).FirstOrDefault();
                int p2id = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant2_id).Select(x => x.racer_id).FirstOrDefault();

                string p1name = dbContext.tb_racers.Where(x => x.racer_id == p1id).Select(x => x.RacerName).FirstOrDefault();
                string p2name = dbContext.tb_racers.Where(x => x.racer_id == p2id).Select(x => x.RacerName).FirstOrDefault();

                string mode = dbContext.tb_flags.Where(x => x.flag_id == pairing.flag_id).Select(x => x.FlagName).FirstOrDefault();

                if (pairing.Participant1Time != null && pairing.Participant2Time != null)
                {
                    string p1result = "";
                    string p2result = "";

                    if (pairing.Participant1Time == 88888 && pairing.Participant2Time == 88888)
                    {
                        s.PairingString = String.Format("{0} vs {1} [DNP] {2}", p1name, p2name, mode != "" ? " <" + mode + ">" : "");
                    }
                    else
                    {
                        if (pairing.Participant1Time == 99999)
                        {
                            p1result = "DNF";
                        }
                        else
                        {
                            p1result = GlobalHelpers.ConvertToTime((int)pairing.Participant1Time);
                        }

                        if (pairing.Participant2Time == 99999)
                        {
                            p2result = "DNF";
                        }
                        else
                        {
                            p2result = GlobalHelpers.ConvertToTime((int)pairing.Participant2Time);
                        }

                        if (pairing.Participant1Time < pairing.Participant2Time)
                        {
                            s.PairingString = String.Format("{0} [{1}] def {2} [{3}] {4}", p1name, GlobalHelpers.ConvertToTime((int)pairing.Participant1Time), p2name, GlobalHelpers.ConvertToTime((int)pairing.Participant2Time), mode != "" ? " <" + mode + ">" : "");
                        }
                        else if (pairing.Participant1Time > pairing.Participant2Time)
                        {
                            s.PairingString = String.Format("{2} [{3}] def {0} [{1}] {4}", p1name, GlobalHelpers.ConvertToTime((int)pairing.Participant1Time), p2name, GlobalHelpers.ConvertToTime((int)pairing.Participant2Time), mode != "" ? " <" + mode + ">" : "");
                        }
                        else if (pairing.Participant1Time == pairing.Participant2Time)
                        {
                            s.PairingString = String.Format("{0} [{1}] and {2} [{3}] tied {4}", p1name, GlobalHelpers.ConvertToTime((int)pairing.Participant1Time), p2name, GlobalHelpers.ConvertToTime((int)pairing.Participant2Time), mode != "" ? " <" + mode + ">" : "");
                        }
                    }
                }
                else
                {
                    s.PairingString = String.Format("[{0}] {1} vs {2}{3} {4}", pairing.PairingID, p1name, p2name, (pairing.RaceDateTime != null ? " - <t:" + GlobalHelpers.ConvertToUTC((DateTime)pairing.RaceDateTime) + ":f>" : ""), mode != "" ? " <" + mode + ">" : "");
                }

                schedulelist.Add(s);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(schedulelist), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage SetInvitationalSchedule(string hash, string raceid, string day, string time, string DiscordID)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            int currentinv = 8;

            int currentweek = dbContext.tb_invitationals.Where(x => x.invitational_id == currentinv).Select(x => x.CurrentWeek).FirstOrDefault();

            tb_invitational_pairings pairing = dbContext.tb_invitational_pairings.Where(x => x.PairingID == raceid && x.Week == currentweek).FirstOrDefault();

            if (pairing != null && pairing.Participant1Time == null && pairing.Participant2Time == null && day != null && (time.Length == 3 || time.Length == 4))
            {
                int p1id = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant1_id).Select(x => x.racer_id).FirstOrDefault();
                int p2id = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant2_id).Select(x => x.racer_id).FirstOrDefault();

                string p1did = dbContext.tb_racers.Where(x => x.racer_id == p1id).Select(x => x.DiscordID).FirstOrDefault();
                string p2did = dbContext.tb_racers.Where(x => x.racer_id == p2id).Select(x => x.DiscordID).FirstOrDefault();

                if (DiscordID == p1did || DiscordID == p2did)
                {
                    int h = 0;
                    int m = 0;

                    if (time.Length == 3)
                    {
                        h = int.Parse(time.Substring(0, 1));
                        m = int.Parse(time.Substring(1, 2));
                    }
                    else
                    {
                        h = int.Parse(time.Substring(0, 2));
                        m = int.Parse(time.Substring(2, 2));
                    }

                    int intday = int.Parse(day);
                    int intmonth = 0;

                    switch (currentweek)
                    {
                        case 0:
                        case 1:
                        case 2:
                            intmonth = 10;
                            break;
                        case 3:
                            intmonth = (intday >= 25 ? 10 : 11);
                            break;
                        case 4:
                            intmonth = 11;
                            break;
                    }

                    DateTime scheduledt = new DateTime(2024, intmonth, intday, h, m, 0);

                    pairing.RaceDateTime = scheduledt;
                    pairing.NeedsUpdates = true;

                    dbContext.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, "OK");

                }

            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, "BAD REQ");

        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage CheckForInvitationalMatches(string hash)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            int currentinv = 8;

            int currentweek = dbContext.tb_invitationals.Where(x => x.invitational_id == currentinv).Select(x => x.CurrentWeek).FirstOrDefault();

            DateTime currentDT = DateTime.Now;

            List<tb_invitational_pairings> pairinglist = dbContext.tb_invitational_pairings.Where(x => x.invitational_id == currentinv && x.Week == currentweek && x.RaceDateTime < currentDT && x.RaceRoom != null && x.RaceFinished == false).ToList();

            bool updatepairings = false;

            foreach (tb_invitational_pairings pairing in pairinglist)
            {
                updatepairings = true;
                string webAddr = "https://racetime.gg/alttpr/" + pairing.RaceRoom + "/data"; //One finished

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();

                    RaceTimeGG rtggObject = JsonConvert.DeserializeObject<RaceTimeGG>(responseText);

                    if (rtggObject.status.value == "finished")
                    {
                        int e1finishtime = (rtggObject.entrants[0].finish_time != null ? ParseTime(rtggObject.entrants[0].finish_time) : 99999);
                        int e2finishtime = (rtggObject.entrants[1].finish_time != null ? ParseTime(rtggObject.entrants[1].finish_time) : 99999);

                        tb_invitational_participants participant1 = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant1_id).FirstOrDefault();
                        tb_invitational_participants participant2 = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant2_id).FirstOrDefault();

                        //If RT1 is our P1
                        if (rtggObject.entrants[0].user.full_name == participant1.RaceTimeGG)
                        {
                            pairing.Participant1Time = e1finishtime;
                            pairing.Participant2Time = e2finishtime;
                        }
                        else
                        {
                            pairing.Participant1Time = e2finishtime;
                            pairing.Participant2Time = e1finishtime;
                        }

                        pairing.RaceFinished = true;

                        dbContext.SaveChanges();
                    }
                }
            }

            if (updatepairings)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "OK");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "NO");
            }
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetInvitationalSeed(string hash, string raceid)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            tb_invitational_pairings ip = dbContext.tb_invitational_pairings.Where(x => x.PairingID == raceid).FirstOrDefault();

            DateTime starttime = (DateTime)ip.RaceDateTime;

            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (ip != null && DateTime.Now >= starttime.AddMinutes(-10))
            {
                var seeddetails = BotSeed(hash, (int)ip.flag_id);

                response.Content = new StringContent(JsonConvert.SerializeObject(seeddetails), Encoding.UTF8, "application/json");
            }
            else
            {
                response.Content = new StringContent(JsonConvert.SerializeObject(false), Encoding.UTF8, "application/json");
            }

            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage GetInvitationalEvent(string hash)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            int currentinv = 8;

            int currentweek = dbContext.tb_invitationals.Where(x => x.invitational_id == currentinv).Select(x => x.CurrentWeek).FirstOrDefault();

            List<tb_invitational_pairings> pairings = dbContext.tb_invitational_pairings.Where(x => x.invitational_id == currentinv && x.Week == currentweek && x.ThreadOpened == false && x.NeedsUpdates == true).Take(1).ToList();

            List<InvitationalEvent> threads = pairings.Select(x => new InvitationalEvent()
            {
                EventID = x.EventID == null ? "" : x.EventID,
                PairingID = x.PairingID,
                EventName = string.Format("Gigatational - {0} vs {1} <{2}>",
                dbContext.tb_racers.Where(y => y.racer_id == dbContext.tb_invitational_participants.Where(z => z.participant_id == x.participant1_id).Select(z => z.racer_id).FirstOrDefault()).Select(y => y.RacerName).FirstOrDefault(),
                dbContext.tb_racers.Where(y => y.racer_id == dbContext.tb_invitational_participants.Where(z => z.participant_id == x.participant2_id).Select(z => z.racer_id).FirstOrDefault()).Select(y => y.RacerName).FirstOrDefault(),
                dbContext.tb_flags.Where(y => y.flag_id == x.flag_id).Select(y => y.FlagName).FirstOrDefault()),
                EventLocation = x.HasRestream ? "https://twitch.tv/alttprladder" : string.Format("https://multistre.am/{0}/{1}", dbContext.tb_racers.Where(y => y.racer_id == dbContext.tb_invitational_participants.Where(z => z.participant_id == x.participant1_id).Select(z => z.racer_id).FirstOrDefault()).Select(y => y.StreamURL).FirstOrDefault().Replace("https://twitch.tv/", ""), dbContext.tb_racers.Where(y => y.racer_id == dbContext.tb_invitational_participants.Where(z => z.participant_id == x.participant2_id).Select(z => z.racer_id).FirstOrDefault()).Select(y => y.StreamURL).FirstOrDefault().Replace("https://twitch.tv/", "")),
                NeedsUpdates = x.NeedsUpdates,
                EventStartTime = x.RaceDateTime != null ? GlobalHelpers.ConvertToUTC((DateTime)x.RaceDateTime).ToString() : GlobalHelpers.ConvertToUTC(DateTime.Parse("2024-12-25 00:00:00.000")).ToString(),
                EventEndTime = x.RaceDateTime != null ? GlobalHelpers.ConvertToUTC(((DateTime)x.RaceDateTime).AddHours(3)).ToString() : GlobalHelpers.ConvertToUTC(DateTime.Parse("2024-12-25 01:00:00.000")).ToString()
            }).ToList();

            if (pairings.Count > 0)
            {
                pairings[0].NeedsUpdates = false;
                dbContext.SaveChanges();
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(threads), Encoding.UTF8, "application/json");
            return response;
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage UpdateInvitationalEvent(string hash, string pairingid, string eventid)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            tb_invitational_pairings pairing = dbContext.tb_invitational_pairings.Where(x => x.PairingID == pairingid).FirstOrDefault();

            pairing.EventID = eventid;
            pairing.NeedsUpdates = false;

            dbContext.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private void GenerateRestreamKeys(int pairing_id)
        {
            tb_invitational_pairings pairing = dbContext.tb_invitational_pairings.Where(x => x.pairing_id == pairing_id).FirstOrDefault();

            string webAddr = "https://alttprtracker.dunka.net/api/v1/RestreamerAPI/CreateNewKeys/2";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();

                RestreamKey restreamObject = JsonConvert.DeserializeObject<RestreamKey>(responseText);

                pairing.Participant1RestreamKey = restreamObject.ReturnItems[0].RestreamerCode;
                pairing.Participant2RestreamKey = restreamObject.ReturnItems[1].RestreamerCode;
                pairing.Participant1TrackingKey = restreamObject.ReturnItems[0].TrackingCode;
                pairing.Participant2TrackingKey = restreamObject.ReturnItems[1].TrackingCode;

                dbContext.SaveChanges();
            }
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public HttpResponseMessage CheckForRaceThreads(string hash)
        {
            if (!ValidateHash(hash))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            int currentinv = 8;

            int currentweek = dbContext.tb_invitationals.Where(x => x.invitational_id == currentinv).Select(x => x.CurrentWeek).FirstOrDefault();

            DateTime currentDT = DateTime.Now.AddMinutes(30);

            List<tb_invitational_pairings> pairinglist = dbContext.tb_invitational_pairings.Where(x => x.invitational_id == currentinv && x.Week == currentweek && x.RaceDateTime < currentDT && x.ThreadOpened == false).ToList();

            if (pairinglist.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "NO");
            }

            List<InvitationalPlayerInformation> playerinfo = new List<InvitationalPlayerInformation>();

            foreach (tb_invitational_pairings pairing in pairinglist)
            {
                GenerateRestreamKeys(pairing.pairing_id);

                tb_invitational_participants p1 = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant1_id).FirstOrDefault();
                tb_invitational_participants p2 = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant2_id).FirstOrDefault();

                tb_racers p1racer = dbContext.tb_racers.Where(x => x.racer_id == p1.racer_id).FirstOrDefault();
                tb_racers p2racer = dbContext.tb_racers.Where(x => x.racer_id == p2.racer_id).FirstOrDefault();

                InvitationalPlayerInformation info = new InvitationalPlayerInformation();

                info.RacerName1 = p1racer.RacerName;
                info.DiscordID1 = p1racer.DiscordID;
                info.RestreamKey1 = pairing.Participant1RestreamKey;
                info.TrackingKey1 = pairing.Participant1TrackingKey;
                info.GetsTrackingKey1 = pairing.HasRestream ? p1.SendTrackerCode : false;
                info.RacerName2 = p2racer.RacerName;
                info.DiscordID2 = p2racer.DiscordID;
                info.RestreamKey2 = pairing.Participant2RestreamKey;
                info.TrackingKey2 = pairing.Participant2TrackingKey;
                info.GetsTrackingKey2 = pairing.HasRestream ? p2.SendTrackerCode : false;
                info.GroupName = pairing.GroupName;

                playerinfo.Add(info);

                pairing.ThreadOpened = true;

                dbContext.SaveChanges();
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(playerinfo), Encoding.UTF8, "application/json");
            return response;
        }

        private static int ParseTime(string rtggtime)
        {
            string h = rtggtime.Substring(rtggtime.IndexOf("H") - 2, 2);
            string m = rtggtime.Substring(rtggtime.IndexOf("M") - 2, 2);
            string s = rtggtime.Substring(rtggtime.IndexOf("M") + 1, 2);

            return (int.Parse(h) * 3600) + (int.Parse(m) * 60) + int.Parse(s);

        }

        private SeedReturn GenerateSeed(int flag_id, string RaceName)
        {
            try
            {
                string json = "";

                SeedReturn seeddetails = new SeedReturn();

                tb_races currentrace = new tb_races();

                if (RaceName != "")
                {
                    currentrace = dbContext.tb_races.Where(x => x.RaceGUID.ToString().StartsWith(RaceName)).FirstOrDefault();

                    flag_id = currentrace.flag_id;
                }

                tb_flags flag = dbContext.tb_flags.Where(x => x.flag_id == flag_id).FirstOrDefault();

                var branchinfo = dbContext.tb_branches.Where(x => x.branch_id == flag.branch_id).FirstOrDefault();

                string webAddr = branchinfo.EndPointURL;

                string seedbaseurl = branchinfo.SeedURL;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                if (flag.branch_id == 3)
                {
                    httpWebRequest.Headers.Add("Authorization", ConfigurationManager.AppSettings["AvianartAuthKey"]);
                }

                if (flag_id == 25)
                {
                    //IF RACE IS GRAB BAG, DETERMINE THE RANDOM FLAG
                    var grabbagflags = dbContext.tb_flags.Where(x => x.IsGrabBag == true).ToList();

                    flag = grabbagflags[GlobalHelpers.RandomInt(grabbagflags.Count) - 1];

                    seeddetails.GrabBag = flag.FlagName;

                    branchinfo = dbContext.tb_branches.Where(x => x.branch_id == flag.branch_id).FirstOrDefault();

                    webAddr = branchinfo.EndPointURL;

                    seedbaseurl = branchinfo.SeedURL;

                    currentrace.grabbag_id = flag.flag_id;
                }

                if (flag.IsMystery == true && flag_id != 49)
                {
                    if (flag_id == 44)
                    {
                        json = GenerateChampionsMysterySeed();
                    }
                    else
                    {
                        json = GenerateMysterySeed();
                    }

                    currentrace.MysteryJSON = json;

                    dbContext.SaveChanges();

                    if (json.Contains("PegasusBoots"))
                    {
                        //Mystery overrides to customizer if it reads Boots in the json
                        branchinfo = dbContext.tb_branches.Where(x => x.branch_id == 2).FirstOrDefault();
                        webAddr = branchinfo.EndPointURL;
                        seedbaseurl = branchinfo.SeedURL;

                        httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                    }
                }
                else
                {
                    json = flag.FlagString;
                }

                //If is Invrosia, replace the check with a random hashed location
                if (json.Contains("INVROSIARBKCHECK"))
                {
                    json = json.Replace("INVROSIARBKCHECK", GTBKPlacement());
                }

                //Inject Troy's text in customizer branches
                if (flag.branch_id == 2)
                {
                    json = json.Replace("\"glitches\"", "\"texts\": {\"sign_north_of_lake\": \"{SPEED0}\\n{MENU}\\nRIP Troy 2010-2024\\nHe was the best boy\\nLove you buddy\"}, \"glitches\"");
                }

                //If from AvianArt and has a preset, override the json with default and send the preset as part of the query string
                if (flag.branch_id == 3)
                {
                    if (flag.FlagString.Contains("&preset="))
                    {
                        httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr + flag.FlagString);
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.Headers.Add("Authorization", ConfigurationManager.AppSettings["AvianartAuthKey"]);

                        if (flag_id == 50)
                        {
                            json = "[{\"args\":{\"race\":true,\"namespace\": \"jem041\"}}]";
                        }
                        else
                        {
                            json = "[{\"args\":{\"race\":true}}]";
                        }
                    }
                    else if (flag_id == 49)
                    {
                        httpWebRequest = (HttpWebRequest)WebRequest.Create("https://avianart.games/api.php?action=mystery");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.Headers.Add("Authorization", ConfigurationManager.AppSettings["AvianartAuthKey"]);
                    }
                }

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string seedurl = "";
                    string seedhash = "";

                    var responseText = streamReader.ReadToEnd();

                    //If from AvianArt, need to get the hash and then loop every 5 seconds for the seed
                    if (flag.branch_id == 3)
                    {
                        AvianArtReturn aar = JsonConvert.DeserializeObject<AvianArtReturn>(responseText);

                        DateTime delay = DateTime.Now.AddSeconds(5);

                        seedurl = aar.response.hash;

                        int maxwait = 30;
                        int currentwait = 0;

                        while (aar.response.status != null && currentwait < maxwait)
                        {
                            if (delay < DateTime.Now)
                            {
                                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://avianart.games/api.php?action=permlink&hash=" + seedurl);
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                httpWebRequest.Headers.Add("Authorization", ConfigurationManager.AppSettings["AvianartAuthKey"]);

                                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                                using (var streamReader2 = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    responseText = streamReader2.ReadToEnd();

                                    aar = JsonConvert.DeserializeObject<AvianArtReturn>(responseText);

                                    delay = DateTime.Now.AddSeconds(3);

                                    currentwait++;
                                }
                            }
                        }

                        seedhash = ConvertAvianHash(aar.response.spoiler.meta.hash);
                    }
                    else
                    {
                        if (flag.IsSpoiler == true)
                        {
                            SpoilerGenerated sg = JsonConvert.DeserializeObject<SpoilerGenerated>(responseText);

                            sg.spoiler.Drops = new Drops();
                            sg.spoiler.Drops.PullTree = new PullTree();
                            sg.spoiler.Drops.RupeeCrab = new RupeeCrab();
                            sg.spoiler.Drops.PrizePacks = new PrizePacks();

                            foreach (var d in sg.patch)
                            {
                                //DiggingGameDigs
                                if (d.ContainsKey("982421"))
                                {
                                    sg.spoiler.Special.DiggingGameDigs = d.Values.First()[0].ToString();
                                }
                                //Stun
                                else if (d.ContainsKey("227731"))
                                {
                                    sg.spoiler.Drops.Stun = GetSpriteOffset(d.Values.First()[0]);
                                }
                                //Pull Tree Tiers
                                else if (d.ContainsKey("981972"))
                                {
                                    sg.spoiler.Drops.PullTree.Tier1 = GetSpriteOffset(d.Values.First()[0]);
                                    sg.spoiler.Drops.PullTree.Tier2 = GetSpriteOffset(d.Values.First()[1]);
                                    sg.spoiler.Drops.PullTree.Tier3 = GetSpriteOffset(d.Values.First()[2]);
                                }
                                //Rupee Crab Main
                                else if (d.ContainsKey("207304"))
                                {
                                    sg.spoiler.Drops.RupeeCrab.Main = GetSpriteOffset(d.Values.First()[0]);
                                }
                                //Rupee Crab Final
                                else if (d.ContainsKey("207300"))
                                {
                                    sg.spoiler.Drops.RupeeCrab.Final = GetSpriteOffset(d.Values.First()[0]);
                                }
                                //Fish Save
                                else if (d.ContainsKey("950988"))
                                {
                                    sg.spoiler.Drops.FishSave = GetSpriteOffset(d.Values.First()[0]);
                                }
                                //Prize Packs
                                else if (d.ContainsKey("227960"))
                                {
                                    List<int> collectionlist = new List<int>();

                                    foreach (var v in d.Values.ElementAt(0))
                                    {
                                        collectionlist.Add(v);
                                    }

                                    sg.spoiler.Drops.PrizePacks.HeartsGroup = GetPrizePacks(collectionlist[0], collectionlist[1], collectionlist[2], collectionlist[3], collectionlist[4], collectionlist[5], collectionlist[6], collectionlist[7]);
                                    sg.spoiler.Drops.PrizePacks.RupeesGroup = GetPrizePacks(collectionlist[8], collectionlist[9], collectionlist[10], collectionlist[11], collectionlist[12], collectionlist[13], collectionlist[14], collectionlist[15]);
                                    sg.spoiler.Drops.PrizePacks.MagicGroup = GetPrizePacks(collectionlist[16], collectionlist[17], collectionlist[18], collectionlist[19], collectionlist[20], collectionlist[21], collectionlist[22], collectionlist[23]);
                                    sg.spoiler.Drops.PrizePacks.BombsGroup = GetPrizePacks(collectionlist[24], collectionlist[25], collectionlist[26], collectionlist[27], collectionlist[28], collectionlist[29], collectionlist[30], collectionlist[31]);
                                    sg.spoiler.Drops.PrizePacks.ArrowsGroup = GetPrizePacks(collectionlist[32], collectionlist[33], collectionlist[34], collectionlist[35], collectionlist[36], collectionlist[37], collectionlist[38], collectionlist[39]);
                                    sg.spoiler.Drops.PrizePacks.SmallVarietyGroup = GetPrizePacks(collectionlist[40], collectionlist[41], collectionlist[42], collectionlist[43], collectionlist[44], collectionlist[45], collectionlist[46], collectionlist[47]);
                                    sg.spoiler.Drops.PrizePacks.LargeVarietyGroup = GetPrizePacks(collectionlist[48], collectionlist[49], collectionlist[50], collectionlist[51], collectionlist[52], collectionlist[53], collectionlist[54], collectionlist[55]);
                                }
                            }

                            StreamReader sr = new StreamReader(string.Format("{0}openspoiler_template.txt", ConfigurationManager.AppSettings["OpenSpoilerTemplateLocation"]));

                            string SpoilerLogHash = GlobalHelpers.RandomString(24);

                            StreamWriter sw = new StreamWriter(string.Format("{0}{1}.txt", ConfigurationManager.AppSettings["OpenSpoilerTemplateLocation"], SpoilerLogHash));

                            string templateText = sr.ReadToEnd();

                            templateText = templateText.Replace("{Tier1}", sg.spoiler.Drops.PullTree.Tier1);
                            templateText = templateText.Replace("{Tier2}", sg.spoiler.Drops.PullTree.Tier2);
                            templateText = templateText.Replace("{Tier3}", sg.spoiler.Drops.PullTree.Tier3);

                            templateText = templateText.Replace("{Main}", sg.spoiler.Drops.RupeeCrab.Main);
                            templateText = templateText.Replace("{Final}", sg.spoiler.Drops.RupeeCrab.Final);

                            templateText = templateText.Replace("{Stun}", sg.spoiler.Drops.Stun);
                            templateText = templateText.Replace("{FishSave}", sg.spoiler.Drops.FishSave);

                            templateText = templateText.Replace("{HeartsGroup}", sg.spoiler.Drops.PrizePacks.HeartsGroup);
                            templateText = templateText.Replace("{RupeesGroup}", sg.spoiler.Drops.PrizePacks.RupeesGroup);
                            templateText = templateText.Replace("{MagicGroup}", sg.spoiler.Drops.PrizePacks.MagicGroup);
                            templateText = templateText.Replace("{BombsGroup}", sg.spoiler.Drops.PrizePacks.BombsGroup);
                            templateText = templateText.Replace("{ArrowsGroup}", sg.spoiler.Drops.PrizePacks.ArrowsGroup);
                            templateText = templateText.Replace("{SmallVarietyGroup}", sg.spoiler.Drops.PrizePacks.SmallVarietyGroup);
                            templateText = templateText.Replace("{LargeVarietyGroup}", sg.spoiler.Drops.PrizePacks.LargeVarietyGroup);

                            templateText = templateText.Replace("{Eastern Palace}", sg.spoiler.EasternPalace.EasternPalacePrize);
                            templateText = templateText.Replace("{Desert Palace}", sg.spoiler.DesertPalace.DesertPalacePrize);
                            templateText = templateText.Replace("{Tower Of Hera}", sg.spoiler.TowerOfHera.TowerofHeraPrize);
                            templateText = templateText.Replace("{Dark Palace}", sg.spoiler.DarkPalace.PalaceofDarknessPrize);
                            templateText = templateText.Replace("{Swamp Palace}", sg.spoiler.SwampPalace.SwampPalacePrize);
                            templateText = templateText.Replace("{Skull Woods}", sg.spoiler.SkullWoods.SkullWoodsPrize);
                            templateText = templateText.Replace("{Thieves Town}", sg.spoiler.ThievesTown.ThievesTownPrize);
                            templateText = templateText.Replace("{Ice Palace}", sg.spoiler.IcePalace.IcePalacePrize);
                            templateText = templateText.Replace("{Misery Mire}", sg.spoiler.MiseryMire.MiseryMirePrize);
                            templateText = templateText.Replace("{Turtle Rock}", sg.spoiler.TurtleRock.TurtleRockPrize);

                            templateText = templateText.Replace("{Turtle Rock Medallion}", sg.spoiler.Special.TurtleRockMedallion);
                            templateText = templateText.Replace("{Misery Mire Medallion}", sg.spoiler.Special.MiseryMireMedallion);
                            templateText = templateText.Replace("{Waterfall Bottle}", sg.spoiler.Special.WaterfallBottle);
                            templateText = templateText.Replace("{Pyramid Bottle}", sg.spoiler.Special.PyramidBottle);
                            templateText = templateText.Replace("{DiggingGameDigs}", sg.spoiler.Special.DiggingGameDigs);

                            templateText = templateText.Replace("{Sanctuary}", sg.spoiler.HyruleCastle.Sanctuary);
                            templateText = templateText.Replace("{Sewers - Secret Room - Left}", sg.spoiler.HyruleCastle.SewersSecretRoomLeft);
                            templateText = templateText.Replace("{Sewers - Secret Room - Middle}", sg.spoiler.HyruleCastle.SewersSecretRoomMiddle);
                            templateText = templateText.Replace("{Sewers - Secret Room - Right}", sg.spoiler.HyruleCastle.SewersSecretRoomRight);
                            templateText = templateText.Replace("{Sewers - Dark Cross}", sg.spoiler.HyruleCastle.SewersDarkCross);
                            templateText = templateText.Replace("{Hyrule Castle - Boomerang Chest}", sg.spoiler.HyruleCastle.HyruleCastleBoomerangChest);
                            templateText = templateText.Replace("{Hyrule Castle - Map Chest}", sg.spoiler.HyruleCastle.HyruleCastleMapChest);
                            templateText = templateText.Replace("{Hyrule Castle - Zelda's Cell}", sg.spoiler.HyruleCastle.HyruleCastleZeldaSCell);
                            templateText = templateText.Replace("{Link's Uncle}", sg.spoiler.HyruleCastle.LinkSUncle);
                            templateText = templateText.Replace("{Secret Passage}", sg.spoiler.HyruleCastle.SecretPassage);

                            templateText = templateText.Replace("{Eastern Palace - Compass Chest}", sg.spoiler.EasternPalace.EasternPalaceCompassChest);
                            templateText = templateText.Replace("{Eastern Palace - Big Chest}", sg.spoiler.EasternPalace.EasternPalaceBigChest);
                            templateText = templateText.Replace("{Eastern Palace - Cannonball Chest}", sg.spoiler.EasternPalace.EasternPalaceCannonballChest);
                            templateText = templateText.Replace("{Eastern Palace - Big Key Chest}", sg.spoiler.EasternPalace.EasternPalaceBigKeyChest);
                            templateText = templateText.Replace("{Eastern Palace - Map Chest}", sg.spoiler.EasternPalace.EasternPalaceMapChest);
                            templateText = templateText.Replace("{Eastern Palace - Boss}", sg.spoiler.EasternPalace.EasternPalaceBoss);

                            templateText = templateText.Replace("{Desert Palace - Big Chest}", sg.spoiler.DesertPalace.DesertPalaceBigChest);
                            templateText = templateText.Replace("{Desert Palace - Map Chest}", sg.spoiler.DesertPalace.DesertPalaceMapChest);
                            templateText = templateText.Replace("{Desert Palace - Torch}", sg.spoiler.DesertPalace.DesertPalaceTorch);
                            templateText = templateText.Replace("{Desert Palace - Big Key Chest}", sg.spoiler.DesertPalace.DesertPalaceBigKeyChest);
                            templateText = templateText.Replace("{Desert Palace - Compass Chest}", sg.spoiler.DesertPalace.DesertPalaceCompassChest);
                            templateText = templateText.Replace("{Desert Palace - Boss}", sg.spoiler.DesertPalace.DesertPalaceBoss);

                            templateText = templateText.Replace("{Tower of Hera - Big Key Chest}", sg.spoiler.TowerOfHera.TowerofHeraBigKeyChest);
                            templateText = templateText.Replace("{Tower of Hera - Basement Cage}", sg.spoiler.TowerOfHera.TowerofHeraBasementCage);
                            templateText = templateText.Replace("{Tower of Hera - Map Chest}", sg.spoiler.TowerOfHera.TowerofHeraMapChest);
                            templateText = templateText.Replace("{Tower of Hera - Compass Chest}", sg.spoiler.TowerOfHera.TowerofHeraCompassChest);
                            templateText = templateText.Replace("{Tower of Hera - Big Chest}", sg.spoiler.TowerOfHera.TowerofHeraBigChest);
                            templateText = templateText.Replace("{Tower of Hera - Boss}", sg.spoiler.TowerOfHera.TowerofHeraBoss);

                            templateText = templateText.Replace("{Castle Tower - Room 03}", sg.spoiler.CastleTower.CastleTowerRoom03);
                            templateText = templateText.Replace("{Castle Tower - Dark Maze}", sg.spoiler.CastleTower.CastleTowerDarkMaze);

                            templateText = templateText.Replace("{Palace of Darkness - Shooter Room}", sg.spoiler.DarkPalace.PalaceofDarknessShooterRoom);
                            templateText = templateText.Replace("{Palace of Darkness - Big Key Chest}", sg.spoiler.DarkPalace.PalaceofDarknessBigKeyChest);
                            templateText = templateText.Replace("{Palace of Darkness - The Arena - Ledge}", sg.spoiler.DarkPalace.PalaceofDarknessTheArenaLedge);
                            templateText = templateText.Replace("{Palace of Darkness - The Arena - Bridge}", sg.spoiler.DarkPalace.PalaceofDarknessTheArenaBridge);
                            templateText = templateText.Replace("{Palace of Darkness - Stalfos Basement}", sg.spoiler.DarkPalace.PalaceofDarknessStalfosBasement);
                            templateText = templateText.Replace("{Palace of Darkness - Map Chest}", sg.spoiler.DarkPalace.PalaceofDarknessMapChest);
                            templateText = templateText.Replace("{Palace of Darkness - Big Chest}", sg.spoiler.DarkPalace.PalaceofDarknessBigChest);
                            templateText = templateText.Replace("{Palace of Darkness - Compass Chest}", sg.spoiler.DarkPalace.PalaceofDarknessCompassChest);
                            templateText = templateText.Replace("{Palace of Darkness - Harmless Hellway}", sg.spoiler.DarkPalace.PalaceofDarknessHarmlessHellway);
                            templateText = templateText.Replace("{Palace of Darkness - Dark Basement - Left}", sg.spoiler.DarkPalace.PalaceofDarknessDarkBasementLeft);
                            templateText = templateText.Replace("{Palace of Darkness - Dark Basement - Right}", sg.spoiler.DarkPalace.PalaceofDarknessDarkBasementRight);
                            templateText = templateText.Replace("{Palace of Darkness - Dark Maze - Top}", sg.spoiler.DarkPalace.PalaceofDarknessDarkMazeTop);
                            templateText = templateText.Replace("{Palace of Darkness - Dark Maze - Bottom}", sg.spoiler.DarkPalace.PalaceofDarknessDarkMazeBottom);
                            templateText = templateText.Replace("{Palace of Darkness - Boss}", sg.spoiler.DarkPalace.PalaceofDarknessBoss);

                            templateText = templateText.Replace("{Swamp Palace - Entrance}", sg.spoiler.SwampPalace.SwampPalaceEntrance);
                            templateText = templateText.Replace("{Swamp Palace - Big Chest}", sg.spoiler.SwampPalace.SwampPalaceBigChest);
                            templateText = templateText.Replace("{Swamp Palace - Big Key Chest}", sg.spoiler.SwampPalace.SwampPalaceBigKeyChest);
                            templateText = templateText.Replace("{Swamp Palace - Map Chest}", sg.spoiler.SwampPalace.SwampPalaceMapChest);
                            templateText = templateText.Replace("{Swamp Palace - West Chest}", sg.spoiler.SwampPalace.SwampPalaceWestChest);
                            templateText = templateText.Replace("{Swamp Palace - Compass Chest}", sg.spoiler.SwampPalace.SwampPalaceCompassChest);
                            templateText = templateText.Replace("{Swamp Palace - Flooded Room - Left}", sg.spoiler.SwampPalace.SwampPalaceFloodedRoomLeft);
                            templateText = templateText.Replace("{Swamp Palace - Flooded Room - Right}", sg.spoiler.SwampPalace.SwampPalaceFloodedRoomRight);
                            templateText = templateText.Replace("{Swamp Palace - Waterfall Room}", sg.spoiler.SwampPalace.SwampPalaceWaterfallRoom);
                            templateText = templateText.Replace("{Swamp Palace - Boss}", sg.spoiler.SwampPalace.SwampPalaceBoss);

                            templateText = templateText.Replace("{Skull Woods - Big Chest}", sg.spoiler.SkullWoods.SkullWoodsBigChest);
                            templateText = templateText.Replace("{Skull Woods - Big Key Chest}", sg.spoiler.SkullWoods.SkullWoodsBigKeyChest);
                            templateText = templateText.Replace("{Skull Woods - Compass Chest}", sg.spoiler.SkullWoods.SkullWoodsCompassChest);
                            templateText = templateText.Replace("{Skull Woods - Map Chest}", sg.spoiler.SkullWoods.SkullWoodsMapChest);
                            templateText = templateText.Replace("{Skull Woods - Bridge Room}", sg.spoiler.SkullWoods.SkullWoodsBridgeRoom);
                            templateText = templateText.Replace("{Skull Woods - Pot Prison}", sg.spoiler.SkullWoods.SkullWoodsPotPrison);
                            templateText = templateText.Replace("{Skull Woods - Pinball Room}", sg.spoiler.SkullWoods.SkullWoodsPinballRoom);
                            templateText = templateText.Replace("{Skull Woods - Boss}", sg.spoiler.SkullWoods.SkullWoodsBoss);

                            templateText = templateText.Replace("{Thieves' Town - Attic}", sg.spoiler.ThievesTown.ThievesTownAttic);
                            templateText = templateText.Replace("{Thieves' Town - Big Key Chest}", sg.spoiler.ThievesTown.ThievesTownBigKeyChest);
                            templateText = templateText.Replace("{Thieves' Town - Map Chest}", sg.spoiler.ThievesTown.ThievesTownMapChest);
                            templateText = templateText.Replace("{Thieves' Town - Compass Chest}", sg.spoiler.ThievesTown.ThievesTownCompassChest);
                            templateText = templateText.Replace("{Thieves' Town - Ambush Chest}", sg.spoiler.ThievesTown.ThievesTownAmbushChest);
                            templateText = templateText.Replace("{Thieves' Town - Big Chest}", sg.spoiler.ThievesTown.ThievesTownBigChest);
                            templateText = templateText.Replace("{Thieves' Town - Blind's Cell}", sg.spoiler.ThievesTown.ThievesTownBlindSCell);
                            templateText = templateText.Replace("{Thieves' Town - Boss}", sg.spoiler.ThievesTown.ThievesTownBoss);

                            templateText = templateText.Replace("{Ice Palace - Big Key Chest}", sg.spoiler.IcePalace.IcePalaceBigKeyChest);
                            templateText = templateText.Replace("{Ice Palace - Compass Chest}", sg.spoiler.IcePalace.IcePalaceCompassChest);
                            templateText = templateText.Replace("{Ice Palace - Map Chest}", sg.spoiler.IcePalace.IcePalaceMapChest);
                            templateText = templateText.Replace("{Ice Palace - Spike Room}", sg.spoiler.IcePalace.IcePalaceSpikeRoom);
                            templateText = templateText.Replace("{Ice Palace - Freezor Chest}", sg.spoiler.IcePalace.IcePalaceFreezorChest);
                            templateText = templateText.Replace("{Ice Palace - Iced T Room}", sg.spoiler.IcePalace.IcePalaceIcedTRoom);
                            templateText = templateText.Replace("{Ice Palace - Big Chest}", sg.spoiler.IcePalace.IcePalaceBigChest);
                            templateText = templateText.Replace("{Ice Palace - Boss}", sg.spoiler.IcePalace.IcePalaceBoss);

                            templateText = templateText.Replace("{Misery Mire - Big Chest}", sg.spoiler.MiseryMire.MiseryMireBigChest);
                            templateText = templateText.Replace("{Misery Mire - Main Lobby}", sg.spoiler.MiseryMire.MiseryMireMainLobby);
                            templateText = templateText.Replace("{Misery Mire - Big Key Chest}", sg.spoiler.MiseryMire.MiseryMireBigKeyChest);
                            templateText = templateText.Replace("{Misery Mire - Compass Chest}", sg.spoiler.MiseryMire.MiseryMireCompassChest);
                            templateText = templateText.Replace("{Misery Mire - Bridge Chest}", sg.spoiler.MiseryMire.MiseryMireBridgeChest);
                            templateText = templateText.Replace("{Misery Mire - Map Chest}", sg.spoiler.MiseryMire.MiseryMireMapChest);
                            templateText = templateText.Replace("{Misery Mire - Spike Chest}", sg.spoiler.MiseryMire.MiseryMireSpikeChest);
                            templateText = templateText.Replace("{Misery Mire - Boss}", sg.spoiler.MiseryMire.MiseryMireBoss);

                            templateText = templateText.Replace("{Turtle Rock - Chain Chomps}", sg.spoiler.TurtleRock.TurtleRockChainChomps);
                            templateText = templateText.Replace("{Turtle Rock - Compass Chest}", sg.spoiler.TurtleRock.TurtleRockCompassChest);
                            templateText = templateText.Replace("{Turtle Rock - Roller Room - Left}", sg.spoiler.TurtleRock.TurtleRockRollerRoomLeft);
                            templateText = templateText.Replace("{Turtle Rock - Roller Room - Right}", sg.spoiler.TurtleRock.TurtleRockRollerRoomRight);
                            templateText = templateText.Replace("{Turtle Rock - Big Chest}", sg.spoiler.TurtleRock.TurtleRockBigChest);
                            templateText = templateText.Replace("{Turtle Rock - Big Key Chest}", sg.spoiler.TurtleRock.TurtleRockBigKeyChest);
                            templateText = templateText.Replace("{Turtle Rock - Crystaroller Room}", sg.spoiler.TurtleRock.TurtleRockCrystarollerRoom);
                            templateText = templateText.Replace("{Turtle Rock - Eye Bridge - Bottom Left}", sg.spoiler.TurtleRock.TurtleRockEyeBridgeBottomLeft);
                            templateText = templateText.Replace("{Turtle Rock - Eye Bridge - Bottom Right}", sg.spoiler.TurtleRock.TurtleRockEyeBridgeBottomRight);
                            templateText = templateText.Replace("{Turtle Rock - Eye Bridge - Top Left}", sg.spoiler.TurtleRock.TurtleRockEyeBridgeTopLeft);
                            templateText = templateText.Replace("{Turtle Rock - Eye Bridge - Top Right}", sg.spoiler.TurtleRock.TurtleRockEyeBridgeTopRight);
                            templateText = templateText.Replace("{Turtle Rock - Boss}", sg.spoiler.TurtleRock.TurtleRockBoss);

                            templateText = templateText.Replace("{Ganon's Tower - Bob's Torch}", sg.spoiler.GanonsTower.GanonSTowerBobSTorch);
                            templateText = templateText.Replace("{Ganon's Tower - DMs Room - Top Left}", sg.spoiler.GanonsTower.GanonSTowerDMsRoomTopLeft);
                            templateText = templateText.Replace("{Ganon's Tower - DMs Room - Top Right}", sg.spoiler.GanonsTower.GanonSTowerDMsRoomTopRight);
                            templateText = templateText.Replace("{Ganon's Tower - DMs Room - Bottom Left}", sg.spoiler.GanonsTower.GanonSTowerDMsRoomBottomLeft);
                            templateText = templateText.Replace("{Ganon's Tower - DMs Room - Bottom Right}", sg.spoiler.GanonsTower.GanonSTowerDMsRoomBottomRight);
                            templateText = templateText.Replace("{Ganon's Tower - Randomizer Room - Top Left}", sg.spoiler.GanonsTower.GanonSTowerRandomizerRoomTopLeft);
                            templateText = templateText.Replace("{Ganon's Tower - Randomizer Room - Top Right}", sg.spoiler.GanonsTower.GanonSTowerRandomizerRoomTopRight);
                            templateText = templateText.Replace("{Ganon's Tower - Randomizer Room - Bottom Left}", sg.spoiler.GanonsTower.GanonSTowerRandomizerRoomBottomLeft);
                            templateText = templateText.Replace("{Ganon's Tower - Randomizer Room - Bottom Right}", sg.spoiler.GanonsTower.GanonSTowerRandomizerRoomBottomRight);
                            templateText = templateText.Replace("{Ganon's Tower - Firesnake Room}", sg.spoiler.GanonsTower.GanonSTowerFiresnakeRoom);
                            templateText = templateText.Replace("{Ganon's Tower - Map Chest}", sg.spoiler.GanonsTower.GanonSTowerMapChest);
                            templateText = templateText.Replace("{Ganon's Tower - Big Chest}", sg.spoiler.GanonsTower.GanonSTowerBigChest);
                            templateText = templateText.Replace("{Ganon's Tower - Hope Room - Left}", sg.spoiler.GanonsTower.GanonSTowerHopeRoomLeft);
                            templateText = templateText.Replace("{Ganon's Tower - Hope Room - Right}", sg.spoiler.GanonsTower.GanonSTowerHopeRoomRight);
                            templateText = templateText.Replace("{Ganon's Tower - Bob's Chest}", sg.spoiler.GanonsTower.GanonSTowerBobSChest);
                            templateText = templateText.Replace("{Ganon's Tower - Tile Room}", sg.spoiler.GanonsTower.GanonSTowerTileRoom);
                            templateText = templateText.Replace("{Ganon's Tower - Compass Room - Top Left}", sg.spoiler.GanonsTower.GanonSTowerCompassRoomTopLeft);
                            templateText = templateText.Replace("{Ganon's Tower - Compass Room - Top Right}", sg.spoiler.GanonsTower.GanonSTowerCompassRoomTopRight);
                            templateText = templateText.Replace("{Ganon's Tower - Compass Room - Bottom Left}", sg.spoiler.GanonsTower.GanonSTowerCompassRoomBottomLeft);
                            templateText = templateText.Replace("{Ganon's Tower - Compass Room - Bottom Right}", sg.spoiler.GanonsTower.GanonSTowerCompassRoomBottomRight);
                            templateText = templateText.Replace("{Ganon's Tower - Big Key Chest}", sg.spoiler.GanonsTower.GanonSTowerBigKeyChest);
                            templateText = templateText.Replace("{Ganon's Tower - Big Key Room - Left}", sg.spoiler.GanonsTower.GanonSTowerBigKeyRoomLeft);
                            templateText = templateText.Replace("{Ganon's Tower - Big Key Room - Right}", sg.spoiler.GanonsTower.GanonSTowerBigKeyRoomRight);
                            templateText = templateText.Replace("{Ganon's Tower - Mini Helmasaur Room - Left}", sg.spoiler.GanonsTower.GanonSTowerMiniHelmasaurRoomLeft);
                            templateText = templateText.Replace("{Ganon's Tower - Mini Helmasaur Room - Right}", sg.spoiler.GanonsTower.GanonSTowerMiniHelmasaurRoomRight);
                            templateText = templateText.Replace("{Ganon's Tower - Pre-Moldorm Chest}", sg.spoiler.GanonsTower.GanonSTowerPreMoldormChest);
                            templateText = templateText.Replace("{Ganon's Tower - Moldorm Chest}", sg.spoiler.GanonsTower.GanonSTowerMoldormChest);

                            templateText = templateText.Replace("{Sahasrahla's Hut - Left}", sg.spoiler.LightWorld.SahasrahlaSHutLeft);
                            templateText = templateText.Replace("{Sahasrahla's Hut - Middle}", sg.spoiler.LightWorld.SahasrahlaSHutMiddle);
                            templateText = templateText.Replace("{Sahasrahla's Hut - Right}", sg.spoiler.LightWorld.SahasrahlaSHutRight);
                            templateText = templateText.Replace("{Sahasrahla}", sg.spoiler.LightWorld.Sahasrahla);
                            templateText = templateText.Replace("{King Zora}", sg.spoiler.LightWorld.KingZora);
                            templateText = templateText.Replace("{Potion Shop}", sg.spoiler.LightWorld.PotionShop);
                            templateText = templateText.Replace("{Zora's Ledge}", sg.spoiler.LightWorld.ZoraSLedge);
                            templateText = templateText.Replace("{Waterfall Fairy - Left}", sg.spoiler.LightWorld.WaterfallFairyLeft);
                            templateText = templateText.Replace("{Waterfall Fairy - Right}", sg.spoiler.LightWorld.WaterfallFairyRight);
                            templateText = templateText.Replace("{Master Sword Pedestal}", sg.spoiler.LightWorld.MasterSwordPedestal);
                            templateText = templateText.Replace("{King's Tomb}", sg.spoiler.LightWorld.KingSTomb);
                            templateText = templateText.Replace("{Kakariko Tavern}", sg.spoiler.LightWorld.KakarikoTavern);
                            templateText = templateText.Replace("{Chicken House}", sg.spoiler.LightWorld.ChickenHouse);
                            templateText = templateText.Replace("{Kakariko Well - Top}", sg.spoiler.LightWorld.KakarikoWellTop);
                            templateText = templateText.Replace("{Kakariko Well - Left}", sg.spoiler.LightWorld.KakarikoWellLeft);
                            templateText = templateText.Replace("{Kakariko Well - Middle}", sg.spoiler.LightWorld.KakarikoWellMiddle);
                            templateText = templateText.Replace("{Kakariko Well - Right}", sg.spoiler.LightWorld.KakarikoWellRight);
                            templateText = templateText.Replace("{Kakariko Well - Bottom}", sg.spoiler.LightWorld.KakarikoWellBottom);
                            templateText = templateText.Replace("{Blind's Hideout - Top}", sg.spoiler.LightWorld.BlindSHideoutTop);
                            templateText = templateText.Replace("{Blind's Hideout - Left}", sg.spoiler.LightWorld.BlindSHideoutLeft);
                            templateText = templateText.Replace("{Blind's Hideout - Right}", sg.spoiler.LightWorld.BlindSHideoutRight);
                            templateText = templateText.Replace("{Blind's Hideout - Far Left}", sg.spoiler.LightWorld.BlindSHideoutFarLeft);
                            templateText = templateText.Replace("{Blind's Hideout - Far Right}", sg.spoiler.LightWorld.BlindSHideoutFarRight);
                            templateText = templateText.Replace("{Pegasus Rocks}", sg.spoiler.LightWorld.PegasusRocks);
                            templateText = templateText.Replace("{Bottle Merchant}", sg.spoiler.LightWorld.BottleMerchant);
                            templateText = templateText.Replace("{Magic Bat}", sg.spoiler.LightWorld.MagicBat);
                            templateText = templateText.Replace("{Sick Kid}", sg.spoiler.LightWorld.SickKid);
                            templateText = templateText.Replace("{Lost Woods Hideout}", sg.spoiler.LightWorld.LostWoodsHideout);
                            templateText = templateText.Replace("{Lumberjack Tree}", sg.spoiler.LightWorld.LumberjackTree);
                            templateText = templateText.Replace("{Graveyard Ledge}", sg.spoiler.LightWorld.GraveyardLedge);
                            templateText = templateText.Replace("{Mushroom}", sg.spoiler.LightWorld.Mushroom);
                            templateText = templateText.Replace("{Floodgate Chest}", sg.spoiler.LightWorld.FloodgateChest);
                            templateText = templateText.Replace("{Link's House}", sg.spoiler.LightWorld.LinkSHouse);
                            templateText = templateText.Replace("{Aginah's Cave}", sg.spoiler.LightWorld.AginahSCave);
                            templateText = templateText.Replace("{Mini Moldorm Cave - Far Left}", sg.spoiler.LightWorld.MiniMoldormCaveFarLeft);
                            templateText = templateText.Replace("{Mini Moldorm Cave - Left}", sg.spoiler.LightWorld.MiniMoldormCaveLeft);
                            templateText = templateText.Replace("{Mini Moldorm Cave - Right}", sg.spoiler.LightWorld.MiniMoldormCaveRight);
                            templateText = templateText.Replace("{Mini Moldorm Cave - Far Right}", sg.spoiler.LightWorld.MiniMoldormCaveFarRight);
                            templateText = templateText.Replace("{Ice Rod Cave}", sg.spoiler.LightWorld.IceRodCave);
                            templateText = templateText.Replace("{Hobo}", sg.spoiler.LightWorld.Hobo);
                            templateText = templateText.Replace("{Bombos Tablet}", sg.spoiler.LightWorld.BombosTablet);
                            templateText = templateText.Replace("{Cave 45}", sg.spoiler.LightWorld.Cave45);
                            templateText = templateText.Replace("{Checkerboard Cave}", sg.spoiler.LightWorld.CheckerboardCave);
                            templateText = templateText.Replace("{Mini Moldorm Cave - NPC}", sg.spoiler.LightWorld.MiniMoldormCaveNPC);
                            templateText = templateText.Replace("{Library}", sg.spoiler.LightWorld.Library);
                            templateText = templateText.Replace("{Maze Race}", sg.spoiler.LightWorld.MazeRace);
                            templateText = templateText.Replace("{Desert Ledge}", sg.spoiler.LightWorld.DesertLedge);
                            templateText = templateText.Replace("{Lake Hylia Island}", sg.spoiler.LightWorld.LakeHyliaIsland);
                            templateText = templateText.Replace("{Sunken Treasure}", sg.spoiler.LightWorld.SunkenTreasure);
                            templateText = templateText.Replace("{Flute Spot}", sg.spoiler.LightWorld.FluteSpot);

                            templateText = templateText.Replace("{Old Man}", sg.spoiler.DeathMountain.OldMan);
                            templateText = templateText.Replace("{Spectacle Rock Cave}", sg.spoiler.DeathMountain.SpectacleRockCave);
                            templateText = templateText.Replace("{Spiral Cave}", sg.spoiler.DeathMountain.SpiralCave);
                            templateText = templateText.Replace("{Mimic Cave}", sg.spoiler.DeathMountain.MimicCave);
                            templateText = templateText.Replace("{Paradox Cave Lower - Far Left}", sg.spoiler.DeathMountain.ParadoxCaveLowerFarLeft);
                            templateText = templateText.Replace("{Paradox Cave Lower - Left}", sg.spoiler.DeathMountain.ParadoxCaveLowerLeft);
                            templateText = templateText.Replace("{Paradox Cave Lower - Right}", sg.spoiler.DeathMountain.ParadoxCaveLowerRight);
                            templateText = templateText.Replace("{Paradox Cave Lower - Far Right}", sg.spoiler.DeathMountain.ParadoxCaveLowerFarRight);
                            templateText = templateText.Replace("{Paradox Cave Lower - Middle}", sg.spoiler.DeathMountain.ParadoxCaveLowerMiddle);
                            templateText = templateText.Replace("{Paradox Cave Upper - Left}", sg.spoiler.DeathMountain.ParadoxCaveUpperLeft);
                            templateText = templateText.Replace("{Paradox Cave Upper - Right}", sg.spoiler.DeathMountain.ParadoxCaveUpperRight);
                            templateText = templateText.Replace("{Floating Island}", sg.spoiler.DeathMountain.FloatingIsland);
                            templateText = templateText.Replace("{Ether Tablet}", sg.spoiler.DeathMountain.EtherTablet);
                            templateText = templateText.Replace("{Spectacle Rock}", sg.spoiler.DeathMountain.SpectacleRock);

                            templateText = templateText.Replace("{Superbunny Cave - Top}", sg.spoiler.DarkWorld.SuperbunnyCaveTop);
                            templateText = templateText.Replace("{Superbunny Cave - Bottom}", sg.spoiler.DarkWorld.SuperbunnyCaveBottom);
                            templateText = templateText.Replace("{Hookshot Cave - Top Right}", sg.spoiler.DarkWorld.HookshotCaveTopRight);
                            templateText = templateText.Replace("{Hookshot Cave - Top Left}", sg.spoiler.DarkWorld.HookshotCaveTopLeft);
                            templateText = templateText.Replace("{Hookshot Cave - Bottom Left}", sg.spoiler.DarkWorld.HookshotCaveBottomLeft);
                            templateText = templateText.Replace("{Hookshot Cave - Bottom Right}", sg.spoiler.DarkWorld.HookshotCaveBottomRight);
                            templateText = templateText.Replace("{Spike Cave}", sg.spoiler.DarkWorld.SpikeCave);
                            templateText = templateText.Replace("{Catfish}", sg.spoiler.DarkWorld.Catfish);
                            templateText = templateText.Replace("{Pyramid}", sg.spoiler.DarkWorld.Pyramid);
                            templateText = templateText.Replace("{Pyramid Fairy - Left}", sg.spoiler.DarkWorld.PyramidFairyLeft);
                            templateText = templateText.Replace("{Pyramid Fairy - Right}", sg.spoiler.DarkWorld.PyramidFairyRight);
                            templateText = templateText.Replace("{Brewery}", sg.spoiler.DarkWorld.Brewery);
                            templateText = templateText.Replace("{C-Shaped House}", sg.spoiler.DarkWorld.CShapedHouse);
                            templateText = templateText.Replace("{Chest Game}", sg.spoiler.DarkWorld.ChestGame);
                            templateText = templateText.Replace("{Hammer Pegs}", sg.spoiler.DarkWorld.HammerPegs);
                            templateText = templateText.Replace("{Bumper Cave}", sg.spoiler.DarkWorld.BumperCave);
                            templateText = templateText.Replace("{Blacksmith}", sg.spoiler.DarkWorld.Blacksmith);
                            templateText = templateText.Replace("{Purple Chest}", sg.spoiler.DarkWorld.PurpleChest);
                            templateText = templateText.Replace("{Hype Cave - Top}", sg.spoiler.DarkWorld.HypeCaveTop);
                            templateText = templateText.Replace("{Hype Cave - Middle Right}", sg.spoiler.DarkWorld.HypeCaveMiddleRight);
                            templateText = templateText.Replace("{Hype Cave - Middle Left}", sg.spoiler.DarkWorld.HypeCaveMiddleLeft);
                            templateText = templateText.Replace("{Hype Cave - Bottom}", sg.spoiler.DarkWorld.HypeCaveBottom);
                            templateText = templateText.Replace("{Stumpy}", sg.spoiler.DarkWorld.Stumpy);
                            templateText = templateText.Replace("{Hype Cave - NPC}", sg.spoiler.DarkWorld.HypeCaveNPC);
                            templateText = templateText.Replace("{Digging Game}", sg.spoiler.DarkWorld.DiggingGame);
                            templateText = templateText.Replace("{Mire Shed - Left}", sg.spoiler.DarkWorld.MireShedLeft);
                            templateText = templateText.Replace("{Mire Shed - Right}", sg.spoiler.DarkWorld.MireShedRight);

                            templateText = templateText.Replace(":1", "");

                            templateText = templateText.Replace("H2", "H2-HyruleCastle");
                            templateText = templateText.Replace("P1", "P1-EasternPalace");
                            templateText = templateText.Replace("P2", "P2-DesertPalace");
                            templateText = templateText.Replace("P3", "P3-TowerOfHera");
                            templateText = templateText.Replace("A1", "A1-CastleTower");
                            templateText = templateText.Replace("D1", "D1-PalaceOfDarkness");
                            templateText = templateText.Replace("D2", "D2-SwampPalace");
                            templateText = templateText.Replace("D3", "D3-SkullWoods");
                            templateText = templateText.Replace("D4", "D4-ThievesTown");
                            templateText = templateText.Replace("D5", "D5-IcePalace");
                            templateText = templateText.Replace("D6", "D6-MiseryMire");
                            templateText = templateText.Replace("D7", "D7-TurtleRock");
                            templateText = templateText.Replace("A2", "A2-GanonsTower");

                            sw.Write(templateText);

                            sw.Close();
                            sr.Dispose();

                            tb_spoilers sp = new tb_spoilers();

                            sp.SpoilerHash = SpoilerLogHash;
                            sp.race_id = currentrace.race_id;
                            sp.FileLocation = string.Format("{0}{1}.txt", ConfigurationManager.AppSettings["OpenSpoilerTemplateLocation"], SpoilerLogHash);

                            dbContext.tb_spoilers.Add(sp);

                            dbContext.SaveChanges();

                            seeddetails.SpoilerHash = SpoilerLogHash;
                        }

                        seedurl = responseText.Substring(responseText.IndexOf("hash\":\"") + 7);

                        seedurl = seedurl.Substring(0, 10);

                        seedhash = "";

                        if (responseText.IndexOf("1573395") > -1)
                        {
                            //Customizer?
                            seedhash = responseText.Substring(responseText.IndexOf("1573395") + 14);
                        }
                        else
                        {
                            seedhash = responseText.Substring(responseText.IndexOf("1573397") + 14);
                        }

                        seedhash = seedhash.Substring(0, seedhash.IndexOf("]"));
                    }

                    seeddetails.SeedHash = seedhash;
                    seeddetails.Hours = flag.HoursToComplete;
                    seeddetails.FlagName = flag.FlagName;

                    if (RaceName != "")
                    {
                        tb_seeds seed = dbContext.tb_seeds.Where(x => x.race_id == currentrace.race_id).FirstOrDefault();

                        if (seed == null)
                        {
                            seed = new tb_seeds();

                            seed.race_id = currentrace.race_id;
                            seed.SeedHash = GlobalHelpers.RandomString(16);

                            dbContext.tb_seeds.Add(seed);
                        }
                        else
                        {
                            seed.SeedHash = GlobalHelpers.RandomString(16);
                        }

                        currentrace.SeedURL = seedbaseurl + seedurl;

                        dbContext.SaveChanges();

                        seeddetails.SeedURL = "https://alttprladder.com/seeds/getseed/" + seed.SeedHash;
                    }
                    else
                    {
                        seeddetails.SeedURL = seedbaseurl + seedurl;
                    }
                }

                seeddetails.valid = true;

                return seeddetails;
            }
            catch (Exception ex)
            {
                tb_logs logs = new tb_logs();

                logs.DateTimeLogged = DateTime.Now;
                logs.LogMessage = ex.ToString();

                dbContext.tb_logs.Add(logs);

                dbContext.SaveChanges();

                if (ErrorAttempts < 5)
                {
                    ErrorAttempts++;
                    return GenerateSeed(flag_id, RaceName);
                }
                else
                {
                    SeedReturn badseed = new SeedReturn();

                    badseed.valid = false;

                    return badseed;
                }
            }

            return null;
        }

        private string ConvertAvianHash(string hash)
        {
            string stringreturn = "";
            List<string> hasharray = hash.Split(',').ToList();

            for (int i = 0; i < 5; i++)
            {
                switch (hasharray[i].Trim())
                {
                    case "Bow":
                        stringreturn += "0";
                        break;
                    case "Boomerang":
                        stringreturn += "1";
                        break;
                    case "Hookshot":
                        stringreturn += "2";
                        break;
                    case "Bomb":
                        stringreturn += "3";
                        break;
                    case "Mushroom":
                        stringreturn += "4";
                        break;
                    case "Powder":
                        stringreturn += "5";
                        break;
                    case "Rod":
                        stringreturn += "6";
                        break;
                    case "Pendant":
                        stringreturn += "7";
                        break;
                    case "Bombos":
                        stringreturn += "8";
                        break;
                    case "Ether":
                        stringreturn += "9";
                        break;
                    case "Quake":
                        stringreturn += "10";
                        break;
                    case "Lamp":
                        stringreturn += "11";
                        break;
                    case "Hammer":
                        stringreturn += "12";
                        break;
                    case "Shovel":
                        stringreturn += "13";
                        break;
                    case "Ocarina":
                        stringreturn += "14";
                        break;
                    case "Bug Net":
                        stringreturn += "15";
                        break;
                    case "Book":
                        stringreturn += "16";
                        break;
                    case "Bottle":
                        stringreturn += "17";
                        break;
                    case "Potion":
                        stringreturn += "18";
                        break;
                    case "Cane":
                        stringreturn += "19";
                        break;
                    case "Cape":
                        stringreturn += "20";
                        break;
                    case "Mirror":
                        stringreturn += "21";
                        break;
                    case "Boots":
                        stringreturn += "22";
                        break;
                    case "Gloves":
                        stringreturn += "23";
                        break;
                    case "Flippers":
                        stringreturn += "24";
                        break;
                    case "Pearl":
                        stringreturn += "25";
                        break;
                    case "Shield":
                        stringreturn += "26";
                        break;
                    case "Tunic":
                        stringreturn += "27";
                        break;
                    case "Heart":
                        stringreturn += "28";
                        break;
                    case "Map":
                        stringreturn += "29";
                        break;
                    case "Compass":
                        stringreturn += "30";
                        break;
                    case "Key":
                        stringreturn += "31";
                        break;
                    default:
                        stringreturn += hasharray[i].Trim();
                        break;
                }

                if (i < 4)
                {
                    stringreturn += ",";
                }
            }

            return stringreturn;
        }

        private string GenerateMysterySeed()
        {
            string json = "";

            bool isentrance = (GlobalHelpers.GetRandomNumber(0, 10) < 3 ? true : false);

            try
            {
                //Goal
                int goalindex = -1;
                List<MysteryChoice> goalChoices = new List<MysteryChoice>();
                goalChoices.Add(new MysteryChoice("ganon", 30));
                goalChoices.Add(new MysteryChoice("fast_ganon", 30));
                goalChoices.Add(new MysteryChoice("dungeons", (isentrance ? 25 : 20)));
                goalChoices.Add(new MysteryChoice("pedestal", 10));
                goalChoices.Add(new MysteryChoice("triforce-hunt", 5));
                if (!isentrance)
                {
                    goalChoices.Add(new MysteryChoice("completionist", 5));
                }
                MysteryCategory goalCategory = new MysteryCategory("goal", GetMinValue(goalChoices), GetMaxValue(goalChoices), goalChoices);

                //Tower Open
                int toweropenindex = -1;
                List<MysteryChoice> towerChoices = new List<MysteryChoice>();
                towerChoices.Add(new MysteryChoice("0", 5));
                towerChoices.Add(new MysteryChoice("1", 10));
                towerChoices.Add(new MysteryChoice("2", 10));
                towerChoices.Add(new MysteryChoice("3", 15));
                towerChoices.Add(new MysteryChoice("4", 15));
                towerChoices.Add(new MysteryChoice("5", 15));
                towerChoices.Add(new MysteryChoice("6", 15));
                towerChoices.Add(new MysteryChoice("7", 15));
                MysteryCategory towerCategory = new MysteryCategory("toweropen", GetMinValue(towerChoices), GetMaxValue(towerChoices), towerChoices);

                //Ganon Vuln
                int ganonvulnindex = -1;
                List<MysteryChoice> vulnChoices = new List<MysteryChoice>();
                vulnChoices.Add(new MysteryChoice("3", 10));
                vulnChoices.Add(new MysteryChoice("4", 15));
                vulnChoices.Add(new MysteryChoice("5", 20));
                vulnChoices.Add(new MysteryChoice("6", 25));
                vulnChoices.Add(new MysteryChoice("7", 30));
                MysteryCategory vulnCategory = new MysteryCategory("ganonvuln", GetMinValue(vulnChoices), GetMaxValue(vulnChoices), vulnChoices);

                //World State
                int worldstateindex = -1;
                List<MysteryChoice> stateChoices = new List<MysteryChoice>();
                stateChoices.Add(new MysteryChoice("open", 30));
                stateChoices.Add(new MysteryChoice("standard", (isentrance ? 20 : 30)));
                stateChoices.Add(new MysteryChoice("retro", (isentrance ? 30 : 20)));
                stateChoices.Add(new MysteryChoice("inverted", 20));
                MysteryCategory stateCategory = new MysteryCategory("worldstate", GetMinValue(stateChoices), GetMaxValue(stateChoices), stateChoices);

                //Entrance Shuffle
                int entranceshuffleindex = -1;
                List<MysteryChoice> entranceChoices = new List<MysteryChoice>();
                entranceChoices.Add(new MysteryChoice("crossed", 80));
                entranceChoices.Add(new MysteryChoice("restricted", 20));
                MysteryCategory entranceCategory = new MysteryCategory("entranceshuffle", GetMinValue(entranceChoices), GetMaxValue(entranceChoices), entranceChoices);

                //Boss Shuffle
                int bossshuffleindex = -1;
                List<MysteryChoice> bossChoices = new List<MysteryChoice>();
                bossChoices.Add(new MysteryChoice("none", 60));
                bossChoices.Add(new MysteryChoice("full", 30));
                bossChoices.Add(new MysteryChoice("random", 10));
                MysteryCategory bossCategory = new MysteryCategory("bossshuffle", GetMinValue(bossChoices), GetMaxValue(bossChoices), bossChoices);

                //Enemy Shuffle
                int enemyshuffleindex = -1;
                List<MysteryChoice> enemyChoices = new List<MysteryChoice>();
                enemyChoices.Add(new MysteryChoice("none", 60));
                enemyChoices.Add(new MysteryChoice("shuffled", 30));
                enemyChoices.Add(new MysteryChoice("random", 10));
                MysteryCategory enemyCategory = new MysteryCategory("enemyshuffle", GetMinValue(enemyChoices), GetMaxValue(enemyChoices), enemyChoices);

                //Hints
                int hintsindex = -1;
                List<MysteryChoice> hintsChoices = new List<MysteryChoice>();
                hintsChoices.Add(new MysteryChoice("on", 50));
                hintsChoices.Add(new MysteryChoice("off", 50));
                MysteryCategory hintsCategory = new MysteryCategory("hints", GetMinValue(hintsChoices), GetMaxValue(hintsChoices), hintsChoices);

                //Swords
                int swordsindex = -1;
                List<MysteryChoice> swordChoices = new List<MysteryChoice>();
                swordChoices.Add(new MysteryChoice("randomized", 50));
                swordChoices.Add(new MysteryChoice("assured", 30));
                swordChoices.Add(new MysteryChoice("vanilla", 10));
                swordChoices.Add(new MysteryChoice("swordless", 10));
                MysteryCategory swordCategory = new MysteryCategory("swords", GetMinValue(swordChoices), GetMaxValue(swordChoices), swordChoices);

                //Item Pool
                int itempoolindex = -1;
                List<MysteryChoice> poolChoices = new List<MysteryChoice>();
                poolChoices.Add(new MysteryChoice("normal", 60));
                poolChoices.Add(new MysteryChoice("hard", 40));
                MysteryCategory poolCategory = new MysteryCategory("itempool", GetMinValue(poolChoices), GetMaxValue(poolChoices), poolChoices);

                //Entrance Items
                int entrancedungeonitemsindex = -1;
                List<MysteryChoice> entranceItemsChoices = new List<MysteryChoice>();
                entranceItemsChoices.Add(new MysteryChoice("standard", 20));
                entranceItemsChoices.Add(new MysteryChoice("full", 80));
                MysteryCategory entranceItemsCategory = new MysteryCategory("entranceshuffleitems", GetMinValue(entranceItemsChoices), GetMaxValue(entranceItemsChoices), entranceItemsChoices);

                //Restricted Shared
                //Accessibility 
                int accessibilityindex = -1;
                List<MysteryChoice> accessChoices = new List<MysteryChoice>();
                accessChoices.Add(new MysteryChoice("items", 90));
                accessChoices.Add(new MysteryChoice("none", 10));
                MysteryCategory accessCategory = new MysteryCategory("accessibility", GetMinValue(accessChoices), GetMaxValue(accessChoices), accessChoices);

                //Enemy Damage
                int enemydamageindex = -1;
                List<MysteryChoice> damageChoices = new List<MysteryChoice>();
                damageChoices.Add(new MysteryChoice("default", 100));
                MysteryCategory damageCategory = new MysteryCategory("enemydamage", GetMinValue(damageChoices), GetMaxValue(damageChoices), damageChoices);

                //Enemy Health
                int enemyhealthindex = -1;
                List<MysteryChoice> healthChoices = new List<MysteryChoice>();
                healthChoices.Add(new MysteryChoice("default", 100));
                MysteryCategory healthCategory = new MysteryCategory("enemyhealth", GetMinValue(healthChoices), GetMaxValue(healthChoices), healthChoices);

                //Wild Maps
                int mapsindex = -1;
                List<MysteryChoice> mapsChoices = new List<MysteryChoice>();
                mapsChoices.Add(new MysteryChoice("true", 70));
                mapsChoices.Add(new MysteryChoice("false", 30));
                MysteryCategory mapsCategory = new MysteryCategory("mapsshuffle", GetMinValue(mapsChoices), GetMaxValue(mapsChoices), mapsChoices);

                //Wild Compasses
                int compassesindex = -1;
                List<MysteryChoice> compassesChoices = new List<MysteryChoice>();
                compassesChoices.Add(new MysteryChoice("true", 70));
                compassesChoices.Add(new MysteryChoice("false", 30));
                MysteryCategory compassesCategory = new MysteryCategory("compassesshuffle", GetMinValue(compassesChoices), GetMaxValue(compassesChoices), compassesChoices);

                //Wild Small Keys
                int smallkeysindex = -1;
                List<MysteryChoice> smallkeysChoices = new List<MysteryChoice>();
                smallkeysChoices.Add(new MysteryChoice("true", 70));
                smallkeysChoices.Add(new MysteryChoice("false", 30));
                MysteryCategory smallkeysCategory = new MysteryCategory("smallkeysshuffle", GetMinValue(smallkeysChoices), GetMaxValue(smallkeysChoices), smallkeysChoices);

                //Wild Big Keys
                int bigkeysindex = -1;
                List<MysteryChoice> bigkeysChoices = new List<MysteryChoice>();
                bigkeysChoices.Add(new MysteryChoice("true", 70));
                bigkeysChoices.Add(new MysteryChoice("false", 30));
                MysteryCategory bigkeysCategory = new MysteryCategory("bigkeysshuffle", GetMinValue(bigkeysChoices), GetMaxValue(bigkeysChoices), bigkeysChoices);

                //Starting Boots
                int bootsindex = -1;
                List<MysteryChoice> bootsChoices = new List<MysteryChoice>();
                bootsChoices.Add(new MysteryChoice("PegasusBoots", 50));
                bootsChoices.Add(new MysteryChoice("", 50));
                MysteryCategory bootsCategory = new MysteryCategory("bootsshuffle", GetMinValue(bootsChoices), GetMaxValue(bootsChoices), bootsChoices);

                //Starting Hookshot
                int hookshotindex = -1;
                List<MysteryChoice> hookshotChoices = new List<MysteryChoice>();
                hookshotChoices.Add(new MysteryChoice("Hookshot", 20));
                hookshotChoices.Add(new MysteryChoice("", 80));
                MysteryCategory hookshotCategory = new MysteryCategory("hookshotshuffle", GetMinValue(hookshotChoices), GetMaxValue(hookshotChoices), hookshotChoices);

                //Starting Fire Rod
                int firerodindex = -1;
                List<MysteryChoice> firerodChoices = new List<MysteryChoice>();
                firerodChoices.Add(new MysteryChoice("FireRod", 20));
                firerodChoices.Add(new MysteryChoice("", 80));
                MysteryCategory firerodCategory = new MysteryCategory("firerodshuffle", GetMinValue(firerodChoices), GetMaxValue(firerodChoices), firerodChoices);

                //Starting Ice Rod
                int icerodindex = -1;
                List<MysteryChoice> icerodChoices = new List<MysteryChoice>();
                icerodChoices.Add(new MysteryChoice("IceRod", 20));
                icerodChoices.Add(new MysteryChoice("", 80));
                MysteryCategory icerodCategory = new MysteryCategory("icerodshuffle", GetMinValue(icerodChoices), GetMaxValue(icerodChoices), icerodChoices);

                //Starting Flippers
                int flippersindex = -1;
                List<MysteryChoice> flippersChoices = new List<MysteryChoice>();
                flippersChoices.Add(new MysteryChoice("Flippers", 20));
                flippersChoices.Add(new MysteryChoice("", 80));
                MysteryCategory flippersCategory = new MysteryCategory("flippersshuffle", GetMinValue(flippersChoices), GetMaxValue(flippersChoices), flippersChoices);

                //Starting Flute
                int fluteindex = -1;
                List<MysteryChoice> fluteChoices = new List<MysteryChoice>();
                fluteChoices.Add(new MysteryChoice("OcarinaInactive", 20));
                fluteChoices.Add(new MysteryChoice("", 80));
                MysteryCategory fluteCategory = new MysteryCategory("fluteshuffle", GetMinValue(fluteChoices), GetMaxValue(fluteChoices), fluteChoices);

                //Starting Mirror
                int mirrorindex = -1;
                List<MysteryChoice> mirrorChoices = new List<MysteryChoice>();
                mirrorChoices.Add(new MysteryChoice("MagicMirror", 20));
                mirrorChoices.Add(new MysteryChoice("", 80));
                MysteryCategory mirrorCategory = new MysteryCategory("mirrorshuffle", GetMinValue(mirrorChoices), GetMaxValue(mirrorChoices), mirrorChoices);

                List<MysteryCategory> categories = new List<MysteryCategory>();

                categories.Add(goalCategory);
                categories.Add(towerCategory);
                categories.Add(vulnCategory);
                categories.Add(stateCategory);
                categories.Add(bossCategory);
                categories.Add(enemyCategory);
                categories.Add(hintsCategory);
                categories.Add(swordCategory);
                categories.Add(poolCategory);
                categories.Add(accessCategory);
                categories.Add(damageCategory);
                categories.Add(healthCategory);
                categories.Add(bootsCategory);

                if (isentrance)
                {
                    categories.Add(entranceCategory);
                    categories.Add(entranceItemsCategory);
                }
                else
                {
                    categories.Add(mapsCategory);
                    categories.Add(compassesCategory);
                    categories.Add(smallkeysCategory);
                    categories.Add(bigkeysCategory);
                    categories.Add(hookshotCategory);
                    categories.Add(firerodCategory);
                    categories.Add(icerodCategory);
                    categories.Add(flippersCategory);
                    categories.Add(fluteCategory);
                    categories.Add(mirrorCategory);
                }

                foreach (MysteryCategory c in categories)
                {
                    int r = GlobalHelpers.GetRandomNumber(0, 100);

                    int i = 0;
                    int valuecount = c.Choices[i].ChoiceValue;

                    while (r > valuecount)
                    {
                        i++;
                        valuecount += c.Choices[i].ChoiceValue;
                    }

                    switch (c.CategoryName)
                    {
                        case "goal":
                            goalindex = i;
                            break;
                        case "toweropen":
                            toweropenindex = i;
                            break;
                        case "ganonvuln":
                            ganonvulnindex = i;
                            break;
                        case "worldstate":
                            worldstateindex = i;
                            break;
                        case "hints":
                            hintsindex = i;
                            break;
                        case "swords":
                            swordsindex = i;
                            break;
                        case "itempool":
                            itempoolindex = i;
                            break;
                        case "accessibility":
                            accessibilityindex = i;
                            break;
                        case "bossshuffle":
                            bossshuffleindex = i;
                            break;
                        case "enemyshuffle":
                            enemyshuffleindex = i;
                            break;
                        case "enemydamage":
                            enemydamageindex = i;
                            break;
                        case "enemyhealth":
                            enemyhealthindex = i;
                            break;
                        case "entranceshuffle":
                            entranceshuffleindex = i;
                            break;
                        case "entranceshuffleitems":
                            entrancedungeonitemsindex = i;
                            break;
                        case "mapsshuffle":
                            mapsindex = i;
                            break;
                        case "compassesshuffle":
                            compassesindex = i;
                            break;
                        case "smallkeysshuffle":
                            smallkeysindex = i;
                            break;
                        case "bigkeysshuffle":
                            bigkeysindex = i;
                            break;
                        case "bootsshuffle":
                            bootsindex = i;
                            break;
                        case "hookshotshuffle":
                            hookshotindex = i;
                            break;
                        case "firerodshuffle":
                            firerodindex = i;
                            break;
                        case "icerodshuffle":
                            icerodindex = i;
                            break;
                        case "flippersshuffle":
                            flippersindex = i;
                            break;
                        case "fluteshuffle":
                            fluteindex = i;
                            break;
                        case "mirrorshuffle":
                            mirrorindex = i;
                            break;
                    }
                }

                int replacementcount = 0;
                if (hookshotindex == 0) replacementcount++;
                if (firerodindex == 0) replacementcount++;
                if (icerodindex == 0) replacementcount++;
                if (flippersindex == 0) replacementcount++;
                if (fluteindex == 0) replacementcount++;
                if (mirrorindex == 0) replacementcount++;

                bool overridevuln = false;
                //Adjust for vuln and enemizer
                if (goalindex == 0 && int.Parse(vulnCategory.Choices[ganonvulnindex].ChoiceName) < int.Parse(towerCategory.Choices[toweropenindex].ChoiceName))
                {
                    overridevuln = true;
                }

                if (swordsindex != 1 && worldstateindex == 1 && enemyshuffleindex != 0)
                {
                    //Change starting swords to assured
                    swordsindex = 1;
                }

                //If completionist goal and inverted, reroll the world state
                //if (goalindex == 5 && worldstateindex >= 2)
                //{
                //    worldstateindex = GlobalHelpers.GetRandomNumber(0, 2);
                //}

                //If triforce hunt, set those values
                bool isTriforce = (goalindex == 4 ? true : false);
                int triforceRequired = 0;

                if (isTriforce)
                {
                    triforceRequired = GlobalHelpers.GetRandomNumber(10, 30) + 20;
                }

                if (isentrance)
                {
                    MysteryEntranceObject meobj = new MysteryEntranceObject();

                    meobj.accessibility = accessCategory.Choices[accessibilityindex].ChoiceName;
                    meobj.allow_quickswap = true;
                    meobj.crystals = new MysteryCrystals();
                    meobj.crystals.ganon = int.Parse(overridevuln ? towerCategory.Choices[toweropenindex].ChoiceName : vulnCategory.Choices[ganonvulnindex].ChoiceName);
                    meobj.crystals.tower = int.Parse(towerCategory.Choices[toweropenindex].ChoiceName);
                    meobj.dungeon_items = entranceItemsCategory.Choices[entrancedungeonitemsindex].ChoiceName;
                    meobj.enemizer = new MysteryEnemizer();
                    meobj.enemizer.boss_shuffle = bossCategory.Choices[bossshuffleindex].ChoiceName;
                    meobj.enemizer.enemy_damage = damageCategory.Choices[enemydamageindex].ChoiceName;
                    meobj.enemizer.enemy_health = healthCategory.Choices[enemyhealthindex].ChoiceName;
                    meobj.enemizer.enemy_shuffle = enemyCategory.Choices[enemyshuffleindex].ChoiceName;
                    meobj.enemizer.pot_shuffle = "off";
                    meobj.entrances = entranceCategory.Choices[entranceshuffleindex].ChoiceName;
                    meobj.glitches = "none";
                    meobj.goal = goalCategory.Choices[goalindex].ChoiceName;
                    meobj.hints = hintsCategory.Choices[hintsindex].ChoiceName;
                    meobj.item = new MysteryItem();
                    meobj.item.functionality = "normal";
                    meobj.item.pool = poolCategory.Choices[itempoolindex].ChoiceName;
                    meobj.item_placement = "advanced";
                    meobj.lang = "en";
                    meobj.mode = stateCategory.Choices[worldstateindex].ChoiceName;
                    meobj.pseudoboots = (bootsindex == 1 ? false : true);
                    meobj.spoilers = "mystery";
                    meobj.tournament = true;
                    meobj.weapons = swordCategory.Choices[swordsindex].ChoiceName;

                    json = JsonConvert.SerializeObject(meobj);
                }
                else
                {
                    MysteryNonEntranceObject mneobj = new MysteryNonEntranceObject();

                    mneobj.accessibility = accessCategory.Choices[accessibilityindex].ChoiceName;
                    mneobj.allow_quickswap = true;
                    mneobj.crystals = new MysteryCrystals();
                    mneobj.crystals.ganon = int.Parse(overridevuln ? towerCategory.Choices[toweropenindex].ChoiceName : vulnCategory.Choices[ganonvulnindex].ChoiceName);
                    mneobj.crystals.tower = int.Parse(towerCategory.Choices[toweropenindex].ChoiceName);
                    mneobj.custom = new MysteryCustom();
                    mneobj.custom.canBombJump = false;
                    mneobj.custom.canBootsClip = false;
                    mneobj.custom.canBunnyRevive = false;
                    mneobj.custom.canBunnySurf = false;
                    mneobj.custom.canDungeonRevive = false;
                    mneobj.custom.canFakeFlipper = false;
                    mneobj.custom.canMirrorClip = false;
                    mneobj.custom.canMirrorWrap = false;
                    mneobj.custom.canOWYBA = false;
                    mneobj.custom.canOneFrameClipOW = false;
                    mneobj.custom.canOneFrameClipUW = false;
                    mneobj.custom.canSuperBunny = false;
                    mneobj.custom.canSuperSpeed = false;
                    mneobj.custom.canWaterFairyRevive = false;
                    mneobj.custom.canWaterWalk = false;
                    mneobj.custom.customPrizePacks = false;
                    mneobj.custom.drop = new MysteryDrop();
                    mneobj.custom.drop.count = new MysteryCount();
                    mneobj.custom.drop.count.ArrowRefill10 = 3;
                    mneobj.custom.drop.count.ArrowRefill5 = 5;
                    mneobj.custom.drop.count.Bee = 0;
                    mneobj.custom.drop.count.BeeGood = 0;
                    mneobj.custom.drop.count.BombRefill1 = 7;
                    mneobj.custom.drop.count.BombRefill4 = 1;
                    mneobj.custom.drop.count.BombRefill8 = 2;
                    mneobj.custom.drop.count.Fairy = 1;
                    mneobj.custom.drop.count.Heart = 13;
                    mneobj.custom.drop.count.MagicRefillFull = 3;
                    mneobj.custom.drop.count.MagicRefillSmall = 6;
                    mneobj.custom.drop.count.RupeeBlue = 7;
                    mneobj.custom.drop.count.RupeeGreen = 9;
                    mneobj.custom.drop.count.RupeeRed = 6;
                    mneobj.custom.item = new MysteryComplexItem();
                    mneobj.custom.item.count = new MysteryComplexCount();
                    mneobj.custom.item.count.Arrow = 1;
                    mneobj.custom.item.count.ArrowUpgrade10 = 0;
                    mneobj.custom.item.count.ArrowUpgrade5 = 0;
                    mneobj.custom.item.count.BigKeyA1 = 0;
                    mneobj.custom.item.count.BigKeyA2 = 1;
                    mneobj.custom.item.count.BigKeyD1 = 1;
                    mneobj.custom.item.count.BigKeyD2 = 1;
                    mneobj.custom.item.count.BigKeyD3 = 1;
                    mneobj.custom.item.count.BigKeyD4 = 1;
                    mneobj.custom.item.count.BigKeyD5 = 1;
                    mneobj.custom.item.count.BigKeyD6 = 1;
                    mneobj.custom.item.count.BigKeyD7 = 1;
                    mneobj.custom.item.count.BigKeyH1 = 0;
                    mneobj.custom.item.count.BigKeyH2 = 0;
                    mneobj.custom.item.count.BigKeyP1 = 1;
                    mneobj.custom.item.count.BigKeyP2 = 1;
                    mneobj.custom.item.count.BigKeyP3 = 1;
                    mneobj.custom.item.count.BlueClock = 0;
                    mneobj.custom.item.count.BlueMail = 0;
                    mneobj.custom.item.count.BlueShield = 0;
                    mneobj.custom.item.count.Bomb = 0;
                    mneobj.custom.item.count.BombUpgrade10 = 0;
                    mneobj.custom.item.count.Bombos = 1;
                    mneobj.custom.item.count.BombUpgrade5 = 0;
                    mneobj.custom.item.count.BookOfMudora = 1;
                    mneobj.custom.item.count.Boomerang = 1;
                    mneobj.custom.item.count.BossHeartContainer = 10;
                    mneobj.custom.item.count.Bottle = 0;
                    mneobj.custom.item.count.BottleWithBee = 0;
                    mneobj.custom.item.count.BottleWithBluePotion = 0;
                    mneobj.custom.item.count.BottleWithFairy = 0;
                    mneobj.custom.item.count.BottleWithGoldBee = 0;
                    mneobj.custom.item.count.BottleWithGreenPotion = 0;
                    mneobj.custom.item.count.BottleWithRandom = 4;
                    mneobj.custom.item.count.BottleWithRedPotion = 0;
                    mneobj.custom.item.count.Bow = 0;
                    mneobj.custom.item.count.BowAndArrows = 0;
                    mneobj.custom.item.count.BowAndSilverArrows = 0;
                    mneobj.custom.item.count.BugCatchingNet = 1;
                    mneobj.custom.item.count.CaneOfByrna = 1;
                    mneobj.custom.item.count.CaneOfSomaria = 1;
                    mneobj.custom.item.count.Cape = 1;
                    mneobj.custom.item.count.CompassA1 = 0;
                    mneobj.custom.item.count.CompassA2 = 1;
                    mneobj.custom.item.count.CompassD1 = 1;
                    mneobj.custom.item.count.CompassD2 = 1;
                    mneobj.custom.item.count.CompassD3 = 1;
                    mneobj.custom.item.count.CompassD4 = 1;
                    mneobj.custom.item.count.CompassD5 = 1;
                    mneobj.custom.item.count.CompassD6 = 1;
                    mneobj.custom.item.count.CompassD7 = 1;
                    mneobj.custom.item.count.CompassH1 = 0;
                    mneobj.custom.item.count.CompassH2 = 0;
                    mneobj.custom.item.count.CompassP1 = 1;
                    mneobj.custom.item.count.CompassP2 = 1;
                    mneobj.custom.item.count.CompassP3 = 1;
                    mneobj.custom.item.count.Ether = 1;
                    mneobj.custom.item.count.FiftyRupees = 7;
                    mneobj.custom.item.count.FireRod = firerodindex;
                    mneobj.custom.item.count.FiveRupees = 4;
                    mneobj.custom.item.count.Flippers = flippersindex;
                    mneobj.custom.item.count.GreenClock = 0;
                    mneobj.custom.item.count.HalfMagic = 1;
                    mneobj.custom.item.count.Hammer = 1;
                    mneobj.custom.item.count.Heart = 0;
                    mneobj.custom.item.count.HeartContainer = 1;
                    mneobj.custom.item.count.Hookshot = hookshotindex;
                    mneobj.custom.item.count.IceRod = icerodindex;
                    mneobj.custom.item.count.KeyA1 = 2;
                    mneobj.custom.item.count.KeyA2 = 4;
                    mneobj.custom.item.count.KeyD1 = 6;
                    mneobj.custom.item.count.KeyD2 = 1;
                    mneobj.custom.item.count.KeyD3 = 3;
                    mneobj.custom.item.count.KeyD4 = 1;
                    mneobj.custom.item.count.KeyD5 = 2;
                    mneobj.custom.item.count.KeyD6 = 3;
                    mneobj.custom.item.count.KeyD7 = 4;
                    mneobj.custom.item.count.KeyH1 = 0;
                    mneobj.custom.item.count.KeyH2 = 1;
                    mneobj.custom.item.count.KeyP1 = 0;
                    mneobj.custom.item.count.KeyP2 = 1;
                    mneobj.custom.item.count.KeyP3 = 1;
                    mneobj.custom.item.count.L1Sword = 0;
                    mneobj.custom.item.count.L1SwordAndShield = 0;
                    mneobj.custom.item.count.L3Sword = 0;
                    mneobj.custom.item.count.L4Sword = 0;
                    mneobj.custom.item.count.Lamp = 1;
                    mneobj.custom.item.count.MagicMirror = mirrorindex;
                    mneobj.custom.item.count.MapA1 = 0;
                    mneobj.custom.item.count.MapA2 = 1;
                    mneobj.custom.item.count.MapD1 = 1;
                    mneobj.custom.item.count.MapD2 = 1;
                    mneobj.custom.item.count.MapD3 = 1;
                    mneobj.custom.item.count.MapD4 = 1;
                    mneobj.custom.item.count.MapD5 = 1;
                    mneobj.custom.item.count.MapD6 = 1;
                    mneobj.custom.item.count.MapD7 = 1;
                    mneobj.custom.item.count.MapH1 = 0;
                    mneobj.custom.item.count.MapH2 = 1;
                    mneobj.custom.item.count.MapP1 = 1;
                    mneobj.custom.item.count.MapP2 = 1;
                    mneobj.custom.item.count.MapP3 = 1;
                    mneobj.custom.item.count.MasterSword = 0;
                    mneobj.custom.item.count.MirrorShield = 0;
                    mneobj.custom.item.count.MoonPearl = 1;
                    mneobj.custom.item.count.Mushroom = 1;
                    mneobj.custom.item.count.Nothing = 0;
                    mneobj.custom.item.count.OcarinaActive = 0;
                    mneobj.custom.item.count.OcarinaInactive = fluteindex;
                    mneobj.custom.item.count.OneHundredRupees = 1;
                    mneobj.custom.item.count.OneRupee = 2;
                    mneobj.custom.item.count.PegasusBoots = bootsindex;
                    mneobj.custom.item.count.PieceOfHeart = 24;
                    mneobj.custom.item.count.Powder = 1;
                    mneobj.custom.item.count.PowerGlove = 0;
                    mneobj.custom.item.count.ProgressiveArmor = 2;
                    mneobj.custom.item.count.ProgressiveBow = 2;
                    mneobj.custom.item.count.ProgressiveGlove = 2;
                    mneobj.custom.item.count.ProgressiveShield = 3;
                    mneobj.custom.item.count.ProgressiveSword = 4;
                    mneobj.custom.item.count.Quake = 1;
                    mneobj.custom.item.count.QuarterMagic = 0;
                    mneobj.custom.item.count.RedBoomerang = 1;
                    mneobj.custom.item.count.RedClock = 0;
                    mneobj.custom.item.count.RedMail = 0;
                    mneobj.custom.item.count.RedShield = 0;
                    mneobj.custom.item.count.Rupoor = 0;
                    mneobj.custom.item.count.Shovel = 1;
                    mneobj.custom.item.count.SilverArrowUpgrade = 0;
                    mneobj.custom.item.count.SmallMagic = 0;
                    mneobj.custom.item.count.TenArrows = 12;
                    mneobj.custom.item.count.TenBombs = 1;
                    mneobj.custom.item.count.ThreeBombs = 16;
                    mneobj.custom.item.count.ThreeHundredRupees = 5;
                    mneobj.custom.item.count.TitansMitt = 0;
                    mneobj.custom.item.count.Triforce = 0;
                    mneobj.custom.item.count.TriforcePiece = (goalindex == 4 ? triforceRequired : 0);
                    mneobj.custom.item.count.TwentyRupees = 28;
                    mneobj.custom.item.count.TwentyRupees2 = replacementcount;
                    mneobj.custom.itemGoalRequired = (goalindex == 4 ? (triforceRequired - 10).ToString() : "");
                    mneobj.custom.itemrequireLamp = false;
                    mneobj.custom.itemvalueBlueClock = "";
                    mneobj.custom.itemvalueGreenClock = "";
                    mneobj.custom.itemvalueRedClock = "";
                    mneobj.custom.itemvalueRupoor = "";
                    mneobj.custom.prizecrossWorld = true;
                    mneobj.custom.prizeshuffleCrystals = true;
                    mneobj.custom.prizeshufflePendants = true;
                    mneobj.custom.regionbossNormalLocation = true;
                    mneobj.custom.regionwildBigKeys = (bigkeysindex == 0 ? true : false);
                    mneobj.custom.regionwildCompasses = (compassesindex == 0 ? true : false);
                    mneobj.custom.regionwildKeys = (smallkeysindex == 0 ? true : false);
                    mneobj.custom.regionwildMaps = (mapsindex == 0 ? true : false);
                    mneobj.custom.romdungeonCount = (compassesindex == 0 ? "pickup" : "off");
                    mneobj.custom.romfreeItemMenu = true;
                    mneobj.custom.romfreeItemText = true;
                    mneobj.custom.romgenericKeys = false;
                    mneobj.custom.rommapOnPickup = (mapsindex == 0 ? true : false);
                    mneobj.custom.romrupeeBow = false;
                    mneobj.custom.romtimerMode = "off";
                    mneobj.custom.romtimerStart = "";
                    mneobj.custom.spoilBootsLocation = false;
                    mneobj.drops = new MysteryDrops();
                    mneobj.drops._0 = new List<string>();
                    mneobj.drops._1 = new List<string>();
                    mneobj.drops._2 = new List<string>();
                    mneobj.drops._3 = new List<string>();
                    mneobj.drops._4 = new List<string>();
                    mneobj.drops._5 = new List<string>();
                    mneobj.drops._6 = new List<string>();

                    for (int i = 0; i < 8; i++)
                    {
                        mneobj.drops._0.Add("auto_fill");
                        mneobj.drops._1.Add("auto_fill");
                        mneobj.drops._2.Add("auto_fill");
                        mneobj.drops._3.Add("auto_fill");
                        mneobj.drops._4.Add("auto_fill");
                        mneobj.drops._5.Add("auto_fill");
                        mneobj.drops._6.Add("auto_fill");
                    }

                    mneobj.drops.crab = new List<string>();
                    mneobj.drops.crab.Add("auto_fill");
                    mneobj.drops.crab.Add("auto_fill");
                    mneobj.drops.fish = new List<string>();
                    mneobj.drops.fish.Add("auto_fill");
                    mneobj.drops.pull = new List<string>();
                    mneobj.drops.pull.Add("auto_fill");
                    mneobj.drops.pull.Add("auto_fill");
                    mneobj.drops.pull.Add("auto_fill");
                    mneobj.drops.stun = new List<string>();
                    mneobj.drops.stun.Add("auto_fill");
                    mneobj.dungeon_items = "standard";
                    mneobj.enemizer = new MysteryEnemizer();
                    mneobj.enemizer.boss_shuffle = bossCategory.Choices[bossshuffleindex].ChoiceName;
                    mneobj.enemizer.enemy_damage = damageCategory.Choices[enemydamageindex].ChoiceName;
                    mneobj.enemizer.enemy_health = healthCategory.Choices[enemyhealthindex].ChoiceName;
                    mneobj.enemizer.enemy_shuffle = enemyCategory.Choices[enemyshuffleindex].ChoiceName;
                    mneobj.enemizer.pot_shuffle = "off";
                    mneobj.entrances = "none";
                    mneobj.eq = new List<string>();
                    if (hookshotindex == 0) { mneobj.eq.Add("Hookshot"); }
                    if (firerodindex == 0) { mneobj.eq.Add("FireRod"); }
                    if (icerodindex == 0) { mneobj.eq.Add("IceRod"); }
                    if (mirrorindex == 0) { mneobj.eq.Add("MagicMirror"); }
                    if (bootsindex == 0) { mneobj.eq.Add("PegasusBoots"); }
                    if (flippersindex == 0) { mneobj.eq.Add("Flippers"); }
                    if (fluteindex == 0)
                    {
                        if (worldstateindex == 1)
                        {
                            mneobj.eq.Add("OcarinaInactive");
                        }
                        else
                        {
                            mneobj.eq.Add("OcarinaActive");
                        }
                    }
                    mneobj.eq.Add("BossHeartContainer");
                    mneobj.eq.Add("BossHeartContainer");
                    mneobj.eq.Add("BossHeartContainer");
                    mneobj.glitches = "none";
                    mneobj.goal = goalCategory.Choices[goalindex].ChoiceName;
                    mneobj.hints = hintsCategory.Choices[hintsindex].ChoiceName;
                    mneobj.item = new MysteryItem();
                    mneobj.item.functionality = "normal";
                    mneobj.item.pool = poolCategory.Choices[itempoolindex].ChoiceName;
                    mneobj.item_placement = "advanced";
                    mneobj.l = new L();
                    mneobj.lang = "en";
                    mneobj.mode = stateCategory.Choices[worldstateindex].ChoiceName;
                    mneobj.name = "";
                    mneobj.notes = "";
                    mneobj.pseudoboots = false;
                    mneobj.spoilers = "mystery";
                    mneobj.tournament = true;
                    mneobj.weapons = swordCategory.Choices[swordsindex].ChoiceName;

                    json = JsonConvert.SerializeObject(mneobj);
                }

                //if (!isentrance)
                //{
                //    StreamWriter sw = new StreamWriter(@"c:\temp\mysterytext.txt", true);

                //    sw.Write(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\t{18}\t{19}\t{20}\t{21}\t{22}", goalCategory.Choices[goalindex].ChoiceName, towerCategory.Choices[toweropenindex].ChoiceName, vulnCategory.Choices[ganonvulnindex].ChoiceName, stateCategory.Choices[worldstateindex].ChoiceName, hintsCategory.Choices[hintsindex].ChoiceName, swordCategory.Choices[swordsindex].ChoiceName, poolCategory.Choices[itempoolindex].ChoiceName, accessCategory.Choices[accessibilityindex].ChoiceName, bossCategory.Choices[bossshuffleindex].ChoiceName, enemyCategory.Choices[enemyshuffleindex].ChoiceName, damageCategory.Choices[enemydamageindex].ChoiceName, healthCategory.Choices[enemyhealthindex].ChoiceName, mapsCategory.Choices[mapsindex].ChoiceName, compassesCategory.Choices[compassesindex].ChoiceName, smallkeysCategory.Choices[smallkeysindex].ChoiceName, bigkeysCategory.Choices[bigkeysindex].ChoiceName, bootsCategory.Choices[bootsindex].ChoiceName, hookshotCategory.Choices[hookshotindex].ChoiceName, firerodCategory.Choices[firerodindex].ChoiceName, icerodCategory.Choices[icerodindex].ChoiceName, flippersCategory.Choices[flippersindex].ChoiceName, fluteCategory.Choices[fluteindex].ChoiceName, mirrorCategory.Choices[mirrorindex].ChoiceName));
                //    sw.Write(Environment.NewLine);

                //    sw.Close();

                //    System.Diagnostics.Debug.WriteLine("");
                //    System.Diagnostics.Debug.WriteLine(string.Format("Goal\t{0}\t{1}", goalCategory.Choices[goalindex].ChoiceName, goalCategory.Choices[goalindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Tower\t{0}\t{1}", towerCategory.Choices[toweropenindex].ChoiceName, towerCategory.Choices[toweropenindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Vuln\t{0}\t{1}", vulnCategory.Choices[ganonvulnindex].ChoiceName, vulnCategory.Choices[ganonvulnindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("World State\t{0}\t{1}", stateCategory.Choices[worldstateindex].ChoiceName, stateCategory.Choices[worldstateindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Hints\t{0}\t{1}", hintsCategory.Choices[hintsindex].ChoiceName, hintsCategory.Choices[hintsindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Swords\t{0}\t{1}", swordCategory.Choices[swordsindex].ChoiceName, swordCategory.Choices[swordsindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Item Pool\t{0}\t{1}", poolCategory.Choices[itempoolindex].ChoiceName, poolCategory.Choices[itempoolindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Accessibility\t{0}\t{1}", accessCategory.Choices[accessibilityindex].ChoiceName, accessCategory.Choices[accessibilityindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Boss Shuffle\t{0}\t{1}", bossCategory.Choices[bossshuffleindex].ChoiceName, bossCategory.Choices[bossshuffleindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Enemy Shuffle\t{0}\t{1}", enemyCategory.Choices[enemyshuffleindex].ChoiceName, enemyCategory.Choices[enemyshuffleindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Damage Shuffle\t{0}\t{1}", damageCategory.Choices[enemydamageindex].ChoiceName, damageCategory.Choices[enemydamageindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Enemy Health\t{0}\t{1}", healthCategory.Choices[enemyhealthindex].ChoiceName, healthCategory.Choices[enemyhealthindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Wild Maps\t{0}\t{1}", mapsCategory.Choices[mapsindex].ChoiceName, mapsCategory.Choices[mapsindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Wild Compasses\t{0}\t{1}", compassesCategory.Choices[compassesindex].ChoiceName, compassesCategory.Choices[compassesindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Wild SMall Keys\t{0}\t{1}", smallkeysCategory.Choices[smallkeysindex].ChoiceName, smallkeysCategory.Choices[smallkeysindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Wild Big Keys\t{0}\t{1}", bigkeysCategory.Choices[bigkeysindex].ChoiceName, bigkeysCategory.Choices[bigkeysindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Starting Boots\t{0}\t{1}", bootsCategory.Choices[bootsindex].ChoiceName, bootsCategory.Choices[bootsindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Starting Hookshot\t{0}\t{1}", hookshotCategory.Choices[hookshotindex].ChoiceName, hookshotCategory.Choices[hookshotindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Starting Fire Rod\t{0}\t{1}", firerodCategory.Choices[firerodindex].ChoiceName, firerodCategory.Choices[firerodindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Starting Ice Rod\t{0}\t{1}", icerodCategory.Choices[icerodindex].ChoiceName, icerodCategory.Choices[icerodindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Starting Flippers\t{0}\t{1}", flippersCategory.Choices[flippersindex].ChoiceName, flippersCategory.Choices[flippersindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Starting Flute\t{0}\t{1}", fluteCategory.Choices[fluteindex].ChoiceName, fluteCategory.Choices[fluteindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Starting Mirror\t{0}\t{1}", mirrorCategory.Choices[mirrorindex].ChoiceName, mirrorCategory.Choices[mirrorindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine("");
                //}
                //else
                //{
                //    StreamWriter sw = new StreamWriter(@"c:\temp\mysterytext.txt", true);

                //    sw.Write(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}", entranceCategory.Choices[entranceshuffleindex].ChoiceName, goalCategory.Choices[goalindex].ChoiceName, towerCategory.Choices[toweropenindex].ChoiceName, vulnCategory.Choices[ganonvulnindex].ChoiceName, stateCategory.Choices[worldstateindex].ChoiceName, hintsCategory.Choices[hintsindex].ChoiceName, swordCategory.Choices[swordsindex].ChoiceName, poolCategory.Choices[itempoolindex].ChoiceName, accessCategory.Choices[accessibilityindex].ChoiceName, bossCategory.Choices[bossshuffleindex].ChoiceName, enemyCategory.Choices[enemyshuffleindex].ChoiceName, damageCategory.Choices[enemydamageindex].ChoiceName, healthCategory.Choices[enemyhealthindex].ChoiceName, entranceItemsCategory.Choices[entrancedungeonitemsindex].ChoiceName));
                //    sw.Write(Environment.NewLine);

                //    sw.Close();

                //    System.Diagnostics.Debug.WriteLine("");
                //    System.Diagnostics.Debug.WriteLine(string.Format("Entrance Type\t{0}\t{1}", entranceCategory.Choices[entranceshuffleindex].ChoiceName, entranceCategory.Choices[entranceshuffleindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Goal\t{0}\t{1}", goalCategory.Choices[goalindex].ChoiceName, goalCategory.Choices[goalindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Tower\t{0}\t{1}", towerCategory.Choices[toweropenindex].ChoiceName, towerCategory.Choices[toweropenindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Vuln\t{0}\t{1}", vulnCategory.Choices[ganonvulnindex].ChoiceName, vulnCategory.Choices[ganonvulnindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("World State\t{0}\t{1}", stateCategory.Choices[worldstateindex].ChoiceName, stateCategory.Choices[worldstateindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Hints\t{0}\t{1}", hintsCategory.Choices[hintsindex].ChoiceName, hintsCategory.Choices[hintsindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Swords\t{0}\t{1}", swordCategory.Choices[swordsindex].ChoiceName, swordCategory.Choices[swordsindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Item Pool\t{0}\t{1}", poolCategory.Choices[itempoolindex].ChoiceName, poolCategory.Choices[itempoolindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Accessibility\t{0}\t{1}", accessCategory.Choices[accessibilityindex].ChoiceName, accessCategory.Choices[accessibilityindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Boss Shuffle\t{0}\t{1}", bossCategory.Choices[bossshuffleindex].ChoiceName, bossCategory.Choices[bossshuffleindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Enemy Shuffle\t{0}\t{1}", enemyCategory.Choices[enemyshuffleindex].ChoiceName, enemyCategory.Choices[enemyshuffleindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Damage Shuffle\t{0}\t{1}", damageCategory.Choices[enemydamageindex].ChoiceName, damageCategory.Choices[enemydamageindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Enemy Health\t{0}\t{1}", healthCategory.Choices[enemyhealthindex].ChoiceName, healthCategory.Choices[enemyhealthindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine(string.Format("Dungeon Items\t{0}\t{1}", entranceItemsCategory.Choices[entrancedungeonitemsindex].ChoiceName, entranceItemsCategory.Choices[entrancedungeonitemsindex].ChoiceValue));
                //    System.Diagnostics.Debug.WriteLine("");
                //}
            }
            catch (WebException ex)
            {
                tb_logs log = new tb_logs();

                log.LogMessage = ex.Message;
                log.DateTimeLogged = DateTime.Now;

                dbContext.tb_logs.Add(log);

                dbContext.SaveChanges();
            }

            return json;
        }

        private string GenerateChampionsMysterySeed()
        {
            string json = "";

            try
            {
                //Goal
                int goalindex = -1;

                List<MysteryChoice> goalChoices = new List<MysteryChoice>();
                goalChoices.Add(new MysteryChoice("ganon", 15));
                goalChoices.Add(new MysteryChoice("fast_ganon", 50));
                goalChoices.Add(new MysteryChoice("pedestal", 35));
                MysteryCategory goalCategory = new MysteryCategory("goal", GetMinValue(goalChoices), GetMaxValue(goalChoices), goalChoices);

                //Tower Open
                int toweropenindex = -1;

                List<MysteryChoice> towerChoices = new List<MysteryChoice>();
                towerChoices.Add(new MysteryChoice("0", 10));
                towerChoices.Add(new MysteryChoice("1", 10));
                towerChoices.Add(new MysteryChoice("2", 20));
                towerChoices.Add(new MysteryChoice("3", 40));
                towerChoices.Add(new MysteryChoice("4", 20));
                MysteryCategory towerCategory = new MysteryCategory("toweropen", GetMinValue(towerChoices), GetMaxValue(towerChoices), towerChoices);

                //Ganon Vuln
                int ganonvulnindex = -1;
                List<MysteryChoice> vulnChoices = new List<MysteryChoice>();
                vulnChoices.Add(new MysteryChoice("1", 10));
                vulnChoices.Add(new MysteryChoice("2", 20));
                vulnChoices.Add(new MysteryChoice("3", 40));
                vulnChoices.Add(new MysteryChoice("4", 20));
                vulnChoices.Add(new MysteryChoice("5", 10));
                MysteryCategory vulnCategory = new MysteryCategory("ganonvuln", GetMinValue(vulnChoices), GetMaxValue(vulnChoices), vulnChoices);

                //World State
                int worldstateindex = -1;
                List<MysteryChoice> stateChoices = new List<MysteryChoice>();
                stateChoices.Add(new MysteryChoice("open", 80));
                stateChoices.Add(new MysteryChoice("inverted", 20));
                MysteryCategory stateCategory = new MysteryCategory("worldstate", GetMinValue(stateChoices), GetMaxValue(stateChoices), stateChoices);

                //Entrance Shuffle
                int entranceshuffleindex = -1;
                List<MysteryChoice> entranceChoices = new List<MysteryChoice>();
                entranceChoices.Add(new MysteryChoice("crossed", 90));
                entranceChoices.Add(new MysteryChoice("restricted", 10));
                MysteryCategory entranceCategory = new MysteryCategory("entranceshuffle", GetMinValue(entranceChoices), GetMaxValue(entranceChoices), entranceChoices);

                //Boss Shuffle
                int bossshuffleindex = -1;
                List<MysteryChoice> bossChoices = new List<MysteryChoice>();
                bossChoices.Add(new MysteryChoice("none", 40));
                bossChoices.Add(new MysteryChoice("full", 30));
                bossChoices.Add(new MysteryChoice("random", 30));
                MysteryCategory bossCategory = new MysteryCategory("bossshuffle", GetMinValue(bossChoices), GetMaxValue(bossChoices), bossChoices);

                //Enemy Shuffle
                int enemyshuffleindex = -1;
                List<MysteryChoice> enemyChoices = new List<MysteryChoice>();
                enemyChoices.Add(new MysteryChoice("none", 40));
                enemyChoices.Add(new MysteryChoice("shuffled", 60));
                MysteryCategory enemyCategory = new MysteryCategory("enemyshuffle", GetMinValue(enemyChoices), GetMaxValue(enemyChoices), enemyChoices);

                //Hints
                int hintsindex = -1;
                List<MysteryChoice> hintsChoices = new List<MysteryChoice>();
                hintsChoices.Add(new MysteryChoice("on", 50));
                hintsChoices.Add(new MysteryChoice("off", 50));
                MysteryCategory hintsCategory = new MysteryCategory("hints", GetMinValue(hintsChoices), GetMaxValue(hintsChoices), hintsChoices);

                //Swords
                int swordsindex = -1;
                List<MysteryChoice> swordChoices = new List<MysteryChoice>();
                swordChoices.Add(new MysteryChoice("randomized", 50));
                swordChoices.Add(new MysteryChoice("assured", 30));
                swordChoices.Add(new MysteryChoice("swordless", 20));
                MysteryCategory swordCategory = new MysteryCategory("swords", GetMinValue(swordChoices), GetMaxValue(swordChoices), swordChoices);

                //Item Pool
                int itempoolindex = -1;
                List<MysteryChoice> poolChoices = new List<MysteryChoice>();
                poolChoices.Add(new MysteryChoice("normal", 70));
                poolChoices.Add(new MysteryChoice("hard", 30));
                MysteryCategory poolCategory = new MysteryCategory("itempool", GetMinValue(poolChoices), GetMaxValue(poolChoices), poolChoices);

                //Entrance Items
                int entrancedungeonitemsindex = -1;
                List<MysteryChoice> entranceItemsChoices = new List<MysteryChoice>();
                entranceItemsChoices.Add(new MysteryChoice("full", 100));
                MysteryCategory entranceItemsCategory = new MysteryCategory("entranceshuffleitems", GetMinValue(entranceItemsChoices), GetMaxValue(entranceItemsChoices), entranceItemsChoices);

                //Restricted Shared
                //Accessibility 
                int accessibilityindex = -1;
                List<MysteryChoice> accessChoices = new List<MysteryChoice>();
                accessChoices.Add(new MysteryChoice("none", 100));
                MysteryCategory accessCategory = new MysteryCategory("accessibility", GetMinValue(accessChoices), GetMaxValue(accessChoices), accessChoices);

                //Enemy Damage
                int enemydamageindex = -1;
                List<MysteryChoice> damageChoices = new List<MysteryChoice>();
                damageChoices.Add(new MysteryChoice("default", 100));
                MysteryCategory damageCategory = new MysteryCategory("enemydamage", GetMinValue(damageChoices), GetMaxValue(damageChoices), damageChoices);

                //Enemy Health
                int enemyhealthindex = -1;
                List<MysteryChoice> healthChoices = new List<MysteryChoice>();
                healthChoices.Add(new MysteryChoice("default", 100));
                MysteryCategory healthCategory = new MysteryCategory("enemyhealth", GetMinValue(healthChoices), GetMaxValue(healthChoices), healthChoices);

                List<MysteryCategory> categories = new List<MysteryCategory>();

                categories.Add(goalCategory);
                categories.Add(towerCategory);
                categories.Add(vulnCategory);
                categories.Add(stateCategory);
                categories.Add(entranceCategory);
                categories.Add(bossCategory);
                categories.Add(enemyCategory);
                categories.Add(hintsCategory);
                categories.Add(swordCategory);
                categories.Add(poolCategory);
                categories.Add(entranceItemsCategory);
                categories.Add(accessCategory);
                categories.Add(damageCategory);
                categories.Add(healthCategory);

                int r = 0;

                for (int i = 0; i < categories.Count; i++)
                {
                    r = GlobalHelpers.GetRandomNumber(0, 100);

                    int j = 0;
                    int valuecount = categories[i].Choices[j].ChoiceValue;

                    while (r > valuecount)
                    {
                        j++;
                        valuecount += categories[i].Choices[j].ChoiceValue;
                    }

                    switch (categories[i].CategoryName)
                    {
                        case "goal":
                            goalindex = j;
                            break;
                        case "toweropen":
                            toweropenindex = j;
                            break;
                        case "ganonvuln":
                            ganonvulnindex = j;
                            break;
                        case "worldstate":
                            worldstateindex = j;
                            break;
                        case "hints":
                            hintsindex = j;
                            break;
                        case "swords":
                            swordsindex = j;
                            break;
                        case "itempool":
                            itempoolindex = j;
                            break;
                        case "accessibility":
                            accessibilityindex = j;
                            break;
                        case "bossshuffle":
                            bossshuffleindex = j;
                            break;
                        case "enemyshuffle":
                            enemyshuffleindex = j;
                            break;
                        case "enemydamage":
                            enemydamageindex = j;
                            break;
                        case "enemyhealth":
                            enemyhealthindex = j;
                            break;
                        case "entranceshuffle":
                            entranceshuffleindex = j;
                            break;
                        case "entranceshuffleitems":
                            entrancedungeonitemsindex = j;
                            break;
                    }
                }

                MysteryEntranceObject meobj = new MysteryEntranceObject();

                meobj.accessibility = accessCategory.Choices[accessibilityindex].ChoiceName;
                meobj.allow_quickswap = true;
                meobj.crystals = new MysteryCrystals();
                meobj.crystals.ganon = int.Parse(vulnCategory.Choices[ganonvulnindex].ChoiceName);
                meobj.crystals.tower = int.Parse(towerCategory.Choices[toweropenindex].ChoiceName);
                meobj.dungeon_items = entranceItemsCategory.Choices[entrancedungeonitemsindex].ChoiceName;
                meobj.enemizer = new MysteryEnemizer();
                meobj.enemizer.boss_shuffle = bossCategory.Choices[bossshuffleindex].ChoiceName;
                meobj.enemizer.enemy_damage = damageCategory.Choices[enemydamageindex].ChoiceName;
                meobj.enemizer.enemy_health = healthCategory.Choices[enemyhealthindex].ChoiceName;
                meobj.enemizer.enemy_shuffle = enemyCategory.Choices[enemyshuffleindex].ChoiceName;
                meobj.enemizer.pot_shuffle = "off";
                meobj.entrances = entranceCategory.Choices[entranceshuffleindex].ChoiceName;
                meobj.glitches = "none";
                meobj.goal = goalCategory.Choices[goalindex].ChoiceName;
                meobj.hints = hintsCategory.Choices[hintsindex].ChoiceName;
                meobj.item = new MysteryItem();
                meobj.item.functionality = "normal";
                meobj.item.pool = poolCategory.Choices[itempoolindex].ChoiceName;
                meobj.item_placement = "advanced";
                meobj.lang = "en";
                meobj.mode = stateCategory.Choices[worldstateindex].ChoiceName;
                meobj.pseudoboots = false;
                meobj.spoilers = "mystery";
                meobj.tournament = true;
                meobj.weapons = swordCategory.Choices[swordsindex].ChoiceName;

                json = JsonConvert.SerializeObject(meobj);

                MysteryObject mysteryobj = JsonConvert.DeserializeObject<MysteryObject>(json);

                //StreamWriter sw = new StreamWriter(@"c:\temp\mysterytext.txt", true);

                //sw.Write(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}", entranceCategory.Choices[entranceshuffleindex].ChoiceName, goalCategory.Choices[goalindex].ChoiceName, towerCategory.Choices[toweropenindex].ChoiceName, vulnCategory.Choices[ganonvulnindex].ChoiceName, stateCategory.Choices[worldstateindex].ChoiceName, hintsCategory.Choices[hintsindex].ChoiceName, swordCategory.Choices[swordsindex].ChoiceName, poolCategory.Choices[itempoolindex].ChoiceName, accessCategory.Choices[accessibilityindex].ChoiceName, bossCategory.Choices[bossshuffleindex].ChoiceName, enemyCategory.Choices[enemyshuffleindex].ChoiceName, damageCategory.Choices[enemydamageindex].ChoiceName, healthCategory.Choices[enemyhealthindex].ChoiceName, entranceItemsCategory.Choices[entrancedungeonitemsindex].ChoiceName));
                //sw.Write(Environment.NewLine);

                //sw.Close();

                //System.Diagnostics.Debug.WriteLine("");
                //System.Diagnostics.Debug.WriteLine(string.Format("Entrance Type\t{0}\t{1}", entranceCategory.Choices[entranceshuffleindex].ChoiceName, entranceCategory.Choices[entranceshuffleindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Goal\t{0}\t{1}", goalCategory.Choices[goalindex].ChoiceName, goalCategory.Choices[goalindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Tower\t{0}\t{1}", towerCategory.Choices[toweropenindex].ChoiceName, towerCategory.Choices[toweropenindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Vuln\t{0}\t{1}", vulnCategory.Choices[ganonvulnindex].ChoiceName, vulnCategory.Choices[ganonvulnindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("World State\t{0}\t{1}", stateCategory.Choices[worldstateindex].ChoiceName, stateCategory.Choices[worldstateindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Hints\t{0}\t{1}", hintsCategory.Choices[hintsindex].ChoiceName, hintsCategory.Choices[hintsindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Swords\t{0}\t{1}", swordCategory.Choices[swordsindex].ChoiceName, swordCategory.Choices[swordsindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Item Pool\t{0}\t{1}", poolCategory.Choices[itempoolindex].ChoiceName, poolCategory.Choices[itempoolindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Accessibility\t{0}\t{1}", accessCategory.Choices[accessibilityindex].ChoiceName, accessCategory.Choices[accessibilityindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Boss Shuffle\t{0}\t{1}", bossCategory.Choices[bossshuffleindex].ChoiceName, bossCategory.Choices[bossshuffleindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Enemy Shuffle\t{0}\t{1}", enemyCategory.Choices[enemyshuffleindex].ChoiceName, enemyCategory.Choices[enemyshuffleindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Damage Shuffle\t{0}\t{1}", damageCategory.Choices[enemydamageindex].ChoiceName, damageCategory.Choices[enemydamageindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Enemy Health\t{0}\t{1}", healthCategory.Choices[enemyhealthindex].ChoiceName, healthCategory.Choices[enemyhealthindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine(string.Format("Dungeon Items\t{0}\t{1}", entranceItemsCategory.Choices[entrancedungeonitemsindex].ChoiceName, entranceItemsCategory.Choices[entrancedungeonitemsindex].ChoiceValue));
                //System.Diagnostics.Debug.WriteLine("");

            }
            catch (WebException ex)
            {
                tb_logs log = new tb_logs();

                log.LogMessage = ex.Message;
                log.DateTimeLogged = DateTime.Now;

                dbContext.tb_logs.Add(log);

                dbContext.SaveChanges();
            }

            return json;

        }


        private string GTBKPlacement()
        {
            int r = GlobalHelpers.GetRandomNumber(0, 22);

            string returnlocation = "";

            switch (r)
            {
                //Bob's Torch
                case 0:
                    returnlocation = "R2Fub24ncyBUb3dlciAtIEJvYidzIFRvcmNoOjE=";
                    break;
                case 1:
                    //DMs Room -Top Left
                    returnlocation = "R2Fub24ncyBUb3dlciAtIERNcyBSb29tIC0gVG9wIExlZnQ6MQ==";
                    break;
                case 2:
                    //DMs Room -Top Right
                    returnlocation = "R2Fub24ncyBUb3dlciAtIERNcyBSb29tIC0gVG9wIFJpZ2h0OjE=";
                    break;
                case 3:
                    //DMs Room -Bottom Left
                    returnlocation = "R2Fub24ncyBUb3dlciAtIERNcyBSb29tIC0gQm90dG9tIExlZnQ6MQ==";
                    break;
                case 4:
                    //DMs Room -Bottom Right
                    returnlocation = "R2Fub24ncyBUb3dlciAtIERNcyBSb29tIC0gQm90dG9tIFJpZ2h0OjE=";
                    break;
                case 5:
                    //Randomizer Room -Top Left
                    returnlocation = "R2Fub24ncyBUb3dlciAtIFJhbmRvbWl6ZXIgUm9vbSAtIFRvcCBMZWZ0OjE=";
                    break;
                case 6:
                    //Randomizer Room -Top Right
                    returnlocation = "R2Fub24ncyBUb3dlciAtIFJhbmRvbWl6ZXIgUm9vbSAtIFRvcCBSaWdodDox";
                    break;
                case 7:
                    //Randomizer Room -Bottom Left
                    returnlocation = "R2Fub24ncyBUb3dlciAtIFJhbmRvbWl6ZXIgUm9vbSAtIEJvdHRvbSBMZWZ0OjE=";
                    break;
                case 8:
                    //Randomizer Room -Bottom Right
                    returnlocation = "R2Fub24ncyBUb3dlciAtIFJhbmRvbWl6ZXIgUm9vbSAtIEJvdHRvbSBSaWdodDox";
                    break;
                case 9:
                    //Firesnake Room
                    returnlocation = "R2Fub24ncyBUb3dlciAtIEZpcmVzbmFrZSBSb29tOjE=";
                    break;
                case 10:
                    //Map Chest
                    returnlocation = "R2Fub24ncyBUb3dlciAtIE1hcCBDaGVzdDox";
                    break;
                case 11:
                    //Hope Room -Left
                    returnlocation = "R2Fub24ncyBUb3dlciAtIEhvcGUgUm9vbSAtIExlZnQ6MQ==";
                    break;
                case 12:
                    //Hope Room -Right
                    returnlocation = "R2Fub24ncyBUb3dlciAtIEhvcGUgUm9vbSAtIFJpZ2h0OjE=";
                    break;
                case 13:
                    //Bob's Chest
                    returnlocation = "R2Fub24ncyBUb3dlciAtIEJvYidzIENoZXN0OjE=";
                    break;
                case 14:
                    //Tile Room
                    returnlocation = "R2Fub24ncyBUb3dlciAtIFRpbGUgUm9vbTox";
                    break;
                case 15:
                    //Compass Room -Top Left
                    returnlocation = "R2Fub24ncyBUb3dlciAtIENvbXBhc3MgUm9vbSAtIFRvcCBMZWZ0OjE=";
                    break;
                case 16:
                    //Compass Room -Top Right
                    returnlocation = "R2Fub24ncyBUb3dlciAtIENvbXBhc3MgUm9vbSAtIFRvcCBSaWdodDox";
                    break;
                case 17:
                    //Compass Room -Bottom Left
                    returnlocation = "R2Fub24ncyBUb3dlciAtIENvbXBhc3MgUm9vbSAtIEJvdHRvbSBMZWZ0OjE=";
                    break;
                case 18:
                    //Compass Room -Bottom Right
                    returnlocation = "R2Fub24ncyBUb3dlciAtIENvbXBhc3MgUm9vbSAtIEJvdHRvbSBSaWdodDox";
                    break;
                case 19:
                    //Big Key Chest
                    returnlocation = "R2Fub24ncyBUb3dlciAtIEJpZyBLZXkgQ2hlc3Q6MQ==";
                    break;
                case 20:
                    //Big Key Room - Left
                    returnlocation = "R2Fub24ncyBUb3dlciAtIEJpZyBLZXkgUm9vbSAtIExlZnQ6MQ==";
                    break;
                case 21:
                    //Big Key Room - Right
                    returnlocation = "R2Fub24ncyBUb3dlciAtIEJpZyBLZXkgUm9vbSAtIFJpZ2h0OjE=";
                    break;
            }

            return returnlocation;
        }

        private string GetSpriteOffset(int val)
        {
            switch (val)
            {
                case 121:
                    return "Bee";
                case 178:
                    return "BeeGood";
                case 216:
                    return "Heart";
                case 217:
                    return "RupeeGreen";
                case 218:
                    return "RupeeBlue";
                case 219:
                    return "RupeeRed";
                case 220:
                    return "BombRefill1";
                case 221:
                    return "BombRefill4";
                case 222:
                    return "BombRefill8";
                case 223:
                    return "MagicRefillSmall";
                case 224:
                    return "MagicRefillFull";
                case 225:
                    return "ArrowRefill5";
                case 226:
                    return "ArrowRefill10";
                case 227:
                    return "Fairy";
                default:
                    return "";
            }
        }

        private string GetPrizePacks(int val1, int val2, int val3, int val4, int val5, int val6, int val7, int val8)
        {
            if (val1 == 216 && val2 == 216 && val3 == 216 && val4 == 216 && val5 == 217 && val6 == 216 && val7 == 216 && val8 == 217)
            {
                return "HeartsPack";
            }
            else if (val1 == 218 && val2 == 217 && val3 == 218 && val4 == 219 && val5 == 218 && val6 == 217 && val7 == 218 && val8 == 218)
            {
                return "RupeesPack";
            }
            else if (val1 == 224 && val2 == 223 && val3 == 223 && val4 == 218 && val5 == 224 && val6 == 223 && val7 == 216 && val8 == 223)
            {
                return "MagicPack";
            }
            else if (val1 == 220 && val2 == 220 && val3 == 220 && val4 == 221 && val5 == 220 && val6 == 220 && val7 == 222 && val8 == 220)
            {
                return "BombsPack";
            }
            else if (val1 == 225 && val2 == 216 && val3 == 225 && val4 == 226 && val5 == 225 && val6 == 216 && val7 == 225 && val8 == 226)
            {
                return "ArrowsPack";
            }
            else if (val1 == 223 && val2 == 217 && val3 == 216 && val4 == 225 && val5 == 223 && val6 == 220 && val7 == 217 && val8 == 216)
            {
                return "SmallVarietyPack";
            }
            else if (val1 == 216 && val2 == 227 && val3 == 224 && val4 == 219 && val5 == 222 && val6 == 216 && val7 == 219 && val8 == 226)
            {
                return "LargeVarietyPack";
            }

            return "";
        }

        private int GetFinishTime(tb_entrants entrant)
        {
            tb_races race = dbContext.tb_races.Where(x => x.race_id == entrant.race_id).FirstOrDefault();

            tb_flags flag = dbContext.tb_flags.Where(x => x.flag_id == race.flag_id).FirstOrDefault();

            TimeSpan ts = DateTime.Now - (DateTime)race.RaceStartTime;

            return (int)Math.Round(ts.TotalSeconds - (flag.IsSpoiler == true ? 900 : 0));
        }

        private int GetMinValue(List<MysteryChoice> choicelist)
        {
            int min = 100;

            foreach (MysteryChoice choice in choicelist)
            {
                if (choice.ChoiceValue < min)
                {
                    min = choice.ChoiceValue;
                }
            }

            return min;
        }

        private int GetMaxValue(List<MysteryChoice> choicelist)
        {
            int max = 0;

            foreach (MysteryChoice choice in choicelist)
            {
                if (choice.ChoiceValue > max)
                {
                    max = choice.ChoiceValue;
                }
            }

            return max;
        }

        // ##### CLASS LIST #####

        public class RegisterReturn
        {
            public bool valid { get; set; }
        }

        public class JoinReturn
        {
            public bool valid { get; set; }
            public string Reason { get; set; }
        }

        public class ScheduleReturn
        {
            public List<ScheduleDetails> ScheduleDetails { get; set; }
        }

        public class ScheduleDetails
        {
            public string RaceName { get; set; }
            public string ModeName { get; set; }
            public int RaceStatus { get; set; }
            public int UTCStart { get; set; }
            public int UTCTimerTicks { get; set; }
            public int Schedule { get; set; }
            public int CurrentlySignedUp { get; set; }
            public int ActivelyRacing { get; set; }
            public string SeedURL { get; set; }
            public string SpoilerHash { get; set; }
            public int Hours { get; set; }
            public bool IsChampionshipRace { get; set; }

        }

        public class StreamReturn
        {
            public List<StreamDetails> StreamList { get; set; }
        }

        public class StreamDetails
        {
            public string Stream { get; set; }
            public string Racer { get; set; }

            public StreamDetails(string stream, string racer)
            {
                Stream = stream;
                Racer = racer;
            }
        }

        public class FinishRaceReturn
        {
            public string RaceGUID { get; set; }
            public string RaceName { get; set; }
            public string FlagName { get; set; }
            public int RacerCount { get; set; }
            public int CurrentlyRacing { get; set; }
            public bool valid { get; set; }
            public string Finish { get; set; }
            public string FinishTime { get; set; }
            public List<RacerStanding> RacerStandingList { get; set; }
        }

        public class RaceStandingsReturn
        {
            public string RaceGUID { get; set; }
            public string RaceName { get; set; }
            public string FlagName { get; set; }
            public int RacerCount { get; set; }
            public int CurrentlyRacing { get; set; }
            public bool valid { get; set; }
            public List<RacerStanding> RacerStandingList { get; set; }
        }

        public class RacerStanding
        {
            public string RacerName { get; set; }
            public int Ranking { get; set; }
            public int FinishTime { get; set; }
            public string RaceTime { get; set; }
            public int Standing { get; set; }
            public string StandingFinish { get; set; }

            public RacerStanding(string name, int ranking, int finishtime, string racetime, int standing, string standingfinish)
            {
                RacerName = name;
                Ranking = ranking;
                FinishTime = finishtime;
                RaceTime = racetime;
                Standing = standing;
                StandingFinish = standingfinish;
            }
        }

        public class SeedReturn
        {
            public bool valid { get; set; }
            public string SeedURL { get; set; }
            public string SeedHash { get; set; }
            public string FlagName { get; set; }
            public int Hours { get; set; }
            public string GrabBag { get; set; }
            public string SpoilerHash { get; set; }

        }

        public class ForfeitReturn
        {
            public bool valid { get; set; }
            public List<string> DiscordID { get; set; }
        }

        public class LightWorld
        {
            [JsonProperty("Sahasrahla's Hut - Left:1")]
            public string SahasrahlaSHutLeft { get; set; }
            [JsonProperty("Sahasrahla's Hut - Middle:1")]
            public string SahasrahlaSHutMiddle { get; set; }
            [JsonProperty("Sahasrahla's Hut - Right:1")]
            public string SahasrahlaSHutRight { get; set; }
            [JsonProperty("Sahasrahla:1")]
            public string Sahasrahla { get; set; }
            [JsonProperty("King Zora:1")]
            public string KingZora { get; set; }
            [JsonProperty("Potion Shop:1")]
            public string PotionShop { get; set; }
            [JsonProperty("Zora's Ledge:1")]
            public string ZoraSLedge { get; set; }
            [JsonProperty("Waterfall Fairy - Left:1")]
            public string WaterfallFairyLeft { get; set; }
            [JsonProperty("Waterfall Fairy - Right:1")]
            public string WaterfallFairyRight { get; set; }
            [JsonProperty("Master Sword Pedestal:1")]
            public string MasterSwordPedestal { get; set; }
            [JsonProperty("King's Tomb:1")]
            public string KingSTomb { get; set; }
            [JsonProperty("Kakariko Tavern:1")]
            public string KakarikoTavern { get; set; }
            [JsonProperty("Chicken House:1")]
            public string ChickenHouse { get; set; }
            [JsonProperty("Kakariko Well - Top:1")]
            public string KakarikoWellTop { get; set; }
            [JsonProperty("Kakariko Well - Left:1")]
            public string KakarikoWellLeft { get; set; }
            [JsonProperty("Kakariko Well - Middle:1")]
            public string KakarikoWellMiddle { get; set; }
            [JsonProperty("Kakariko Well - Right:1")]
            public string KakarikoWellRight { get; set; }
            [JsonProperty("Kakariko Well - Bottom:1")]
            public string KakarikoWellBottom { get; set; }
            [JsonProperty("Blind's Hideout - Top:1")]
            public string BlindSHideoutTop { get; set; }
            [JsonProperty("Blind's Hideout - Left:1")]
            public string BlindSHideoutLeft { get; set; }
            [JsonProperty("Blind's Hideout - Right:1")]
            public string BlindSHideoutRight { get; set; }
            [JsonProperty("Blind's Hideout - Far Left:1")]
            public string BlindSHideoutFarLeft { get; set; }
            [JsonProperty("Blind's Hideout - Far Right:1")]
            public string BlindSHideoutFarRight { get; set; }
            [JsonProperty("Pegasus Rocks:1")]
            public string PegasusRocks { get; set; }
            [JsonProperty("Bottle Merchant:1")]
            public string BottleMerchant { get; set; }
            [JsonProperty("Magic Bat:1")]
            public string MagicBat { get; set; }
            [JsonProperty("Sick Kid:1")]
            public string SickKid { get; set; }
            [JsonProperty("Lost Woods Hideout:1")]
            public string LostWoodsHideout { get; set; }
            [JsonProperty("Lumberjack Tree:1")]
            public string LumberjackTree { get; set; }
            [JsonProperty("Graveyard Ledge:1")]
            public string GraveyardLedge { get; set; }
            [JsonProperty("Mushroom:1")]
            public string Mushroom { get; set; }
            [JsonProperty("Floodgate Chest:1")]
            public string FloodgateChest { get; set; }
            [JsonProperty("Link's House:1")]
            public string LinkSHouse { get; set; }
            [JsonProperty("Aginah's Cave:1")]
            public string AginahSCave { get; set; }
            [JsonProperty("Mini Moldorm Cave - Far Left:1")]
            public string MiniMoldormCaveFarLeft { get; set; }
            [JsonProperty("Mini Moldorm Cave - Left:1")]
            public string MiniMoldormCaveLeft { get; set; }
            [JsonProperty("Mini Moldorm Cave - Right:1")]
            public string MiniMoldormCaveRight { get; set; }
            [JsonProperty("Mini Moldorm Cave - Far Right:1")]
            public string MiniMoldormCaveFarRight { get; set; }
            [JsonProperty("Ice Rod Cave:1")]
            public string IceRodCave { get; set; }
            [JsonProperty("Hobo:1")]
            public string Hobo { get; set; }
            [JsonProperty("Bombos Tablet:1")]
            public string BombosTablet { get; set; }
            [JsonProperty("Cave 45:1")]
            public string Cave45 { get; set; }
            [JsonProperty("Checkerboard Cave:1")]
            public string CheckerboardCave { get; set; }
            [JsonProperty("Mini Moldorm Cave - NPC:1")]
            public string MiniMoldormCaveNPC { get; set; }
            [JsonProperty("Library:1")]
            public string Library { get; set; }
            [JsonProperty("Maze Race:1")]
            public string MazeRace { get; set; }
            [JsonProperty("Desert Ledge:1")]
            public string DesertLedge { get; set; }
            [JsonProperty("Lake Hylia Island:1")]
            public string LakeHyliaIsland { get; set; }
            [JsonProperty("Sunken Treasure:1")]
            public string SunkenTreasure { get; set; }
            [JsonProperty("Flute Spot:1")]
            public string FluteSpot { get; set; }
        }

        public class HyruleCastle
        {
            [JsonProperty("Sanctuary:1")]
            public string Sanctuary { get; set; }
            [JsonProperty("Sewers - Secret Room - Left:1")]
            public string SewersSecretRoomLeft { get; set; }
            [JsonProperty("Sewers - Secret Room - Middle:1")]
            public string SewersSecretRoomMiddle { get; set; }
            [JsonProperty("Sewers - Secret Room - Right:1")]
            public string SewersSecretRoomRight { get; set; }
            [JsonProperty("Sewers - Dark Cross:1")]
            public string SewersDarkCross { get; set; }
            [JsonProperty("Hyrule Castle - Boomerang Chest:1")]
            public string HyruleCastleBoomerangChest { get; set; }
            [JsonProperty("Hyrule Castle - Map Chest:1")]
            public string HyruleCastleMapChest { get; set; }
            [JsonProperty("Hyrule Castle - Zelda's Cell:1")]
            public string HyruleCastleZeldaSCell { get; set; }
            [JsonProperty("Link's Uncle:1")]
            public string LinkSUncle { get; set; }
            [JsonProperty("Secret Passage:1")]
            public string SecretPassage { get; set; }
        }

        public class EasternPalace
        {
            [JsonProperty("Eastern Palace - Compass Chest:1")]
            public string EasternPalaceCompassChest { get; set; }
            [JsonProperty("Eastern Palace - Big Chest:1")]
            public string EasternPalaceBigChest { get; set; }
            [JsonProperty("Eastern Palace - Cannonball Chest:1")]
            public string EasternPalaceCannonballChest { get; set; }
            [JsonProperty("Eastern Palace - Big Key Chest:1")]
            public string EasternPalaceBigKeyChest { get; set; }
            [JsonProperty("Eastern Palace - Map Chest:1")]
            public string EasternPalaceMapChest { get; set; }
            [JsonProperty("Eastern Palace - Boss:1")]
            public string EasternPalaceBoss { get; set; }
            [JsonProperty("Eastern Palace - Prize:1")]
            public string EasternPalacePrize { get; set; }
        }

        public class DesertPalace
        {
            [JsonProperty("Desert Palace - Big Chest:1")]
            public string DesertPalaceBigChest { get; set; }
            [JsonProperty("Desert Palace - Map Chest:1")]
            public string DesertPalaceMapChest { get; set; }
            [JsonProperty("Desert Palace - Torch:1")]
            public string DesertPalaceTorch { get; set; }
            [JsonProperty("Desert Palace - Big Key Chest:1")]
            public string DesertPalaceBigKeyChest { get; set; }
            [JsonProperty("Desert Palace - Compass Chest:1")]
            public string DesertPalaceCompassChest { get; set; }
            [JsonProperty("Desert Palace - Boss:1")]
            public string DesertPalaceBoss { get; set; }
            [JsonProperty("Desert Palace - Prize:1")]
            public string DesertPalacePrize { get; set; }
        }

        public class DeathMountain
        {
            [JsonProperty("Old Man:1")]
            public string OldMan { get; set; }
            [JsonProperty("Spectacle Rock Cave:1")]
            public string SpectacleRockCave { get; set; }
            [JsonProperty("Ether Tablet:1")]
            public string EtherTablet { get; set; }
            [JsonProperty("Spectacle Rock:1")]
            public string SpectacleRock { get; set; }
            [JsonProperty("Spiral Cave:1")]
            public string SpiralCave { get; set; }
            [JsonProperty("Mimic Cave:1")]
            public string MimicCave { get; set; }
            [JsonProperty("Paradox Cave Lower - Far Left:1")]
            public string ParadoxCaveLowerFarLeft { get; set; }
            [JsonProperty("Paradox Cave Lower - Left:1")]
            public string ParadoxCaveLowerLeft { get; set; }
            [JsonProperty("Paradox Cave Lower - Right:1")]
            public string ParadoxCaveLowerRight { get; set; }
            [JsonProperty("Paradox Cave Lower - Far Right:1")]
            public string ParadoxCaveLowerFarRight { get; set; }
            [JsonProperty("Paradox Cave Lower - Middle:1")]
            public string ParadoxCaveLowerMiddle { get; set; }
            [JsonProperty("Paradox Cave Upper - Left:1")]
            public string ParadoxCaveUpperLeft { get; set; }
            [JsonProperty("Paradox Cave Upper - Right:1")]
            public string ParadoxCaveUpperRight { get; set; }
            [JsonProperty("Floating Island:1")]
            public string FloatingIsland { get; set; }
        }

        public class TowerOfHera
        {
            [JsonProperty("Tower of Hera - Big Key Chest:1")]
            public string TowerofHeraBigKeyChest { get; set; }
            [JsonProperty("Tower of Hera - Basement Cage:1")]
            public string TowerofHeraBasementCage { get; set; }
            [JsonProperty("Tower of Hera - Map Chest:1")]
            public string TowerofHeraMapChest { get; set; }
            [JsonProperty("Tower of Hera - Compass Chest:1")]
            public string TowerofHeraCompassChest { get; set; }
            [JsonProperty("Tower of Hera - Big Chest:1")]
            public string TowerofHeraBigChest { get; set; }
            [JsonProperty("Tower of Hera - Boss:1")]
            public string TowerofHeraBoss { get; set; }
            [JsonProperty("Tower of Hera - Prize:1")]
            public string TowerofHeraPrize { get; set; }
        }

        public class CastleTower
        {
            [JsonProperty("Castle Tower - Room 03:1")]
            public string CastleTowerRoom03 { get; set; }
            [JsonProperty("Castle Tower - Dark Maze:1")]
            public string CastleTowerDarkMaze { get; set; }
        }

        public class DarkWorld
        {
            [JsonProperty("Superbunny Cave - Top:1")]
            public string SuperbunnyCaveTop { get; set; }
            [JsonProperty("Superbunny Cave - Bottom:1")]
            public string SuperbunnyCaveBottom { get; set; }
            [JsonProperty("Hookshot Cave - Top Right:1")]
            public string HookshotCaveTopRight { get; set; }
            [JsonProperty("Hookshot Cave - Top Left:1")]
            public string HookshotCaveTopLeft { get; set; }
            [JsonProperty("Hookshot Cave - Bottom Left:1")]
            public string HookshotCaveBottomLeft { get; set; }
            [JsonProperty("Hookshot Cave - Bottom Right:1")]
            public string HookshotCaveBottomRight { get; set; }
            [JsonProperty("Spike Cave:1")]
            public string SpikeCave { get; set; }
            [JsonProperty("Catfish:1")]
            public string Catfish { get; set; }
            [JsonProperty("Pyramid:1")]
            public string Pyramid { get; set; }
            [JsonProperty("Pyramid Fairy - Left:1")]
            public string PyramidFairyLeft { get; set; }
            [JsonProperty("Pyramid Fairy - Right:1")]
            public string PyramidFairyRight { get; set; }
            [JsonProperty("Brewery:1")]
            public string Brewery { get; set; }
            [JsonProperty("C-Shaped House:1")]
            public string CShapedHouse { get; set; }
            [JsonProperty("Chest Game:1")]
            public string ChestGame { get; set; }
            [JsonProperty("Hammer Pegs:1")]
            public string HammerPegs { get; set; }
            [JsonProperty("Bumper Cave:1")]
            public string BumperCave { get; set; }
            [JsonProperty("Blacksmith:1")]
            public string Blacksmith { get; set; }
            [JsonProperty("Purple Chest:1")]
            public string PurpleChest { get; set; }
            [JsonProperty("Hype Cave - Top:1")]
            public string HypeCaveTop { get; set; }
            [JsonProperty("Hype Cave - Middle Right:1")]
            public string HypeCaveMiddleRight { get; set; }
            [JsonProperty("Hype Cave - Middle Left:1")]
            public string HypeCaveMiddleLeft { get; set; }
            [JsonProperty("Hype Cave - Bottom:1")]
            public string HypeCaveBottom { get; set; }
            [JsonProperty("Stumpy:1")]
            public string Stumpy { get; set; }
            [JsonProperty("Hype Cave - NPC:1")]
            public string HypeCaveNPC { get; set; }
            [JsonProperty("Digging Game:1")]
            public string DiggingGame { get; set; }
            [JsonProperty("Mire Shed - Left:1")]
            public string MireShedLeft { get; set; }
            [JsonProperty("Mire Shed - Right:1")]
            public string MireShedRight { get; set; }
        }

        public class DarkPalace
        {
            [JsonProperty("Palace of Darkness - Shooter Room:1")]
            public string PalaceofDarknessShooterRoom { get; set; }
            [JsonProperty("Palace of Darkness - Big Key Chest:1")]
            public string PalaceofDarknessBigKeyChest { get; set; }
            [JsonProperty("Palace of Darkness - The Arena - Ledge:1")]
            public string PalaceofDarknessTheArenaLedge { get; set; }
            [JsonProperty("Palace of Darkness - The Arena - Bridge:1")]
            public string PalaceofDarknessTheArenaBridge { get; set; }
            [JsonProperty("Palace of Darkness - Stalfos Basement:1")]
            public string PalaceofDarknessStalfosBasement { get; set; }
            [JsonProperty("Palace of Darkness - Map Chest:1")]
            public string PalaceofDarknessMapChest { get; set; }
            [JsonProperty("Palace of Darkness - Big Chest:1")]
            public string PalaceofDarknessBigChest { get; set; }
            [JsonProperty("Palace of Darkness - Compass Chest:1")]
            public string PalaceofDarknessCompassChest { get; set; }
            [JsonProperty("Palace of Darkness - Harmless Hellway:1")]
            public string PalaceofDarknessHarmlessHellway { get; set; }
            [JsonProperty("Palace of Darkness - Dark Basement - Left:1")]
            public string PalaceofDarknessDarkBasementLeft { get; set; }
            [JsonProperty("Palace of Darkness - Dark Basement - Right:1")]
            public string PalaceofDarknessDarkBasementRight { get; set; }
            [JsonProperty("Palace of Darkness - Dark Maze - Top:1")]
            public string PalaceofDarknessDarkMazeTop { get; set; }
            [JsonProperty("Palace of Darkness - Dark Maze - Bottom:1")]
            public string PalaceofDarknessDarkMazeBottom { get; set; }
            [JsonProperty("Palace of Darkness - Boss:1")]
            public string PalaceofDarknessBoss { get; set; }
            [JsonProperty("Palace of Darkness - Prize:1")]
            public string PalaceofDarknessPrize { get; set; }
        }

        public class SwampPalace
        {
            [JsonProperty("Swamp Palace - Entrance:1")]
            public string SwampPalaceEntrance { get; set; }
            [JsonProperty("Swamp Palace - Big Chest:1")]
            public string SwampPalaceBigChest { get; set; }
            [JsonProperty("Swamp Palace - Big Key Chest:1")]
            public string SwampPalaceBigKeyChest { get; set; }
            [JsonProperty("Swamp Palace - Map Chest:1")]
            public string SwampPalaceMapChest { get; set; }
            [JsonProperty("Swamp Palace - West Chest:1")]
            public string SwampPalaceWestChest { get; set; }
            [JsonProperty("Swamp Palace - Compass Chest:1")]
            public string SwampPalaceCompassChest { get; set; }
            [JsonProperty("Swamp Palace - Flooded Room - Left:1")]
            public string SwampPalaceFloodedRoomLeft { get; set; }
            [JsonProperty("Swamp Palace - Flooded Room - Right:1")]
            public string SwampPalaceFloodedRoomRight { get; set; }
            [JsonProperty("Swamp Palace - Waterfall Room:1")]
            public string SwampPalaceWaterfallRoom { get; set; }
            [JsonProperty("Swamp Palace - Boss:1")]
            public string SwampPalaceBoss { get; set; }
            [JsonProperty("Swamp Palace - Prize:1")]
            public string SwampPalacePrize { get; set; }
        }

        public class SkullWoods
        {
            [JsonProperty("Skull Woods - Big Chest:1")]
            public string SkullWoodsBigChest { get; set; }
            [JsonProperty("Skull Woods - Big Key Chest:1")]
            public string SkullWoodsBigKeyChest { get; set; }
            [JsonProperty("Skull Woods - Compass Chest:1")]
            public string SkullWoodsCompassChest { get; set; }
            [JsonProperty("Skull Woods - Map Chest:1")]
            public string SkullWoodsMapChest { get; set; }
            [JsonProperty("Skull Woods - Bridge Room:1")]
            public string SkullWoodsBridgeRoom { get; set; }
            [JsonProperty("Skull Woods - Pot Prison:1")]
            public string SkullWoodsPotPrison { get; set; }
            [JsonProperty("Skull Woods - Pinball Room:1")]
            public string SkullWoodsPinballRoom { get; set; }
            [JsonProperty("Skull Woods - Boss:1")]
            public string SkullWoodsBoss { get; set; }
            [JsonProperty("Skull Woods - Prize:1")]
            public string SkullWoodsPrize { get; set; }
        }

        public class ThievesTown
        {
            [JsonProperty("Thieves' Town - Attic:1")]
            public string ThievesTownAttic { get; set; }
            [JsonProperty("Thieves' Town - Big Key Chest:1")]
            public string ThievesTownBigKeyChest { get; set; }
            [JsonProperty("Thieves' Town - Map Chest:1")]
            public string ThievesTownMapChest { get; set; }
            [JsonProperty("Thieves' Town - Compass Chest:1")]
            public string ThievesTownCompassChest { get; set; }
            [JsonProperty("Thieves' Town - Ambush Chest:1")]
            public string ThievesTownAmbushChest { get; set; }
            [JsonProperty("Thieves' Town - Big Chest:1")]
            public string ThievesTownBigChest { get; set; }
            [JsonProperty("Thieves' Town - Blind's Cell:1")]
            public string ThievesTownBlindSCell { get; set; }
            [JsonProperty("Thieves' Town - Boss:1")]
            public string ThievesTownBoss { get; set; }
            [JsonProperty("Thieves' Town - Prize:1")]
            public string ThievesTownPrize { get; set; }
        }

        public class IcePalace
        {
            [JsonProperty("Ice Palace - Big Key Chest:1")]
            public string IcePalaceBigKeyChest { get; set; }
            [JsonProperty("Ice Palace - Compass Chest:1")]
            public string IcePalaceCompassChest { get; set; }
            [JsonProperty("Ice Palace - Map Chest:1")]
            public string IcePalaceMapChest { get; set; }
            [JsonProperty("Ice Palace - Spike Room:1")]
            public string IcePalaceSpikeRoom { get; set; }
            [JsonProperty("Ice Palace - Freezor Chest:1")]
            public string IcePalaceFreezorChest { get; set; }
            [JsonProperty("Ice Palace - Iced T Room:1")]
            public string IcePalaceIcedTRoom { get; set; }
            [JsonProperty("Ice Palace - Big Chest:1")]
            public string IcePalaceBigChest { get; set; }
            [JsonProperty("Ice Palace - Boss:1")]
            public string IcePalaceBoss { get; set; }
            [JsonProperty("Ice Palace - Prize:1")]
            public string IcePalacePrize { get; set; }
        }

        public class MiseryMire
        {
            [JsonProperty("Misery Mire - Big Chest:1")]
            public string MiseryMireBigChest { get; set; }
            [JsonProperty("Misery Mire - Main Lobby:1")]
            public string MiseryMireMainLobby { get; set; }
            [JsonProperty("Misery Mire - Big Key Chest:1")]
            public string MiseryMireBigKeyChest { get; set; }
            [JsonProperty("Misery Mire - Compass Chest:1")]
            public string MiseryMireCompassChest { get; set; }
            [JsonProperty("Misery Mire - Bridge Chest:1")]
            public string MiseryMireBridgeChest { get; set; }
            [JsonProperty("Misery Mire - Map Chest:1")]
            public string MiseryMireMapChest { get; set; }
            [JsonProperty("Misery Mire - Spike Chest:1")]
            public string MiseryMireSpikeChest { get; set; }
            [JsonProperty("Misery Mire - Boss:1")]
            public string MiseryMireBoss { get; set; }
            [JsonProperty("Misery Mire - Prize:1")]
            public string MiseryMirePrize { get; set; }
        }

        public class TurtleRock
        {
            [JsonProperty("Turtle Rock - Chain Chomps:1")]
            public string TurtleRockChainChomps { get; set; }
            [JsonProperty("Turtle Rock - Compass Chest:1")]
            public string TurtleRockCompassChest { get; set; }
            [JsonProperty("Turtle Rock - Roller Room - Left:1")]
            public string TurtleRockRollerRoomLeft { get; set; }
            [JsonProperty("Turtle Rock - Roller Room - Right:1")]
            public string TurtleRockRollerRoomRight { get; set; }
            [JsonProperty("Turtle Rock - Big Chest:1")]
            public string TurtleRockBigChest { get; set; }
            [JsonProperty("Turtle Rock - Big Key Chest:1")]
            public string TurtleRockBigKeyChest { get; set; }
            [JsonProperty("Turtle Rock - Crystaroller Room:1")]
            public string TurtleRockCrystarollerRoom { get; set; }
            [JsonProperty("Turtle Rock - Eye Bridge - Bottom Left:1")]
            public string TurtleRockEyeBridgeBottomLeft { get; set; }
            [JsonProperty("Turtle Rock - Eye Bridge - Bottom Right:1")]
            public string TurtleRockEyeBridgeBottomRight { get; set; }
            [JsonProperty("Turtle Rock - Eye Bridge - Top Left:1")]
            public string TurtleRockEyeBridgeTopLeft { get; set; }
            [JsonProperty("Turtle Rock - Eye Bridge - Top Right:1")]
            public string TurtleRockEyeBridgeTopRight { get; set; }
            [JsonProperty("Turtle Rock - Boss:1")]
            public string TurtleRockBoss { get; set; }
            [JsonProperty("Turtle Rock - Prize:1")]
            public string TurtleRockPrize { get; set; }
        }

        public class GanonsTower
        {
            [JsonProperty("Ganon's Tower - Bob's Torch:1")]
            public string GanonSTowerBobSTorch { get; set; }
            [JsonProperty("Ganon's Tower - DMs Room - Top Left:1")]
            public string GanonSTowerDMsRoomTopLeft { get; set; }
            [JsonProperty("Ganon's Tower - DMs Room - Top Right:1")]
            public string GanonSTowerDMsRoomTopRight { get; set; }
            [JsonProperty("Ganon's Tower - DMs Room - Bottom Left:1")]
            public string GanonSTowerDMsRoomBottomLeft { get; set; }
            [JsonProperty("Ganon's Tower - DMs Room - Bottom Right:1")]
            public string GanonSTowerDMsRoomBottomRight { get; set; }
            [JsonProperty("Ganon's Tower - Randomizer Room - Top Left:1")]
            public string GanonSTowerRandomizerRoomTopLeft { get; set; }
            [JsonProperty("Ganon's Tower - Randomizer Room - Top Right:1")]
            public string GanonSTowerRandomizerRoomTopRight { get; set; }
            [JsonProperty("Ganon's Tower - Randomizer Room - Bottom Left:1")]
            public string GanonSTowerRandomizerRoomBottomLeft { get; set; }
            [JsonProperty("Ganon's Tower - Randomizer Room - Bottom Right:1")]
            public string GanonSTowerRandomizerRoomBottomRight { get; set; }
            [JsonProperty("Ganon's Tower - Firesnake Room:1")]
            public string GanonSTowerFiresnakeRoom { get; set; }
            [JsonProperty("Ganon's Tower - Map Chest:1")]
            public string GanonSTowerMapChest { get; set; }
            [JsonProperty("Ganon's Tower - Big Chest:1")]
            public string GanonSTowerBigChest { get; set; }
            [JsonProperty("Ganon's Tower - Hope Room - Left:1")]
            public string GanonSTowerHopeRoomLeft { get; set; }
            [JsonProperty("Ganon's Tower - Hope Room - Right:1")]
            public string GanonSTowerHopeRoomRight { get; set; }
            [JsonProperty("Ganon's Tower - Bob's Chest:1")]
            public string GanonSTowerBobSChest { get; set; }
            [JsonProperty("Ganon's Tower - Tile Room:1")]
            public string GanonSTowerTileRoom { get; set; }
            [JsonProperty("Ganon's Tower - Compass Room - Top Left:1")]
            public string GanonSTowerCompassRoomTopLeft { get; set; }
            [JsonProperty("Ganon's Tower - Compass Room - Top Right:1")]
            public string GanonSTowerCompassRoomTopRight { get; set; }
            [JsonProperty("Ganon's Tower - Compass Room - Bottom Left:1")]
            public string GanonSTowerCompassRoomBottomLeft { get; set; }
            [JsonProperty("Ganon's Tower - Compass Room - Bottom Right:1")]
            public string GanonSTowerCompassRoomBottomRight { get; set; }
            [JsonProperty("Ganon's Tower - Big Key Chest:1")]
            public string GanonSTowerBigKeyChest { get; set; }
            [JsonProperty("Ganon's Tower - Big Key Room - Left:1")]
            public string GanonSTowerBigKeyRoomLeft { get; set; }
            [JsonProperty("Ganon's Tower - Big Key Room - Right:1")]
            public string GanonSTowerBigKeyRoomRight { get; set; }
            [JsonProperty("Ganon's Tower - Mini Helmasaur Room - Left:1")]
            public string GanonSTowerMiniHelmasaurRoomLeft { get; set; }
            [JsonProperty("Ganon's Tower - Mini Helmasaur Room - Right:1")]
            public string GanonSTowerMiniHelmasaurRoomRight { get; set; }
            [JsonProperty("Ganon's Tower - Pre-Moldorm Chest:1")]
            public string GanonSTowerPreMoldormChest { get; set; }
            [JsonProperty("Ganon's Tower - Moldorm Chest:1")]
            public string GanonSTowerMoldormChest { get; set; }
        }

        public class Special
        {
            [JsonProperty("Turtle Rock Medallion:1")]
            public string TurtleRockMedallion { get; set; }
            [JsonProperty("Misery Mire Medallion:1")]
            public string MiseryMireMedallion { get; set; }
            [JsonProperty("Waterfall Bottle:1")]
            public string WaterfallBottle { get; set; }
            [JsonProperty("Pyramid Bottle:1")]
            public string PyramidBottle { get; set; }
            public string DiggingGameDigs { get; set; }
        }

        public class PullTree
        {
            public string Tier1 { get; set; }
            public string Tier2 { get; set; }
            public string Tier3 { get; set; }
        }

        public class RupeeCrab
        {
            public string Main { get; set; }
            public string Final { get; set; }
        }

        public class PrizePacks
        {
            public string HeartsGroup { get; set; }
            public string RupeesGroup { get; set; }
            public string MagicGroup { get; set; }
            public string BombsGroup { get; set; }
            public string ArrowsGroup { get; set; }
            public string SmallVarietyGroup { get; set; }
            public string LargeVarietyGroup { get; set; }
        }

        public class Drops
        {
            public PullTree PullTree { get; set; }
            public RupeeCrab RupeeCrab { get; set; }
            public string Stun { get; set; }
            public string FishSave { get; set; }
            public PrizePacks PrizePacks { get; set; }
        }

        public class Meta
        {
            public string entry_crystals_ganon { get; set; }
            public string entry_crystals_tower { get; set; }
            public int worlds { get; set; }
            public string item_placement { get; set; }
            public string item_pool { get; set; }
            public string item_functionality { get; set; }
            public string dungeon_items { get; set; }
            public string logic { get; set; }
            public string accessibility { get; set; }
            public string rom_mode { get; set; }
            public string goal { get; set; }
            public string build { get; set; }
            public string mode { get; set; }
            public string weapons { get; set; }
            public int world_id { get; set; }
            public bool tournament { get; set; }
            public int size { get; set; }
            public string hints { get; set; }
            public string spoilers { get; set; }
            public bool allow_quickswap { get; set; }
            [JsonProperty("enemizer.boss_shuffle")]
            public string EnemizerBossShuffle { get; set; }
            [JsonProperty("enemizer.enemy_shuffle")]
            public string EnemizerEnemyShuffle { get; set; }
            [JsonProperty("enemizer.enemy_damage")]
            public string EnemizerEnemyDamage { get; set; }
            [JsonProperty("enemizer.enemy_health")]
            public string EnemizerEnemyHealth { get; set; }
            [JsonProperty("enemizer.pot_shuffle")]
            public string EnemizerPotShuffle { get; set; }
        }

        public class Bosses
        {
            [JsonProperty("Eastern Palace")]
            public string EasternPalace { get; set; }
            [JsonProperty("Desert Palace")]
            public string DesertPalace { get; set; }
            [JsonProperty("Tower Of Hera")]
            public string TowerOfHera { get; set; }
            [JsonProperty("Hyrule Castle")]
            public string HyruleCastle { get; set; }
            [JsonProperty("Palace Of Darkness")]
            public string PalaceOfDarkness { get; set; }
            [JsonProperty("Swamp Palace")]
            public string SwampPalace { get; set; }
            [JsonProperty("Skull Woods")]
            public string SkullWoods { get; set; }
            [JsonProperty("Thieves Town")]
            public string ThievesTown { get; set; }
            [JsonProperty("Ice Palace")]
            public string IcePalace { get; set; }
            [JsonProperty("Misery Mire")]
            public string MiseryMire { get; set; }
            [JsonProperty("Turtle Rock")]
            public string TurtleRock { get; set; }
            [JsonProperty("Ganons Tower Basement")]
            public string GanonsTowerBasement { get; set; }
            [JsonProperty("Ganons Tower Middle")]
            public string GanonsTowerMiddle { get; set; }
            [JsonProperty("Ganons Tower Top")]
            public string GanonsTowerTop { get; set; }
            [JsonProperty("Ganons Tower")]
            public string GanonsTower { get; set; }
            public string Ganon { get; set; }
        }

        public class Spoiler
        {
            [JsonProperty("Light World")]
            public LightWorld LightWorld { get; set; }
            [JsonProperty("Hyrule Castle")]
            public HyruleCastle HyruleCastle { get; set; }
            [JsonProperty("Eastern Palace")]
            public EasternPalace EasternPalace { get; set; }
            [JsonProperty("Desert Palace")]
            public DesertPalace DesertPalace { get; set; }
            [JsonProperty("Death Mountain")]
            public DeathMountain DeathMountain { get; set; }
            [JsonProperty("Tower Of Hera")]
            public TowerOfHera TowerOfHera { get; set; }
            [JsonProperty("Castle Tower")]
            public CastleTower CastleTower { get; set; }
            [JsonProperty("Dark World")]
            public DarkWorld DarkWorld { get; set; }
            [JsonProperty("Dark Palace")]
            public DarkPalace DarkPalace { get; set; }
            [JsonProperty("Swamp Palace")]
            public SwampPalace SwampPalace { get; set; }
            [JsonProperty("Skull Woods")]
            public SkullWoods SkullWoods { get; set; }
            [JsonProperty("Thieves Town")]
            public ThievesTown ThievesTown { get; set; }
            [JsonProperty("Ice Palace")]
            public IcePalace IcePalace { get; set; }
            [JsonProperty("Misery Mire")]
            public MiseryMire MiseryMire { get; set; }
            [JsonProperty("Turtle Rock")]
            public TurtleRock TurtleRock { get; set; }
            [JsonProperty("Ganons Tower")]
            public GanonsTower GanonsTower { get; set; }
            public Special Special { get; set; }
            public Drops Drops { get; set; }
            public Meta meta { get; set; }
            public Bosses Bosses { get; set; }
        }

        public class SpoilerGenerated
        {
            public string logic { get; set; }
            [JsonProperty("patch")]
            public Dictionary<string, int[]>[] patch { get; set; }
            public Spoiler spoiler { get; set; }
            public string hash { get; set; }
            public DateTime generated { get; set; }
            public int size { get; set; }
            public string current_rom_hash { get; set; }
        }

        public class MysteryCategory
        {
            public MysteryCategory(string name, int min, int max, List<MysteryChoice> choices)
            {
                CategoryName = name;
                ValueMin = min;
                ValueMax = max;
                Choices = choices;
            }

            public string CategoryName { get; set; }
            public int ValueMin { get; set; }
            public int ValueMax { get; set; }
            public List<MysteryChoice> Choices { get; set; }

        }

        public class MysteryChoice
        {
            public MysteryChoice(string name, int value)
            {
                ChoiceName = name;
                ChoiceValue = value;
            }

            public string ChoiceName { get; set; }
            public int ChoiceValue { get; set; }

        }

        public class MysteryCrystals
        {
            public int ganon { get; set; }
            public int tower { get; set; }
        }

        public class MysteryEnemizer
        {
            public string boss_shuffle { get; set; }
            public string enemy_damage { get; set; }
            public string enemy_health { get; set; }
            public string enemy_shuffle { get; set; }
            public string pot_shuffle { get; set; }
        }

        public class MysteryItem
        {
            public string functionality { get; set; }
            public string pool { get; set; }
        }


        //Non-Entrance Object
        public class MysteryEntranceObject
        {
            public string accessibility { get; set; }
            public bool allow_quickswap { get; set; }
            public MysteryCrystals crystals { get; set; }
            public string dungeon_items { get; set; }
            public MysteryEnemizer enemizer { get; set; }
            public string entrances { get; set; }
            public string glitches { get; set; }
            public string goal { get; set; }
            public string hints { get; set; }
            public MysteryItem item { get; set; }
            public string item_placement { get; set; }
            public string lang { get; set; }
            public string mode { get; set; }
            public bool pseudoboots { get; set; }
            public string spoilers { get; set; }
            public bool tournament { get; set; }
            public string weapons { get; set; }
        }

        public class MysteryComplexCount
        {
            public int Arrow { get; set; }
            public int ArrowUpgrade10 { get; set; }
            public int ArrowUpgrade5 { get; set; }
            public int BigKeyA1 { get; set; }
            public int BigKeyA2 { get; set; }
            public int BigKeyD1 { get; set; }
            public int BigKeyD2 { get; set; }
            public int BigKeyD3 { get; set; }
            public int BigKeyD4 { get; set; }
            public int BigKeyD5 { get; set; }
            public int BigKeyD6 { get; set; }
            public int BigKeyD7 { get; set; }
            public int BigKeyH1 { get; set; }
            public int BigKeyH2 { get; set; }
            public int BigKeyP1 { get; set; }
            public int BigKeyP2 { get; set; }
            public int BigKeyP3 { get; set; }
            public int BlueClock { get; set; }
            public int BlueMail { get; set; }
            public int BlueShield { get; set; }
            public int Bomb { get; set; }
            public int BombUpgrade10 { get; set; }
            public int BombUpgrade5 { get; set; }
            public int Bombos { get; set; }
            public int BookOfMudora { get; set; }
            public int Boomerang { get; set; }
            public int BossHeartContainer { get; set; }
            public int Bottle { get; set; }
            public int BottleWithBee { get; set; }
            public int BottleWithBluePotion { get; set; }
            public int BottleWithFairy { get; set; }
            public int BottleWithGoldBee { get; set; }
            public int BottleWithGreenPotion { get; set; }
            public int BottleWithRandom { get; set; }
            public int BottleWithRedPotion { get; set; }
            public int Bow { get; set; }
            public int BowAndArrows { get; set; }
            public int BowAndSilverArrows { get; set; }
            public int BugCatchingNet { get; set; }
            public int CaneOfByrna { get; set; }
            public int CaneOfSomaria { get; set; }
            public int Cape { get; set; }
            public int CompassA1 { get; set; }
            public int CompassA2 { get; set; }
            public int CompassD1 { get; set; }
            public int CompassD2 { get; set; }
            public int CompassD3 { get; set; }
            public int CompassD4 { get; set; }
            public int CompassD5 { get; set; }
            public int CompassD6 { get; set; }
            public int CompassD7 { get; set; }
            public int CompassH1 { get; set; }
            public int CompassH2 { get; set; }
            public int CompassP1 { get; set; }
            public int CompassP2 { get; set; }
            public int CompassP3 { get; set; }
            public int Ether { get; set; }
            public int FiftyRupees { get; set; }
            public int FireRod { get; set; }
            public int FiveRupees { get; set; }
            public int Flippers { get; set; }
            public int GreenClock { get; set; }
            public int HalfMagic { get; set; }
            public int Hammer { get; set; }
            public int Heart { get; set; }
            public int HeartContainer { get; set; }
            public int Hookshot { get; set; }
            public int IceRod { get; set; }
            public int KeyA1 { get; set; }
            public int KeyA2 { get; set; }
            public int KeyD1 { get; set; }
            public int KeyD2 { get; set; }
            public int KeyD3 { get; set; }
            public int KeyD4 { get; set; }
            public int KeyD5 { get; set; }
            public int KeyD6 { get; set; }
            public int KeyD7 { get; set; }
            public int KeyH1 { get; set; }
            public int KeyH2 { get; set; }
            public int KeyP1 { get; set; }
            public int KeyP2 { get; set; }
            public int KeyP3 { get; set; }
            public int L1Sword { get; set; }
            public int L1SwordAndShield { get; set; }
            public int L3Sword { get; set; }
            public int L4Sword { get; set; }
            public int Lamp { get; set; }
            public int MagicMirror { get; set; }
            public int MapA1 { get; set; }
            public int MapA2 { get; set; }
            public int MapD1 { get; set; }
            public int MapD2 { get; set; }
            public int MapD3 { get; set; }
            public int MapD4 { get; set; }
            public int MapD5 { get; set; }
            public int MapD6 { get; set; }
            public int MapD7 { get; set; }
            public int MapH1 { get; set; }
            public int MapH2 { get; set; }
            public int MapP1 { get; set; }
            public int MapP2 { get; set; }
            public int MapP3 { get; set; }
            public int MasterSword { get; set; }
            public int MirrorShield { get; set; }
            public int MoonPearl { get; set; }
            public int Mushroom { get; set; }
            public int Nothing { get; set; }
            public int OcarinaActive { get; set; }
            public int OcarinaInactive { get; set; }
            public int OneHundredRupees { get; set; }
            public int OneRupee { get; set; }
            public int PegasusBoots { get; set; }
            public int PieceOfHeart { get; set; }
            public int Powder { get; set; }
            public int PowerGlove { get; set; }
            public int ProgressiveArmor { get; set; }
            public int ProgressiveBow { get; set; }
            public int ProgressiveGlove { get; set; }
            public int ProgressiveShield { get; set; }
            public int ProgressiveSword { get; set; }
            public int Quake { get; set; }
            public int QuarterMagic { get; set; }
            public int RedBoomerang { get; set; }
            public int RedClock { get; set; }
            public int RedMail { get; set; }
            public int RedShield { get; set; }
            public int Rupoor { get; set; }
            public int Shovel { get; set; }
            public int SilverArrowUpgrade { get; set; }
            public int SmallMagic { get; set; }
            public int TenArrows { get; set; }
            public int TenBombs { get; set; }
            public int ThreeBombs { get; set; }
            public int ThreeHundredRupees { get; set; }
            public int TitansMitt { get; set; }
            public int Triforce { get; set; }
            public int TriforcePiece { get; set; }
            public int TwentyRupees { get; set; }
            public int TwentyRupees2 { get; set; }
        }

        public class MysteryCount
        {
            public int ArrowRefill10 { get; set; }
            public int ArrowRefill5 { get; set; }
            public int Bee { get; set; }
            public int BeeGood { get; set; }
            public int BombRefill1 { get; set; }
            public int BombRefill4 { get; set; }
            public int BombRefill8 { get; set; }
            public int Fairy { get; set; }
            public int Heart { get; set; }
            public int MagicRefillFull { get; set; }
            public int MagicRefillSmall { get; set; }
            public int RupeeBlue { get; set; }
            public int RupeeGreen { get; set; }
            public int RupeeRed { get; set; }
        }

        public class MysteryCustom
        {
            public bool canBombJump { get; set; }
            public bool canBootsClip { get; set; }
            public bool canBunnyRevive { get; set; }
            public bool canBunnySurf { get; set; }
            public bool canDungeonRevive { get; set; }
            public bool canFakeFlipper { get; set; }
            public bool canMirrorClip { get; set; }
            public bool canMirrorWrap { get; set; }
            public bool canOWYBA { get; set; }
            public bool canOneFrameClipOW { get; set; }
            public bool canOneFrameClipUW { get; set; }
            public bool canSuperBunny { get; set; }
            public bool canSuperSpeed { get; set; }
            public bool canWaterFairyRevive { get; set; }
            public bool canWaterWalk { get; set; }
            public bool customPrizePacks { get; set; }
            public MysteryDrop drop { get; set; }
            public MysteryComplexItem item { get; set; }

            [JsonProperty("item.Goal.Required")]
            public string itemGoalRequired { get; set; }

            [JsonProperty("item.require.Lamp")]
            public bool itemrequireLamp { get; set; }

            [JsonProperty("item.value.BlueClock")]
            public string itemvalueBlueClock { get; set; }

            [JsonProperty("item.value.GreenClock")]
            public string itemvalueGreenClock { get; set; }

            [JsonProperty("item.value.RedClock")]
            public string itemvalueRedClock { get; set; }

            [JsonProperty("item.value.Rupoor")]
            public string itemvalueRupoor { get; set; }

            [JsonProperty("prize.crossWorld")]
            public bool prizecrossWorld { get; set; }

            [JsonProperty("prize.shuffleCrystals")]
            public bool prizeshuffleCrystals { get; set; }

            [JsonProperty("prize.shufflePendants")]
            public bool prizeshufflePendants { get; set; }

            [JsonProperty("region.bossNormalLocation")]
            public bool regionbossNormalLocation { get; set; }

            [JsonProperty("region.wildBigKeys")]
            public bool regionwildBigKeys { get; set; }

            [JsonProperty("region.wildCompasses")]
            public bool regionwildCompasses { get; set; }

            [JsonProperty("region.wildKeys")]
            public bool regionwildKeys { get; set; }

            [JsonProperty("region.wildMaps")]
            public bool regionwildMaps { get; set; }

            [JsonProperty("rom.dungeonCount")]
            public string romdungeonCount { get; set; }

            [JsonProperty("rom.freeItemMenu")]
            public bool romfreeItemMenu { get; set; }

            [JsonProperty("rom.freeItemText")]
            public bool romfreeItemText { get; set; }

            [JsonProperty("rom.genericKeys")]
            public bool romgenericKeys { get; set; }

            [JsonProperty("rom.mapOnPickup")]
            public bool rommapOnPickup { get; set; }

            [JsonProperty("rom.rupeeBow")]
            public bool romrupeeBow { get; set; }

            [JsonProperty("rom.timerMode")]
            public string romtimerMode { get; set; }

            [JsonProperty("rom.timerStart")]
            public string romtimerStart { get; set; }

            [JsonProperty("spoil.BootsLocation")]
            public bool spoilBootsLocation { get; set; }
        }

        public class MysteryDrop
        {
            public MysteryCount count { get; set; }
        }

        public class MysteryDrops
        {
            [JsonProperty("0")]
            public List<string> _0 { get; set; }

            [JsonProperty("1")]
            public List<string> _1 { get; set; }

            [JsonProperty("2")]
            public List<string> _2 { get; set; }

            [JsonProperty("3")]
            public List<string> _3 { get; set; }

            [JsonProperty("4")]
            public List<string> _4 { get; set; }

            [JsonProperty("5")]
            public List<string> _5 { get; set; }

            [JsonProperty("6")]
            public List<string> _6 { get; set; }
            public List<string> crab { get; set; }
            public List<string> fish { get; set; }
            public List<string> pull { get; set; }
            public List<string> stun { get; set; }
        }

        public class MysteryComplexItem
        {
            public MysteryComplexCount count { get; set; }
        }

        public class MysteryNonEntranceObject
        {
            public string accessibility { get; set; }
            public bool allow_quickswap { get; set; }
            public MysteryCrystals crystals { get; set; }
            public MysteryCustom custom { get; set; }
            public MysteryDrops drops { get; set; }
            public string dungeon_items { get; set; }
            public MysteryEnemizer enemizer { get; set; }
            public string entrances { get; set; }
            public List<string> eq { get; set; }
            public string glitches { get; set; }
            public string goal { get; set; }
            public string hints { get; set; }
            public MysteryItem item { get; set; }
            public string item_placement { get; set; }
            public L l { get; set; }
            public string lang { get; set; }
            public string mode { get; set; }
            public string name { get; set; }
            public string notes { get; set; }
            public bool pseudoboots { get; set; }
            public string spoilers { get; set; }
            public bool tournament { get; set; }
            public string weapons { get; set; }
        }

        public class AvianArtResponse
        {
            public AvianArtSpoiler spoiler { get; set; }
            public string status { get; set; }
            public int attempts { get; set; }
            public string hash { get; set; }
            public int starttime { get; set; }
            public string message { get; set; }
            public string type { get; set; }
        }

        public class AvianArtReturn
        {
            public AvianArtResponse response { get; set; }
        }

        public class AvianArtSpoiler
        {
            public AvianArtMeta meta { get; set; }
        }

        public class AvianArtMeta
        {
            //public string version { get; set; }
            //public string logic { get; set; }
            //public string mode { get; set; }
            //public bool bombbag { get; set; }
            //public string weapons { get; set; }
            //public string flute_mode { get; set; }
            //public string bow_mode { get; set; }
            //public string goal { get; set; }
            //public string shuffle { get; set; }
            //public bool shuffleganon { get; set; }
            //public bool shufflelinks { get; set; }
            //public bool shuffletavern { get; set; }
            //public string skullwoods { get; set; }
            //public string linked_drops { get; set; }
            //public string take_any { get; set; }
            //public string overworld_map { get; set; }
            //public string door_shuffle { get; set; }
            //public int intensity { get; set; }
            //public string door_type_mode { get; set; }
            //public string trap_door_mode { get; set; }
            //public string key_logic { get; set; }
            //public bool decoupledoors { get; set; }
            //public bool door_self_loops { get; set; }
            //public string dungeon_counters { get; set; }
            //public string item_pool { get; set; }
            //public string item_functionality { get; set; }
            //public int gt_crystals { get; set; }
            //public int ganon_crystals { get; set; }
            //public string open_pyramid { get; set; }
            //public string accessibility { get; set; }
            //public string restricted_boss_items { get; set; }
            //public bool hints { get; set; }
            //public bool mapshuffle { get; set; }
            //public bool compassshuffle { get; set; }
            //public string keyshuffle { get; set; }
            //public bool bigkeyshuffle { get; set; }
            //public string boss_shuffle { get; set; }
            //public string enemy_shuffle { get; set; }
            //public string enemy_health { get; set; }
            //public string enemy_damage { get; set; }
            //public string any_enemy_logic { get; set; }
            //public int players { get; set; }
            //public int teams { get; set; }
            //public bool experimental { get; set; }
            //public string dropshuffle { get; set; }
            //public string pottery { get; set; }
            //public bool potshuffle { get; set; }
            //public bool shopsanity { get; set; }
            //public bool pseudoboots { get; set; }
            //public int triforcegoal { get; set; }
            //public int triforcepool { get; set; }
            //public bool race { get; set; }
            //public string user_notes { get; set; }
            //public string code { get; set; }
            public string hash { get; set; }
            //public int gentime { get; set; }
            //public int startgen { get; set; }
        }


        public class InvitationalEvent
        {
            public string EventID { get; set; }
            public string PairingID { get; set; }
            public bool NeedsUpdates { get; set; }
            public string EventName { get; set; }
            public string EventLocation { get; set; }
            public string EventStartTime { get; set; }
            public string EventEndTime { get; set; }

        }

        public class InvitationalSchedule
        {
            public string GroupName { get; set; }
            public string PairingString { get; set; }
        }

        public class RaceTimeGGEntrant
        {
            public RaceTimeGGUser user { get; set; }
            public RaceTimeGGStatus status { get; set; }
            public string finish_time { get; set; }
        }

        public class RaceTimeGG
        {
            public RaceTimeGGStatus status { get; set; }
            public List<RaceTimeGGEntrant> entrants { get; set; }
        }

        public class RaceTimeGGStatus
        {
            public string value { get; set; }
        }

        public class RaceTimeGGUser
        {
            public string full_name { get; set; }
        }

        public class ReturnItem
        {
            public string RestreamerCode { get; set; }
            public string TrackingCode { get; set; }
        }

        public class RestreamKey
        {
            public List<ReturnItem> ReturnItems { get; set; }
        }
        public class InvitationalPlayerInformation
        {
            public string RacerName1 { get; set; }
            public string DiscordID1 { get; set; }
            public string RestreamKey1 { get; set; }
            public string TrackingKey1 { get; set; }
            public bool GetsTrackingKey1 { get; set; }
            public string RacerName2 { get; set; }
            public string DiscordID2 { get; set; }
            public string RestreamKey2 { get; set; }
            public string TrackingKey2 { get; set; }
            public bool GetsTrackingKey2 { get; set; }
            public string GroupName { get; set; }

        }

        public class RankingsObject
        {
            public int racer_id { get; set; }
            public int FinishTime { get; set; }
            public int StartingRating { get; set; }
            public int Wager { get; set; }
            public int Result { get; set; }
            public int ResultPerc { get; set; }
            public int Gain { get; set; }
            public int Remainder { get; set; }
            public int Reward { get; set; }
            public int NewRating { get; set; }
            public int GainLoss { get; set; }
            public int PlacementPerc { get; set; }
            public int PlacementBonus { get; set; }

        }
    }
}
