using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


namespace PotionMenager
{
  class PotionMenager
  {
		private Menu menu;
		private MenuItem hpborder;
		private MenuItem manaborder;
		private MenuItem serafborder;
		private double hpborder_value;
		private double manaborder_value;
		private double dmg;
		private double recivedmg;
		private Spell Barrier;
		private Spell Heal;

		public static InventorySlot HealthPotion;

		private string debug_hp;
		private string debug_mana;
		private string debug_dmg;

		private static Items.Item hpPotion = new Items.Item(2003, 0);
		private static Items.Item manaPotion = new Items.Item(2004, 0);
		private static Items.Item Flask = new Items.Item(2041, 0);
		private static Items.Item Zhonya = new Items.Item(3157, 0);
		private static Items.Item Seraf = new Items.Item(3040, 0);


		public PotionMenager()
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
			Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
      Game.OnGameUpdate += Game_OnGameUpdate;
    }



		void Game_OnGameLoad(EventArgs args)
		{
			menu = new Menu("Potion Menager", "PotionMenager", true);
			hpborder = new MenuItem("hpBorder", "Use HP Potion when below %").SetValue(new Slider(50, 0, 100));
			manaborder = new MenuItem("manaBorder", "Use Mana Potion when below %").SetValue(new Slider(30, 0, 100));
			serafborder = new MenuItem("serafBorder", "Use Seraf when HP below %").SetValue(new Slider(15, 0, 100));
			menu.AddItem(hpborder);
			menu.AddItem(manaborder);
			menu.AddItem(serafborder);

			menu.AddToMainMenu();

			if (Utility.Map.GetMap().Name == "CrystalScar" || Utility.Map.GetMap().Name == "TwistedTreeline")
			{
				Zhonya = new Items.Item(3090, 0);
			}

			Game.PrintChat("Potion Menager loaded");
		}


		void Game_OnGameUpdate(EventArgs args)
		{

			double HpPercentage = ObjectManager.Player.HealthPercentage();
			double manaPercentage = ObjectManager.Player.ManaPercentage();

			debug_hp = HpPercentage.ToString();
			debug_mana = manaPercentage.ToString();
			//Game.PrintChat(debug_hp);

			//https://privatepaste.com/59e7aebdf2
			//https://github.com/h3h3/BehaviorSharp/tree/master/BehaviorSharp
			//HealthPotion = ObjectManager.Player.InventoryItems.FirstOrDefault(i => i.Id == ItemId.Health_Potion);

			if (Utility.CountEnemysInRange(1000) > 0 )
			{
				if (HpPercentage <= hpborder.GetValue<Slider>().Value)
					{
							if (hpPotion.IsReady() && !ObjectManager.Player.HasBuff("RegenerationPotion", true))
									hpPotion.Cast();
							else if (Flask.IsReady() && !ObjectManager.Player.HasBuff("ItemCrystalFlask", true))
									Flask.Cast();
					}
			}

			if (manaPercentage <= manaborder.GetValue<Slider>().Value)
			{
				//Check mana buff
				if (manaPotion.IsReady() && !ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true))
					manaPotion.Cast();
			}
		}

		void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
    {
			Barrier = new Spell("SummonerBarrier");
			Heal = new Spell("SummonerHeal");
			if (sender.IsValid<Obj_AI_Hero>() && sender.IsEnemy && args.Target.IsMe )//&& !args.SData.IsAutoAttack()
			{
				var spell = sender.GetDamageSpell(ObjectManager.Player, args.SData.Name);

				spell.
				dmg = sender.GetSpellDamage(ObjectManager.Player, args.SData.Name);
				Game.PrintChat(args.SData.Name.ToString());
				//recivedmg = sender.CalcDamage(ObjectManager.Player, Damage.DamageType.Magical, dmg);
				//double HpLeft = ObjectManager.Player.Health - CastedSpellsDamage();

				double HpLeft = ObjectManager.Player.Health - dmg;
				double HpPercentage = (HpLeft / ObjectManager.Player.MaxHealth)*100;

				if (HpPercentage <= hpborder.GetValue<Slider>().Value)
				{
					if (hpPotion.IsReady() && !ObjectManager.Player.HasBuff("RegenerationPotion", true))
						hpPotion.Cast();
				}

				if ((ObjectManager.Player.Health - dmg) <= 10)
				{
					Zhonya.Cast();
				}else if (HpPercentage < serafborder.GetValue<Slider>().Value)
				{
					Seraf.Cast();
				}
			}
    }

		internal class Spell
		{
			private string name;
			private SpellSlot slot;

			public Spell(string name)
			{
				this.name = name;
				slot = Utility.GetSpellSlot(ObjectManager.Player, name);
			}

			public bool IsReady()
			{

				return ObjectManager.Player.Spellbook.CanUseSpell(slot) == SpellState.Ready;
				//return ObjectManager.Player.SummonerSpellbook.CanUseSpell(slot) == SpellState.Ready;
			}

			public bool Cast()
			{
				return ObjectManager.Player.Spellbook.CastSpell(slot, ObjectManager.Player);
			}
		}
			//Fajne do sprawdzania buffów
			//foreach (var buff in ObjectManager.Player.Buffs)
			//{
				//Game.PrintChat("Name:{0} Display:{1}", buff.Name, buff.DisplayName);
				//Console.WriteLine("Name:{0} Display:{1}", buff.Name, buff.DisplayName);
			//}

	}
}
