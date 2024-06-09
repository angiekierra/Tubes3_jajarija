using System;
class BM
{
    public static int BmMatch(string text, string pattern)
    {
        int[] last = BuildLast(pattern);
        int n = text.Length;
        int m = pattern.Length;
        int i = m - 1;

        // If longer, no match
        if (i > n - 1)
            return -1; 

        int j = m - 1;

        // Cari sampe ketemu
        do
        {
            if (pattern[j] == text[i])
            {
                if (j == 0)
                    return i; // match
                else
                { // Decrement untill start of string
                    i--;
                    j--;
                }
            }
            else
            { // character jump technique
                int lo = last[text[i]]; // last occ
                i = i + m - Math.Min(j, 1 + lo);
                j = m - 1;
            }
        } while (i <= n - 1);

        return -1; // no match
    }

    public static int[] BuildLast(string pattern)
    {
        int[] last = new int[256]; // ASCII char set
        for (int i = 0; i < 256; i++)
            last[i] = -1; // initialize array

        for (int i = 0; i < pattern.Length; i++)
            last[pattern[i]] = i;

        return last;
    }
}
