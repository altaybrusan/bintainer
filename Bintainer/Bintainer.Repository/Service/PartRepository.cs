using Azure.Core;
using Bintainer.Model;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bintainer.Repository.Service
{
    public class PartRepository : IPartRepository
    {
        BintainerDbContext _dbContext;
        
        public PartRepository(BintainerDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public Part? GetPart(string partNumber, string userId)
        {
            var part = _dbContext.Parts
                .Include(p => p.PartBinAssociations)
                    .ThenInclude(b => b.Bin)
                    .ThenInclude(b => b.BinSubspaces)
                .Include(p => p.OrderPartAssociations)
                .Include(p => p.PartAttributes)
                .Include(p => p.Package)
                .Include(p => p.Groups)
                // Load the category hierarchy recursively
                .Include(p => p.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Where(p => p.Number.Contains(partNumber) && p.UserId == userId)
                .FirstOrDefault();

            // Load the full parent category hierarchy
            if (part?.Category != null)
            {
                part.Category = LoadFullCategoryHierarchy(part.Category);
            }

            return part;
        }

        public Part? GetPart(Guid partGuidId) 
        {
            var part = _dbContext.Parts.Where(p => p.GuidId == partGuidId)
                                       .Include(p => p.PartBinAssociations)
                                           .ThenInclude(b => b.Bin)
                                           .ThenInclude(b => b.BinSubspaces)
                                       .Include(p => p.OrderPartAssociations)
                                       .Include(p => p.PartAttributes)
                                       .Include(p => p.Package)
                                       .Include(p => p.Groups)
                                       .Include(p => p.Category)
                                           .ThenInclude(c => c.ParentCategory)
                                       .Where(p => p.GuidId == partGuidId)
                                       .FirstOrDefault();
            if (part?.Category != null)
            {
                part.Category = LoadFullCategoryHierarchy(part.Category);
            }
            return part;
        }



        public List<Part> GetParts(PartFilterCriteria criteria)
        {
            
            var parameter = Expression.Parameter(typeof(Part), "p");
            Expression filterExpression = Expression.Constant(true);
                  
            if (!string.IsNullOrEmpty(criteria.Number))
            {
                var nameProperty = Expression.Property(parameter, "Number");
                var nameValue = Expression.Constant(criteria.Number);
                var nameEquals = Expression.Equal(nameProperty, nameValue);

                filterExpression = Expression.AndAlso(filterExpression, nameEquals);
            }

            if (!string.IsNullOrEmpty(criteria.Supplier))
            {
                var supplierProperty = Expression.Property(parameter, "Supplier");
                var supplierValue = Expression.Constant(criteria.Supplier);
                var supplierEquals = Expression.Equal(supplierProperty, supplierValue);

                filterExpression = Expression.AndAlso(filterExpression, supplierEquals);
            }

            if (!string.IsNullOrEmpty(criteria.UserId))
            {
                var userIdProperty = Expression.Property(parameter, "UserId");
                var userIdValue = Expression.Constant(criteria.UserId);
                var userIdEquals = Expression.Equal(userIdProperty, userIdValue);

                filterExpression = Expression.AndAlso(filterExpression, userIdEquals);
            }

            var lambda = Expression.Lambda<Func<Part,bool>>(filterExpression, parameter);

            return _dbContext.Parts.Where(lambda).ToList();
        }

        public PartPackage GetOrCreatePackage(string packageName, string userId) 
        {
            PartPackage? package = _dbContext.PartPackages.FirstOrDefault(p => p.Name == packageName && p.UserId == userId);

            if (package is null)
            {
                 package = new PartPackage() { Name = packageName, UserId = userId };
                _dbContext.PartPackages.Add(package);
                _dbContext.SaveChanges(true);                
            }
            return package;
        }

        public Part? UpdatePart(Part? part)
        {
            if(part is not null) 
            {
                _dbContext.Parts.Update(part);
                _dbContext.SaveChanges(true);
            }
            return part;
        }
        
        public PartGroup? GetPartGroup(string name, string userId) 
        {
            return _dbContext.PartGroups.Where(g => g.Name == name && g.UserId == userId).FirstOrDefault();

        }

        //public bool TryInsertPartIntoBin(List<int> targetIndices, string label, ref Part part, ref Bin bin)
        //{
        //    foreach (int subSpaceIndex in targetIndices)
        //    {
        //        var targetSubspace = bin.BinSubspaces.Where(s => s.SubspaceIndex == subSpaceIndex).FirstOrDefault();
        //        if (targetSubspace is null)
        //        {
        //            // the bin's subspace is not already used
        //            targetSubspace = new BinSubspace()
        //            {
        //                SubspaceIndex = subSpaceIndex,
        //                Label = label
        //            };
        //            bin.BinSubspaces.Add(targetSubspace);
        //        }
        //        else
        //        {
        //            int partId = part.Id;
        //            int binId = bin.Id;
        //            int subspaceId = targetSubspace.Id;
        //            if (_dbContext.PartBinAssociations.Where(a => a.PartId == partId && a.SubspaceId == subspaceId && a.BinId == binId).Any())
        //            {
        //                // the incomming part is same as already on
        //            }
        //            else
        //            {

        //            }
        //        }
        //    }
        //    return true;
        //}

        public void UpdatePartQuantityInsideSubspace(BinSubspace subspace, int partId, int partQuantity)
        {
            var assoc = subspace.PartBinAssociations.Where(a => a.PartId == partId).FirstOrDefault();
            if (assoc is not null)
            {
                int updatedValue = assoc.Quantity + partQuantity;
                if (updatedValue < 0) { updatedValue = 0; }
                assoc.Quantity = updatedValue;
                _dbContext.PartBinAssociations.Update(assoc);
                _dbContext.SaveChanges();
            }

            return;
        }

        public void RemovePartQuantity(Bin bin, Part part, Dictionary<int, int> subspaceQuantity, bool? isFillAll = false, int? quantity = null)
        {
            if (isFillAll.HasValue && isFillAll.Value== true) 
            {
                //var assocs = _dbContext.PartBinAssociations.Where(a => a.PartId == part.Id && a.BinId == bin.Id).ToList();
                var assocs = part.PartBinAssociations.Where(a => a.BinId == bin.Id).ToList();
                //foreach (var assoc in assocs) 
                //{
                //    assoc.Quantity = 0;
                //}
                //_dbContext.PartBinAssociations.UpdateRange(assocs);

                _dbContext.PartBinAssociations.RemoveRange(assocs);
                _dbContext.SaveChanges(true);
                                
                var subSpaces = bin.BinSubspaces.ToList();
                _dbContext.BinSubspaces.RemoveRange(subSpaces);
                _dbContext.SaveChanges(true);

            }
            else 
            {
                var subspaceIndecies = subspaceQuantity.Keys.ToList();
                var assocs = part.PartBinAssociations.Where(a => a.BinId == bin.Id && subspaceIndecies.Contains(a.Subspace.SubspaceIndex!.Value)).ToList();                
                _dbContext.PartBinAssociations.RemoveRange(assocs);
                _dbContext.SaveChanges(true);

                var subSpaces = bin.BinSubspaces.Where(s => subspaceIndecies.Contains(s.SubspaceIndex!.Value)).ToList();
                _dbContext.BinSubspaces.RemoveRange(subSpaces);
                _dbContext.SaveChanges(true);



                //foreach (var index in subspaceQuantity.Keys)
                //{
                //    var subspace = bin.BinSubspaces.Where(s => s.SubspaceIndex == index).FirstOrDefault();
                //    var assoc = _dbContext.PartBinAssociations.Where(a => a.PartId == part.Id && a.BinId == bin.Id && a.SubspaceId == subspace.Id).FirstOrDefault();
                //    assoc.Quantity = 0;
                //    _dbContext.PartBinAssociations.Update(assoc);
                //    _dbContext.SaveChanges(true);
                //}
            }
        }

        public void RemoveAllPartQuantityInsideBin(int partId, int binId, int? partQuantity) 
        {
            var assocs = _dbContext.PartBinAssociations.Where(a => a.PartId == partId && a.BinId == binId).ToList();
            if (assocs is not null)
            {
                foreach (var assoc in assocs)
                {
                    assoc.Quantity = 0;
                }
                _dbContext.PartBinAssociations.UpdateRange(assocs);
                _dbContext.SaveChanges();
            }
            return;
        }

        public List<PartBinAssociation>? UpdatePartBinassociations(List<PartBinAssociation>? associations) 
        {
            if (associations is null)
                return associations;
            _dbContext.PartBinAssociations.UpdateRange(associations);
            _dbContext.SaveChanges();
            return associations;
        }

        public List<string> GetPartNameList(string userId)
        {
            return _dbContext.Parts.Select(p => p.Number.Trim()).ToList();
        }

        public List<PartGroup> CreateOrUpdateGroup(string?[] groupNames, string userId)
        {
            List<PartGroup> result = new List<PartGroup>();
            foreach (var groupName in groupNames) 
            {
                var existingGroup = _dbContext.PartGroups.Where(p => p.UserId == userId && p.Name!.Contains(groupName!)).FirstOrDefault();
                if (existingGroup is null) 
                {
                    PartGroup newGroup = new() 
                    {
                        Name = groupName,                       
                        UserId = userId,
                    };
                    result.Add(newGroup);
                    _dbContext.PartGroups.Add(newGroup);
                    _dbContext.SaveChanges(true);
                }
                else 
                {
                    result.Add(existingGroup);
                }
            }
            return result;
        }

        private PartCategory LoadFullCategoryHierarchy(PartCategory category)
        {
            while (category.ParentCategory != null)
            {
                category.ParentCategory = _dbContext.PartCategories
                    .Include(c => c.ParentCategory)
                    .FirstOrDefault(c => c.Id == category.ParentCategory.Id);

                category = category.ParentCategory;
            }
            return category;
        }


    }
}
