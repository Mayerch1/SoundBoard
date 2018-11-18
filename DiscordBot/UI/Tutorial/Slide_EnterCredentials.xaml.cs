﻿using System;
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

namespace DiscordBot.UI.Tutorial
{
#pragma warning disable CS1591
    /// <summary>
    /// Interaction logic for Slide_EnterCredentials.xaml
    /// </summary>
    public partial class Slide_EnterCredentials : UserControl
    {
        public Slide_EnterCredentials()
        {
            InitializeComponent();
        }

        private void btn_Accept(object sender, RoutedEventArgs e)
        {
            Handle.Data.Persistent.ClientName = box_Username.Text;
            Handle.Data.Persistent.Token = box_Token.Text;
        }

        private void box_Username_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox box)
            {
                //replace all blancs
                box.Text = box.Text.Replace(" ", String.Empty);

                box.SelectionStart = box.Text.Length;
                box.SelectionLength = 0;
            }
        }
    }
#pragma warning restore CS1591
}