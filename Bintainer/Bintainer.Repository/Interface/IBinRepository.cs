using Bintainer.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Interface
{
    public interface IBinRepository
    {
        public void SaveBin(Bin bin);
        public void UpdateBin(Bin bin);
    }
}
