namespace GdMyStash.Data.Saves;

public sealed record GdcBlockInfo(uint Id, int Length, int Offset);

public sealed record GdcFileInfo(
    uint Seed,
    uint Magic,
    uint PreambleVersion,
    string Name,
    bool Male,
    string ClassTag,
    uint Level,
    bool Hardcore,
    byte Expansion,
    uint DataVersion,
    IReadOnlyList<GdcBlockInfo> Blocks);

/// <summary>
/// Structural reader for player.gdc. It validates the header and every block
/// checksum. Block 3 (inventory and equipment) is parsed far enough to walk
/// its nested sack blocks; other blocks are skipped as opaque payloads.
/// </summary>
public static class GdcInspector
{
    public const uint MagicValue = 0x58434447u; // 'GDCX' in little-endian order

    private const uint BlockInventory = 3;
    private const uint BlockPersonalStash = 4;
    private const uint ItemV2BlockVersion = 11;

    public static GdcFileInfo Inspect(byte[] data, TextWriter? log = null)
    {
        var reader = new GdReader(data);
        var magic = reader.ReadUInt32();

        if (magic != MagicValue)
        {
            throw new GdFormatException($"Not a GDC file: magic 0x{magic:X8}.");
        }

        var preambleVersion = reader.ReadUInt32();
        var name = reader.ReadWideString();
        var male = reader.ReadBool();
        var classTag = reader.ReadString();
        var level = reader.ReadUInt32();
        var hardcore = reader.ReadBool();
        var expansion = reader.ReadByte();

        reader.VerifyChecksum();

        var dataVersion = reader.ReadUInt32();
        reader.ReadBytes(16); // mystery header bytes

        log?.WriteLine(
            $"header: seed=0x{reader.Seed:X8} preamble={preambleVersion} name='{name}' male={male} " +
            $"class='{classTag}' level={level} hardcore={hardcore} expansion={expansion} " +
            $"dataVersion={dataVersion} headerEnd=0x{reader.Position:X} state=0x{reader.State:X8}");

        var blocks = new List<GdcBlockInfo>();

        while (reader.Remaining > 0)
        {
            var offset = reader.Position;
            log?.WriteLine($"block at 0x{offset:X}: state=0x{reader.State:X8}");

            var (blockId, end) = reader.BeginBlock();
            var length = end - reader.Position;
            log?.WriteLine($"  id={blockId} length={length} payloadStart=0x{reader.Position:X} end=0x{end:X}");

            if (blockId == BlockInventory)
            {
                ReadBlock3(reader, log);
            }
            else if (blockId == BlockPersonalStash)
            {
                ReadBlock4(reader, log);
            }
            else
            {
                reader.ReadBytes(length);
            }

            log?.WriteLine($"  after payload: pos=0x{reader.Position:X} state=0x{reader.State:X8}");

            try
            {
                reader.EndBlock(blockId, end);
            }
            catch (GdFormatException ex)
            {
                log?.WriteLine($"  checksum failed: {ex.Message}");
                throw;
            }

            blocks.Add(new GdcBlockInfo(blockId, length, offset));
        }

        return new GdcFileInfo(
            reader.Seed,
            magic,
            preambleVersion,
            name,
            male,
            classTag,
            level,
            hardcore,
            expansion,
            dataVersion,
            blocks);
    }

    private static void ReadBlock3(GdReader reader, TextWriter? log)
    {
        var version = reader.ReadUInt32();
        var v2 = version >= ItemV2BlockVersion;
        var hasData = reader.ReadBool();

        if (!hasData)
        {
            log?.WriteLine($"  block 3: version={version} hasData=false");
            return;
        }

        var sackCountRaw = reader.ReadUInt32();
        var focusedSack = reader.ReadUInt32();
        var selectedSack = reader.ReadUInt32();

        if (sackCountRaw > 100)
        {
            throw new GdFormatException($"Block 3: unreasonable sack count {sackCountRaw}.");
        }

        var sackCount = (int)sackCountRaw;
        log?.WriteLine(
            $"  block 3: version={version} sacks={sackCount} focused={focusedSack} selected={selectedSack}");

        for (var sackIndex = 0; sackIndex < sackCount; sackIndex++)
        {
            var (subId, subEnd) = reader.BeginBlock(0);
            reader.ReadBool(); // unused

            var itemCountRaw = reader.ReadUInt32();
            if (itemCountRaw > 1000)
            {
                throw new GdFormatException(
                    $"Block 3: unreasonable item count {itemCountRaw} in sack {sackIndex}.");
            }

            var itemCount = (int)itemCountRaw;
            for (var itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                GdItemReader.Skip(reader, v2);
                reader.ReadUInt32(); // x
                reader.ReadUInt32(); // y
            }

            reader.EndBlock(subId, subEnd);
            log?.WriteLine($"    sack {sackIndex}: {itemCount} items");
        }

        reader.ReadBool(); // use_alt_weaponset

        for (var i = 0; i < 12; i++)
        {
            GdItemReader.Skip(reader, v2);
            reader.ReadBool(); // attached
        }

        for (var setIndex = 0; setIndex < 2; setIndex++)
        {
            reader.ReadBool(); // unused
            for (var i = 0; i < 2; i++)
            {
                GdItemReader.Skip(reader, v2);
                reader.ReadBool(); // attached
            }
        }

        log?.WriteLine("    equipment: 12 slots, 2 weapon sets");
    }

    private static void ReadBlock4(GdReader reader, TextWriter? log)
    {
        var version = reader.ReadUInt32();
        var v2 = version >= ItemV2BlockVersion;
        var pageCountRaw = reader.ReadUInt32();

        if (pageCountRaw > 100)
        {
            throw new GdFormatException($"Block 4: unreasonable stash page count {pageCountRaw}.");
        }

        var pageCount = (int)pageCountRaw;

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            var (subId, subEnd) = reader.BeginBlock(0);
            var width = reader.ReadUInt32();
            var height = reader.ReadUInt32();
            var itemCountRaw = reader.ReadUInt32();

            if (itemCountRaw > 10_000)
            {
                throw new GdFormatException(
                    $"Block 4: unreasonable item count {itemCountRaw} on stash page {pageIndex}.");
            }

            var itemCount = (int)itemCountRaw;
            for (var itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                GdItemReader.Skip(reader, v2);
                reader.ReadUInt32(); // x (float)
                reader.ReadUInt32(); // y (float)
            }

            if (v2)
            {
                for (var i = 0; i < 5; i++)
                {
                    reader.ReadUInt32(); // page trailer
                }
            }

            reader.EndBlock(subId, subEnd);
            log?.WriteLine($"    stash page {pageIndex}: {width}x{height}, {itemCount} items");
        }
    }

}
