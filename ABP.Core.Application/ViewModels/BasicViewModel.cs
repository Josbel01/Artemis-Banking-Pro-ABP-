using System;

namespace ABP.Core.Application.ViewModels
{
    public class BasicViewModel<TKey>
    {
        public required TKey Id { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Updated { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
