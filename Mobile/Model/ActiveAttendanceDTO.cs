namespace GripMobile.Model
{
    /// <summary>
    /// Class <c>ActiveAttendanceDTO</c> represents a Data Transfer Object and used when the user holds their phone to the NFC station.
    /// </summary>
    public class ActiveAttendanceDTO
    {
        /// <value>Property <c>Message</c> contains the station number, unix time and a salt separated with an "_".</value>
        public string Message { get; set; }

        /// <value>Property <c>Token</c> represents a signed token got back from the station.</value>
        public string Token { get; set; }
    }
}
