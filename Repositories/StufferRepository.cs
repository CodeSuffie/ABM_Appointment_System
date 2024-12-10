// using Database;
// using Database.Models;
//
// namespace Repositories;
//
// public sealed class StufferRepository(ModelDbContext context)
// {
//     // TODO: Stuffer
//     public IQueryable<Stuffer> Get()
//     {
//         var stuffer = context.Stuffers;
//
//         return stuffer;
//     }
//
//     // TODO: Stuffer
//     public async Task AddAsync(Stuffer stuffer, CancellationToken cancellationToken)
//     {
//         await context.Stuffers
//             .AddAsync(stuffer, cancellationToken);
//         
//         await context.SaveChangesAsync(cancellationToken);
//     }
//
//     // TODO: Stuffer
//     public async Task AddAsync(Stuffer stuffer, HubShift hubShift, CancellationToken cancellationToken)
//     {
//         stuffer.Shifts.Add(hubShift);
//         hubShift.Stuffer = stuffer;
//         
//         await context.SaveChangesAsync(cancellationToken);
//     }
// }