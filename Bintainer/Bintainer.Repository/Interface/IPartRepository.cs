﻿using Bintainer.Model.DTO;
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
        public Part? GetPart(Guid partGuidId);
        public Part? GetPart(string partNumber,string userId);
        public List<Part>? GetAllPartsInCategory(int? categoryId,string userId);
        public List<Part>? GetAllPartsInGroup(string groupName,string userId);
        public Part? UpdatePart(Part? part);
        public List<Part> GetParts(PartFilterCriteria criteria);
        public PartPackage GetOrCreatePackage(string packageName, string userId);
        public PartGroup? GetPartGroup(string name, string userId);        
        public void UpdatePartQuantityInsideSubspace(BinSubspace subspace, int partId, int partQuantity);
        public void RemovePartQuantity(Bin bin, Part part, Dictionary<int, int> subspaceQuantity, bool? isFillAll = false, int? quantity = null);
        public List<PartBinAssociation>? UpdatePartBinassociations(List<PartBinAssociation>? associations);
        public List<string> GetPartNameList(string userId);
        public List<string> GetGroupNameList(string userId);
        public List<PartGroup> CreateOrUpdateGroup(string?[] groupNames, string userId);
        public List<Part>? GetPartsOfUser(string userId);
    }
}
