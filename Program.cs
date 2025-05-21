using SkillAppDaemon;
const int SLEEP_TIME = 1000 * 60;

SQLConnector connector = new SQLConnector();
APIConnector api = new APIConnector();

connector.MakeConnection();




while (true)
{

    var skills = connector.GetSkills();

    var skillsToUpdate = new List<string>();
    foreach (var skill in skills)
    {
        if (skill.Item2 == 0)
        {
            Console.WriteLine("Practice your skill: " + skill.Item1);
            var res = await api.GetSuggestionFor(skill.Item1);

            var startIndex = 1;
            var endIndex = res.Length - 1;
            Console.WriteLine("Suggestion for practicing that skill: " + res[startIndex..endIndex]);

            skillsToUpdate.Add(skill.Item1);
        }
    }

    if (skillsToUpdate.Count > 0)
    {
        connector.ResetSkills(skillsToUpdate);
    }

    Thread.Sleep(SLEEP_TIME);
}