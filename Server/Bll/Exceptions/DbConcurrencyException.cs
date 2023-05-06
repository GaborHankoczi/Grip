using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Exceptions
{
    public class DbConcurrencyException : Exception
    {
        public DbConcurrencyException(string? message = null) : base(message)
        {
        }
    }
}