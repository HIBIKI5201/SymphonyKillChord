namespace SinfoniaStudio.SinfoniaOperator
{
    internal class NotionTaskListReader
    {
        public NotionTaskListReader(
            string notionToken,
            string databaseID,
            string datePropertyName,
            string namePropertyName)
        {
            _notionToken = notionToken;
            _databaseID = databaseID;
            _datePropertyName = datePropertyName;
            _namePropertyName = namePropertyName;
        }

        private readonly string _notionToken;
        private readonly string _databaseID;
        private readonly string _datePropertyName;
        private readonly string _namePropertyName;
    }
}
