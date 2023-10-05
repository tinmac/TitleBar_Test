using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.CompilerServices;

//using Microsoft.Toolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TitleBar_Test
{
    //Microsoft.UI.Xaml.Data.INotifyPropertyChanged
    public class WinUiBaseINPC : INotifyPropertyChanged //, ObservableRecipient   ObservableObject implements & replaces INotifyPropertyChanged
    {
        //// Base Props
        //private bool isBusy = false;
        //public bool IsBusy
        //{
        //    get => isBusy;
        //    set
        //    {
        //        Set(ref isBusy, value);
        //    }
        //}


        // INPC
        // [NotMapped]
        // public bool IsDirty { get; private set; }

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword


        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {

            if (Object.Equals(storage, value))
                return false;

            storage = value;

            // IsDirty = true;

            //if (propertyName == "AlwaysShowTheDocumentSendOptions")
            //Debug.WriteLine($"WinUiBaseINPC   OnPropertyChanged: {propertyName} - {value}");

            OnPropertyChanged(propertyName);
            return true;
        }


#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        protected void OnPropertyChanged(string propertyName)
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            //if (propertyName == "AlwaysShowTheDocumentSendOptions")
            // Debug.WriteLine($"WinUiBaseINPC    OnPropertyChanged: {propertyName}");

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
