namespace GdMyStash.Data.Saves;

/// <summary>
/// Shared item record layout used by player.gdc and transfer.gst.
/// The writer and the full stat reader will live here as the data layer grows.
/// </summary>
internal static class GdItemReader
{
    /// <summary>
    /// Walks one item record without materializing it. The caller supplies the
    /// container version: FoA containers (version >= 11) add ascended strings
    /// and two reroll counters around the common tail.
    /// </summary>
    public static void Skip(GdReader reader, bool v2)
    {
        reader.ReadString(); // basename
        reader.ReadString(); // prefix
        reader.ReadString(); // suffix
        reader.ReadString(); // modifier
        reader.ReadString(); // transmute
        reader.ReadUInt32(); // seed

        reader.ReadString(); // relic name
        reader.ReadString(); // relic bonus
        reader.ReadUInt32(); // relic seed

        reader.ReadString(); // augment name
        reader.ReadString(); // augment bonus
        reader.ReadUInt32(); // augment seed

        if (v2)
        {
            reader.ReadString(); // ascended name
            reader.ReadString(); // ascended bonus
        }

        reader.ReadUInt32(); // relic completion level
        reader.ReadUInt32(); // stack count

        if (v2)
        {
            reader.ReadUInt32(); // seed reroll count
            reader.ReadUInt32(); // affix reroll count
        }
    }
}
