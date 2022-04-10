using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivanov_WPF_MusicPlayer.Classes
{
    public class Song
    {
        private string number;
        private string name;
        private string path;

        public Song(string number, string name, string url)
        {
            this.number = number;
            this.name = name;
            this.path = url;
        }

        public string SongNumber
        {
            get { return number; }
            set { number = value; }
        }

        public string SongName
        {
            get { return name; }
            set { name = value; }
        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }
    }

    // ObservableCollection автоматически обновляет элементы пользовательского интерфейса, связанные с полем DataContext
    public class Songs : ObservableCollection<Song>
    {
        public Songs() { }

        public void Add(string number, string name, string path)
        {
            Add(new Song(number, name, path));
        }
    }
}
