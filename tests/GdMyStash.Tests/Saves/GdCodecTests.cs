using System.Buffers.Binary;
using GdMyStash.Data.Saves;

namespace GdMyStash.Tests.Saves;

public sealed class GdCodecTests
{
    [Fact]
    public void GenerateTable_MatchesReference()
    {
        var table = GdCodec.GenerateTable(0x12345678);

        Assert.Equal(256, table.Length);
        Assert.Equal(0x0862173Cu, table[0]);
        Assert.Equal(0xAD03019Eu, table[1]);
        Assert.Equal(0x62817BCFu, table[2]);
        Assert.Equal(0xC35BB0E7u, table[3]);
        Assert.Equal(0x0E024773u, table[4]);
        Assert.Equal(0x9A0850B9u, table[5]);
        Assert.Equal(0x272FB45Cu, table[6]);
        Assert.Equal(0x8FEBA02Eu, table[7]);
    }

    [Fact]
    public void SeedHeader_IsMasked()
    {
        const uint seed = 0x12345678;

        var writer = new GdWriter(seed);
        writer.WriteUInt32(0);
        var data = writer.ToArray();

        Assert.Equal(seed ^ GdCodec.SeedMask, BinaryPrimitives.ReadUInt32LittleEndian(data));

        var reader = new GdReader(data);
        Assert.Equal(seed, reader.Seed);
        Assert.Equal(0u, reader.ReadUInt32());
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void RoundTrip_Primitives()
    {
        var writer = new GdWriter(0xDEADBEEF);
        writer.WriteUInt32(0x12345678);
        writer.WriteByte(0xAB);
        writer.WriteBool(true);
        writer.WriteFloat(3.14159f);
        writer.WriteString("records/items/foo.dbr");
        writer.WriteWideString("Тест");
        writer.WriteUInt32NoUpdate(0xCAFEBABE);

        var reader = new GdReader(writer.ToArray());

        Assert.Equal(0x12345678u, reader.ReadUInt32());
        Assert.Equal((byte)0xAB, reader.ReadByte());
        Assert.True(reader.ReadBool());
        Assert.Equal(3.14159f, reader.ReadFloat());
        Assert.Equal("records/items/foo.dbr", reader.ReadString());
        Assert.Equal("Тест", reader.ReadWideString());
        Assert.Equal(0xCAFEBABEu, reader.ReadBlockLength());
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void BlockLength_DoesNotAdvanceState()
    {
        var writer = new GdWriter(123);
        writer.WriteUInt32(0x11111111);
        writer.WriteUInt32NoUpdate(0x22222222);

        var reader = new GdReader(writer.ToArray());
        Assert.Equal(0x11111111u, reader.ReadUInt32());

        var stateBefore = reader.State;
        Assert.Equal(0x22222222u, reader.ReadBlockLength());
        Assert.Equal(stateBefore, reader.State);
    }

    [Fact]
    public void RoundTrip_Block()
    {
        var writer = new GdWriter(42);
        writer.BeginBlock(18);
        writer.WriteUInt32(5);
        writer.WriteString("hello");
        writer.WriteByte(7);
        writer.EndBlock();

        var reader = new GdReader(writer.ToArray());
        var (blockId, end) = reader.BeginBlock();

        Assert.Equal(18u, blockId);
        Assert.Equal(5u, reader.ReadUInt32());
        Assert.Equal("hello", reader.ReadString());
        Assert.Equal((byte)7, reader.ReadByte());

        reader.EndBlock(blockId, end);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void RoundTrip_NestedBlocks()
    {
        var writer = new GdWriter(0xAABBCCDD);
        writer.BeginBlock(10);
        writer.WriteString("outer");
        writer.BeginBlock(11);
        writer.WriteUInt32(99);
        writer.EndBlock();
        writer.EndBlock();

        var reader = new GdReader(writer.ToArray());

        var (outerId, outerEnd) = reader.BeginBlock();
        Assert.Equal(10u, outerId);
        Assert.Equal("outer", reader.ReadString());

        var (innerId, innerEnd) = reader.BeginBlock();
        Assert.Equal(11u, innerId);
        Assert.Equal(99u, reader.ReadUInt32());
        reader.EndBlock(innerId, innerEnd);

        reader.EndBlock(outerId, outerEnd);
        Assert.Equal(0, reader.Remaining);
    }

    [Fact]
    public void Reader_RejectsBadChecksum()
    {
        var writer = new GdWriter(42);
        writer.BeginBlock(1);
        writer.WriteUInt32(1);
        writer.EndBlock();

        var data = writer.ToArray();
        data[^1] ^= 0xFF;

        var reader = new GdReader(data);
        var (blockId, end) = reader.BeginBlock();
        reader.ReadUInt32();

        Assert.Throws<GdFormatException>(() => reader.EndBlock(blockId, end));
    }

    [Fact]
    public void Reader_RejectsTruncatedFile()
    {
        Assert.Throws<GdFormatException>(() => new GdReader(new byte[4]));

        var writer = new GdWriter(42);
        writer.WriteUInt32(1);
        writer.WriteUInt32(2);

        var data = writer.ToArray()[..^2];
        var reader = new GdReader(data);
        reader.ReadUInt32();

        Assert.Throws<GdFormatException>(() => reader.ReadUInt32());
    }

    [Fact]
    public void Reader_RejectsWrongBlockId()
    {
        var writer = new GdWriter(42);
        writer.BeginBlock(7);
        writer.WriteUInt32(1);
        writer.EndBlock();

        var reader = new GdReader(writer.ToArray());

        Assert.Throws<GdFormatException>(() => reader.BeginBlock(8));
    }
}
