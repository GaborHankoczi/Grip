using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO.Everlink
{
    public record TableDTO(List<string> Headers, List<List<string>> Rows);
}