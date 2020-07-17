using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Product.gRPC.API.Repositories
{
    public interface IProductRepository
    {
        
        Task<IEnumerable<Product.gRPC.API.Models.Product>> GetAll();
        Task<IEnumerable<Product.gRPC.API.Models.Product>> GetSpecific(int id);
    }
}
