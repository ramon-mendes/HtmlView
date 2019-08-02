using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Internals;
using CefSharp.Wpf;

namespace HtmlView
{
	public class CEFHost : IDownloadHandler, IDisposable
	{
		public ChromiumWebBrowser _browser;
		public IWebBrowser _iwb;
		public MarginControl _mc;

		// CEF checks (MUST not initialize CEF to avoid memory usage)
		static CEFHost()
		{
			var settings = GetCEFSettings();
			try
			{
				StopwatchAuto sw = new StopwatchAuto();
				DependencyChecker.AssertAllDependenciesPresent(null, settings.LocalesDirPath, settings.ResourcesDirPath, false, settings.BrowserSubprocessPath);
				sw.StopAndLog();
			}
			catch(Exception ex)
			{
				Utils.MsgBox("[HtmlView] CEFSharp initialization check failed: \n\n" + ex);
			}
		}

		static CefSettings GetCEFSettings()
		{
			CefSharpSettings.LegacyJavascriptBindingEnabled = true;

			string path = Assembly.GetAssembly(typeof(EditorMargin)).Location;
			string dir = Path.GetDirectoryName(path) + '\\';// + "\\CEF";
			return new CefSettings()
			{
				LocalesDirPath = dir + "locales",
				ResourcesDirPath = dir,
				BrowserSubprocessPath = dir + "CefSharp.BrowserSubprocess.exe",
#if DEBUG
				LogSeverity = LogSeverity.Verbose
#endif
			};
		}


		// HOSTED interface -----------------------------------------------------------------------
		public void LoadPage(string url)
		{
			_mc.consoleBox.Text = string.Format("HtmlView - Chromium: {0}, CEF: {1}\n", Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion);
			_browser.Address = url;
		}

		public MarginControl CreateBrowserUI()
		{
			if(!Cef.IsInitialized && !Cef.Initialize(GetCEFSettings()))// CEF is delay loaded this way
			{
				string path = Assembly.GetAssembly(typeof(EditorMargin)).Location;
				string dir = Path.GetDirectoryName(path) + '\\';

				Utils.MsgBox("Could not Initialize CEFSharp: " + dir);
				return null;
			}

			_browser = new ChromiumWebBrowser();
			_browser.RegisterJsObject("HtmlView", new BoundObject());
			_browser.LoadError += WebBrowser_LoadError;
			_browser.LoadingStateChanged += WebBrowser_LoadingStateChanged;
			_browser.IsBrowserInitializedChanged += WebBrowser_IsBrowserInitializedChanged;
			// settings
			_browser.BrowserSettings.WebSecurity = CefState.Disabled;
			_browser.BrowserSettings.FileAccessFromFileUrls = CefState.Disabled;
			// WPF events
			_browser.MouseLeftButtonDown += WebBrowser_MouseLeftButtonDown;
			_browser.DownloadHandler = this;

			Grid.SetRow(_browser, 0);

			_mc = new MarginControl(this);
			_mc.dock.Children.Add(_browser);
			_mc.PreviewKeyDown += Margin_PreviewKeyDown;
			
			return _mc;
		}


		// Internal ----------------------------------------------------------------
		private class BoundObject
		{
			public bool active { get { return true; } }
		}

		public void ShowDevTools(Point? pt = null)
		{
			//_dt = new DevToolsWndHost(this);
			//_mc.inspectorGrid.Children.Add(_dt);

			var host = _iwb.GetBrowser().GetHost();
			host.CloseDevTools();

			var wi = new WindowInfo();
			wi.SetAsPopup(new WindowInteropHelper(Window.GetWindow(_mc)).Handle, "");

			if(pt != null)
				host.ShowDevTools(wi, (int)pt.Value.X, (int)pt.Value.Y);
			else
				host.ShowDevTools(wi);
		}


		// CEF handlers ----------------------------------------------------------------
		#region
		private List<string> _log_messages = new List<string>();

		private void WebBrowser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
            if((bool)e.NewValue==true)
            {
			    Debug.Assert(_iwb == null && _mc.IsInitialized);
			    _iwb = _browser.WebBrowser;
			    Debug.Assert(_iwb != null);
			    _iwb.ConsoleMessage += WebBrowser_ConsoleMessage;
            }
            else
            {
				// shutdown
				_iwb.Dispose();
				_iwb = null;
				_browser = null;
			}
		}

		private void WebBrowser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
		{
			lock(_log_messages)
			{
				_log_messages.Add(e.Message + '\n');

				if(_log_messages.Count == 1)
				{
					_mc.Dispatcher.BeginInvoke(new Action(() =>
					{
						lock(_log_messages)
						{
							_mc.consoleBox.Text += String.Concat(_log_messages);
							_log_messages.Clear();
						}
						_mc.consoleBox.ScrollToEnd();
					}), DispatcherPriority.ApplicationIdle, null);
				}
			}
		}
		private void WebBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
		{
			_mc.Dispatcher.Invoke(() => _mc.lblAddr.Text = _browser.Address);
		}
		private void WebBrowser_LoadError(object sender, LoadErrorEventArgs e)
		{
#if DEBUG
			if(e.ErrorCode != CefErrorCode.Aborted)
				Debugger.Break();
#endif
		}
		#endregion

		// WPF handlers ----------------------------------------------------------------
		#region
		private void Margin_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.SystemKey == Key.F3 || e.Key == Key.F3)
			{
				_mc.ToggleConsole();
				e.Handled = true;
			}
			else if(e.SystemKey == Key.F12 || e.Key == Key.F12)
			{
				ShowDevTools();
				e.Handled = true;
			}
		}
		private void WebBrowser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
			{
				ShowDevTools(e.GetPosition(_mc));
				e.Handled = true;
			}
		}
		#endregion

		// Overrides
		public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			if(!callback.IsDisposed)
			{
				using(callback)
				{
					callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
				}
			}
		}

		public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
		{
		}

		public void Dispose()
		{
			_browser.Dispose();
			Debug.Assert(_browser == null);
			Debug.Assert(_iwb == null);
		}
	}
}