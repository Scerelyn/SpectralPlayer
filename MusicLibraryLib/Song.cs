using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MusicLibraryLib
{
    /// <summary>
    /// Represents a song in a music library
    /// </summary>
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
        private string _year = "0";
        private int _trackNumber = 0;

        #endregion

        #region Public Properties

        [XmlElement]
        /// <summary>
        /// The filepath of the song's audio file
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; FieldChanged(); }
        }

        [XmlElement]
        /// <summary>
        /// The name of the song
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; FieldChanged(); }
        }

        [XmlElement]
        /// <summary>
        /// The name of the album this song is on
        /// </summary>
        public string AlbumName
        {
            get { return _albumName; }
            set { _albumName = value; FieldChanged(); }
        }

        [XmlElement]
        /// <summary>
        /// The name of the artist that made the album this song is on
        /// </summary>
        public string AlbumArtist
        {
            get { return _albumArtist; }
            set { _albumArtist = value; FieldChanged(); }
        }

        [XmlElement]
        /// <summary>
        /// The name of the artist that made the song
        /// </summary>
        public string Artist
        {
            get { return _artist; }
            set { _artist = value; FieldChanged(); }
        }

        /// <summary>
        /// The genre of music this song belongs to
        /// </summary>
        public string Genre
        {
            get { return _genre;  }
            set { _genre = value; FieldChanged(); }
        }

        [XmlElement]
        /// <summary>
        /// The year the song, or the album this song is on, is from
        /// </summary>
        public string Year
        {
            get { return _year; }
            set { _year = value; FieldChanged(); }
        }

        [XmlElement]
        /// <summary>
        /// The track number of this song on the album it is on
        /// </summary>
        public int TrackNumber
        {
            get { return _trackNumber; }
            set { _trackNumber = value; FieldChanged(); }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Name} by {Artist} on {AlbumName}" + (Year != "0" ? Year : "");
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
