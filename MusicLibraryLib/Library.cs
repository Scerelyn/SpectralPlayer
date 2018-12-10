using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MusicLibraryLib
{
    /// <summary>
    /// Represents a music library of saved songs
    /// </summary>
    [Serializable]
    [XmlRoot]
    public class Library
    {
        #region Public Properties

        [XmlArray]
        /// <summary>
        /// The list of every song in the music library
        /// </summary>
        public ObservableCollection<Song> SongList { get; } = new ObservableCollection<Song>();

        [XmlArray]
        /// <summary>
        /// The list of every playlist in the music library
        /// </summary>
        public ObservableCollection<PlayList> PlayListList { get; } = new ObservableCollection<PlayList>();

        #endregion

        #region Public methods

        /// <summary>
        /// Returns an IEnumerable of the entire music library ordered by song name alphabetically
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Song> GetOrderedListBySong()
        {
            return SongList.OrderBy(song => song.Name);
        }

        /// <summary>
        /// Returns an IEnumerable of the entire music library ordered by albums, whose songs are then ordered by track number
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Song> GetOrderedListByAlbum()
        {
            return SongList.OrderBy(song => song.AlbumName).ThenBy(song => song.TrackNumber);
        }

        /// <summary>
        /// Returns an IEnumerable of the entire music library ordered by the artist, then by album and track number
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Song> GetOrderedListByArtist()
        {
            return SongList.OrderBy(song => song.Artist).ThenBy(song => song.AlbumName).ThenBy(song => song.TrackNumber);
        }

        /// <summary>
        /// Returns an IEnumerable of the entire music library ordered by genre alphabetically
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Song> GetOrderedListByGenre()
        {
            return SongList.OrderBy(song => song.Genre);
        }

        /// <summary>
        /// Returns an IEnumerable of the entire music library order by the year of the song
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Song> GetOrderedListByYear()
        {
            return SongList.OrderBy(song => song.Year).ThenBy(song => song.Artist);
        }


        public Dictionary<string, List<Song>> GroupByProperty(SongPropertyGroup propertyGroup)
        {
            Dictionary<string, List<Song>> Groupings = new Dictionary<string, List<Song>>();
            foreach(Song s in SongList)
            {
                switch (propertyGroup)
                {
                    case SongPropertyGroup.Album:
                        if (Groupings.Keys.Contains(s.AlbumName))
                        {
                            Groupings[s.AlbumName].Add(s);
                        }
                        else
                        {
                            Groupings.Add(s.AlbumName, new List<Song>());
                            Groupings[s.AlbumName].Add(s);
                        }
                        break;
                    case SongPropertyGroup.Artist:
                        if (Groupings.Keys.Contains(s.Artist))
                        {
                            Groupings[s.Artist].Add(s);
                        }
                        else
                        {
                            Groupings.Add(s.Artist, new List<Song>());
                            Groupings[s.Artist].Add(s);
                        }
                        break;
                    case SongPropertyGroup.Genre:
                        if (Groupings.Keys.Contains(s.Genre))
                        {
                            Groupings[s.Genre].Add(s);
                        }
                        else
                        {
                            Groupings.Add(s.Genre, new List<Song>());
                            Groupings[s.Genre].Add(s);
                        }
                        break;
                }
            }
            return Groupings;
        }

        #endregion

        #region Events/Delegates

        [field: NonSerialized]
        /// <summary>
        /// Field changing event
        /// </summary>
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

    /// <summary>
    /// Used for determining which property to group songs by
    /// </summary>
    public enum SongPropertyGroup { Artist, Album, Genre }
}
