using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

using Assembly_Browser;

namespace View
{
    class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {
            Namespaces = new List<Assembly_Browser.Container>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly AssemblyBrowser assemblyBrowser = new AssemblyBrowser();
        public List<Assembly_Browser.Container> Namespaces { get; set; }

        private string _openedFile;

        public string OpenedFile
        {
            get
            {
                return _openedFile;
            }
            set
            {
                _openedFile = value;
                Namespaces = null;
                try
                {
                    Namespaces = new List<Assembly_Browser.Container>(assemblyBrowser.GetAssemblyInfo(value));
                }
                catch (Exception e)
                {
                    _openedFile = string.Format("Error: [{0}]", e.Message);
                }
                OnPropertyChanged(nameof(Namespaces));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand OpenFile { get { return new OpenFileCommand(OpenAssembly); } }

        public void OpenAssembly()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Assemblies|*.dll;*.exe";
                openFileDialog.Title = "Select assembly";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    OpenedFile = openFileDialog.FileName;
                    OnPropertyChanged(nameof(OpenedFile));
                }
            }
        }
    }
}
