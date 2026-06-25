using Core.DataAccess.EntityFramework;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework.EfTicketDal;
public class EfTicketDal : EfEntityRepositoryBase<Ticket, MsSqlDbContext>, ITicketDal
{
    public EfTicketDal(MsSqlDbContext context) : base(context) { }
    public async Task<TicketDto?> GetTicketDtoByIdAsync(int id)
    {
        var result = await (from t in _context.Tickets where t.Id == id
                     select new TicketDto
                     {
                         Ticket = t,
                         Assignment = _context.Assignments.FirstOrDefault(a => a.TicketId == t.Id),
                         Responses = _context.Responses.Where(r => r.TicketId == t.Id).ToList(),
                         Attachments = _context.Attachments.Where(r => r.TicketId == t.Id).ToList(),
                     }).FirstOrDefaultAsync();
        return result;
    }
}
