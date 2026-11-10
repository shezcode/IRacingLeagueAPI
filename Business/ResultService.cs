using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public sealed class ResultService(IResultRepository results, IRegistrationRepository registrations, IUserRepository users, IUnitOfWork unitOfWork) : IResultService
{
    public async Task<Result> ApplyResultAsync(Registration registration, Race race, Result result, CancellationToken cancellationToken = default)
    {
        var user = await users.GetAsync(registration.UserId, cancellationToken) ?? throw new NotFoundException($"User with id {registration.UserId} not found.");
        var existing = await results.FindForRaceAndRegistrationAsync(race.RaceId, registration.RegistrationId, cancellationToken);
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (existing is not null)
            {
                registration.AddPoints(result.Points - existing.Points); user.UndoRaceOutcome(existing.Position, existing.IncidentPoints); user.ApplyRaceOutcome(result.Position, result.IncidentPoints);
                existing.Position = result.Position; existing.FastestLapSeconds = result.FastestLapSeconds; existing.Points = result.Points; existing.IncidentPoints = result.IncidentPoints; existing.Dnf = result.Dnf; existing.Notes = result.Notes; existing.FinishedAt = result.FinishedAt;
                await results.UpdateAsync(existing, ct); result = existing;
            }
            else
            {
                result.RegistrationId = registration.RegistrationId; result.RaceId = race.RaceId; await results.AddAsync(result, ct); registration.AddPoints(result.Points); user.ApplyRaceOutcome(result.Position, result.IncidentPoints);
            }
            await registrations.UpdateAsync(registration, ct); await users.UpdateAsync(user, ct);
        }, cancellationToken);
        return result;
    }
    public Task<IReadOnlyList<Result>> GetByRaceAsync(int raceId, CancellationToken cancellationToken = default) => results.GetByRaceAsync(raceId, cancellationToken);
    public Task<PagedResponse<Result>> GetByRacePageAsync(int raceId, PaginationParameters parameters, CancellationToken cancellationToken = default) => results.GetByRacePageAsync(raceId, parameters, cancellationToken);
    public async Task<Result> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await results.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"Result with id {id} not found.");
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { if (await results.GetAsync(id, cancellationToken) is null) throw new NotFoundException($"Result with id {id} not found."); await unitOfWork.ExecuteInTransactionAsync(ct => BackOutAsync(id, ct), cancellationToken); }
    public Task DeleteForRaceAsync(int raceId, CancellationToken cancellationToken = default) => unitOfWork.ExecuteInTransactionAsync(async ct => { foreach (var result in await results.GetByRaceAsync(raceId, ct)) await BackOutAsync(result.ResultId, ct); }, cancellationToken);
    public Task DeleteForRegistrationAsync(int registrationId, CancellationToken cancellationToken = default) => unitOfWork.ExecuteInTransactionAsync(async ct => { foreach (var result in await results.GetByRegistrationAsync(registrationId, ct)) await BackOutAsync(result.ResultId, ct); }, cancellationToken);
    private async Task BackOutAsync(int resultId, CancellationToken ct)
    {
        var result = await results.GetAsync(resultId, ct); if (result is null) return;
        var registration = await registrations.GetAsync(result.RegistrationId, ct);
        if (registration is not null) { registration.AddPoints(-result.Points); await registrations.UpdateAsync(registration, ct); var user = await users.GetAsync(registration.UserId, ct); if (user is not null) { user.UndoRaceOutcome(result.Position, result.IncidentPoints); await users.UpdateAsync(user, ct); } }
        await results.DeleteAsync(result.ResultId, ct);
    }
}
