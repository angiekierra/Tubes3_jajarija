using System;
using System.Collections.Generic;
using System.Linq;

class AlayTransformer
{

    public static string BesarKecil(string text)
    {
        Random random = new Random();
        string hasil = "";
        for (int i = 0; i < text.Length; i++)
        {
            if (random.Next(2) == 0)
            {
                hasil += char.ToUpper(text[i]);
            }
            else
            {
                hasil += char.ToLower(text[i]);
            }
        }
        return hasil;
    }

    public static string GabunganAngka(string text)
    {
        Dictionary<char, char> kamus = new Dictionary<char, char>
        {
            {'a', '4'},
            {'e', '3'},
            {'g', '6'},
            {'i', '1'},
            {'j', '7'},
            {'o', '0'},
            {'s', '5'},
            {'z', '2'}
        };

        string hasil = "";
        foreach (char c in text)
        {
            if (kamus.ContainsKey(c))
            {
                hasil += kamus[c];
            }
            else
            {
                hasil += c;
            }
        }
        return hasil;
    }

    public static string Singkat(string text)
    {
        Random rnd = new Random();
        string vowels = "aiueoAIUEO";
        
        // Find all indices of vowel characters
        var vowelIndices = text
            .Select((c, index) => new { Char = c, Index = index })
            .Where(x => vowels.Contains(x.Char))
            .Select(x => x.Index)
            .ToList();

        if (vowelIndices.Count == 0)
        {
            // No vowels found, return the original text
            return text;
        }

        // Randomly select an index from the list of vowel indices
        int index = vowelIndices[rnd.Next(vowelIndices.Count)];
        text = text.Remove(index, 1);

        // 50% chance to remove another vowel
        if (vowelIndices.Count > 1 && rnd.Next(2) == 0)
        {
            // Update vowel indices list
            vowelIndices.Remove(index);

            // Select another random index from the updated list of vowel indices
            index = vowelIndices[rnd.Next(vowelIndices.Count)];
            text = text.Remove(index, 1);
        }

        return text;
    }


    public static string Gabungan(string text){
        return GabunganAngka(Singkat(BesarKecil(text)));
    }
}


