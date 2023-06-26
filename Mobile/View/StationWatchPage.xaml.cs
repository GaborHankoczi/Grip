using GripMobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StationWatchPage : ContentPage
    {
        public StationWatchPage(StationWatchViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}