# Lucrarea 2: Crearea unui sistem de tipuri pentru un model Domain Driven Design (DDD) 

**Context**: coșul de cumpărături pentru un magazin virtual. 

**Obiective**: înțelegerea conceptelor de tip valoare și tip entitate, construirea unui sistem de tipuri [1] specific pentru un anumit domeniu 

## Sarcina 1

Analizați și rulați soluția din directorul [Exemple](Exemple/) (`Exemple/Examples.slnx`). Identificați elementele noi vis-a-vis de modul în care este scris și organizat codul sursă. Observați în special:

* obiectele-valoare (`Grade`, `StudentRegistrationNumber`) se construiesc printr-un *smart constructor* static (`Create`/`Parse`) care întoarce `Result` în loc să arunce excepții — vedeți `Examples.Domain/ValueObjects/`;
* stările examenului (`Exam.Unvalidated`, `Exam.Validated`, ...) sunt o ierarhie închisă de înregistrări (`abstract record` cu constructor privat) — vedeți `Examples.Domain/States/Exam.cs`.

## Sarcina 2

Implementarea unui sistem de tipuri pentru a reprezenta un coș de cumpărături și realizarea unei aplicații consolă care să folosească acele tipuri. 
Sistemul de tipuri trebuie să folosească: 
* o interfață pentru a reprezenta coșul de cumpărături, stările coșului (gol, nevalidat, validat, plătit) vor fi reprezentate prin implementarea unui tip pentru a reprezenta fiecare stare.  
* tipuri valoare imutabile pentru a reprezenta cantitatea produselor comandate, codul produsului, adresa 
* tipuri entitate pentru a reprezenta coșul de cumpărături și clientul 

Aplicația consolă trebuie să permită crearea unui coș gol, adăugarea de produse în coș și trecerea unui coș dintr-o stare fără însă a aplica validările.

Pentru a interpreta stările coșului de cumpărături se va folosi construcția `switch-expression` (vezi [Lucrarea 1](../Lucrarea-01/README.md)).

## Concepte noi în exemplu

* `Result<TSuccess, TFailure>` și `Option<T>` (proiectul `Examples.Functional`) — tipuri-sumă închise pentru "poate reușeurile" și "poate lipseurile", folosite în locul excepțiilor și al `null`-ului.
* Stările examenului formează o ierarhie închisă (`Examples.Domain/States/Exam.cs`): un `abstract record` cu constructor privat și înregistrări imbricate `sealed`, potrivită exhaustiv cu `switch`.

## Pentru proiect

Începeți descoperirea domeniului echipei cu Event Storming — vezi [Lucrarea 4, Partea 1](../Lucrarea-04/README.md). Rezultatul acelui exercițiu este livrabilul din săptămâna a 7-a ([Proiect](../Proiect/README.md)).

## Rulare

```
dotnet run --project Exemple/Examples.ConsoleApp
```

## GitHub Copilot

### Value Type

A value type in Domain-Driven Design (DDD) represents a concept that is defined by its attributes or properties. It is immutable, meaning its state cannot be changed once it is created. Value types are used to model characteristics or measurements of entities in the domain.

For example, let's consider a registration number in a car rental system. The registration number is a value type because it is defined by its attributes, such as the country code, the state code, and the unique identifier. Once a registration number is assigned to a vehicle, its attributes cannot be modified.

Value types are important in DDD as they help in creating a rich and expressive domain model by encapsulating behavior and enforcing consistency within the domain.

### Entity Type

An entity type in Domain-Driven Design (DDD) represents a concept that has a unique identity and is defined by its attributes or properties. Unlike value types, entity types are mutable, meaning their state can be changed over time. Entity types are used to model objects in the domain that have a distinct identity and can undergo different states or behaviors.

For example, let's consider a car entity type in a car rental system. Each car has a unique identifier and is defined by its attributes such as the make, model, year, and current status. The state of a car entity can change as it is rented, returned, or undergoes maintenance.

Entity types play a crucial role in DDD as they enable the modeling of complex business processes and interactions between different objects in the domain.

În exemplele acestui laborator, entitatea (examenul) este reprezentată prin stări imutabile: o tranziție de stare creează o instanță nouă, nu modifică una existentă.

## Decizii de arhitectură

Alegerile de proiectare ale acestui laborator – sistemul de tipuri închis, `Result` în loc de excepții, absența stării `Invalid` – împreună cu alternativele respinse și consecințele lor sunt documentate în [ADR-0001](docs/adr/0001-sistem-de-tipuri-inchis-cu-erori-ca-valori.md), primul din lanțul de ADR-uri al laboratoarelor cu cod.

## Referințe

[1] Scott Wlaschin, [Domain Modeling Made Functional](https://www.amazon.com/Domain-Modeling-Made-Functional-Domain-Driven-ebook/dp/B07B44BPFB/ref=sr_1_1?dchild=1&keywords=Domain+Modeling+Made+Functional&qid=1632338254&sr=8-1), Pragmatic Bookshelf, 2018  
