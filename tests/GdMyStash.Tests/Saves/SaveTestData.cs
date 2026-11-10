using GdMyStash.Data.Saves;

namespace GdMyStash.Tests.Saves;

internal static class SaveTestData
{
    public static void WriteEmptyItem(GdWriter writer, bool v2)
    {
        writer.WriteString("records/items/test.dbr");
        writer.WriteString(""); // prefix
        writer.WriteString(""); // suffix
        writer.WriteString(""); // modifier
        writer.WriteString(""); // transmute
        writer.WriteUInt32(0x12345678); // seed

        writer.WriteString(""); // relic name
        writer.WriteString(""); // relic bonus
        writer.WriteUInt32(0); // relic seed

        writer.WriteString(""); // augment name
        writer.WriteString(""); // augment bonus
        writer.WriteUInt32(0); // augment seed

        if (v2)
        {
            writer.WriteString(""); // ascended name
            writer.WriteString(""); // ascended bonus
        }

        writer.WriteUInt32(0); // relic completion level
        writer.WriteUInt32(1); // stack count

        if (v2)
        {
            writer.WriteUInt32(0); // seed reroll count
            writer.WriteUInt32(0); // affix reroll count
        }
    }
}
