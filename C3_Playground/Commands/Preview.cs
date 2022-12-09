using C3_Playground.CommandAttributes;
using Cocona;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3_Playground.Commands
{
    internal record PlayerPreviewArgs(
        [Option('a')][FileExists] string Armor,
        [Option('b')][FileExists] string ArmorTexture,
        [Option('h')][FileExists(true)] string? Armet,
        [Option('i')][FileExists(true)] string? ArmetTexture,
        [Option('w')][FileExists(true)] string? LeftWeapon,
        [Option('x')][FileExists(true)] string? LeftWeaponTexture,
        [Option('s')][FileExists(true)] string? RightWeapon,
        [Option('t')][FileExists(true)] string? RightWeaponTexture,
        [Option('m')][FileExists(true)] string? Motion,
        [Option('p')][FileExists(true)] string? Mount,
        [Option('q')][FileExists(true)] string? MountTexture,
        [Option('r')][FileExists(true)] string? MountMotion
        ) : ICommandParameterSet;
    internal class PreviewCommands
    {
        [Command("player")]
        public void Player([Argument] int WindowWidth, [Argument] int WindowHeight, PlayerPreviewArgs args)
        {
            using (var game = new Preview.RenderWindow(new Preview.PreviewArgs()
            {
                Width = WindowWidth,
                Height = WindowHeight,
                Armor = args.Armor,
                ArmorTexture = args.ArmorTexture,
                Armet = args.Armet,
                ArmetTexture = args.ArmetTexture,
                LeftWeapon = args.LeftWeapon,
                LeftWeaponTexture = args.LeftWeaponTexture,
                RightWeapon = args.RightWeapon,
                RightWeaponTexture = args.RightWeaponTexture,
                Motion = args.Motion,
                Mount = args.Mount,
                MountTexture = args.MountTexture,
                MountMotion = args.MountMotion
            }))
                game.Run();
        }
    }
}
