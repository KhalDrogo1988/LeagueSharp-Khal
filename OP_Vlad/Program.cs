#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
//using BuffLib;
#endregion

namespace Vladimir
{
    public class Program
    {
        public const string ChampionName = "Vladimir";
        //Spells
        public static List<Spell> SpellList = new List<Spell>();

				public static SkillshotType PredictedType;
				public static Orbwalking.Orbwalker Orbwalker;
				public static bool Farm = false;


        public static Spell Q;
				public static Spell W;
        public static Spell E;
        public static Spell R;
        public float lastE = 0f;

				public static  int[] levels = new int[] { 1,2,3,1,1,4,1,1,3,3,3,4,3,2,2,2,2,4 };

				public static double dmg;
        //Menu
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Console.WriteLine("Vlad LOADED");
        }


        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            //if (Player.BaseSkinName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 600);
						W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 610);
            R = new Spell(SpellSlot.R, 700);

            SpellList.Add(Q);
						SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

						W.SetSkillshot(0.25f, 175, 300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 175, 700, false, SkillshotType.SkillshotCircle);

            //Create the menu
            Config = new Menu(ChampionName, ChampionName, true);

						Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
						//Load the orbwalker and add it to the submenu.
						Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc")
                .AddItem(
                    new MenuItem("StackE", "StackE (toggle)!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Toggle)));
 
            Config.AddToMainMenu();

            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }


				private static double ShouldUseW(string SpellName)
				{
					if (SpellName == "ViR")
						return 1;
					if (SpellName == "FallenOne")
						return 1;
					if (SpellName == "SyndraR")
						return 1;
					if (SpellName == "DariusExeGrabCone") //Hak Dariusa
						return 1;
					if (SpellName == "DariusExecute") //Ult Dariusa
						return 1;
					if (SpellName == "Feast")
						return 1;
					if (SpellName == "Crowstorm")
						return 1;
					if (SpellName == "RivenMartyr")
						return 1;
					if (SpellName == "KatarinaR")
						return 1;
					if (SpellName == "zedult")
						return 1;
					if (SpellName == "ZyraBrambleZone")
						return 1;
					if (SpellName == "MonkeyKingSpinToWin")
						return 1;
					if (SpellName == "GragasExplosiveCask")
						return 1;
					if (SpellName == "BrandWildfire")
						return 1;
					if (SpellName == "UFSlash")
						return 1;
					if (SpellName == "VeigarPrimordialBurst")
						return 1;
					if (SpellName == "AlZaharNetherGrasp")
						return 1;
					if (SpellName == "GarenJustice")
						return 1;
					if (SpellName == "GalioIdolOfDurand")
						return 1;
					if (SpellName == "InfernalGuardian")
						return 1;
					if (SpellName == "FizzMarinerDoom")
						return 1;
					if (SpellName == "SonaCrescendo")
						return 1;
					if (SpellName == "GarenQ")
						return 1;
					if (SpellName == "GarenR")
						return 1;
					if (SpellName == "PantheonW")
						return 1;
					if (SpellName == "JaxCounterStrike")
					{
						//To do Odczekac 0.5 sec i wtedy zkastowac w bo jaxa e i vlad w maja po 2 sec ale jax mozezkastowac E troszeczke pozniej :)
						return 1;
					}
					if (SpellName == "IreliaEquilibriumStrike")
					{
						//To Do Jeœli moje hp % wieksze od % HP ireli 
						return 1; 
					}
						

					return 0;
				}

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs attack)
        {	
					//if(!attack.SData.IsAutoAttack())
					//Game.PrintChat(attack.SData.Name.ToString());

					if (unit.IsValid<Obj_AI_Hero>() && unit.IsEnemy && attack.Target.IsMe && W.IsReady())
					{

						dmg = unit.GetSpellDamage(ObjectManager.Player, attack.SData.Name);
						double HpLeft = ObjectManager.Player.Health - dmg;
						double HpPercentage = (dmg * 100) / ObjectManager.Player.MaxHealth;
						//Game.PrintChat(dmg.ToString());
						//Game.PrintChat(HpLeft.ToString());
						//Game.PrintChat(HpPercentage.ToString());
						//Game.PrintChat(attack.SData.Name.ToString());
						W.Cast();

						if (ShouldUseW(attack.SData.Name) == 1)
						{
							W.Cast();
						}

						if (HpLeft <= 0 )
						{
							W.Cast();
						}else if (HpPercentage >= 30)
						{
							W.Cast();
						}
					}
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
						int qL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level;
						int wL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level;
						int eL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level;
						int rL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level;

						if (qL + wL + eL + rL < ObjectManager.Player.Level)
						{
							int[] level = new int[] { 0, 0, 0, 0 };
							for (int i = 0; i < ObjectManager.Player.Level; i++)
								level[levels[i] - 1] = level[levels[i] - 1] + 1;

							if (qL < level[0]) ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.Q);
							if (wL < level[1]) ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.W);
							if (eL < level[2]) ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.E);
							if (rL < level[3]) ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.R);
						}

						//Game.PrintChat(Game.Time.ToString());
						var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);


						if (Orbwalker.ActiveMode.ToString() == "LaneClear")
							Farm = true;
						else
							Farm = false;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
							Combo();
						}else{
							var target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Magical);

							if (Q.IsReady() || E.IsReady() && target == null)
								{
										if (Farm)
										{
												foreach (var minion in allMinions)
												{
														Q.CastOnUnit(minion);
														E.CastOnUnit(minion);
												}
											}
								}
							else
							{
								Combo();
							}
						}

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
							Harass();
            }

        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                if (Player.Distance(target.ServerPosition) <= Q.Range && Q.IsReady())
                    Q.Cast(target);
                if (Player.Distance(target.ServerPosition) <= E.Range && E.IsReady())
                    E.Cast();
								if (Player.Distance(target.ServerPosition) <= R.Range && R.IsReady() && target.CountEnemiesInRange(350f) > 2)
                    R.Cast(target, true, true);
								if (Player.Distance(target.ServerPosition) <= R.Range && R.IsReady() && Player.GetSpellDamage(target, SpellSlot.R) > target.Health)
									R.Cast(target, true, true);
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                if (Player.Distance(target.ServerPosition) <= Q.Range && Q.IsReady())
                    Q.Cast(target, false);
                if (Player.Distance(target.ServerPosition) <= E.Range && E.IsReady())
                    E.Cast();
            }
        }
    }
}