
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using System.Management;

class Program
{
    // Dictionary to store the last printed times of files
    static Dictionary<string, DateTime> printedFiles = new Dictionary<string, DateTime>();

    static void Main(string[] args)
    {
        try
        {
            string scriptDir = AppDomain.CurrentDomain.BaseDirectory;
            string configFilePath = Path.Combine(scriptDir, "config.json");

            if (!File.Exists(configFilePath))
            {
                Console.WriteLine("Configuration file 'config.json' not found.");
                return;
            }
            Console.WriteLine($"Configuration file: {configFilePath}");

            string configFileContent = File.ReadAllText(configFilePath, System.Text.Encoding.UTF8);
            var config = JsonConvert.DeserializeObject<Config>(configFileContent);
            //var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFilePath));

            // Default image directory
            string defaultImageDir = Path.Combine(scriptDir, "images");
            EnsureDirectoryExists(defaultImageDir);

            // Get user inputs from JSON config file
            string folderPath = config.folder_path ?? defaultImageDir;
            Console.WriteLine($"Watch for new images: {folderPath}");
            int x = config.x;
            int y = config.y;
            int width = config.width;
            int height = config.height;

            // Default image directory
            //string scriptDir = AppDomain.CurrentDomain.BaseDirectory;
            //string defaultImageDir = Path.Combine(scriptDir, "images");
            //EnsureDirectoryExists(defaultImageDir);

            // Get user inputs
            //Console.Write($"Enter the folder path to watch for new images (default: {defaultImageDir}): ");
            //string folderPath = Console.ReadLine();
            //if (string.IsNullOrEmpty(folderPath)) folderPath = defaultImageDir;
            //
            //Console.Write("Enter the x position (points): ");
            //int x = int.TryParse(Console.ReadLine(), out x) ? x : 0;
            //
            //Console.Write("Enter the y position (points): ");
            //int y = int.TryParse(Console.ReadLine(), out y) ? y : 0;
            //
            //Console.Write("Enter the width (points): ");
            //int width = int.TryParse(Console.ReadLine(), out width) ? width : 200;
            //
            //Console.Write("Enter the height (points): ");
            //int height = int.TryParse(Console.ReadLine(), out height) ? height : 200;

            // Create event handler
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = folderPath;
            watcher.Filter = "*.*"; // Watch all files

            watcher.Created += (sender, e) => ProcessFile(e.FullPath, x, y, width, height);
            //watcher.Changed += (sender, e) => ProcessFile(e.FullPath, x, y, width, height);
            //watcher.Renamed += (sender, e) => ProcessFile(e.FullPath, x, y, width, height);

            watcher.EnableRaisingEvents = true;

            // Start observing
            Console.WriteLine("Script is now waiting for new images to be added to the folder.");
            Console.WriteLine("Press Ctrl+C to stop the script.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occured: \n\r{ex.Message}");
        }
        // Keep the application running
        while (true)
        {
            Thread.Sleep(1000);
        }
    }

    static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    static void ProcessFile(string filePath, int x, int y, int width, int height)
    {
        // Ensure the file exists before proceeding
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File {filePath} was deleted or does not exist. Skipping...");
            return;
        }

        string fileExtension = Path.GetExtension(filePath).ToLower();

        // Process only JPEG files
        if (fileExtension == ".jpg" || fileExtension == ".jpeg")
        {
            Console.WriteLine($"Detected new or modified file: {filePath}");

            // Add a 1-second delay before printing
            Thread.Sleep(1000);

            // Check if the file has been printed within the last 10 seconds
            if (printedFiles.TryGetValue(filePath, out DateTime lastPrintedTime))
            {
                if ((DateTime.Now - lastPrintedTime).TotalSeconds < 10)
                {
                    Console.WriteLine($"File {filePath} was printed less than 10 seconds ago. Skipping...");
                    return;
                }
            }

            // Update the last printed time
            printedFiles[filePath] = DateTime.Now;

            // Convert image to PDF and print it
            ConvertImageToPdfAndPrint(filePath, x, y, width, height);
        }
    }

    static void ConvertImageToPdfAndPrint(string imagePath, int x, int y, int width, int height)
    {
        try
        {
            // Create a temporary file for the output PDF
            string tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

            // Initialize PDF writer and document
            using (var writer = new PdfWriter(tempPdfPath))
            {
                using (var pdf = new PdfDocument(writer))
                {
                    using (var document = new Document(pdf))
                    {
                        // Load image
                        ImageData imageData = ImageDataFactory.Create(imagePath);
                        iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData);

                        // Set image size and position
                        image.SetFixedPosition(x, y);
                        image.ScaleToFit(width, height);

                        // Add image to the document
                        document.Add(image);
                    }
                }
            }

            Console.WriteLine($"Printable PDF created at {tempPdfPath}");

            // Attempt to print the PDF using Ghostscript
            PrintPdfWithGhostscript(tempPdfPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file {imagePath}: {ex.Message}");
        }
    }

    static void PrintPdfWithGhostscript(string pdfPath)
    {
        try
        {
            // Ghostscript command and arguments
            string gsArguments = $"-dPrinted -dBATCH -dNOSAFER -dNOPAUSE -dNOPROMPT -q -sDEVICE=mswinpr2 -sOutputFile=%printer%\"{GetDefaultPrinter()}\" \"{pdfPath}\"";

            ProcessStartInfo gsProcessInfo = new ProcessStartInfo("gswin64c.exe", gsArguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process gsProcess = new Process { StartInfo = gsProcessInfo })
            {
                gsProcess.Start();
                gsProcess.WaitForExit();

                if (gsProcess.ExitCode != 0)
                {
                    string error = gsProcess.StandardError.ReadToEnd();
                    Console.WriteLine($"Failed to print PDF: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to print PDF: {ex.Message}");
        }
    }

    static string GetDefaultPrinter()
    {
        try
        {
            // Query for the default printer
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Default = TRUE"))
            {
                foreach (var printer in searcher.Get())
                {
                    return printer["Name"].ToString();
                }
            }
            throw new InvalidOperationException("No default printer found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting default printer: {ex.Message}");
            throw;
        }
    }


    // JSON Config class
    public class Config
    {
        public string folder_path { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}
