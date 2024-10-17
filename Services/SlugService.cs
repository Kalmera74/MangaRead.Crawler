
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class SlugService
{

    public static string ToSlug(this string input)
    {
        if (string.IsNullOrEmpty(input)) { throw new FormatException("Input for slug is null or empty"); }

        // Convert to lowercase
        string slug = input.ToLowerInvariant().Trim();

        // Remove diacritics (accents)
        slug = RemoveDiacritics(slug);

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s", "-");

        // Allow periods and commas between digits by updating the regex
        slug = Regex.Replace(slug, @"[^a-z0-9\.,\-]", "");

        // Remove consecutive hyphens
        slug = Regex.Replace(slug, @"-{2,}", "-");

        // Ensure the period and comma are only between numbers (optional: remove standalone ones)
        slug = Regex.Replace(slug, @"(?<!\d)[\.,](?!\d)", "");  // Removes periods/commas not between digits

        return slug;
    }

    private static string RemoveDiacritics(string text)
    {
        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }


}