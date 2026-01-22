using System;
using System.Collections.Generic;

public static class TeamManager
{
    private static List<String> AllTeams 
        = new()
        {
            "Hurricane", 
            "Vanguard", 
            "Noobies",
            "Buccaneers",
            "ENIGM4",
            "Echelon", 
            "Shadow Vortex",
            "Xyphoraxis",
            "Fantastic_4",
            "Scavengers",
            "CODE HUNTERS",
            "NESCIENT",
            "Riddler’s crew",
            "Team Feluda",
            "Aesthetic",
            "Kidnappers",
            "Lazarus",
            "Pegasus", 
            "Quite Horizon",
            "EDTA", 
            "Zero Flux",
            "Codebreakers",
            "FlyingDutchman",
            "Anything",
            "ridhwandll",
            "__Admin",
        };

    public static bool IsTeamNameValid(String inputTeamName)
    {
        foreach (var teamName in AllTeams)
        {
            if(teamName.Equals(inputTeamName))
                return true;
        }
        
        return false;
    }
}
