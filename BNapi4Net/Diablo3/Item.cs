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

        System.Windows.Media.ImageSource _smallImage = null;
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

        System.Windows.Media.ImageSource _largeImage = null;
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

        public System.Drawing.Image GetIcon(IconSize size)
        {
            // small or large
            string sizeStr = "large";
            if (size == IconSize.Small) sizeStr = "small";

            System.IO.Stream s = Client.ReadMedia("d3/icons/items/" + sizeStr + "/" + this.Icon + ".png");
            return System.Drawing.Image.FromStream(s);
        }

        public int RequiredLevel;
        public int ItemLevel;
        public int BonusAffixes;
        public MinMax Dps;
        public MinMax AttacksPerSecond;
        public MinMax MinDamage;
        public MinMax MaxDamage;

        public List<SocketEffect> SocketEffects;

        public List<Salvage> Salvage;

        public string TypeName;
        public ItemType Type;
        public List<SocketedGem> Gems;

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
        public double Chance;
        public Item Item;
        public int Quantity;
    }

    public class SocketEffect
    {
        public ItemTypeId ItemTypeId;
        public string ItemTypeName;

        /// <summary>
        /// Numeric Value for attributes
        /// </summary>
        public Dictionary<string, MinMax> AttributesRaw;

        /// <summary>
        /// Text descriptions of attributes
        /// </summary>
        public List<String> Attributes;
    }

    public class SocketedGem
    {
        public Item Item;

        /// <summary>
        /// Numeric Value for attributes
        /// </summary>
        public Dictionary<string, MinMax> AttributesRaw;

        /// <summary>
        /// Text descriptions of attributes
        /// </summary>
        public List<String> Attributes;
    }

    public class MinMax
    {
        public double Min;
        public double Max;
    }

    public class ItemType
    {
        public ItemTypeId Id; // TODO: make this an ENUM
        public bool TwoHanded;
    }
}
