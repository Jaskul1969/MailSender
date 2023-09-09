// See https://aka.ms/new-console-template for more information
using MySql.Data.MySqlClient;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Layout.Element;
using System.Net;
using System.Net.Mail;
static void Wyslij(string email, string haslo, string tresc, string temat)//tworzenie funkcji wysyłającej maila
{

    MailAddress from = new MailAddress(email);//pobieranie adresu nadawcy 
    MailAddress to = new MailAddress("tu wpisz adres email adresata", "Tu wpisz imię i nazwisko adrssata", System.Text.Encoding.UTF8);//adres i nazwa odbiorcy
    MailMessage message = new MailMessage(from, to);//tworzenie wiadomości
    message.Body = tresc;//wstawianie treści
    message.Subject = temat;//wstawianie teamtu
    SmtpClient smtp = new SmtpClient("smtp-mail.outlook.com", 587);//łączenie się z serwerem SMTP nadawcy
    smtp.EnableSsl = true;//uruchamianie SSL
    smtp.Credentials = new NetworkCredential(email, haslo);//pobieranie emaila i hasła nadawcy do logowania
    smtp.Send(message);//wysyłanie maila przez serwer SMTP nadawcy 

}
//łączenie z bazą danych
string conStr = "host=localhost;user=root;database=ideal1;";//definiowanie połączenia z bazą danych
MySqlConnection con = new MySqlConnection(conStr);//tworzenie połączenia
bool polaczono;
//próba łączenia
try
{
    con.Open();
    polaczono = true;
}
catch (Exception ex)//nieudane połączenia
{
    Console.WriteLine("Nie udało się połączyć z bazą danych");
    polaczono = false;
}

//wysyłanie maila
if (polaczono)
{ //zapytanie SQL wypisujące pracowników którzy za 30 dni ukończą 55 lat
    string sql = "SELECT * FROM tabela WHERE DATE_ADD(data_urodzenia, INTERVAL 55 YEAR) - INTERVAL 30 DAY = CURRENT_DATE;";
    MySqlCommand cmd = new MySqlCommand(sql, con);
    MySqlDataReader dr = cmd.ExecuteReader();
    //tworzenie wiadomości jeżeli jest spełniony warunek
    while (dr.Read())
    {
        //tworzenie wiadomości 
        string tresc = $"ID:{dr[0]}, Imię:{dr[1]}, Nazwisko:{dr[2]}, Data urodzenia:{dr[3]}, Adres zamieszkania:{dr[4]}, Adres korespondencyjny:{dr[5]}, Adres zameldowania:{dr[6]}, Adres podatkowy:{dr[7]}, Wojewodztwo:{dr[8]}";
        string temat = "Raport osob, które za 30 dni skończą 55 lat";
        Wyslij("tu wpisz email do konta outlook", "tu wpisz haslo do konta outlook", tresc, temat);//definiowanie emaila i hasła nadawcy oraz treści i tematu wiadomości
    }
    dr.Close();//zamykanie czytnika
    //tworzenie zapytanie sprawdzającego czy są powtórzone dane
    sql = "SELECT imie, nazwisko, data_urodzenia, COUNT(*) AS LiczbaWystapien FROM tabela GROUP BY imie, nazwisko, data_urodzenia HAVING COUNT(*) > 1;";
    MySqlCommand cmd1 = new MySqlCommand(sql, con);
    MySqlDataReader dr1 = cmd1.ExecuteReader();
    while (dr1.Read())
    {
        //tworzenie drugiej wiadowmości 
        string tresc = $"Imię:{dr1[0]}, Nazwisko:{dr1[1]}, Data urodzenia:{dr1[2]}, liczba powtórzeń{dr1[3]}";
        string temat = "Raport powtórzonych osób";
        Wyslij("tu wpisz email do konta outlook", "tu wpisz hasło do konta outlook", tresc, temat);
    }

}






string wejscie;//utworzenie zmiennej wejścia

Console.WriteLine("Jaki raport chcesz wygenerować? Wpisz jedną z opcji:top-wojewodztw lub zestawienie-urodzin");//zapytanie użytkownika jaki raport chce wygenerować
wejscie = Console.ReadLine();//odczytanie odpowiedzi użytkownika


//tworzenie raportu
string sciezka = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Raport Top 5.pdf");//ścieżka raportu
PdfWriter writer = new PdfWriter(sciezka);
PdfDocument dokPdf = new PdfDocument(writer);
Document dok = new Document(dokPdf);
PdfFont czcionka = PdfFontFactory.CreateFont("C:\\Windows\\Fonts\\Calibri.ttf", PdfEncodings.IDENTITY_H);//definiowanie czcionki i kodowania na zawierające polskie znaki
dok.SetFont(czcionka);//ustawianie czcionki w całym dokumencie
if (polaczono & wejscie == "top-wojewodztw")//sprawdzanie czy jest połączenie z bazą i co wpisał użytkownik
{
    string sql = "SELECT tabela.wojewodztwo,COUNT( * ) as ilosc from tabela GROUP by wojewodztwo ORDER by ilosc DESC LIMIT 5;\r\n";//zapytanie SQL
    
    MySqlCommand cmd = new MySqlCommand(sql,con);//funkcja tworząca zapytanie
    MySqlDataReader dr = cmd.ExecuteReader();//funkcja sprawdzająca co zwróciło zapytanie
    //nagłówek dokumentu
    Paragraph nag = new Paragraph("Raport Top 5 województw pod wz.ilości pracowników").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFontSize(28);
    dok.Add(nag);
    while (dr.Read())// sprawdzanie czy czytnik sprawdza dane
    {
        Paragraph tr = new Paragraph($"Województwo:{dr[0]}; Ilość pracowników:{dr[1]}");//tworzenie paragrafu dokumentu
        dok.Add(tr);
    }
    dok.Close();//zamykanie dokumentu
}
else if(polaczono & wejscie == "zestawienie-urodzin") //sprawdzanie następnej opcji wejścia jeżeli pierwsza się nie spełniła
{
    string sql = "SELECT wojewodztwo, DAY(data_urodzenia) as dzien, COUNT(*) as ilosc FROM tabela GROUP BY dzien ORDER BY dzien;\r\n";
    MySqlCommand cmd = new MySqlCommand(sql,con);
    MySqlDataReader dr = cmd.ExecuteReader();
    Paragraph nag = new Paragraph("Raport z ilością ludzi urodzonych w danym miesiącu").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFontSize(28);
    dok.Add(nag);
    while (dr.Read())
    {
        Paragraph tr = new Paragraph($"Dzień miesiąca:{dr[1]}, województwo:{dr[0]}, ilość osób:{dr[2]}");
        dok.Add(tr);
    }
    dok.Close();
}
else//informacja o nieprawidłowym zapytaniu jeżeli żadne z powyższych nie zostało spełnione
{
    Console.WriteLine("Nieprawidłowe zapytanie.");
}


