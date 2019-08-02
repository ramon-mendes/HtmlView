using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Internals;
using CefSharp.Wpf;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HtmlView
{
	class EditorMargin : MarginBase
	{
		private CEFHost _cefhost;
		private bool _active = false;
		private bool _cfg_console;
		private ITextDocument Document;
		private string _filepath;

		// Contructor
		public EditorMargin(ITextView textView, ITextDocument document)
		{
			_filepath = "file://" + document.FilePath.Replace('\\', '/');

			Document = document;
			document.FileActionOccurred += Doc_FileActionOccurred;

			CheckUserEnabled();

			if(_active)
			{
				Dispatcher.BeginInvoke((Action)ReloadPage, DispatcherPriority.ApplicationIdle, null);
			}
		}

		// Internal ----------------------------------------------------------------
		private void CheckUserEnabled()
		{
			string header = Document.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(0).GetText().Trim();

			if(header.Contains("HtmlView:off"))
				_active = false;
			else
				_active = Path.GetFileName(Document.FilePath).Contains("unittest") || (header.StartsWith("<!--") && header.EndsWith("-->") && header.Contains("HtmlView"));

			if(_active)
			{
				_cfg_console = !header.Contains("console:off");
			}

			SetVisible(_active);
		}

		private void ReloadPage()
		{
			if(_active)
			{
				_cefhost._mc.SetConsole(_cfg_console);
				_cefhost.LoadPage(_filepath);

				//Utils.ReleaseOutputString("ReloadPage: " + _filepath);
			}
		}

		private void Doc_FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
		{
			switch(e.FileActionType)
			{
				case FileActionTypes.ContentSavedToDisk:
					CheckUserEnabled();
					ReloadPage();
					break;

				case FileActionTypes.DocumentRenamed:
					ReloadPage();
					break;

				case FileActionTypes.ContentLoadedFromDisk:
					// from file reload dialog
					CheckUserEnabled();
					ReloadPage();
					break;
			}
		}

		// Overrides ----------------------------------------------------------------
		protected override FrameworkElement CreatePreviewControl()
		{
			_cefhost = new CEFHost();
			return _cefhost.CreateBrowserUI();
		}

		protected override void DisposePreviewControl()
		{
			_cefhost.Dispose();
		}
	}

	/*class DevToolsWndHost : HwndHost
	{
		private EditorMargin _margin;

		public DevToolsWndHost(EditorMargin margin)
		{
			_margin = margin;
		}

		protected override HandleRef BuildWindowCore(HandleRef hwndParent)
		{
			var host = _margin._iwb.GetBrowser().GetHost();
			var wi = new WindowInfo();
			wi.SetAsChild(hwndParent.Handle, 0, 0, 100, 100);
			host.ShowDevTools(wi);
			var wh = wi.WindowHandle;

			return new HandleRef(this, wh);
		}

		protected override void DestroyWindowCore(HandleRef hwnd)
		{
			var host = _margin._iwb.GetBrowser().GetHost();
			host.CloseDevTools();
		}
	}*/

	abstract class MarginBase : DockPanel, IWpfTextViewMargin
	{
		FrameworkElement _previewControl;

		private bool _created = false;
		private bool _isDisposed = false;
		private KeystrokeThief _thief = new KeystrokeThief();
		

		protected void SetVisible(bool show)
		{
			if(show)
				CreateMarginControls();
			Visibility = show ? Visibility.Visible : Visibility.Collapsed;
		}

		protected abstract FrameworkElement CreatePreviewControl();
		protected abstract void DisposePreviewControl();

		protected void CreateMarginControls()
		{
			if(_created)
				return;
			_created = true;

			this.GotFocus += MarginBase_GotFocus;
			this.LostFocus += MarginBase_LostFocus;

			const int WIDTH = 500;

			Grid grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Pixel) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(WIDTH, GridUnitType.Pixel) });
			grid.RowDefinitions.Add(new RowDefinition());

			_previewControl = CreatePreviewControl();
			Debug.Assert(_previewControl != null);
			grid.Children.Add(_previewControl);

			Grid.SetColumn(_previewControl, 2);
			Grid.SetRow(_previewControl, 0);

			GridSplitter splitter = new GridSplitter();
			splitter.Width = 5;
			splitter.ResizeDirection = GridResizeDirection.Columns;
			splitter.VerticalAlignment = VerticalAlignment.Stretch;
			splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
			splitter.DragCompleted += Splitter_DragCompleted;

			grid.Children.Add(splitter);
			Grid.SetColumn(splitter, 1);
			Grid.SetRow(splitter, 0);

			Children.Add(grid);
		}

		private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
		}

		private void MarginBase_GotFocus(object sender, RoutedEventArgs e)
		{
			_thief.StartStealing();
		}

		private void MarginBase_LostFocus(object sender, RoutedEventArgs e)
		{
			_thief.StopStealing();
		}

		private void ThrowIfDisposed()
		{
			if(_isDisposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		#region IWpfTextViewMargin Members

		public FrameworkElement VisualElement { get { return this; } }

		#endregion

		#region ITextViewMargin Members

		public double MarginSize
		{
			// Since this is a horizontal margin, its width will be bound to the width of the text view.
			// Therefore, its size is its height.
			get
			{
				ThrowIfDisposed();
				return ActualHeight;
			}
		}

		public bool Enabled { get { ThrowIfDisposed(); return true; } }

		public ITextViewMargin GetTextViewMargin(string marginName)
		{
			ThrowIfDisposed();
			return (marginName == GetType().Name) ? this : null;
		}


		///<summary>Releases all resources used by the MarginBase.</summary>
		public void Dispose()
		{
			if(_created)
				DisposePreviewControl();

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		///<summary>Releases the unmanaged resources used by the MarginBase and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
			}
		}
		#endregion
	}
}