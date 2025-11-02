using System;
using System.Diagnostics;
using System.IO;

namespace nugetpackCuore
{
    internal class Program
    {
        private static void Main()
        {
            using (StreamWriter outputFile = new StreamWriter("output.log"))
            {
                if (!File.Exists("CuoreUI.Winforms.nuspec") || !File.Exists("nuget.exe"))
                {
                    outputFile.WriteLine("\"CuoreUI.Winforms.nuspec\" or \"nuget.exe\" not found!");
                    return;
                }
                outputFile.WriteLine("needed files are present\n");

                Console.WriteLine("If this is a revision, provide the revision number: (default: 0)");
                Console.Write(" > ");

                int revision = 0;
                int.TryParse(Console.ReadLine(), out revision);
                outputFile.WriteLine($"revision number provided is {revision}");

                outputFile.WriteLine("scanning the nuspec..");
                string[] contents = File.ReadAllLines("CuoreUI.Winforms.nuspec");
                int index = 0;
                foreach (string str in contents)
                {
                    if (str.TrimStart().StartsWith("<version>"))
                    {
                        outputFile.WriteLine("overwriting version line");
                        string revisionStr = revision == 0 ? string.Empty : $".{revision}";
                        contents[index] = $"    <version>{DateTime.Now.ToString("yyyy.MM.dd")}{revisionStr}</version>";
                        outputFile.WriteLine($"\nnew content:\n{contents[index]}\n");
                    }
                    ++index;
                }

                outputFile.WriteLine("saving the new .nuspec");
                File.WriteAllLines("CuoreUI.Winforms.nuspec", contents);

                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "nuget.exe",
                        Arguments = "pack",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.OutputDataReceived += (DataReceivedEventHandler)((sender, args) =>
                {
                    if (args.Data == null)
                        return;
                    outputFile.WriteLine(args.Data);
                });
                process.ErrorDataReceived += (DataReceivedEventHandler)((sender, args) =>
                {
                    if (args.Data == null)
                        return;
                    outputFile.WriteLine(args.Data);
                });
                outputFile.WriteLine("packing into .nupkg..");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                outputFile.WriteLine("finished");
            }
        }
    }
}