namespace IRacingLeague.Business;

// Thrown when a user cannot be deleted because they still own one or more leagues.
// Owned leagues hold other drivers' data, so the admin must delete or reassign them first.
public class UserOwnsLeaguesException : Exception
{
    public UserOwnsLeaguesException(string message) : base(message) { }
}
