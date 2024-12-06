using GeoPoint.Data;
using GeoPoint.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPoint
{
    public class GeoService
    {
        public static readonly GeoContext _context = new();
        public async Task<List<GeoDatum>> GetKmlsAsync()
            => await _context.GeoData.ToListAsync();
    }
}
