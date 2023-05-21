using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.DTO;

/// <summary>
/// Represents the data transfer object for a student absence.
/// </summary>
/// <param name="Class">Info about the class the student was absent from</param>
/// <param name="HasExempt">Wether the student had an exemption that was valid at the time</param>
public record AbsenceDTO(ClassDTO Class, bool HasExempt);
