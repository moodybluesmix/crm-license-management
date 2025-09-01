using Product.Domain.Common;

namespace Product.Domain.Entities
{
    public class ProductGroup : AuditedEntity<Guid>
    {
        public Guid Product_Group_ID { get; set; }
        public Guid Product_Group_Code { get; set; }
        public Guid Product_Group_Name { get; set; }
    }
}
