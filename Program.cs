using System.Text.Json;
using Ps2IsoTools.UDF;

string[] argz = Environment.GetCommandLineArgs();

void PrintHelp()
{
    Console.WriteLine(@"Usage:
Unpack: ps2iso unpack <path to .iso>
Pack: ps2iso pack <path to METADATA.json>");
    return;
}

if (argz.Length < 3 || argz.Length > 3)
{
    PrintHelp();
    return;
}

switch (argz[1])
{
    case "unpack":
        string isoPath = argz[2];

        if (!File.Exists(isoPath))
        {
            Console.WriteLine("ISO not found");
            return;
        }

        using (var reader = new UdfReader(isoPath))
        {
            List<string> entries = reader.GetAllFileFullNames();
            int entryTotal = entries.Count;

            string isoFileName = Path.GetFileName(isoPath);
            string parentIsoPath = Path.GetDirectoryName(isoPath)!;
            string unpackPath = Path.Combine(parentIsoPath, $"UNPACK_{isoFileName}");
            string filesPath = Path.Combine(unpackPath, "FILES");

            Directory.CreateDirectory(filesPath);

            Metadata metadata = new Metadata
            {
                VolumeLabel = reader.VolumeLabel,
                Entries = new List<string>()
            };

            for (int i = 0; i < entryTotal; i++)
            {
                string entry = entries[i];
                string normalizeEntryPath =
                    Path.DirectorySeparatorChar.ToString() == @"\"
                        ? entry
                        : entry.Replace(@"\", Path.DirectorySeparatorChar.ToString());

                string parentEntryPath = Path.GetDirectoryName(normalizeEntryPath)!;
                if (parentEntryPath.Length != 0)
                {
                    Directory.CreateDirectory(Path.Combine(filesPath, parentEntryPath));
                }

                metadata.Entries.Add(normalizeEntryPath);

                Console.WriteLine($"{i + 1}/{entryTotal}({normalizeEntryPath}): start");
                reader.CopyFile(reader.GetFileByName(entry)!,
                                Path.Combine(filesPath, normalizeEntryPath));
                Console.WriteLine($"{i + 1}/{entryTotal}({normalizeEntryPath}): done");
            }

            File.WriteAllText(
                Path.Combine(unpackPath, "METADATA.json"),
                JsonSerializer.Serialize(
                    metadata, new JsonSerializerOptions { WriteIndented = true }));
        }

        break;
    case "pack":
        string metadataPath = argz[2];

        if (!File.Exists(metadataPath))
        {
            Console.WriteLine("METADATA.json not found");
            return;
        }

        string parentMetadataPath = Path.GetDirectoryName(metadataPath)!;

        string metadataJson = File.ReadAllText(metadataPath);
        Metadata m = JsonSerializer.Deserialize<Metadata>(metadataJson)!;

        var builder = new UdfBuilder();
        builder.VolumeIdentifier = m.VolumeLabel;

        foreach (var entry in m.Entries)
        {
            Console.WriteLine($"Add {entry}");
            builder.AddFile(entry, Path.Combine(parentMetadataPath, "FILES", entry));
        }

        Console.WriteLine("Build ISO");
        builder.Build(Path.Combine(parentMetadataPath, "OUTPUT.iso"));

        break;
    default:
        PrintHelp();
        break;
}
