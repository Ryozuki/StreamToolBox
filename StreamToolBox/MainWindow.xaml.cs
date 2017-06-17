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
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StreamToolBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Timers.Timer SecondTimer = new System.Timers.Timer();

        bool CountDownStarted = false;
        bool CountUpStarted = false;

        TimeSpan CountDownSpan;
        TimeSpan CountUpSpan;

        public MainWindow()
        {
            InitializeComponent();

            CountDownSpan = TimeSpan.Zero;
            CountUpSpan = TimeSpan.Zero;

            string VERSION = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            creditLabel.Content = ((string)creditLabel.Content).Replace("%v", VERSION);

            textBoxDate.ToolTip = "%y = year, %M = month, %d = day\n%h = hours %m = minutes, %s = seconds, %L = miliseconds";
            textBoxTime.ToolTip = "%h = hours %m = minutes, %s = seconds, %L = miliseconds";

            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.dateformat))
                Properties.Settings.Default.dateformat = textBoxDate.Text;
            else
                textBoxDate.Text = Properties.Settings.Default.dateformat;
            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.timeformat))
                Properties.Settings.Default.timeformat = textBoxTime.Text;
            else
                textBoxTime.Text = Properties.Settings.Default.timeformat;

            textBoxPath.Text = Properties.Settings.Default.filepath;

            SecondTimer.Elapsed += ATimer_Elapsed;
            SecondTimer.Interval = 1000;
            SecondTimer.Enabled = true;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
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
            Properties.Settings.Default.filepath = textBoxPath.Text;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.filepath = textBoxPath.Text;
            Properties.Settings.Default.timeformat = textBoxTime.Text;
            Properties.Settings.Default.dateformat = textBoxDate.Text;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        private void WriteFiles()
        {
            this.Dispatcher.Invoke(() =>
            {
                string timepath = System.IO.Path.Combine(Properties.Settings.Default.filepath, "time.txt");
                string datepath = System.IO.Path.Combine(Properties.Settings.Default.filepath, "date.txt");
                string countdownpath = System.IO.Path.Combine(Properties.Settings.Default.filepath, "countdown.txt");
                string countuppath = System.IO.Path.Combine(Properties.Settings.Default.filepath, "countup.txt");

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

                    if (CountDownSpan.TotalSeconds != 0 && CountDownStarted)
                    {
                        File.WriteAllText(countdownpath, string.Empty);
                        CountDownSpan = TimeSpan.FromSeconds(CountDownSpan.TotalSeconds - 1);

                        File.WriteAllText(countdownpath, String.Format("{0:00}:{1:00}:{2:00}", (int)CountDownSpan.TotalHours, 
                            CountDownSpan.Minutes, CountDownSpan.Seconds));

                        countdownBox.Text = String.Format("{0:00}:{1:00}:{2:00}", (int)CountDownSpan.TotalHours, 
                            CountDownSpan.Minutes, CountDownSpan.Seconds);
                    }
                    else
                    {
                        CountDownSpan = TimeSpan.Zero;
                        countDownButton.Content = "Start";
                    }

                    if(CountUpStarted)
                    {
                        CountUpSpan = TimeSpan.FromSeconds(CountUpSpan.TotalSeconds + 1);
                        File.WriteAllText(countuppath, string.Empty);

                        File.WriteAllText(countuppath, String.Format("{0:00}:{1:00}:{2:00}", (int)CountUpSpan.TotalHours, 
                            CountUpSpan.Minutes, CountUpSpan.Seconds));

                        countupBox.Text = String.Format("{0:00}:{1:00}:{2:00}", (int)CountUpSpan.TotalHours, 
                            CountUpSpan.Minutes, CountUpSpan.Seconds);
                    }
                }
            });
        }

        private void SetCountText(ref System.Windows.Controls.TextBox textbox, ref TimeSpan time)
        {
            const string pattern = @"^([0-9]+):([0-5][0-9]):([0-5][0-9])$";
            Match match = Regex.Match(textbox.Text, pattern);

            double t = (double.Parse(match.Groups[1].Value) * 60 * 60) // Convert hours to seconds
                    + (double.Parse(match.Groups[2].Value) * 60) // Convert minutes to seconds
                    + double.Parse(match.Groups[3].Value);

            time = TimeSpan.FromSeconds(t);
        }

        private void countDownButton_Click(object sender, RoutedEventArgs e)
        {
            CountDownStarted = !CountDownStarted;

            if(CountDownStarted)
            {
                countDownButton.Content = "Stop";
                SetCountText(ref countdownBox, ref CountDownSpan);
                if(CountDownSpan.TotalSeconds == 0)
                {
                    CountDownStarted = false;
                    countDownButton.Content = "Start";
                }
            }
            else
            {
                countDownButton.Content = "Start";
            }
        }

        private void countupButton_Click(object sender, RoutedEventArgs e)
        {
            CountUpStarted = !CountUpStarted;

            if (CountUpStarted)
            {
                countupButton.Content = "Stop";
                SetCountText(ref countupBox, ref CountUpSpan);
            }
            else
            {
                countupButton.Content = "Start";
            }
        }
    }
}
