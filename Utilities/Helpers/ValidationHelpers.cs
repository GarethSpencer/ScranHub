namespace Utilities.Helpers;

public static class ValidationHelpers
{
    public static bool CheckProfanity(string groupName)
    {
        var filter = new ProfanityFilter.ProfanityFilter();
        var detected = filter.DetectAllProfanities(groupName);
        return detected == null || detected.Count == 0;
    }
}
