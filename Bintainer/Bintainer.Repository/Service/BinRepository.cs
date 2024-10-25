using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Service
{
    public class BinRepository:IBinRepository
    {
        readonly BintainerDbContext _dbContext;
        public BinRepository(BintainerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void SaveBin(Bin bin)
        {
            _dbContext.Bins.Add(bin);
            _dbContext.SaveChanges();

        }
        public void UpdateBin(Bin bin)
        {
            _dbContext.Bins.Update(bin);
            _dbContext.SaveChanges(true);
        }
    }
}
