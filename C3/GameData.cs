using C3.IniFiles.Entities;
using C3.IniFiles.FileSet;
using System.Net;
using System.Security.Cryptography;
using System.Threading;

namespace C3
{
    public class GameData
    {
        private readonly FileSet _fileSet;
        public GameData(string clientDirectory)
        {
            _fileSet = new FileSet(clientDirectory);

        }
        public List<Item> GetHair()
        {
            List<Item> list = new List<Item>();
            foreach(var item in _fileSet.Armet.Where(p=> (p.Key / 1000) % 1000 == 119))
            {
                foreach (var subPart in item.Value.Parts)
                {
                    Item hairItem = new()
                    {
                        Name = item.Key.ToString(),
                        Type = ItemType.Hair
                    };
                    if (TryGetObjAndTexture(subPart.MeshId, subPart.TextureId, out var val))
                        hairItem.BaseModel.Add(0, val);

                    list.Add(hairItem);
                }
            }
            return list;
        }
        public List<Item> GetItems()
        {
            List<Item> items = new ();

            foreach (var itemType in _fileSet.Itemtype)
            {
                ItemType type = (itemType.Value / 1_000) switch
                {
                    >= 111 and <= 116 => ItemType.Helmet,
                    >= 130 and <= 137 => ItemType.Armor,
                    >= 181 and <= 191 => ItemType.Armor,
                    >= 410 and <= 490 => ItemType.OneHander,
                    500 => ItemType.Bow,
                    >= 501 and <= 580 => ItemType.TwoHander,
                    601 => ItemType.OneHander,
                    900 => ItemType.Shield,
                    _ => ItemType.Other
                };

                if (type == ItemType.Other) continue;

                Item item = new()
                {
                    Name = itemType.Key,
                    Type = type
                };

                var dataSet = item.Type switch
                {
                    ItemType.OneHander or 
                    ItemType.TwoHander or 
                    ItemType.Shield => _fileSet.Weapon,
                    ItemType.Armor => _fileSet.Armor,
                    ItemType.Helmet => _fileSet.Armet,
                    _ => new()
                };

                RolePartInfo? part = null;
                switch (item.Type)
                {
                    case ItemType.OneHander:
                    case ItemType.TwoHander:
                    case ItemType.Shield:
                        if (dataSet.TryGetValue(itemType.Value, out part))
                        {
                            foreach (var subPart in part.Parts)
                            {
                                if (TryGetObjAndTexture(subPart.MeshId, subPart.TextureId, out var val))
                                    item.BaseModel.Add(0, val);
                            }
                        }
                        else
                            Console.WriteLine($"[GameData](GetItems) Failed to find weapon info: {itemType.Value}");
                        break;
                    case ItemType.Armor:
                    case ItemType.Helmet:
                        for (uint i = 1; i <= 4; i++)
                        {
                            uint itemId = i * 1_000_000 + itemType.Value;
                            if (dataSet.TryGetValue(itemId, out part))
                            {
                                foreach (var subPart in part.Parts)
                                {
                                    if (TryGetObjAndTexture(subPart.MeshId, subPart.TextureId, out var val))
                                        item.BaseModel.Add(i, val);
                                }
                            }
                            else
                                Console.WriteLine($"[GameData](GetItems) Failed to find armor/armet info: {itemType.Value} for body: {i}");
                        }
                        break;
                }

                items.Add(item);
            }
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
                    if(TryGetObjAndTexture(part.MeshId, part.TextureId, out var val))
                    {
                        monster.BaseModel.Add(val);
                    }
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
        public List<string> GetArmorC3()
        {
            List<string> result = new();
            var playerArmors = _fileSet.Armor.Where(p => p.Key / 1_000_000 <= 4).ToList();
            foreach (var armor in playerArmors)
            {
                foreach(var part in armor.Value.Parts)
                {
                    if (TryGetObjAndTexture(part.MeshId, part.TextureId, out var val))
                    {
                        result.Add(val.Item1);
                    }
                }
            }

            return result;
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
                        if (TryGetObjAndTexture(part.MeshId, part.TextureId, out var val))
                            npc.BaseModel.Add(val);
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
        private bool TryGetObjAndTexture(ulong MeshId, ulong TextureId, out (string, string) value)
        {
            if (_fileSet._3DObjs.TryGetValue(MeshId, out string MeshPath))
            {
                if (_fileSet._3DTextures.TryGetValue(TextureId, out string TexturePath))
                {
                    value = (MeshPath, TexturePath);
                    return true;
                }
                else
                    Console.WriteLine($"[GameData] Failed to find Texture {TextureId}");
            }
            else
                Console.WriteLine($"[GameData] Failed to find Mesh {MeshId}");
            value = ("", "");
            return false;
        }
    }
}
