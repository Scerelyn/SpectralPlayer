using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibraryLib
{
    [Serializable]
    public class Song
    {
        #region Private Fields

        private string _filePath = "";
        private string _name = "";
        private string _albumName = "Unknown Album";
        private string _albumArtist = "";
        private string _artist = "Unknown Artist";
        private string _genre = "";
        private string _year = "";
        private int _trackNumber = 0;

        #endregion

        #region Public Properties

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; FieldChanged(); }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; FieldChanged(); }
        }
        public string AlbumName
        {
            get { return _albumName; }
            set { _albumName = value; FieldChanged(); }
        }
        public string AlbumArtist
        {
            get { return _albumArtist; }
            set { _albumArtist = value; FieldChanged(); }
        }
        public string Artist
        {
            get { return _artist; }
            set { _artist = value; FieldChanged(); }
        }
        public string Genre
        {
            get { return _genre;  }
            set { _genre = value; FieldChanged(); }
        }
        public string Year
        {
            get { return _year; }
            set { _year = value; FieldChanged(); }
        }
        public int TrackNumber
        {
            get { return _trackNumber; }
            set { _trackNumber = value; FieldChanged(); }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Name} by {Artist} on {AlbumName} ({Year})";
        }

        #endregion

        #region Events/Delegates

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void FieldChanged(string field = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(field));
            }
        }

        #endregion
    }
}
