namespace GdMyStash.Data.Saves;

public sealed record GstBlockInfo(uint Id, int Length, int Offset);

public sealed record GstPageInfo(uint Width, uint Height, uint ItemCount);

public sealed record GstFileInfo(
    uint Seed,
    uint HeaderValue,
    uint Version,
    uint Unknown,
    string Mod,
    byte Expansion,
    uint PageCount,
    IReadOnlyList<GstPageInfo> Pages,
    IReadOnlyList<GstBlockInfo> Blocks);

/// <summary>
/// Structural reader for transfer.gst / transfer.gsh. It validates the header,
/// the nested page blocks, and every checksum. Items are walked but not
/// materialized yet.
/// </summary>
public static class GstInspector
{
    public const uint StashBlockId = 18;

    private const uint ItemV2BlockVersion = 11;

    public static GstFileInfo Inspect(byte[] data, TextWriter? log = null)
    {
        var reader = new GdReader(data);
        var headerValue = reader.ReadUInt32();

        if (headerValue != 2)
        {
            throw new GdFormatException($"Unexpected stash header {headerValue} (expected 2).");
        }

        var blocks = new List<GstBlockInfo>();
        var pages = new List<GstPageInfo>();
        var version = 0u;
        var unknown = 0u;
        var pageCount = 0u;
        var mod = string.Empty;
        byte expansion = 0;

        while (reader.Remaining > 0)
        {
            var offset = reader.Position;
            var (blockId, end) = reader.BeginBlock();
            var length = end - reader.Position;

            if (blockId == StashBlockId)
            {
                version = reader.ReadUInt32();
                var v2 = version >= ItemV2BlockVersion;
                unknown = reader.ReadBlockLength();
                mod = reader.ReadString();
                expansion = reader.ReadByte();
                pageCount = reader.ReadUInt32();

                if (pageCount > 100)
                {
                    throw new GdFormatException($"Unreasonable stash page count {pageCount}.");
                }

                for (var pageIndex = 0; pageIndex < (int)pageCount; pageIndex++)
                {
                    var (pageId, pageEnd) = reader.BeginBlock(0);
                    var width = reader.ReadUInt32();
                    var height = reader.ReadUInt32();
                    var itemCount = reader.ReadUInt32();

                    if (itemCount > 10_000)
                    {
                        throw new GdFormatException(
                            $"Unreasonable item count {itemCount} on stash page {pageIndex}.");
                    }

                    for (var itemIndex = 0; itemIndex < (int)itemCount; itemIndex++)
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

                    reader.EndBlock(pageId, pageEnd);
                    pages.Add(new GstPageInfo(width, height, itemCount));
                    log?.WriteLine($"  page {pageIndex}: {width}x{height}, {itemCount} items");
                }
            }

            var remaining = end - reader.Position;
            if (remaining > 0)
            {
                reader.ReadBytes(remaining);
            }

            reader.EndBlock(blockId, end);
            blocks.Add(new GstBlockInfo(blockId, length, offset));
            log?.WriteLine($"block {blockId}: {length} bytes at 0x{offset:X}");
        }

        return new GstFileInfo(
            reader.Seed,
            headerValue,
            version,
            unknown,
            mod,
            expansion,
            pageCount,
            pages,
            blocks);
    }
}
