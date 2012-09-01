using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BNapi4Net.Diablo3
{

    public class D3Converter : Newtonsoft.Json.JsonConverter
    {
        D3Client client;
        public D3Converter(D3Client c)
        {
            client = c;
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType.FullName.StartsWith("BNapi4Net") && !objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jobj = JObject.Load(reader);

            var target = Activator.CreateInstance(objectType);

            D3Object d3obj = target as D3Object;
            if (d3obj != null)
            {
                d3obj.Client = client;
            }
            serializer.Populate(jobj.CreateReader(), target);

            Profile prof = target as Profile;
            if (prof != null)
            {
                // add battle tag heros
                foreach (Hero h in prof.Heroes)
                {
                    h.battleTag = prof.BattleTag.Replace('#', '-');
                }

                foreach (FallenHero h in prof.FallenHeroes)
                {
                    h.battleTag = prof.BattleTag.Replace('#', '-');
                }
            }

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
