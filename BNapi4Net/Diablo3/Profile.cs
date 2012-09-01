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
        public Progress Normal;
        public Progress Nightmare;
        public Progress Hell;
        public Progress Inferno;
    }
    public class Progress
    {
        public ActProgress Act1;
        public ActProgress Act2;
        public ActProgress Act3;
        public ActProgress Act4;
    }

    public class ActProgress
    {
        public bool Completed;
        public List<Quest> CompletedQuests;
    }
    public class Quest
    {
        public string Slug;
        public string Name;

        public override string ToString()
        {
            return "{Quest " + Slug + "}";
        }
    }

    public class Death
    {
        public int Killer;
        public int Location;
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Time;
    }

}
