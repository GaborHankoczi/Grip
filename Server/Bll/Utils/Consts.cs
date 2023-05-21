using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Utils
{
    /// <summary>
    /// Contains constants
    /// </summary>
    public static class Consts
    {
        /// <summary>
        /// Regex representing a valid password
        /// </summary>
        public const string UserPasswordRegex = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$";

        /// <summary>
        /// Regex representing a valid username
        /// </summary>
        public const string UserNameRegex = @"^[a-zA-Z-ÁÉÍŐÚŰÓÜÖáéíőúűöüó. ]+$";

    }
}