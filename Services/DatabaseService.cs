using FullStackRebelsTestCSharp.DB;
using FullStackRebelsTestCSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace FullStackRebelsTestCSharp.Services
{
    public class DatabaseService
    {
        DatabaseContext _databaseContext;

        public DatabaseService(IServiceProvider services, DbContextOptions dbOptions)
        {
            _databaseContext = new DatabaseContext(dbOptions);
        }

        public async Task AddShow(Show showToAdd)
        {
            // check that the item is not already in the db
            var isAlreadySaved = await _databaseContext.Shows.FindAsync(showToAdd.Id);
            if (isAlreadySaved == null)
            {
                // remove the person from the object
                await _databaseContext.Shows.AddAsync(showToAdd);
                await _databaseContext.SaveChangesAsync();
            }

            return;
        }

        public async Task<bool> AlreadyInDatabase(Show show)
        {
            return await _databaseContext.Shows.FindAsync(show.Id) != null;
        }

        public async Task<List<Show>> GetResults(int limit, int currentPage)
        {
            List<Show> shows = _databaseContext.Shows.Include(show => show.Cast).Skip(limit * currentPage).Take(limit).ToList();
            foreach (Show show in shows)
            {
                show.Cast.Sort((x, y) => DateTime.Compare(y.Birthday.GetValueOrDefault(), x.Birthday.GetValueOrDefault()));
            }
            return shows;
        }
    }
}
