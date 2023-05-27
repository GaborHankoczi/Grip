using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripMobile.Model.Api
{
    public partial class UserDTO
    {
        public override string ToString()
        {
            return this.UserName;
        }
    }
    public partial class GroupDTO
    {
        public override string ToString()
        {
            return this.Name;
        }
    }

    public partial class UserInfoDTO
    {
        public override string ToString()
        {
            return this.UserName;
        }
    }
}
