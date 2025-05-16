using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace ALTTPR_Ladder.Controllers
{
    public class SpoilersController : Controller
    {
        public ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();

        public ActionResult GetSpoilerLog(string id)
        {
            tb_spoilers s = dbContext.tb_spoilers.Where(x => x.SpoilerHash == id).FirstOrDefault();

            if (s != null)
            {
                tb_races r = dbContext.tb_races.Where(x => x.race_id == s.race_id && x.flag_id == 14 && x.HasStarted == true).FirstOrDefault();

                if (r != null)
                {
                    string filepath = s.FileLocation;
                    byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                    string contentType = MimeMapping.GetMimeMapping(filepath);

                    return File(filedata, "text/plain", s.SpoilerHash + ".txt");

                }
            }

            return View();
        }
    }
}