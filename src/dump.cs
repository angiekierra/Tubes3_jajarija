// Console.WriteLine("Please enter the text to search:");
//         string text = Console.ReadLine() ?? string.Empty;

//         Console.WriteLine("Please enter the pattern to search for:");
//         string pattern = Console.ReadLine() ?? string.Empty;


//         // Tes bahasa alay

//         // Console.WriteLine("Please enter the text to search:");

//         string orisinil = "Bintang Dwi Marthen";
//         string alay = "bintanG DwI mArthen";
//         string alay2 = "B1nt4n6 Dw1 M4rthen";
//         string alay3 = "Bntng Dw Mrthen";
//         string alay4 = "b1ntN6 Dw mrthn";


//         bool res1 = IsAlayVersion(orisinil, alay);
//         if (res1)
//         {
//             Console.WriteLine("Alay version detected");
//         }
//         else
//         {
//             Console.WriteLine("Alay version not detected");
//         }

//         bool res2 = IsAlayVersion(orisinil, alay2);

//         if (res2)
//         {
//             Console.WriteLine("Alay version detected");
//         }
//         else
//         {
//             Console.WriteLine("Alay version not detected");
//         }

//         bool res3 = IsAlayVersion(orisinil, alay3);

//         if (res3)
//         {
//             Console.WriteLine("Alay version detected");
//         }
//         else
//         {
//             Console.WriteLine("Alay version not detected");
//         }

//         bool res4 = IsAlayVersion(orisinil, alay4);

//         if (res4)
//         {
//             Console.WriteLine("Alay version detected");
//         }
//         else
//         {
//             Console.WriteLine("Alay version not detected");
//         }

//         IsAlayVersion("Aku cinta kamu", "4ku c1nt4 k4mu"); // true


//         int index1 = KmpMatch(text, pattern);
//         if (index1 != -1)
//         {
//             Console.WriteLine($"Pattern found at index: {index1}");
//         }
//         else
//         {
//             Console.WriteLine("Pattern not found.");
//         }

//         int index = BmMatch(text, pattern);
//         if (index != -1)
//         {
//             Console.WriteLine($"Pattern found at index: {index}");
//         }
//         else
//         {
//             Console.WriteLine("Pattern not found.");
//         }