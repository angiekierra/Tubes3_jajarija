using System.Text.RegularExpressions;

class AlayMatcher
{
    public static bool IsAlayVersion(string original, string alay)
    {
        // Handle case huruf gede kecil
        original = original.ToLower();
        alay = alay.ToLower();

        // Remove spaces from both strings
        original = original.Replace(" ", "");
        alay = alay.Replace(" ", "");


        // Handle case where vocals is not present (allowed)
        original = Regex.Replace(original, @"[a]", "a?", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"[i]", "i?", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"[u]", "u?", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"[e]", "e?", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"[o]", "o?", RegexOptions.IgnoreCase);
        

        // handle ganti huruf
        original = Regex.Replace(original, @"a", "[a@4]", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"i", "[i1!]", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"s", "[s5$]", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"t", "[t7+]", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"g", "[g69]", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"e", "[e3]", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"o", "[o0]", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"b", "[b8]", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"l", "[l1]", RegexOptions.IgnoreCase);
        original = Regex.Replace(original, @"z", "[z2]", RegexOptions.IgnoreCase);




        // Construct the regex pattern for matching alay strings
        string regexPattern = $"^{original}$";

        // Check if the alay string matches the modified original string using regex
        return Regex.IsMatch(alay, regexPattern, RegexOptions.IgnoreCase);
    }
}
