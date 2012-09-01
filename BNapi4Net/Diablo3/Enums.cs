using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net.Diablo3
{

    public enum Gender
    {
        Male = 0,
        Female = 1
    }

    /// <summary>
    /// Found at http://us.battle.net/api/d3/data/follower/
    /// </summary>
    public enum FollowerType
    {
        Enchantress,
        Templar,
        Scoundrel,
    }

    /// <summary>
    /// Found at http://us.battle.net/api/d3/data/artisan/
    /// </summary>
    public enum ArtisanType
    {
        Blacksmith,
        Jeweler,
    }

    /// <summary>
    /// List of ItemTypes, the list is indented in the code to show
    /// parent/child structure,
    /// Each BYTE in the value represents the tree strucure, this means
    /// we can only have a depth of 8 and 256 items per level
    /// </summary>
    [Flags]
    public enum ItemTypeId : ulong
    {
        All      =0,
          Weapon = 0x0100000000000000 ,
            Melee = 0x0101000000000000 ,
              Swing = 0x0101010000000000 ,
                GenericSwingWeapon = 0x0101010100000000 ,
                  Axe = 0x0101010101000000 ,
                  Axe2H = 0x0101010102000000 ,
                  Sword = 0x0101010103000000 ,
                  Sword2H = 0x0101010104000000 ,
                  Mace = 0x0101010105000000 ,
                  Mace2H = 0x0101010106000000 ,
                  Staff = 0x0101010107000000 ,
                FistWeapon = 0x0101010200000000 ,
                CombatStaff = 0x0101010300000000 ,
                MightyWeapon1H = 0x0101010400000000 ,
                MightyWeapon2H = 0x0101010500000000 ,
              Thrust = 0x0101020000000000 ,
                GenericThrustWeapon = 0x0101020100000000 ,
                  Dagger = 0x0101020101000000 ,
                  Polearm = 0x0101020102000000 ,
                  Spear = 0x0101020103000000 ,
                CeremonialDagger = 0x0101020200000000 ,
            Ranged = 0x0102000000000000 ,
              BowClass = 0x0102010000000000 ,
                GenericBowWeapon = 0x0102010100000000 ,
                  Bow = 0x0102010101000000 ,
                  Crossbow = 0x0102010102000000 ,
                HandXbow = 0x0102010200000000 ,
              GenericRangedWeapon = 0x0102020000000000 ,
              Wand = 0x0102030000000000 ,
          Armor = 0x0200000000000000 ,
            Helm = 0x0201000000000000 ,
              GenericHelm = 0x0201010000000000 ,
              SpiritStone_Monk = 0x0201020000000000 ,
              WizardHat = 0x0201030000000000 ,
              VoodooMask = 0x0201040000000000 ,
            Gloves = 0x0202000000000000 ,
            Boots = 0x0203000000000000 ,
            Shoulders = 0x0204000000000000 ,
            ChestArmor = 0x0205000000000000 ,
              GenericChestArmor = 0x0205010000000000 ,
              Cloak = 0x0205020000000000 ,
            Belt = 0x0206000000000000 ,
              GenericBelt = 0x0206010000000000 ,
              Belt_Barbarian = 0x0206020000000000 ,
            Legs = 0x0207000000000000 ,
            Bracers = 0x0208000000000000 ,
          Offhand = 0x0300000000000000 ,
            GenericOffHand = 0x0301000000000000 ,
              OffhandOther = 0x0301010000000000 ,
              Shield = 0x0301020000000000 ,
            Orb = 0x0302000000000000 ,
            Mojo = 0x0303000000000000 ,
            Quiver = 0x0304000000000000 ,
          Jewelry = 0x0400000000000000 ,
            Ring = 0x0401000000000000 ,
            Amulet = 0x0402000000000000 ,
            FollowerSpecial = 0x0403000000000000 ,
              TemplarSpecial = 0x0403010000000000 ,
              ScoundrelSpecial = 0x0403020000000000 ,
              EnchantressSpecial = 0x0403030000000000 ,
          Socketable = 0x0500000000000000 ,
            Gem = 0x0501000000000000 ,
          SpellRune = 0x0600000000000000 ,
            Runestone_Unattuned = 0x0601000000000000 ,
            Runestone_A = 0x0602000000000000 ,
            Runestone_B = 0x0603000000000000 ,
            Runestone_C = 0x0604000000000000 ,
            Runestone_D = 0x0605000000000000 ,
            Runestone_E = 0x0606000000000000 ,
          CraftingReagent = 0x0700000000000000 ,
          Utility = 0x0800000000000000 ,
            ChaosShard = 0x0801000000000000 ,
            GeneralUtility = 0x0802000000000000 ,
            Scroll = 0x0803000000000000 ,
              ScrollIdentify = 0x0803010000000000 ,
              ScrollCalling = 0x0803020000000000 ,
              ScrollGreed = 0x0803030000000000 ,
              ScrollCompanion = 0x0803040000000000 ,
              ScrollGirththing = 0x0803050000000000 ,
              ScrollReforgeA = 0x0803060000000000 ,
              ScrollReforgeB = 0x0803070000000000 ,
              ScrollReforgeC = 0x0803080000000000 ,
            Potion = 0x0804000000000000 ,
              HealthPotion = 0x0804010000000000 ,
              PowerPotion = 0x0804020000000000 ,
            Dye = 0x0805000000000000 ,
            KnowledgeUtility = 0x0806000000000000 ,
              PageOfTraining = 0x0806010000000000 ,
              TrainingTome = 0x0806020000000000 ,
            NephalemCube = 0x0807000000000000 ,
            TalismanUnlock = 0x0808000000000000 ,
            StoneOfWealth = 0x0809000000000000 ,
            StoneOfRecall = 0x080A000000000000 ,
          CraftingPlan = 0x0900000000000000 ,
            CraftingPlanGeneric  = 0x0901000000000000 ,
            CraftingPlanLegendary  = 0x0902000000000000 ,
            CraftingPlan_Smith = 0x0903000000000000 ,
            CraftingPlan_Mystic = 0x0904000000000000 ,
            CraftingPlan_Jeweler = 0x0905000000000000 ,
            CraftingPlanLegendary_Smith = 0x0906000000000000 ,
          Glyph = 0x0A00000000000000 ,
            HealthGlyph = 0x0A01000000000000 ,
          Quest = 0x0B00000000000000 ,
            QuestUsable = 0x0B01000000000000 ,
          Gold = 0x0C00000000000000 ,
          Junk = 0x0D00000000000000 ,
          Book = 0x0E00000000000000 ,
          Ornament = 0x0F00000000000000 ,
          Calldown = 0x1000000000000000 ,
    }

    public static class ItemTypeIdExtention
    {
        /// <summary>
        /// Returns true of enum is a CHILD of the parent
        /// </summary>
        /// <param name="self">enum value</param>
        /// <param name="parent">Parent value to compare against</param>
        /// <returns></returns>
        public static bool IsChildOf(this ItemTypeId self, ItemTypeId parent)
        {
            if (self == ItemTypeId.All) return false;  // child of none
            //if (parent == ItemTypeId.All) return true; // parent of everything

            ulong pkey = (ulong)parent;

            // find first 00 in parent value
            ulong mask = 0;
            int i=0;
            for (i = 0; i < 8; i++)
            {                
                if( (pkey & (0xFF00000000000000>>(i*8))) == 0) break;
                mask |= 0xFF00000000000000 >> (i * 8);
            }


            // compare parent value with matching section of self value
            return (mask & (ulong)self) == pkey;
        }
    }
}
