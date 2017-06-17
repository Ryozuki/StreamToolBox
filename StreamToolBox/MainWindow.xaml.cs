using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.IO;

namespace StreamToolBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public System.Timers.Timer aTimer = new System.Timers.Timer();

        public MainWindow()
        {
            InitializeComponent();

            const string VERSION = "0.0.1";

            creditLabel.Content = ((string)creditLabel.Content).Replace("%v", VERSION);

            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.dateformat))
                Properties.Settings.Default.dateformat = textBoxDate.Text;
            else
                textBoxDate.Text = Properties.Settings.Default.dateformat;
            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.timeformat))
                Properties.Settings.Default.timeformat = textBoxTime.Text;
            else
                textBoxTime.Text = Properties.Settings.Default.timeformat;

            textBoxPath.Text = Properties.Settings.Default.filepath;

            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
            Properties.Settings.Default.Save();
        }

        private void ATimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            WriteFiles();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            string dummyFileName = "Save in this folder";
            SaveFileDialog sf = new SaveFileDialog();
            
            sf.CheckFileExists = false;
            sf.FileName = dummyFileName;
            sf.ShowDialog();
            textBoxPath.Text = String.IsNullOrWhiteSpace(sf.FileName) ? "" : System.IO.Path.GetDirectoryName(sf.FileName);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.filepath = textBoxPath.Text;
            Properties.Settings.Default.timeformat = textBoxTime.Text;
            Properties.Settings.Default.dateformat = textBoxDate.Text;
            Properties.Settings.Default.Save();
        }

        private void WriteFiles()
        {
            this.Dispatcher.Invoke(() =>
            {
                string timepath = System.IO.Path.Combine(Properties.Settings.Default.filepath, "time.txt");
                string datepath = System.IO.Path.Combine(Properties.Settings.Default.filepath, "date.txt");

                string timeformat = textBoxTime.Text;
                string dateformat = textBoxDate.Text;

                DateTime time = DateTime.Now;
                timeformat = timeformat
                    .Replace("%h", time.Hour.ToString())
                    .Replace("%m", time.Minute.ToString())
                    .Replace("%s", time.Second.ToString())
                    .Replace("%L", time.Millisecond.ToString());

                dateformat = dateformat
                    .Replace("%y", time.Year.ToString())
                    .Replace("%M", time.Month.ToString())
                    .Replace("%d", time.Day.ToString())
                    .Replace("%h", time.Hour.ToString())
                    .Replace("%m", time.Minute.ToString())
                    .Replace("%s", time.Second.ToString())
                    .Replace("%L", time.Millisecond.ToString());

                if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.filepath))
                {
                    File.WriteAllText(timepath, string.Empty);
                    File.WriteAllText(datepath, string.Empty);

                    File.WriteAllText(timepath, timeformat);
                    File.WriteAllText(datepath, dateformat);
                }
            });
        }
    }
}
