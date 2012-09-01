using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net.Diablo3
{
    public class Follower
    {
        public string Slug;
        public string Name;
        public string RealName;
        public string Portrait;
        public int Level;
        public Dictionary<string, Item> Items;
        public List<Skill> Skills;
        public List<Skill> Passive;
    }
}
