using Microsoft.EntityFrameworkCore;
using WebApplication3.Interface;
using WebApplication3.Maping;
using WebApplication3.Models.Enum;
using WebApplication3.Models;
using AutoMapper;
using WebApplication3.Dtos.Rating_Dto;
using WebApplication3.Dtos.DishDTo;

namespace WebApplication3.Repository
{
    public class DishRepository : IDishRepositry
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<DishRepository> _logger;

        public DishRepository(ApplicationDbContext context, ILogger<DishRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// Checks if a dish exists with the given ID.
        public bool DishExists(Guid id)
        {
            try
            {
                return _context.Dishes.Any(d => d.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if dish exists.");
                throw; // Re-throw to indicate failure
            }
        }

        /// Retrieves a single dish by its ID.
        public Dish GetDish(Guid id)
        {
            try
            {
                var dish = _context.Dishes.FirstOrDefault(d => d.Id == id);
                if (dish == null)
                {
                    throw new KeyNotFoundException($"Dish with ID {id} not found.");
                }
                return dish;
            }
            catch (KeyNotFoundException)
            {
                throw;// Re-throw KeyNotFoundException to let the caller know it can't be found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dish.");
                throw; // Re-throw to indicate a generic error
            }
        }


        /// Retrieves all dishes.
        public async Task<Dish?> GetDishAsync(Guid id)
        {
            try
            {
                var dish = await _context.Dishes.FirstOrDefaultAsync(d => d.Id == id);
                return _mapper.Map<Dish>(dish);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving dish with ID {id}");
                throw;
            }
        }

        public async Task<ICollection<Dish>> GetDishes(Category[]? categories, bool? vegetarian, DishSorting? sortBy, int page = 1)
        {
            try
            {
                IQueryable<Dish> query = _context.Dishes;

                if (categories != null && categories.Length > 0)
                {
                    query = query.Where(d => categories.Contains(d.Category));
                }
                if (vegetarian.HasValue)
                {
                    query = query.Where(d => d.Vegetarian == vegetarian.Value);
                }

                if (sortBy.HasValue)
                {
                    query = sortBy switch
                    {
                        DishSorting.NameAsc => query.OrderBy(d => d.Name),
                        DishSorting.NameDesc => query.OrderByDescending(d => d.Name),
                        DishSorting.PriceAsc => query.OrderBy(d => d.Price),
                        DishSorting.PriceDesc => query.OrderByDescending(d => d.Price),
                        _ => query.OrderBy(d => d.Id)
                    };
                }
                else
                {
                    query = query.OrderBy(d => d.Id);
                }


                return await query.Skip((page - 1) * 10).ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dishes with filters and sorting.");
                throw;  // Re-throw to indicate failure
            }
        }

        public Task<ICollection<Dish>> GetDishes(DishQueryParams queryParams)
        {
            throw new NotImplementedException();
        }
    }
}