//css_ref System.IO.Compression
//css_ref System.IO.Compression.FileSystem
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

class Script
{
	const string VSIXNAME = "HtmlView";
	const string VSIXID = "HtmlView.MISoftware.57ef93d2-bc83-4264-b2f4-c9a5eb66faa1";

	static readonly string DIR_RI = @"D:\Projetos-VSX\HtmlView\ReleaseInfo";

	[STAThread]
	static public void Main(string[] args)
	{
		Environment.CurrentDirectory = DIR_RI;

		// Copy VSIX
		File.Delete("HtmlView.vsix");
		File.Copy(@"..\HtmlView\bin\x86\Release\HtmlView.vsix", "HtmlView.vsix");

		// Pack CEF
		PackCEF();

		#region Uninstall / Install VSIX
		string log = "/logFile:vsixlog.txt ";
		string logfile = Environment.ExpandEnvironmentVariables("%TEMP%") + "\\vsixlog.txt";
		File.Delete(logfile);

		Console.WriteLine("### Uninstalling VSIX - " + VSIXNAME);
		{
			//SpawnProcess(@"C:\Program Files (x86)\Microsoft Visual Studio " + VS_NUM + @".0\Common7\IDE\VSIXInstaller.exe",
			SpawnProcess(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\VSIXInstaller.exe",
				log + "/quiet /uninstall:" + VSIXID, true);// ignores error

			Console.WriteLine(File.ReadAllText(logfile));
			File.Delete(logfile);
		}

		Console.WriteLine("### Installing VSIX - " + VSIXNAME);
		{
			string vsix_path = VSIXNAME + ".vsix";

			SpawnProcess(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\VSIXInstaller.exe",
				log + "/quiet " + vsix_path);

			Console.WriteLine(File.ReadAllText(logfile));
			File.Delete(logfile);
		}
		#endregion

		// Start VS2015
		Console.WriteLine("### Starting VSIX");
		SpawnProcess(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe", @"D:\Projetos-VSX\HtmlView\HtmlView.sln /log", false, false);
		
		File.Delete("HtmlView.vsix.zip");
		File.Move("HtmlView.vsix", "HtmlView.vsix.zip");
	}

	private static void PackCEF()
	{
		var vsix_zip = ZipFile.Open(VSIXNAME + ".vsix", ZipArchiveMode.Update);
		string dir = Path.GetFullPath(@"..\HtmlView\bin\x86\Release\");

		// [Content_Types].xml
		vsix_zip.GetEntry("[Content_Types].xml").Delete();
		vsix_zip.CreateEntryFromFile("[Content_Types].xml", "[Content_Types].xml");

		vsix_zip.Dispose();
		return;

		// CEF files
		string[] cef_files = new[]
		{
			"CefSharp.BrowserSubprocess.exe",
			"CefSharp.BrowserSubprocess.Core.dll",
			"d3dcompiler_47.dll",
			"libcef.dll",
			"icudtl.dat",
			"cef.pak",
			"cef_100_percent.pak",
			"cef_200_percent.pak",
			"chrome_elf.dll",
			"devtools_resources.pak",
			"cef_extensions.pak",
			"libEGL.dll",
			"libGLESv2.dll",
			"natives_blob.bin",
			"snapshot_blob.bin"
		};

		foreach(var file in cef_files)
		{
			Debug.Assert(File.Exists(dir + file));

			string zip_file = file;
			vsix_zip.CreateEntryFromFile(dir + file, zip_file);
			Console.WriteLine("Packed file " + zip_file);
		}

		foreach(var item in Directory.EnumerateFiles(dir + "locales"))
		{
			string zip_file = "locales/" + Path.GetFileName(item);
			vsix_zip.CreateEntryFromFile(item, zip_file);
			Console.WriteLine("Packed file " + zip_file);
		}
		vsix_zip.Dispose();
	}

	static public void SpawnProcess(string exe, string args, bool ignore_error = false, bool wait = true)
	{
		var startInfo = new ProcessStartInfo(exe, args)
		{
			FileName = exe,
			Arguments = args,
			UseShellExecute = false,
			WorkingDirectory = DIR_RI
		};

		var p = Process.Start(startInfo);
		if(wait)
		{
			p.WaitForExit();

			if(p.ExitCode != 0 && ignore_error == false)
			{
				Console.ForegroundColor = ConsoleColor.Red;

				string msg = exe + ' ' + args;
				Console.WriteLine();
				Console.WriteLine("-------------------------");
				Console.WriteLine("FAILED: " + msg);
				Console.WriteLine("EXIT CODE: " + p.ExitCode);
				Console.WriteLine("Press ENTER to exit");
				Console.WriteLine("-------------------------");

				Console.ReadLine();
				Environment.Exit(0);
			}
		}
	}
}