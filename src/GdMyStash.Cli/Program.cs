using GdMyStash.Data.Saves;

if (args.Length >= 1 && args[0].Equals("inspect", StringComparison.OrdinalIgnoreCase))
{
    if (args.Length is < 2 or > 3)
    {
        Console.Error.WriteLine("Usage: gdmystash inspect <file> [--verbose]");
        return 2;
    }

    var verbose = args.Length == 3 && args[2].Equals("--verbose", StringComparison.OrdinalIgnoreCase);
    return Inspect(args[1], verbose);
}

Console.WriteLine("GD My Stash CLI");
Console.WriteLine();
Console.WriteLine("Usage:");
Console.WriteLine("  gdmystash inspect <save-file> [--verbose]   Inspect a .gdc, .gst, or .gsh file");
return 0;

static int Inspect(string path, bool verbose)
{
    if (!File.Exists(path))
    {
        Console.Error.WriteLine($"File not found: {path}");
        return 1;
    }

    var data = File.ReadAllBytes(path);

    try
    {
        if (path.EndsWith(".gdc", StringComparison.OrdinalIgnoreCase))
        {
            var info = GdcInspector.Inspect(data, verbose ? Console.Out : null);
            Console.WriteLine(
                $"GDC seed=0x{info.Seed:X8} magic=0x{info.Magic:X8} preamble={info.PreambleVersion} " +
                $"name={info.Name} class={info.ClassTag} level={info.Level} hardcore={info.Hardcore} " +
                $"expansion={info.Expansion} dataVersion={info.DataVersion}");

            foreach (var block in info.Blocks)
            {
                Console.WriteLine($"  block {block.Id}: {block.Length} bytes at 0x{block.Offset:X}");
            }

            return 0;
        }

        if (path.EndsWith(".gst", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".gsh", StringComparison.OrdinalIgnoreCase))
        {
            var info = GstInspector.Inspect(data, verbose ? Console.Out : null);
            Console.WriteLine(
                $"GST seed=0x{info.Seed:X8} header={info.HeaderValue} version={info.Version} " +
                $"mod='{info.Mod}' expansion={info.Expansion} pages={info.PageCount}");

            foreach (var page in info.Pages)
            {
                Console.WriteLine($"  page {page.Width}x{page.Height}, items={page.ItemCount}");
            }

            foreach (var block in info.Blocks)
            {
                Console.WriteLine($"  block {block.Id}: {block.Length} bytes at 0x{block.Offset:X}");
            }

            return 0;
        }

        Console.Error.WriteLine($"Unsupported file extension: {Path.GetExtension(path)}");
        return 2;
    }
    catch (GdFormatException ex)
    {
        Console.Error.WriteLine($"Format error: {ex.Message}");
        return 1;
    }
}
