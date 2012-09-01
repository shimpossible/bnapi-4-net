using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net.Diablo3
{
    public class Artisan
    {
        public string Slug;
        public string Name;
        public string Portrait;

        public Training Training;

        #region Hero Specific
        public int Level;
        public int StepMax;
        public int StepCurrent;
        #endregion
    }

    public class Training
    {
        public List<TierClass> Tiers;
    }

    public class TierClass
    {
        public int Tier;
        public List<Level> Levels;

        public class Level
        {
            public int Tier;
            public int TierLevel;

            /// <summary>
            /// Percentage as a number from 0 to 100
            /// </summary>
            public int Percent;
            public List<Receipe> TrainedRecipes;
            public List<Receipe> TaughtRecipes;

            public long UpgradeCost;
        }
    }

    public class Receipe
    {
        public string Slug;
        public string Name;
        public long Cost;
        public List<Reagent> Reagents;
        public Item ItemProduced;

        public override string ToString()
        {
            return "{Receipe " + Name + "}";
        }

    }

    public class Reagent
    {
        public int Quantity;
        public Item Item;

        public override string ToString()
        {
            return Item + " x " + Quantity;
        }
    }
    
}
