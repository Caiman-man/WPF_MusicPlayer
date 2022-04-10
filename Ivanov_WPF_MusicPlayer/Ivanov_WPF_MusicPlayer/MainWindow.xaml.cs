using Ivanov_WPF_MusicPlayer.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Un4seen.Bass;
using Un4seen.Bass.Misc;


namespace Ivanov_WPF_MusicPlayer
{
    public partial class MainWindow : Window
    {
        enum WindowMode { Minimized, Maximized, Normal }
        WindowMode currentWindowMode;

        enum PlayMode { Play, Stop, Pause }
        PlayMode currentPlayMode = PlayMode.Stop;

        enum VisualizerMode { Bar, PeakBar, Lines, Wave, Dot, Bean, Eclipse, WaveForm, SpectrumText }
        VisualizerMode visualizerMode = VisualizerMode.Bar;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherTimer2 = new System.Windows.Threading.DispatcherTimer();
        bool isMute = false;
        bool canDragWindow;
        int _stream;
        string currentSong;
        int currentSongsAmount = 0;

        //список песен
        List<string> songs = new List<string>();

        //data context
        RadioStations r = new RadioStations();
        Songs s = new Songs();
            
        //визуализатор
        Visuals spectrum = new Visuals();
        

        public MainWindow()
        {
            InitializeComponent();
            currentWindowMode = WindowMode.Normal;
            canDragWindow = true;
            CreateToolTips();
            RadioListFill();

            //инициализация библиотеки Bass.NET
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            //присвоить контекст
            this.DataContext = s;
        }

        //----------------------------------------------------------------------------------
        //WINDOW

