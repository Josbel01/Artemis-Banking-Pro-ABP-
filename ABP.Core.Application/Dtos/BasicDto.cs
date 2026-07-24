using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Core.Application.Dtos
{
    public class BasicDto<TKey>
    {
        public required TKey Id { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Updated { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
