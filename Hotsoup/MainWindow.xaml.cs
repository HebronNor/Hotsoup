using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Hotsoup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        public static int AutoStartDelay = 600;
        public static string AutoStartKey;

        public static bool shutdownEnabled;
        public static int shutdownDelay;

        public static int traktAutoUpdate;

        public static System.Timers.Timer timer = new System.Timers.Timer();
        public static System.Timers.Timer timer2 = new System.Timers.Timer();

        System.ComponentModel.BackgroundWorker[] worker = new System.ComponentModel.BackgroundWorker[4];

        List<Button> buttonList = new List<Button>();

        public static Version AssemblyVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
        public static string AssemblyVersionText = string.Format(@"{0}.{1}.{2} (r{3})", AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build, AssemblyVersion.Revision);

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Application.ThreadException += LogMsg.Application_ThreadException;

            if (Properties.Settings.Default.SettingsVersion != AssemblyVersion.ToString())
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.SettingsVersion = AssemblyVersion.ToString();
                Properties.Settings.Default.Save();
            }

            string[] arguments = Environment.GetCommandLineArgs();
            string xmlFile = AppDomain.CurrentDomain.BaseDirectory + "Hotsoup.xml";

            foreach (string arg in arguments)
            {
                if (arg == "/WindowMode")
                {
                    this.WindowState = System.Windows.WindowState.Normal;
                    this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                    this.ResizeMode = System.Windows.ResizeMode.CanResize;
                }

                if (Regex.IsMatch(arg, ".xml$", RegexOptions.IgnoreCase))
                    xmlFile = arg;
            }

            try
            {
                XML.ReadXML(xmlFile);
            }
            catch (System.IO.FileNotFoundException exp)
            {
                MessageBox.Show(string.Format("XML config-file {0} was not found, unable to continue...", exp.FileName));
                Environment.Exit(1);
            }
            catch (NullReferenceException)
            {
                textStatus.Text = "Failed to load XML config file.";
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }

            lblPlexStart.Content = "Loading...";
            textStatus.Opacity = 0;

            textHotsoup.Opacity = 0;
            textHotsoup.Text = "Hotsoup v." + AssemblyVersionText;

            txtAirDatesHeader.Opacity = 0;
            txtTrendMoviesHeader.Opacity = 0;
            txtTrendShowsHeader.Opacity = 0;
            txtUnwatchedHeader.Opacity = 0;

            txtAirDates.Opacity = 0;
            txtTrendMovies.Opacity = 0;
            txtTrendShows.Opacity = 0;
            txtUnwatched.Opacity = 0;

            buttonList.Add(btnApp0);
            buttonList.Add(btnApp1);
            buttonList.Add(btnApp2);
            buttonList.Add(btnApp3);

            foreach (Button item in buttonList)
                item.Visibility = System.Windows.Visibility.Collapsed;

            int x = 0;
            foreach (ApplicationInfo appInfo in ApplicationInfo.List)
            {
                if (appInfo.Enabled)
                {
                    buttonList[x].Visibility = System.Windows.Visibility.Visible;
                    buttonList[x].ToolTip = appInfo.Label;
                    buttonList[x].Content = ReturnImage(appInfo.Icon);
                }

                x++;
            }

            if (!shutdownEnabled) btnShutDown.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                btnShutDown.ToolTip = "Shut down";
                btnShutDown.Content = ReturnImage("shutdown_icon.png");
            }

            if (ApplicationInfo.List.Count > 4) MessageBox.Show("Max 4 applications can be defined...");

            timer.Interval = 100;
            timer.Elapsed += timer_Elapsed;
            progressBar.Maximum = AutoStartDelay;

            timer2.Interval = TimeSpan.FromMinutes(traktAutoUpdate).TotalMilliseconds;
            timer2.Elapsed += (obj, f) =>
            {
                DoTraktUpdate();
            };

            worker[0] = new System.ComponentModel.BackgroundWorker();
            worker[1] = new System.ComponentModel.BackgroundWorker();
            worker[2] = new System.ComponentModel.BackgroundWorker();
            worker[3] = new System.ComponentModel.BackgroundWorker();

            Trakt trakt = new Trakt();

            worker[0].DoWork += (obj, f) => { f.Result = trakt.GetMovies(); };
            worker[1].DoWork += (obj, f) => { f.Result = trakt.GetShows(); };
            worker[2].DoWork += (obj, f) => { f.Result = trakt.GetAirs(); };
            worker[3].DoWork += (obj, f) => { f.Result = trakt.GetProgress(); };

            worker[0].RunWorkerCompleted += (obj, f) =>
                {
                    this.Dispatcher.Invoke((Action)(() => { txtTrendMoviesHeader.Text = "Trending movies:"; })); 

                    StringBuilder sb = new StringBuilder();

                    if ((bool)f.Result == true)
                    {
                        foreach (Movie item in Movie.List)
                        {
                            if (sb.Length > 0) sb.AppendLine();
                            sb.Append(item.title);
                        }
                    }
                    else sb.Append("Failed!");

                    this.Dispatcher.Invoke((Action)(() => { txtTrendMovies.Text = sb.ToString(); }));
                };

            worker[1].RunWorkerCompleted += (obj, f) =>
                {
                    this.Dispatcher.Invoke((Action)(() => { txtTrendShowsHeader.Text = "Trending shows:"; })); 

                    StringBuilder sb = new StringBuilder();

                    if ((bool)f.Result == true)
                    {
                        foreach (Serie item in Serie.List)
                        {
                            if (sb.Length > 0) sb.AppendLine();
                            sb.Append(item.title);
                        }
                    }
                    else sb.Append("Failed!");

                    this.Dispatcher.Invoke((Action)(() => { txtTrendShows.Text = sb.ToString(); }));
                };

            worker[2].RunWorkerCompleted += (obj, f) =>
                {
                    this.Dispatcher.Invoke((Action)(() => { txtAirDatesHeader.Text = "Air dates:"; })); 

                    StringBuilder sb = new StringBuilder();

                    if ((bool)f.Result == true)
                    {
                        foreach (tCalender item in tCalender.Airs)
                        {
                            if (sb.Length > 0) sb.AppendLine();
                            sb.Append(item.date);

                            foreach (Episode item2 in item.episodes)
                            {
                                sb.AppendLine();
                                sb.AppendFormat("     {0}", item2.show["title"]);
                            }
                        }
                    }
                    else sb.Append("Failed!");

                    this.Dispatcher.Invoke((Action)(() => { txtAirDates.Text = sb.ToString(); }));
                };

            worker[3].RunWorkerCompleted += (obj, f) =>
                {
                    this.Dispatcher.Invoke((Action)(() => { txtUnwatchedHeader.Text = "Unwatched:"; })); 

                    StringBuilder sb = new StringBuilder();

                    if ((bool)f.Result == true)
                    {
                        foreach (tProgress item in tProgress.Prog)
                        {
                            if (Convert.ToInt32(item.progress["left"]) > 0)
                            {
                                if (sb.Length > 0) sb.AppendLine();
                                sb.AppendFormat("{0} {1,5}", item.show["title"], item.progress["left"]);
                            }
                        }
                    }
                    else sb.Append("Failed!");

                    this.Dispatcher.Invoke((Action)(() => { txtUnwatched.Text = sb.ToString(); }));
                };

            DoTraktUpdate();

            if (!timer.Enabled) CleanUpCountDown();
            
            ChangeBackground();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exp = (Exception)e.ExceptionObject;
            string logSubject = "Unhandled Exception";

            MessageBox.Show(exp.Message, logSubject, MessageBoxButton.OK, MessageBoxImage.Error);

            using (System.IO.StreamWriter w = System.IO.File.AppendText(baseDir + "Debug.log"))
            {
                w.WriteLine("{0} : {1} {2}", logSubject.ToUpper(), DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
                w.WriteLine("Version : {0}", AssemblyVersionText);
                w.WriteLine("Type    : {0}", exp.GetType().Name);
                w.WriteLine("Source  : {0}", exp.Source);
                w.WriteLine("Target  : {0}", exp.TargetSite);
                w.WriteLine();
                w.WriteLine("{0}", exp.Message);
                w.WriteLine();
                w.WriteLine("{0}", exp.StackTrace);
                w.WriteLine("-------------------------------------------------------------------------");
                w.WriteLine();
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            AutoStartDelay--;
            string plural = string.Empty;

            if (AutoStartDelay / 10 != 1) plural = "s";
            string countText = string.Format("Starting {0} in {1} second{2}...", ApplicationInfo.ReturnObj(AutoStartKey).Label, AutoStartDelay / 10, plural);
            this.Dispatcher.Invoke((Action)(() =>
            {
                lblPlexStart.Content = countText;
                progressBar.Value = AutoStartDelay;
            }));


            if (AutoStartDelay == 0)
            {
                CleanUpCountDown();
                StartProcess(ApplicationInfo.ReturnObj(AutoStartKey).Executable);
            }
        }

        private void ShutDown_Click(object sender, RoutedEventArgs e)
        {
            textStatus.Text = "Shutting down, please wait...";

            CleanUpCountDown();
            StartProcess(@"shutdown.exe", string.Format("/s /t {0}", shutdownDelay));
        }

        private void StartProcess(string app, string arg = null)
        {
            try
            {
                Process.Start(app, arg);
            }
            catch (Exception)
            {
                this.Dispatcher.Invoke((Action)(() => { textStatus.Text = "Unable to start: " + app; }));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CleanUpCountDown();
        }

        private void CleanUpCountDown()
        {
            timer.Enabled = false;
            this.Dispatcher.Invoke((Action)(() =>
            {
                lblPlexStart.Visibility = System.Windows.Visibility.Hidden;
                progressBar.Visibility = System.Windows.Visibility.Hidden;
                buttonCancelPlex.Visibility = System.Windows.Visibility.Hidden;
            }));
        }

        private void AbortStart_Click(object sender, RoutedEventArgs e)
        {
            textStatus.Text = string.Format("Aborted {0} auto-start.", ApplicationInfo.ReturnObj(AutoStartKey).Label);

            CleanUpCountDown();
        }

        private void StartApp_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int index = buttonList.IndexOf(btn);

            textStatus.Text = string.Format("Starting {0}...", ApplicationInfo.List[index].Label);

            CleanUpCountDown();
            StartProcess(ApplicationInfo.List[index].Executable);
        }

        private void DoTraktUpdate()
        {
            if (Trakt.traktEnabled)
            {
                if (Trakt.trendEnabled)
                {
                    if (!worker[0].IsBusy) worker[0].RunWorkerAsync();
                    if (!worker[1].IsBusy) worker[1].RunWorkerAsync();
                }
                if (!worker[2].IsBusy) worker[2].RunWorkerAsync();
                if (!worker[3].IsBusy) worker[3].RunWorkerAsync();

                this.Dispatcher.Invoke((Action)(() => { textStatus.Text = "Getting information from Trakt..."; }));
            }
        }

        private void ChangeBackground()
        {
            string wallPaperDir = baseDir + "Wallpapers";

            if (System.IO.Directory.Exists(wallPaperDir))
            {
                string[] wallpapers = System.IO.Directory.GetFiles(wallPaperDir, "*.jpg");

                if (wallpapers.Length > 0)
                {
                    int intWall = Properties.Settings.Default.Wallpaper;
                    if (intWall > wallpapers.Length - 1) intWall = 0;
                    string wallpaper = wallpapers[intWall];

                    intWall++;
                    Properties.Settings.Default.Wallpaper = intWall;
                    Properties.Settings.Default.Save();

                    this.Dispatcher.Invoke((Action)(() => { mainGrid.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), wallpaper))); }));
                }
            }
        }

        private Image ReturnImage(string source)
        {
            Image img = new Image();

            img.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), baseDir + "Icons/" + source));
            img.Stretch = Stretch.Uniform;
            img.Width = 240;
            img.Height = 240;
            img.Cursor = Cursors.Hand;

            return img;
        }

    }
}
