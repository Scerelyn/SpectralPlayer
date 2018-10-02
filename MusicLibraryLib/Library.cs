using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibraryLib
{
    [Serializable]
    public class Library
    {
        #region Public Properties

        public ObservableCollection<Song> SongList { get; } = new ObservableCollection<Song>();

        #endregion

        #region Public methods

        public IEnumerable<Song> GetOrderedListBySong()
        {
            return SongList.OrderBy(song => song.Name);
        }

        public IEnumerable<Song> GetOrderedListByAlbum()
        {
            return SongList.OrderBy(song => song.AlbumName).ThenBy(song => song.TrackNumber);
        }

        public IEnumerable<Song> GetOrderedListByArtist()
        {
            return SongList.OrderBy(song => song.Artist).ThenBy(song => song.AlbumName).ThenBy(song => song.TrackNumber);
        }

        public IEnumerable<Song> GetOrderedListByGenre()
        {
            return SongList.OrderBy(song => song.Genre);
        }

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
