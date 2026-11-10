// Ported from grim-dawn-save-editor (MIT), Copyright (c) 2026 Zez (kraken444444).
// https://github.com/kraken444444/grim-dawn-save-editor/blob/main/gdsave/codec.py
using System.Buffers.Binary;
using System.Text;

namespace GdMyStash.Data.Saves;

/// <summary>Raised when a Grim Dawn save file does not match the expected structure.</summary>
public sealed class GdFormatException : Exception
{
    public GdFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// The rolling XOR stream used by Grim Dawn save files (.gdc, .gst, .gsh).
/// The first four bytes are the seed masked with <see cref="SeedMask"/>.
/// </summary>
public static class GdCodec
{
    public const uint SeedMask = 0x55555555u;
    public const uint Multiplier = 39916801u;

    public static uint[] GenerateTable(uint seed)
    {
        var table = new uint[256];
        var value = seed;

        for (var i = 0; i < table.Length; i++)
        {
            // Rotate right by one, then multiply modulo 2^32.
            value = unchecked(((value << 31) | (value >> 1)) * Multiplier);
            table[i] = value;
        }

        return table;
    }
}

/// <summary>Decrypting reader over an in-memory save file.</summary>
public sealed class GdReader
{
    private readonly byte[] _data;
    private readonly uint[] _table;
    private int _position;

    public GdReader(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length < 8)
        {
            throw new GdFormatException("File is too short to be a Grim Dawn save.");
        }

        _data = data;
        _position = 4;
        Seed = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, 4)) ^ GdCodec.SeedMask;
        State = Seed;
        _table = GdCodec.GenerateTable(Seed);
    }

    public uint Seed { get; }

    public uint State { get; private set; }

    public int Position => _position;

    public int Remaining => _data.Length - _position;

    public uint ReadUInt32()
    {
        Need(4);

        var cipher = BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(_position, 4));
        var value = cipher ^ State;
        var state = State;

        for (var i = 0; i < 4; i++)
        {
            state ^= _table[_data[_position + i]];
        }

        State = state;
        _position += 4;
        return value;
    }

    public int ReadInt32() => unchecked((int)ReadUInt32());

    public float ReadFloat() => BitConverter.UInt32BitsToSingle(ReadUInt32());

    public byte ReadByte()
    {
        Need(1);

        var cipher = _data[_position];
        var value = (byte)(cipher ^ (State & 0xFF));
        State ^= _table[cipher];
        _position++;
        return value;
    }

    public bool ReadBool() => ReadByte() == 1;

    public byte[] ReadBytes(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        Need(count);

        var result = new byte[count];
        var state = State;

        for (var i = 0; i < count; i++)
        {
            var cipher = _data[_position + i];
            result[i] = (byte)(cipher ^ (state & 0xFF));
            state ^= _table[cipher];
        }

        State = state;
        _position += count;
        return result;
    }

    public string ReadString()
    {
        var length = ReadUInt32();
        if (length > (uint)Remaining)
        {
            throw new GdFormatException(
                $"String length {length} at offset {_position} exceeds the file size.");
        }

        return Encoding.Latin1.GetString(ReadBytes((int)length));
    }

    public string ReadWideString()
    {
        var codeUnits = ReadUInt32();
        if (codeUnits > (uint)(Remaining / 2))
        {
            throw new GdFormatException(
                $"Wide string length {codeUnits} at offset {_position} exceeds the file size.");
        }

        var byteCount = checked((int)codeUnits * 2);
        return Encoding.Unicode.GetString(ReadBytes(byteCount));
    }

    /// <summary>
    /// Reads a block payload length. The value is XORed against the current state,
    /// but the state does not advance.
    /// </summary>
    public uint ReadBlockLength()
    {
        Need(4);

        var cipher = BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(_position, 4));
        _position += 4;
        return cipher ^ State;
    }

    public (uint BlockId, int EndPosition) BeginBlock(uint? expectedId = null)
    {
        var blockId = ReadUInt32();
        var length = ReadBlockLength();
        var end = (long)_position + length;

        if (end > _data.Length)
        {
            throw new GdFormatException(
                $"Block {blockId} at offset {_position - 8} claims length {length}, " +
                "which runs past the end of the file.");
        }

        if (expectedId.HasValue && blockId != expectedId.Value)
        {
            throw new GdFormatException(
                $"Expected block {expectedId.Value} but found block {blockId} at offset {_position - 8}.");
        }

        return (blockId, (int)end);
    }

    public void EndBlock(uint blockId, int expectedEndPosition)
    {
        if (_position != expectedEndPosition)
        {
            throw new GdFormatException(
                $"Block {blockId}: parser stopped at offset {_position}, expected {expectedEndPosition} " +
                $"(off by {expectedEndPosition - _position}).");
        }

        Need(4);

        var checksum = BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(_position, 4));
        _position += 4;

        if (checksum != State)
        {
            throw new GdFormatException(
                $"Block {blockId}: checksum mismatch at offset {_position - 4} " +
                $"(file 0x{checksum:X8}, computed 0x{State:X8}).");
        }
    }

    /// <summary>Verifies a bare running-state checksum, such as the one after the GDC header.</summary>
    public void VerifyChecksum()
    {
        Need(4);

        var checksum = BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(_position, 4));
        _position += 4;

        if (checksum != State)
        {
            throw new GdFormatException(
                $"Header checksum mismatch at offset {_position - 4} " +
                $"(file 0x{checksum:X8}, computed 0x{State:X8}).");
        }
    }

    private void Need(int count)
    {
        if (count < 0 || _position + count > _data.Length)
        {
            throw new GdFormatException(
                $"Unexpected end of file at offset {_position} (wanted {count} more bytes).");
        }
    }
}

