using GdMyStash.Data.Saves;

namespace GdMyStash.Tests.Saves;

public sealed class GdcInspectorTests
{
    [Fact]
    public void Inspect_ReadsHeaderAndBlocks()
    {
        var info = GdcInspector.Inspect(BuildMinimalGdc());

        Assert.Equal(0x12345678u, info.Seed);
        Assert.Equal("TestChar", info.Name);
        Assert.True(info.Male);
        Assert.Equal("tagSkillClassName02", info.ClassTag);
        Assert.Equal(4u, info.Level);
        Assert.False(info.Hardcore);
        Assert.Equal((byte)7, info.Expansion);
        Assert.Equal(8u, info.DataVersion);
        Assert.Equal(new uint[] { 1, 2, 3, 4, 10 }, info.Blocks.Select(b => b.Id).ToArray());
    }

    [Fact]
    public void Inspect_WalksInventoryAndStashNestedBlocks()
    {
        var info = GdcInspector.Inspect(BuildGdcWithNestedBlocks());

        Assert.Contains(info.Blocks, b => b.Id == 3);
        Assert.Contains(info.Blocks, b => b.Id == 4);
        Assert.Contains(info.Blocks, b => b.Id == 10);
    }

    private static byte[] BuildMinimalGdc()
    {
        var writer = new GdWriter(0x12345678);
        WriteHeader(writer);

        WriteSimpleBlock(writer, 1);
        WriteSimpleBlock(writer, 2);
        WriteEmptyInventoryBlock(writer, hasData: false);
        WriteEmptyStashBlock(writer, pages: 0);
        WriteSimpleBlock(writer, 10);

        return writer.ToArray();
    }

    private static byte[] BuildGdcWithNestedBlocks()
    {
        var writer = new GdWriter(0x12345678);
        WriteHeader(writer);

        WriteSimpleBlock(writer, 1);
        WriteSimpleBlock(writer, 2);
        WriteEmptyInventoryBlock(writer, hasData: true);
        WriteEmptyStashBlock(writer, pages: 1);
        WriteSimpleBlock(writer, 10);

        return writer.ToArray();
    }

    private static void WriteHeader(GdWriter writer)
    {
        writer.WriteUInt32(GdcInspector.MagicValue);
        writer.WriteUInt32(2); // preamble version
        writer.WriteWideString("TestChar");
        writer.WriteBool(true);
        writer.WriteString("tagSkillClassName02");
        writer.WriteUInt32(4);
        writer.WriteBool(false);
        writer.WriteByte(7);
        writer.WriteChecksum();
        writer.WriteUInt32(8); // data version
        writer.WriteBytes(new byte[16]); // mystery
    }

    private static void WriteSimpleBlock(GdWriter writer, uint blockId)
    {
        writer.BeginBlock(blockId);
        writer.WriteUInt32(1);
        writer.EndBlock();
    }

    private static void WriteEmptyInventoryBlock(GdWriter writer, bool hasData)
    {
        writer.BeginBlock(3);
        writer.WriteUInt32(11); // FoA item layout
        writer.WriteBool(hasData);

        if (hasData)
        {
            writer.WriteUInt32(1); // sack count
            writer.WriteUInt32(0); // focused sack
            writer.WriteUInt32(0); // selected sack

            writer.BeginBlock(0);
            writer.WriteBool(false); // unused
            writer.WriteUInt32(0); // items
            writer.EndBlock();

            writer.WriteBool(false); // use alternate weapon set

            for (var i = 0; i < 12; i++)
            {
                SaveTestData.WriteEmptyItem(writer, v2: true);
                writer.WriteBool(false); // attached
            }

            for (var set = 0; set < 2; set++)
            {
                writer.WriteBool(false); // unused
                for (var i = 0; i < 2; i++)
                {
                    SaveTestData.WriteEmptyItem(writer, v2: true);
                    writer.WriteBool(false); // attached
                }
            }
        }

        writer.EndBlock();
    }

    private static void WriteEmptyStashBlock(GdWriter writer, int pages)
    {
        writer.BeginBlock(4);
        writer.WriteUInt32(11); // FoA item layout
        writer.WriteUInt32((uint)pages);

        for (var page = 0; page < pages; page++)
        {
            writer.BeginBlock(0);
            writer.WriteUInt32(10); // width
            writer.WriteUInt32(19); // height
            writer.WriteUInt32(0); // items

            for (var i = 0; i < 5; i++)
            {
                writer.WriteUInt32(0); // page trailer
            }

            writer.EndBlock();
        }

        writer.EndBlock();
    }
}
