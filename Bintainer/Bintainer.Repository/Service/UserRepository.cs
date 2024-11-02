using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Service
{
    public class UserRepository : IUserRepository
    {
        private readonly BintainerDbContext _dbContext;
        public UserRepository(BintainerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public AspNetUser? GetUser(string userId)
        {
            return _dbContext.AspNetUsers.Where(u=>u.Id == userId).FirstOrDefault();
        }
    }
}
