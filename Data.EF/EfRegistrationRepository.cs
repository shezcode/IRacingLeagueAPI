using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class EfRegistrationRepository : IRegistrationRepository
{
    private readonly AppDbContext _context;

    public EfRegistrationRepository(AppDbContext context) => _context = context;

    public void Add(Registration registration) => _context.Registrations.Add(registration);

    public Registration? Get(int id) => _context.Registrations.Find(id);

    public IEnumerable<Registration> GetAll() => _context.Registrations.AsNoTracking().ToList();

    public void Update(Registration registration) => _context.Registrations.Update(registration);

    public void Delete(int id)
    {
        var registration = _context.Registrations.Find(id);
        if (registration is not null)
            _context.Registrations.Remove(registration);
    }

    public void SaveChanges() => _context.SaveChanges();
}
