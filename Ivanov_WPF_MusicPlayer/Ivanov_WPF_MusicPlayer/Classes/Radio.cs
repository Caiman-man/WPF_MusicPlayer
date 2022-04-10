using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivanov_WPF_MusicPlayer.Classes
{
    public class Radio
    {
        private string number;
        private string name;
        private string url;

        public Radio(string number, string name, string url)
        {
            this.number = number;
            this.name = name;
            this.url = url;
        }

        public string RadioNumber
        {
            get { return number; }
            set { number = value; }
        }

        public string RadioName
        {
            get { return name; }
            set { name = value; }
        }

        public string URL
        {
            get { return url; }
            set { url = value; }
        }
    }

    // ObservableCollection автоматически обновляет элементы пользовательского интерфейса, связанные с полем DataContext
    public class RadioStations : ObservableCollection<Radio>
    {
        public RadioStations() { }

        public void Add(string number, string name, string url)
        {
            Add(new Radio(number, name, url));
        }
    }
}
