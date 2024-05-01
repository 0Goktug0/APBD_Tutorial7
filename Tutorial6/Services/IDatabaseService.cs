using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tutorial6.Dto;

namespace Tutorial6.Services
{
    public interface IDatabaseService
    {
        public bool CheckData(int IdProduct, int IdWarehouse, int Amount);

        public bool CheckOrder(int IdProduct, int Amount, DateTime CreatedAt);
        public bool IfCompleted(int IdOrder);
        public void UpdateFullfill(int IdProduct);
        public void RegisterProduct(int IdProduct, int IdWarehouse, int Amount, DateTime CreatedAt);
        string AddProductUsingStoredProc(Warehouse warehouse);
    }
}
