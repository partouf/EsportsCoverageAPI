# EsportsCoverageAPI

C# API to control your stream at esportscoverage.net

Available functions:
* public StreamDetails GetStreamDetails()
* public Dictionary<int, string> ListEventNames()
* public void SetEvent(int event_id)
* public bool SetCurrentPlayers(string player1, string player2, int score1 = 0, int score2 = 0)
* public void SetCurrentScore(int score1, int score2)
