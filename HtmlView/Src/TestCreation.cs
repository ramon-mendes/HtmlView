#if DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace HtmlView
{
	[Export(typeof(IWpfTextViewCreationListener))]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Interactive)]
	class TestCreation : IWpfTextViewCreationListener
	{
		public void TextViewCreated(IWpfTextView textView)
		{
			string dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(EditorMarginFactory)).Location) + '\\';

			if(textView.TextDataModel.DocumentBuffer.ContentType.IsOfType("htmlx"))
			{
				//string cwd = Environment.CurrentDirectory;
				string[] files = new[]
				{
					dir + "libcef.dll",
					dir + "CefSharp.BrowserSubprocess.exe",
					dir + "CefSharp.BrowserSubprocess.Core.dll"
				};

				if(files.Any(f => !File.Exists(f)))
				//if(true)
				{
					Process.Start("explorer", dir);

					// copie os arquivos CEF para a pasta aberta antes de continuar!
					Debugger.Break();
				}
			}
		}
	}
}
#endif