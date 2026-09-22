namespace Examples.Events;

/// <summary>Cum s-a încheiat tratarea unui mesaj primit, folosit de ascultător pentru a decide cum îl decontează.</summary>
public enum EventProcessingResult
{
    /// <summary>Mesajul a fost tratat cu succes; se poate elimina din coadă/subscripție.</summary>
    Completed,

    /// <summary>Eroare tranzitorie; mesajul trebuie livrat din nou.</summary>
    Retry,

    /// <summary>Mesajul nu poate fi tratat (date nevalide); se trimite direct în coada de mesaje moarte.</summary>
    Failed,
}
