using Microsoft.EntityFrameworkCore;
using WebApplication3.Interface;
using WebApplication3.Maping;
using WebApplication3.Models.Enum;
using WebApplication3.Models;

namespace WebApplication3.Repository
{
    public class DishRepository : IDishRepositry
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DishRepository> _logger; // Added logger
        public DishRepository(ApplicationDbContext context, ILogger<DishRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        /// Checks if a dish exists with the given ID.
        public bool DishExists(Guid Id)
        {
            try
            {
                return _context.Dishes.Any(d => d.Id == Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if dish exists.");
                return false;
            }

        }

        /// Retrieves a single dish by its ID.
        public Dish GetDish(Guid Id)
        {
            try
            {
                return _context.Dishes.Where(d => d.Id == Id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dish.");
                return null; // Log error and return null
            }

        }

        /// Retrieves all dishes.
        public ICollection<Dish> GetDishes()
        {
            try
            {
                return _context.Dishes.OrderBy(d => d.Id).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all dishes.");
                return new List<Dish>(); // Log error and return empty list
            }
        }
        public async Task<ICollection<Dish>?> GetDishes(Category[]? categories, bool? Vegetarian, DishSorting? sortBy, int page = 1)
        {
            try
            {
                IQueryable<Dish> query = _context.Dishes;

                if (categories != null && categories.Length > 0)
                {
                    query = query.Where(d => categories.Contains(d.Category));
                }
                if (Vegetarian.HasValue)
                {
                    query = query.Where(d => d.Vegetarian == Vegetarian.Value);
                }

                if (sortBy.HasValue)
                {
                    query = sortBy switch
                    {
                        DishSorting.NameAsc => query.OrderBy(d => d.Name),
                        DishSorting.NameDesc => query.OrderByDescending(d => d.Name),
                        DishSorting.PriceAsc => query.OrderBy(d => d.Price),
                        DishSorting.PriceDesc => query.OrderByDescending(d => d.Price),
                        DishSorting.RatingAsc => query.OrderBy(d => _context.Ratings.Where(r => r.DishId == d.Id).Average(r => (double?)r.Score) ?? 0.0),
                        DishSorting.RatingDesc => query.OrderByDescending(d => _context.Ratings.Where(r => r.DishId == d.Id).Average(r => (double?)r.Score) ?? 0.0),
                        _ => query.OrderBy(d => d.Id)
                    };
                }
                else
                {
                    query = query.OrderBy(d => d.Id);
                }

                query = query.Skip((page - 1) * 10);
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dishes with filters and sorting.");
                return null;
            }
        }
    }
}


