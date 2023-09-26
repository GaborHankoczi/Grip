using System;
using System.ComponentModel.DataAnnotations;

namespace Grip.Bll.DTO
{
    /// <summary>
    /// Represents the data transfer object for creating an exemption.
    /// </summary>
    public record CreateExemptDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateExemptDTO"/> class.
        /// </summary>
        /// <param name="issuedToId"></param>
        /// <param name="validFrom"></param>
        /// <param name="validTo"></param>
        public CreateExemptDTO(int issuedToId, DateTime validFrom, DateTime validTo)
        {
            IssuedToId = issuedToId;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        /// <summary>
        /// Gets or sets the ID of the entity to which the exemption is issued.
        /// </summary>
        [Required]
        public int IssuedToId { get; init; }

        /// <summary>
        /// Gets or sets the date and time from which the exemption is valid.
        /// </summary>
        [Required]
        public DateTime ValidFrom { get; init; }

        /// <summary>
        /// Gets or sets the date and time until which the exemption is valid.
        /// </summary>
        [Required]
        public DateTime ValidTo { get; init; }
    }
}
