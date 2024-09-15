using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Eventos.Models
{
    public class SelectionEventArgs : EventArgs
    {
        public bool IsSelected { get; set; } = true;
    }

    public class Bindableitem : INotifyPropertyChanged
    {
        private bool isSelected;
        private ICommand selectCommand;

        /// <summary>
        /// PropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// PropertyChanged
        /// </summary>
        public event EventHandler<SelectionEventArgs> SelectionChanged;


        /// <summary>
        /// Adds operators (manually) to the daily role
        /// </summary>
        public ICommand SelectCommand
        {
            get
            {
                return this.selectCommand ??= new Command(() =>
                {
                    this.IsSelected = !this.IsSelected;
                });
            }
        }


        /// <summary>
        /// Indicates if an item is being currently selected
        /// </summary>
        [NotMapped]
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (this.isSelected != value)
                {
                    if (this.SelectionChanged != null && value)
                    {
                        var args = new SelectionEventArgs { IsSelected = value };
                        SelectionChanged(this, args);
                        this.isSelected = args.IsSelected;
                        OnPropertyChanged(nameof(IsSelected));

                    }
                    else
                    {
                        this.isSelected = value;
                        OnPropertyChanged(nameof(IsSelected));
                    }
                }
            }
        }



        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="name"></param>
        public void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
