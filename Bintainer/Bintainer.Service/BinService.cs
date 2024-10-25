using Bintainer.Model.Entity;
using Bintainer.Repository.Interface;
using Bintainer.Repository.Service;
using Bintainer.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service
{
    public class BinService : IBinService
    {
        readonly IBinRepository _binRepository;
        readonly IInventoryRepository _inventoryRepository;
        public BinService(IBinRepository binRepository, IInventoryRepository inventoryRepository)
        {
            _binRepository = binRepository;
            _inventoryRepository = inventoryRepository;
        }

        public void UpdateBin(Bin bin)
        {
            _binRepository.UpdateBin(bin);
        }

        public void SaveBin(Bin bin)
        {
            _binRepository.SaveBin(bin);
        }

        public Dictionary<int,int> DistributeQuantityAcrossSubspaces(in Bin bin, int totalQuantity)
        {
            //TODO: should protect agianst null value.
            int subspaceCount = bin.Section.SubspaceCount.Value;
            int[] quantities = DividePartsEvenly(totalQuantity, subspaceCount);
            Dictionary<int, int> subspaceQuantity = new Dictionary<int, int>();

            for (int i = 1; i <= quantities.Length; i++)
            {
                subspaceQuantity.Add(i, quantities[i - 1]);
            }

            return subspaceQuantity;
        }

        private int[] DividePartsEvenly(int totalParts, int totalSubspaces)
        {
            int[] result = new int[totalSubspaces];
            int baseDivision = totalParts / totalSubspaces;
            int remainder = totalParts % totalSubspaces;

            for (int i = 0; i < totalSubspaces; i++)
            {
                result[i] = baseDivision;
            }

            for (int i = 0; i < remainder; i++)
            {
                result[i]++;
            }

            return result;
        }


    }
}
