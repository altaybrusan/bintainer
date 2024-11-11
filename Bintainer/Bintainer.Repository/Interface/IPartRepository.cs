using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Interface
{
    public interface IPartRepository
    {
        public Part? GetPart(string partNumber,string userId);
        public Part? UpdatePart(Part? part);
        public List<Part> GetParts(PartFilterCriteria criteria);
        public PartPackage GetOrCreatePackage(string packageName, string userId);
        public PartGroup? GetPartGroup(string name, string userId);
        //public bool TryInsertPartIntoBin(List<int> targetIndices, string label, ref Part part, ref Bin bin);
        public void UpdatePartQuantityInsideSubspace(BinSubspace subspace, int partId, int partQuantity);
        public List<PartBinAssociation>? UpdatePartBinassociations(List<PartBinAssociation>? associations);
        public List<string> GetPartNameList(string userId);
        public List<PartGroup> CreateOrUpdateGroup(string?[] groupNames, string userId);
    }
}
