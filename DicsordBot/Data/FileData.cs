﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicsordBot.Data
{
    /// <summary>
    /// represents one file out of all local files,
    /// </summary>
    [Serializable()]
    public class FileData : INotifyPropertyChanged
    {
        private string name = "";
        private string path = "";
        private string author = "";
        private TimeSpan length = TimeSpan.Zero;

        /// <summary>
        /// Name of file
        /// </summary>
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }

        /// <summary>
        /// path to file
        /// </summary>
        public string Path { get { return path; } set { path = value; OnPropertyChanged("Path"); } }

        /// <summary>
        /// Author of the title
        /// </summary>
        public string Author { get { return author; } set { author = value; OnPropertyChanged("Author"); } }

        /// <summary>
        /// the dateTime of the last replay
        /// </summary>
        public TimeSpan Length { get { return length; } set { length = value; OnPropertyChanged("Length"); } }

        #region event

        /// <summary>
        /// PropertyChanged Event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// propertychanged method, notifies the actual handler
        /// </summary>
        /// <param name="info"></param>
        private void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion event
    }
}