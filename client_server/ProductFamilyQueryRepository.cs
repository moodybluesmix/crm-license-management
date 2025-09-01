using Microsoft.Extensions.Configuration;
using Product.Application.Repositories.Query;
using Product.Persistence.Repository.Query.Base;

namespace ProductFamily.Persistence.Repository.Query
{
    internal class ProductFamilyQueryRepository : QueryRepository<Product.Domain.Entities.ProductFamily>, IProductFamilyQueryRepository
    {
        public ProductFamilyQueryRepository(IConfiguration configuration)
            : base(configuration)
        {

        }
    }
}
