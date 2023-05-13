using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO;
public record AbsenceDTO(ClassDTO Class, bool HasExempt);
