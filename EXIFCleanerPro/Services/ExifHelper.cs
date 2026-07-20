using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace EXIFCleanerPro.Services
{
    internal class ExifHelper
    {
        // Method to extract and format EXIF data
        public static string GetExifData(string filePath)
        {
            try
            {
                // Validate if file exists
                if (!File.Exists(filePath))
                    return "File not found.";

                var directories = ImageMetadataReader.ReadMetadata(filePath);

                // Build a formatted string with EXIF data
                StringBuilder exifData = new StringBuilder();
                exifData.AppendLine("EXIF Information:");
                exifData.AppendLine(new string('-', 20));  // Separator line

                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        exifData.AppendLine($"{tag.Name}: {tag.Description}");
                    }
                }

                return exifData.ToString();
            }
            catch (Exception ex)
            {
                return $"Error reading EXIF data: {ex.Message}";
            }
        }

        // Method to clean EXIF data from an image file
        public static bool CleanExifData(string inputFilePath, string outputFilePath)
        {
            try
            {
                if (!File.Exists(inputFilePath))
                    throw new FileNotFoundException("Input file not found.", inputFilePath);

                string extension = Path.GetExtension(inputFilePath).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    throw new NotSupportedException("Only JPG and PNG files are supported for cleaning.");

                if (extension == ".jpg" || extension == ".jpeg")
                {
                    CleanJpegMetadataLosslessly(inputFilePath, outputFilePath);
                    return true;
                }

                using (var sourceImage = Image.FromFile(inputFilePath))
                {
                    ApplyExifOrientation(sourceImage);
                    using Bitmap cleanBitmap = new(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
                    if (sourceImage.HorizontalResolution > 0 && sourceImage.VerticalResolution > 0)
                    {
                        cleanBitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                    }

                    using (Graphics graphics = Graphics.FromImage(cleanBitmap))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(sourceImage, new Rectangle(0, 0, cleanBitmap.Width, cleanBitmap.Height));
                    }

                    cleanBitmap.Save(outputFilePath, ImageFormat.Png);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error cleaning EXIF data: {ex.Message}", ex);
            }
        }

        private static void CleanJpegMetadataLosslessly(string inputFilePath, string outputFilePath)
        {
            ushort? orientation = ReadExifOrientation(inputFilePath);
            using FileStream input = new(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using FileStream output = new(outputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

            if (ReadRequiredByte(input) != 0xFF || ReadRequiredByte(input) != 0xD8)
            {
                throw new InvalidDataException("The input does not contain a valid JPEG start marker.");
            }

            output.WriteByte(0xFF);
            output.WriteByte(0xD8);
            bool orientationWritten = false;

            while (input.Position < input.Length)
            {
                if (ReadRequiredByte(input) != 0xFF)
                {
                    throw new InvalidDataException("Unexpected data before a JPEG marker.");
                }

                int marker;
                do
                {
                    marker = ReadRequiredByte(input);
                }
                while (marker == 0xFF);

                if (!orientationWritten && marker != 0xE0)
                {
                    WriteOrientationSegment(output, orientation);
                    orientationWritten = true;
                }

                if (marker == 0xDA)
                {
                    output.WriteByte(0xFF);
                    output.WriteByte((byte)marker);
                    input.CopyTo(output);
                    return;
                }

                if (marker == 0xD9)
                {
                    output.WriteByte(0xFF);
                    output.WriteByte((byte)marker);
                    return;
                }

                if (marker == 0x01 || marker is >= 0xD0 and <= 0xD7)
                {
                    output.WriteByte(0xFF);
                    output.WriteByte((byte)marker);
                    continue;
                }

                int segmentLength = (ReadRequiredByte(input) << 8) | ReadRequiredByte(input);
                if (segmentLength < 2)
                {
                    throw new InvalidDataException("A JPEG segment has an invalid length.");
                }

                byte[] payload = new byte[segmentLength - 2];
                input.ReadExactly(payload);
                if (!ShouldKeepJpegSegment(marker, payload))
                {
                    continue;
                }

                output.WriteByte(0xFF);
                output.WriteByte((byte)marker);
                WriteBigEndianUInt16(output, segmentLength);
                output.Write(payload);
            }

            throw new InvalidDataException("The JPEG ended before its scan data or end marker.");
        }

        private static bool ShouldKeepJpegSegment(int marker, ReadOnlySpan<byte> payload)
        {
            if (marker == 0xE0 || marker == 0xEE)
            {
                return true;
            }

            if (marker == 0xE2)
            {
                return payload.StartsWith("ICC_PROFILE\0"u8);
            }

            if (marker == 0xE1 || marker is >= 0xE3 and <= 0xED || marker == 0xEF || marker == 0xFE)
            {
                return false;
            }

            return true;
        }

        private static ushort? ReadExifOrientation(string filePath)
        {
            const int OrientationPropertyId = 0x0112;
            using Image image = Image.FromFile(filePath);
            if (!image.PropertyIdList.Contains(OrientationPropertyId))
            {
                return null;
            }

            PropertyItem? property = image.GetPropertyItem(OrientationPropertyId);
            if (property?.Value is null || property.Value.Length < 2)
            {
                return null;
            }

            ushort orientation = BitConverter.ToUInt16(property.Value, 0);
            return orientation is >= 2 and <= 8 ? orientation : null;
        }

        private static void WriteOrientationSegment(Stream output, ushort? orientation)
        {
            if (orientation is null)
            {
                return;
            }

            byte[] payload =
            [
                (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0,
                (byte)'I', (byte)'I', 42, 0, 8, 0, 0, 0,
                1, 0,
                0x12, 0x01, 3, 0, 1, 0, 0, 0,
                (byte)orientation.Value, 0, 0, 0,
                0, 0, 0, 0
            ];
            output.WriteByte(0xFF);
            output.WriteByte(0xE1);
            WriteBigEndianUInt16(output, payload.Length + 2);
            output.Write(payload);
        }

        private static int ReadRequiredByte(Stream stream)
        {
            int value = stream.ReadByte();
            return value >= 0 ? value : throw new EndOfStreamException("Unexpected end of JPEG data.");
        }

        private static void WriteBigEndianUInt16(Stream stream, int value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void ApplyExifOrientation(Image image)
        {
            const int OrientationPropertyId = 0x0112;
            if (!image.PropertyIdList.Contains(OrientationPropertyId))
            {
                return;
            }

            PropertyItem? orientationProperty = image.GetPropertyItem(OrientationPropertyId);
            if (orientationProperty?.Value is null || orientationProperty.Value.Length < 2)
            {
                return;
            }

            ushort orientation = BitConverter.ToUInt16(orientationProperty.Value, 0);
            RotateFlipType transform = orientation switch
            {
                2 => RotateFlipType.RotateNoneFlipX,
                3 => RotateFlipType.Rotate180FlipNone,
                4 => RotateFlipType.Rotate180FlipX,
                5 => RotateFlipType.Rotate90FlipX,
                6 => RotateFlipType.Rotate90FlipNone,
                7 => RotateFlipType.Rotate270FlipX,
                8 => RotateFlipType.Rotate270FlipNone,
                _ => RotateFlipType.RotateNoneFlipNone
            };
            if (transform != RotateFlipType.RotateNoneFlipNone)
            {
                image.RotateFlip(transform);
            }
        }
    }
}