/// <summary>Encrypting writer that produces an in-memory save file.</summary>
public sealed class GdWriter
{
    private readonly List<byte> _buffer;
    private readonly Stack<BlockFrame> _blocks = new();
    private readonly uint[] _table;
    private uint _state;

    public GdWriter(uint seed)
    {
        Seed = seed;
        _state = seed;
        _table = GdCodec.GenerateTable(seed);
        _buffer = new List<byte>(1024);
        AppendUInt32Raw(seed ^ GdCodec.SeedMask);
    }

    public uint Seed { get; }

    public uint State => _state;

    public void WriteUInt32(uint value)
    {
        var cipher = value ^ _state;
        AppendUInt32Raw(cipher);

        var state = _state;
        for (var i = 0; i < 4; i++)
        {
            state ^= _table[(byte)(cipher >> (8 * i))];
        }

        _state = state;
    }

    public void WriteInt32(int value) => WriteUInt32(unchecked((uint)value));

    public void WriteFloat(float value) => WriteUInt32(BitConverter.SingleToUInt32Bits(value));

    public void WriteByte(byte value)
    {
        var cipher = (byte)(value ^ (_state & 0xFF));
        _buffer.Add(cipher);
        _state ^= _table[cipher];
    }

    public void WriteBool(bool value) => WriteByte(value ? (byte)1 : (byte)0);

    public void WriteBytes(ReadOnlySpan<byte> data)
    {
        var state = _state;

        foreach (var value in data)
        {
            var cipher = (byte)(value ^ (state & 0xFF));
            _buffer.Add(cipher);
            state ^= _table[cipher];
        }

        _state = state;
    }

    public void WriteString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        foreach (var c in value)
        {
            if (c > 0xFF)
            {
                throw new GdFormatException($"String contains a non-Latin-1 character: U+{(int)c:X4}.");
            }
        }

        var bytes = Encoding.Latin1.GetBytes(value);
        WriteUInt32((uint)bytes.Length);
        WriteBytes(bytes);
    }

    public void WriteWideString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var bytes = Encoding.Unicode.GetBytes(value);
        WriteUInt32((uint)(bytes.Length / 2));
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a 32-bit value XORed against the current state without advancing it.
    /// Used for block lengths and a few opaque fields.
    /// </summary>
    public void WriteUInt32NoUpdate(uint value) => AppendUInt32Raw(value ^ _state);

    public void BeginBlock(uint blockId)
    {
        WriteUInt32(blockId);
        _blocks.Push(new BlockFrame(blockId, _buffer.Count, _state));
        AppendUInt32Raw(0); // length placeholder, patched in EndBlock
    }

    public void EndBlock()
    {
        if (_blocks.Count == 0)
        {
            throw new InvalidOperationException("No open block.");
        }

        var frame = _blocks.Pop();
        var payloadLength = _buffer.Count - frame.LengthOffset - 4;
        PatchUInt32(frame.LengthOffset, (uint)payloadLength ^ frame.LengthState);
        AppendUInt32Raw(_state); // checksum = raw running state
    }

    /// <summary>Writes a bare running-state checksum, such as the one after the GDC header.</summary>
    public void WriteChecksum() => AppendUInt32Raw(_state);

    public byte[] ToArray()
    {
        if (_blocks.Count != 0)
        {
            throw new InvalidOperationException("Cannot finish a writer with open blocks.");
        }

        return _buffer.ToArray();
    }

    private void AppendUInt32Raw(uint value)
    {
        _buffer.Add((byte)value);
        _buffer.Add((byte)(value >> 8));
        _buffer.Add((byte)(value >> 16));
        _buffer.Add((byte)(value >> 24));
    }

    private void PatchUInt32(int offset, uint value)
    {
        _buffer[offset] = (byte)value;
        _buffer[offset + 1] = (byte)(value >> 8);
        _buffer[offset + 2] = (byte)(value >> 16);
        _buffer[offset + 3] = (byte)(value >> 24);
    }

    private readonly record struct BlockFrame(uint BlockId, int LengthOffset, uint LengthState);
}
