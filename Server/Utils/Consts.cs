using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Utils
{
    public static class Consts
    {
        public const string UserPasswordRegex = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$";
        public const string UserNameRegex = @"^[a-zA-Z-ÁÉÍŐÚŰÓÜÖáéíőúűöüó. ]+$";
        
    }
}