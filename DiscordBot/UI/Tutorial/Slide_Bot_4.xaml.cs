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
    /// Interaction logic for Slide_Bot_4.xaml
    /// </summary>
    public partial class Slide_Bot_4 : UserControl
    {
        public Slide_Bot_4()
        {
            InitializeComponent();
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            //save token
            if (!String.IsNullOrWhiteSpace(box_Token.Text))
                Handle.Token = box_Token.Text;
        }
    }
#pragma warning restore CS1591
}
