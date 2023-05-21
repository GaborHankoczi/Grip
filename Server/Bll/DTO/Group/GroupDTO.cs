using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO;

/// <summary>
/// Represents the data transfer object for a group.
/// </summary>
/// <param name="Id">Id of the group</param>
/// <param name="Name">Name of the group</param>
public record GroupDTO([Required] int Id, [Required] string Name);