        //drag window
        private void grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canDragWindow)
                DragMove();
        }


        //minimize window
        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            currentWindowMode = WindowMode.Minimized;
        }


        //maximize window
        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if ((currentWindowMode == WindowMode.Minimized || 
                currentWindowMode == WindowMode.Normal) &&
                (this.Width != SystemParameters.WorkArea.Width ||
                this.Height != SystemParameters.WorkArea.Height) &&
                WindowState != WindowState.Maximized)
            {
                maxIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
                this.Top = 0;
                this.Left = 0;
                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;
                this.ResizeMode = ResizeMode.NoResize;
                currentWindowMode = WindowMode.Maximized;
                canDragWindow = false;
            }
            else
            {
                maxIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
                this.Top = SystemParameters.WorkArea.Height / 2 - 300;
                this.Left = SystemParameters.WorkArea.Width / 2 - 500;
                this.Height = 600;
                this.Width = 1000;
                this.ResizeMode = ResizeMode.CanResizeWithGrip;
                currentWindowMode = WindowMode.Normal;
                WindowState = WindowState.Normal;
                canDragWindow = true;
            }
        }


        //exit
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Bass.BASS_ChannelStop(_stream);
            Bass.BASS_StreamFree(_stream);
            this.Close();
        }


        //tooltips
        public void CreateToolTips()
        {
            List<ToolTip> tips = new List<ToolTip>();
            tips.Add(new ToolTip());
            tips[0].Content = "Home";
            homeButton.ToolTip = tips[0];

            tips.Add(new ToolTip());
            tips[1].Content = "Open files";
            openButton.ToolTip = tips[1];

            tips.Add(new ToolTip());
            tips[2].Content = "Radio";
            radioButton.ToolTip = tips[2];

            tips.Add(new ToolTip());
            tips[3].Content = "Favourites";
            favouriteButton.ToolTip = tips[3];

            tips.Add(new ToolTip());
            tips[4].Content = "Add playlist";
            addPlaylistButton.ToolTip = tips[4];

            tips.Add(new ToolTip());
            tips[5].Content = "Play";
            playlistPlayButton.ToolTip = tips[5];

            tips.Add(new ToolTip());
            tips[6].Content = "Minimize";
            minimizeButton.ToolTip = tips[6];

            tips.Add(new ToolTip());
            tips[7].Content = "Maximize";
            maximizeButton.ToolTip = tips[7];

            tips.Add(new ToolTip());
            tips[8].Content = "Close";
            closeButton.ToolTip = tips[8];

            tips.Add(new ToolTip());
            tips[9].Content = "Skip previous";
            skipPreviousButton.ToolTip = tips[9];

            tips.Add(new ToolTip());
            tips[10].Content = "Skip next";
            skipNextButton.ToolTip = tips[10];

            tips.Add(new ToolTip());
            tips[11].Content = "Repeat";
            repeatButton.ToolTip = tips[11];

            tips.Add(new ToolTip());
            tips[12].Content = "Play";
            playButton.ToolTip = tips[12];

            tips.Add(new ToolTip());
            tips[13].Content = "Stop";
            stopButton.ToolTip = tips[13];

            tips.Add(new ToolTip());
            tips[14].Content = "Volume";
            volumeButton.ToolTip = tips[14];

            foreach (var t in tips)
                t.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
        }


        //запрет на maximize при перетягивании окна к верхнему краю
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                maximizeButton_Click(this, null);
        }

        //----------------------------------------------------------------------------------
        //PLAYER

        //play
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (homeListBox.SelectedIndex != -1)
                {
                    if (currentSong != s[homeListBox.SelectedIndex].Path.ToString())
                        stopButton_Click(this, null);

                    PlaySong(s[homeListBox.SelectedIndex].Path.ToString());
                }
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                if (radioListBox.SelectedIndex != -1)
                {
                    if (currentSong != r[radioListBox.SelectedIndex].URL.ToString())
                        stopButton_Click(this, null);

                    PlayRadioWave(r[radioListBox.SelectedIndex].URL.ToString());
                }
            }
        }

        //playlist button
        private void playlistPlayButton_Click(object sender, RoutedEventArgs e)
        {
            playButton_Click(this, null);
        }


        //проигрывать песню
        public void PlaySong(string fileName)
        {
            if (currentPlayMode == PlayMode.Stop)
            {
                _stream = Bass.BASS_StreamCreateFile(fileName, 0, 0, BASSFlag.BASS_DEFAULT);

                if (_stream != 0)
                {
                    Bass.BASS_ChannelPlay(_stream, false);
                    playIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    playlistIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    currentPlayMode = PlayMode.Play;
                    currentSong = fileName;

                    startLabel.Text = TimeSpan.FromSeconds(GetStreamPosition(_stream)).ToString();
                    endLabel.Text = TimeSpan.FromSeconds(GetStreamTime(_stream)).ToString();
                    soundSlider.Maximum = GetStreamTime(_stream);
                    soundSlider.Value = GetStreamPosition(_stream);

                    StartTimer();
                    StartTimer2();
                }
            }
            else if (currentPlayMode == PlayMode.Pause)
            {
                Bass.BASS_ChannelPlay(_stream, false);
                playIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                playlistIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                currentPlayMode = PlayMode.Play;
            }
            else
            {
                Bass.BASS_ChannelPause(_stream);
                playIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                playlistIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                currentPlayMode = PlayMode.Pause;
            }
        }


        //проигрывать радио-волну
        public void PlayRadioWave(string url)
        {
            if (currentPlayMode == PlayMode.Stop)
            {
                _stream = Bass.BASS_StreamCreateURL(url, 0, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);

                if (_stream != 0)
                {
                    Bass.BASS_ChannelPlay(_stream, false);
                    playIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    playlistIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    currentPlayMode = PlayMode.Play;
                    currentSong = url;
                }
            }
            else if (currentPlayMode == PlayMode.Pause)
            {
                Bass.BASS_ChannelPlay(_stream, false);
                playIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                playlistIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                currentPlayMode = PlayMode.Play;
            }
            else
            {
                Bass.BASS_ChannelPause(_stream);
                playIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                playlistIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                currentPlayMode = PlayMode.Pause;
            }
        }


        //таймер для отображения sound bar
        private void StartTimer()
        {
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();
        }


        //timer-tick
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            startLabel.Text = TimeSpan.FromSeconds(GetStreamPosition(_stream)).ToString();
            soundSlider.Value = GetStreamPosition(_stream);
        }


        //track bar changed
        private void soundSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Bass.BASS_ChannelSetPosition(_stream, soundSlider.Value);
        }


        //подсчитать время песни
        public static int GetStreamTime(int stream)
        {
            long timeBytes = Bass.BASS_ChannelGetLength(stream);
            double streamTime = Bass.BASS_ChannelBytes2Seconds(stream, timeBytes);
            return (int)streamTime;
        }


        //найти текущую позицию в потоке
        public static int GetStreamPosition(int stream)
        {
            long pos = Bass.BASS_ChannelGetPosition(stream);
            int streamPosition = (int)Bass.BASS_ChannelBytes2Seconds(stream, pos);
            return streamPosition;
        }


        //stop
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            Bass.BASS_ChannelStop(_stream);
            Bass.BASS_StreamFree(_stream);
            playIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            currentPlayMode = PlayMode.Stop;
            dispatcherTimer.Stop();
            dispatcherTimer2.Stop();
            soundSlider.Value = 0;
            startLabel.Text = "00:00:00";
            endLabel.Text = "00:00:00";
        }


        //skip previous
        private void skipPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (homeListBox.SelectedIndex != -1)
                {
                    if (currentSong != s[--homeListBox.SelectedIndex].Path.ToString())
                        stopButton_Click(this, null);

                    PlaySong(s[homeListBox.SelectedIndex].Path.ToString());
                }
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                if (radioListBox.SelectedIndex != -1)
                {
                    if (currentSong != r[--radioListBox.SelectedIndex].URL.ToString())
                        stopButton_Click(this, null);

                    PlayRadioWave(r[radioListBox.SelectedIndex].URL.ToString());
                }
            }
        }


        //skip next
        private void skipNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (homeListBox.SelectedIndex != -1)
                {
                    if (currentSong != s[++homeListBox.SelectedIndex].Path.ToString())
                        stopButton_Click(this, null);

                    PlaySong(s[homeListBox.SelectedIndex].Path.ToString());
                }
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                if (radioListBox.SelectedIndex != -1)
                {
                    if (currentSong != r[++radioListBox.SelectedIndex].URL.ToString())
                        stopButton_Click(this, null);

                    PlayRadioWave(r[radioListBox.SelectedIndex].URL.ToString());
                }
            }
        }


        //mute
        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMute == false)
            {
                volumeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
                volumeSlider.Value = 0;
                isMute = true;
            }
            else
            {
                volumeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
                volumeSlider.Value = 10;
                isMute = false;
            }
        }


        //volume
        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, (float)volumeSlider.Value / 10);
            if (volumeSlider.Value != 0)
                volumeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
            else
                volumeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
        }


        //----------------------------------------------------------------------------------
        //HOME

        //home
        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            soundSlider.IsEnabled = true;
            clearButton.Visibility = Visibility.Visible;
            visual_grid.Visibility = Visibility.Visible;
            settingsButton.Visibility = Visibility.Visible;

            //привязка коллекции песен к XAML
            this.DataContext = s;

            homeListBox.SelectedIndex = 0;
        }


        //----------------------------------------------------------------------------------
        //OPEN

        //open files
        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;
            dlg.DefaultExt = ".mp3";
            dlg.Filter = "MP3 Files (*.mp3)|*.mp3|FLAC Files (*.flac)|*.flac";
            bool? result = dlg.ShowDialog();

            if (result == true)
            {               
                foreach (var song in dlg.FileNames)
                {
                    string tmp = System.IO.Path.GetFileName(song);

                    if (!songs.Contains(song))
                    {
                        //добавляем песни в коллекцию песен для привязки к XAML
                        s.Add((++currentSongsAmount).ToString(), tmp, song);

                        //добавляем песни в список текущих песен
                        songs.Add(song);
                    }
                    else
                    {
                        MessageBox.Show($"{tmp} is already exist!");
                    }
                    
                }             
            }

            this.DataContext = s;
            homeButton_Click(this, null);
        }

        //clear
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                s.Clear();
                songs.Clear();
                currentSongsAmount = 0;
                this.DataContext = s;
            }
            //else if (tabControl1.SelectedIndex == 2)
            //{

            //}
        }

        //----------------------------------------------------------------------------------
        //RADIO

        //radio
        private void radioButton_Click(object sender, RoutedEventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            soundSlider.IsEnabled = false;
            clearButton.Visibility = Visibility.Hidden;
            visual_grid.Visibility = Visibility.Hidden;
            settingsButton.Visibility = Visibility.Hidden;

            //привязка коллекции радио к XAML
            this.DataContext = r;
            radioListBox.SelectedIndex = 0;
        }


        //radio list fill
        public void RadioListFill()
        {
            r.Add("1", "Europa Plus", @"http://ep256.hostingradio.ru:8052/europaplus256.mp3");
            r.Add("2", "Russian Radio", @"http://144.76.92.71/stream");
            r.Add("3", "Shanson", @"http://chanson.hostingradio.ru:8041/chanson256.mp3");
            r.Add("4", "Shanson Plus", @"http://radio.radioshansonplus.com:8000/radio");
            r.Add("5", "Eldoradio", @"http://emgspb.hostingradio.ru/eldoradio128.mp3");
            r.Add("6", "Dorozhnoe", @"http://dorognoe.hostingradio.ru:8000/radio");
            r.Add("7", "Classic", @"http://stream.srg-ssr.ch/m/rsc_de/mp3_128");
            r.Add("8", "SoulLive", @"http://radio.soullive.ru:8000/livedj");
            r.Add("9", "Retro FM", @"http://retroserver.streamr.ru:8043/retro256.mp3");
            r.Add("10", "Vesti FM", @"http://icecast.vgtrk.cdnvideo.ru/vestifm_mp3_192kbps");
            r.Add("11", "Kommersant FM", @"http://kommersant77.hostingradio.ru:8016/kommersant128.mp3");
            r.Add("12", "Mayak", @"http://icecast.vgtrk.cdnvideo.ru/mayakfm_mp3_192kbps");
            r.Add("13", "Anime", @"http://pool.anison.fm:9000/AniSonFM(320)?nocache=0.98");
            r.Add("14", "PromoDJ - TOP 100", @"http://radio.promodj.com/top100-192");
            r.Add("15", "PromoDJ - Club", @"http://radio.promodj.com/klubb-192");
            r.Add("16", "PromoDJ - DubStep", @"http://radio.promodj.com/dubstep-192");
            r.Add("17", "PromoDJ - Deep", @"http://radio.promodj.com/deep-192");
            r.Add("18", "PromoDJ - Minimal", @"http://radio.promodj.com/mini-192");
            r.Add("19", "PromoDJ - Pop", @"http://radio.promodj.com/pop-192");
            r.Add("20", "PromoDJ - OldSchool", @"http://radio.promodj.com/oldschool-192");
            r.Add("21", "PromoDJ - Fool Moon", @"http://radio.promodj.com/fullmoon-192");
        }        


        //----------------------------------------------------------------------------------
        //FAVOURITES

        //favourites
        private void favouriteButton_Click(object sender, RoutedEventArgs e)
        {
            tabControl1.SelectedIndex = 2;
            soundSlider.IsEnabled = true;
        }


        //----------------------------------------------------------------------------------
        //VISUALIZER

        //visualizer
        public void Visualizer()
        {
            System.Drawing.Color clr1 = System.Drawing.Color.FromArgb(22, 177, 216);
            System.Drawing.Color clr2 = System.Drawing.Color.LightGreen;
            System.Drawing.Color clr3 = System.Drawing.Color.FromArgb(20, 19, 28);
            Bitmap bmp = null;
            BitmapSource bs;
            ImageBrush ib;

            if (visualizerMode == VisualizerMode.Bar)
                bmp = spectrum.CreateSpectrumLine(_stream, 200, 60, clr1, clr2, clr3, 7, 3, true, true, true);  
            else if (visualizerMode == VisualizerMode.PeakBar)
                bmp = spectrum.CreateSpectrumLinePeak(_stream, 200, 60, clr1, clr2, clr2, clr3, 7, 1, 3, 50, false, false, true);
            else if (visualizerMode == VisualizerMode.Lines)
                bmp = spectrum.CreateSpectrum(_stream, 200, 60, clr1, clr2, clr3, false, false, true);
            else if (visualizerMode == VisualizerMode.Wave)
                bmp = spectrum.CreateSpectrumWave(_stream, 200, 60, clr1, clr2, clr3, 1, false, false, true);
            else if (visualizerMode == VisualizerMode.Dot)
                bmp = spectrum.CreateSpectrumDot(_stream, 200, 60, clr1, clr2, clr3, 7, 3, false, false, true);
            else if (visualizerMode == VisualizerMode.Bean)
                bmp = spectrum.CreateSpectrumBean(_stream, 200, 60, clr1, clr2, clr3, 1, false, false, true);
            else if (visualizerMode == VisualizerMode.Eclipse)
                bmp = spectrum.CreateSpectrumEllipse(_stream, 200, 60, clr1, clr2, clr3, 1, 3, false, false, true);
            else if (visualizerMode == VisualizerMode.WaveForm)
                bmp = spectrum.CreateWaveForm(_stream, 200, 60, clr1, clr2, clr3, clr3, 1, false, false, true);
            else if (visualizerMode == VisualizerMode.SpectrumText)
                bmp = spectrum.CreateSpectrumText(_stream, 200, 60, clr1, clr2, clr3, "MUSIC PLAYER", false, false, true);

            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
                ib = new ImageBrush(bs);
                visual_grid.Background = ib;
            }
            catch (Exception)
            {
                GC.Collect(0, GCCollectionMode.Forced);
                visual_grid.Background = null;
            }
        }


        //visualizer timer
        private void StartTimer2()
        {
            dispatcherTimer2.Tick += new EventHandler(dispatcherTimer2_Tick);
            dispatcherTimer2.Interval = TimeSpan.FromMilliseconds(20);
            dispatcherTimer2.Start();
        }


        //visualizer timer-tick
        private void dispatcherTimer2_Tick(object sender, EventArgs e)
        {
            Visualizer();
        }


        //settings
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings set = new Settings();
            set.Owner = this;
            set.PerformVisualizer += ChooseVisualizerEvent;
            set.ShowDialog();
        }


        //choose visualizer event
        private void ChooseVisualizerEvent(string name)
        {
            if (name == "Bar")
                visualizerMode = VisualizerMode.Bar;
            else if (name == "PeakBar")
                visualizerMode = VisualizerMode.PeakBar;
            else if (name == "Lines")
                visualizerMode = VisualizerMode.Lines;
            else if (name == "Wave")
                visualizerMode = VisualizerMode.Wave;
            else if (name == "Dot")
                visualizerMode = VisualizerMode.Dot;
            else if (name == "Bean")
                visualizerMode = VisualizerMode.Bean;
            else if (name == "Eclipse")
                visualizerMode = VisualizerMode.Eclipse;
            else if (name == "WaveForm")
                visualizerMode = VisualizerMode.WaveForm;
            else if (name == "SpectrumText")
                visualizerMode = VisualizerMode.SpectrumText;
        }
    }
}