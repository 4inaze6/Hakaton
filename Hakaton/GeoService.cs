using Hakaton.Data;
using Hakaton.Models;
using Microsoft.EntityFrameworkCore;

namespace Hakaton
{
    public class GeoService
    {
        public static readonly GeoContext _context = new();

        public async Task AddGeoData(GeoDatum geoDatum)
        {
            await _context.GeoData.AddAsync(geoDatum);
            _context.SaveChanges();
        }

        public async Task<bool> SearchGeoData(GeoDatum geoDatum)
        {
            var data = await _context.GeoData.FirstOrDefaultAsync(g => g.ImagePath == geoDatum.ImagePath);
            if (data != null)
            {
                return false;
            }
            return true;
        }
    }
}
