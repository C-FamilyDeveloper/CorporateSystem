using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateSystem.Auth.Domain.Entities
{
    public class Department
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public IEnumerable<User> Users { get; init; }
    }
}
