using GdMyStash.Data.Saves;

namespace GdMyStash.Tests.Saves;

public sealed class GstInspectorTests
{
    [Fact]
    public void Inspect_ReadsEmptyStash()
    {
        var info = GstInspector.Inspect(BuildStash(itemCount: 0));

        Assert.Equal(2u, info.HeaderValue);
        Assert.Equal(11u, info.Version);
        Assert.Equal((byte)7, info.Expansion);
        Assert.Equal(1u, info.PageCount);
        Assert.Single(info.Pages);
        Assert.Equal(10u, info.Pages[0].Width);
        Assert.Equal(19u, info.Pages[0].Height);
        Assert.Equal(0u, info.Pages[0].ItemCount);
        Assert.Single(info.Blocks);
        Assert.Equal(18u, info.Blocks[0].Id);
    }

    [Fact]
    public void Inspect_WalksItemInPage()
    {
        var info = GstInspector.Inspect(BuildStash(itemCount: 1));

        Assert.Equal(1u, info.PageCount);
        Assert.Equal(1u, info.Pages[0].ItemCount);
    }

    private static byte[] BuildStash(int itemCount)
    {
        var writer = new GdWriter(0x6268D592);
        writer.WriteUInt32(2);

        writer.BeginBlock(18);
        writer.WriteUInt32(11); // FoA layout
        writer.WriteUInt32NoUpdate(0); // unknown
        writer.WriteString(""); // mod
        writer.WriteByte(7); // expansion
        writer.WriteUInt32(1); // page count

        writer.BeginBlock(0);
        writer.WriteUInt32(10); // width
        writer.WriteUInt32(19); // height
        writer.WriteUInt32((uint)itemCount);

        for (var i = 0; i < itemCount; i++)
        {
            SaveTestData.WriteEmptyItem(writer, v2: true);
            writer.WriteUInt32(0); // x (float)
            writer.WriteUInt32(0); // y (float)
        }

        for (var i = 0; i < 5; i++)
        {
            writer.WriteUInt32(0); // page trailer
        }

        writer.EndBlock();
        writer.EndBlock();

        return writer.ToArray();
    }
}
