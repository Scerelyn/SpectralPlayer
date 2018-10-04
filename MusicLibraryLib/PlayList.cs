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
    /// A collection of songs put together by the user
    /// </summary>
    [Serializable]
    public class PlayList
    {
        #region Private fields

        private string _name = "";
        
        #endregion

        #region Public Properties

        /// <summary>
        /// The list of songs on this playlist
        /// </summary>
        public ObservableCollection<Song> SongList { get; } = new ObservableCollection<Song>();

        /// <summary>
        /// The name of the playlist
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; FieldChanged(); }
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
