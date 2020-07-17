using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Product.gRPC.API.Repositories
{
    public class MockProductRepository : IProductRepository
    {
        private readonly List<Models.Product> _listOfProducts;

        public MockProductRepository()
        {
            _listOfProducts = new List<Models.Product>();
            _listOfProducts.Add(new Models.Product()
            {
                Id = 1,
                Name = "Mug",
                Price = 1.2,
                isAvailableToPurchase = true
            });
            _listOfProducts.Add(new Models.Product()
            {
                Id = 2,
                Name = "Cup",
                Price = 2.1,
                isAvailableToPurchase = false
            });
            _listOfProducts.Add(new Models.Product()
            {
                Id = 3,
                Name = "Shirt",
                Price = 4.1,
                isAvailableToPurchase = true
            });
        }

        public async Task<IEnumerable<Models.Product>> GetAll()
        {
            return _listOfProducts;
        }

        public async Task<IEnumerable<Models.Product>> GetSpecific(int id)
        {            
            return _listOfProducts.FindAll((options) => options.Id == id);
        }
    }
}
