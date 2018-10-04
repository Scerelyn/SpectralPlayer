using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibraryLib
{
    /// <summary>
    /// Represents a music library of saved songs
    /// </summary>
    [Serializable]
    public class Library
    {
        #region Public Properties

        /// <summary>
        /// The list of every song in the music library
        /// </summary>
        public ObservableCollection<Song> SongList { get; } = new ObservableCollection<Song>();

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
}
