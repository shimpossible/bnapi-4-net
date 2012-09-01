using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net.Diablo3
{
    public class Artisan
    {
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Portrait { get; set; }

        public Training Training { get; set; }

        #region Hero Specific
        public int Level { get; set; }
        public int StepMax { get; set; }
        public int StepCurrent { get; set; }
        #endregion
    }

    public class Training
    {
        public List<TierClass> Tiers { get; set; }
    }

    public class TierClass
    {
        public int Tier { get; set; }
        public List<Level> Levels { get; set; }

        public class Level
        {
            public int Tier { get; set; }
            public int TierLevel { get; set; }

            /// <summary>
            /// Percentage as a number from 0 to 100
            /// </summary>
            public int Percent { get; set; }
            public List<Receipe> TrainedRecipes { get; set; }
            public List<Receipe> TaughtRecipes { get; set; }

            public long UpgradeCost;
        }
    }

    public class Receipe
    {
        public string Slug { get; set; }
        public string Name { get; set; }
        public long Cost { get; set; }
        public List<Reagent> Reagents { get; set; }
        public Item ItemProduced { get; set; }

        public override string ToString()
        {
            return "{Receipe " + Name + "}";
        }

    }

    public class Reagent
    {
        public int Quantity { get; set; }
        public Item Item { get; set; }

        public override string ToString()
        {
            return Item + " x " + Quantity;
        }
    }
    
}
