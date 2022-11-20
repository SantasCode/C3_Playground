using C3.IniFiles.Entities;
using C3.IniFiles.FileSet;
using System.Net;

namespace C3
{
    public class GameData
    {
        private readonly FileSet _fileSet;
        public GameData(FileSet fileSet)
        {
            _fileSet = fileSet;


        }
        public List<Item> GetItems()
        {
            List<Item> items = new ();
            //One-Hand
            var oneHands = _fileSet.Weapon.Where(p=> p.)
            //Two-Hand

            //Shield

            return items;
        }
        public List<Monster> GetMonsters()
        {
            List<Monster> monsters = new();

            var monstersDefined = _fileSet.Armor.Where(p => p.Key / 1_000_000 >= 100);

            foreach (var armor in monstersDefined)
            {
                ulong MonsterTypeId = armor.Key / 1_000_000;

                if (MonsterTypeId < 100) continue;

                Monster monster = new();

                monster.Name = MonsterTypeId.ToString();

                foreach (var part in armor.Value.Parts)
                {
                    if (_fileSet._3DObjs.TryGetValue(part.MeshId, out string MeshPath) && _fileSet._3DTextures.TryGetValue(part.TextureId, out string TexturePath))
                    {
                        monster.BaseModel.Add((MeshPath, TexturePath));
                    }
                    else
                        Console.WriteLine($"[GameData](GetNpcs) Failed to find Monster mesh or texture {monster.Name} - ({part.MeshId},{part.TextureId})")
                }

                //Get Monster Motions.

                var relevantMotions = _fileSet._3DMotions.Where(p => p.Key / 1_000_000 == MonsterTypeId);

                foreach(var motion in relevantMotions)
                {
                    monster.Motion.Add((motion.Key % 1000).ToString(), motion.Value);
                }

            }

            return monsters;
        }
        public List<NPC> GetNpcs()
        {
            List<NPC> npcs = new();

            foreach (var obj in _fileSet.Npcs.Values)
            {
                NPC npc = new();

                //Get base model info from 3dsimpleobj ID
                if (_fileSet._3DSimpleObj.TryGetValue(obj.SimpleObjId, out var partInfo)) 
                {
                    foreach (var part in partInfo.Parts) 
                    {
                        if (_fileSet._3DObjs.TryGetValue(part.MeshId, out string MeshPath) && _fileSet._3DTextures.TryGetValue(part.TextureId, out string TexturePath))
                        {
                            npc.BaseModel.Add((MeshPath, TexturePath));
                        }
                        else
                            Console.WriteLine($"[GameData](GetNpcs) Failed to find NPC mesh or texture {obj.Name} - ({part.MeshId},{part.TextureId})");
                    }
                }
                else
                    Console.WriteLine($"[GameData](GetNpcs) Failed to find NPC SimpleObjID in _3DSimpleObj {obj.SimpleObjId}");

                //Get Motions of model.
                if (_fileSet._3DMotions.TryGetValue(obj.StandyByMotionId, out string standbyPath ))
                {
                    npc.Motion.Add("Standby", standbyPath);
                }
                else
                    Console.WriteLine($"[GameData](GetNpcs) Failed to find NPC motion in _3DMotion {obj.StandyByMotionId}");

                if (_fileSet._3DMotions.TryGetValue(obj.BlazeByMotionId, out string blazePath))
                {
                    npc.Motion.Add("Standby", blazePath);
                }
                else
                    Console.WriteLine($"[GameData](GetNpcs) Failed to find NPC motion in _3DMotion {obj.BlazeByMotionId}");

                if (_fileSet._3DMotions.TryGetValue(obj.RestByMotionId, out string restPath))
                {
                    npc.Motion.Add("Standby", restPath);
                }
                else
                    Console.WriteLine($"[GameData](GetNpcs) Failed to find NPC motion in _3DMotion {obj.RestByMotionId}");

                //TODO: GET EFFECT
            }
            return npcs;
        }
    }
}
