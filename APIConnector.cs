namespace SkillAppDaemon;


class APIConnector
{
    const string API_URL = "http://localhost:5054/skillsuggestion?skill=";

    /// <summary>
    /// Gets a suggestion for how to best practice the given skill from an API.
    /// </summary>
    /// <param name="skill">The skill to get a suggestion for</param>
    /// <returns>The suggestion as a string</returns>
    public async Task<string> GetSuggestionFor(string skill)
    {

        string FULL_URL = API_URL + Uri.EscapeDataString(skill);

        using var client = new HttpClient();
        using var response = await client.GetAsync(FULL_URL);
        if (response != null && response.IsSuccessStatusCode)
        {
            var content = response.Content;
            var suggestionString = await content.ReadAsStringAsync();

            if (suggestionString != null)
            {
                return suggestionString;
            }
        }


        return "";
    }
}
