using System;

namespace ProjectBranchSelector.Contract
{

    public class TreeModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

    }

    public class TreeFilter: Filter
    {
        public string Name { get; set; }
    }

    public class Filter
    { 
        public int Page { get; set; }
        public int Size { get; set; }
        public string Sort { get; set; }
    }
}
