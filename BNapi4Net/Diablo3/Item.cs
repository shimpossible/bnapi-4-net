using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net.Diablo3
{

    public class Item : D3Object
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string DisplayColor { get; set; }
        public string TooltipParams { get; set; }

        public override string ToString()
        {
            return "{Item " + this.Name + "}";
        }

        /// <summary>
        /// Refresh the data
        /// </summary>
        public void Refresh()
        {
            Item other = Client.GetItem(this.TooltipParams);
            this.AttacksPerSecond = other.AttacksPerSecond;
            this.Attributes = other.Attributes;
            this.AttributesRaw = other.AttributesRaw;
            this.BonusAffixes = other.BonusAffixes;
            this.DisplayColor = other.DisplayColor;
            this.Dps = other.Dps;
            this.Gems = other.Gems;
            this.Icon = other.Icon;
            this.Id = other.Id;
            this.ItemLevel = other.ItemLevel;
            this.MaxDamage = other.MaxDamage;
            this.MinDamage = other.MinDamage;
            this.Name = other.Name;
            this.RequiredLevel = other.RequiredLevel;
            this.Salvage = other.Salvage;
            this.SocketEffects = other.SocketEffects;
            this.TooltipParams = other.TooltipParams;
            this.Type = other.Type;
            this.TypeName = other.TypeName;            
        }

        public string GetTooltip()
        {
            // http://us.battle.net/d3/en/tooltip/item/CiAIjOnFhAESBwgEFRRVXUQdmrd1NTAJOKACQABQBmD8Ahio1OrFCFACWAI

            System.IO.Stream s = Client.ReadMedia("d3/en/tooltip/" + TooltipParams);
            using (System.IO.StreamReader r = new System.IO.StreamReader(s))
            {
                return r.ReadToEnd();
            }
        }

        System.Drawing.Image _smallIcon = null;        
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
        public System.Drawing.Image LargeIcon
        {
            get
            {
                if (_smallIcon == null)
                {
                    _smallIcon = GetIcon(IconSize.Large);
                }
                return _smallIcon;
            }
        }
        
        System.Windows.Media.ImageSource _smallImage = null;
        System.Windows.Media.ImageSource _largeImage = null;
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
        /// Download a WPF compatible ImageSource of the given size
        /// small is half the size of large
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public System.Windows.Media.ImageSource GetImage(IconSize size)
        {
            // small or large
            string sizeStr = "large";
            if (size == IconSize.Small) sizeStr = "small";

            System.IO.Stream s = Client.ReadMedia("d3/icons/items/" + sizeStr + "/" + this.Icon + ".png");
            System.Windows.Media.Imaging.BitmapImage b = new System.Windows.Media.Imaging.BitmapImage(); ;
            b.BeginInit();
            b.StreamSource = s;
            b.EndInit();

            return b;
        }

        /// <summary>
        /// Download a GDI compatible image of the icon
        /// small is half the size of large
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public System.Drawing.Image GetIcon(IconSize size)
        {
            // small or large
            string sizeStr = "large";
            if (size == IconSize.Small) sizeStr = "small";

            System.IO.Stream s = Client.ReadMedia("d3/icons/items/" + sizeStr + "/" + this.Icon + ".png");
            return System.Drawing.Image.FromStream(s);
        }

        public int RequiredLevel { get; set; }
        public int ItemLevel { get; set; }
        public int BonusAffixes { get; set; }
        public MinMax Dps { get; set; }
        public MinMax AttacksPerSecond { get; set; }
        public MinMax MinDamage { get; set; }
        public MinMax MaxDamage { get; set; }

        public List<SocketEffect> SocketEffects { get; set; }

        public List<Salvage> Salvage { get; set; }

        public string TypeName { get; set; }
        public ItemType Type { get; set; }
        public List<SocketedGem> Gems { get; set; }

        /// <summary>
        /// Numeric Value for attributes
        /// </summary>
        public Dictionary<string, MinMax> AttributesRaw;

        /// <summary>
        /// Text descriptions of attributes
        /// </summary>
        public List<String> Attributes { get; set; }

    }

    public class Salvage
    {
        public double Chance { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
    }

    public class SocketEffect
    {
        public ItemTypeId ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }

        /// <summary>
        /// Numeric Value for attributes
        /// </summary>
        public Dictionary<string, MinMax> AttributesRaw { get; set; }

        /// <summary>
        /// Text descriptions of attributes
        /// </summary>
        public List<String> Attributes { get; set; }
    }

    public class SocketedGem
    {
        public Item Item;

        /// <summary>
        /// Numeric Value for attributes
        /// </summary>
        public Dictionary<string, MinMax> AttributesRaw { get; set; }

        /// <summary>
        /// Text descriptions of attributes
        /// </summary>
        public List<String> Attributes { get; set; }
    }

    public class MinMax
    {
        public double Min { get; set; }
        public double Max { get; set; }
    }

    public class ItemType
    {
        public ItemTypeId Id { get; set; }
        public bool TwoHanded { get; set; }
    }
}
