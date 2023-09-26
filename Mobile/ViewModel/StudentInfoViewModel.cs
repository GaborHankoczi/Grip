using CommunityToolkit.Mvvm.ComponentModel;
using GripMobile.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.ViewModel
{
    public partial class StudentInfoViewModel : ObservableObject
    {
        [ObservableProperty]
        private StudentDetailDTO student;

        public StudentInfoViewModel(StudentDetailDTO student)
        {
            Student = student;
        }
    }
}
