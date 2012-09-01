using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BNapi4Net.Diablo3
{
    public class Hero : D3Object
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public Gender Gender { get; set; }
        public int Level { get; set; }
        public bool Hardcore { get; set; }
        public Skills Skills { get; set; }
        public Dictionary<string, Item> Items {get;set;}
        public Dictionary<string, Follower> Followers { get; set; }
        public Dictionary<string, double> Stats { get; set; }
        public Progression Progress { get; set; }
        public Kills Kills { get; set; }

        [JsonProperty("last-updated")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastUpdated { get; set; }

        public bool Dead { get; set; }

        internal string battleTag { get; set; }
        /// <summary>
        /// Refresh the data
        /// </summary>
        public void Refresh()
        {
            Hero other = Client.GetHero(battleTag, Id);
            this.Name = other.Name;
            this.Class = other.Class;
            this.Gender = other.Gender;
            this.Level = other.Level;
            this.Hardcore = other.Hardcore;
            this.Skills = other.Skills;
            this.Items = other.Items;
            this.Followers = other.Followers;
            this.Stats = other.Stats;
            this.Progress = other.Progress;
            this.Kills = other.Kills;
            this.LastUpdated = other.LastUpdated;
            this.Dead = other.Dead;
        }
    }

    public class Skills
    {
        public List<SkillRune> Active { get; set; }
        public List<SkillRune> Passive { get; set; }
    }

    public class SkillRune
    {
        public Skill Skill { get; set; }
        public Rune Rune { get; set; }
    }

    public enum IconSize
    {
        Small = 21,
        Medium = 42,
        Large  = 64,
    }
    public class Skill : D3Object
    {
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }


        System.Windows.Media.ImageSource _largeImage;
        System.Windows.Media.ImageSource _medImage;
        System.Windows.Media.ImageSource _smallImage;

        /// <summary>
        /// 64x64 thumnail of skill image
        /// </summary>
        public System.Windows.Media.ImageSource LargeImage
        {
            get
            {
                if (_largeImage == null)
                {
                    _largeImage = GetImage(IconSize.Large);
                }
                return _largeImage;
            }
        }

        /// <summary>
        /// 42x42 thumnail of skill image
        /// </summary>
        public System.Windows.Media.ImageSource  MediumImage
        {
            get
            {
                if (_medImage == null)
                {
                    _medImage = GetImage(IconSize.Medium);
                }
                return _medImage;
            }
        }

        /// <summary>
        /// 21x21 thumnail of skill image
        /// </summary>
        public System.Windows.Media.ImageSource SmallImage
        {
            get
            {
                if (_smallImage == null)
                {
                    _smallImage = GetImage(IconSize.Small);
                }
                return _smallImage;
            }
        }


        /// <summary>
        /// Download a WPF compatible image of the skill
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public System.Windows.Media.ImageSource GetImage(IconSize size)
        {
            // small or large
            string sizeStr = "64";
            if (size == IconSize.Medium) sizeStr = "42";
            if (size == IconSize.Small) sizeStr = "21";

            System.IO.Stream s = Client.ReadMedia("d3/icons/skills/" + sizeStr + "/" + this.Icon + ".png");
            System.Windows.Media.Imaging.BitmapImage b = new System.Windows.Media.Imaging.BitmapImage(); ;
            b.BeginInit();
            b.StreamSource = s;
            b.EndInit();

            return b;
        }

        System.Drawing.Image _smallIcon = null;
        System.Drawing.Image _medIcon = null;        
        System.Drawing.Image _largeIcon = null;
        public System.Drawing.Image SmallIcon
        {
            get
            {
                if (_smallIcon == null)
                {
                    _smallIcon = GetIcon(IconSize.Small);
                }
                return _smallIcon;
            }
        }
        public System.Drawing.Image MediumIcon
        {
            get
            {
                if (_medIcon == null)
                {
                    _medIcon = GetIcon(IconSize.Medium);
                }
                return _medIcon;
            }
        }
        public System.Drawing.Image LargeIcon
        {
            get
            {
                if (_largeIcon == null)
                {
                    _largeIcon = GetIcon(IconSize.Large);
                }
                return _largeIcon;
            }
        }

        /// <summary>
        /// Download a GDI compatible image of the skill
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public System.Drawing.Image GetIcon(IconSize size)
        {
            System.IO.Stream s = Client.ReadMedia("d3/icons/skills/"+ (int)size + "/"+this.Icon+".png");
            return System.Drawing.Image.FromStream(s);
        }

        public int Level;
        public string CategorySlug;
        public string TooltipUrl;
        public string Description;
        public string SimpleDescription;
        public string SkillCalcId;
    }
    public class Rune : D3Object
    {
        public string Slug { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string Description { get; set; }
        public string SimpleDescription { get; set; }
        public string TooltipParams { get; set; }
        public string SkillCalcId { get; set; }
        public int Order { get; set; }
    }

    public class FallenHero : Hero
    {
        public Death Death;
    }
}
