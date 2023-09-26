using GripMobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateGroupPage : ContentPage
    {
        public CreateGroupPage(CreateGroupViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}