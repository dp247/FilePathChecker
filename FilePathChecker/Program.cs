using System;
using static System.Console;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Konsole;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace FilePathChecker
{
    public class ConsoleSpinner
    {
        int counter;

        public void Turn()
        {
            counter++;
            switch (counter % 4)
            {
                case 0: Console.Write("/"); counter = 0; break;
                case 1: Console.Write("-"); break;
                case 2: Console.Write("\\"); break;
                case 3: Console.Write("|"); break;
            }
            //Thread.Sleep(100);
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
    }

    class Program
    {
        #region Window Config 
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        #endregion

        static string path;
        public static int lineCount = 0;
        static string filename;

        static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.:@\s\-\\%]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        //TODO: Add custom file drop locations using application settings

        /// <summary>
        /// Validates a .txt file in the user's My Documents folder
        /// </summary>
        static void CheckFile()
        {
            Console.WriteLine("Please place the file in My Documents, then specify the name:");
            filename = ReadLine();

            if (String.IsNullOrWhiteSpace(filename))
            {
                filename = "locs.txt";
            }
            if (!filename.EndsWith(".txt"))
            {
                filename = String.Concat(filename, ".txt");
            }

            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);

            if (!File.Exists(path))
            {
                Console.WriteLine($"\n{filename} does not exist! Press any key to exit!");
                Console.ReadKey();
                Environment.Exit(0);
            }

            else
            {
                WriteLine($"\n{filename} loaded!");
            }

            using (var reader = new StreamReader(path))
            {
                while (reader.ReadLine() != null)
                {
                    ++lineCount;
                }
                Console.WriteLine($"{lineCount.ToString()} lines found. Press any key to continue!");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void Main(string[] args)
        {
            int counter = 0;
            string line;
            List<string> validFiles = new List<string>();
            List<string> invalidFiles = new List<string>();
            Dictionary<string, DateTime> myFiles = new Dictionary<string, DateTime>();

            #region Window Handling
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                //DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND);
                //DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }
            #endregion

            CheckFile();

            //TODO: Check if output files already exist before creating again
            string validFileExportName = String.Concat(filename, "_", "valid.txt");
            string validFileAndDateExportName = String.Concat(filename, "_", "valid_with_dates.txt");
            string invalidFileExportName = String.Concat(filename, "_", "invalid.txt");

            var con = Window.OpenBox("File Path Checker", 110, 27);
            StreamReader file = new StreamReader(path);

            con.WriteLine($"Opened file: {path}\n");
            con.WriteLine(ConsoleColor.DarkYellow, "Processed   |   Valid   |   Invalid");

            //Time for some nice UI elements
            var verbose = new Window(con, 90, 20, ConsoleColor.DarkYellow, ConsoleColor.Black).Concurrent();
            var progress = new ProgressBar(con, lineCount, 10);
            var spinner = new ConsoleSpinner();

            while ((line = file.ReadLine()) != null)
            {
                var cleanLine = CleanInput(line);
                Debug.WriteLine($"Regular line: {line}");
                Debug.WriteLine($"Clean line: {cleanLine}");


                if (!File.Exists(cleanLine))
                {
                    invalidFiles.Add(line);
                    Debug.WriteLine("INVALID");
                }
                else
                {
                    validFiles.Add(cleanLine);
                    Debug.WriteLine("VALID");
                    FileInfo fi = new FileInfo(cleanLine);
                    myFiles.Add(string.Concat(fi.Directory.ToString().Replace("\\", String.Empty), " - ", fi.Name), fi.LastWriteTime);
                }


                ++counter;
                progress.Refresh(counter, "Progress");
                verbose.WriteLine($"{counter.ToString()}    |    {validFiles.Count.ToString()}    |    {invalidFiles.Count.ToString()}");
                spinner.Turn();
            }
            file.Close();
            verbose.Clear();
            verbose.WriteLine($"{counter.ToString()}    |    {validFiles.Count.ToString()}    |    {invalidFiles.Count.ToString()}");

            var validWindow = new Window(con, 60, 4, 33, 4, ConsoleColor.White, ConsoleColor.DarkGreen).Concurrent();
            var invalidWindow = new Window(con, 60, 8, 33, 4, ConsoleColor.White, ConsoleColor.DarkRed).Concurrent();
            var statusWindow = new Window(con, 60, 12, 33, 4, ConsoleColor.Black, ConsoleColor.White).Concurrent();
            DateTime minDate = DateTime.Now, maxDate = new DateTime(2000, 01, 01);

            validWindow.WriteLine("Valid");
            validWindow.WriteLine("------");
            validWindow.WriteLine($"{validFiles.Count.ToString()} valid files");
            invalidWindow.WriteLine("Invalid");
            invalidWindow.WriteLine("------");
            invalidWindow.WriteLine($"{invalidFiles.Count.ToString()} invalid files");

            File.WriteAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), validFileExportName), validFiles);
            File.WriteAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), invalidFileExportName), invalidFiles);

            using (StreamWriter writer = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), validFileAndDateExportName)))
                foreach (var item in myFiles)
                {
                    writer.WriteLine("[{0} | {1}]", item.Key, item.Value);
                }

            foreach (var item in myFiles)
            {
                if (item.Value < minDate)
                {
                    minDate = item.Value;
                }
                if (item.Value > maxDate)
                {
                    maxDate = item.Value;
                }
            }
            statusWindow.WriteLine("Results exported!");
            if (validFiles.Count > 0)
            {
                statusWindow.WriteLine($"Files modified between {minDate.ToShortDateString()}and {maxDate.ToShortDateString()}.");
            }
            else
            {
                statusWindow.WriteLine($"No valid file paths found!");
            }

            Console.WriteLine("Done! Press any key to exit application.");
            Console.ReadLine();
        }
    }
}