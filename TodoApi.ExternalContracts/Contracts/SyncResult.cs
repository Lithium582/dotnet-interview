namespace TodoApi.ExternalContracts.Contracts
{
    public class SyncResult
    {

        public int ListCreations { get; set; }
        public int ListUpdates { get; set; }
        public int ListDeleted { get; set; }

        public int ItemCreations { get; set; }
        public int ItemUpdates { get; set; }
        public int ItemDeleted { get; set; }
    }
}
