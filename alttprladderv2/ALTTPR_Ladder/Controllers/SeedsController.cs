using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace ALTTPR_Ladder.Controllers
{
    public class SeedsController : Controller
    {
        public ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();

        public ActionResult GetSeed(string id)
        {
            Helpers.GlobalHelpers.BrowserLogging("Get Seed", Request.UserHostAddress.ToString(), id.ToString());

            tb_seeds seed = dbContext.tb_seeds.Where(x => x.SeedHash == id).FirstOrDefault();

            if (seed != null)
            {
                tb_seed_tracking st = new tb_seed_tracking();

                st.seed_id = seed.seed_id;
                st.IPAddress = Request.UserHostAddress.ToString();
                st.DownloadDateTime = DateTime.Now;

                dbContext.tb_seed_tracking.Add(st);

                dbContext.SaveChanges();

                return Redirect(dbContext.tb_races.Where(x => x.race_id == seed.race_id).Select(x => x.SeedURL).FirstOrDefault());
            }

            return Redirect("https://alttprladder.com/");
        }
    }
}