﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace DiscordBot.UI
{
    /// <summary>
    /// Interaction logic for ButtonUI.xaml
    /// </summary>
    public partial class ButtonUI : INotifyPropertyChanged
    {
#pragma warning disable CS1591

        public delegate void InstantButtonClickedHandler(int btnListIndex);

        public InstantButtonClickedHandler InstantButtonClicked;

        public delegate void ToggleHotkeyHandler(bool isEnabled);

        public ToggleHotkeyHandler ToggleHotkey;

        public ButtonUI()
        {
            InitializeComponent();

            Handle.Data.resizeBtnList();

            this.DataContext = Handle.Data.Persistent;
            btnControl.ItemsSource = Handle.Data.Persistent.BtnList;
        }

        private void btn_Instant_Click(object sender, RoutedEventArgs e)
        {
            //event is handled in MainWindow

            Button btn = (Button)sender;

            int index = (int)btn.Tag;

            InstantButtonClicked(index);
        }

        private void btn_FileChooser_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = (int)btn.Tag;

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            string allFormats = "*" + Handle.Data.Persistent.supportedFormats.Aggregate((i, j) => i + ";" + "*" + j);
            string allFormatString = "all supported types |" + allFormats;

            openFileDialog.Filter = allFormatString +
                                    "|mp3/wav files (*.mp3/*.wav)|*.mp3;*.wav" +
                                    "|all files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true && openFileDialog.CheckFileExists)
            {
                Handle.Data.Persistent.BtnList[index].Name = evaluateName(openFileDialog.FileName);
                Handle.Data.Persistent.BtnList[index].File = openFileDialog.FileName;
            }
        }

        private void btn_HotkeyEditor_Click(object sender, RoutedEventArgs e)
        {
            int tag = (int)((FrameworkElement)sender).Tag;

            //disable hotkeys, while editing them
            ToggleHotkey(false);
            //trigger blur effect
            IO.BlurEffectManager.ToggleBlurEffect(true);

            var popup = new ButtonHotkeyPopup(tag, Application.Current.MainWindow);
            popup.IsOpen = true;

            popup.Closed += delegate (object pSender, EventArgs pE)
            {
                IO.BlurEffectManager.ToggleBlurEffect(false);
                ToggleHotkey(true);
            };
        }

        //return only the file name from a Path to a file
        private string evaluateName(string filePath)
        {
            var fileType = System.IO.Path.GetFileName(filePath);
            var fileName = fileType.Substring(0, fileType.LastIndexOf('.'));

            return fileName;
        }

        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(null, new PropertyChangedEventArgs(info));
            }
        }

        private void btn_Return_Click(object sender, RoutedEventArgs e)
        {
            Handle.Data.resizeBtnList();
        }

#pragma warning restore CS1591
    }
}