using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace HtmlView
{
	/// <summary>
	/// Interaction logic for MarginControl.xaml
	/// </summary>
	public partial class MarginControl : UserControl
	{
		private CEFHost _host;
		private bool _console = true;
		private GridLength _console_height = new GridLength(160);

		public MarginControl(CEFHost host)
		{
			_host = host;

			InitializeComponent();

			btnClear.Click += BtnClear_Click;
			btnDevTools.Click += BtnDevTools_Click;
			btnConsole.Click += BtnConsole_Click;
		}

		public void SetConsole(bool b)
		{
			if(b != _console)
				ToggleConsole();
		}
		public void ToggleConsole()
		{
			_console = !_console;
			if(_console)
			{
				dock.RowDefinitions[1].Height = new GridLength(5);
				dock.RowDefinitions[2].Height = _console_height;

				colClear1.Visibility = Visibility.Visible;
				colClear2.Visibility = Visibility.Visible;
			} else {
				_console_height = dock.RowDefinitions[2].Height;
				dock.RowDefinitions[1].Height = new GridLength(0);
				dock.RowDefinitions[2].Height = new GridLength(0);
				
				colClear1.Visibility = Visibility.Collapsed;
				colClear2.Visibility = Visibility.Collapsed;
			}
		}

		private void BtnClear_Click(object sender, RoutedEventArgs e)
		{
			consoleBox.Text = "";
		}
		private void BtnConsole_Click(object sender, RoutedEventArgs e)
		{
			ToggleConsole();
		}
		private void BtnDevTools_Click(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(() => _host.ShowDevTools()));
		}
	}
}