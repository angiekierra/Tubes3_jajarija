using System;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp.ColorSpaces;

class Converter
{
    public static bool[] ConvertFullImageToBinary(Bitmap bitmap)
    {
        int width = bitmap.PixelSize.Width;
        int height = bitmap.PixelSize.Height;

        bool[] binaryImage = new bool[width * height];

        int stride = width * 4; 
        
        IntPtr buffer = Marshal.AllocHGlobal(height * stride);

        try
        {
            Avalonia.PixelRect pixelRect = new(0, 0, width, height);
            bitmap.CopyPixels(pixelRect, buffer, height * stride, stride);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x * 4;
                    byte b = Marshal.ReadByte(buffer, index);
                    byte g = Marshal.ReadByte(buffer, index + 1);
                    byte r = Marshal.ReadByte(buffer, index + 2);

                
                    int gray = (int)(0.3 * r + 0.59 * g + 0.11 * b);

                    binaryImage[y * width + x] = gray >= 128;
                }
            }
        }
        finally
        {
   
            Marshal.FreeHGlobal(buffer);
        }

        return binaryImage;
    }

    public static bool[] GetSelectedBinary(Bitmap bitmap, String type)
    {
        int width = bitmap.PixelSize.Width;
        int height = bitmap.PixelSize.Height;

        int startX = (int)Math.Ceiling((width - 88) / 2.0);
        startX = startX / 8 * 8;  

        int row = type == "BOTTOM" ? (int)Math.Ceiling(0.75 * height) : (int)Math.Ceiling(0.95 * height);

        if (startX < 0) startX = 0;
        if (startX + 88 > width) startX = width - 88;
        if (row < 0) row = 0;
        if (row >= height) row = height - 1;


        bool[] binaryImage = new bool[88];


        int stride = width * 4;

        IntPtr buffer = Marshal.AllocHGlobal(height * stride);

        try
        {
            // Define the region of the image to copy
            Avalonia.PixelRect pixelRect = new(0, 0, width, height);
            bitmap.CopyPixels(pixelRect, buffer, height * stride, stride);




            for (int x = 0; x < 88; x++)
            {
                int index = (row - 1) * stride + ((x + startX) * 4); // RGBA - 4 bytes per pixel
                byte b = Marshal.ReadByte(buffer, index);
                byte g = Marshal.ReadByte(buffer, index + 1);
                byte r = Marshal.ReadByte(buffer, index + 2);

                // Convert pixel to grayscale
                int gray = (int)(0.3 * r + 0.59 * g + 0.11 * b);

                binaryImage[x] = gray >= 128;
            }
        }
        finally
        {
            // Free unmanaged memory
            Marshal.FreeHGlobal(buffer);
        }

        return binaryImage;
    }

    public static string ConvertBinaryToAscii(bool[] binary)
    {
        int len = binary.Length;
        StringBuilder result = new();

        int toDec = 0;
        int bitCount = 0;

        for (int i = 0; i < len; i++)
        {
            toDec = (toDec << 1) | (binary[i] ? 1 : 0);
            bitCount++;

            if (bitCount == 8)
            {
                result.Append((char)toDec);
                toDec = 0;
                bitCount = 0;
            }
        }

        return result.ToString();
    }

    public static string ConvertImageToAsciiFull(Bitmap bitmap)
    {
        bool[] binaryImage = ConvertFullImageToBinary(bitmap);
        return ConvertBinaryToAscii(binaryImage);
    }

    public static void PrintBinaryImage(bool[] binaryImage)
    {
        int counter = 1;
        for (int i = 0; i < binaryImage.Length; i++)
        {
            if ((i + 1) % 96 == 1)
            {
                Console.Write("Row " + counter + ": ");
            }
            Console.Write(binaryImage[i] ? "1" : "0");
            if ((i + 1) % 96 == 0)
            {
                Console.WriteLine();
                counter++;
            }
        }
    }

    public static void PrintAsciiImage(bool[] binaryImage)
    {
        PrintBinaryImage(binaryImage);
        string asciiImage = ConvertBinaryToAscii(binaryImage);
        int start = 0;
        for (int i = 0; i < 103; i++)
        {
            Console.Write("Row " + i + ": " + asciiImage.Substring(start, 12));
            start += 12;

            Console.WriteLine();
        }
    }

    public static void Print5Ascii(bool[] binaryImage)
    {
        string asciiImage = ConvertBinaryToAscii(binaryImage);
        Console.WriteLine(asciiImage);
    }
}
