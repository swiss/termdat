namespace Bk.Termdat.Business.Domain.Models;

public enum Reliability
{
    // Erster Entwurf (3220)
    FirstDraft = 0,

    // Provisorisch (3221)
    Tentative = 1,

    // Sprachlich/formal überprüft (3222)
    Translated = 2,

    // Fachlich überprüft (3223)
    Verified = 3,

    // Normiert (3224)
    Approved = 4
}
