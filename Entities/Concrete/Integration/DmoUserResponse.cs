using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.Integration
{
    public class DmoUserResponse
    {
        public Guid Id { get; set; }
        public string EbysId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string DepartmentId { get; set; }
        public string ParentDepartmentId { get; set; }
        public string BranchId { get; set; }
        public string ParentBranchId { get; set; }
        public SubeTur BranchType { get; set; }
        public string Position { get; set; }
        public PositionType PositionType
        {
            get
            {
                if (!string.IsNullOrEmpty(Position) && Position.Contains("Başkan")) return PositionType.Chief;
                else if (!string.IsNullOrEmpty(Position) && Position.Contains("Müdür")) return PositionType.Manager;
                else return PositionType.Staff;
            }
        }
        public string Phone { get; set; }
        public string AdminName { get; set; }
        public List<string> Roles { get; set; } = [];
    }

    public enum SubeTur
    {
        Unspecified,
        Catalog,
        PriceResearch,
        Tender
    }
    public enum PositionType
    {
        Chief = 1,
        Manager,
        Staff
    }
}
