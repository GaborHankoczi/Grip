namespace Grip.Bll.DTO
{
    /// <summary>
    /// Represents a data transfer object (DTO) for an exemption.
    /// </summary>
    public record ExemptDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExemptDTO"/> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="issuedBy"></param>
        /// <param name="issuedTo"></param>
        /// <param name="validFrom"></param>
        /// <param name="validTo"></param>
        public ExemptDTO(int id, UserInfoDTO issuedBy, UserInfoDTO issuedTo, DateTime validFrom, DateTime validTo)
        {
            Id = id;
            IssuedBy = issuedBy;
            IssuedTo = issuedTo;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }
        /// <summary>
        /// Gets or sets the ID of the exemption.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets or sets the information about the user who issued the exemption.
        /// </summary>
        public UserInfoDTO IssuedBy { get; init; } = null!;

        /// <summary>
        /// Gets or sets the information about the user to whom the exemption is issued.
        /// </summary>
        public UserInfoDTO IssuedTo { get; init; } = null!;

        /// <summary>
        /// Gets or sets the date and time from which the exemption is valid.
        /// </summary>
        public DateTime ValidFrom { get; init; }

        /// <summary>
        /// Gets or sets the date and time until which the exemption is valid.
        /// </summary>
        public DateTime ValidTo { get; init; }
    }
}
