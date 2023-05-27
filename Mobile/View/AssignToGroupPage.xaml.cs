using GripMobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AssignToGroupPage : ContentPage
    {
        public AssignToGroupPage(AssignToGroupViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}