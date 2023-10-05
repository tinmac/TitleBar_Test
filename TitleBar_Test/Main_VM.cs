using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitleBar_Test 
{
    public class Main_VM : WinUiBaseINPC
    {

        #region PROPERTIES

        private bool light;
        private bool dark;
        private bool plain;
        private bool mica;
        private bool acrylic;

        public bool IsLight
        {
            get => light;
            set
            {
                Set(ref light, value);// 
                // Debug.WriteLine($"IsLight Setter {value}");

                //if (IsLight)
                //    ChangeTheme("Light");

                // IsDark = false;
            }
        }

        public bool IsDark
        {
            get => dark;
            set
            {
                Set(ref dark, value);

                //if (IsDark)
                //    ChangeTheme("Dark");
               
                // IsLight = false;
            }
        }

        public bool IsPlain
        {
            get => plain;
            set
            {
                Set(ref plain, value);

                //Settings_Local_WinUi.IsMica = false;
                //Settings_Local_WinUi.IsAcrylic = false;
                ////ChangeBackdrop();
                //WinMarshall.SetBackdrop();
            }
        }

        public bool IsMica
        {
            get => mica;
            set
            {
                Set(ref mica, value);

                if (IsMica)
                {
                    //Settings_Local_WinUi.IsMica = true;
                    //Settings_Local_WinUi.IsAcrylic = false;
                    ////ChangeBackdrop();
                    //WinMarshall.SetBackdrop();
                }
            }
        }

        public bool IsAcrylic
        {
            get => acrylic;
            set
            {
                Set(ref acrylic, value);

                if (IsAcrylic)
                {
                    //Settings_Local_WinUi.IsMica = false;
                    //Settings_Local_WinUi.IsAcrylic = true;
                    //// ChangeBackdrop();
                    //WinMarshall.SetBackdrop();
                }
            }
        }


        private string _SelectedTheme;
        public string SelectedTheme
        {
            get => _SelectedTheme;
            set
            {
                Set(ref _SelectedTheme, value);
            }
        }

        private string _ChangeStatus;
        public string ChangeStatus
        {
            get => _ChangeStatus;
            set
            {
                Set(ref _ChangeStatus, value);
            }
        }

        #endregion


        // ctor
        public Main_VM()
        {
                
        }




    }
}
