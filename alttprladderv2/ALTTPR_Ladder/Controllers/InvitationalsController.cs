using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALTTPR_Ladder.Controllers
{
    public class InvitationalsController : Controller
    {
        public ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();

        public ActionResult Index(int id = 0)
        {
            Helpers.GlobalHelpers.BrowserLogging("Invitationals", Request.UserHostAddress.ToString(), id.ToString());

            InvitationalModel invmodel = new InvitationalModel();

            invmodel.InvList = GetInvList();

            if (id == 0)
            {
                id = invmodel.InvList[invmodel.InvList.Count - 1].invitational_id;
            }

            invmodel.Week1 = GetInvWeek(id, 1);
            invmodel.Week2 = GetInvWeek(id, 2);
            invmodel.Week3 = GetInvWeek(id, 3);
            invmodel.Week4 = GetInvWeek(id, 4);
            invmodel.Week5 = GetInvWeek(id, 5);

            invmodel.ActiveID = id;
            invmodel.InvitationalName = invmodel.InvList[id - 1].InvitationalName;
            invmodel.UseFiveWeeks = id == 8;

            return View(invmodel);
        }

        private List<InvitationalList> GetInvList()
        {
            List<InvitationalList> fulllist = new List<InvitationalList>();

            List<tb_invitationals> invlist = dbContext.tb_invitationals.ToList();

            foreach (tb_invitationals inv in invlist)
            {
                InvitationalList invgroup = new InvitationalList();

                invgroup.invitational_id = inv.invitational_id;
                invgroup.InvitationalName = inv.InvitationalName;

                fulllist.Add(invgroup);
            }

            return fulllist;
        }

        private InvitationalWeek GetInvWeek(int id, int week)
        {
            InvitationalWeek weekitem = new InvitationalWeek();

            weekitem.GroupA = new List<InvitationalGroup>();
            weekitem.GroupB = new List<InvitationalGroup>();
            weekitem.GroupC = new List<InvitationalGroup>();
            weekitem.GroupD = new List<InvitationalGroup>();

            weekitem.ResultsA = new List<InvitationalResults>();
            weekitem.ResultsB = new List<InvitationalResults>();
            weekitem.ResultsC = new List<InvitationalResults>();
            weekitem.ResultsD = new List<InvitationalResults>();

            List<tb_invitational_pairings> pairingslist = dbContext.tb_invitational_pairings.Where(x => x.invitational_id == id && x.Week == week).ToList();

            foreach (tb_invitational_pairings pairing in pairingslist)
            {
                InvitationalGroup ig = new InvitationalGroup();

                int racer1_id = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant1_id && x.invitational_id == id).Select(x => x.racer_id).FirstOrDefault();
                int racer2_id = dbContext.tb_invitational_participants.Where(x => x.participant_id == pairing.participant2_id && x.invitational_id == id).Select(x => x.racer_id).FirstOrDefault();
                ig.Racer1Name = dbContext.tb_racers.Where(x => x.racer_id == racer1_id).Select(x => x.RacerName).FirstOrDefault();
                ig.Racer2Name = dbContext.tb_racers.Where(x => x.racer_id == racer2_id).Select(x => x.RacerName).FirstOrDefault();
                ig.Racer1Time = (pairing.Participant1Time == null ? "" : ConvertToTime((int)pairing.Participant1Time));
                ig.Racer2Time = (pairing.Participant2Time == null ? "" : ConvertToTime((int)pairing.Participant2Time));
                ig.ScheduledDateTime = pairing.RaceDateTime;
                ig.Mode = dbContext.tb_flags.Where(x => x.flag_id == pairing.flag_id).Select(x => x.FlagName).FirstOrDefault();

                if (pairing.RaceFinished)
                {
                    if (pairing.Participant1Time == 88888)
                    {
                        ig.RaceResult = String.Format("{0} vs {1} [DNP] {2}", ig.Racer1Name, ig.Racer2Name, ig.Mode != null ? " <" + ig.Mode + ">" : "");
                    }
                    else
                    {
                        if (pairing.Participant1Time < pairing.Participant2Time)
                        {
                            ig.RaceResult = String.Format("{0} [{1}] def {2} [{3}] {4}", ig.Racer1Name, ig.Racer1Time, ig.Racer2Name, ig.Racer2Time, ig.Mode != null ? " <" + ig.Mode + ">" : "");
                        }
                        else
                        {
                            ig.RaceResult = String.Format("{0} [{1}] def {2} [{3}] {4}", ig.Racer2Name, ig.Racer2Time, ig.Racer1Name, ig.Racer1Time, ig.Mode != null ? " <" + ig.Mode + ">" : "");
                        }
                    }
                }
                else
                {
                    ig.RaceResult = String.Format("{0} vs {1} {2} {3}", ig.Racer1Name, ig.Racer2Name, ig.ScheduledDateTime != null ? ((DateTime)ig.ScheduledDateTime).ToString("M/d/yyyy h:mm tt") : "", ig.Mode != null ? " <" + ig.Mode + ">" : "");
                }

                bool add1toresults = true;
                bool add2toresults = true;

                if (pairing.GroupName == "A" || pairing.GroupName == "F")
                {
                    weekitem.GroupA.Add(ig);

                    foreach (InvitationalResults ir in weekitem.ResultsA)
                    {
                        if (ir.participant_id == racer1_id)
                        {
                            add1toresults = false;
                        }
                        if (ir.participant_id == racer2_id)
                        {
                            add2toresults = false;
                        }
                    }

                    if (add1toresults)
                    {
                        InvitationalResults invres = new InvitationalResults();

                        invres.participant_id = racer1_id;
                        invres.Racer = ig.Racer1Name;
                        invres.Wins = 0;
                        invres.Losses = 0;
                        invres.Percentage = 0;

                        weekitem.ResultsA.Add(invres);
                    }

                    if (add2toresults)
                    {
                        InvitationalResults invres = new InvitationalResults();

                        invres.participant_id = racer2_id;
                        invres.Racer = ig.Racer2Name;
                        invres.Wins = 0;
                        invres.Losses = 0;
                        invres.Percentage = 0;

                        weekitem.ResultsA.Add(invres);
                    }

                    if (pairing.RaceFinished && pairing.Participant1Time != 88888)
                    {
                        for (int i = 0; i < weekitem.ResultsA.Count; i++)
                        {
                            if (weekitem.ResultsA[i].participant_id == racer1_id)
                            {
                                if (pairing.Participant1Time < pairing.Participant2Time)
                                {
                                    weekitem.ResultsA[i].Wins++;
                                }
                                else
                                {
                                    weekitem.ResultsA[i].Losses++;
                                }

                                weekitem.ResultsA[i].Percentage = (weekitem.ResultsA[i].Wins * 100) / (weekitem.ResultsA[i].Wins + weekitem.ResultsA[i].Losses);
                            }
                            else if (weekitem.ResultsA[i].participant_id == racer2_id)
                            {
                                if (pairing.Participant2Time < pairing.Participant1Time)
                                {
                                    weekitem.ResultsA[i].Wins++;
                                }
                                else
                                {
                                    weekitem.ResultsA[i].Losses++;
                                }

                                weekitem.ResultsA[i].Percentage = (weekitem.ResultsA[i].Wins * 100) / (weekitem.ResultsA[i].Wins + weekitem.ResultsA[i].Losses);
                            }
                        }
                    }
                }
                else if (pairing.GroupName == "B")
                {
                    weekitem.GroupB.Add(ig);

                    foreach (InvitationalResults ir in weekitem.ResultsB)
                    {
                        if (ir.participant_id == racer1_id)
                        {
                            add1toresults = false;
                        }
                        if (ir.participant_id == racer2_id)
                        {
                            add2toresults = false;
                        }
                    }

                    if (add1toresults)
                    {
                        InvitationalResults invres = new InvitationalResults();

                        invres.participant_id = racer1_id;
                        invres.Racer = ig.Racer1Name;
                        invres.Wins = 0;
                        invres.Losses = 0;
                        invres.Percentage = 0;

                        weekitem.ResultsB.Add(invres);
                    }

                    if (add2toresults)
                    {
                        InvitationalResults invres = new InvitationalResults();

                        invres.participant_id = racer2_id;
                        invres.Racer = ig.Racer2Name;
                        invres.Wins = 0;
                        invres.Losses = 0;
                        invres.Percentage = 0;

                        weekitem.ResultsB.Add(invres);
                    }

                    if (pairing.RaceFinished && pairing.Participant1Time != 88888)
                    {
                        for (int i = 0; i < weekitem.ResultsB.Count; i++)
                        {
                            if (weekitem.ResultsB[i].participant_id == racer1_id)
                            {
                                if (pairing.Participant1Time < pairing.Participant2Time)
                                {
                                    weekitem.ResultsB[i].Wins++;
                                }
                                else
                                {
                                    weekitem.ResultsB[i].Losses++;
                                }

                                weekitem.ResultsB[i].Percentage = (weekitem.ResultsB[i].Wins * 100) / (weekitem.ResultsB[i].Wins + weekitem.ResultsB[i].Losses);
                            }
                            else if (weekitem.ResultsB[i].participant_id == racer2_id)
                            {
                                if (pairing.Participant2Time < pairing.Participant1Time)
                                {
                                    weekitem.ResultsB[i].Wins++;
                                }
                                else
                                {
                                    weekitem.ResultsB[i].Losses++;
                                }

                                weekitem.ResultsB[i].Percentage = (weekitem.ResultsB[i].Wins * 100) / (weekitem.ResultsB[i].Wins + weekitem.ResultsB[i].Losses);
                            }
                        }
                    }
                }
                else if (pairing.GroupName == "C")
                {
                    weekitem.GroupC.Add(ig);

                    foreach (InvitationalResults ir in weekitem.ResultsC)
                    {
                        if (ir.participant_id == racer1_id)
                        {
                            add1toresults = false;
                        }
                        if (ir.participant_id == racer2_id)
                        {
                            add2toresults = false;
                        }
                    }

                    if (add1toresults)
                    {
                        InvitationalResults invres = new InvitationalResults();

                        invres.participant_id = racer1_id;
                        invres.Racer = ig.Racer1Name;
                        invres.Wins = 0;
                        invres.Losses = 0;
                        invres.Percentage = 0;

                        weekitem.ResultsC.Add(invres);
                    }

                    if (add2toresults)
                    {
                        InvitationalResults invres = new InvitationalResults();

                        invres.participant_id = racer2_id;
                        invres.Racer = ig.Racer2Name;
                        invres.Wins = 0;
                        invres.Losses = 0;
                        invres.Percentage = 0;

                        weekitem.ResultsC.Add(invres);
                    }

                    if (pairing.RaceFinished && pairing.Participant1Time != 88888)
                    {
                        for (int i = 0; i < weekitem.ResultsC.Count; i++)
                        {
                            if (weekitem.ResultsC[i].participant_id == racer1_id)
                            {
                                if (pairing.Participant1Time < pairing.Participant2Time)
                                {
                                    weekitem.ResultsC[i].Wins++;
                                }
                                else
                                {
                                    weekitem.ResultsC[i].Losses++;
                                }

                                weekitem.ResultsC[i].Percentage = (weekitem.ResultsC[i].Wins * 100) / (weekitem.ResultsC[i].Wins + weekitem.ResultsC[i].Losses);
                            }
                            else if (weekitem.ResultsC[i].participant_id == racer2_id)
                            {
                                if (pairing.Participant2Time < pairing.Participant1Time)
                                {
                                    weekitem.ResultsC[i].Wins++;
                                }
                                else
                                {
                                    weekitem.ResultsC[i].Losses++;
                                }

                                weekitem.ResultsC[i].Percentage = (weekitem.ResultsC[i].Wins * 100) / (weekitem.ResultsC[i].Wins + weekitem.ResultsC[i].Losses);
                            }
                        }
                    }
                }
                else if (pairing.GroupName == "D")
                {
                    weekitem.GroupD.Add(ig);

                    foreach (InvitationalResults ir in weekitem.ResultsD)
                    {
                        if (ir.participant_id == racer1_id)
                        {
                            add1toresults = false;
                        }
                        if (ir.participant_id == racer2_id)
                        {
                            add2toresults = false;
                        }
                    }

                    if (add1toresults)
                    {
                        InvitationalResults invres = new InvitationalResults();

                        invres.participant_id = racer1_id;
                        invres.Racer = ig.Racer1Name;
                        invres.Wins = 0;
                        invres.Losses = 0;
                        invres.Percentage = 0;

                        weekitem.ResultsD.Add(invres);
                    }

                    if (add2toresults)
                    {
                        InvitationalResults invres = new InvitationalResults();

                        invres.participant_id = racer2_id;
                        invres.Racer = ig.Racer2Name;
                        invres.Wins = 0;
                        invres.Losses = 0;
                        invres.Percentage = 0;

                        weekitem.ResultsD.Add(invres);
                    }

                    if (pairing.RaceFinished && pairing.Participant1Time != 88888)
                    {
                        for (int i = 0; i < weekitem.ResultsD.Count; i++)
                        {
                            if (weekitem.ResultsD[i].participant_id == racer1_id)
                            {
                                if (pairing.Participant1Time < pairing.Participant2Time)
                                {
                                    weekitem.ResultsD[i].Wins++;
                                }
                                else
                                {
                                    weekitem.ResultsD[i].Losses++;
                                }

                                weekitem.ResultsD[i].Percentage = (weekitem.ResultsD[i].Wins * 100) / (weekitem.ResultsD[i].Wins + weekitem.ResultsD[i].Losses);
                            }
                            else if (weekitem.ResultsD[i].participant_id == racer2_id)
                            {
                                if (pairing.Participant2Time < pairing.Participant1Time)
                                {
                                    weekitem.ResultsD[i].Wins++;
                                }
                                else
                                {
                                    weekitem.ResultsD[i].Losses++;
                                }

                                weekitem.ResultsD[i].Percentage = (weekitem.ResultsD[i].Wins * 100) / (weekitem.ResultsD[i].Wins + weekitem.ResultsD[i].Losses);
                            }
                        }
                    }
                }
            }

            if (weekitem.ResultsA.Count > 0)
            {
                weekitem.ResultsA = weekitem.ResultsA.OrderByDescending(x => x.Percentage).ToList();
            }
            if (weekitem.ResultsB.Count > 0)
            {
                weekitem.ResultsB = weekitem.ResultsB.OrderByDescending(x => x.Percentage).ToList();
            }
            if (weekitem.ResultsC.Count > 0)
            {
                weekitem.ResultsC = weekitem.ResultsC.OrderByDescending(x => x.Percentage).ToList();
            }
            if (weekitem.ResultsD.Count > 0)
            {
                weekitem.ResultsD = weekitem.ResultsD.OrderByDescending(x => x.Percentage).ToList();
            }


            return weekitem;
        }

        private string ConvertToTime(int FinishTime)
        {
            if (FinishTime == 99999)
            {
                return "FF";
            }

            if (FinishTime == 88888)
            {
                return "DNF";
            }

            string h;
            string m;
            string s;

            h = (FinishTime / 3600).ToString();
            m = ((FinishTime % 3600) / 60).ToString();
            s = (FinishTime % 60).ToString();

            return string.Format("{0}:{1}:{2}", h, (m.Length == 2 ? m : "0" + m), (s.Length == 2 ? s : "0" + s));
        }

    }

    public class InvitationalModel
    {
        public List<InvitationalList> InvList { get; set; }
        public string InvitationalName { get; set; }
        public InvitationalWeek Week1 { get; set; }
        public InvitationalWeek Week2 { get; set; }
        public InvitationalWeek Week3 { get; set; }
        public InvitationalWeek Week4 { get; set; }
        public InvitationalWeek Week5 { get; set; }
        public int ActiveID { get; set; }
        public bool UseFiveWeeks { get; set; }

    }

    public class InvitationalList
    {
        public int invitational_id { get; set; }
        public string InvitationalName { get; set; }
    }

    public class InvitationalWeek
    {
        public List<InvitationalGroup> GroupA { get; set; }
        public List<InvitationalGroup> GroupB { get; set; }
        public List<InvitationalGroup> GroupC { get; set; }
        public List<InvitationalGroup> GroupD { get; set; }
        public List<InvitationalResults> ResultsA { get; set; }
        public List<InvitationalResults> ResultsB { get; set; }
        public List<InvitationalResults> ResultsC { get; set; }
        public List<InvitationalResults> ResultsD { get; set; }

    }

    public class InvitationalResults
    {
        public int participant_id { get; set; }
        public string Racer { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Percentage { get; set; }

    }

    public class InvitationalGroup
    { 
        public string Racer1Name { get; set; }
        public string Racer2Name { get; set; }
        public string Racer1Time { get; set; }
        public string Racer2Time { get; set; }
        public DateTime? ScheduledDateTime { get; set; }
        public string RaceResult { get; set; }
        public string Mode { get; set; }

    }

}