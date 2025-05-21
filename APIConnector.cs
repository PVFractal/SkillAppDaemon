namespace SkillAppDaemon;



class APIConnector
{
    const string API_URL = "http://localhost:5054/skillsuggestion?skill=";
    public async Task<string> GetSuggestionFor(string skill)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(API_URL + skill);
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
