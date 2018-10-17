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

        #region Public Methods

        /// <summary>
        /// Removes and returns the first song in the playlist
        /// </summary>
        /// <returns>The first song in the playlist, or null if there are no new songs</returns>
        public Song PopNextSong()
        {
            if (SongList.Count() <= 0)
            {
                return null;
            }
            else
            {
                Song popped = SongList.First();
                SongList.RemoveAt(0);
                return popped;
            }
        }

        /// <summary>
        /// Inserts an IEnumerable of Song objects randomly into SongList
        /// </summary>
        /// <param name="songsToAdd">The songs to insert randomly</param>
        public void ShuffleSongsInto(IEnumerable<Song> songsToAdd)
        {
            Random r = new Random();
            foreach (Song s in songsToAdd)
            {
                SongList.Insert(r.Next(songsToAdd.Count()), s);
            }
        }

        /// <summary>
        /// Randomizes the order of songs in SongList
        /// </summary>
        public void Shuffle()
        {
            Random r = new Random();
            ObservableCollection<Song> newSongList = new ObservableCollection<Song>();
            while(SongList.Count() > 0)
            {
                // randomly get via index, remove and add to a new list
                int randIndex = r.Next(SongList.Count());
                Song rand = SongList[randIndex];
                SongList.RemoveAt(randIndex);
                newSongList.Add(rand);
            }
            // neewSongList is now in a new random order, so readd to SongList and its shuffled
            foreach (Song s in newSongList)
            {
                SongList.Add(s);
            }
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
