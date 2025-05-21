namespace SkillAppDaemon;
using System.IO;
using Npgsql;
using System.Collections.Generic;

/// <summary>
/// Responsible for connecting to a postgresql database
/// </summary>
class SQLConnector
{

    NpgsqlConnection connection;

    /// <summary>
    /// Constructor that initializes the connection field
    /// </summary>
    public SQLConnector()
    {
        connection = new NpgsqlConnection();
    }

    /// <summary>
    /// Gives the connection field all it needs to connect to the database
    /// </summary>
    public void MakeConnection()
    {
        StreamReader reader = new StreamReader(".env");

        string? nextLine;
        do
        {
            nextLine = reader.ReadLine();
            if (nextLine == null)
            {
                break;
            }
            var kvPair = nextLine.Split(": ");
            if (kvPair.Length > 1)
            {
                Environment.SetEnvironmentVariable(kvPair[0], kvPair[1]);
            }
        } while (nextLine != null);



        var host = Environment.GetEnvironmentVariable("host");
        var port = Environment.GetEnvironmentVariable("port");
        var database = Environment.GetEnvironmentVariable("database");
        var username = Environment.GetEnvironmentVariable("username");
        var password = Environment.GetEnvironmentVariable("password");

        var connStrBuilder = new NpgsqlConnectionStringBuilder();
        connStrBuilder.Host = host;
        connStrBuilder.Username = username;
        connStrBuilder.Password = password;
        connStrBuilder.Database = database;
        try
        {
            if (port != null)
            {
                connStrBuilder.Port = int.Parse(port);
            }
        }
        catch
        {
            Console.WriteLine(".evn variable port could not be converted to int: port=" + port);
        }


        connection.ConnectionString = connStrBuilder.ConnectionString;
    }

    /// <summary>
    /// Gets the rows from the skills table, in the form of (<skill>, <time left before notification>)
    /// </summary>
    /// <returns>Returns the skills and their time left before notification</returns>
    public List<(string, long)> GetSkills()
    {
        var skillsList = new List<(string, long)>();

        connection.Open();
        var unixTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM skills", connection);
        var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var skill = reader.GetString(0);
            var timeElapsed = unixTimeStamp - reader.GetInt32(2);
            var timeLeft = reader.GetInt32(1) - timeElapsed;
            if (timeLeft < 0)
            {
                timeLeft = 0;
            }
            skillsList.Add((skill, timeLeft));
        }


        connection.Close();

        return skillsList;
    }

    /// <summary>
    /// Resets the timer on the given skill
    /// </summary>
    /// <param name="skillsList">The skill to reset the timer on</param>
    public void ResetSkills(List<string> skillsList)
    {
        connection.Open();

        var unixTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // The timer is reset by setting start_time to the current time
        var baseString = "UPDATE skills SET start_time = @start_time WHERE";

        var conditions = "";

        for (var i = 1; i <= skillsList.Count; i++)
        {
            if (i > 1)
            {
                conditions += " OR";
            }
            conditions += " skill = @skill" + i.ToString();
        }

        using NpgsqlCommand cmd = new NpgsqlCommand(baseString + conditions, connection);
        cmd.Parameters.AddWithValue("@start_time", unixTimeStamp);

        var valCounter = 1;
        foreach (var skill in skillsList)
        {
            cmd.Parameters.AddWithValue("@skill" + valCounter.ToString(), skill);
            valCounter++;
        }

        cmd.ExecuteNonQuery();

        connection.Close();
    }
}