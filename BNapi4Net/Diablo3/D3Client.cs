using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BNapi4Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BNapi4Net.Diablo3
{
    public class D3Client : BaseClient
    {
        /*
         * http://us.media.blizzard.com/
         * d3/icons/items/large/<URL>.png
         * d3/icons/items/small/<URL>.png
         * d3/icons/skills/64/barbarian_rend.png
         * d3/icons/skills/42/barbarian_rend.png
         * d3/icons/skills/21/barbarian_rend.png
         */
        D3Converter converter;

        public D3Client(Region r = Region.US)
            : base(r, System.Globalization.CultureInfo.CurrentCulture)
        {
            converter = new D3Converter(this);
        }
                /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="r">Region</param>
        /// <param name="loc">Current Culture</param>
        /// <param name="publicKey">public key for authentication</param>
        /// <param name="privateKey">private key for authentication</param>
        /// <param name="cache">Use a </param>
        public D3Client(Region r,
            System.Globalization.CultureInfo loc,
            string publicKey = null,
            string privateKey = null,
            bool cache = true)
            : base(r, loc, publicKey, privateKey, cache)
        {
            converter = new D3Converter(this);
        }

        public Profile GetProfile(string battleTag)
        {
            using (Stream s = base.ReadData("d3/profile/" + battleTag + "/?"+ Locale.ToString() ))
            {
                string json = new StreamReader(s).ReadToEnd();
                return JsonConvert.DeserializeObject<Profile>(json, converter);
            }
        }

        public Hero GetHero(string battleTag, int id)
        {
            using (Stream s = base.ReadData("d3/profile/" + battleTag + "/hero/" + id + "?" + Locale.ToString() ))
            {
                string json = new StreamReader(s).ReadToEnd();
                Hero h = JsonConvert.DeserializeObject<Hero>(json, converter);
                h.battleTag = battleTag.Replace('#', '-');
                return h;
            }
        }

        public Item GetItem(string itemData)
        {
            if (itemData.StartsWith("item/"))
            {
                itemData = itemData.Substring(5);
            }

            using (Stream s = base.ReadData("d3/data/item/"+ itemData + "?"+ Locale.ToString() ))
            {
                string json = new StreamReader(s).ReadToEnd();
                return JsonConvert.DeserializeObject<Item>(json, converter);
            }
        }

        public Follower GetFollower(FollowerType type)
        {
            string follower_type = "";
            switch (type)
            {
                case FollowerType.Enchantress:
                    follower_type = "enchantress";
                    break;
                case FollowerType.Scoundrel:
                    follower_type = "scoundrel";
                    break;
                case FollowerType.Templar:
                    follower_type = "templar";
                    break;
            }
            using (Stream s = base.ReadData("d3/data/follower/" + follower_type + "?" + Locale.ToString()))
            {
                string json = new StreamReader(s).ReadToEnd();
                return JsonConvert.DeserializeObject<Follower>(json, converter);
            }

        }

        public Artisan GetArtisan(ArtisanType type)
        {
            
            string art_type = "";
            switch (type)
            {
                case  ArtisanType.Blacksmith:
                    art_type = "blacksmith";
                    break;
                case ArtisanType.Jeweler:
                    art_type = "jeweler";
                    break;                
            }

            using (Stream s = base.ReadData("d3/data/artisan/" + art_type + "?" + Locale.ToString() ))
            {
                string json = new StreamReader(s).ReadToEnd();
                return JsonConvert.DeserializeObject<Artisan>(json, converter);
            }

        }
    }

}
