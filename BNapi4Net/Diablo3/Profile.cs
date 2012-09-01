using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BNapi4Net.Diablo3
{
    public class Profile
    {
        public List<Hero> Heroes { get; set; }

        /// <summary>
        /// Id of last played hero
        /// </summary>
        public int LastHeroPlayed { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastUpdated { get; set; }

        public List<Artisan> Artisans { get; set; }
        public List<Artisan> HardcoreArtisans { get; set; }

        public Kills Kills { get; set; }
        public Dictionary<string, float> TimePlayed { get; set; }

        public List<Hero> FallenHeroes { get; set; }

        public string BattleTag { get; set; }
        public Progression Progression { get; set; }
        public Progression HardcoreProgression { get; set; }

    }

    public class Progression
    {
        public Progress Normal { get; set; }
        public Progress Nightmare { get; set; }
        public Progress Hell { get; set; }
        public Progress Inferno { get; set; }
    }
    public class Progress
    {
        public ActProgress Act1 { get; set; }
        public ActProgress Act2 { get; set; }
        public ActProgress Act3 { get; set; }
        public ActProgress Act4 { get; set; }
    }

    public class ActProgress
    {
        public bool Completed { get; set; }
        public List<Quest> CompletedQuests { get; set; }
    }
    public class Quest
    {
        public string Slug { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return "{Quest " + Slug + "}";
        }
    }

    public class Death
    {
        public int Killer { get; set; }
        public int Location { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Time { get; set; }
    }

}
