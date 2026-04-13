using Core.DataAccess.EntityFramework;
using DataAccess.Abstract.TicketDal;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;

namespace DataAccess.Concrete.EntityFramework.EfTicketDal;
public class EfTicketDal : EfEntityRepositoryBase<Ticket, MsSqlDbContext>, ITicketDal
{
    public EfTicketDal(MsSqlDbContext context) : base(context) { }
    public TicketDto? GetTicketDtoById(int id)
    {
        var result =(from t in _context.Tickets where t.Id == id
                     select new TicketDto
                     {
                         Ticket = t,
                         Assignment = _context.Assignments.FirstOrDefault(a => a.TicketId == t.Id),
                         Responses = _context.Responses.Where(r => r.TicketId == t.Id).ToList(),
                         Attachments = _context.Attachments.Where(r => r.TicketId == t.Id).ToList(),
                     }).FirstOrDefault();
        return result;
    }
}
