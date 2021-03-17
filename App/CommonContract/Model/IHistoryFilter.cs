using System;

namespace ProjectBranchSelector.Models
{
    public interface IHistoryFilter
    {
        DateTimeOffset? From { get; set; }
        Guid? Id { get; set; }
        DateTimeOffset? To { get; set; }
    }
}