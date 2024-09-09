using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crud.Infrastructure.Dtos
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();

        [DefaultValueAttribute(0)]
        public int TotalCount { get; set; }
    }
}
