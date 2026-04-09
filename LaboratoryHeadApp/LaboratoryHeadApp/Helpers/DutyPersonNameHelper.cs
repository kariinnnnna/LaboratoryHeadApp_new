namespace LaboratoryHeadApp.Helpers
{
    public static class DutyPersonNameHelper
    {
        public static string GetInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return string.Empty;
            }

            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .ToList();

            return string.Concat(parts.Select(x => char.ToUpper(x[0])));
        }
    }
}
