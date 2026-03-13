\# Bookovaní místa

Aplikace pro interní rezervování pracovního místa v kanceláři



\# Instalace

TODO NuGet

SQLite



\# Postup řešení, později # Využití aplikace

\* vytvořím projekt. Přidávat názvy použitých NuGet

\* začnu přidáním DB, použiju SQLite

\* vytvořím strukturu DB podle náčrtu v příloze

\* hrubě si rozvrhnu stránky a metody

\* připravím login na který využiju ASP.NET Core Identity

\* jednotlivé stránky:

úvodní: 4 dlaždice

&#x20; \* akce zabookovat místo, přidat nová rezervace

&#x20; \* vypsat z DB rezervace.od, rezervace.do, misto.oznaceni, sekce.nazev pro aktuálního uživatele

&#x20; \* vypsat z DB misto.oznaceni, zamestnanci.jmeno kde rezervace.od < getDate() a rezervace.do > getDate() pro vybranou sekci, počítám s možností rezervace vůce dnů

&#x20; \* vypsat z DB misto.oznaceni, sekce.nazev, vytíženost, kde vytíženost jsou rezervované dny v poměru s výběrem, pouze pracovní dny, pro vybranou sekci + počet rezervovaných dnů

\* Bookování místa

tlačítko na bookování > zobrazí celé patro > z obrázku se vybere místnost ze čtyř(devíti) možností > konkrétní místnost s možností rezervace

(beru jen naší stranu od hl. kuchyňky, devět pokud bych neodělovat dveřma, ale "zdmi")

v <svg> si nakreslím místnosti, udělat místo na jména pro Denní obsazenost a hover

zeleně volné místo, červeně obsazené, modře moje rezervace, světle modrá vybraná rezervace

tlačítko na potvrzení rezervace a výběr dne

u zabookovaných míst zobrazit rezervátora, hover na místo

možnost zabookovat jen jedno místo ve vybraný den

při potvrzení rezervace volat DB pro ověření aktuální dostupnosti

\* Moje historie

nová stránka, tabulka pro hodnoty, výpis z DB, 100 záznamů, od nejnovějších

\* Denní obsazenost

read-only grafika z Bookování místa

vždy zobrazovat jméno rezervátora

tlačítko na změnu data s defaultem na dnešku

\* Vytíženost

tabulka podobně jako Moje historie

default jako celý aktuální měsíc

vypsat pouze místa s alespoň jednou rezervací

řadit podle počtu rezervovaných míst



\# Dokumetace

\* DB struktura

\* Word doc



\# DB model

https://dbdiagram.io/d

Table zamestnanci {

&#x20; id integer \[primary key]

&#x20; jmeno varchar

&#x20; email varchar

}

Table sekce {

&#x20; id integer \[primary key]

&#x20; nazev varchar

}

Table misto {

&#x20; id integer \[primary key]

&#x20; sekce\_id integer

&#x20; oznaceni varchar

}

Table rezervace {

&#x20; id integer \[primary key]

&#x20; zamestnanec\_id integer

&#x20; misto\_id integer

&#x20; rezervovano\_od date

&#x20; rezervovano\_do date

}

Ref: "zamestnanci"."id" < "rezervace"."zamestnanec\_id"

Ref: "misto"."id" < "rezervace"."misto\_id"

Ref: "sekce"."id" < "misto"."sekce\_id"



