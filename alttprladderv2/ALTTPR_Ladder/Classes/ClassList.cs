using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace ALTTPR_Ladder
{
    // ####### PUBLIC RETURN CLASSES #######
    public class PublicRacers
    {
        public Guid RacerGUID { get; set; }
        public string RacerName { get; set; }
        public string DiscordName { get; set; }

    }

    public class PublicRacerDetails
    {
        public string RacerName { get; set; }
        public string RacerTag { get; set; }
        public string RacerStream { get; set; }
    }

    public class PublicRacerRanking
    {
        public string FlagName { get; set; }
        public int Ranking { get; set; }
        public int Rank { get; set; }
        public int Firsts { get; set; }
        public int Seconds { get; set; }
        public int Thirds { get; set; }
        public int Forfeits { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PublicRacerHistory
    {
        public string FlagName { get; set; }
        public List<PublicRacerResult> Results { get; set; }
    }

    public class PublicRacerResult
    {
        public string RaceName { get; set; }
        public Guid RaceGUID { get; set; }
        public DateTime StartDateTime { get; set; }
        public int FinishTime { get; set; }
        public string Result { get; set; }
        public int RacerCount { get; set; }
        public int Ranking { get; set; }
        public int GainLoss { get; set; }
    }

    public class PublicSeasons
    {
        public int season_id { get; set; }
        public string SeasonName { get; set; }
        public bool IsCurrentSeason { get; set; }
    }

    public class PublicFlags
    {
        public string FlagName { get; set; }
        public int flag_id { get; set; }
        public int HoursToComplete { get; set; }
    }

    public class PublicSchedule
    {
        public int race_id { get; set; }
        public string Season { get; set; }
        public string Mode { get; set; }
        public string StartTime { get; set; }
        public string RaceName { get; set; }
        public bool HasCompleted { get; set; }
        public int ParticipantCount { get; set; }
    }

    public class PublicRacerRaceHistory
    {
        public string RacerName { get; set; }
        public int TotalCount { get; set; }
        //public List<PublicRacerHistory> RaceHistory { get; set; }
    }

    public class Pairings
    {
        public int racer_id { get; set; }
        public int ranking { get; set; }
        public string RacerName { get; set; }

    }

    public class RandomizerResponse
    {

        public string logic { get; set; }
        public string patch { get; set; }
        public string hash { get; set; }

    }

    public class Crystals
    {
        public int ganon { get; set; }
        public int tower { get; set; }
    }

    public class Custom 
    { 
        public bool regionwildBigKeys { get; set; }
        public bool regionwildCompasses { get; set; }
        public bool regionwildKeys { get; set; }
        public bool regionwildMaps { get; set; }
        public string romtimerMode { get; set; }
    }

    public class Enemizer
    {
        public string boss_shuffle { get; set; }
        public string enemy_damage { get; set; }
        public string enemy_health { get; set; }
        public string enemy_shuffle { get; set; }
    }

    public class Item2
    {
        public string functionality { get; set; }
        public string pool { get; set; }
    }

    public class L
    {
    }

    public class Settings
    {
        public string accessibility { get; set; }
        public bool allow_quickswap { get; set; }
        public Crystals crystals { get; set; }
        public Custom custom { get; set; }
        public string dungeon_items { get; set; }
        public Enemizer enemizer { get; set; }
        public List<string> eq { get; set; }
        public string glitches { get; set; }
        public string goal { get; set; }
        public string hints { get; set; }
        public Item2 item { get; set; }
        public string item_placement { get; set; }
        public L l { get; set; }
        public string weapons { get; set; }
    }

    public class MysteryObject
    {
        public bool customizer { get; set; }
        public string endpoint { get; set; }
        public Settings settings { get; set; }
    }

    public class PublicRankingModel
    {
        public string FlagName { get; set; }
        public int flag_id { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<PublicRankingObject> Rankings { get; set; }
    }

    public class PublicRankingObject
    {
        public Guid RacerGUID { get; set; }
        public string RacerName { get; set; }
        public int Ranking { get; set; }
        public int Rank { get; set; }
        public int Firsts { get; set; }
        public int Seconds { get; set; }
        public int Thirds { get; set; }
        public int Forfeits { get; set; }
    }

    public class PublicStatsReturn
    {
        public string Category { get; set; }
        public int flag_id { get; set; }
        public int Played { get; set; }
        public int Finished { get; set; }
        public int Fastest { get; set; }
        public int race_id { get; set; }
        public int Average { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
    }

    public class PublicRaceReturn
    {
        public Guid RaceGUID { get; set; }
        public string RaceName { get; set; }
        public string RaceStatus { get; set; }
        public int TotalRacers { get; set; }
        public int ActiveRacers { get; set; }
        public int RaceSeconds { get; set; }
        public string RaceTimer { get; set; }
    }
}