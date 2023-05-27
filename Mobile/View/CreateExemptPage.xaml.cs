using GripMobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateExemptPage : ContentPage
    {
        public CreateExemptPage(CreateExemptViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}