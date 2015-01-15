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

        public static Spell Q;
				public static Spell W;
        public static Spell E;
        public static Spell R;
        public float lastE = 0f;

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
            if (Player.BaseSkinName != ChampionName) return;

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
					if (SpellName == "DariusExecute")
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

					return 0;
				}

				public static bool WillHit(Vector3 from, Vector3 point, Vector3 castPosition, SkillshotType Type, float Width, float Range, int extraWidth = 0)
				{
					switch (Type)
					{
						case SkillshotType.SkillshotCircle:
							if (point.To2D().Distance(castPosition) < Width)
							{
								return true;
							}
							break;

						case SkillshotType.SkillshotLine:
							if (point.To2D().Distance(castPosition.To2D(), from.To2D(), true) < Width + extraWidth)
							{
								return true;
							}
							break;
						case SkillshotType.SkillshotCone:
							var edge1 = (castPosition.To2D() - from.To2D()).Rotated(-Width / 2);
							var edge2 = edge1.Rotated(Width);
							var v = point.To2D() - from.To2D();
							if (point.To2D().Distance(from) < Range && edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0)
							{
								return true;
							}
							break;
						default:
							return true;
					}

					return false;
				}

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs attack)
        {

					if (unit.IsValid<Obj_AI_Hero>() && unit.IsEnemy && attack.Target.IsMe && W.IsReady())
					{

						dmg = unit.GetSpellDamage(ObjectManager.Player, attack.SData.Name);
						double HpLeft = ObjectManager.Player.Health - dmg;
						double HpPercentage = (dmg * 100) / ObjectManager.Player.MaxHealth;

						if (HpLeft <= 0 )
						{
							W.Cast();
						}else if (HpPercentage >= 30)
						{
							W.Cast();
						}
					}

					if (ShouldUseW(attack.SData.Name) == 1 && W.IsReady())
					{
						W.Cast();
					}

					if ((attack.SData.SpellTotalTime < 0 || attack.SData.LineWidth > 0) && attack.Target.GetType().Name != "Obj_AI_Hero")
					{
						if (attack.SData.MissileSpeed == 0 && attack.SData.LineWidth == 0)
						{
							PredictedType = SkillshotType.SkillshotCone;
						}
						else if (attack.SData.LineWidth == 0)
						{
							PredictedType = SkillshotType.SkillshotCircle;
						}
						else if (attack.SData.LineWidth > 0 && attack.SData.MissileSpeed > 0)
						{
							PredictedType = SkillshotType.SkillshotLine;
						}
					}

					if (WillHit(attack.Start, ObjectManager.Player.ServerPosition, attack.End, PredictedType, attack.SData.LineWidth, attack.SData.CastRange[0]) && W.IsReady())
					{
						W.Cast();
					}	
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
							Combo();
            };
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
							Harass();
            };

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
                if (Player.Distance(target.ServerPosition) <= R.Range + R.Width && R.IsReady())
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