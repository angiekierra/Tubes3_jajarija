using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Alay
{
    static Random random = new Random();

    public static void run()
    {
        string inputFile = "C:\\Users\\Imanuel Girsang\\OneDrive - Institut Teknologi Bandung\\Documents\\IF-General\\Semester-4\\Stima\\Tubes\\NewTubes3\\Tubes3_jajarija\\person.csv";
        string outputFile = "alay.csv";

        List<string[]> rows = ReadCSV(inputFile);

        // Modify some of the names randomly
        foreach (var row in rows.Skip(1)) // Skipping header row
        {
            if (random.Next(2) == 0) // Randomly decide whether to modify this row
            {
                int nameIndex = 1; // Index of the 'nama' column
                row[nameIndex] = MakeAlay(row[nameIndex]);
            }
        }

        WriteCSV(outputFile, rows);
        Console.WriteLine("CSV file with alay names generated successfully.");
    }

    static string MakeAlay(string original)
    {
        Random random = new Random();
        int num = random.Next(1, 5);

        if (num == 1)
        {
            return AlayTransformer.BesarKecil(original);
        }
        else if (num == 2)
        {
            return AlayTransformer.GabunganAngka(original);
        }
        else if (num == 3)
        {
            return AlayTransformer.Singkat(original);
        }
        else
        {
            return AlayTransformer.Gabungan(original);
        }
    }

    static List<string[]> ReadCSV(string filePath)
    {
        List<string[]> rows = new List<string[]>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');
                rows.Add(values);
            }
        }
        return rows;
    }

    static void WriteCSV(string filePath, List<string[]> rows)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var row in rows)
            {
                writer.WriteLine(string.Join(",", row));
            }
        }
    }
}
