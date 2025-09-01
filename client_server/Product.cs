using Product.Domain.Common;

namespace Product.Domain.Entities
{
    public class Product : AuditedEntity<Guid>
    {
        public Guid Product_ID { get; set; }
        public string Product_Code { get; set; }
        public string Product_Name { get; set; }
        public string Product_Description { get; set; }
        public Guid Organization_ID { get; set; }
        public string Control_Code { get; set; }
        public int User_Limit { get; set; }
        public byte Demo_Time { get; set; }
        public bool Is_Locked { get; set; }
        public bool Is_Portal { get; set; }
        public bool Is_Passive { get; set; }
        public bool Is_Demo { get; set; }
        public bool Is_Renewal { get; set; }
        public bool Is_Yearly { get; set; }
        public Guid Product_Type_ID { get; set; }
        public Guid Product_Producer_ID { get; set; }
        public Guid Product_Version_ID { get; set; }
        public Guid Product_Family_ID { get; set; }
        public Guid Product_Segment_ID { get; set; }
        public Guid Product_Module_ID { get; set; }
        public Guid Product_Register_Type_ID { get; set; }
        public Guid Product_Register_Reason_Type_ID { get; set; }
        public Guid Product_Register_Renewal_Type_ID { get; set; }
        public string ERP_Code { get; set; }
        public string Product_IncludedModule { get; set; }
        public Guid Product_Group_ID { get; set; }
        public Guid Product_Platform_ID { get; set; }
        public byte License_Control_Type { get; set; }
        public byte License_Package_Type { get; set; }
        public string Logo_Address { get; set; }
    }
}
