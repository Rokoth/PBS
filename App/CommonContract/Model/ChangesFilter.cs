namespace ProjectBranchSelector.Models
{
    public class ChangesFilter
    {
        public ChangesFilter(long lastHid, int size)
        {
            LastHid = lastHid;
            Size = size;
        }
        public long LastHid { get; set; }
        public int Size { get; set; }
    }
}
