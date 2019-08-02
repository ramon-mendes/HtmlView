using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HtmlView
{
	static class Utils
	{
		private static DTE2 _dte;

		public static DTE2 DTE
		{
			get
			{
				if(_dte == null)
					_dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

				return _dte;
			}
		}

		public static void MsgBox(string msg)
		{
			VsShellUtilities.ShowMessageBox(
					ServiceProvider.GlobalProvider,
					msg,
					Vsix.Name,
					OLEMSGICON.OLEMSGICON_INFO,
					OLEMSGBUTTON.OLEMSGBUTTON_OK,
					OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
		}

		public static void DebugOutputString(string output)
		{
#if DEBUG
			ReleaseOutputString(output);
#endif
		}

		public static void ReleaseOutputString(string output)
		{
			try
			{
				if(DTE == null)
				{
					Debug.WriteLine(output);
					return;
				}

				const string OUTPUT_WINDOW_NAME = "General";
				Window window = DTE.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
				OutputWindow outputWindow = (OutputWindow)window.Object;
				OutputWindowPane outputWindowPane = null;

				for(uint i = 1; i <= outputWindow.OutputWindowPanes.Count; i++)
				{
					if(outputWindow.OutputWindowPanes.Item(i).Name.Equals(OUTPUT_WINDOW_NAME, StringComparison.CurrentCultureIgnoreCase))
					{
						outputWindowPane = outputWindow.OutputWindowPanes.Item(i);
						break;
					}
				}

				if(outputWindowPane == null)
					outputWindowPane = outputWindow.OutputWindowPanes.Add(OUTPUT_WINDOW_NAME);

				outputWindow.ActivePane.Activate();
				outputWindow.ActivePane.OutputString(output + "\n");
			}
			catch(Exception)
			{
			}
		}
	}

	class StopwatchAuto
	{
		private Stopwatch _sw = new Stopwatch();

		public StopwatchAuto()
		{
			_sw.Start();
		}

		public void StopAndLog(string what = null)
		{
			_sw.Stop();

			if(what == null)
			{
				StackTrace stackTrace = new StackTrace();
				what = stackTrace.GetFrame(1).GetMethod().Name + "()";
			}
			Utils.DebugOutputString(what + " took " + _sw.ElapsedMilliseconds + "ms");
		}

		public void StopAndLogRelease(string what = null)
		{
			_sw.Stop();

			if(what == null)
			{
				StackTrace stackTrace = new StackTrace();
				what = stackTrace.GetFrame(1).GetMethod().Name + "()";
			}
			Utils.ReleaseOutputString(what + " took " + _sw.ElapsedMilliseconds + "ms");
		}
	}

}