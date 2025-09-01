using System;

namespace Member_System.Models.Company
{
    public class UserCompanyRegistryPointIdModel
    {
        public Guid CompanyId { get; set; }
        public Guid OrganizationId { get; set; }
        public string UserCompany_Registry_Point_Code { get; set; }
        public Guid UserCompany_Registry_Point_ID { get; set; }
        public string UserCompany_Registry_Point_Name { get; set; }
        public Guid Created_User { get; set; }
    }
}
