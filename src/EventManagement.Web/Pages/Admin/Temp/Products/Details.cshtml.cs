using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using losol.EventManagement.Data;
using losol.EventManagement.Domain;

namespace losol.EventManagement.Pages.Admin.Temp.Products
{
    public class DetailsModel : PageModel
    {
        private readonly losol.EventManagement.Data.ApplicationDbContext _context;

        public DetailsModel(losol.EventManagement.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Product Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product = await _context.Products
                .Include(p => p.Eventinfo).SingleOrDefaultAsync(m => m.ProductId == id);

            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
