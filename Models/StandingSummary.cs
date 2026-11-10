namespace IRacingLeague.Models;

public sealed record StandingSummary(Registration Registration, int RacesCompleted, int Points, int Wins, int Podiums, int IncidentPoints, int BestFinish);
