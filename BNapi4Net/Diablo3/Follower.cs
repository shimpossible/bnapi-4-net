using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net.Diablo3
{
    public class Follower
    {
        public string Slug { get; set; }
        public string Name { get; set; }
        public string RealName { get; set; }
        public string Portrait { get; set; }
        public int Level { get; set; }
        public Dictionary<string, Item> Items { get; set; }
        public List<Skill> Skills { get; set; }
        public List<Skill> Passive { get; set; }
    }
}
