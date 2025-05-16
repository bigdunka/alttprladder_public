using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALTTPR_Ladder.Helpers
{
    public class GlobalHelpers
    {
        public static ALTTPR_LadderEntities1 dbContext = new ALTTPR_LadderEntities1();
        private static Random random = new Random();
        public static readonly Random rnd = new Random();

        public static void BrowserLogging(string request, string ip, string parameter = "")
        {
            var browseobject = new tb_browse_logs();

            browseobject.Request = request;
            browseobject.IPAddress = ip;
            browseobject.Parameter = parameter;
            browseobject.RequestDateTime = DateTime.Now;

            dbContext.tb_browse_logs.Add(browseobject);

            dbContext.SaveChanges();
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static int RandomInt(int length)
        {
            return random.Next(0, length) + 1;
        }

        public static int GetRandomNumber(int min, int max)
        {
            rnd.Next(min, max);
            rnd.Next(min, max);
            rnd.Next(min, max);

            return rnd.Next(min, max);
        }

        public static string ConvertToTime(int FinishTime)
        {
            if (FinishTime == 0)
            {
                return "RACING";
            }

            if (FinishTime == 99999)
            {
                return "FF";
            }

            if (FinishTime == 88888)
            {
                return "DQ";
            }

            string h;
            string m;
            string s;

            h = (FinishTime / 3600).ToString();
            m = ((FinishTime % 3600) / 60).ToString();
            s = (FinishTime % 60).ToString();

            return string.Format("{0}:{1}:{2}", h, (m.Length == 2 ? m : "0" + m), (s.Length == 2 ? s : "0" + s));
        }


        public static int ConvertToUTC(DateTime dt)
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            return (int)((dt.ToUniversalTime().Ticks - epochTicks) / TimeSpan.TicksPerSecond);
        }

        public static string ConvertToPlace(int place)
        {
            string suffix = "";
            if (place % 10 == 1 && place != 11)
            {
                suffix = "st";
            }
            else if (place % 10 == 2 && place != 12)
            {
                suffix = "nd";
            }
            else if (place % 10 == 3 && place != 13)
            {
                suffix = "rd";
            }
            else
            {
                suffix = "th";
            }

            return place.ToString() + suffix;
        }
    }
}