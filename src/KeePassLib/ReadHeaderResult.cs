namespace KeePass
{
    public class ReadHeaderResult
    {
        /// <summary>
        /// Gets or sets the format of the database file.
        /// </summary>
        public FileFormats Format { get; set; }

        /// <summary>
        /// Gets or sets the headers of the database file.
        /// </summary>
        public FileHeaders Headers { get; set; }
    }
}