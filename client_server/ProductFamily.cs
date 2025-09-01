using Product.Domain.Common;

namespace Product.Domain.Entities
{
    public class ProductFamily : AuditedEntity<Guid>
    {
        public Guid Product_Family_ID { get; set; }
        public Guid Organization_ID { get; set; }
        public Guid Product_Producer_ID { get; set; }
        public Guid Product_Version_ID { get; set; }
        public string Product_Family_Code { get; set; }
        public string Product_Family_Name { get; set; }
        public string Control_Code { get; set; }
    }
}
