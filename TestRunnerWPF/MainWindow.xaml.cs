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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using CefSharp.Wpf;
using HtmlView;

namespace TestRunnerWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private CEFHost _cefhost;
		private ChromiumWebBrowser _browser;

		public MainWindow()
		{
			InitializeComponent();

			_cefhost = new CEFHost();
			_cefhost.CreateBrowserUI();
			_browser = _cefhost._browser;
			grid.Children.Add(_cefhost._mc);

			Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
			{
				var file = new StackTrace(true).GetFrame(0).GetFileName();
				var dir = Path.GetFullPath(Path.GetDirectoryName(file));
				var url = ("file://" + dir + @"\input\test.html").Replace('\\', '/');

				_cefhost.LoadPage(url);
				//c.ShowDevTools();
			}), DispatcherPriority.ApplicationIdle, null);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_browser.ReloadCommand.Execute(null);
		}
	}
}
