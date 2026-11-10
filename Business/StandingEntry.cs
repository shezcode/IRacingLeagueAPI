using IRacingLeague.Models;

namespace IRacingLeague.Business;

// A single standings row: the league membership plus how many races that
// driver has a recorded result for (across all races in the league, DNFs included).
public record StandingEntry(Registration Registration, int RacesCompleted, int Points = 0, int Wins = 0, int Podiums = 0, int IncidentPoints = 0, int BestFinish = 0, int Position = 0);
