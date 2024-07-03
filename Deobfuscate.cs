using System.Linq;
using System.Text;

namespace Deobfuscate
{
    //NOTE: I adapted this from Twigzie's C# port of the original rust script, linked below.
    //https://github.com/Twigzie/Fantality-LostArkRenamer
    //Credit to the author below. He created the original in rust.
    //https://www.gildor.org/smf/index.php/topic,3055.msg46444.html#msg46444
    public static class Deobfuscate
    {
        private record TableData(string Item1, char Item2, int Item3);

        private static readonly TableData[] OPT_KEY_TABLE = new[]
        {
            new TableData("QP", 'Q', 0),
            new TableData("QD", 'Q', 1),
            new TableData("QW", 'Q', 2),
            new TableData("Q4", 'Q', 3),
            new TableData("QL", '-', 0),
            new TableData("QB", '-', 1),
            new TableData("QO", '-', 2),
            new TableData("Q5", '-', 3),
            new TableData("QC", '_', 0),
            new TableData("QN", '_', 1),
            new TableData("QT", '_', 2),
            new TableData("Q9", '_', 3),
            new TableData("XU", 'X', 0),
            new TableData("XN", 'X', 1),
            new TableData("XH", 'X', 2),
            new TableData("X3", 'X', 3),
            new TableData("XW", '!', 0),
            new TableData("XS", '!', 1),
            new TableData("XZ", '!', 2),
            new TableData("X0", '!', 3),
        };

        private static string Clean(string source)
        {
            source = source.ToUpper();
            var outStr = new StringBuilder();
            int i = 0;
            while (i < source.Length)
            {
                var subst = OPT_KEY_TABLE.FirstOrDefault(t =>
                    source.Substring(i).StartsWith(t.Item1)
                );
                if (subst != null && i % 4 == subst.Item3)
                {
                    outStr.Append(subst.Item2);
                    i += subst.Item1.Length;
                }
                else
                {
                    outStr.Append(source[i]);
                    i++;
                }
            }
            return outStr.ToString();
        }

        public static string Decrypt(string source)
        {
            source = source.ToUpper();
            var outStr = new System.Text.StringBuilder();
            foreach (char c in source)
            {
                int x = c;
                if (c >= '0' && c <= '9')
                {
                    x += 43;
                }
                int i = (31 * (x - source.Length - 65) % 36 + 36) % 36 + 65;
                if (i >= 91)
                {
                    i -= 43;
                }
                outStr.Append((char)i);
            }
            string unescaped = Clean(outStr.ToString());
            return unescaped.Contains("!") ? unescaped.Split('!')[0] : unescaped;
        }
    }
}
