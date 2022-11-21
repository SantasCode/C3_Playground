using C3.IniFiles.FileSet;

namespace C3.IniFiles.Loaders
{
    internal static class MonsterInfoLoader
    {
        internal static Dictionary<uint, MonsterInfo> Load(TextReader tr)
        {
            var dict = new Dictionary<uint, MonsterInfo>();
            
            string headers = tr.ReadLine() ?? "";
            
            while (tr.Peek() != -1)
            {
                string line = tr.ReadLine() ?? "";

                string[] parts = line.Split(",");

                if(parts.Length != 4)
                {
                    Console.WriteLine($"Invalid Monster Info entry - {line}");
                    continue;
                }

                MonsterInfo monsterInfo = new MonsterInfo()
                {
                    Id = uint.Parse(parts[0]),
                    Name = parts[1],
                    Type = uint.Parse(parts[2]),
                    LookFace = uint.Parse(parts[3])
                };
                dict.Add(monsterInfo.Id, monsterInfo);
            }

            return dict;
        }
    }
}
