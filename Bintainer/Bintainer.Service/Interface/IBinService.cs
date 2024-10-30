using Bintainer.Model.Entity;
using Bintainer.Model.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service.Interface
{
    public interface IBinService
    {
        public Response<Dictionary<int, int>?> DistributeQuantityAcrossSubspaces(in Bin bin, int totalQuantity);
        public Response<Bin> UpdateBin(Bin bin);
        public Response<Bin> SaveBin(Bin bin);
    }
}
