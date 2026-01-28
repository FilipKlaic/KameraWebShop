Här är lite instruktioner för vad du behöver för att koppla dig till min azure databas.
Du behöver skapa en .json fil i rotmappen av projektet som heter "appsettings.json" där är har jag min connection string som jag har dolt då lösenordet finns med där

det du behöver göra då nu är att:
1. skapa en appsettings.json fil när du kommer in i projektet
2. klistra in detta:

{
  "ConnectionStrings": {
    "MyDbConnection": "Server=tcp:filip-webshop-server.database.windows.net,1433;Initial Catalog=WebshopDb;Persist Security Info=False;User ID=dbadmin;Password=DITT_NYA_LÖSENORD_HÄR;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}


Lösenordet kommer tilldelas till dig under redovisningen och som en separat .txt fil vid inlämningen. Lämnar den inte på min github då jag inte vill att lösenordet ska vara synligt för andra
